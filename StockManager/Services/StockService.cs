using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StockManager.Data;
using StockManager.Models;

namespace StockManager.Services;

/// <summary>
/// Toute la logique métier de l'inventaire : produits, mouvements et enregistrement.
/// Chaque opération est écrite immédiatement dans la base SQLite.
/// </summary>
public class StockService
{
    /// <summary>Seuil de stock faible utilisé tant que l'utilisateur ne l'a pas modifié.</summary>
    public const int DefaultLowStockThreshold = 2;

    private const string LowStockThresholdKey = "LowStockThreshold";
    private const int MaxNameLength = 120;

    /// <summary>Inventaire de départ, créé une seule fois à la première ouverture.</summary>
    private static readonly (string Name, int Quantity)[] InitialProducts =
    {
        ("Housse de couettes", 9),
        ("Drap housse", 9),
        ("Grandes serviettes", 9),
        ("Petites serviettes", 6),
        ("Taies", 9),
        ("Tapis", 1),
        ("Serviettes mains", 1),
        ("Torchons", 2)
    };

    private readonly Func<AppDbContext> _contextFactory;

    public StockService(Func<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    /// <summary>
    /// Crée la base et les tables si nécessaire, puis insère l'inventaire de départ
    /// uniquement lorsque la base vient d'être créée.
    /// </summary>
    public void Initialize()
    {
        AppPaths.EnsureDataDirectory();

        using var db = _contextFactory();

        if (db.Database.EnsureCreated())
        {
            SeedInitialProducts(db);
        }
    }

    private static void SeedInitialProducts(AppDbContext db)
    {
        var createdAt = DateTime.Now;

        foreach (var (name, quantity) in InitialProducts)
        {
            db.Products.Add(new Product
            {
                Name = name,
                Quantity = quantity,
                CreatedAt = createdAt
            });
        }

        db.SaveChanges();
    }

    // ---------------------------------------------------------------- lecture

    /// <summary>Tous les produits, triés par nom.</summary>
    public IReadOnlyList<Product> GetProducts()
    {
        using var db = _contextFactory();

        return db.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToList();
    }

    /// <summary>Les derniers mouvements de stock, du plus récent au plus ancien.</summary>
    public IReadOnlyList<StockMovement> GetMovements(int limit = 500)
    {
        using var db = _contextFactory();

        return db.StockMovements
            .AsNoTracking()
            .Include(m => m.Product)
            .OrderByDescending(m => m.Date)
            .ThenByDescending(m => m.Id)
            .Take(limit)
            .ToList();
    }

    // ---------------------------------------------------------------- produits

    /// <summary>Crée un produit et, si la quantité initiale est positive, le mouvement d'entrée correspondant.</summary>
    public Product CreateProduct(string? rawName, int quantity)
    {
        var name = NormalizeName(rawName);

        if (quantity < 0)
        {
            throw new StockException("La quantité ne peut pas être négative.");
        }

        using var db = _contextFactory();

        EnsureNameIsAvailable(db, name, excludedProductId: null);

        var product = new Product
        {
            Name = name,
            Quantity = quantity,
            CreatedAt = DateTime.Now
        };

        db.Products.Add(product);

        if (quantity > 0)
        {
            product.Movements.Add(new StockMovement
            {
                Type = MovementType.Entree,
                Quantity = quantity,
                Date = DateTime.Now
            });
        }

        SaveChanges(db, name);
        return product;
    }

    /// <summary>Renomme un produit sans toucher à sa quantité en stock.</summary>
    public Product RenameProduct(int productId, string? rawName)
    {
        var name = NormalizeName(rawName);

        using var db = _contextFactory();

        var product = db.Products.Find(productId) ?? throw ProductNotFound();

        EnsureNameIsAvailable(db, name, excludedProductId: productId);

        product.Name = name;

        SaveChanges(db, name);
        return product;
    }

    /// <summary>Supprime un produit ainsi que son historique de mouvements.</summary>
    public void DeleteProduct(int productId)
    {
        using var db = _contextFactory();

        var product = db.Products.Find(productId) ?? throw ProductNotFound();

        // Suppression explicite des mouvements : l'historique ne laisse aucune
        // ligne orpheline, quelle que soit la configuration des clés étrangères.
        var movements = db.StockMovements.Where(m => m.ProductId == productId).ToList();
        db.StockMovements.RemoveRange(movements);
        db.Products.Remove(product);

        db.SaveChanges();
    }

    // ---------------------------------------------------------------- mouvements

    /// <summary>Ajoute une quantité au stock et enregistre une entrée.</summary>
    public Product AddStock(int productId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new StockException("La quantité doit être supérieure à 0.");
        }

        using var db = _contextFactory();
        using var transaction = db.Database.BeginTransaction();

        var product = db.Products.Find(productId) ?? throw ProductNotFound();

        if (quantity > int.MaxValue - product.Quantity)
        {
            throw new StockException("La quantité totale en stock serait trop élevée.");
        }

        product.Quantity += quantity;

        db.StockMovements.Add(new StockMovement
        {
            ProductId = product.Id,
            Type = MovementType.Entree,
            Quantity = quantity,
            Date = DateTime.Now
        });

        db.SaveChanges();
        transaction.Commit();

        return product;
    }

    /// <summary>
    /// Retire une quantité du stock et enregistre une sortie.
    /// Le stock ne peut jamais devenir négatif.
    /// </summary>
    public Product RemoveStock(int productId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new StockException("La quantité doit être supérieure à 0.");
        }

        using var db = _contextFactory();
        using var transaction = db.Database.BeginTransaction();

        // La quantité est relue dans la base : le contrôle porte toujours
        // sur le stock réellement enregistré.
        var product = db.Products.Find(productId) ?? throw ProductNotFound();

        if (quantity > product.Quantity)
        {
            throw new StockException(
                $"Stock insuffisant. La quantité disponible est de {product.Quantity}.");
        }

        product.Quantity -= quantity;

        db.StockMovements.Add(new StockMovement
        {
            ProductId = product.Id,
            Type = MovementType.Sortie,
            Quantity = quantity,
            Date = DateTime.Now
        });

        db.SaveChanges();
        transaction.Commit();

        return product;
    }

    // ---------------------------------------------------------------- paramètres

    /// <summary>Seuil à partir duquel un stock est signalé comme faible.</summary>
    public int GetLowStockThreshold()
    {
        using var db = _contextFactory();

        var setting = db.Settings.AsNoTracking().FirstOrDefault(s => s.Key == LowStockThresholdKey);

        if (setting is not null &&
            int.TryParse(setting.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) &&
            value >= 0)
        {
            return value;
        }

        return DefaultLowStockThreshold;
    }

    public void SetLowStockThreshold(int threshold)
    {
        if (threshold < 0 || threshold > 1000)
        {
            throw new StockException("Le seuil doit être compris entre 0 et 1000.");
        }

        using var db = _contextFactory();

        var setting = db.Settings.FirstOrDefault(s => s.Key == LowStockThresholdKey);

        if (setting is null)
        {
            db.Settings.Add(new AppSetting
            {
                Key = LowStockThresholdKey,
                Value = threshold.ToString(CultureInfo.InvariantCulture)
            });
        }
        else
        {
            setting.Value = threshold.ToString(CultureInfo.InvariantCulture);
        }

        db.SaveChanges();
    }

    // ---------------------------------------------------------------- utilitaires

    private static string NormalizeName(string? rawName)
    {
        var name = (rawName ?? string.Empty).Trim();

        if (name.Length == 0)
        {
            throw new StockException("Veuillez saisir le nom du produit.");
        }

        if (name.Length > MaxNameLength)
        {
            throw new StockException($"Le nom du produit ne peut pas dépasser {MaxNameLength} caractères.");
        }

        return name;
    }

    private static void EnsureNameIsAvailable(AppDbContext db, string name, int? excludedProductId)
    {
        var exists = db.Products
            .AsNoTracking()
            .Any(p => EF.Functions.Collate(p.Name, "NOCASE") == name
                      && (excludedProductId == null || p.Id != excludedProductId));

        if (exists)
        {
            throw DuplicateName(name);
        }
    }

    /// <summary>
    /// Enregistre les modifications en traduisant une violation de l'index unique
    /// en message compréhensible ; les autres erreurs restent inchangées.
    /// </summary>
    private static void SaveChanges(AppDbContext db, string name)
    {
        try
        {
            db.SaveChanges();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw DuplicateName(name);
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        => exception.InnerException is SqliteException sqlite
           && sqlite.SqliteErrorCode == 19; // SQLITE_CONSTRAINT

    private static StockException DuplicateName(string name)
        => new($"Le produit « {name} » existe déjà.");

    private static StockException ProductNotFound()
        => new("Ce produit n'existe plus. Veuillez actualiser la liste.");
}

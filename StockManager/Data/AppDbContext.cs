using Microsoft.EntityFrameworkCore;
using StockManager.Models;

namespace StockManager.Data;

/// <summary>
/// Contexte Entity Framework Core pointant sur la base SQLite locale.
/// </summary>
public class AppDbContext : DbContext
{
    private readonly string _databasePath;

    public AppDbContext()
        : this(AppPaths.DatabaseFile)
    {
    }

    public AppDbContext(string databasePath)
    {
        _databasePath = databasePath;
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public DbSet<AppSetting> Settings => Set<AppSetting>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite($"Data Source={_databasePath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(120)
                // NOCASE : « torchons » et « Torchons » sont considérés identiques,
                // ce qui rend l'index unique ci-dessous insensible à la casse.
                .UseCollation("NOCASE");

            entity.HasIndex(p => p.Name).IsUnique();

            entity.Property(p => p.Quantity).IsRequired();
            entity.Property(p => p.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.ToTable("StockMovements");
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Type)
                .IsRequired()
                .HasMaxLength(16)
                .HasConversion<string>();

            entity.Property(m => m.Quantity).IsRequired();
            entity.Property(m => m.Date).IsRequired();

            entity.HasIndex(m => m.Date);

            // Un produit possède plusieurs mouvements ; supprimer le produit
            // supprime son historique (pas de lignes orphelines).
            entity.HasOne(m => m.Product)
                .WithMany(p => p.Movements)
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.ToTable("Settings");
            entity.HasKey(s => s.Key);
            entity.Property(s => s.Key).HasMaxLength(64);
            entity.Property(s => s.Value).IsRequired().HasMaxLength(256);
        });
    }
}

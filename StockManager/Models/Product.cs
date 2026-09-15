namespace StockManager.Models;

/// <summary>
/// Un article de l'inventaire et la quantité actuellement disponible.
/// </summary>
public class Product
{
    public int Id { get; set; }

    /// <summary>Nom du produit. Unique (sans tenir compte de la casse).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Quantité actuellement en stock. Ne peut jamais être négative.</summary>
    public int Quantity { get; set; }

    /// <summary>Date de création de la fiche produit.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>Historique des entrées et sorties du produit.</summary>
    public ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();
}

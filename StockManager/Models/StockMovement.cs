namespace StockManager.Models;

/// <summary>
/// Une entrée ou une sortie de stock enregistrée pour un produit.
/// </summary>
public class StockMovement
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    /// <summary>Entrée (ajout) ou Sortie (retrait).</summary>
    public MovementType Type { get; set; }

    /// <summary>Quantité déplacée. Toujours strictement positive.</summary>
    public int Quantity { get; set; }

    /// <summary>Date et heure du mouvement.</summary>
    public DateTime Date { get; set; } = DateTime.Now;
}

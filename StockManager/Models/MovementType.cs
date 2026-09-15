namespace StockManager.Models;

/// <summary>
/// Sens d'un mouvement de stock.
/// </summary>
public enum MovementType
{
    /// <summary>Ajout de marchandise dans le stock.</summary>
    Entree,

    /// <summary>Retrait de marchandise du stock.</summary>
    Sortie
}

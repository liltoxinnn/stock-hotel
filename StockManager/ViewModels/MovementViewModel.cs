using StockManager.Models;

namespace StockManager.ViewModels;

/// <summary>
/// Une ligne de l'historique des mouvements de stock.
/// </summary>
public class MovementViewModel
{
    public MovementViewModel(StockMovement movement)
    {
        IsEntree = movement.Type == MovementType.Entree;
        DateText = movement.Date.ToString("dd/MM/yyyy HH:mm");
        ProductName = movement.Product?.Name ?? "Produit supprimé";
        TypeText = IsEntree ? "Entrée" : "Sortie";
        QuantityText = (IsEntree ? "+" : "-") + movement.Quantity.ToString();
    }

    public string DateText { get; }

    public string ProductName { get; }

    public string TypeText { get; }

    /// <summary>Quantité signée, par exemple « +5 » ou « -2 ».</summary>
    public string QuantityText { get; }

    public bool IsEntree { get; }
}

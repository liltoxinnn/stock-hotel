using StockManager.Models;

namespace StockManager.ViewModels;

/// <summary>
/// Une ligne du tableau de stock. Reconstruit à chaque actualisation de la liste.
/// </summary>
public class ProductViewModel
{
    private readonly int _lowStockThreshold;

    public ProductViewModel(Product product, int lowStockThreshold)
    {
        Model = product;
        _lowStockThreshold = lowStockThreshold;
    }

    public Product Model { get; }

    public int Id => Model.Id;

    public string Name => Model.Name;

    public int Quantity => Model.Quantity;

    /// <summary>Le produit est épuisé.</summary>
    public bool IsOutOfStock => Quantity <= 0;

    /// <summary>Le stock est faible sans être épuisé.</summary>
    public bool IsLowStock => !IsOutOfStock && Quantity <= _lowStockThreshold;

    /// <summary>Stock faible ou épuisé : la quantité est mise en évidence.</summary>
    public bool NeedsAttention => IsOutOfStock || IsLowStock;
}

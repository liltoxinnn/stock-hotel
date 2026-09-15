using StockManager.Models;
using StockManager.Services;

namespace StockManager.ViewModels;

/// <summary>Fenêtre « Ajouter au stock » (mouvement d'entrée).</summary>
public class AddStockViewModel : StockMovementViewModel
{
    public AddStockViewModel(StockService stockService, Product product)
        : base(stockService, product)
    {
    }

    public override string Title => "Ajouter au stock";

    public override string QuantityLabel => "Quantité à ajouter";

    public override string ConfirmLabel => "Ajouter";

    protected override void Execute(StockService stockService, int productId, int quantity)
        => stockService.AddStock(productId, quantity);
}

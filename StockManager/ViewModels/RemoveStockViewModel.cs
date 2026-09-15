using StockManager.Models;
using StockManager.Services;

namespace StockManager.ViewModels;

/// <summary>Fenêtre « Retirer du stock » (mouvement de sortie).</summary>
public class RemoveStockViewModel : StockMovementViewModel
{
    public RemoveStockViewModel(StockService stockService, Product product)
        : base(stockService, product)
    {
    }

    public override string Title => "Retirer du stock";

    public override string QuantityLabel => "Quantité à retirer";

    public override string ConfirmLabel => "Retirer";

    protected override void Execute(StockService stockService, int productId, int quantity)
        => stockService.RemoveStock(productId, quantity);
}

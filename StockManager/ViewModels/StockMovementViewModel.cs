using StockManager.Models;
using StockManager.Services;

namespace StockManager.ViewModels;

/// <summary>
/// Base commune aux fenêtres « Ajouter au stock » et « Retirer du stock ».
/// </summary>
public abstract class StockMovementViewModel : DialogViewModel
{
    private readonly StockService _stockService;
    private readonly int _productId;
    private string _quantity = "1";

    protected StockMovementViewModel(StockService stockService, Product product)
    {
        _stockService = stockService;
        _productId = product.Id;

        ProductName = product.Name;
        CurrentStock = product.Quantity;
    }

    /// <summary>Titre de la fenêtre, par exemple « Ajouter au stock ».</summary>
    public abstract string Title { get; }

    /// <summary>Libellé du champ, par exemple « Quantité à ajouter ».</summary>
    public abstract string QuantityLabel { get; }

    /// <summary>Libellé du bouton de validation, par exemple « Ajouter ».</summary>
    public abstract string ConfirmLabel { get; }

    public string ProductName { get; }

    public int CurrentStock { get; }

    public string Quantity
    {
        get => _quantity;
        set
        {
            if (SetProperty(ref _quantity, value))
            {
                ClearError();
            }
        }
    }

    protected abstract void Execute(StockService stockService, int productId, int quantity);

    protected override bool TryConfirm()
    {
        try
        {
            var quantity = QuantityParser.ParseMovementQuantity(Quantity);
            Execute(_stockService, _productId, quantity);
            return true;
        }
        catch (StockException exception)
        {
            ErrorMessage = exception.Message;
            return false;
        }
    }
}

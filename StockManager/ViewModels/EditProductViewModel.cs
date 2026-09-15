using StockManager.Models;
using StockManager.Services;

namespace StockManager.ViewModels;

/// <summary>
/// Fenêtre « Modifier le produit ». Seul le nom est modifiable :
/// la quantité en stock ne peut pas être changée par erreur depuis cette fenêtre.
/// </summary>
public class EditProductViewModel : DialogViewModel
{
    private readonly StockService _stockService;
    private readonly int _productId;

    private string _name;

    public EditProductViewModel(StockService stockService, Product product)
    {
        _stockService = stockService;
        _productId = product.Id;
        _name = product.Name;

        CurrentStock = product.Quantity;
    }

    /// <summary>Stock actuel, affiché en lecture seule.</summary>
    public int CurrentStock { get; }

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                ClearError();
            }
        }
    }

    protected override bool TryConfirm()
    {
        try
        {
            _stockService.RenameProduct(_productId, Name);
            return true;
        }
        catch (StockException exception)
        {
            ErrorMessage = exception.Message;
            return false;
        }
    }
}

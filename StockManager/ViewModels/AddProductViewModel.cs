using StockManager.Services;

namespace StockManager.ViewModels;

/// <summary>Fenêtre « Ajouter un produit ».</summary>
public class AddProductViewModel : DialogViewModel
{
    private readonly StockService _stockService;

    private string _name = string.Empty;
    private string _quantity = "0";

    public AddProductViewModel(StockService stockService)
    {
        _stockService = stockService;
    }

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

    protected override bool TryConfirm()
    {
        try
        {
            var quantity = QuantityParser.ParseInitialQuantity(Quantity);
            _stockService.CreateProduct(Name, quantity);
            return true;
        }
        catch (StockException exception)
        {
            ErrorMessage = exception.Message;
            return false;
        }
    }
}

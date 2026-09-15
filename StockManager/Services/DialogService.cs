using System.Windows;
using StockManager.Models;
using StockManager.ViewModels;
using StockManager.Views;

namespace StockManager.Services;

/// <summary>
/// Ouvre les fenêtres modales de l'application et renvoie le résultat
/// au ViewModel appelant.
/// </summary>
public class DialogService : IDialogService
{
    private readonly StockService _stockService;

    public DialogService(StockService stockService)
    {
        _stockService = stockService;
    }

    /// <summary>Fenêtre principale, utilisée comme parente des fenêtres modales.</summary>
    public Window? Owner { get; set; }

    public bool ShowAddStock(Product product)
        => ShowDialog(new AddStockWindow(new AddStockViewModel(_stockService, product)));

    public bool ShowRemoveStock(Product product)
        => ShowDialog(new RemoveStockWindow(new RemoveStockViewModel(_stockService, product)));

    public bool ShowAddProduct()
        => ShowDialog(new AddProductWindow(new AddProductViewModel(_stockService)));

    public bool ShowEditProduct(Product product)
        => ShowDialog(new EditProductWindow(new EditProductViewModel(_stockService, product)));

    public bool ConfirmDeleteProduct(string productName)
        => ShowDialog(new ConfirmationWindow(ConfirmationViewModel.ForProductDeletion(productName)));

    public void ShowError(string message)
        => ShowDialog(new ConfirmationWindow(ConfirmationViewModel.ForError(message)));

    private bool ShowDialog(Window window)
    {
        window.Owner = Owner;
        return window.ShowDialog() == true;
    }
}

using StockManager.Models;

namespace StockManager.Services;

/// <summary>
/// Ouverture des fenêtres secondaires depuis les ViewModels.
/// Chaque méthode renvoie <c>true</c> lorsque l'opération a été confirmée.
/// </summary>
public interface IDialogService
{
    bool ShowAddProduct();

    bool ShowEditProduct(Product product);

    bool ConfirmDeleteProduct(string productName);

    void ShowError(string message);
}

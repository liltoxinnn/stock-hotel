using System.Windows;
using StockManager.ViewModels;

namespace StockManager.Views;

/// <summary>
/// Relie une fenêtre modale à son ViewModel : la fenêtre se ferme
/// lorsque le ViewModel signale que l'opération est terminée.
/// </summary>
internal static class DialogWindow
{
    public static void Attach(Window window, DialogViewModel viewModel)
    {
        window.DataContext = viewModel;
        viewModel.CloseRequested += (_, result) => window.DialogResult = result;
    }
}

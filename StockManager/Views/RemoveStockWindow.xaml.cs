using System.Windows;
using StockManager.ViewModels;

namespace StockManager.Views;

/// <summary>Fenêtre « Retirer du stock ».</summary>
public partial class RemoveStockWindow : Window
{
    public RemoveStockWindow(RemoveStockViewModel viewModel)
    {
        InitializeComponent();
        DialogWindow.Attach(this, viewModel);

        NumericInput.Restrict(QuantityBox);
        Loaded += (_, _) => QuantityBox.SelectAll();
    }
}

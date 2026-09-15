using System.Windows;
using StockManager.ViewModels;

namespace StockManager.Views;

/// <summary>Fenêtre « Ajouter au stock ».</summary>
public partial class AddStockWindow : Window
{
    public AddStockWindow(AddStockViewModel viewModel)
    {
        InitializeComponent();
        DialogWindow.Attach(this, viewModel);

        NumericInput.Restrict(QuantityBox);
        Loaded += (_, _) => QuantityBox.SelectAll();
    }
}

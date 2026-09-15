using System.Windows;
using StockManager.ViewModels;

namespace StockManager.Views;

/// <summary>Fenêtre « Ajouter un produit ».</summary>
public partial class AddProductWindow : Window
{
    public AddProductWindow(AddProductViewModel viewModel)
    {
        InitializeComponent();
        DialogWindow.Attach(this, viewModel);

        NumericInput.Restrict(QuantityBox);
        Loaded += (_, _) => NameBox.Focus();
    }
}

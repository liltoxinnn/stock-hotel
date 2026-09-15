using System.Windows;
using StockManager.ViewModels;

namespace StockManager.Views;

/// <summary>Fenêtre « Modifier le produit ».</summary>
public partial class EditProductWindow : Window
{
    public EditProductWindow(EditProductViewModel viewModel)
    {
        InitializeComponent();
        DialogWindow.Attach(this, viewModel);

        Loaded += (_, _) => NameBox.SelectAll();
    }
}

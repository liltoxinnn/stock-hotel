using System.Windows;
using System.Windows.Controls;
using StockManager.ViewModels;

namespace StockManager.Views;

public partial class StockView : UserControl
{
    public StockView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Clic en dehors du champ de quantité : la saisie est validée comme
    /// avec la touche Entrée.
    /// </summary>
    private void QuantityEditor_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: ProductViewModel product })
        {
            product.CommitEdit();
        }
    }
}

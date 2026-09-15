using System.Windows;
using System.Windows.Media;
using StockManager.ViewModels;

namespace StockManager.Views;

/// <summary>
/// Fenêtre de confirmation (suppression d'un produit) ou d'information (erreur).
/// </summary>
public partial class ConfirmationWindow : Window
{
    public ConfirmationWindow(ConfirmationViewModel viewModel)
    {
        InitializeComponent();
        DialogWindow.Attach(this, viewModel);

        if (!viewModel.IsDestructive)
        {
            // Message d'information : bouton neutre et pastille bleue.
            ConfirmButton.Style = (Style)FindResource("PrimaryButtonStyle");
            IconBadge.Background = (Brush)FindResource("PrimarySoftBrush");
            IconPath.Stroke = (Brush)FindResource("PrimaryBrush");
        }

        Loaded += (_, _) => ConfirmButton.Focus();
    }
}

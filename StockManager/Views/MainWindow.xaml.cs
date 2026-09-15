using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace StockManager.Views;

public partial class MainWindow : Window
{
    private WindowState _stateBeforeFullScreen = WindowState.Normal;
    private bool _isFullScreen;

    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>F11 bascule le plein écran, Échap en sort.</summary>
    private void OnWindowKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F11)
        {
            ToggleFullScreen();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && _isFullScreen)
        {
            ExitFullScreen();
            e.Handled = true;
        }
    }

    private void OnFullScreenButtonClick(object sender, RoutedEventArgs e) => ToggleFullScreen();

    private void ToggleFullScreen()
    {
        if (_isFullScreen)
        {
            ExitFullScreen();
        }
        else
        {
            EnterFullScreen();
        }
    }

    private void EnterFullScreen()
    {
        _stateBeforeFullScreen = WindowState;

        // Une fenêtre déjà agrandie ne recouvre la barre des tâches qu'après
        // être repassée par l'état normal.
        WindowState = WindowState.Normal;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        WindowState = WindowState.Maximized;

        _isFullScreen = true;
        UpdateFullScreenButton();
    }

    private void ExitFullScreen()
    {
        WindowStyle = WindowStyle.SingleBorderWindow;
        ResizeMode = ResizeMode.CanResize;
        WindowState = _stateBeforeFullScreen;

        _isFullScreen = false;
        UpdateFullScreenButton();
    }

    private void UpdateFullScreenButton()
    {
        FullScreenIcon.Data = (Geometry)FindResource(
            _isFullScreen ? "IconExitFullscreen" : "IconFullscreen");

        FullScreenButton.ToolTip = _isFullScreen
            ? "Quitter le plein écran (F11 ou Échap)"
            : "Passer en plein écran (F11)";
    }
}

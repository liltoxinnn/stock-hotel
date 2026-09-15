using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using StockManager.Data;
using StockManager.Services;
using StockManager.ViewModels;
using StockManager.Views;

namespace StockManager;

/// <summary>
/// Point d'entrée de l'application : préparation de la base de données
/// puis ouverture de la fenêtre principale.
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ApplyFrenchCulture();

        DispatcherUnhandledException += OnUnhandledException;

        var stockService = new StockService(() => new AppDbContext(AppPaths.DatabaseFile));

        try
        {
            // Crée la base, les tables et l'inventaire de départ si nécessaire.
            stockService.Initialize();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                "Impossible d'ouvrir la base de données de l'inventaire.\n\n" +
                $"Emplacement : {AppPaths.DatabaseFile}\n\n" +
                $"Détail : {exception.Message}",
                "Gestion de stock",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(1);
            return;
        }

        var dialogService = new DialogService(stockService);
        var mainWindow = new MainWindow
        {
            DataContext = new MainViewModel(stockService, dialogService)
        };

        dialogService.Owner = mainWindow;

        MainWindow = mainWindow;
        mainWindow.Show();
    }

    /// <summary>Dates et nombres au format français dans toute l'interface.</summary>
    private static void ApplyFrenchCulture()
    {
        try
        {
            var culture = new CultureInfo("fr-FR");

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));
        }
        catch (CultureNotFoundException)
        {
            // La culture française n'est pas installée : on garde celle du système.
        }
    }

    /// <summary>
    /// Filet de sécurité : une erreur inattendue affiche un message
    /// au lieu de fermer brutalement l'application.
    /// </summary>
    private void OnUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            "Une erreur inattendue est survenue.\n\n" + e.Exception.Message,
            "Gestion de stock",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        e.Handled = true;
    }
}

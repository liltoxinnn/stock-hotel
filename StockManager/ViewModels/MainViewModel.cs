using System.Windows.Input;
using StockManager.Core;
using StockManager.Services;

namespace StockManager.ViewModels;

/// <summary>
/// Fenêtre principale : en-tête, navigation (Stock / Historique / Paramètres)
/// et page actuellement affichée.
/// </summary>
public class MainViewModel : ObservableObject
{
    private object _currentPage;

    public MainViewModel(StockService stockService, IDialogService dialogService)
    {
        Stock = new StockViewModel(stockService, dialogService);
        History = new HistoryViewModel(stockService);
        Settings = new SettingsViewModel(stockService, Stock.Refresh);

        NavigateCommand = new RelayCommand(parameter => Navigate(parameter as string));

        Stock.Refresh();
        _currentPage = Stock;
    }

    public StockViewModel Stock { get; }

    public HistoryViewModel History { get; }

    public SettingsViewModel Settings { get; }

    public ICommand NavigateCommand { get; }

    public object CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (SetProperty(ref _currentPage, value))
            {
                OnPropertyChanged(nameof(IsStockActive));
                OnPropertyChanged(nameof(IsHistoryActive));
                OnPropertyChanged(nameof(IsSettingsActive));
                OnPropertyChanged(nameof(PageTitle));
                OnPropertyChanged(nameof(PageSubtitle));
            }
        }
    }

    public bool IsStockActive => ReferenceEquals(CurrentPage, Stock);

    public bool IsHistoryActive => ReferenceEquals(CurrentPage, History);

    public bool IsSettingsActive => ReferenceEquals(CurrentPage, Settings);

    public string PageTitle => CurrentPage switch
    {
        HistoryViewModel => "HISTORIQUE",
        SettingsViewModel => "PARAMÈTRES",
        _ => "GESTION DE STOCK"
    };

    public string PageSubtitle => CurrentPage switch
    {
        HistoryViewModel => "Entrées et sorties enregistrées",
        SettingsViewModel => "Réglages de l'application",
        _ => "Gestion simple de votre inventaire"
    };

    /// <summary>Affiche une page et recharge ses données.</summary>
    private void Navigate(string? destination)
    {
        switch (destination)
        {
            case "history":
                History.Refresh();
                CurrentPage = History;
                break;

            case "settings":
                Settings.Refresh();
                CurrentPage = Settings;
                break;

            default:
                Stock.Refresh();
                CurrentPage = Stock;
                break;
        }
    }
}

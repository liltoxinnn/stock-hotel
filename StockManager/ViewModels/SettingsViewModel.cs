using System.Globalization;
using System.Windows.Input;
using StockManager.Core;
using StockManager.Data;
using StockManager.Services;

namespace StockManager.ViewModels;

/// <summary>
/// Page « Paramètres » : seuil de stock faible et emplacement des données.
/// </summary>
public class SettingsViewModel : ObservableObject
{
    private readonly StockService _stockService;
    private readonly Action _thresholdChanged;

    private string _lowStockThresholdText = string.Empty;
    private string _errorMessage = string.Empty;
    private string _confirmationMessage = string.Empty;

    public SettingsViewModel(StockService stockService, Action thresholdChanged)
    {
        _stockService = stockService;
        _thresholdChanged = thresholdChanged;

        SaveCommand = new RelayCommand(Save);
    }

    public ICommand SaveCommand { get; }

    /// <summary>Emplacement du fichier de base de données, affiché à titre indicatif.</summary>
    public string DatabaseFile => AppPaths.DatabaseFile;

    public string LowStockThresholdText
    {
        get => _lowStockThresholdText;
        set
        {
            if (SetProperty(ref _lowStockThresholdText, value))
            {
                ErrorMessage = string.Empty;
                ConfirmationMessage = string.Empty;
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public string ConfirmationMessage
    {
        get => _confirmationMessage;
        private set
        {
            if (SetProperty(ref _confirmationMessage, value))
            {
                OnPropertyChanged(nameof(HasConfirmation));
            }
        }
    }

    public bool HasConfirmation => !string.IsNullOrEmpty(ConfirmationMessage);

    /// <summary>Relit le seuil enregistré.</summary>
    public void Refresh()
    {
        ErrorMessage = string.Empty;
        ConfirmationMessage = string.Empty;
        LowStockThresholdText = _stockService.GetLowStockThreshold().ToString(CultureInfo.InvariantCulture);
    }

    private void Save()
    {
        try
        {
            var threshold = QuantityParser.ParseThreshold(LowStockThresholdText);
            _stockService.SetLowStockThreshold(threshold);

            ErrorMessage = string.Empty;
            ConfirmationMessage = "Le seuil a bien été enregistré.";

            _thresholdChanged();
        }
        catch (StockException exception)
        {
            ConfirmationMessage = string.Empty;
            ErrorMessage = exception.Message;
        }
    }
}

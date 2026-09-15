using System.Collections.ObjectModel;
using StockManager.Core;
using StockManager.Services;

namespace StockManager.ViewModels;

/// <summary>
/// Page « Historique » : entrées et sorties, du plus récent au plus ancien.
/// </summary>
public class HistoryViewModel : ObservableObject
{
    private readonly StockService _stockService;

    public HistoryViewModel(StockService stockService)
    {
        _stockService = stockService;
    }

    public ObservableCollection<MovementViewModel> Movements { get; } = new();

    public bool HasMovements => Movements.Count > 0;

    public bool IsEmpty => Movements.Count == 0;

    /// <summary>Recharge l'historique depuis la base de données.</summary>
    public void Refresh()
    {
        Movements.Clear();

        foreach (var movement in _stockService.GetMovements())
        {
            Movements.Add(new MovementViewModel(movement));
        }

        OnPropertyChanged(nameof(HasMovements));
        OnPropertyChanged(nameof(IsEmpty));
    }
}

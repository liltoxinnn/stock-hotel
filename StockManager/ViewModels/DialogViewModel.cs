using System.Windows.Input;
using StockManager.Core;

namespace StockManager.ViewModels;

/// <summary>
/// Base des ViewModels de fenêtres : validation, message d'erreur affiché
/// dans la fenêtre et demande de fermeture.
/// L'annulation est gérée par le bouton « Annuler » (IsCancel) de la fenêtre.
/// </summary>
public abstract class DialogViewModel : ObservableObject
{
    private string _errorMessage = string.Empty;

    protected DialogViewModel()
    {
        ConfirmCommand = new RelayCommand(Confirm);
    }

    /// <summary>Demande de fermeture de la fenêtre ; <c>true</c> si l'opération a été validée.</summary>
    public event EventHandler<bool>? CloseRequested;

    public ICommand ConfirmCommand { get; }

    /// <summary>Message d'erreur en français affiché dans la fenêtre.</summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        protected set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>Exécute l'opération. Renvoie <c>false</c> et remplit <see cref="ErrorMessage"/> en cas de refus.</summary>
    protected abstract bool TryConfirm();

    protected void ClearError() => ErrorMessage = string.Empty;

    private void Confirm()
    {
        if (TryConfirm())
        {
            CloseRequested?.Invoke(this, true);
        }
    }
}

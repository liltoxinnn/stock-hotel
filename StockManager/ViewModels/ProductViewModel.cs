using StockManager.Core;
using StockManager.Models;

namespace StockManager.ViewModels;

/// <summary>
/// Une ligne du tableau de stock.
/// La quantité affichée peut être ajustée avec les boutons − et + sans rien
/// enregistrer : la modification n'est écrite en base qu'une fois confirmée.
/// </summary>
public class ProductViewModel : ObservableObject
{
    /// <summary>Quantité maximale acceptée pour un produit.</summary>
    public const int MaxQuantity = 9_999_999;

    private readonly int _lowStockThreshold;

    private int _pendingQuantity;
    private bool _isEditing;
    private string _editText = string.Empty;

    public ProductViewModel(Product product, int lowStockThreshold)
    {
        Model = product;
        _lowStockThreshold = lowStockThreshold;
        _pendingQuantity = product.Quantity;
    }

    public Product Model { get; }

    public int Id => Model.Id;

    public string Name => Model.Name;

    /// <summary>Quantité réellement enregistrée en base.</summary>
    public int Quantity => Model.Quantity;

    /// <summary>Quantité affichée dans la ligne, ajustée par les boutons − et +.</summary>
    public int PendingQuantity
    {
        get => _pendingQuantity;
        private set
        {
            if (SetProperty(ref _pendingQuantity, value))
            {
                RaiseChangeState();
            }
        }
    }

    /// <summary>Un ajustement attend d'être confirmé.</summary>
    public bool HasPendingChange => PendingQuantity != Quantity;

    /// <summary>Aucun ajustement en attente : les actions habituelles sont disponibles.</summary>
    public bool HasNoPendingChange => !HasPendingChange;

    /// <summary>Écart entre la quantité affichée et la quantité enregistrée.</summary>
    public int Delta => PendingQuantity - Quantity;

    /// <summary>Libellé du bouton de validation, par exemple « Confirmer +3 ».</summary>
    public string ConfirmLabel => Delta > 0 ? $"Confirmer +{Delta}" : $"Confirmer {Delta}";

    /// <summary>La quantité est en cours de saisie au clavier.</summary>
    public bool IsEditing
    {
        get => _isEditing;
        private set => SetProperty(ref _isEditing, value);
    }

    /// <summary>Contenu du champ de saisie de la quantité.</summary>
    public string EditText
    {
        get => _editText;
        set => SetProperty(ref _editText, value);
    }

    // Les indicateurs portent sur la quantité enregistrée, pas sur l'ajustement
    // en attente : rien n'est signalé tant que la modification n'est pas confirmée.
    public bool IsOutOfStock => Quantity <= 0;

    public bool IsLowStock => !IsOutOfStock && Quantity <= _lowStockThreshold;

    public bool NeedsAttention => IsOutOfStock || IsLowStock;

    /// <summary>Ajoute une unité à la quantité affichée.</summary>
    public void Increment()
    {
        if (PendingQuantity < MaxQuantity)
        {
            PendingQuantity++;
        }
    }

    /// <summary>Retire une unité de la quantité affichée, sans jamais passer sous zéro.</summary>
    public void Decrement()
    {
        if (PendingQuantity > 0)
        {
            PendingQuantity--;
        }
    }

    /// <summary>Abandonne l'ajustement en cours et revient à la quantité enregistrée.</summary>
    public void CancelPending()
    {
        IsEditing = false;
        PendingQuantity = Quantity;
    }

    /// <summary>Passe la quantité en saisie clavier (double-clic sur le nombre).</summary>
    public void BeginEdit()
    {
        EditText = PendingQuantity.ToString();
        IsEditing = true;
    }

    /// <summary>
    /// Termine la saisie clavier. Une valeur inutilisable est simplement ignorée :
    /// la quantité affichée reste inchangée.
    /// </summary>
    public void CommitEdit()
    {
        if (!IsEditing)
        {
            return;
        }

        IsEditing = false;

        if (int.TryParse(EditText, out var value) && value >= 0 && value <= MaxQuantity)
        {
            PendingQuantity = value;
        }
    }

    /// <summary>Prend en compte la quantité renvoyée par la base après enregistrement.</summary>
    public void ApplySavedQuantity(int quantity)
    {
        Model.Quantity = quantity;
        IsEditing = false;
        PendingQuantity = quantity;

        OnPropertyChanged(nameof(Quantity));
        OnPropertyChanged(nameof(IsOutOfStock));
        OnPropertyChanged(nameof(IsLowStock));
        OnPropertyChanged(nameof(NeedsAttention));
        RaiseChangeState();
    }

    private void RaiseChangeState()
    {
        OnPropertyChanged(nameof(HasPendingChange));
        OnPropertyChanged(nameof(HasNoPendingChange));
        OnPropertyChanged(nameof(Delta));
        OnPropertyChanged(nameof(ConfirmLabel));
    }
}

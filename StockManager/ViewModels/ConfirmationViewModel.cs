namespace StockManager.ViewModels;

/// <summary>
/// Fenêtre de confirmation ou d'information (suppression d'un produit, message d'erreur).
/// </summary>
public class ConfirmationViewModel : DialogViewModel
{
    private ConfirmationViewModel(string title, string message, string confirmLabel, bool isDestructive, bool showCancel)
    {
        Title = title;
        Message = message;
        ConfirmLabel = confirmLabel;
        IsDestructive = isDestructive;
        ShowCancel = showCancel;
    }

    public string Title { get; }

    public string Message { get; }

    public string ConfirmLabel { get; }

    /// <summary>Action irréversible : le bouton de validation est affiché en rouge.</summary>
    public bool IsDestructive { get; }

    public bool ShowCancel { get; }

    /// <summary>Demande de confirmation avant la suppression d'un produit.</summary>
    public static ConfirmationViewModel ForProductDeletion(string productName) => new(
        "Supprimer ce produit ?",
        $"Voulez-vous vraiment supprimer « {productName} » ?\nSon historique de mouvements sera également supprimé.",
        "Supprimer",
        isDestructive: true,
        showCancel: true);

    /// <summary>Message d'erreur avec un seul bouton.</summary>
    public static ConfirmationViewModel ForError(string message) => new(
        "Une erreur est survenue",
        message,
        "Fermer",
        isDestructive: false,
        showCancel: false);

    protected override bool TryConfirm() => true;
}

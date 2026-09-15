namespace StockManager.Services;

/// <summary>
/// Erreur métier dont le message est directement affichable à l'utilisateur (en français).
/// </summary>
public class StockException : Exception
{
    public StockException(string message)
        : base(message)
    {
    }
}

using System.Globalization;

namespace StockManager.Services;

/// <summary>
/// Lecture et contrôle des quantités saisies par l'utilisateur.
/// Lève une <see cref="StockException"/> dont le message est affichable tel quel.
/// </summary>
public static class QuantityParser
{
    /// <summary>Quantité d'un mouvement : nombre entier strictement positif.</summary>
    public static int ParseMovementQuantity(string? input)
    {
        var value = ParseInteger(input);

        if (value <= 0)
        {
            throw new StockException("La quantité doit être supérieure à 0.");
        }

        return value;
    }

    /// <summary>Quantité initiale d'un produit : nombre entier positif ou nul.</summary>
    public static int ParseInitialQuantity(string? input)
    {
        var value = ParseInteger(input);

        if (value < 0)
        {
            throw new StockException("La quantité ne peut pas être négative.");
        }

        return value;
    }

    /// <summary>Seuil de stock faible : nombre entier entre 0 et 1000.</summary>
    public static int ParseThreshold(string? input)
    {
        int value;

        try
        {
            value = ParseInteger(input);
        }
        catch (StockException)
        {
            throw new StockException("Veuillez entrer un seuil valide.");
        }

        if (value < 0 || value > 1000)
        {
            throw new StockException("Le seuil doit être compris entre 0 et 1000.");
        }

        return value;
    }

    private static int ParseInteger(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new StockException("Veuillez entrer une quantité valide.");
        }

        // Les espaces (y compris insécables) servent parfois de séparateur de milliers.
        var cleaned = input.Replace(" ", string.Empty)
            .Replace(" ", string.Empty)
            .Replace(" ", string.Empty)
            .Trim();

        if (!int.TryParse(cleaned, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value))
        {
            throw new StockException("Veuillez entrer une quantité valide.");
        }

        return value;
    }
}

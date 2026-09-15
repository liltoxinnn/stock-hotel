using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StockManager.Converters;

/// <summary>
/// Affiche ou masque un élément selon un booléen.
/// <see cref="Invert"/> permet d'inverser la logique (masquer quand la valeur est vraie).
/// </summary>
public class BooleanToVisibilityConverter : IValueConverter
{
    /// <summary>Inverse le résultat : <c>true</c> masque l'élément.</summary>
    public bool Invert { get; set; }

    /// <summary>Utilise <see cref="Visibility.Hidden"/> au lieu de <see cref="Visibility.Collapsed"/>.</summary>
    public bool UseHidden { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var flag = value is bool boolean && boolean;

        if (Invert)
        {
            flag = !flag;
        }

        return flag
            ? Visibility.Visible
            : UseHidden ? Visibility.Hidden : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

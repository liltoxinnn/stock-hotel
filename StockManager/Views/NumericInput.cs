using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StockManager.Views;

/// <summary>
/// Limite la saisie d'un champ aux chiffres. La valeur reste vérifiée
/// au moment de la validation : ce filtre n'est qu'un confort de saisie.
/// </summary>
internal static class NumericInput
{
    public static void Restrict(TextBox textBox)
    {
        textBox.PreviewTextInput += OnPreviewTextInput;
        DataObject.AddPastingHandler(textBox, OnPaste);
    }

    private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !IsDigitsOnly(e.Text);
    }

    private static void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        var pasted = e.DataObject.GetDataPresent(DataFormats.UnicodeText)
            ? e.DataObject.GetData(DataFormats.UnicodeText) as string
            : null;

        if (!IsDigitsOnly(pasted))
        {
            e.CancelCommand();
        }
    }

    private static bool IsDigitsOnly(string? text)
        => !string.IsNullOrEmpty(text) && text.All(char.IsDigit);
}

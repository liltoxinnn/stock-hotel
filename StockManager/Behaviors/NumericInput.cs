using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StockManager.Behaviors;

/// <summary>
/// Limite la saisie d'un champ aux chiffres :
/// <c>behaviors:NumericInput.DigitsOnly="True"</c>.
/// La valeur reste vérifiée à la validation ; ce filtre n'est qu'un confort de saisie.
/// </summary>
public static class NumericInput
{
    public static readonly DependencyProperty DigitsOnlyProperty =
        DependencyProperty.RegisterAttached(
            "DigitsOnly",
            typeof(bool),
            typeof(NumericInput),
            new PropertyMetadata(false, OnDigitsOnlyChanged));

    public static void SetDigitsOnly(DependencyObject element, bool value)
        => element.SetValue(DigitsOnlyProperty, value);

    public static bool GetDigitsOnly(DependencyObject element)
        => (bool)element.GetValue(DigitsOnlyProperty);

    private static void OnDigitsOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox)
        {
            return;
        }

        textBox.PreviewTextInput -= OnPreviewTextInput;
        DataObject.RemovePastingHandler(textBox, OnPaste);

        if (e.NewValue is true)
        {
            textBox.PreviewTextInput += OnPreviewTextInput;
            DataObject.AddPastingHandler(textBox, OnPaste);
        }
    }

    private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        => e.Handled = !IsDigitsOnly(e.Text);

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

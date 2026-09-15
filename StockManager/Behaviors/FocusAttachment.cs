using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace StockManager.Behaviors;

/// <summary>
/// Donne le focus à un champ et sélectionne son contenu dès qu'il devient visible :
/// <c>behaviors:FocusAttachment.IsFocused="{Binding IsEditing}"</c>.
/// </summary>
public static class FocusAttachment
{
    public static readonly DependencyProperty IsFocusedProperty =
        DependencyProperty.RegisterAttached(
            "IsFocused",
            typeof(bool),
            typeof(FocusAttachment),
            new PropertyMetadata(false, OnIsFocusedChanged));

    public static void SetIsFocused(DependencyObject element, bool value)
        => element.SetValue(IsFocusedProperty, value);

    public static bool GetIsFocused(DependencyObject element)
        => (bool)element.GetValue(IsFocusedProperty);

    private static void OnIsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox || e.NewValue is not true)
        {
            return;
        }

        // Le champ vient seulement d'être rendu visible : on attend que la mise
        // en page soit terminée avant de lui donner le focus.
        textBox.Dispatcher.BeginInvoke(
            DispatcherPriority.Input,
            new Action(() =>
            {
                textBox.Focus();
                textBox.SelectAll();
            }));
    }
}

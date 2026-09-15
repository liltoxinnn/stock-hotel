namespace StockManager.Models;

/// <summary>
/// Paramètre de l'application conservé dans la base (clé / valeur).
/// </summary>
public class AppSetting
{
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}

// Les projets WPF n'importent pas System.IO implicitement.
using System.IO;

namespace StockManager.Data;

/// <summary>
/// Emplacements des fichiers de l'application sur le poste de l'utilisateur.
/// </summary>
public static class AppPaths
{
    /// <summary>Dossier de données : %APPDATA%\StockManager</summary>
    public static string DataDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "StockManager");

    /// <summary>Fichier SQLite contenant l'inventaire.</summary>
    public static string DatabaseFile { get; } = Path.Combine(DataDirectory, "stock.db");

    /// <summary>Crée le dossier de données s'il n'existe pas encore.</summary>
    public static void EnsureDataDirectory() => Directory.CreateDirectory(DataDirectory);
}

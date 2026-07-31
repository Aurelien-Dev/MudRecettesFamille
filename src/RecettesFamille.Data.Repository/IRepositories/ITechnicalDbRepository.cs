namespace RecettesFamille.Data.Repository.IRepositories;

/// <summary>
/// Interface pour les opérations techniques sur la base de données.
/// Fournit des méthodes pour la maintenance et l'administration de la base de données.
/// </summary>
public interface ITechnicalDbRepository
{
    /// <summary>
    /// Déclenche une sauvegarde de la base de données.
    /// </summary>
    /// <returns>
    /// Un tuple contenant:
    /// - Un booléen indiquant si la sauvegarde a réussi
    /// - Le message de statut de l'opération
    /// - Le chemin vers le fichier de sauvegarde
    /// </returns>
    (bool, string, string) TriggerBackup();

    /// <summary>
    /// Récupère la liste de toutes les migrations définies dans le code.
    /// </summary>
    /// <returns>Liste des noms de migrations</returns>
    Task<IEnumerable<string>> GetAllMigrationsAsync();

    /// <summary>
    /// Récupère la liste des migrations déjà appliquées à la base de données.
    /// </summary>
    /// <returns>Liste des noms de migrations appliquées</returns>
    Task<IEnumerable<string>> GetAppliedMigrationsAsync();

    /// <summary>
    /// Récupère la liste des migrations en attente (non appliquées).
    /// </summary>
    /// <returns>Liste des noms de migrations en attente</returns>
    Task<IEnumerable<string>> GetPendingMigrationsAsync();

    /// <summary>
    /// Applique toutes les migrations en attente à la base de données.
    /// </summary>
    /// <returns>
    /// Un tuple contenant:
    /// - Un booléen indiquant si l'opération a réussi
    /// - Le message de statut de l'opération
    /// </returns>
    Task<(bool success, string message)> ApplyMigrationsAsync();
}

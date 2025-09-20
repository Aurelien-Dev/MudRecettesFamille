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
}

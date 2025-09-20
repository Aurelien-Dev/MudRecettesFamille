﻿using Microsoft.AspNetCore.Identity;

namespace RecettesFamille.Data.Repository.IRepositories;

/// <summary>
/// Interface pour la gestion des utilisateurs dans l'application RecettesFamille.
/// Fournit des méthodes pour gérer les utilisateurs et leurs rôles.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Assigne un rôle spécifique à un utilisateur.
    /// </summary>
    /// <param name="userEmail">L'adresse email de l'utilisateur.</param>
    /// <param name="role">Le rôle à assigner (Admin, Contributor, Reader, etc.).</param>
    /// <returns>Une tâche représentant l'opération asynchrone.</returns>
    Task AssignRoleAsync(string userEmail, string role);

    /// <summary>
    /// Récupère tous les utilisateurs avec leurs rôles respectifs.
    /// </summary>
    /// <returns>Une liste de tuples contenant les utilisateurs et leurs rôles associés.</returns>
    Task<List<(ApplicationUser User, List<string> Roles)>> GetAllUsersWithRolesAsync();

    /// <summary>
    /// Retire un rôle spécifique d'un utilisateur.
    /// </summary>
    /// <param name="userEmail">L'adresse email de l'utilisateur.</param>
    /// <param name="role">Le rôle à retirer (Admin, Contributor, Reader, etc.).</param>
    /// <returns>Une tâche représentant l'opération asynchrone.</returns>
    Task RemoveRoleAsync(string userEmail, string role);

    /// <summary>
    /// Supprime un utilisateur du système.
    /// </summary>
    /// <param name="userEmail">L'adresse email de l'utilisateur à supprimer.</param>
    /// <returns>Un résultat d'identité indiquant si l'opération a réussi ou échoué.</returns>
    /// <remarks>
    /// Cette opération supprime définitivement l'utilisateur et toutes ses informations associées.
    /// Assurez-vous que les données liées à l'utilisateur sont correctement gérées avant de l'appeler.
    /// </remarks>
    Task<IdentityResult> DeleteUserAsync(string userEmail);
}
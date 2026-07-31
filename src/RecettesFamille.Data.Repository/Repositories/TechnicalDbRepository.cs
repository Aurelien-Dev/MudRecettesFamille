using Microsoft.EntityFrameworkCore;
using RecettesFamille.Data;
using RecettesFamille.Data.Repository.IRepositories;

namespace RecettesFamille.Data.Repository.Repositories;

public class TechnicalDbRepository : ITechnicalDbRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public TechnicalDbRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public (bool, string, string) TriggerBackup()
    {
        return ApplicationDbContext.TriggerBackup();
    }

    public async Task<IEnumerable<string>> GetAllMigrationsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var migrations = context.Database.GetMigrations();
        return migrations;
    }

    public async Task<IEnumerable<string>> GetAppliedMigrationsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
        return appliedMigrations;
    }

    public async Task<IEnumerable<string>> GetPendingMigrationsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        return pendingMigrations;
    }

    public async Task<(bool success, string message)> ApplyMigrationsAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

            if (!pendingMigrations.Any())
            {
                return (true, "Aucune migration en attente. La base de données est à jour.");
            }

            await context.Database.MigrateAsync();

            return (true, $"{pendingMigrations.Count()} migration(s) appliquée(s) avec succès.");
        }
        catch (Exception ex)
        {
            return (false, $"Erreur lors de l'application des migrations : {ex.Message}");
        }
    }
}

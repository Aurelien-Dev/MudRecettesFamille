using RecettesFamille.Data.Repository.IRepositories;

namespace RecettesFamille.Data.Repository.Repositories;

public class TechnicalDbRepository(ApplicationDbContext context) : ITechnicalDbRepository
{
    public (bool, string, string) TriggerBackup()
    {
        return context.TriggerBackup();
    }
}

using RecettesFamille.Data.Repository.IRepositories;

namespace RecettesFamille.Data.Repository.Repositories;

public class TechnicalDbRepository() : ITechnicalDbRepository
{
    public (bool, string, string) TriggerBackup()
    {
        return ApplicationDbContext.TriggerBackup();
    }
}

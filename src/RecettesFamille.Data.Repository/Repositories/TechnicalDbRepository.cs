using RecettesFamille.Data.Repository.IRepositories;

namespace RecettesFamille.Data.Repository.Repositories;

public class TechnicalDbRepository(ApplicationDbContext Context) : ITechnicalDbRepository
{
    public (bool, string, string) TriggerBackup()
    {
        return Context.TriggerBackup();
    }

}

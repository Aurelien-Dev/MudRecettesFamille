namespace RecettesFamille.Data.Repository.IRepositories;

public interface ITechnicalDbRepository
{
    (bool, string, string) TriggerBackup();
}

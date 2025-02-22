using RecettesFamille.Dto.Models;

namespace RecettesFamille.Data.Repository.IRepositories;

public interface ITagRepository
{
    Task<List<TagDto>> GetAll(CancellationToken cancellationToken = default);
    Task<List<TagDto>> GetAllVisible(CancellationToken cancellationToken = default);
    Task<TagDto> AddTag(TagDto tag, CancellationToken cancellationToken = default);
    Task<bool> AddTag(TagDto[] tags, CancellationToken cancellationToken = default);
    Task UpdateTag(TagDto tag, CancellationToken cancellationToken = default);
    Task<bool> DeleteTagOnRecipe(TagDto tag, CancellationToken cancellationToken = default);
}

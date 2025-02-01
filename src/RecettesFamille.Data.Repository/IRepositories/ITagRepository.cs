using RecettesFamille.Data.EntityModel;
using RecettesFamille.Dto.Models.Blocks;

namespace RecettesFamille.Data.Repository.IRepositories;

public interface ITagRepository
{
    Task<BlockBaseDto> AddTag(TagDto tag, CancellationToken cancellationToken = default);
    Task<BlockBaseDto> AddTag(TagDto[] tags, CancellationToken cancellationToken = default);
    Task UpdateBlock(TagDto tag, CancellationToken cancellationToken = default);
}

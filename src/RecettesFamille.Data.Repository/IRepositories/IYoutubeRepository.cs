using RecettesFamille.Data.EntityModel;
using RecettesFamille.Dto.Models;

namespace RecettesFamille.Data.Repository.IRepositories;

public interface IYoutubeRepository
{

    Task<YoutubeResumeDto> AddSummary(YoutubeResumeDto youtubeSummary, CancellationToken cancellationToken = default);
    Task<List<YoutubeResumeDto>> GetAllSummary(CancellationToken cancellationToken = default);
}
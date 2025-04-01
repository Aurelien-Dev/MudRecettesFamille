using RecettesFamille.Data.EntityModel;
using RecettesFamille.Dto.Models;

namespace RecettesFamille.Data.Repository.IRepositories;

public interface IYoutubeRepository
{

    Task<YoutubeSummaryRequestDto> AddSummary(YoutubeSummaryRequestDto youtubeSummary, CancellationToken cancellationToken = default);
}
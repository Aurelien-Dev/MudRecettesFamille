using RecettesFamille.Dto.Models;

namespace RecettesFamille.Data.Repository.IRepositories;

public interface IAiRepository
{
    Task<bool> AddPrompt(PromptDto prompt, CancellationToken cancellationToken = default);
    Task<PromptDto> GetPrompt(string promptName, CancellationToken cancellationToken = default);
    Task<List<PromptDto>> GetPrompt(CancellationToken cancellationToken = default);
    Task ReportConsumption(AiConsumptionDto aiConsumptionDto, CancellationToken cancellationToken = default);
    Task UpdatePrompt(PromptDto prompt, CancellationToken cancellationToken = default);
}
using RecettesFamille.Api;
using RecettesFamille.Data.Repository.IRepositories;
using RecettesFamille.Dto.Models;
using RecettesFamille.Managers.AiGenerators;
using RecettesFamille.Services.TravelPlanner.Sources;

namespace RecettesFamille.Services.TravelPlanner;

/// <summary>
/// Service orchestrateur pour la gestion des résumés de contenu.
/// Coordonne l'extraction de contenu, la génération AI, et les opérations de base de données.
/// </summary>
public class SummaryService : ISummaryService
{
    private readonly YoutubeSourceService _youtubeSourceService;
    private readonly IAiManager _aiManager;
    private readonly IYoutubeRepository _youtubeRepository;

    /// <summary>
    /// Initialise une nouvelle instance du service de résumés.
    /// </summary>
    public SummaryService(
        YoutubeSourceService youtubeSourceService,
        IAiManager aiManager,
        IYoutubeRepository youtubeRepository)
    {
        _youtubeSourceService = youtubeSourceService;
        _aiManager = aiManager;
        _youtubeRepository = youtubeRepository;
    }

    /// <inheritdoc/>
    public async Task<YoutubeResumeDto> CreateSummaryFromYoutube(string youtubeUrl, int? travelId = null, CancellationToken cancellationToken = default)
    {
        // 1. Extraire les métadonnées de la vidéo
        var metadata = await _youtubeSourceService.ExtractMetadata(youtubeUrl, cancellationToken);

        // 2. Récupérer le transcript
        var transcript = await _youtubeSourceService.GetContent(youtubeUrl, cancellationToken);

        // 3. Créer l'objet de requête pour l'AI Manager
        var youtubeSummaryRequest = new YoutubeSummaryJson
        {
            Transcript = transcript,
            Url = metadata.Url,
            Title = metadata.Title
        };

        // 4. Générer le résumé via l'AI Manager (qui gère aussi la sauvegarde en base)
        var resumeText = await _aiManager.GetYoutubeResume(youtubeSummaryRequest, travelId, cancellationToken);

        // 5. Retourner le résumé complet
        return new YoutubeResumeDto
        {
            Resume = resumeText,
            Url = metadata.Url,
            Title = metadata.Title,
            CreatedDate = DateTime.UtcNow,
            TravelId = travelId
        };
    }

    /// <inheritdoc/>
    public async Task<List<YoutubeResumeDto>> GetAllSummaries(CancellationToken cancellationToken = default)
    {
        return await _youtubeRepository.GetAllSummary(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<YoutubeResumeDto?> GetSummaryById(int id, CancellationToken cancellationToken = default)
    {
        var allSummaries = await _youtubeRepository.GetAllSummary(cancellationToken);
        return allSummaries.FirstOrDefault(s => s.Id == id);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteSummary(int id, CancellationToken cancellationToken = default)
    {
        return await _youtubeRepository.DeleteSummary(id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateSummaryTravel(int summaryId, int? travelId, CancellationToken cancellationToken = default)
    {
        return await _youtubeRepository.UpdateSummaryTravel(summaryId, travelId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateFavorite(int summaryId, bool isFavorite, CancellationToken cancellationToken = default)
    {
        return await _youtubeRepository.UpdateFavorite(summaryId, isFavorite, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateStatus(int summaryId, SummaryStatus status, CancellationToken cancellationToken = default)
    {
        return await _youtubeRepository.UpdateStatus(summaryId, status, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateCategories(int summaryId, List<int> categoryIds, CancellationToken cancellationToken = default)
    {
        return await _youtubeRepository.UpdateCategories(summaryId, categoryIds, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateTitle(int summaryId, string title, CancellationToken cancellationToken = default)
    {
        return await _youtubeRepository.UpdateTitle(summaryId, title, cancellationToken);
    }
}

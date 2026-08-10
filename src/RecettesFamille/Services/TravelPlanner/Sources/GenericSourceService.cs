using RecettesFamille.Services.Models;
using RecettesFamille.ServicesExternal;

namespace RecettesFamille.Services.TravelPlanner.Sources;

/// <summary>
/// Service générique pour l'extraction de contenu depuis toute URL supportée par Supadata.
/// Supporte YouTube, TikTok, Instagram, X (Twitter), Facebook et fichiers publics.
/// Utilisé comme service de base ou en fallback lorsqu'aucun service spécialisé ne prend en charge l'URL.
/// </summary>
public class GenericSourceService : IContentSourceService
{
    private readonly ISupadataService _supadataService;

    /// <inheritdoc/>
    public string SourceType => "Generic";

    /// <summary>
    /// Initialise une nouvelle instance du service générique.
    /// </summary>
    /// <param name="supadataService">Service pour les appels à l'API Supadata.</param>
    public GenericSourceService(ISupadataService supadataService)
    {
        _supadataService = supadataService;
    }

    /// <summary>
    /// Indique si ce service peut traiter l'URL donnée.
    /// Le service générique accepte toujours toute URL valide.
    /// </summary>
    public bool CanHandle(string url) => Uri.TryCreate(url, UriKind.Absolute, out _);

    /// <inheritdoc/>
    public async Task<ContentMetadata> ExtractMetadata(string sourceUrl, CancellationToken cancellationToken = default)
    {
        var metadataResponse = await _supadataService.GetMetadataAsync(sourceUrl, cancellationToken);

        var duration = metadataResponse.Media?.Duration is int seconds
            ? TimeSpan.FromSeconds(seconds)
            : (TimeSpan?)null;

        var publishedDate = DateTime.TryParse(metadataResponse.CreatedAt, out var dt)
            ? dt
            : (DateTime?)null;

        return new ContentMetadata(
            Title: metadataResponse.Title ?? sourceUrl,
            Url: metadataResponse.Url ?? sourceUrl,
            SourceType: metadataResponse.Platform ?? SourceType,
            Author: metadataResponse.Author?.DisplayName,
            PublishedDate: publishedDate,
            Duration: duration,
            ThumbnailUrl: metadataResponse.Media?.ThumbnailUrl
        );
    }

    /// <inheritdoc/>
    public async Task<string> GetContent(string sourceUrl, CancellationToken cancellationToken = default)
    {
        var transcriptResponse = await _supadataService.GetTranscriptAsync(sourceUrl, cancellationToken: cancellationToken);
        return transcriptResponse.Content;
    }
}

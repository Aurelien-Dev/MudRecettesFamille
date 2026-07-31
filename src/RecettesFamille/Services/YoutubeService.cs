using RecettesFamille.Api;
using RecettesFamille.Dto.Models;
using RecettesFamille.Managers.AiGenerators;
using RecettesFamille.Services.Models;
using System.Text.Json;

namespace RecettesFamille.Services;

/// <summary>
/// Service pour la gestion des opérations YouTube incluant l'extraction de transcripts
/// et la génération de résumés via l'intelligence artificielle.
/// </summary>
public class YoutubeService : IYoutubeService
{
    private readonly HttpClient _httpClient;
    private readonly IAiManager _aiManager;

    /// <summary>
    /// Initialise une nouvelle instance du service YouTube.
    /// </summary>
    /// <param name="httpClientFactory">Factory pour créer des instances HttpClient configurées.</param>
    /// <param name="aiManager">Manager pour les opérations d'intelligence artificielle.</param>
    public YoutubeService(IHttpClientFactory httpClientFactory, IAiManager aiManager)
    {
        _httpClient = httpClientFactory.CreateClient("Supadata");
        _aiManager = aiManager;
    }

    /// <inheritdoc/>
    public async Task<YoutubeResumeDto> GenerateSummaryFromUrl(string youtubeUrl, CancellationToken cancellationToken = default)
    {
        // 1. Valider et extraire le videoId depuis l'URL
        var videoId = ExtractVideoId(youtubeUrl);

        // 2. Récupérer le titre de la vidéo via YouTube oEmbed
        var videoTitle = await GetVideoTitle(videoId, cancellationToken);

        // 3. Récupérer le transcript depuis l'API Supadata
        var transcriptResponse = await GetTranscriptFromSupadata(videoId, cancellationToken);

        // 4. Construire l'URL YouTube standard pour la sauvegarde
        var standardYoutubeUrl = $"https://www.youtube.com/watch?v={videoId}";

        // 5. Créer l'objet de requête pour l'AI Manager
        var youtubeSummaryRequest = new YoutubeSummaryJson
        {
            Transcript = transcriptResponse.Content,
            Url = standardYoutubeUrl,
            Title = videoTitle ?? transcriptResponse.Title ?? $"Vidéo YouTube {videoId}"
        };

        // 6. Générer le résumé via l'AI Manager (qui gère aussi la sauvegarde en base)
        var resumeText = await _aiManager.GetYoutubeResume(youtubeSummaryRequest, cancellationToken);

        // 7. Retourner le résumé complet
        return new YoutubeResumeDto
        {
            Resume = resumeText,
            Url = standardYoutubeUrl,
            Title = youtubeSummaryRequest.Title,
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Extrait l'identifiant de la vidéo YouTube depuis différents formats d'URL.
    /// </summary>
    /// <param name="url">L'URL YouTube à analyser.</param>
    /// <returns>L'identifiant de la vidéo (11 caractères).</returns>
    /// <exception cref="ArgumentException">Lancée si l'URL n'est pas une URL YouTube valide ou si le videoId ne peut pas être extrait.</exception>
    private static string ExtractVideoId(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("L'URL ne peut pas être vide.", nameof(url));
        }

        try
        {
            var uri = new Uri(url);

            // Format: https://www.youtube.com/watch?v=VIDEO_ID ou https://youtube.com/watch?v=VIDEO_ID
            if ((uri.Host.Contains("youtube.com") || uri.Host.Contains("www.youtube.com")) && uri.AbsolutePath == "/watch")
            {
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var videoId = query["v"];

                if (!string.IsNullOrWhiteSpace(videoId))
                {
                    return videoId;
                }
            }

            // Format: https://youtu.be/VIDEO_ID
            if (uri.Host == "youtu.be" && uri.AbsolutePath.Length > 1)
            {
                return uri.AbsolutePath.TrimStart('/');
            }

            // Format: https://www.youtube.com/embed/VIDEO_ID
            if ((uri.Host.Contains("youtube.com") || uri.Host.Contains("www.youtube.com")) && uri.AbsolutePath.StartsWith("/embed/"))
            {
                var videoId = uri.AbsolutePath.Replace("/embed/", "");
                if (!string.IsNullOrWhiteSpace(videoId))
                {
                    return videoId;
                }
            }

            throw new ArgumentException($"Impossible d'extraire l'identifiant de la vidéo depuis l'URL : {url}", nameof(url));
        }
        catch (UriFormatException)
        {
            throw new ArgumentException($"L'URL fournie n'est pas valide : {url}", nameof(url));
        }
    }

    /// <summary>
    /// Récupère le titre d'une vidéo YouTube via l'API oEmbed (gratuite, sans clé).
    /// </summary>
    /// <param name="videoId">L'identifiant de la vidéo YouTube.</param>
    /// <param name="cancellationToken">Token d'annulation pour l'opération asynchrone.</param>
    /// <returns>Le titre de la vidéo ou null si non disponible.</returns>
    private async Task<string?> GetVideoTitle(string videoId, CancellationToken cancellationToken)
    {
        try
        {
            var oembedUrl = $"https://www.youtube.com/oembed?url=https://www.youtube.com/watch?v={videoId}&format=json";
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(oembedUrl, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var oembedResponse = JsonSerializer.Deserialize<YoutubeOEmbedResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return oembedResponse?.Title;
            }
        }
        catch
        {
            // Si l'appel échoue, on retournera null et utilisera un fallback
        }

        return null;
    }

    /// <summary>
    /// Récupère le transcript d'une vidéo YouTube via l'API Supadata.
    /// </summary>
    /// <param name="videoId">L'identifiant de la vidéo YouTube.</param>
    /// <param name="cancellationToken">Token d'annulation pour l'opération asynchrone.</param>
    /// <returns>La réponse de l'API Supadata contenant le transcript et les métadonnées.</returns>
    /// <exception cref="HttpRequestException">Lancée si l'appel à l'API échoue.</exception>
    /// <exception cref="JsonException">Lancée si la désérialisation de la réponse échoue.</exception>
    private async Task<SupadataTranscriptResponse> GetTranscriptFromSupadata(string videoId, CancellationToken cancellationToken)
    {
        // Forcer l'anglais en priorité pour plus de fiabilité dans le traitement AI
        // Si l'anglais n'est pas disponible, l'API retournera la première langue disponible
        var requestUrl = $"/v1/youtube/transcript?videoId={videoId}&text=true&lang=en";

        var response = await _httpClient.GetAsync(requestUrl, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"L'API Supadata a retourné une erreur {response.StatusCode}: {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var transcriptResponse = JsonSerializer.Deserialize<SupadataTranscriptResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (transcriptResponse == null || string.IsNullOrWhiteSpace(transcriptResponse.Content))
        {
            throw new InvalidOperationException("La réponse de l'API Supadata est vide ou invalide.");
        }

        return transcriptResponse;
    }
}

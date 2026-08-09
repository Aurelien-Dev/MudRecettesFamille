using RecettesFamille.Services.Models;
using System.Text.Json;

namespace RecettesFamille.ServicesExternal;

/// <summary>
/// Service centralisant tous les appels à l'API Supadata.
/// </summary>
public class SupadataService : ISupadataService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initialise une nouvelle instance du service Supadata.
    /// </summary>
    /// <param name="httpClientFactory">Factory pour créer des instances HttpClient configurées.</param>
    public SupadataService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Supadata");
    }

    /// <inheritdoc/>
    public async Task<SupadataTranscriptResponse> GetYoutubeTranscriptAsync(string videoId, CancellationToken cancellationToken = default)
    {
        // Forcer l'anglais en priorité pour plus de fiabilité dans le traitement AI.
        // Si l'anglais n'est pas disponible, l'API retournera la première langue disponible.
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

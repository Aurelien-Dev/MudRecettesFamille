using Microsoft.AspNetCore.Mvc;
using RecettesFamille.Managers.AiGenerators;
using System.Text.Json;

namespace RecettesFamille.Api;

/// <summary>
/// Extension methods pour enregistrer les endpoints utilitaires
/// </summary>
public static class UtilityEndpoints
{
    /// <summary>
    /// Enregistre tous les endpoints utilitaires
    /// </summary>
    public static IEndpointRouteBuilder MapUtilityEndpoints(this IEndpointRouteBuilder app)
    {
        // Endpoint pour obtenir un résumé YouTube
        app.MapPost("/api/youtube-summary", HandleYoutubeSummary).WithName("YoutubeSummary");
        return app;
    }

    /// <summary>
    /// Gère la demande de résumé YouTube
    /// </summary>
    private static async Task<IResult> HandleYoutubeSummary(HttpRequest request, [FromServices] AiManager aiManager, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();
        var requestBody = JsonSerializer.Deserialize<YoutubeSummaryJson>(body);
        if (requestBody is null)
        {
            return Results.BadRequest("Invalid request body.");
        }

        var resume = await aiManager.GetYoutubeResume(requestBody, null, cancellationToken);
        return Results.Ok(new { result = resume });
    }
}

/// <summary>
/// Modèle de requête pour le résumé YouTube
/// </summary>
public class YoutubeSummaryJson
{
    public required string Transcript { get; set; }
    public required string Url { get; set; }
    public required string Title { get; set; }
}

using ZXing;

namespace RecettesFamille.MinimalApi;

public static class ApiRoutes
{
    public static IEndpointConventionBuilder MapPageRoute(this IEndpointRouteBuilder endpoints)
    {
        var apiGroup = endpoints.MapGroup("/api");

        apiGroup.MapGet("DownloadBackup", async () =>
        {
            var filePath = Path.Combine("/app/wwwroot/", "backup_2025-01-18.sql");

            if (!System.IO.File.Exists(filePath))
            {
                return Results.NotFound("Fichier introuvable.");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var contentType = "application/octet-stream";

            return Results.File(fileBytes, contentType, "fileName");
        })
        .WithName("DownloadBackup");
        //.RequireAuthorization();

        return apiGroup;
    }
}
namespace RecettesFamille.Dto.Models;

public class YoutubeResumeDto
{
    public int Id { get; set; }
    public required string Resume { get; set; }
    public required string Url { get; set; }
    public required string Title { get; set; }
}

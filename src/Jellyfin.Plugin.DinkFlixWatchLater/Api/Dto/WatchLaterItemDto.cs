namespace Jellyfin.Plugin.DinkFlixWatchLater.Api.Dto;
public sealed class WatchLaterItemDto
{
    public int TmdbId { get; set; }
    public string MediaType { get; set; } = "movie";
    public string Title { get; set; } = "";
    public string? PosterPath { get; set; }
    public string? BackdropPath { get; set; }
    public string? OriginalLanguage { get; set; }
    public List<string> Genres { get; set; } = [];
    public DateTime AddedAtUtc { get; set; }
}

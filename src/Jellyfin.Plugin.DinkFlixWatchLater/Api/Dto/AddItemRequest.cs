namespace Jellyfin.Plugin.DinkFlixWatchLater.Api.Dto;
public sealed class AddItemRequest
{
    public int TmdbId { get; set; }
    public string MediaType { get; set; } = "movie";
    public string Title { get; set; } = "";
    public string? PosterPath { get; set; }
    public string? BackdropPath { get; set; }
    public string? OriginalLanguage { get; set; }
}

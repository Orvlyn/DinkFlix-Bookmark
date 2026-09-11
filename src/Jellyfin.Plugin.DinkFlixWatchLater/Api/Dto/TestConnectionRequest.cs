namespace Jellyfin.Plugin.DinkFlixWatchLater.Api.Dto;

public sealed class TestConnectionRequest
{
    public string SeerrUrl { get; set; } = string.Empty;
    public string SeerrApiKey { get; set; } = string.Empty;
}

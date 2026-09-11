namespace Jellyfin.Plugin.DinkFlixWatchLater.Api.Dto;
public sealed class AvailabilityDto
{
    public bool AvailableOnJellyfin { get; set; }
    public string? JellyfinItemId { get; set; }
}

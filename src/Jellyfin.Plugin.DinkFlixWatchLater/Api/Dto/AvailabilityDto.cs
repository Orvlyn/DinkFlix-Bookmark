namespace Jellyfin.Plugin.DinkFlixWatchLater.Api.Dto
{
    public class AvailabilityDto
    {
        public bool AvailableOnJellyfin { get; set; }

        public string? JellyfinItemId { get; set; }

        /// <summary>"unknown" | "pending" | "processing" | "partially_available" | "available".</summary>
        public string SeerrStatus { get; set; } = "unknown";

        public bool CanRequest { get; set; } = true;
    }
}

namespace Jellyfin.Plugin.DinkFlixWatchLater.Api.Dto
{
    public class RequestItemRequest
    {
        /// <summary>"movie" or "tv".</summary>
        public string MediaType { get; set; } = "movie";
    }
}

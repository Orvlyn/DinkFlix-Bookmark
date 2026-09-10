namespace Jellyfin.Plugin.DinkFlixWatchLater.Api.Dto
{
    public class AddItemRequest
    {
        public int TmdbId { get; set; }

        /// <summary>"movie" or "tv".</summary>
        public string MediaType { get; set; } = "movie";
    }
}

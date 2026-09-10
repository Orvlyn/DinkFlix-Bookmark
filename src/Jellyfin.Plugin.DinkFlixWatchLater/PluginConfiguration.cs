using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.DinkFlixWatchLater
{
    /// <summary>
    /// Connection settings for the Seerr (Jellyseerr/Overseerr) instance backing Watch Later.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        public string SeerrUrl { get; set; } = string.Empty;

        public string SeerrApiKey { get; set; } = string.Empty;
    }
}

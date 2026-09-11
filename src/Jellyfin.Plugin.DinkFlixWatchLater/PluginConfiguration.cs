using MediaBrowser.Model.Plugins;
namespace Jellyfin.Plugin.DinkFlixWatchLater;
public sealed class PluginConfiguration : BasePluginConfiguration
{
    public bool EnableHomeIntegration { get; set; } = true;
    public bool EnableSeerrButtons { get; set; } = true;
}

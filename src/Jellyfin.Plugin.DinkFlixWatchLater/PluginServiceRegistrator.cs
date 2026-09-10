using Jellyfin.Plugin.DinkFlixWatchLater.Services;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.DinkFlixWatchLater
{
    /// <summary>
    /// Registers plugin services with Jellyfin's dependency injection container.
    /// </summary>
    public class PluginServiceRegistrator : IPluginServiceRegistrator
    {
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            serviceCollection.AddHttpClient();
            serviceCollection.AddSingleton<WatchLaterStore>();
            serviceCollection.AddSingleton<SeerrClient>();
        }
    }
}

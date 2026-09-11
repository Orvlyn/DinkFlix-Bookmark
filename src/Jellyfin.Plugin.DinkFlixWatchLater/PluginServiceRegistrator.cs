using Jellyfin.Plugin.DinkFlixWatchLater.Services;using MediaBrowser.Controller;using MediaBrowser.Controller.Plugins;using Microsoft.Extensions.DependencyInjection;
namespace Jellyfin.Plugin.DinkFlixWatchLater;
public class PluginServiceRegistrator:IPluginServiceRegistrator{public void RegisterServices(IServiceCollection s,IServerApplicationHost h){s.AddHttpClient();s.AddSingleton<WatchLaterStore>();s.AddSingleton<SeerrClient>();}}

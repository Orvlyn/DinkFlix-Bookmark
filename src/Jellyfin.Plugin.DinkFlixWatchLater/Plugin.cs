using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.DinkFlixWatchLater;

public sealed class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
        TryRegisterPluginPage();
    }

    public static Plugin? Instance { get; private set; }
    public override string Name => "DinkFlix Watch Later";
    public override Guid Id => Guid.Parse("6f2c9e2a-2d0a-4a7c-9c9d-8b1f6a9d1234");
    public override string Description => "A personal Watch Later list for Jellyfin and Seerr.";

    public IEnumerable<PluginPageInfo> GetPages()
    {
        yield return new PluginPageInfo
        {
            Name = "dinkflixwatchlater-config",
            EmbeddedResourcePath = $"{GetType().Namespace}.Configuration.configPage.html"
        };
    }

    public void RegisterUserPage()
    {
        TryRegisterPluginPage();
    }

    private void TryRegisterPluginPage()
    {
        try
        {
            var assembly = AssemblyLoadContext.All.SelectMany(x => x.Assemblies)
                .FirstOrDefault(x => x.GetType("Jellyfin.Plugin.PluginPages.PluginInterface") != null);
            var interfaceType = assembly?.GetType("Jellyfin.Plugin.PluginPages.PluginInterface");
            var register = interfaceType?.GetMethod("RegisterPage", BindingFlags.Public | BindingFlags.Static);
            var jobjectType = Type.GetType("Newtonsoft.Json.Linq.JObject, Newtonsoft.Json");
            var parse = jobjectType?.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
            if (register is null || parse is null) return;

            // Plugin Pages 3.0 is the Jellyfin 12 integration point. The page payload is
            // deliberately built as JSON so DinkFlix does not load Plugin Pages into its
            // own AssemblyLoadContext. Unknown optional fields are ignored by Plugin Pages.
            var payload = new
            {
                id = "DinkFlixWatchLater",
                displayName = "Watch Later",
                name = "Watch Later",
                route = "dinkflix-watch-later",
                url = "/DinkFlixWatchLater/web/page",
                pageUrl = "/DinkFlixWatchLater/web/page",
                icon = "bookmark",
                enableInMainMenu = true
            };
            var json = JsonSerializer.Serialize(payload);
            var jObject = parse.Invoke(null, new object?[] { json });
            register.Invoke(null, new[] { jObject });
        }
        catch
        {
            // Optional integration. The core plugin remains usable if Plugin Pages is unavailable.
        }
    }

    private void TryRemovePluginPage()
    {
        try
        {
            var assembly = AssemblyLoadContext.All.SelectMany(x => x.Assemblies)
                .FirstOrDefault(x => x.GetType("Jellyfin.Plugin.PluginPages.PluginInterface") != null);
            var interfaceType = assembly?.GetType("Jellyfin.Plugin.PluginPages.PluginInterface");
            interfaceType?.GetMethod("RemovePage", BindingFlags.Public | BindingFlags.Static)
                ?.Invoke(null, new object?[] { "DinkFlixWatchLater" });
        }
        catch { }
    }
}

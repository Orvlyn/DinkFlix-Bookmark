using System;
using System.Collections.Generic;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.DinkFlixWatchLater
{
    /// <summary>
    /// Plugin entry point. Registers configuration and the admin settings page.
    /// </summary>
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public static Plugin? Instance { get; private set; }

        public override string Name => "DinkFlix Watch Later";

        public override Guid Id => Guid.Parse("6f2c9e2a-2d0a-4a7c-9c9d-8b1f6a9d1234");

        public override string Description =>
            "Save titles from Seerr as a wishlist without requesting them. Play if you already have it, request if you don't.";

        public IEnumerable<PluginPageInfo> GetPages()
        {
            yield return new PluginPageInfo
            {
                Name = "dinkflixwatchlater",
                EmbeddedResourcePath = string.Format("{0}.Configuration.configPage.html", GetType().Namespace)
            };
        }
    }
}

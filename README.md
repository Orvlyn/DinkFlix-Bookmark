# DinkFlix Watch Later

> **A clean Watch Later / wishlist layer for Jellyfin, powered by Seerr.**

Save movies and TV shows without requesting them. Come back later to play what is already in your Jellyfin library, or send an unavailable title to Seerr when you are ready.

[![Jellyfin 12](https://img.shields.io/badge/Jellyfin-12.0.0-00ffc6?style=flat-square&labelColor=0b0f15)](https://jellyfin.org/)
[![Seerr](https://img.shields.io/badge/Seerr-supported-00ffc6?style=flat-square&labelColor=0b0f15)](https://seerr.dev/)
[![Target](https://img.shields.io/badge/.NET-10-00ffc6?style=flat-square&labelColor=0b0f15)](https://dotnet.microsoft.com/)

---

## What it does

**DinkFlix Watch Later** gives each Jellyfin user a private list of titles they want to come back to.

- 🔖 Save a title without requesting it
- 🔎 Search Seerr directly from Watch Later
- 🎬 Separate Movies, TV Shows and Anime views
- ✨ Generate recommendations from your saved titles
- ▶️ Detect titles already available in Jellyfin and open them for playback
- 📥 Request unavailable titles through Seerr when you are ready
- 👤 Keep lists separate for each Jellyfin user
- 🔐 Keep the Seerr API key server-side
- 🖥️ Works as a standalone Jellyfin Web page
- 🎨 Uses a clean DinkFlix dark interface with `#00ffc6` accents

The plugin is intentionally a **Watch Later system**, not a second request manager. Seerr remains responsible for requests and media management.

---

## Requirements

| Requirement | Version / note |
|---|---|
| Jellyfin Server | **12.0.0** |
| Jellyfin Web | Included with Jellyfin |
| Seerr | Current supported release |
| Seerr API key | Required |
| .NET | 10, for building from source only |

Seerr exposes API-key authentication through the `X-Api-Key` header. urlSeerr API documentationhttps://docs.seerr.dev/api/seerr-api/

---

## Installation

### Option A · Install the release ZIP

1. Open the repository's **Releases** page.
2. Download the latest `DinkFlixWatchLater_*.zip`.
3. In Jellyfin, open **Dashboard → Plugins → Repositories** and install the plugin from the release package using your normal Jellyfin plugin installation workflow.
4. Restart Jellyfin if it asks you to.
5. Open **Dashboard → Plugins → Installed** and select **DinkFlix Watch Later**.

> The release ZIP contains the compiled plugin DLL at the ZIP root, which is the format produced by the included build workflow.

### Option B · Build it yourself

```bash
dotnet build src/Jellyfin.Plugin.DinkFlixWatchLater/Jellyfin.Plugin.DinkFlixWatchLater.csproj -c Release -o publish
```

The compiled DLL will be in `publish/`.

---

## Configuration

Open:

**Jellyfin Dashboard → Plugins → DinkFlix Watch Later**

Enter:

### Seerr URL

Your Seerr base address, for example:

```text
http://192.168.1.10:5055
```

Do not add `/api/v1` to the URL. DinkFlix adds the API path itself.

### Seerr API key

Find it in:

**Seerr → Settings → General → API Key**

Seerr specifically warns that this key can provide administrator-level access, so treat it like a password and do not publish it. urlSeerr General Settingshttps://docs.seerr.dev/using-seerr/settings/general/

### Test connection

The **Test connection** button tests the URL and API key currently entered in the form. It does **not** depend on the settings having already been saved.

A successful test means DinkFlix can reach Seerr and Seerr accepted the supplied API key.

---

## Using Watch Later

Open the Watch Later page from the plugin configuration screen, or add it to Jellyfin's custom menu.

Recommended direct path:

```text
/DinkFlixWatchLater/web/page
```

### Search

Search for a movie or TV show. Results are retrieved from Seerr and can be saved directly to Watch Later.

### Save

Saving a title only adds it to your personal DinkFlix list.

**It does not request the title.**

### Play

When a saved title is already in Jellyfin, DinkFlix detects the matching TMDB ID and gives you a **Play** action.

### Request

If the title is not available and Seerr does not report an existing request, DinkFlix provides a **Request** action.

The request is submitted through Seerr's request API.

### Recommendations

The Recommendations tab uses Seerr recommendations for your most recently saved titles, removes titles already in your Watch Later list, and presents the remaining results as additional things to save.

---

## Custom Jellyfin menu link

If you want Watch Later permanently available in Jellyfin's main navigation, add a custom menu entry to Jellyfin Web's configuration.

Example:

```json
{
  "menuLinks": [
    {
      "name": "Watch Later",
      "icon": "bookmark",
      "url": "/DinkFlixWatchLater/web/page"
    }
  ]
}
```

The exact location and configuration format for custom menu links depends on the Jellyfin Web setup you are using.

---

## How the plugin works

```text
                     ┌──────────────────────┐
                     │   Jellyfin Web       │
                     │   Watch Later page   │
                     └──────────┬───────────┘
                                │
                                ▼
                     ┌──────────────────────┐
                     │ DinkFlix Watch Later │
                     │      plugin API      │
                     └───────┬───────┬──────┘
                             │       │
                    saved list│       │Seerr API
                             │       │
                             ▼       ▼
                    ┌────────────┐ ┌────────────┐
                    │  Jellyfin  │ │   Seerr    │
                    │  library   │ │ discovery  │
                    │  matching  │ │ & requests │
                    └────────────┘ └────────────┘
```

Saved items are stored per Jellyfin user. Seerr is used for metadata, search, recommendations and requests. Jellyfin is used to determine whether a saved title is already available for playback.

---

## API endpoints

All endpoints are under:

```text
/DinkFlixWatchLater
```

| Method | Endpoint | Purpose |
|---|---|---|
| `GET` | `/items` | Get the current user's saved titles |
| `POST` | `/items` | Save a title |
| `DELETE` | `/items/{tmdbId}` | Remove a title |
| `GET` | `/search?query=` | Search Seerr |
| `GET` | `/recommendations` | Get recommendations from saved titles |
| `GET` | `/availability/{tmdbId}` | Check Jellyfin / Seerr availability |
| `POST` | `/request/{tmdbId}` | Request a title through Seerr |
| `POST` | `/test-connection` | Test supplied Seerr credentials |
| `GET` | `/test-connection` | Test saved Seerr configuration |
| `GET` | `/web/page` | Watch Later UI |

---

## Security

- Jellyfin authentication is required for user data and normal plugin API operations.
- Saved lists are keyed by Jellyfin user ID.
- The Seerr API key is stored in Jellyfin plugin configuration and is never placed in the Watch Later browser JavaScript.
- The browser sends its Jellyfin session token to the plugin API for authenticated operations.
- The standalone Watch Later page does not expose the Seerr API key to the client.

**Do not commit your Seerr API key to this repository.**

---

## Project structure

```text
DinkFlix-Bookmark/
├── .github/
│   └── workflows/
│       └── build.yml
├── manifest.json
├── README.md
└── src/
    └── Jellyfin.Plugin.DinkFlixWatchLater/
        ├── Api/
        │   ├── Dto/
        │   └── WatchLaterController.cs
        ├── Configuration/
        │   └── configPage.html
        ├── Services/
        │   ├── SeerrClient.cs
        │   └── WatchLaterStore.cs
        ├── Web/
        │   ├── watchlater.html
        │   ├── watchlater.css
        │   └── watchlater.js
        ├── Plugin.cs
        ├── PluginConfiguration.cs
        ├── PluginServiceRegistrator.cs
        └── Jellyfin.Plugin.DinkFlixWatchLater.csproj
```

---

## Build & release workflow

Every push to `main` runs the GitHub Actions workflow.

The workflow:

1. Checks out the complete repository history.
2. Installs .NET 10.
3. Builds the plugin in Release mode.
4. Verifies the compiled DLL exists.
5. Packages the DLL into a Jellyfin-compatible ZIP.
6. Verifies the ZIP contents.
7. Publishes a GitHub release.
8. Updates the repository manifest with the release URL and checksum.
9. Rebases and retries the manifest push if another commit reaches `main` during the workflow.

The workflow uses the current `actions/checkout@v5` and `actions/setup-dotnet@v5` actions.

---

## Troubleshooting

### Test connection fails

Check all three of these:

1. Jellyfin can reach the Seerr host and port.
2. The URL points to the Seerr base URL, such as `http://192.168.1.10:5055`.
3. The API key is the current key shown in **Seerr → Settings → General**.

Seerr's own troubleshooting documentation recommends checking network/DNS connectivity when the server cannot reach external services. urlSeerr troubleshootinghttps://docs.seerr.dev/troubleshooting/

### Search works but requests fail

Seerr must have working Radarr/Sonarr configuration and at least one default server configured for requests. urlSeerr service configurationhttps://docs.seerr.dev/using-seerr/settings/services/

### A title says it is unavailable even though Jellyfin has it

DinkFlix matches saved movies and TV shows using the Jellyfin TMDB provider ID. Make sure the Jellyfin item has TMDB metadata assigned.

### Watch Later page is blank

Open the page while signed into Jellyfin Web and make sure the browser has an active Jellyfin session. The page uses that session to authenticate its API requests.

---

## Development notes

The plugin targets Jellyfin **12.0.0** and .NET **10**.

The Jellyfin plugin template demonstrates the standard configuration-page save flow using `ApiClient.getPluginConfiguration`, `ApiClient.updatePluginConfiguration`, and `Dashboard.processPluginConfigurationUpdateResult`. DinkFlix follows that pattern for configuration persistence. urlJellyfin plugin template configuration pagehttps://raw.githubusercontent.com/jellyfin/jellyfin-plugin-template/master/Jellyfin.Plugin.Template/Configuration/configPage.html

Seerr's current API documentation defines the search endpoint as `GET /search`, the status endpoint as `GET /status`, and API-key authentication through `X-Api-Key`. citeturn3search0turn7view0turn0search0

---

## License

This project is provided as-is for personal Jellyfin / DinkFlix use.

# DinkFlix Watch Later

A Jellyfin plugin for **Jellyfin + Seerr** that lets users save movies and TV shows for later without immediately requesting them.

## Features

- Per-user Watch Later storage
- Search Seerr from the Watch Later page
- Save movies and TV shows without triggering Radarr/Sonarr
- Automatic Anime section
- Live Jellyfin availability checks using TMDB IDs
- Play titles already in Jellyfin
- Request titles through Seerr when ready
- Remove saved titles
- Seerr-powered recommendations based on saved titles
- Server-side Seerr API key
- Jellyfin-authenticated API
- No external hosting or database required

## Why Watch Later?

Seerr's request queue answers **"I want this."**

Watch Later answers **"I might want this."**

Saving a title does not request it. The user decides later whether to play it or send it to Seerr.

## Installation

Upload this repository to GitHub and push to `main`. The included GitHub Actions workflow builds a Jellyfin plugin ZIP, creates a release, and updates `manifest.json`.

Then add your repository's raw `manifest.json` to:

`Dashboard → Plugins → Repositories`

Install **DinkFlix Watch Later** and restart Jellyfin.

## Configuration

Open:

`Dashboard → Plugins → DinkFlix Watch Later`

Set:

- **Seerr URL**, e.g. `http://192.168.1.10:5055`
- **Seerr API Key**, from `Seerr → Settings → General → API Key`

Use **Test connection** before leaving the page.

## Watch Later page

Open:

`/DinkFlixWatchLater/web/page`

Example:

`http://your-server:8096/DinkFlixWatchLater/web/page`

The page reads the current Jellyfin Web session and never receives the Seerr API key.

## Jellyfin menu

For a permanent **Watch Later** menu entry, use Jellyfin Web's supported custom menu links and point it to:

`/DinkFlixWatchLater/web/page`

Suggested label: **Watch Later**

Suggested icon: **bookmark**

This is preferable to editing `jellyfin-web/index.html`, because Jellyfin web updates will not overwrite the menu configuration.

## Recommendations

The Recommendations tab uses the user's most recently saved Watch Later titles as recommendation seeds and calls Seerr's movie/TV recommendation endpoints. Saved titles are removed from the recommendation results and duplicates are collapsed.

## Data

Each Jellyfin user has a local JSON file under:

`plugins/DinkFlixWatchLater/data/`

The plugin never trusts a user ID supplied by the browser. The user ID comes from the authenticated Jellyfin request.

## Project layout

```text
.github/workflows/build.yml
manifest.json
README.md

src/Jellyfin.Plugin.DinkFlixWatchLater/
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

## Requirements

- Jellyfin 12
- .NET 10
- Seerr / Jellyseerr / compatible Seerr API

## Build

```bash
dotnet build src/Jellyfin.Plugin.DinkFlixWatchLater/Jellyfin.Plugin.DinkFlixWatchLater.csproj -c Release
```

The GitHub workflow packages the DLL at the root of the release ZIP.

## API

The plugin uses Seerr server-side for:

- Search
- Movie/TV metadata
- Recommendations
- Requests
- Availability status

Seerr's API supports `X-Api-Key` authentication.

## Credits

Built by **Orvlyn** for the DinkFlix / Jellyfin stack.

- Jellyfin: https://github.com/jellyfin/jellyfin
- Seerr: https://github.com/seerr-team/seerr

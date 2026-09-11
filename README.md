# DinkFlix Watch Later

> Save it now. Request it when you are ready.

DinkFlix Watch Later is a lightweight Jellyfin 12 plugin for keeping a personal list of movies and TV shows you want to watch later. It is designed around **Jellyfin Enhanced 12.6.x** and **Seerr**, so it does not duplicate Seerr credentials or build a second request system.

## What it does

- Saves movies and TV shows per Jellyfin user.
- Stores titles without requesting or downloading them.
- Uses TMDB IDs so saved titles remain tied to the Seerr/Jellyfin ecosystem.
- Can save directly from a Seerr URL when browsing Seerr outside Jellyfin.
- Detects when a saved title is already in the Jellyfin library.
- Opens available titles directly in Jellyfin.
- Opens unavailable titles in Seerr so the existing Jellyfin Enhanced request flow remains responsible for permissions, profiles and request handling.
- Provides a dedicated Watch Later page through Plugin Pages 3.0.0.0 on Jellyfin 12.
- Uses safe atomic JSON writes for the per-user list.

## Designed for your stack

| Component | Required | Purpose |
| --- | --- | --- |
| Jellyfin 12.0.0 | Yes | Server/runtime |
| Jellyfin Enhanced 12.6.0.0+ | Recommended | Seerr search, discovery and requests |
| Plugin Pages 3.0.0.0 | Yes for the Watch Later page | Jellyfin 12 user-facing page registration |
| File Transformation 3.0.0.0 | Keep installed | Required by Plugin Pages/Jellyfin Enhanced in this setup |
| Seerr | Recommended | Discovery and requesting |

Jellyfin Enhanced 12.6.0.0 officially targets stable Jellyfin 12 and recommends using native Jellyfin tabs where available. Its Seerr integration provides search, requests, discovery, recommendations and watchlist syncing.

Plugin Pages 3.0.0.0 added the Jellyfin 12-compatible `PluginInterface.RegisterPage(JObject)` API specifically for dependent plugins to register pages programmatically.

## How it is intended to work

### 1. Find something

Use Seerr directly or use Jellyfin Enhanced's Seerr search/discovery integration inside Jellyfin.

### 2. Save it

Use the DinkFlix **Watch Later** action where the integration is available. Saving an item does **not** request it. If a client does not expose the action yet, paste the Seerr movie/show URL into the Watch Later page. This is a fallback and does not require a second Seerr API key.

### 3. Open Watch Later

The Watch Later page shows your saved titles only. Each title is checked against the Jellyfin library.

### 4. Decide later

- **Available:** Play
- **Unavailable:** Open in Seerr and use Jellyfin Enhanced's normal Request flow
- **No longer interested:** Remove

This separation is deliberate. DinkFlix owns the saved list; Jellyfin Enhanced owns Seerr authentication, request permissions and request configuration.

## Installation

1. Install Jellyfin 12.0.0.
2. Install File Transformation 3.0.0.0.
3. Install Plugin Pages 3.0.0.0.
4. Install Jellyfin Enhanced 12.6.0.0 or newer.
5. Install DinkFlix Watch Later.
6. Restart Jellyfin.
7. Open the Jellyfin user menu and look for **Watch Later**.

If the page does not appear after installation, fully reload the Jellyfin Web client. Plugin Pages 3.0.0.0 specifically changed its Jellyfin 12 injection and routing implementation.

## Configuration

There is intentionally no Seerr URL or Seerr API key field in DinkFlix Watch Later. Your Seerr configuration belongs to Jellyfin Enhanced.

This avoids maintaining a second set of credentials and avoids the common situation where a plugin reaches Seerr but receives HTTP 401 because it is not using the same user-aware integration as Jellyfin Enhanced.

## Data storage

Watch Later data is stored per Jellyfin user under the plugin data directory:

`plugins/DinkFlixWatchLater/data/<jellyfin-user-id>.json`

Only the server stores the saved list. No Seerr API key is stored by DinkFlix.

## API

Authenticated endpoints:

- `GET /DinkFlixWatchLater/items`
- `POST /DinkFlixWatchLater/items`
- `DELETE /DinkFlixWatchLater/items/{tmdbId}?mediaType=movie|tv`
- `GET /DinkFlixWatchLater/availability/{tmdbId}?mediaType=movie|tv`

The page assets are served from:

- `/DinkFlixWatchLater/web/page`
- `/DinkFlixWatchLater/web/watchlater.css`
- `/DinkFlixWatchLater/web/watchlater.js`

The page route is registered through Plugin Pages rather than being exposed as a manually constructed Jellyfin Web route.

## Important limitation

DinkFlix intentionally does not impersonate Jellyfin Enhanced's Seerr request API. The current Jellyfin Enhanced release has its own user-aware Seerr proxy, permissions and request flow. DinkFlix therefore opens Seerr for the final request action instead of attempting to duplicate those internals.

The dedicated Watch Later page and library availability logic work independently of that request hand-off.

## Build

```bash
dotnet build src/Jellyfin.Plugin.DinkFlixWatchLater/Jellyfin.Plugin.DinkFlixWatchLater.csproj -c Release
```

The GitHub Actions workflow builds against .NET 10, verifies the resulting assembly and ZIP, publishes a release, and updates the Jellyfin plugin manifest.

## Project layout

```text
DinkFlix-Bookmark/
├── .github/workflows/build.yml
├── manifest.json
├── README.md
└── src/Jellyfin.Plugin.DinkFlixWatchLater/
    ├── Api/
    ├── Configuration/
    ├── Services/
    ├── Web/
    ├── Plugin.cs
    ├── PluginConfiguration.cs
    └── PluginServiceRegistrator.cs
```

## License

This project is distributed under the license used by the repository.

# DinkFlix Watch Later

A Jellyfin plugin that adds the wishlist/bookmark feature Jellyseerr/Overseerr
doesn't have: save a title's metadata and poster **without** triggering a
Radarr/Sonarr request, then decide later whether to request it or just hit
Play if it's already in your library.

```
DinkFlix
│
├── Jellyfin
├── Seerr
└── DinkFlix Watch Later   <- this plugin
      ├── Movies
      ├── TV
      └── Anime
```

## What it does

- Adds a **DinkFlix Watch Later** page (server-rendered by the plugin, no
  external hosting needed) with Movies / TV / Anime tabs.
- "Add to Watch Later" saves the TMDB id, title, poster/backdrop, genres and
  timestamp — nothing is sent to Radarr/Sonarr and no download is triggered.
- Each saved card checks live status and shows:
  - **Play** — if the title is already in your Jellyfin library (matched by
    TMDB provider id), links straight to it.
  - **Request** — if not requested yet, submits the request to Seerr on click.
  - Current Seerr status (pending / processing / partially available) if a
    request already exists.
  - **Remove** — deletes it from the wishlist.
- Anime is auto-detected (original language `ja` + genre `Animation`) so it
  doesn't clutter the TV tab.
- Everything (Seerr URL + API key) is configured from the normal Jellyfin
  admin Dashboard → Plugins page — no manual config files.

## Why a plugin instead of modifying Seerr

Seerr's request queue and this wishlist are different concepts, and Seerr
itself has an open feature request for exactly this that hasn't landed. Living
as its own plugin means:
- Nothing about Seerr's codebase is touched, so Seerr updates can't break it.
- It talks to Seerr only through its public REST API (`X-Api-Key` auth).
- It's local-only, same as the rest of your stack.

## Project layout

```
.github/workflows/build.yml                  GitHub builds the plugin for you, no local install needed
src/Jellyfin.Plugin.DinkFlixWatchLater/
    Plugin.cs                                plugin entry point, registers the admin config page
    PluginConfiguration.cs                   SeerrUrl / SeerrApiKey settings
    PluginServiceRegistrator.cs              DI registration
    Configuration/configPage.html            admin Dashboard settings page
    Api/WatchLaterController.cs              REST endpoints + serves the Watch Later web page
    Api/Dto/                                 request/response models
    Services/WatchLaterStore.cs              JSON-file-per-user storage (in plugin data dir)
    Services/SeerrClient.cs                  talks to Seerr's REST API
    Web/watchlater.html|.css|.js             the Watch Later page itself (DinkFlix-styled)
```

## Getting a working plugin — no coding required on your end

You do **not** need to install .NET, run any commands, or know how to code.
GitHub builds the plugin for you, creates a release, and Jellyfin installs it
straight from your repo like any other plugin catalog entry.

1. **Create a GitHub repository** (e.g. `DinkFlix-WatchLater`) — Public, don't
   add a README (you already have one).
2. **Upload the files** — **Add file → Upload files**, drag in everything from
   `F:\Jellyfin\DinkFlix Bookmark` (keep the folder structure — `.github`,
   `src`, `README.md`, `manifest.json` etc. all at the top level). Commit.
   - If `.github/workflows/build.yml` doesn't end up in the repo (browsers
     sometimes drop dot-folders on drag/drop), create it manually: **Add file
     → Create new file**, type `.github/workflows/build.yml` as the filename,
     and paste in the contents of that file from your local copy.
3. **Wait for the build** — click the **Actions** tab, watch the "Build and
   Publish Plugin" run go green (a couple of minutes). It automatically:
   - builds the plugin
   - creates a GitHub **Release** with the compiled zip attached
   - updates `manifest.json` in the repo with that release's download link
     and checksum
   - If it goes red instead, open the run, open the failing step, and paste
     the error back to me.
4. **Add the repository in Jellyfin** — Dashboard → Plugins → **Repositories**
   → **Add Repository**:
   - Name: `DinkFlix`
   - URL:
     ```
     https://raw.githubusercontent.com/<your-username>/<your-repo-name>/main/manifest.json
     ```
5. Go to the **Catalog** tab — "DinkFlix Watch Later" should show up. Click it,
   **Install**, then restart Jellyfin when prompted.
6. **Dashboard → Plugins → My Plugins → DinkFlix Watch Later** → enter your
   Seerr URL and API key.

Every time you push new changes to `main`, the workflow builds a new version
and updates the manifest automatically — Jellyfin will just show an update
available for the plugin, same as any other.

## Setup after installing

1. **Dashboard → Plugins → DinkFlix Watch Later** → enter your Seerr URL
   (e.g. `http://192.168.1.10:5055`) and API key (Seerr → Settings → General).
2. Open the Watch Later page directly at:
   ```
   http://<your-server>:8096/DinkFlixWatchLater/web/page
   ```

## Adding a "Watch Later" nav link (optional)

Jellyfin's stock web client only accepts **Custom CSS**, not custom JS, from
the Dashboard — so a real nav button (not just a decorative image, like the
DinkFlix logo trick in `dinkflix.css`) needs a tiny script injected into
`index.html`. Two common ways to do that on a local server:

**Option A — reverse proxy (nginx), if you already proxy Jellyfin:**
```nginx
sub_filter '</body>'
  '<script>
     var a=document.createElement("a");
     a.href="/DinkFlixWatchLater/web/page";
     a.textContent="Watch Later";
     a.style.cssText="position:fixed;top:1em;right:9em;z-index:1000;color:#00ffc6;font-weight:600;text-decoration:none;";
     document.body.appendChild(a);
   </script></body>';
sub_filter_once on;
```

**Option B — edit the web client directly:**
Add the same `<script>` block right before `</body>` in Jellyfin's
`jellyfin-web/index.html` on the server (re-apply after every Jellyfin
update, since it ships a fresh `index.html`).

## Credits

Companion project for the [DinkFlix](https://github.com/Orvlyn/DinkFlix) theme.

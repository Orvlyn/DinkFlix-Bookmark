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

## Getting a working plugin — no coding or install required on your end

You do **not** need to install .NET, run any commands, or know how to code.
GitHub will compile the plugin for you for free every time you upload changes.

1. **Create a GitHub repository** (e.g. `DinkFlix-WatchLater`) — on
   github.com, click **New repository**, make it Public, don't add a README
   (you already have one).
2. **Upload the files** — open the repo page, click **Add file → Upload
   files**, then drag the entire contents of `F:\Jellyfin\DinkFlix Bookmark`
   into the browser window (keep the folder structure: `.github`, `src`,
   `README.md`, etc. all at the top level of the repo). Click **Commit
   changes**.
3. **Wait for the build** — click the **Actions** tab at the top of your repo.
   You'll see a run called "Build Plugin" start automatically (takes a couple
   of minutes). A green check mark means it succeeded.
   - If it fails (red ✗), click into the run and open the "Build" step to see
     the error — that means something in the C# needs a small fix; paste the
     error back to me and I'll fix it.
4. **Download the result** — once it's green, open that run, scroll to the
   **Artifacts** section at the bottom, and download **DinkFlixWatchLater**.
   It's a zip containing a `DinkFlixWatchLater` folder with the plugin `.dll`
   inside.
5. **Install it on your Jellyfin server** — unzip it, then copy the
   `DinkFlixWatchLater` folder into your Jellyfin server's `plugins` folder
   (e.g. `%ProgramData%\Jellyfin\Server\plugins\` on Windows, or
   `/var/lib/jellyfin/plugins/` on Linux/Docker). Restart Jellyfin.
6. It'll now show up under **Dashboard → Plugins → My Plugins** as "DinkFlix
   Watch Later" — click it to enter your Seerr URL and API key.

That's the entire process — everything after step 2 is you clicking buttons
on github.com and copying one folder. You only need to repeat steps 3-5 when
you want to update the plugin later.

> Re-run steps 3-5 any time you (or I) push new changes to the repo — every
> push to `main` rebuilds it automatically.

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

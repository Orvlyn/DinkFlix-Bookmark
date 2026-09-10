using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.DinkFlixWatchLater.Api.Dto;
using Jellyfin.Plugin.DinkFlixWatchLater.Services;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.DinkFlixWatchLater.Api
{
    [ApiController]
    [Authorize]
    [Route("DinkFlixWatchLater")]
    public class WatchLaterController : ControllerBase
    {
        // matches Jellyfin.Api.Constants.InternalClaimTypes.UserId (not referenceable from plugins)
        private const string UserIdClaimType = "Jellyfin-UserId";

        private readonly WatchLaterStore _store;
        private readonly SeerrClient _seerrClient;
        private readonly ILibraryManager _libraryManager;

        public WatchLaterController(WatchLaterStore store, SeerrClient seerrClient, ILibraryManager libraryManager)
        {
            _store = store;
            _seerrClient = seerrClient;
            _libraryManager = libraryManager;
        }

        // never trust a client-supplied user id - always resolve it from the verified auth token
        private bool TryGetUserId(out string userId)
        {
            userId = User.FindFirst(UserIdClaimType)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? string.Empty;
            return !string.IsNullOrWhiteSpace(userId);
        }

        [HttpGet("items")]
        public async Task<ActionResult<IEnumerable<WatchLaterItemDto>>> GetItems()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var items = await _store.GetItemsAsync(userId).ConfigureAwait(false);
            return Ok(items.OrderByDescending(i => i.AddedAtUtc));
        }

        [HttpPost("items")]
        public async Task<ActionResult<WatchLaterItemDto>> AddItem([FromBody] AddItemRequest request)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var mediaType = request.MediaType == "tv" ? "tv" : "movie";
            var media = mediaType == "tv"
                ? await _seerrClient.GetTvAsync(request.TmdbId).ConfigureAwait(false)
                : await _seerrClient.GetMovieAsync(request.TmdbId).ConfigureAwait(false);

            if (media is null)
            {
                return NotFound("Could not resolve this title from Seerr.");
            }

            var item = new WatchLaterItemDto
            {
                TmdbId = request.TmdbId,
                MediaType = mediaType,
                Title = media.Title ?? media.Name ?? "Unknown title",
                PosterPath = media.PosterPath,
                BackdropPath = media.BackdropPath,
                OriginalLanguage = media.OriginalLanguage,
                Genres = media.Genres?.Select(g => g.Name).ToList() ?? new List<string>(),
                AddedAtUtc = DateTime.UtcNow
            };

            await _store.AddItemAsync(userId, item).ConfigureAwait(false);
            return Ok(item);
        }

        [HttpDelete("items/{tmdbId:int}")]
        public async Task<ActionResult> RemoveItem(
            [FromRoute] int tmdbId,
            [FromQuery] string mediaType = "movie")
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            await _store.RemoveItemAsync(userId, tmdbId, mediaType).ConfigureAwait(false);
            return NoContent();
        }

        [HttpGet("availability/{tmdbId:int}")]
        public async Task<ActionResult<AvailabilityDto>> GetAvailability(
            [FromRoute] int tmdbId,
            [FromQuery] string mediaType = "movie")
        {
            var result = new AvailabilityDto();

            var query = new InternalItemsQuery
            {
                HasAnyProviderId = new Dictionary<string, string> { { "Tmdb", tmdbId.ToString() } },
                IncludeItemTypes = mediaType == "tv"
                    ? new[] { BaseItemKind.Series }
                    : new[] { BaseItemKind.Movie }
            };

            var jellyfinItem = _libraryManager.GetItemList(query).FirstOrDefault();
            if (jellyfinItem is not null)
            {
                result.AvailableOnJellyfin = true;
                result.JellyfinItemId = jellyfinItem.Id.ToString("N");
            }

            var media = mediaType == "tv"
                ? await _seerrClient.GetTvAsync(tmdbId).ConfigureAwait(false)
                : await _seerrClient.GetMovieAsync(tmdbId).ConfigureAwait(false);

            result.SeerrStatus = media?.MediaInfo?.Status switch
            {
                2 => "pending",
                3 => "processing",
                4 => "partially_available",
                5 => "available",
                _ => "unknown"
            };

            result.CanRequest = !result.AvailableOnJellyfin && result.SeerrStatus == "unknown";

            return Ok(result);
        }

        [HttpPost("request/{tmdbId:int}")]
        public async Task<ActionResult> RequestItem([FromRoute] int tmdbId, [FromBody] RequestItemRequest request)
        {
            var mediaType = request.MediaType == "tv" ? "tv" : "movie";
            var success = await _seerrClient.RequestMediaAsync(tmdbId, mediaType).ConfigureAwait(false);
            return success ? Ok() : StatusCode(502, "Seerr rejected the request.");
        }

        // embedded resource names are rooted at the assembly's RootNamespace, not this controller's namespace
        private static readonly string ResourceRoot = typeof(Plugin).Namespace!;

        // fixed whitelist - never build the resource name from unvalidated client input alone
        private static readonly Dictionary<string, string> WebAssets = new(StringComparer.OrdinalIgnoreCase)
        {
            ["watchlater.css"] = "text/css",
            ["watchlater.js"] = "application/javascript"
        };

        [HttpGet("web/page")]
        [AllowAnonymous]
        public ActionResult GetPage()
        {
            var stream = GetType().Assembly.GetManifestResourceStream($"{ResourceRoot}.Web.watchlater.html");
            return stream is null ? NotFound() : File(stream, "text/html");
        }

        [HttpGet("web/{fileName}")]
        [AllowAnonymous]
        public ActionResult GetAsset([FromRoute] string fileName)
        {
            if (!WebAssets.TryGetValue(fileName, out var contentType))
            {
                return NotFound();
            }

            var stream = GetType().Assembly.GetManifestResourceStream($"{ResourceRoot}.Web.{fileName}");
            return stream is null ? NotFound() : File(stream, contentType);
        }
    }
}

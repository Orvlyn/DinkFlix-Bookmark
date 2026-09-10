using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly WatchLaterStore _store;
        private readonly SeerrClient _seerrClient;
        private readonly ILibraryManager _libraryManager;

        public WatchLaterController(WatchLaterStore store, SeerrClient seerrClient, ILibraryManager libraryManager)
        {
            _store = store;
            _seerrClient = seerrClient;
            _libraryManager = libraryManager;
        }

        [HttpGet("items")]
        public async Task<ActionResult<IEnumerable<WatchLaterItemDto>>> GetItems([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("userId is required.");
            }

            var items = await _store.GetItemsAsync(userId).ConfigureAwait(false);
            return Ok(items.OrderByDescending(i => i.AddedAtUtc));
        }

        [HttpPost("items")]
        public async Task<ActionResult<WatchLaterItemDto>> AddItem([FromBody] AddItemRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return BadRequest("userId is required.");
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

            await _store.AddItemAsync(request.UserId, item).ConfigureAwait(false);
            return Ok(item);
        }

        [HttpDelete("items/{tmdbId:int}")]
        public async Task<ActionResult> RemoveItem(
            [FromRoute] int tmdbId,
            [FromQuery] string userId,
            [FromQuery] string mediaType = "movie")
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("userId is required.");
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

        [HttpGet("web/page")]
        [AllowAnonymous]
        public ActionResult GetPage()
        {
            var stream = GetType().Assembly.GetManifestResourceStream($"{GetType().Namespace}.Web.watchlater.html");
            return stream is null ? NotFound() : File(stream, "text/html");
        }

        [HttpGet("web/{fileName}")]
        [AllowAnonymous]
        public ActionResult GetAsset([FromRoute] string fileName)
        {
            var contentType = fileName.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
                ? "text/css"
                : "application/javascript";

            var stream = GetType().Assembly.GetManifestResourceStream($"{GetType().Namespace}.Web.{fileName}");
            return stream is null ? NotFound() : File(stream, contentType);
        }
    }
}

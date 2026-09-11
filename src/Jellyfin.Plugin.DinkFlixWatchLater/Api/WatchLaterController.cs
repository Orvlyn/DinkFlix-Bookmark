using System.Security.Claims;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.DinkFlixWatchLater.Api.Dto;
using Jellyfin.Plugin.DinkFlixWatchLater.Services;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.DinkFlixWatchLater.Api;

[ApiController]
[Authorize]
[Route("DinkFlixWatchLater")]
public sealed class WatchLaterController : ControllerBase
{
    private const string UserClaim = "Jellyfin-UserId";
    private static readonly string Root = typeof(Plugin).Namespace!;
    private static readonly Dictionary<string, string> Assets = new(StringComparer.OrdinalIgnoreCase)
    { ["watchlater.css"] = "text/css", ["watchlater.js"] = "application/javascript" };

    private readonly WatchLaterStore _store;
    private readonly ILibraryManager _library;

    public WatchLaterController(WatchLaterStore store, ILibraryManager library)
    {
        _store = store;
        _library = library;
    }

    private bool TryGetUserId(out string id)
    {
        id = HttpContext.User.FindFirst(UserClaim)?.Value
            ?? HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? string.Empty;
        return !string.IsNullOrWhiteSpace(id);
    }

    [HttpGet("items")]
    public async Task<ActionResult<IEnumerable<WatchLaterItemDto>>> GetItems()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        return Ok((await _store.GetItemsAsync(userId)).OrderByDescending(x => x.AddedAtUtc));
    }

    [HttpPost("items")]
    public async Task<ActionResult<WatchLaterItemDto>> AddItem([FromBody] AddItemRequest request)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        if (request.TmdbId <= 0) return BadRequest("A valid TMDB ID is required.");
        var mediaType = string.Equals(request.MediaType, "tv", StringComparison.OrdinalIgnoreCase) ? "tv" : "movie";
        if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("A title is required.");

        var item = new WatchLaterItemDto
        {
            TmdbId = request.TmdbId, MediaType = mediaType, Title = request.Title.Trim(),
            PosterPath = request.PosterPath, BackdropPath = request.BackdropPath,
            OriginalLanguage = request.OriginalLanguage, AddedAtUtc = DateTime.UtcNow
        };
        await _store.AddItemAsync(userId, item);
        return Ok(item);
    }

    [HttpDelete("items/{tmdbId:int}")]
    public async Task<IActionResult> RemoveItem(int tmdbId, [FromQuery] string mediaType = "movie")
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        await _store.RemoveItemAsync(userId, tmdbId, mediaType == "tv" ? "tv" : "movie");
        return NoContent();
    }

    [HttpGet("availability/{tmdbId:int}")]
    public ActionResult<AvailabilityDto> Availability(int tmdbId, [FromQuery] string mediaType = "movie")
    {
        if (!TryGetUserId(out _)) return Unauthorized();
        var result = new AvailabilityDto();
        var query = new InternalItemsQuery
        {
            HasAnyProviderId = new Dictionary<string, string> { ["Tmdb"] = tmdbId.ToString() },
            IncludeItemTypes = mediaType == "tv" ? new[] { BaseItemKind.Series } : new[] { BaseItemKind.Movie }
        };
        var item = _library.GetItemList(query).FirstOrDefault();
        if (item is not null)
        {
            result.AvailableOnJellyfin = true;
            result.JellyfinItemId = item.Id.ToString("N");
        }
        return Ok(result);
    }

    [HttpGet("web/page")]
    [AllowAnonymous]
    public ActionResult Page()
    {
        var stream = GetType().Assembly.GetManifestResourceStream($"{Root}.Web.watchlater.html");
        return stream is null ? NotFound() : File(stream, "text/html");
    }

    [HttpGet("web/{fileName}")]
    [AllowAnonymous]
    public ActionResult Asset(string fileName)
    {
        if (!Assets.TryGetValue(fileName, out var contentType)) return NotFound();
        var stream = GetType().Assembly.GetManifestResourceStream($"{Root}.Web.{fileName}");
        return stream is null ? NotFound() : File(stream, contentType);
    }
}

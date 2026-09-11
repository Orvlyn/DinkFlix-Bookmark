using System.Text.Json;
using System.Threading;
using Jellyfin.Plugin.DinkFlixWatchLater.Api.Dto;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Plugin.DinkFlixWatchLater.Services;

public sealed class WatchLaterStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    private readonly string _directory;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public WatchLaterStore(IApplicationPaths applicationPaths)
    {
        _directory = Path.Combine(applicationPaths.PluginsPath, "DinkFlixWatchLater", "data");
        Directory.CreateDirectory(_directory);
    }

    private string PathFor(string userId) => Path.Combine(_directory, $"{userId}.json");

    public async Task<List<WatchLaterItemDto>> GetItemsAsync(string userId)
    {
        await _gate.WaitAsync();
        try { return await LoadAsync(userId); }
        finally { _gate.Release(); }
    }

    public async Task AddItemAsync(string userId, WatchLaterItemDto item)
    {
        await _gate.WaitAsync();
        try
        {
            var items = await LoadAsync(userId);
            items.RemoveAll(x => x.TmdbId == item.TmdbId && x.MediaType == item.MediaType);
            items.Add(item);
            await SaveAsync(userId, items);
        }
        finally { _gate.Release(); }
    }

    public async Task RemoveItemAsync(string userId, int tmdbId, string mediaType)
    {
        await _gate.WaitAsync();
        try
        {
            var items = await LoadAsync(userId);
            items.RemoveAll(x => x.TmdbId == tmdbId && string.Equals(x.MediaType, mediaType, StringComparison.OrdinalIgnoreCase));
            await SaveAsync(userId, items);
        }
        finally { _gate.Release(); }
    }

    private async Task<List<WatchLaterItemDto>> LoadAsync(string userId)
    {
        var path = PathFor(userId);
        if (!File.Exists(path)) return [];
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<List<WatchLaterItemDto>>(stream, Options) ?? [];
    }

    private async Task SaveAsync(string userId, List<WatchLaterItemDto> items)
    {
        var path = PathFor(userId);
        var temp = path + ".tmp";
        await using (var stream = File.Create(temp))
            await JsonSerializer.SerializeAsync(stream, items, Options);
        File.Move(temp, path, true);
    }
}

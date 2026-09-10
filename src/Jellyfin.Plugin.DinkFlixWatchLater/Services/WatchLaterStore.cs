using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.DinkFlixWatchLater.Api.Dto;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Plugin.DinkFlixWatchLater.Services
{
    /// <summary>
    /// Persists each user's Watch Later list as a JSON file under the plugin's data directory.
    /// </summary>
    public class WatchLaterStore
    {
        private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

        private readonly string _dataDirectory;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public WatchLaterStore(IApplicationPaths applicationPaths)
        {
            _dataDirectory = Path.Combine(applicationPaths.PluginsPath, "DinkFlixWatchLater", "data");
            Directory.CreateDirectory(_dataDirectory);
        }

        private string GetPathForUser(string userId) => Path.Combine(_dataDirectory, $"{userId}.json");

        public async Task<List<WatchLaterItemDto>> GetItemsAsync(string userId)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                return await LoadUnlockedAsync(userId).ConfigureAwait(false);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task AddItemAsync(string userId, WatchLaterItemDto item)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                var items = await LoadUnlockedAsync(userId).ConfigureAwait(false);
                items.RemoveAll(i => i.TmdbId == item.TmdbId && i.MediaType == item.MediaType);
                items.Add(item);
                await SaveUnlockedAsync(userId, items).ConfigureAwait(false);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task RemoveItemAsync(string userId, int tmdbId, string mediaType)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                var items = await LoadUnlockedAsync(userId).ConfigureAwait(false);
                items.RemoveAll(i => i.TmdbId == tmdbId && i.MediaType == mediaType);
                await SaveUnlockedAsync(userId, items).ConfigureAwait(false);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<List<WatchLaterItemDto>> LoadUnlockedAsync(string userId)
        {
            var path = GetPathForUser(userId);
            if (!File.Exists(path))
            {
                return new List<WatchLaterItemDto>();
            }

            await using var stream = File.OpenRead(path);
            var items = await JsonSerializer.DeserializeAsync<List<WatchLaterItemDto>>(stream).ConfigureAwait(false);
            return items ?? new List<WatchLaterItemDto>();
        }

        private async Task SaveUnlockedAsync(string userId, List<WatchLaterItemDto> items)
        {
            var path = GetPathForUser(userId);
            await using var stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, items, SerializerOptions).ConfigureAwait(false);
        }
    }
}

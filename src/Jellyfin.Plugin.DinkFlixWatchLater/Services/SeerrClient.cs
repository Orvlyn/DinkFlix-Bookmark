using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.DinkFlixWatchLater.Services
{
    /// <summary>
    /// Thin client for the parts of Seerr's (Jellyseerr/Overseerr) REST API Watch Later needs.
    /// </summary>
    public class SeerrClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SeerrClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateClient()
        {
            var config = Plugin.Instance!.Configuration;
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(config.SeerrUrl.TrimEnd('/') + "/");
            client.DefaultRequestHeaders.Add("X-Api-Key", config.SeerrApiKey);
            return client;
        }

        public Task<SeerrMediaResponse?> GetMovieAsync(int tmdbId) =>
            CreateClient().GetFromJsonAsync<SeerrMediaResponse>($"api/v1/movie/{tmdbId}");

        public Task<SeerrMediaResponse?> GetTvAsync(int tmdbId) =>
            CreateClient().GetFromJsonAsync<SeerrMediaResponse>($"api/v1/tv/{tmdbId}");

        public async Task<bool> RequestMediaAsync(int tmdbId, string mediaType)
        {
            var body = new { mediaType, mediaId = tmdbId };
            var response = await CreateClient().PostAsJsonAsync("api/v1/request", body).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }
    }

    public class SeerrMediaResponse
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("backdrop_path")]
        public string? BackdropPath { get; set; }

        [JsonPropertyName("original_language")]
        public string? OriginalLanguage { get; set; }

        [JsonPropertyName("genres")]
        public SeerrGenre[]? Genres { get; set; }

        [JsonPropertyName("mediaInfo")]
        public SeerrMediaInfo? MediaInfo { get; set; }
    }

    public class SeerrGenre
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class SeerrMediaInfo
    {
        // Seerr status codes: 1=unknown 2=pending 3=processing 4=partially available 5=available
        [JsonPropertyName("status")]
        public int Status { get; set; }
    }
}

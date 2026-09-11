using System;using System.Collections.Generic;using System.Net.Http;using System.Net.Http.Json;using System.Text.Json.Serialization;using System.Threading.Tasks;
namespace Jellyfin.Plugin.DinkFlixWatchLater.Services;
public class SeerrClient{
 readonly IHttpClientFactory _factory; public SeerrClient(IHttpClientFactory factory)=>_factory=factory;
 HttpClient C(){var cfg=Plugin.Instance?.Configuration??throw new InvalidOperationException("Plugin not initialized.");if(string.IsNullOrWhiteSpace(cfg.SeerrUrl))throw new InvalidOperationException("Seerr URL is not configured.");var c=_factory.CreateClient();c.BaseAddress=new Uri(cfg.SeerrUrl.TrimEnd('/')+"/");if(!string.IsNullOrWhiteSpace(cfg.SeerrApiKey))c.DefaultRequestHeaders.Add("X-Api-Key",cfg.SeerrApiKey);return c;}
 public Task<SeerrMediaResponse?> GetMovieAsync(int id)=>C().GetFromJsonAsync<SeerrMediaResponse>($"api/v1/movie/{id}");
 public Task<SeerrMediaResponse?> GetTvAsync(int id)=>C().GetFromJsonAsync<SeerrMediaResponse>($"api/v1/tv/{id}");
 public async Task<bool> RequestMediaAsync(int id,string type){using var r=await C().PostAsJsonAsync("api/v1/request",new{mediaType=type=="tv"?"tv":"movie",mediaId=id});return r.IsSuccessStatusCode;}
 public Task<SeerrSearchResponse?> SearchAsync(string q)=>C().GetFromJsonAsync<SeerrSearchResponse>($"api/v1/search?query={Uri.EscapeDataString(q.Trim())}");
 public async Task<List<SeerrDiscoverItem>> RecommendationsAsync(int id,string type){var p=type=="tv"?$"api/v1/tv/{id}/recommendations":$"api/v1/movie/{id}/recommendations";var r=await C().GetFromJsonAsync<SeerrDiscoverResponse>(p);return r?.Results??[];}
 public async Task<(bool Connected, string Message)> TestConnectionAsync(string url, string apiKey)
{
    if (string.IsNullOrWhiteSpace(url)) return (false, "Seerr URL is empty.");
    if (string.IsNullOrWhiteSpace(apiKey)) return (false, "Seerr API key is empty.");
    try
    {
        using var client = _factory.CreateClient();
        client.BaseAddress = new Uri(url.Trim().TrimEnd('/') + "/");
        client.DefaultRequestHeaders.Remove("X-Api-Key");
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey.Trim());
        using var response = await client.GetAsync("api/v1/status");
        if (response.IsSuccessStatusCode) return (true, "Seerr is reachable and accepted the API key.");
        return (false, $"Seerr returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).");
    }
    catch (Exception ex)
    {
        return (false, ex.Message);
    }
}

public Task<(bool Connected, string Message)> TestConnectionAsync()
{
    var cfg=Plugin.Instance?.Configuration??throw new InvalidOperationException("Plugin not initialized.");
    return TestConnectionAsync(cfg.SeerrUrl, cfg.SeerrApiKey);
}
}
public class SeerrMediaResponse{[JsonPropertyName("id")]public int Id{get;set;}[JsonPropertyName("title")]public string? Title{get;set;}[JsonPropertyName("name")]public string? Name{get;set;}[JsonPropertyName("poster_path")]public string? PosterPath{get;set;}[JsonPropertyName("backdrop_path")]public string? BackdropPath{get;set;}[JsonPropertyName("original_language")]public string? OriginalLanguage{get;set;}[JsonPropertyName("genres")]public SeerrGenre[]? Genres{get;set;}[JsonPropertyName("mediaInfo")]public SeerrMediaInfo? MediaInfo{get;set;}}
public class SeerrGenre{[JsonPropertyName("name")]public string Name{get;set;}=string.Empty;}
public class SeerrMediaInfo{[JsonPropertyName("status")]public int Status{get;set;}}
public class SeerrSearchResponse{[JsonPropertyName("results")]public List<SeerrDiscoverItem> Results{get;set;}=[];}
public class SeerrDiscoverResponse{[JsonPropertyName("results")]public List<SeerrDiscoverItem> Results{get;set;}=[];}
public class SeerrDiscoverItem{
[JsonPropertyName("id")]public int Id{get;set;}[JsonPropertyName("mediaType")]public string? MediaType{get;set;}[JsonPropertyName("media_type")]public string? MediaTypeSnake{get;set;}
[JsonPropertyName("title")]public string? Title{get;set;}[JsonPropertyName("name")]public string? Name{get;set;}[JsonPropertyName("posterPath")]public string? PosterPath{get;set;}[JsonPropertyName("poster_path")]public string? PosterSnake{get;set;}[JsonPropertyName("backdropPath")]public string? BackdropPath{get;set;}[JsonPropertyName("backdrop_path")]public string? BackdropSnake{get;set;}[JsonPropertyName("originalLanguage")]public string? OriginalLanguage{get;set;}[JsonPropertyName("original_language")]public string? OriginalLanguageSnake{get;set;}
[JsonIgnore]public string Type=>(MediaType??MediaTypeSnake??"").Trim().ToLowerInvariant();
[JsonIgnore]public string DisplayTitle=>Title??Name??"Unknown title";[JsonIgnore]public string? Poster=>PosterPath??PosterSnake;[JsonIgnore]public string? Backdrop=>BackdropPath??BackdropSnake;[JsonIgnore]public string? Language=>OriginalLanguage??OriginalLanguageSnake;}

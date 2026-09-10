using System;
using System.Collections.Generic;
using System.Linq;

namespace Jellyfin.Plugin.DinkFlixWatchLater.Api.Dto
{
    /// <summary>
    /// A single saved Watch Later entry.
    /// </summary>
    public class WatchLaterItemDto
    {
        public int TmdbId { get; set; }

        /// <summary>"movie" or "tv".</summary>
        public string MediaType { get; set; } = "movie";

        public string Title { get; set; } = string.Empty;

        public string? PosterPath { get; set; }

        public string? BackdropPath { get; set; }

        public string? OriginalLanguage { get; set; }

        public List<string> Genres { get; set; } = new();

        public DateTime AddedAtUtc { get; set; }

        /// <summary>Anime is split out of the TV tab for Japanese animation.</summary>
        public bool IsAnime => OriginalLanguage == "ja" && Genres.Contains("Animation");

        public string Category => IsAnime ? "anime" : (MediaType == "tv" ? "tv" : "movie");
    }
}

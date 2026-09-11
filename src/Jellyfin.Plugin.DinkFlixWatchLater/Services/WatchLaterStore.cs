using System.Collections.Generic;using System.IO;using System.Text.Json;using System.Threading;using System.Threading.Tasks;using Jellyfin.Plugin.DinkFlixWatchLater.Api.Dto;using MediaBrowser.Common.Configuration;
namespace Jellyfin.Plugin.DinkFlixWatchLater.Services;
public class WatchLaterStore{static readonly JsonSerializerOptions O=new(){WriteIndented=true,PropertyNameCaseInsensitive=true};readonly string dir;readonly SemaphoreSlim gate=new(1,1);public WatchLaterStore(IApplicationPaths a){dir=Path.Combine(a.PluginsPath,"DinkFlixWatchLater","data");Directory.CreateDirectory(dir);}string PathFor(string u)=>Path.Combine(dir,$"{u}.json");
public async Task<List<WatchLaterItemDto>> GetItemsAsync(string u){await gate.WaitAsync();try{return await Load(u);}finally{gate.Release();}}
public async Task AddItemAsync(string u,WatchLaterItemDto i){await gate.WaitAsync();try{var x=await Load(u);x.RemoveAll(a=>a.TmdbId==i.TmdbId&&a.MediaType==i.MediaType);x.Add(i);await Save(u,x);}finally{gate.Release();}}
public async Task RemoveItemAsync(string u,int id,string t){await gate.WaitAsync();try{var x=await Load(u);x.RemoveAll(a=>a.TmdbId==id&&a.MediaType==t);await Save(u,x);}finally{gate.Release();}}
async Task<List<WatchLaterItemDto>> Load(string u){var p=PathFor(u);if(!File.Exists(p))return [];await using var s=File.OpenRead(p);return await JsonSerializer.DeserializeAsync<List<WatchLaterItemDto>>(s,O)??[];}
async Task Save(string u,List<WatchLaterItemDto> x){var p=PathFor(u);var t=p+".tmp";await using(var s=File.Create(t)){await JsonSerializer.SerializeAsync(s,x,O);}File.Move(t,p,true);}
}

using System;
using System.Net.Http;
using System.Threading.Tasks;
using OverwatchCore.Data;
using OverwatchCore.Enums;
using Newtonsoft.Json;
using OverwatchCore.Core.Parser;
using OverwatchCore.Extensions;

namespace OverwatchCore.Core
{
    public class OverstatClient
    {
        public async Task<Player> GetPlayerStatsAsync(Platform platform, string userName)
        {
            RawPlayerStats stats;
            string url;

            using (var client = new HttpClient() { BaseAddress = new Uri("https://ovrstat.com/stats/") })
            {
                var path = "";
                switch(platform)
                {
                    case Platform.Psn:
                        if (!userName.IsValidPsnId()) return null;
                        path = $"psn/{userName}";
                        break;
                    case Platform.Xbl:
                        if (!userName.IsValidXblId()) return null;
                        path = $"xbl/{userName}";
                        break;
                    case Platform.Pc:
                    default:
                        if (!userName.IsValidBattletag()) return null;
                        path = $"pc/us/{ userName.BattletagToUrlFriendlyString() }";
                        break;
                }

                using(var request = await client.GetAsync(path))
                {
                    if (!request.IsSuccessStatusCode) return null;
                    var rsltContent = await request.Content.ReadAsStringAsync();
                    if (rsltContent.Contains("Player Not Found")) return null;
                    url = request.RequestMessage.RequestUri.ToString();
                    stats = RawPlayerStats.FromJson(rsltContent);
                }
            }

            return stats?.Parse(platform, url);
        }
    }
}

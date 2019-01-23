using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OverwatchCore.Enums;
using OverwatchCore.Extensions;

namespace OverwatchCore.Core.WebClient
{
    internal sealed class HttpProfileClient : ProfileClient
    {
        private readonly HttpClient _careerClient;
        private readonly HttpClient _searchClient;

        internal HttpProfileClient()
        {
            // TODO: Keep an eye on this to see if TLS 1.2 support makes it's way into older framework versions - seems unlikely though.
            try { ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; }
            catch { ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11; }
            _careerClient = new HttpClient
            {
                BaseAddress = new Uri("https://playoverwatch.com/en-us/career/")
            };
            _searchClient = new HttpClient
            {
                BaseAddress = new Uri("https://playoverwatch.com/en-us/career/platforms/")
            };
        }

        public override void Dispose() 
        {
            _careerClient.Dispose();
            _searchClient.Dispose();
        }

        internal override Task<ProfileRequestData> GetProfileExact(string username, Platform platform)
        {
            var reqUrl = platform != Platform.Pc 
                ? $"{platform.ToLowerString()}/{username}" 
                : $"pc/{username.BattletagToUrlFriendlyString()}";
            return GetProfileRequestData(reqUrl, platform);
        }

        internal override async Task<ProfileRequestData> GetProfileDetectPlatform(string username)
        {
            if (username.IsValidBattletag()) return await GetProfileRequestData($"pc/{username.BattletagToUrlFriendlyString()}", Platform.Pc);
            foreach(var platform in Enum.GetValues(typeof(Platform)).Cast<Platform>().Where(x => x != Platform.Pc))
            {
                var result = await GetProfileRequestData($"{platform.ToLowerString()}/{username.BattletagToUrlFriendlyString()}", platform);
                if (result == null) continue;
                return result;
            }
            return null;
        }

        internal async Task<ProfileRequestData> GetProfileRequestData(string reqString, Platform platform)
        {
            var pageInfo = await GetProfilePageInformation(reqString, platform);
            if (!pageInfo.HasValue) return null;
            
            return new ProfileRequestData(pageInfo.Value.Url, pageInfo.Value.Content, platform);
        }

        internal async Task<(string Url, string Content)?> GetProfilePageInformation(string reqString, Platform platform)
        {
            using (var result = await _careerClient.GetAsync(reqString))
            {
                if (!result.IsSuccessStatusCode) return null;
                var rsltContent = await result.Content.ReadAsStringAsync();
                if (rsltContent.Contains("Profile Not Found")) return null;
                var rsltUrl = result.RequestMessage.RequestUri.ToString();
                return (rsltUrl, rsltContent);
            }
        }

        internal async Task<(string Url, string Content)?> GetProfileApiInformation(string userId)
        {
            using (var secResult = await _searchClient.GetAsync(userId))
            {
                if (!secResult.IsSuccessStatusCode) return null;
                var secRsltContent = await secResult.Content.ReadAsStringAsync();
                if (secRsltContent.Contains("[]")) return null;
                var secRsltUrl = secResult.RequestMessage.RequestUri.ToString();
                return (secRsltUrl, secRsltContent);
            }
        }

        internal override async Task<List<Alias>> GetAliases(string id)
        {
            using (var result = await _careerClient.GetAsync($"platforms/{id}"))
            {
                if (!result.IsSuccessStatusCode) return null;
                var jsonText = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Alias>>(jsonText);
            }
        }
    }
}

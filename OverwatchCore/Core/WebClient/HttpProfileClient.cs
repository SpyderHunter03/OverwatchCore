using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OverwatchCore.Data;
using OverwatchCore.Enums;
using OverwatchCore.Extensions;

namespace OverwatchCore.Core.WebClient
{
    internal sealed class HttpProfileClient : ProfileClient
    {
        private readonly Uri _careerUri;
        private readonly Uri _platformUri;
        private readonly Uri _ovrstatUri;

        internal HttpProfileClient()
        {
            // TODO: Keep an eye on this to see if TLS 1.2 support makes it's way into older framework versions - seems unlikely though.
            try { ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; }
            catch { ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11; }
            
            _careerUri = new Uri("https://playoverwatch.com/en-us/career/");
            _platformUri = new Uri("https://playoverwatch.com/en-us/career/platforms/");
            _ovrstatUri = new Uri("https://ovrstat.com/stats/");
        }

        public override void Dispose() 
        {
            base.Dispose();
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
            if (username.IsValidBattletag())
                return await GetProfileRequestData($"pc/{username.BattletagToUrlFriendlyString()}", Platform.Pc);
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
            var values = await GetValuesFromClient(_careerUri, reqString);
            
            if (!values.HasValue || values.Value.Content.Contains("Profile Not Found")) 
                return null;

            return values;
        }

        internal override async Task<ICollection<Alias>> GetAliases(string id)
        {
            var values = await GetValuesFromClient(_platformUri, id);

            if (!values.HasValue || values.Value.Content.Contains("[]")) 
                return null;

            var aliases = JsonConvert.DeserializeObject<List<Alias>>(values.Value.Content);

            return aliases;
        }

        internal async Task<(string Url, string Content)?> GetOverstatProfile(string reqString, Platform platform)
        {
            var values = await GetValuesFromClient(_careerUri, reqString);
            
            if (!values.HasValue || values.Value.Content.Contains("Player Not Found"))
                return null;

            return values;
        }

        private async Task<(string Url, string Content)?> GetValuesFromClient(Uri uri, string getValue)
        {
            using (var result = await new HttpClient().GetAsync($"{uri}{getValue}"))
            {
                if (!result.IsSuccessStatusCode) return null;
                var rsltContent = await result.Content.ReadAsStringAsync();
                var rsltUrl = result.RequestMessage.RequestUri.ToString();
                return (rsltUrl, rsltContent);
            }
        }
    }
}

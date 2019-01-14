using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OverwatchCore;
using OverwatchCore.Core.WebClient;
using OverwatchCore.Enums;
using static OverwatchCore.Data.Player;

namespace OverwatchCore.Tests.Core.WebClient
{
    internal sealed class MockProfileClient : ProfileClient
    {
        private readonly ProfileRequestData _mockData;

        public MockProfileClient()
        {
            _mockData = new ProfileRequestData(
                "https://playoverwatch.com/en-gb/career/pc/eu/SpyderHunter-1589", 
                File.ReadAllText("TestSource.txt"), 
                Platform.Pc, 
                "https://playoverwatch.com/en-us/search/account-by-name/SpyderHunter#1589",
                File.ReadAllText("SearchTestSource.txt"));
        }

        internal override Task<ProfileRequestData> GetProfileExact(string username, Platform platform)
        {
            return Task.FromResult(_mockData);
        }

        internal override Task<ProfileRequestData> GetProfileDetectPlatform(string username)
        {
            throw new NotImplementedException(); // TODO: no real way to test this but maybe someday...
        }

        internal override Task<List<Alias>> GetAliases(string id)
        {
            throw new NotImplementedException(); // TODO: Maybe test this too.
        }
    }
}

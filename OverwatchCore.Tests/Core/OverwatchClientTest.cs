using System;
using OverwatchCore;
using OverwatchCore.Core;
using OverwatchCore.Enums;
using OverwatchCore.Tests.Core.WebClient;
using Xunit;

namespace OverwatchCore.Tests.Core
{
    public class OverwatchClientTest
    {
        [Fact]
        public async void GetPlayer_Username_Only_Overload_With_Battletag_Argument_Returns_Valid_Page()
        {
            var mockWebClient = new MockProfileClient();
            using (var owClient = new OverwatchClient(mockWebClient))
            {
                var result = await owClient.GetPlayerAsync("SpyderHunter#1589");
                Assert.Equal("https://playoverwatch.com/en-gb/career/pc/eu/SpyderHunter-1589", result.ProfileUrl);
            }
        }

        [Fact]
        public async void GetPlayer_Username_Only_Overload_With_Battletag_Argument_And_No_Pc_Region_Should_Throw_Exception()
        {
            var mockWebClient = new MockProfileClient();
            using (var owClient = new OverwatchClient(mockWebClient, Platform.Psn, Platform.Xbl))
            {
                await Assert.ThrowsAsync<ArgumentException>(async () => await owClient.GetPlayerAsync("SpyderHunter#1589"));
            }
        }
    }
}

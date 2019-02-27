using FluentAssertions;
using OverwatchCore.Core;
using OverwatchCore.Enums;
using Xunit;

namespace OverwatchCore.Tests.Core
{
    public class OverstatClientTest
    {
        private string name = "SpyderHunter#1589";
        private string url = "https://playoverwatch.com/en-gb/career/pc/eu/SpyderHunter-1589";

        [Fact]
        public async void GetPlayerStats_ReturnsNotNull_WhenLookingForMyPersonalProfile()
        {
            using (var osClient = new OverstatClient())
            {
                var playerStats = await osClient.GetPlayerStatsAsync(Platform.Pc, name);

                playerStats.Should().NotBeNull();
            }
        }

        [Fact]
        public async void GetPlayerAsync_ReturnsNotNull_WhenLookingForMyPersonalProfile()
        {
            using (var osClient = new OverstatClient())
            {
                var playerStats = await osClient.GetPlayerAsync(name);

                playerStats.Should().NotBeNull();
                playerStats.PlayerLevel.Should().BeGreaterOrEqualTo(717);
            }
        }
    }
}
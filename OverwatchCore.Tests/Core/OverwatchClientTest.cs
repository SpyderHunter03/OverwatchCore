using System;
using System.Threading.Tasks;
using FluentAssertions;
using OverwatchCore;
using OverwatchCore.Core;
using OverwatchCore.Enums;
using OverwatchCore.Tests.Core.WebClient;
using Xunit;

namespace OverwatchCore.Tests.Core
{
    public class OverwatchClientTest
    {
        private string name = "SpyderHunter#1589";
        private string url = "https://playoverwatch.com/en-gb/career/pc/eu/SpyderHunter-1589";

        [Fact]
        public async void GetPlayer_Username_Only_Overload_With_Battletag_Argument_Returns_Valid_Page()
        {
            using (var owClient = new OverwatchClient(new MockProfileClient()))
            {
                var result = await owClient.GetPlayerAsync(name);

                result.Should().NotBeNull();
                result.ProfileUrl.Should().NotBeNull();
                result.ProfileUrl.Should().Be(url);
            }
        }

        [Fact]
        public async void GetPlayer_Username_Only_Overload_With_Battletag_Argument_And_No_Pc_Region_Should_Throw_Exception()
        {
            using (var owClient = new OverwatchClient(new MockProfileClient(), Platform.Psn, Platform.Xbl))
            {
                Func<Task> getPlayerAsync = () => owClient.GetPlayerAsync(name);
                await getPlayerAsync.Should().ThrowAsync<ArgumentException>();
            }
        }
    }
}

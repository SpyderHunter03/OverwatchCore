using FluentAssertions;
using OverwatchCore;
using OverwatchCore.Core;
using OverwatchCore.Enums;
using Xunit;

namespace OverwatchCore.Tests.Core
{
    public class OverwatchClientIntergrationTests
    {
        private const string psnUsername = "Frostbit794";
        private const string psnUrl = "https://playoverwatch.com/en-us/career/psn/Frostbit794";
        private const string pcUsername = "Frostbit#11387";
        private const string pcUrl = "https://playoverwatch.com/en-us/career/pc/Frostbit-11387";

        private const string pcTestUsername = "Kephrii#11520";
        private const string pcTestUrl = "https://playoverwatch.com/en-us/career/pc/Kephrii-11520";

        private static readonly OverwatchClient Client = new OverwatchClient();

        [Fact]
        public async void GetPlayer_Psn_AutoDetect_Should_Return_Correct_Private_Page_With_Other_Platforms_Listed()
        { 
            var result = await Client.GetPlayerAsync(psnUsername);
            result.ProfileUrl.Should().Be(psnUrl);
            result.Platform.Should().Be(Platform.Psn);
            result.IsProfilePrivate.Should().BeFalse();

            await Client.GetAliasesAsync(result);

            // result.Aliases.Should().Contain(x => x.Platform == Platform.Pc);
            
            var otherProfiles = await Client.GetOtherProfilesAsync(result);
            
            otherProfiles.Should().NotBeEmpty();
            otherProfiles.Should().Contain(x => x.ProfileUrl.Equals(pcUrl));
            otherProfiles.Should().Contain(x => x.Username.Equals(pcUsername));
        }

        [Fact]
        public async void GetPlayer_Pc_AutoDetect_Should_Return_Correct_Public_Page_With_No_Other_Platforms_Listed()
        {
            var result = await Client.GetPlayerAsync(pcTestUsername);
            result.ProfileUrl.Should().Be(pcTestUrl);
            result.Platform.Should().Be(Platform.Pc);
            result.IsProfilePrivate.Should().BeFalse();
        }
    }
}

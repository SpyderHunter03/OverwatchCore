using FluentAssertions;
using System.IO;
using OverwatchCore;
using OverwatchCore.Extensions;
using OverwatchCore.Core.Parser;
using OverwatchCore.Core.WebClient;
using Xunit;
using OverwatchCore.Data;
using OverwatchCore.Enums;

namespace OverwatchCore.Tests.Core.Parser
{
    public class ProfileParserTest
    {
        // These tests are run on the source from this page: http://playoverwatch.com/en-us/career/pc/moiph-1288
        // The source of the page was downloaded and stored on 02/07/2018
        // Will serve as the basis for parser tests barring a more reliable solution.

        private static readonly Player _testPlayer;

        static ProfileParserTest()
        {
            var source = File.ReadAllText("TestSource.txt");
            var searchSource = File.ReadAllText("SearchTestSource.txt");
            var data = new ProfileClient.ProfileRequestData("", source, Platform.Pc, "", searchSource);
            var parser = new ProfileParser();
            _testPlayer = new Player();
            parser.Parse(_testPlayer, data).GetAwaiter().GetResult();
        }       

        [Fact]
        public void Parsed_Profile_PlayerLevel_Should_Be_Correct() => 
            _testPlayer.PlayerLevel.Should().BeGreaterOrEqualTo(600);

        [Fact]
        public void Parsed_Profile_EndorsementLevel_Should_Be_Correct() =>
            _testPlayer.EndorsementLevel.Should().BeGreaterOrEqualTo(1);

        [Fact]
        public void Parsed_Profile_EndorsementStats_Should_Be_Correct() =>
            _testPlayer.Endorsements[Endorsement.GoodTeammate].Should().Be(0.1m);

        [Fact]
        public void Parsed_Profile_CompetitiveRank_Should_Be_Correct() => 
            _testPlayer.CompetitiveRank.Should().BeGreaterOrEqualTo(2300);

        [Fact]
        public void Parsed_Profile_CasualStats_Should_Be_Correct() => 
            _testPlayer.CasualStats.GetStatExact("AllHeroes", "Assists", "Healing Done").Value.Should().Be(2576219);

        [Fact]
        public void Parsed_Profile_CompetitiveStats_Should_Be_Correct() => 
            _testPlayer.CompetitiveStats.GetStatExact("Lucio", "Game", "Time Played").Value.Should().Be(1560);

        [Fact]
        public void Parsed_Profile_Achievements_Should_Be_Correct()
        {
            _testPlayer.Achievements.FilterByName("Hog Wild").IsEarned.Should().BeTrue();
            _testPlayer.Achievements.FilterByName("Rapid Discord").IsEarned.Should().BeTrue();
        }

        [Fact]
        public void Parsed_Profile_PortraitImage_Should_Be_Correct() => 
            _testPlayer.ProfilePortraitUrl.Should().Be("https://assets.webn.mobi/overwatch/5122deb567422e30496f656856f70d028bfc70a89eaa28d8ea662308b5df42fa.png");

        [Fact]
        public void Parsed_Profile_CompetitiveRankImage_Should_Be_Correct() => 
            _testPlayer.CompetitiveRankImageUrl.Should().Be("https://d1u1mce87gyfbn.cloudfront.net/game/rank-icons/season-2/rank-3.png");

        [Fact]
        public void Parsed_Profile_PlayerLevelImage_Should_Be_Correct() => 
            _testPlayer.PlayerLevelImage.Should().Be("https://d1u1mce87gyfbn.cloudfront.net/game/playerlevelrewards/0x0250000000000974_Border.png");
    }
}

using FluentAssertions;
using System.IO;
using OverwatchCore;
using OverwatchCore.Extensions;
using OverwatchCore.Core.Parser;
using OverwatchCore.Core.WebClient;
using Xunit;
using OverwatchCore.Data;
using OverwatchCore.Enums;
using System;
using System.Linq;
using System.Diagnostics;
using Xunit.Abstractions;
using System.Threading.Tasks;

namespace OverwatchCore.Tests.Core.Parser
{
    public class ProfileParserTest
    {
        // These tests are run on the source from this page: http://playoverwatch.com/en-us/career/pc/moiph-1288
        // The source of the page was downloaded and stored on 02/07/2018
        // Will serve as the basis for parser tests barring a more reliable solution.

        private readonly Player _testPlayer;
        private readonly ITestOutputHelper _testOutputHelper;

        public ProfileParserTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            var source = File.ReadAllText("TestSource.txt");
            //var searchSource = File.ReadAllText("SearchTestSource.txt");
            var data = new ProfileClient.ProfileRequestData("", source, Platform.Pc);
            var parser = new ProfileParser();
            _testPlayer = new Player();
            parser.Parse(_testPlayer, data).GetAwaiter().GetResult();
        }

        [Fact]
        public void Parsed_Profile_PlayerLevel_Should_Be_Correct()
        {
            _testPlayer.PlayerLevel.Should().BeLessOrEqualTo(100);
            _testPlayer.PlayerLevel.Should().BeGreaterOrEqualTo(1);
        }

        [Fact]
        public void Parsed_Profile_EndorsementLevel_Should_Be_Correct()
        {
            _testPlayer.EndorsementLevel.Should().BeLessOrEqualTo(5);
            _testPlayer.EndorsementLevel.Should().BeGreaterOrEqualTo(1);
        }

        [Fact]
        public void Parsed_Profile_EndorsementStats_Should_Be_Correct()
        {
            // _testPlayer.Endorsements[Endorsement.GoodTeammate].Should().BeLessOrEqualTo(1);
            // _testPlayer.Endorsements[Endorsement.GoodTeammate].Should().BeGreaterOrEqualTo(0);
        }

        [Fact]
        public void Parsed_Profile_CompetitiveRank_Should_Be_Correct()
        {
            _testPlayer.CompetitiveRank.Should().BeLessOrEqualTo(4500);
            _testPlayer.CompetitiveRank.Should().BeGreaterOrEqualTo(2000);
        }

        [Fact]
        public void Parsed_Profile_CasualStats_Should_Be_Correct()
        {
            // string val = _testPlayer.CasualStats.CareerStats.GetStatExact("AllHeroes", "Assists", "HealingDone").Value;
            // val.Should().Be("994997");
        }
            

        [Fact]
        public void Parsed_Profile_CompetitiveStats_Should_Be_Correct()
        {
            // TimeSpan stat = _testPlayer.CompetitiveStats.TopHeroes.GetStatExact("Lucio", "Game", "TimePlayed")?.Value;
            // stat.Minutes.Should().Be(9);
            // stat.Seconds.Should().Be(40);
        }

        [Fact]
        public void Parsed_Profile_Achievements_Should_Be_Correct()
        {
            // _testPlayer.Achievements.FilterByName("Hog Wild").IsEarned.Should().BeFalse();
            // _testPlayer.Achievements.FilterByName("Level 10").IsEarned.Should().BeTrue();
        }

        // [Fact]
        // public void Parsed_Profile_PortraitImage_Should_Be_Correct() => 
        //     _testPlayer.ProfilePortraitUrl.Should().NotBeNullOrEmpty();

        // [Fact]
        // public void Parsed_Profile_CompetitiveRankImage_Should_Be_Correct() => 
        //     _testPlayer.CompetitiveRankImageUrl.Should().NotBeNullOrEmpty();

        // [Fact]
        // public void Parsed_Profile_PlayerLevelImage_Should_Be_Correct() => 
        //     _testPlayer.PlayerLevelImage.Should().NotBeNullOrEmpty();

        // [Fact]
        // public void Parsed_CasualStats_Should_Have_All_Heroes_In_CareerStats()
        // {
        //     foreach(var hero in Enum.GetNames(typeof(OverwatchHero)))
        //     {
        //         _testPlayer.CasualStats.CareerStats.GetStatExact(hero, "Game", "TimePlayed").Should().NotBeNull($"because {hero} should be in the stats.");

        //     }
            
        // }
    }
}

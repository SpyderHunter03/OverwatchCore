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
        public void Parsed_Profile_PlayerId_Should_Be_Correct()
        {
            _testPlayer.PlayerId.Should().NotBeNullOrEmpty();
            _testPlayer.PlayerId.Should().Be("53950316");
        }
    }
}

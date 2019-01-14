using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using OverwatchCore.Core.WebClient;
using OverwatchCore.Data;
using OverwatchCore.Enums;
using OverwatchCore.StaticResources;

[assembly: InternalsVisibleToAttribute("OverwatchCore.Tests")]
namespace OverwatchCore.Core.Parser
{
    internal sealed class ProfileParser
    {
        private readonly HtmlParser _parser = new HtmlParser();

        // todo: find a more concrete solution to this problem.
        private static bool IsPlayerProfilePrivate(IHtmlDocument pageData)
        {
            return pageData.QuerySelector(".masthead-permission-level-text")?.TextContent == "Private Profile";
        }

        internal async Task<Player> Parse(Player player, ProfileClient.ProfileRequestData pageData)
        {
            using (var doc = await _parser.ParseDocumentAsync(pageData.ReqContent))
            {
                player.ProfileUrl = pageData.ReqUrl;
                player.CompetitiveRank = CompetitiveRank(doc);
                player.CompetitiveRankImageUrl = CompetitiveRankImage(doc);
                player.PlayerLevel = PlayerLevel(doc);
                player.ProfilePortraitUrl = PortraitImage(doc);
                player.PlayerLevelImage = PlayerLevelImage(doc);
                player.Platform = pageData.PlayerPlatform;
                player.EndorsementLevel = EndorsementLevel(doc);
                player.Endorsements = Endorsements(doc);
                player.PlayerId = PlayerId(doc);
                if (IsPlayerProfilePrivate(doc))
                {
                    player.IsProfilePrivate = true;
                    return player;
                }
                player.CompetitiveStats = Stats(doc, Mode.Competitive);
                player.CasualStats = Stats(doc, Mode.Casual);
                player.Achievements = Achievements(doc);
                return player;
            }
        }

        private static readonly Regex PlayerLevelImageRegex = new Regex("(0x\\w*)(?=_)");
        private static readonly Regex PlayerIdRegex = new Regex("\\d+");

        private static string PlayerId(IHtmlDocument doc)
        {
            var lastScript = doc.QuerySelectorAll("script").Last().TextContent;
            return PlayerIdRegex.Match(lastScript).Value;
        }

        private static string PortraitImage(IHtmlDocument doc) => doc.QuerySelector(".player-portrait").GetAttribute("src");

        private static ushort CompetitiveRank(IHtmlDocument doc)
        {
            ushort.TryParse(doc.QuerySelector("div.competitive-rank div")?.TextContent, out var parsedCompetitiveRank);
            return parsedCompetitiveRank;
        }

        private static string CompetitiveRankImage(IHtmlDocument doc)
        {
            var compImg = doc.QuerySelector("div.competitive-rank img")?.OuterHtml;
            return !string.IsNullOrEmpty(compImg) ? compImg.Replace("<img src=\"", "").Replace("\">", "") : string.Empty;
        }

        private static ushort PlayerLevel(IHtmlDocument doc)
        {
            ushort.TryParse(doc.QuerySelector("div.player-level div")?.TextContent, out var parsedPlayerLevel);
            var playerLevelImageId = PlayerLevelImageRegex.Match(doc.QuerySelector("div.player-level").GetAttribute("style")).Value;
            if (!string.IsNullOrEmpty(playerLevelImageId))
                parsedPlayerLevel += Prestige.Definitions[playerLevelImageId];
            return parsedPlayerLevel;
        }

        private static string PlayerLevelImage(IHtmlDocument doc)
        {
            var str = doc.QuerySelector("div.player-level").GetAttribute("style");
            var startIndex = str.IndexOf('(') + 1;
            return str.Substring(startIndex, str.IndexOf(')') - startIndex);
        }

        private static ushort EndorsementLevel(IHtmlDocument doc)
        {
            ushort.TryParse(doc.QuerySelector("div.endorsement-level div.u-center")?.TextContent, out ushort parsedEndorsementLevel);
            return parsedEndorsementLevel;
        }

        private static List<Achievement> Achievements(IHtmlDocument doc)
        {
            var contents = new List<Achievement>();
            var innerContent = doc.QuerySelector("section[id='achievements-section']");
            foreach (var dropdownitem in innerContent.QuerySelectorAll("select > option"))
            {
                var achievementBlock = innerContent.QuerySelector($"div[data-category-id='{dropdownitem.GetAttribute("value")}']");
                var categoryName = dropdownitem.GetAttribute("option-id");
                foreach (var achievement in achievementBlock.QuerySelectorAll("div.achievement-card"))
                {
                    contents.Add(new Achievement
                    {
                        CategoryName = categoryName,
                        Name = achievement.QuerySelector("div.media-card-title").TextContent,
                        IsEarned = !achievement.GetAttribute("class").Contains("m-disabled")
                    });
                }
            }

            return contents;
        }

        private static Dictionary<Endorsement, decimal> Endorsements(IHtmlDocument doc)
        {
            var contents = new Dictionary<Endorsement, decimal>();

            var innerContent = doc.QuerySelector("div.endorsement-level");

            if (innerContent != null)
            {
                foreach (var endorsement in innerContent.QuerySelectorAll("svg"))
                {
                    var dataValue = endorsement.GetAttribute("data-value");

                    if (dataValue != null)
                    {
                        var className = endorsement.GetAttribute("class");
                        // parse the endorsement type out of the class name
                        const string endorsementTypeSeparator = "--";
                        var endorsementName = className.Substring(className.IndexOf(endorsementTypeSeparator, StringComparison.Ordinal) + endorsementTypeSeparator.Length);
                        contents.Add(ParseEndorsementName(endorsementName), decimal.Parse(dataValue));
                    }
                }
            }
            return contents;
        }

        private static List<Platform> Platforms(IHtmlDocument doc)
        {
            var platformDiv = doc.QuerySelector("#profile-platforms");
            if (platformDiv == null) return null;
            var platforms = new List<Platform>();
            var html = platformDiv.ToHtml();
            foreach (var platform in platformDiv.QuerySelectorAll("a[href]"))
            {
                var platformString = platform.TextContent;
                if(string.Equals(platformString, Platform.Pc.ToString(), StringComparison.OrdinalIgnoreCase))
                    platforms.Add(Platform.Pc);
                if (string.Equals(platformString, Platform.Psn.ToString(), StringComparison.OrdinalIgnoreCase))
                    platforms.Add(Platform.Psn);
                if (string.Equals(platformString, Platform.Xbl.ToString(), StringComparison.OrdinalIgnoreCase))
                    platforms.Add(Platform.Xbl);
            }

            return platforms;
        }

        private static List<Stat> Stats(IHtmlDocument doc, Mode mode)
        {
            var contents = new List<Stat>();
            var divModeId = string.Empty;
            switch (mode)
            {
                case Mode.Casual:
                    divModeId = "quickplay";
                    break;
                case Mode.Competitive:
                    divModeId = "competitive";
                    break;
            }
            var innerContent = doc.QuerySelector($"div[id='{divModeId}']");
            var idDictionary = new Dictionary<string, string>();
            foreach (var dropdownitem in innerContent.QuerySelectorAll("select > option"))
            {
                var id = dropdownitem.GetAttribute("value");
                if (id.StartsWith("0x0"))
                {
                    idDictionary.Add(id, ParseHeroName(dropdownitem.TextContent));
                }
            }
            foreach (var section in innerContent.QuerySelectorAll("div[data-group-id='stats']"))
            {
                var catId = section.GetAttribute("data-category-id");
                var heroName = idDictionary[catId];
                foreach (var table in section.QuerySelectorAll($"div[data-category-id='{catId}'] table.data-table"))
                {
                    var catName = table.QuerySelector("thead").TextContent;
                    foreach (var row in table.QuerySelectorAll("tbody tr"))
                    {
                        contents.Add(new Stat
                        {
                            CategoryName = catName,
                            HeroName = heroName,
                            Name = row.Children[0].TextContent,
                            Value = OwValToDouble(row.Children[1].TextContent)
                        });
                    }
                }
            }

            return contents;
        }

        private static double OwValToDouble(string input)
        {
            if (input.ToLower().Contains("hour"))
                return TimeSpan.FromHours(int.Parse(input.Substring(0, input.IndexOf(" ", StringComparison.Ordinal)))).TotalSeconds;
            if (input.ToLower().Contains("minute"))
                return TimeSpan.FromMinutes(int.Parse(input.Substring(0, input.IndexOf(" ", StringComparison.Ordinal)))).TotalSeconds;
            if (!input.Contains(":"))
                return double.TryParse(input.Replace(",", "").Replace("%", ""), out var rslt1) ? rslt1 : 0;
            if (TimeSpan.TryParseExact(input, @"mm\:ss", CultureInfo.CurrentCulture, out var outputTime))
                return outputTime.TotalSeconds;
            if (TimeSpan.TryParseExact(input, @"hh\:mm\:ss", CultureInfo.CurrentCulture, out var outputTime1))
                return outputTime1.TotalSeconds;
            return double.TryParse(input.Replace(",", "").Replace("%", ""), out var rslt2) ? rslt2 : 0;
        }

        private static string ParseHeroName(string input)
        {
            // todo: This is disgusting I don't know how this line has survived so long.
            return input.ToLower() == "all heroes" ? "AllHeroes" : input.Replace("ú", "u").Replace(":", "").Replace(" ", "").Replace("ö", "o").Replace(".", "");
        }

        private static Endorsement ParseEndorsementName(string input)
        {
            switch (input)
            {
                case "teammate":
                    return Endorsement.GoodTeammate;
                case "sportsmanship":
                    return Endorsement.Sportsmanship;
                case "shotcaller":
                    return Endorsement.Shotcaller;
                default:
                    return Endorsement.GoodTeammate;
            }
        }
    }
}

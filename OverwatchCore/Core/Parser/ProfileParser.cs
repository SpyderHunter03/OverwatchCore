using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Newtonsoft.Json;
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

        internal async Task<Player> Parse(Player player, ProfileClient.ProfileRequestData pageData)
        {
            using (var doc = await _parser.ParseDocumentAsync(pageData.ReqContent))
            {
                //Checks if profile not found, site still returns 200 in this case
                if (doc.QuerySelector("h1.u-align-center")?.FirstChild?.TextContent.Equals("Profile Not Found") ?? false)
                    return null;

                //Scrapes all stats for the passed user and sets member data
                player = ParseGeneralInfo(player, doc);
                player.ProfileUrl = pageData.ReqUrl;
                
                //Get user id from script at page
                player.PlayerId = PlayerId(doc);
                player.Platform = pageData.PlayerPlatform;

                var profile = await ParseProfile(player);
                if (profile == null)
                    return player;
                player.Username = profile.Name;
                player.Prestige = (ushort)(profile.PlayerLevel / 100);

                if (IsPlayerProfilePrivate(doc))
                {
                    player.IsProfilePrivate = true;
                    return player;
                }

                player.Endorsements = Endorsements(doc);
                //player.CompetitiveStats = Stats(doc, Mode.Competitive);
                player.CasualStats = ParseDetailedStats(doc, Mode.Casual);
                player.CompetitiveStats = ParseDetailedStats(doc, Mode.Competitive);
                player.Achievements = Achievements(doc);
                return player;
            }
        }

        internal Player ParseGeneralInfo(Player player, IHtmlDocument doc)
        {
            player.ProfilePortraitUrl = PortraitImage(doc);
            player.PlayerLevel = PlayerLevel(doc);
            player.PlayerLevelImage = PlayerLevelImage(doc);
            player.PrestigeImage = PlayerPrestigeImage(doc);
            player.EndorsementLevel = EndorsementLevel(doc);
            player.EndorsementImage = EndorsementImage(doc);
            player.CompetitiveRank = CompetitiveRank(doc);
            player.CompetitiveRankImageUrl = CompetitiveRankImage(doc);
            player.GamesWon = GamesWon(doc);
            return player;
        }

        internal async Task<Profile> ParseProfile(Player player)
        {
            using (var httpClient = new HttpProfileClient())
            {
                var profileInformation = await httpClient.GetProfileApiInformation(player.PlayerId);
                if (!profileInformation.HasValue) return null;

                var profiles = JsonConvert.DeserializeObject<List<Profile>>(profileInformation.Value.Content);
                return profiles.FirstOrDefault(p => p.Platform.Equals(player.Platform.ToString(), StringComparison.InvariantCultureIgnoreCase));
            }
        }

        private static readonly Regex PlayerLevelImageRegex = new Regex("(0x\\w*)(?=_)");
        private static readonly Regex PlayerIdRegex = new Regex("window\\.app\\.career\\.init\\((\\d+)\\,");

        private static bool IsPlayerProfilePrivate(IHtmlDocument pageData)
        {
            return pageData.QuerySelector("p.masthead-permission-level-text")?.FirstChild?.TextContent == "Private Profile";
        }

        private static string PlayerId(IHtmlDocument doc)
        {
            var lastScript = doc.QuerySelectorAll("script").Last().TextContent;
            var playerIdRegex = PlayerIdRegex.Match(lastScript).Value;
            var playerId = playerIdRegex.Substring(playerIdRegex.IndexOf('(') + 1);
            return playerId.Substring(0, playerId.Length-1);
        }

        private static string PortraitImage(IHtmlDocument doc) => 
            doc.QuerySelector("img.player-portrait").GetAttribute("src");

        private static ushort CompetitiveRank(IHtmlDocument doc)
        {
            ushort.TryParse(doc.QuerySelector("div.competitive-rank div.u-align-center")?.FirstChild?.TextContent, out var parsedCompetitiveRank);
            return parsedCompetitiveRank;
        }

        private static string CompetitiveRankImage(IHtmlDocument doc)
        {
            var compImg = doc.QuerySelector("div.competitive-rank img")?.GetAttribute("src");
            return compImg ?? "";
        }

        private static ushort PlayerLevel(IHtmlDocument doc)
        {
            ushort.TryParse(doc.QuerySelector("div.player-level div.u-vertical-center")?.FirstChild?.TextContent, out var parsedPlayerLevel);
            return parsedPlayerLevel;
        }

        private static string PlayerLevelImage(IHtmlDocument doc)
        {
            var str = doc.QuerySelector("div.player-level").GetAttribute("style");
            var startIndex = str.IndexOf('(') + 1;
            return str.Substring(startIndex, str.IndexOf(')') - startIndex);
        }

        private static string PlayerPrestigeImage(IHtmlDocument doc)
        {
            var str = doc.QuerySelector("div.player-rank")?.GetAttribute("style");
            if (str == null) return "";
            var startIndex = str.IndexOf('(') + 1;
            return str.Substring(startIndex, str.IndexOf(')') - startIndex);
        }

        private static ushort EndorsementLevel(IHtmlDocument doc)
        {
            ushort.TryParse(doc.QuerySelector("div.endorsement-level div.u-center")?.FirstChild?.TextContent, out ushort parsedEndorsementLevel);
            return parsedEndorsementLevel;
        }

        private static string EndorsementImage(IHtmlDocument doc)
        {
            var str = doc.QuerySelector("div.EndorsementIcon").GetAttribute("style");
            var startIndex = str.IndexOf('(') + 1;
            return str.Substring(startIndex, str.IndexOf(')') - startIndex);

        }

        private static ushort GamesWon(IHtmlDocument doc)
        {
            var str = doc.QuerySelector("div.masthead p.masthead-detail.h4 span")?.TextContent?.Replace(" games won", "");
            if (str == null) return 0;
            ushort.TryParse(str, out ushort parsedgameswon);
            return parsedgameswon;
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

        private static Stat ParseDetailedStats(IHtmlDocument doc, Mode mode)
        {
            var stat = new Stat();
            stat.TopHeroes = ParseHeroStats(doc, mode);
            stat.CareerStats = ParseCareerStats(doc, mode);
            return stat;
        }

        private static List<StatValue> ParseHeroStats(IHtmlDocument doc, Mode mode)
        {
            var contents = new List<StatValue>();
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
            var innerContent = doc.QuerySelector($"div#{divModeId}");
            var idDictionary = new Dictionary<string, List<Stat>>();
            foreach (var item in innerContent.QuerySelectorAll("div.progress-category"))
            {
                var categoryId = item.GetAttribute("data-category-id");
                categoryId = categoryId.Replace("0x0860000000000", string.Empty);
                foreach (var innerItem in item.QuerySelectorAll("div.ProgressBar"))
                {
                    var heroName = innerItem.QuerySelector("div.ProgressBar-title")?.FirstChild?.TextContent;
                    var stat = innerItem.QuerySelector("div.ProgressBar-description")?.FirstChild?.TextContent;
                    heroName = ClearSpecialCharacters(heroName);
                    
                    switch(categoryId)
                    {
                        case "021":
                            // Time played in seconds
                            var timePlayedInSeconds = 0;
                            var time = stat.Split(":");
                            var hour = 0;
                            var minute = 0;
                            var second = 0;
                            if (time.Length == 3 && int.TryParse(time[0], out hour))
                            {
                                timePlayedInSeconds += hour * 60 * 60;
                                time = time.Skip(1).ToArray();
                            }
                            if (time.Length == 2 && int.TryParse(time[0], out minute))
                            {
                                timePlayedInSeconds += minute * 60;
                                time = time.Skip(1).ToArray();
                            }
                            if (time.Length == 1 && int.TryParse(time[0], out second))
                            {
                                timePlayedInSeconds += second;
                            }
                            contents.Add(new StatValue { HeroName = heroName, CategoryName = "Game", Name = "TimePlayedInSeconds", Value = timePlayedInSeconds});
                            contents.Add(new StatValue { HeroName = heroName, CategoryName = "Game", Name = "TimePlayed", Value = new TimeSpan(hour, minute, second)});
                            break;
                        case "039":
                            contents.Add(new StatValue { HeroName = heroName, CategoryName = "Game", Name = "GamesWon", Value = int.Parse(stat)});
                            break;
                        case "3D1":
                            contents.Add(new StatValue { HeroName = heroName, CategoryName = "Game", Name = "WinPercentage", Value = int.Parse(stat.Replace("%", ""))});
                            break;
                        case "02F":
                            contents.Add(new StatValue { HeroName = heroName, CategoryName = "Game", Name = "WeaponAccuracy", Value = int.Parse(stat.Replace("%", ""))});
                            break;
                        case "3D2":
                            contents.Add(new StatValue { HeroName = heroName, CategoryName = "Game", Name = "EliminationsPerLife", Value = float.Parse(stat)});
                            break;
                        case "346":
                            contents.Add(new StatValue { HeroName = heroName, CategoryName = "Game", Name = "MultiKillBest", Value = int.TryParse(stat, out var multiKillBest) ? multiKillBest : 0});
                            break;
                        case "39C":
                            contents.Add(new StatValue { HeroName = heroName, CategoryName = "Game", Name = "ObjectiveKills", Value = float.TryParse(stat, out var objectiveKills) ? objectiveKills : 0});
                            break;
                    }
                }
            }

            return contents;
        }

        private static List<StatValue> ParseCareerStats(IHtmlDocument doc, Mode mode)
        {
            var csMap = new List<StatValue>();
            var heroMap = new Dictionary<string, string>();

            // Populates tempHeroMap to match hero ID to name in second scrape
            foreach (var heroSel in doc.QuerySelectorAll("select option"))
            {
                var heroVal = heroSel.GetAttribute("value");
                heroMap[heroVal] = heroSel.TextContent;
            }

            // Iterates over every hero div
            foreach (var heroStatsSel in doc.QuerySelectorAll("div.row div.js-stats"))
            {
                var currentHero = heroStatsSel.GetAttribute("data-category-id");
                currentHero = CleanJSONKey(heroMap[currentHero]);
                currentHero = ClearSpecialCharacters(currentHero);

                //Iterates over every stat box
                foreach(var statBoxSel in heroStatsSel.QuerySelectorAll("div.column.xs-12"))
                {
                    var statType = CleanJSONKey(statBoxSel.QuerySelector(".stat-title").TextContent);
                    
                    // Iterates over stat row
                    foreach(var statSel in statBoxSel.QuerySelectorAll("table.DataTable tbody tr"))
                    {
                        // Iterates over every stat td
                        var statKey = "";
                        var statVal = "";
                        foreach(var statKV in statSel.QuerySelectorAll("td").Select((value, index) => new { value, index}))
                        {
                            switch(statKV.index)
                            {
                                case 0:
                                    //statKey = TransformKey(CleanJSONKey(statKV.value.TextContent)); //IDK what transformkey does... Look into
                                    statKey = CleanJSONKey(statKV.value.TextContent);
                                    break;
                                case 1:
                                    statVal = statKV.value.TextContent.Replace(",", "");
                                    csMap.Add(new StatValue { HeroName = currentHero, CategoryName = statType, Name = statKey, Value = statVal});
                                    break;
                            }
                        }
                    }
                }
            }

            return csMap;
        }

        // cleanJSONKey
        private static string CleanJSONKey(string str) {
            // Removes localization rubish
            if (str.Contains("} other {"))
            {
                var re = new Regex("{count, plural, one {.+} other {(.+)}}");
                if (re.Matches(str).Count == 2)
                {
                    var otherForm = re.Matches(str)[1];
                    str = re.Replace(str, otherForm.Value);
                }
            }

            str = str.Replace("-", "").Replace(".", "").Replace(":", ""); // Removes all dashes, dots, and colons from titles
            str = str.ToLower();
            str = new CultureInfo("en-US", false).TextInfo.ToTitleCase(str); // Uppercases lowercase leading characters
            str = str.Replace(" ", ""); // Removes Spaces
            return str;
        }

        private static string ClearSpecialCharacters(string str)
        {
            return str.Replace("ú","u");
            // byte[] tempBytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(str);
            // string asciiStr = System.Text.Encoding.UTF8.GetString(tempBytes);
            // return asciiStr;
            // return System.Web.HttpUtility.UrlDecode(
            //     System.Web.HttpUtility.UrlEncode(
            //     str, Encoding.GetEncoding("iso-8859-7")));
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

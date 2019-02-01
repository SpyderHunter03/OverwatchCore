using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using OverwatchCore.Core.WebClient;
using OverwatchCore.Data;
using OverwatchCore.Enums;
using OverwatchCore.Extensions;
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

                //Get user id from script at page
                player.PlayerId = PlayerId(doc);

                return player.Validate();
            }
        }

        private static readonly Regex PlayerIdRegex = new Regex("window\\.app\\.career\\.init\\((\\d+)\\,");

        private static string PlayerId(IHtmlDocument doc)
        {
            var lastScript = doc.QuerySelectorAll("script")?.Last()?.TextContent;
            if (lastScript == null) return null;
            
            var playerIdRegex = PlayerIdRegex.Match(lastScript).Value;
            var playerId = playerIdRegex.Substring(playerIdRegex.IndexOf('(') + 1);
            if (playerId == null || playerId.Length == 0) return null;
            return playerId.Substring(0, playerId.Length-1);
        }
    }
}

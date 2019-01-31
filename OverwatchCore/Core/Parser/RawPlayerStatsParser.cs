using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OverwatchCore.Data;
using OverwatchCore.Enums;

namespace OverwatchCore.Core.Parser
{
    internal static class RawPlayerStatsParser
    {
        internal static Player Parse(this RawPlayerStats stats, Platform platform, string profileUrl)
        {
            Player player = new Player();
            player.Username = stats.Name;
            player.Platform = platform;
            player.ProfileUrl = new Uri(profileUrl);
            player.PlayerLevel = stats.Level;
            player.PlayerLevelImage = stats.LevelIcon;
            player.Prestige = stats.Prestige;
            player.PrestigeImage = stats.PrestigeIcon;
            player.CompetitiveRank = stats.Rating;
            player.CompetitiveRankImageUrl = stats.RatingIcon;
            player.EndorsementLevel = stats.Endorsement;
            player.EndorsementImage = stats.EndorsementIcon;
            player.IsProfilePrivate = stats.Private;
            player.GamesWon = stats.GamesWon;
            player.ProfilePortraitUrl = stats.Icon;

            player.CasualStats = ParseCasualStats(stats);
            player.CompetitiveStats = ParseCompetitiveStats(stats);

            return player;
        }

        internal static Stat ParseCasualStats(RawPlayerStats stats) => ParseStat(stats.QuickPlayStats);

        internal static Stat ParseCompetitiveStats(RawPlayerStats stats) => ParseStat(stats.CompetitiveStats);

        internal static Stat ParseStat(RawStats rawStats) => 
            new Stat
            {
                TopHeroes = GetTopHeroStatValues(rawStats),
                CareerStats = GetCareerStatsStatValues(rawStats)
            };

        private static ICollection<StatValue> GetTopHeroStatValues(RawStats rawStats) =>
            rawStats.TopHeroes.ToList().SelectMany(kvp => typeof(TopHero)
                .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Select(prop => new StatValue 
                    {
                        HeroName = kvp.Key,
                        Name = prop.Name,
                        Value = prop.GetValue(kvp.Value)
                    })).ToList();

        private static ICollection<StatValue> GetCareerStatsStatValues(RawStats rawStats) =>
            rawStats.CareerStats.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Select(hp => new { HeroName = hp.Name, Hero = hp.GetValue(rawStats.CareerStats)})
                    .Where(hp => hp.Hero != null)
                    .SelectMany(h => h.Hero.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        .Select(hsp => new { CategoryName = hsp.Name, CategoryStats = hsp.GetValue(h.Hero) })
                        .Where(hsp => hsp.CategoryStats != null)
                        .SelectMany(a => a.CategoryStats.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).ToList()
                            .Select(csp => new StatValue 
                            { 
                                HeroName = h.HeroName,
                                CategoryName = a.CategoryName,
                                Name = csp.Name,
                                Value = csp.GetValue(a.CategoryStats)
                            }))).ToList();
    }
}

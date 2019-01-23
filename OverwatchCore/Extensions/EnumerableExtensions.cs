using System;
using System.Collections.Generic;
using System.Linq;
using OverwatchCore.Data;

namespace OverwatchCore.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<StatValue> FilterByHero(this IEnumerable<StatValue> stats, string heroName)
        {
            heroName = heroName.Contains(" ") ? heroName.Replace(" ", string.Empty) : heroName;
            return stats.Where(x => x.HeroName.EqualsIgnoreCase(heroName));
        }

        public static IEnumerable<StatValue> FilterByCategory(this IEnumerable<StatValue> stats, string categoryName) =>
            stats.Where(x => x.CategoryName.EqualsIgnoreCase(categoryName));

        public static IEnumerable<StatValue> FilterByName(this IEnumerable<StatValue> stats, string statName) =>
            stats.Where(x => x.Name.EqualsIgnoreCase(statName));

        public static StatValue GetStatExact(this List<StatValue> statValues, string heroName, string categoryName, string statName) =>
            
            statValues.FilterByHero(heroName)
                .FilterByCategory(categoryName)
                .FilterByName(statName)
                .FirstOrDefault();

        public static IEnumerable<Achievement> FilterByCategory(this IEnumerable<Achievement> achievements, string categoryName) =>
            achievements.Where(x => x.CategoryName.EqualsIgnoreCase(categoryName));

        public static Achievement FilterByName(this IEnumerable<Achievement> achievements, string achievementName) =>
            achievements.FirstOrDefault(x => x.Name.EqualsIgnoreCase(achievementName));
    
    }
}

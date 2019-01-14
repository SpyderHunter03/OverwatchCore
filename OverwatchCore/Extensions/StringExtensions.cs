using System;
using System.Linq;
using System.Text.RegularExpressions;
using OverwatchCore.Enums;

namespace OverwatchCore.Extensions
{
    public static class StringExtensions
    {
        private static readonly Regex BattletagRegex = new Regex(@"\w+#\d+");
        private static readonly Regex PsnIdRegex = new Regex(@"^[a-zA-Z]{1}[\w\d-]{2,12}$");
        private static readonly Regex XblIdRegex = new Regex(@"^[a-zA-Z0-9\s]{1,15}$");

        internal static bool IsValidBattletag(this string battletag) => BattletagRegex.IsMatch(battletag);

        internal static bool IsValidXblId(this string xblId) => XblIdRegex.IsMatch(xblId);

        internal static bool IsValidPsnId(this string psnId) => PsnIdRegex.IsMatch(psnId);

        internal static string ToProfileUrl(this string username, Platform platform) => 
            $"https://playoverwatch.com/en-gb/career/{platform}/{username.BattletagToUrlFriendlyString()}";

        internal static string BattletagToUrlFriendlyString(this string battletag) => battletag.Replace('#','-');

        internal static bool EqualsIgnoreCase(this string source, string toCompare) =>
            string.Equals(source, toCompare, StringComparison.OrdinalIgnoreCase);

        internal static Platform PlatformStringToEnum(this string platformString)
        {
            return Enum.GetValues(typeof(Platform))
                .Cast<Platform>()
                .FirstOrDefault(platform => string.Equals(platformString.ToLower(), platform.ToLowerString()));
        }
    }
}

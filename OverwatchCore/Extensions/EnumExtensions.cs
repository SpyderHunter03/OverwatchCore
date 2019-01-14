using System;
using OverwatchCore.Enums;

namespace OverwatchCore.Extensions
{
    public static class EnumExtensions
    {
        public static string ToLowerString(this Platform platform) => platform.ToString().ToLower();
    }
}

using System;
using System.Collections.Generic;
using OverwatchCore.Data;

namespace OverwatchCore.StaticResources
{
    public static class Validator
    {
        public static bool Validate(string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        public static bool Validate<T>(Nullable<T> nu) where T : struct
        {
            return nu.HasValue;
        }

        public static bool Validate<T>(ICollection<T> enu)
        {
            return enu != null && enu.Count > 0;
        }

        public static bool Validate<T,U>(Dictionary<T, U> dict)
        {
            return dict != null && dict.Count > 0;
        }

        public static bool Validate(Stat stat)
        {
            return stat != null && Validate(stat.TopHeroes) && Validate(stat.CareerStats);
        }
    }
}

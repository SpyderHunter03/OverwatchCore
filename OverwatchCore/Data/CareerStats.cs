using System;
using System.Collections.Generic;

namespace OverwatchCore.Data
{
    public class CareerStats
    {
        public Dictionary<string, string> Assists { get; set; }
        public Dictionary<string, string> Average { get; set; }
        public Dictionary<string, string> Best { get; set; }
        public Dictionary<string, string> Combat { get; set; }
        public Dictionary<string, string> Deaths { get; set; }
        public Dictionary<string, string> HeroSpecific { get; set; }
        public Dictionary<string, string> Game { get; set; }
        public Dictionary<string, string> MatchAwards { get; set; }
        public Dictionary<string, string> Miscellaneous { get; set; }
    }
}

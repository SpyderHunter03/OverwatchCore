using System;
using System.Collections.Generic;

namespace OverwatchCore.Data
{
    public class Stat
    {
        public ICollection<StatValue> TopHeroes { get; set; }

        public ICollection<StatValue> CareerStats { get; set; }
    }
}

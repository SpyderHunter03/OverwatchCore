using System;
using System.Collections.Generic;
using OverwatchCore.Enums;
using OverwatchCore.Extensions;

namespace OverwatchCore.Data
{
    public sealed class Player
    {
        public string Username { get; set; }
        internal string UsernameUrlFriendly => Username.BattletagToUrlFriendlyString();
        public Platform Platform { get; set; }
        public Uri ProfileUrl { get; set; }
        public long? PlayerLevel { get; set; }
        public Uri PlayerLevelImage { get; set; }
        public long? Prestige { get; set; }
        public Uri PrestigeImage { get; set; }
        public long? CompetitiveRank { get; set; }
        public long? EndorsementLevel { get; set; }
        public Uri EndorsementImage { get; set; }
        public bool? IsProfilePrivate { get; set; }
        public long? GamesWon { get; set; }
        public Stat CasualStats { get; set; }
        public Stat CompetitiveStats { get; set; }
        public Uri CompetitiveRankImageUrl { get; set; }
        public Uri ProfilePortraitUrl { get; set; }

        //public List<Alias> Aliases { get; set; }
        //public string PlayerId { get; set; }
        //public List<Achievement> Achievements { get; set; }
        //public Dictionary<Endorsement, decimal> Endorsements { get; set; }
        // public sealed class Alias
        // {
        //     public Platform Platform { get; set; }
        //     public string Username { get; set; }
        //     internal string UrlName;
        //     public Visibility ProfileVisibility { get; set; }
        // }
    }
}

using System;

namespace OverwatchCore.Data
{
    public class Alias
    {
        public string platform { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string urlName { get; set; }
        public Visibility visibility { get; set; }

        public sealed class Visibility
        {
            public string name { get; set; }
            public bool isPublic { get; set; }
            public bool isPrivate { get; set; }
            public bool isFriendsOnly { get; set; }
        }
    }
}

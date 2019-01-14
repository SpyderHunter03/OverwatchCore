using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using OverwatchCore.Data;
using OverwatchCore.Enums;

[assembly: InternalsVisibleToAttribute("OverwatchCore.Tests")]
namespace OverwatchCore.Core.WebClient
{
    public abstract class ProfileClient : IDisposable
    {
        public virtual void Dispose() { }

        internal Task<ProfileRequestData> GetProfileExact(Player player) =>
            GetProfileExact(player.UsernameUrlFriendly, player.Platform);

        internal abstract Task<ProfileRequestData> GetProfileExact(string username, Platform platform);

        internal Task<ProfileRequestData> GetProfileDetectPlatform(Player player) =>
            GetProfileDetectPlatform(player.UsernameUrlFriendly);
        
        internal abstract Task<ProfileRequestData> GetProfileDetectPlatform(string username);

        internal abstract Task<List<Alias>> GetAliases(string id);

        public sealed class ProfileRequestData
        {
            internal string ReqUrl;
            internal string ReqContent;
            internal Platform PlayerPlatform;
            internal string SecReqUrl;
            internal string ProfilesJson;

            public ProfileRequestData(string reqUrl, string reqContent, Platform playerPlatform, string secReqUrl, string profilesJson)
            {
                ReqUrl = reqUrl;
                ReqContent = reqContent;
                PlayerPlatform = playerPlatform;
                SecReqUrl = secReqUrl;
                ProfilesJson = profilesJson;
            }
        }

        internal sealed class Visibility
        {
            public string name { get; set; }
            public bool isPublic { get; set; }
            public bool isPrivate { get; set; }
            public bool isFriendsOnly { get; set; }
        }

        internal sealed class Alias
        {
            public string platform { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public string urlName { get; set; }
            public Visibility visibility { get; set; }
        }

        internal sealed class Profile
        {
            public string platform { get; set; }
            public string name { get; set; }
            public string urlName { get; set; }
            public int level { get; set; }
            public string portrait { get; set; }
            public bool isPublic { get; set; }
        }
    }
}

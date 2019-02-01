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

        internal abstract Task<ICollection<Alias>> GetAliases(string id);

        public sealed class ProfileRequestData
        {
            internal string ReqUrl;
            internal string ReqContent;
            internal Platform PlayerPlatform;

            public ProfileRequestData(string reqUrl, string reqContent)
            {
                ReqUrl = reqUrl;
                ReqContent = reqContent;
            }

            public ProfileRequestData(string reqUrl, string reqContent, Platform playerPlatform)
            {
                ReqUrl = reqUrl;
                ReqContent = reqContent;
                PlayerPlatform = playerPlatform;
            }
        }
    }
}

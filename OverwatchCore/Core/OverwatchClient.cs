using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OverwatchCore.Core.Parser;
using OverwatchCore.Core.WebClient;
using OverwatchCore.Data;
using OverwatchCore.Enums;
using OverwatchCore.Extensions;

namespace OverwatchCore.Core
{
    /// <summary>
    /// The client for making requests and receiving player data, if you plan to make multiple requests it is recommended to use only one instance.
    /// Ensure that you dispose of this object when complete.
    /// <see cref="IDisposable"/>
    /// </summary>
    public sealed class OverwatchClient : IDisposable
    {
        public IReadOnlyList<Platform> DetectedPlatforms { get; private set; }

        private readonly ProfileClient _profileClient;
        private readonly ProfileParser _profileParser;

        /// <summary>
        /// Create a new instance of the OverwatchClient.
        /// </summary>
        /// <param name="platforms">If you only wish to detect certain platforms, provide them here - otherwise all platforms will be used.</param>
        public OverwatchClient(params Platform[] platforms)
        {
            _profileParser = new ProfileParser();
            _profileClient = new HttpProfileClient();
            if (platforms == null || platforms.Length == 0)
                DetectedPlatforms = Enum.GetValues(typeof(Platform)).Cast<Platform>().ToList();
            else
                DetectedPlatforms = platforms.Distinct().ToList();
        }

        internal OverwatchClient(ProfileClient profileClient, params Platform[] platforms)
        {
            _profileClient = profileClient;
            _profileParser = new ProfileParser();
            if (platforms == null || platforms.Length == 0)
                DetectedPlatforms = Enum.GetValues(typeof(Platform)).Cast<Platform>().ToList();
            else
                DetectedPlatforms = platforms.Distinct().ToList();
        }

        /// <summary>
        /// Uses both platform detection to find a player. Not as accurate or fast as providing a platform.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>A <see cref="Player"/> if it was succesfully found, otherwise returns null.</returns>
        public async Task<Player> GetPlayerAsync(string username)
        {
            if (username.IsValidBattletag() && DetectedPlatforms.Contains(Platform.Pc))
                return await GetPlayerAsync(username, Platform.Pc);
            if (!username.IsValidPsnId() && !username.IsValidXblId())
                throw new ArgumentException("Not a valid XBL, PSN or Battlenet ID", nameof(username));
            var player = new Player { Username = username };

            Player retval;
            var retry = 0;

            do
            {
                var result = await _profileClient.GetProfileDetectPlatform(player);

                if (result == null)
                    return null;
                    
                retval = await _profileParser.Parse(player, result);

                retry++;
            } while (retval == null && retry <= 5);

            return retval;
        }

        /// <summary>
        /// The fastest and most precise method of finding a player.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="platform"></param>
        /// <param name="region"></param>
        /// <returns>A <see cref="Player"/> object if it was succesfully found, otherwise returns null.</returns>
        public async Task<Player> GetPlayerAsync(string username, Platform platform)
        {
            if (platform == Platform.Pc && !username.IsValidBattletag())
                throw new ArgumentException($"{username} is not a valid BattleTag - valid example: Example#1234", nameof(username));
            if (platform == Platform.Psn && !username.IsValidPsnId())
                throw new ArgumentException($"{username} is not a valid PSN ID.", nameof(username));
            if (platform == Platform.Xbl && !username.IsValidXblId())
                throw new ArgumentException($"{username} is not a valid XBL ID.", nameof(username));

            var player = new Player()
            {
                Username = username,
                Platform = platform
            };

            ProfileClient.ProfileRequestData pageData;
            Player retval;
            var retry = 0;

            do
            {
                if (platform != Platform.Pc)
                    pageData = await _profileClient.GetProfileExact(player);
                else
                    pageData = await _profileClient.GetProfileExact(player);

                if (pageData == null)
                    return null;
            
                retval = await _profileParser.Parse(player, pageData);

                retry++;
            } while (retval == null && retry <= 5);

            return retval;
        }

        /// <summary>
        /// Updates a Players stats using a pre-existing Player object as the basis for the request.
        /// </summary>
        /// <param name="player">An existing "Player" object</param>
        /// <returns>A "Player" object if it was succesfully found, otherwise returns null.</returns>
        public async Task<Player> UpdatePlayerAsync(Player player)
        {
            if (string.IsNullOrEmpty(player.Username))
                throw new ArgumentException("Player Username is Null or Empty",nameof(player));
            if (player.Username.IsValidBattletag() && player.Platform != Platform.Pc)
                throw new ArgumentException("Invalid Username for Platform", nameof(player));
            if (!player.Username.IsValidBattletag() && player.Platform == Platform.Pc)
                throw new ArgumentException("Invalid Username for PC", nameof(player));
            
            Player retval;
            var retry = 0;

            do
            {
                var pageData = await _profileClient.GetProfileExact(player);

                if (pageData == null)
                    return null;

                retval = await _profileParser.Parse(player, pageData);

                retry++;
            } while (retval == null && retry <= 5);
            
            return retval;
        }

        /// <summary>
        /// Get the information about other profiles connected to this player
        /// The player MUST have been loaded first with <see cref="GetPlayerAsync(string)"/> or <seealso cref="GetPlayerAsync(string,Platform)"/>
        /// The player object will be updated, the <see cref="Player.OtherKnownProfiles"/> property of the player will be populated with data.
        /// </summary>
        /// <param name="player">A populated player profile.</param>
        public async Task GetAliasesAsync(Player player)
        {
            // if(player == null)
            //     throw new ArgumentNullException(nameof(player));
            // if(string.IsNullOrWhiteSpace(player.ProfileUrl))
            //     throw new ArgumentException("Player has not had their profile loaded", nameof(player));
            
            // player.Aliases = new List<Player.Alias>();
            // var rslt = await _profileClient.GetAliases(player.PlayerId);
            // var profiles = rslt.Where(x => !string.Equals(x.platform, player.Platform.ToString(), StringComparison.OrdinalIgnoreCase));
            // foreach (var profile in profiles)
            // {
            //     player.Aliases.Add(new Player.Alias()
            //     {
            //         UrlName = profile.urlName,
            //         Username = profile.name,
            //         Platform = profile.platform.PlatformStringToEnum(),
            //         //Visibility is null in first test
            //         ProfileVisibility = 
            //             profile?.visibility?.isFriendsOnly ?? false ? Visibility.FriendsOnly 
            //             : profile?.visibility?.isPrivate ?? false ? Visibility.Private
            //             : Visibility.Public
            //     });
            // }
        }

        /// <summary>
        /// Load any other profiles associated with the player
        /// The player MUST have had their Aliases populated via <see cref="GetAliasesAsync(Player)"/> first.
        /// </summary>
        /// <param name="player">A loaded player profile that has been passed through <see cref="GetAliasesAsync(Player)"/></param>
        /// <returns>A List of player profiles connected with the given player, or null if no other profiles are found.</returns>
        public async Task<List<Player>> GetOtherProfilesAsync(Player player)
        {
            // if (player.Aliases == null)
            //     throw new ArgumentException("Player has no aliases loaded, use GetAliasesAsync first.", nameof(player));
            // if (player.Aliases.Count == 0)
            //     return null;
            // var profiles = new List<Player>();
            // foreach (var alias in player.Aliases)
            //     profiles.Add(await GetOtherProfileFromAliasAsync(alias));
            // return profiles;
            return null;
        }

        // public async Task<Player> GetOtherProfileFromAliasAsync(Player.Alias playerAlias) =>
        //     await GetPlayerAsync(playerAlias.Username, playerAlias.Platform);

        public void Dispose() => _profileClient.Dispose();
    }
}

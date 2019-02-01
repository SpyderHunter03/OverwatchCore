using System;
using OverwatchCore.Data;
using OverwatchCore.StaticResources;

namespace OverwatchCore.Extensions
{
    public static class PlayerExtensions
    {
        public static Player Validate(this Player player, bool privateProfile = false)
        {
            return Validator.Validate(player.PlayerId) ? player : null;
        }
    }
}

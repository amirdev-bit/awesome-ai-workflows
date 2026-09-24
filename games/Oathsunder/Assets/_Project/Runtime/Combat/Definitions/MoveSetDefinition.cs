using System.Collections.Generic;

namespace Oathsunder.Combat.Definitions
{
    /// <summary>A named collection of moves and projectiles (the universal set, or one weapon class).</summary>
    public sealed class MoveSetDefinition
    {
        /// <summary>Stable id, e.g. "weapon.katana" or "universal".</summary>
        public string Id = "";

        /// <summary>Display name.</summary>
        public string DisplayName = "";

        /// <summary>Moves in declaration order.</summary>
        public readonly List<MoveDefinition> Moves = new List<MoveDefinition>();

        /// <summary>Projectiles.</summary>
        public readonly List<ProjectileDefinition> Projectiles = new List<ProjectileDefinition>();
    }
}

using Oathsunder.Core.Mathematics;

namespace Oathsunder.Combat.Definitions
{
    /// <summary>Gameplay bounds of an arena. Visuals live in the Unity scene; only these numbers are simulated.</summary>
    public sealed class StageDefinition
    {
        /// <summary>Stable id, e.g. "stage.emberfall.belltower".</summary>
        public string Id = "";

        /// <summary>Display name.</summary>
        public string Name = "";

        /// <summary>X of the left wall (metres).</summary>
        public Fixed LeftWall = Fixed.FromInt(-12);

        /// <summary>X of the right wall (metres).</summary>
        public Fixed RightWall = Fixed.FromInt(12);

        /// <summary>Maximum horizontal distance between two opponents (camera framing).</summary>
        public Fixed MaxSeparation = Fixed.FromInt(8);

        /// <summary>Round-start distance of each fighter from the centre.</summary>
        public Fixed SpawnOffset = Fixed.FromMilli(1600);

        /// <summary>Whether wall bounces are possible (open-edged arenas disable them).</summary>
        public bool WallBounceEnabled = true;
    }
}

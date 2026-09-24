using Oathsunder.Core.Mathematics;

namespace Oathsunder.Combat.Definitions
{
    /// <summary>A projectile type (wind slashes, thrown knives, arcane bolts).</summary>
    public sealed class ProjectileDefinition
    {
        /// <summary>Stable id, e.g. "katana.severingwind".</summary>
        public string Id = "";

        /// <summary>Index in the owning blueprint.</summary>
        public int Index = -1;

        /// <summary>Frames before it dissipates.</summary>
        public int Lifetime = 60;

        /// <summary>Facing-relative velocity in metres per frame.</summary>
        public FixedVector2 Velocity;

        /// <summary>Hitbox relative to the projectile position (facing-relative).</summary>
        public FixedAabb Box;

        /// <summary>Attack payload.</summary>
        public AttackSpec Attack = new AttackSpec();

        /// <summary>Durability: hits (on fighters or clashing projectiles) before it is destroyed.</summary>
        public int Hits = 1;

        /// <summary>Frames before the same victim can be hit again (0 = once per victim).</summary>
        public int RehitInterval;

        /// <summary>Presentation key (VFX prefab).</summary>
        public string Visual = "";

        /// <summary>Copy with its own index and attack.</summary>
        public ProjectileDefinition CloneForResolution()
        {
            var clone = (ProjectileDefinition)MemberwiseClone();
            clone.Index = -1;
            clone.Attack = Attack.Clone();
            return clone;
        }
    }
}

using Oathsunder.Core.Mathematics;

namespace Oathsunder.Combat.Definitions
{
    /// <summary>
    /// What happens when a hitbox connects. Immutable after content loading. All velocities are in metres per
    /// frame and facing-relative (+X pushes the victim away from the attacker).
    /// </summary>
    public sealed class AttackSpec
    {
        /// <summary>Key of the attack inside its move ("a", "sweetspot", …).</summary>
        public string Key = "";

        /// <summary>Base damage before stats, modifiers and combo scaling.</summary>
        public int Damage;

        /// <summary>Damage dealt through guard.</summary>
        public int ChipDamage;

        /// <summary>Posture damage on block (half is applied on hit; scaled up on parry).</summary>
        public int PostureDamage;

        /// <summary>Hitstun frames (after hitstop).</summary>
        public int Hitstun;

        /// <summary>Blockstun frames (after hitstop).</summary>
        public int Blockstun;

        /// <summary>Hitstop frames applied to attacker and victim on hit.</summary>
        public int Hitstop;

        /// <summary>Guard requirement.</summary>
        public AttackHeight Height = AttackHeight.Mid;

        /// <summary>Behaviour flags.</summary>
        public AttackFlags Flags;

        /// <summary>Element for RPG resistances and VFX.</summary>
        public DamageElement Element = DamageElement.Physical;

        /// <summary>Ground knockback velocity on hit.</summary>
        public Fixed Knockback;

        /// <summary>Pushback velocity on block.</summary>
        public Fixed BlockPushback;

        /// <summary>Velocity given to a launched or knocked-down victim.</summary>
        public FixedVector2 LaunchVelocity;

        /// <summary>Velocity given to an airborne victim hit without launch.</summary>
        public FixedVector2 AirKnockback;

        /// <summary>Juggle points consumed when this hits an already-juggled victim.</summary>
        public int JuggleCost = 1;

        /// <summary>Scaling applied to the whole combo when this attack is the combo starter (1000 = none).</summary>
        public int ProrationPermille = 1000;

        /// <summary>Move to switch the attacker into on hit (throw success, ultimate cinematic). Empty for none.</summary>
        public string OnHitMoveId = "";

        /// <summary>Resolved index of <see cref="OnHitMoveId"/> in the fighter blueprint (-1 for none).</summary>
        public int OnHitMoveIndex = -1;

        /// <summary>Shallow copy (every field is a value or an immutable string).</summary>
        public AttackSpec Clone() => (AttackSpec)MemberwiseClone();

        /// <summary>True when any of <paramref name="flags"/> is set.</summary>
        public bool Has(AttackFlags flags) => (Flags & flags) != 0;
    }
}

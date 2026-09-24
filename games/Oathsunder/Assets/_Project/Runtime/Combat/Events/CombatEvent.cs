using System;
using Oathsunder.Core.Mathematics;

namespace Oathsunder.Combat.Events
{
    /// <summary>Kind of combat event.</summary>
    public enum CombatEventType : byte
    {
        /// <summary>Countdown started.</summary>
        RoundStart,

        /// <summary>Control given ("Fight!").</summary>
        RoundFight,

        /// <summary>A move started. Value = move index.</summary>
        MoveStart,

        /// <summary>A data cue fired. Value = cue index.</summary>
        Cue,

        /// <summary>A jump left the ground.</summary>
        Jump,

        /// <summary>Touched down.</summary>
        Land,

        /// <summary>A strike hit. Value = damage.</summary>
        Hit,

        /// <summary>A strike was blocked. Value = chip damage.</summary>
        Block,

        /// <summary>Perfect Parry. Actor = parrier, Target = attacker.</summary>
        Parry,

        /// <summary>Perfect Dodge. Actor = dodger, Target = attacker.</summary>
        PerfectDodge,

        /// <summary>Counter stance caught an attack. Actor = stance user.</summary>
        CounterStance,

        /// <summary>Armor absorbed a hit. Actor = attacker, Target = armored fighter.</summary>
        ArmorHit,

        /// <summary>Posture broke.</summary>
        GuardBreak,

        /// <summary>Victim launched.</summary>
        Launch,

        /// <summary>Victim bounced off a wall.</summary>
        WallBounce,

        /// <summary>Victim bounced off the ground.</summary>
        GroundBounce,

        /// <summary>Victim knocked down.</summary>
        Knockdown,

        /// <summary>Victim tech-rolled.</summary>
        TechRoll,

        /// <summary>Victim started waking up.</summary>
        WakeUp,

        /// <summary>A combo ended. Value = damage, Value2 = hits.</summary>
        ComboEnd,

        /// <summary>Grab connected; paired action started.</summary>
        GrabConnect,

        /// <summary>Throw teched.</summary>
        ThrowTech,

        /// <summary>Scripted paired damage. Value = damage.</summary>
        PairedHit,

        /// <summary>Paired victim released.</summary>
        PairedRelease,

        /// <summary>Execution started (cinematic).</summary>
        ExecutionStart,

        /// <summary>Ultimate cinematic started.</summary>
        UltimateCinematic,

        /// <summary>Projectile spawned. Value = projectile index, Value2 = instance.</summary>
        ProjectileSpawn,

        /// <summary>Projectiles clashed.</summary>
        ProjectileClash,

        /// <summary>Projectile expired or was destroyed.</summary>
        ProjectileEnd,

        /// <summary>Ember Rage started.</summary>
        RageStart,

        /// <summary>Ember Rage ended.</summary>
        RageEnd,

        /// <summary>Umbral Shadow started.</summary>
        ShadowStart,

        /// <summary>Umbral Shadow ended.</summary>
        ShadowEnd,

        /// <summary>A shadow echo struck. Value = damage.</summary>
        ShadowEcho,

        /// <summary>Shadow Time began on Target (slowed).</summary>
        TimeDilationStart,

        /// <summary>Fighter knocked out. Flags carry <see cref="CombatEventFlags.Finisher"/>.</summary>
        KnockOut,

        /// <summary>Round time expired.</summary>
        TimeOver,

        /// <summary>Round decided. Value = winning team (-2 draw).</summary>
        RoundEnd,

        /// <summary>Match decided. Value = winning team (-2 draw).</summary>
        MatchEnd,
    }

    /// <summary>Modifiers of an event.</summary>
    [Flags]
    public enum CombatEventFlags : ushort
    {
        /// <summary>None.</summary>
        None = 0,

        /// <summary>Counter hit.</summary>
        Counter = 1 << 0,

        /// <summary>Punish.</summary>
        Punish = 1 << 1,

        /// <summary>Lethal.</summary>
        Lethal = 1 << 2,

        /// <summary>Cinematic finisher.</summary>
        Finisher = 1 << 3,

        /// <summary>Caused by a projectile.</summary>
        Projectile = 1 << 4,

        /// <summary>Low attack.</summary>
        Low = 1 << 5,

        /// <summary>Overhead attack.</summary>
        Overhead = 1 << 6,

        /// <summary>Juggle hit.</summary>
        Juggle = 1 << 7,

        /// <summary>Heavy impact (heavy tag, launcher, knockdown).</summary>
        Heavy = 1 << 8,

        /// <summary>Hit during Ember Rage.</summary>
        Rage = 1 << 9,
    }

    /// <summary>
    /// One thing that happened during a simulation tick. Plain data; the presentation layer turns it into VFX,
    /// SFX, camera shakes, haptics and UI. The tuple (Frame, Type, Actor, Target, Instance) is stable across
    /// rollback re-simulation, which lets presentation deduplicate effects that were already played.
    /// </summary>
    public readonly struct CombatEvent
    {
        /// <summary>World frame.</summary>
        public readonly int Frame;

        /// <summary>Type.</summary>
        public readonly CombatEventType Type;

        /// <summary>Acting fighter (-1 when none).</summary>
        public readonly int Actor;

        /// <summary>Affected fighter (-1 when none).</summary>
        public readonly int Target;

        /// <summary>Move or projectile instance id (0 when none).</summary>
        public readonly int Instance;

        /// <summary>Primary value (damage, move index, cue index, team…).</summary>
        public readonly int Value;

        /// <summary>Secondary value (hits, move index for hits…).</summary>
        public readonly int Value2;

        /// <summary>World position (contact point, landing spot…).</summary>
        public readonly FixedVector2 Position;

        /// <summary>Modifiers.</summary>
        public readonly CombatEventFlags Flags;

        /// <summary>Creates an event.</summary>
        public CombatEvent(int frame, CombatEventType type, int actor, int target, int instance, int value, int value2, FixedVector2 position, CombatEventFlags flags)
        {
            Frame = frame;
            Type = type;
            Actor = actor;
            Target = target;
            Instance = instance;
            Value = value;
            Value2 = value2;
            Position = position;
            Flags = flags;
        }

        /// <summary>True when the flag is set.</summary>
        public bool Has(CombatEventFlags flag) => (Flags & flag) != 0;

        /// <inheritdoc />
        public override string ToString() => $"#{Frame} {Type} a={Actor} t={Target} v={Value}/{Value2} {Flags}";
    }
}

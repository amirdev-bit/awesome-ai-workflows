using System;

namespace Oathsunder.Combat.Simulation
{
    /// <summary>High-level state of a fighter. Everything data-driven runs inside <see cref="Move"/>.</summary>
    public enum FighterAction : byte
    {
        /// <summary>Standing still.</summary>
        Idle,

        /// <summary>Walking toward the target.</summary>
        WalkForward,

        /// <summary>Walking away from the target.</summary>
        WalkBackward,

        /// <summary>Crouching.</summary>
        Crouch,

        /// <summary>Jump startup (grounded).</summary>
        PreJump,

        /// <summary>Airborne without an attack.</summary>
        Airborne,

        /// <summary>Landing recovery.</summary>
        Landing,

        /// <summary>Holding standing guard.</summary>
        GuardStand,

        /// <summary>Holding crouching guard.</summary>
        GuardCrouch,

        /// <summary>Recovering from a successful Perfect Parry (can act immediately).</summary>
        ParryRecovery,

        /// <summary>Performing a data-driven move (attacks, specials, movement, throws, executions).</summary>
        Move,

        /// <summary>Blocking an attack.</summary>
        Blockstun,

        /// <summary>Grounded hitstun.</summary>
        Hitstun,

        /// <summary>Airborne hitstun that resets to a normal landing.</summary>
        AirHitstun,

        /// <summary>Launched into a juggle; lands in a knockdown.</summary>
        Launched,

        /// <summary>Lying on the ground.</summary>
        Knockdown,

        /// <summary>Getting up (fully invulnerable).</summary>
        WakeUp,

        /// <summary>Staggered by a Guard Break or a parried heavy attack.</summary>
        Staggered,

        /// <summary>Held by an opponent's paired action (throw, execution, ultimate).</summary>
        PairedVictim,

        /// <summary>Knocked out.</summary>
        KnockedOut,
    }

    /// <summary>Why a fighter is staggered.</summary>
    public enum StaggerKind : byte
    {
        /// <summary>Not staggered.</summary>
        None,

        /// <summary>Posture was broken (executable).</summary>
        GuardBreak,

        /// <summary>A heavy attack was parried.</summary>
        Parried,
    }

    /// <summary>Contact result of the current move.</summary>
    [Flags]
    public enum MoveContact : byte
    {
        /// <summary>No contact yet.</summary>
        None = 0,

        /// <summary>Hit a victim.</summary>
        Hit = 1 << 0,

        /// <summary>Was blocked.</summary>
        Blocked = 1 << 1,

        /// <summary>Was parried or caught by a counter stance.</summary>
        Parried = 1 << 2,

        /// <summary>Hit armor.</summary>
        Armored = 1 << 3,

        /// <summary>Was perfectly dodged.</summary>
        Evaded = 1 << 4,
    }

    /// <summary>Phase of the current round.</summary>
    public enum RoundPhase : byte
    {
        /// <summary>Countdown; no control.</summary>
        PreRound,

        /// <summary>Fighting.</summary>
        Fighting,

        /// <summary>A team was knocked out (or time ran out); finisher plays.</summary>
        RoundEnding,

        /// <summary>The match is decided.</summary>
        MatchOver,
    }
}

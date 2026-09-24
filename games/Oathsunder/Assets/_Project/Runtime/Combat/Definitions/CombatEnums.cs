using System;

namespace Oathsunder.Combat.Definitions
{
    /// <summary>How an attack must be guarded (canon §7.2).</summary>
    public enum AttackHeight : byte
    {
        /// <summary>Blockable standing or crouching; whiffs over crouchers by geometry.</summary>
        High,

        /// <summary>Blockable standing or crouching.</summary>
        Mid,

        /// <summary>Must be blocked crouching.</summary>
        Low,

        /// <summary>Must be blocked standing.</summary>
        Overhead,

        /// <summary>Cannot be blocked (can still be parried unless also unparryable).</summary>
        Unblockable,
    }

    /// <summary>Behaviour flags of an attack.</summary>
    [Flags]
    public enum AttackFlags : uint
    {
        /// <summary>No flags.</summary>
        None = 0,

        /// <summary>Launches a grounded victim into a juggle.</summary>
        Launch = 1u << 0,

        /// <summary>Knocks down (sweeps, trips).</summary>
        Knockdown = 1u << 1,

        /// <summary>Knockdown cannot be teched.</summary>
        HardKnockdown = 1u << 2,

        /// <summary>Victim bounces off the wall once per combo.</summary>
        WallBounce = 1u << 3,

        /// <summary>Victim bounces off the ground once per combo.</summary>
        GroundBounce = 1u << 4,

        /// <summary>Breaks guard instantly when blocked.</summary>
        GuardCrush = 1u << 5,

        /// <summary>Cannot be parried.</summary>
        Unparryable = 1u << 6,

        /// <summary>This is a grab: it connects only with throwable victims and starts a paired action.</summary>
        Grab = 1u << 7,

        /// <summary>Grab that also connects with airborne victims.</summary>
        AirGrab = 1u << 8,

        /// <summary>Execution grab: ignores throw invulnerability, requires an executable victim.</summary>
        Execution = 1u << 9,

        /// <summary>Hits knocked-down victims (once per combo).</summary>
        OffTheGround = 1u << 10,

        /// <summary>Launches on counter hit.</summary>
        LaunchOnCounter = 1u << 11,

        /// <summary>Attacker is staggered when this attack is parried.</summary>
        StaggerOnParry = 1u << 12,

        /// <summary>Ignores armor.</summary>
        ArmorBreak = 1u << 13,

        /// <summary>Not limited by juggle points.</summary>
        IgnoreJuggleLimit = 1u << 14,

        /// <summary>A lethal hit from this attack is a cinematic Finisher.</summary>
        Finisher = 1u << 15,

        /// <summary>Chip damage from this attack may be lethal.</summary>
        ChipKills = 1u << 16,

        /// <summary>
        /// Spikes airborne victims to the ground (air knockback + knockdown, ground bounce if flagged); grounded
        /// victims take normal hitstun instead. Used by diving air attacks.
        /// </summary>
        Spike = 1u << 17,
    }

    /// <summary>Damage element (consumed by RPG resistances and VFX).</summary>
    public enum DamageElement : byte
    {
        /// <summary>Steel.</summary>
        Physical,

        /// <summary>Fire / life (Ember).</summary>
        Ember,

        /// <summary>Shadow / void (Umbra).</summary>
        Umbra,

        /// <summary>Ice.</summary>
        Frost,

        /// <summary>Lightning / wind.</summary>
        Storm,

        /// <summary>Poison / rot.</summary>
        Venom,

        /// <summary>Holy light (Oath-light).</summary>
        Radiant,
    }

    /// <summary>Classification tags of a move. Cancel windows target tags.</summary>
    [Flags]
    public enum MoveTags : uint
    {
        /// <summary>No tags.</summary>
        None = 0,

        /// <summary>Normal attack.</summary>
        Normal = 1u << 0,

        /// <summary>Command normal (direction + button).</summary>
        Command = 1u << 1,

        /// <summary>Special skill.</summary>
        Special = 1u << 2,

        /// <summary>Ultimate ability.</summary>
        Ultimate = 1u << 3,

        /// <summary>Throw.</summary>
        Throw = 1u << 4,

        /// <summary>Execution.</summary>
        Execution = 1u << 5,

        /// <summary>Dash, backstep, roll, dodge.</summary>
        Movement = 1u << 6,

        /// <summary>Light attack family.</summary>
        Light = 1u << 7,

        /// <summary>Heavy attack family (gains armor in Ember Rage).</summary>
        Heavy = 1u << 8,

        /// <summary>Charged attack.</summary>
        Charged = 1u << 9,

        /// <summary>Air move.</summary>
        Air = 1u << 10,

        /// <summary>Launcher.</summary>
        Launcher = 1u << 11,

        /// <summary>Counter stance.</summary>
        Counter = 1u << 12,

        /// <summary>Spawns projectiles.</summary>
        Projectile = 1u << 13,

        /// <summary>Rage / Shadow activation.</summary>
        ModeActivation = 1u << 14,

        /// <summary>Evasive (has invulnerability).</summary>
        Evasive = 1u << 15,

        /// <summary>Cinematic sequence (ultimate follow-up, execution).</summary>
        Cinematic = 1u << 16,

        /// <summary>Low attack.</summary>
        Low = 1u << 17,

        /// <summary>Overhead attack.</summary>
        Overhead = 1u << 18,
    }

    /// <summary>Structural flags of a move.</summary>
    [Flags]
    public enum MoveFlags : uint
    {
        /// <summary>No flags.</summary>
        None = 0,

        /// <summary>Only reachable through cancels, never from neutral.</summary>
        ChainOnly = 1u << 0,

        /// <summary>Landing ends the move (air attacks).</summary>
        LandCancel = 1u << 1,

        /// <summary>Horizontal velocity is kept when no motion segment covers a frame.</summary>
        KeepMomentum = 1u << 2,

        /// <summary>Returns to crouch when finished if down is held.</summary>
        EndsCrouched = 1u << 3,

        /// <summary>Do not auto-face the target when the move starts.</summary>
        NoAutoFace = 1u << 4,

        /// <summary>Use the crouching hurtbox by default.</summary>
        CrouchingHurtbox = 1u << 5,

        /// <summary>No body (pushbox) collision while the move runs (rolls pass through opponents).</summary>
        PassThrough = 1u << 6,
    }

    /// <summary>States from which a trigger can fire.</summary>
    [Flags]
    public enum TriggerStance : byte
    {
        /// <summary>Never.</summary>
        None = 0,

        /// <summary>Standing or crouching on the ground (direction masks separate the two).</summary>
        Grounded = 1 << 0,

        /// <summary>In the air.</summary>
        Airborne = 1 << 1,

        /// <summary>While in hitstun, blockstun or a juggle (bursts).</summary>
        Stunned = 1 << 2,

        /// <summary>Grounded or airborne.</summary>
        Any = Grounded | Airborne,
    }

    /// <summary>Extra requirements of a trigger.</summary>
    [Flags]
    public enum TriggerConditions : ushort
    {
        /// <summary>No extra requirement.</summary>
        None = 0,

        /// <summary>Target must be executable and in execution range.</summary>
        TargetExecutable = 1 << 0,

        /// <summary>Ember Rage must be active.</summary>
        RageActive = 1 << 1,

        /// <summary>Ember Rage must not be active.</summary>
        RageInactive = 1 << 2,

        /// <summary>Umbral Shadow must be active.</summary>
        ShadowActive = 1 << 3,

        /// <summary>Umbral Shadow must not be active.</summary>
        ShadowInactive = 1 << 4,
    }

    /// <summary>Which control schemes a trigger belongs to.</summary>
    [Flags]
    public enum ControlSchemeMask : byte
    {
        /// <summary>None.</summary>
        None = 0,

        /// <summary>Classic scheme (motion inputs).</summary>
        Classic = 1 << 0,

        /// <summary>Simplified scheme (direction + button; touch-first).</summary>
        Simplified = 1 << 1,

        /// <summary>Both schemes.</summary>
        All = Classic | Simplified,
    }

    /// <summary>A fighter's chosen control scheme.</summary>
    public enum ControlScheme : byte
    {
        /// <summary>Motion inputs.</summary>
        Classic,

        /// <summary>Direction + button.</summary>
        Simplified,
    }

    /// <summary>Invulnerability categories.</summary>
    [Flags]
    public enum InvulnerabilityMask : byte
    {
        /// <summary>Vulnerable.</summary>
        None = 0,

        /// <summary>Immune to strikes.</summary>
        Strike = 1 << 0,

        /// <summary>Immune to throws.</summary>
        Throw = 1 << 1,

        /// <summary>Immune to projectiles.</summary>
        Projectile = 1 << 2,

        /// <summary>Immune to everything.</summary>
        All = Strike | Throw | Projectile,
    }

    /// <summary>When a cancel window is open, relative to the move's contact result.</summary>
    public enum CancelCondition : byte
    {
        /// <summary>Always.</summary>
        Always,

        /// <summary>Only after the move hit.</summary>
        OnHit,

        /// <summary>Only after the move was blocked.</summary>
        OnBlock,

        /// <summary>After hit, block or armor contact.</summary>
        OnContact,

        /// <summary>Only if the move made no contact.</summary>
        OnWhiff,

        /// <summary>Only after the move perfectly dodged an attack (Perfect Dodge follow-ups).</summary>
        OnEvade,
    }

    /// <summary>How a cancel window is entered.</summary>
    public enum CancelInput : byte
    {
        /// <summary>Use the target moves' triggers.</summary>
        Trigger,

        /// <summary>Automatically when the button is held.</summary>
        Held,

        /// <summary>Automatically when the button is released.</summary>
        Released,

        /// <summary>Automatically, unconditionally.</summary>
        Auto,
    }

    /// <summary>Gameplay effects a move can fire on a specific frame.</summary>
    public enum MoveEffect : byte
    {
        /// <summary>Starts Ember Rage.</summary>
        ActivateRage,

        /// <summary>Starts Umbral Shadow.</summary>
        ActivateShadow,

        /// <summary>Turns to face the target.</summary>
        FaceTarget,
    }
}

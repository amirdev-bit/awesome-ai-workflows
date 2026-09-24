using Oathsunder.Core.Mathematics;

namespace Oathsunder.Combat.Definitions
{
    /// <summary>
    /// Global combat mechanics constants. Loaded from <c>tuning.combat.json</c>; the defaults below are the
    /// shipped values and are what the automated tests assert against.
    /// </summary>
    public sealed class CombatTuning
    {
        // ----- Input -----

        /// <summary>Frames (fighter-local) a press stays buffered.</summary>
        public int InputBufferFrames = 8;

        /// <summary>Frames within which chord buttons count as simultaneous.</summary>
        public int ChordWindowFrames = 3;

        /// <summary>Extra startup frames of Special moves performed with a Simplified-only trigger.</summary>
        public int SimplifiedStartupPenalty = 2;

        // ----- Defence -----

        /// <summary>Perfect Parry window (frames from the Guard press, inclusive).</summary>
        public int ParryWindowFrames = 6;

        /// <summary>Parry window when Guard is mashed.</summary>
        public int ParryWindowMashFrames = 2;

        /// <summary>A new Guard press within this many frames of the previous one counts as mashing.</summary>
        public int ParryMashThresholdFrames = 20;

        /// <summary>Duration of the parry recovery (actionable from frame 1).</summary>
        public int ParryRecoveryFrames = 8;

        /// <summary>Minimum hitstop on a parry.</summary>
        public int ParryHitstop = 10;

        /// <summary>Posture damage dealt to the attacker on parry (permille of the attack's posture damage).</summary>
        public int ParryPostureDamagePermille = 1500;

        /// <summary>Stagger duration for attacks flagged StaggerOnParry.</summary>
        public int ParryStaggerFrames = 30;

        /// <summary>Stagger duration after a Guard Break (executable).</summary>
        public int GuardBreakStaggerFrames = 80;

        /// <summary>Hitstop multiplier on block (permille).</summary>
        public int BlockHitstopPermille = 750;

        /// <summary>Posture damage applied on hit (permille of the attack's posture damage).</summary>
        public int PostureOnHitPermille = 500;

        /// <summary>Frames without posture damage before posture regenerates.</summary>
        public int PostureRegenDelayFrames = 90;

        /// <summary>Posture regenerated per frame.</summary>
        public int PostureRegenPerFrame = 3;

        /// <summary>Frames of throw invulnerability after leaving hit/blockstun.</summary>
        public int ThrowInvulnerableAfterStunFrames = 4;

        /// <summary>Pushback velocity when a throw is teched (m/frame).</summary>
        public Fixed ThrowTechPushback = Fixed.FromRatio(8, 60);

        /// <summary>Recovery after a throw tech.</summary>
        public int ThrowTechRecoveryFrames = 14;

        /// <summary>Hitstop of a counter-stance catch.</summary>
        public int CounterStanceHitstop = 12;

        /// <summary>Armor hitstop.</summary>
        public int ArmorHitstop = 6;

        /// <summary>Hitstop of a lethal blow.</summary>
        public int KnockoutHitstop = 24;

        // ----- Offence -----

        /// <summary>Counter-hit damage multiplier (permille).</summary>
        public int CounterHitDamagePermille = 1200;

        /// <summary>Counter-hit extra hitstun.</summary>
        public int CounterHitHitstunBonus = 4;

        /// <summary>Counter-hit extra hitstop.</summary>
        public int CounterHitHitstopBonus = 2;

        /// <summary>Punish (hit during recovery) damage multiplier (permille).</summary>
        public int PunishDamagePermille = 1100;

        /// <summary>Punish extra hitstun.</summary>
        public int PunishHitstunBonus = 2;

        // ----- Combos -----

        /// <summary>Scaling lost per hit from the third hit onward (permille).</summary>
        public int ComboScalingStepPermille = 100;

        /// <summary>Minimum combo scaling (permille).</summary>
        public int MinComboScalingPermille = 300;

        /// <summary>Hit count from which hitstun decays.</summary>
        public int HitstunDecayStartHit = 6;

        /// <summary>One frame of hitstun is removed every this many hits past the start.</summary>
        public int HitstunDecayEveryHits = 2;

        /// <summary>Minimum hitstun.</summary>
        public int MinHitstun = 6;

        /// <summary>Juggle point budget per combo.</summary>
        public int MaxJugglePoints = 8;

        /// <summary>Extra gravity per juggle hit (permille).</summary>
        public int JuggleGravityPerHitPermille = 40;

        /// <summary>Maximum juggle gravity multiplier (permille).</summary>
        public int MaxJuggleGravityPermille = 1800;

        /// <summary>Vertical velocity of a ground bounce (m/frame).</summary>
        public Fixed GroundBounceVelocity = Fixed.FromRatio(9, 60);

        /// <summary>Horizontal restitution of a wall bounce (permille).</summary>
        public int WallBounceRestitutionPermille = 450;

        /// <summary>Minimum upward velocity after a wall bounce (m/frame).</summary>
        public Fixed WallBounceUpVelocity = Fixed.FromRatio(7, 60);

        /// <summary>Pop velocity when a sweep has no launch velocity (m/frame).</summary>
        public FixedVector2 TripVelocity = new FixedVector2(Fixed.FromRatio(1, 60), Fixed.FromRatio(4, 60));

        /// <summary>Velocity of a knocked-out body (m/frame).</summary>
        public FixedVector2 KnockoutLaunchVelocity = new FixedVector2(Fixed.FromRatio(5, 60), Fixed.FromRatio(8, 60));

        /// <summary>Landing recovery after being air-reset.</summary>
        public int AirResetLandingFrames = 6;

        /// <summary>Frames after hitting the ground during which a soft knockdown can be teched.</summary>
        public int TechWindowFrames = 10;

        /// <summary>Air actions (from neutral) per jump.</summary>
        public int MaxAirActions = 1;

        // ----- Meters -----

        /// <summary>Meter capacity (Rage, Shadow, Ultimate).</summary>
        public int MeterMax = 1000;

        /// <summary>Ultimate gained per point of damage dealt (permille).</summary>
        public int UltimateGainDealtPermille = 1000;

        /// <summary>Ultimate gained per point of damage taken (permille).</summary>
        public int UltimateGainTakenPermille = 500;

        /// <summary>Rage gained per point of damage taken (permille).</summary>
        public int RageGainTakenPermille = 700;

        /// <summary>Rage gained per point of damage dealt (permille).</summary>
        public int RageGainDealtPermille = 150;

        /// <summary>Shadow gained per hit dealt.</summary>
        public int ShadowGainPerHit = 12;

        /// <summary>Shadow gained per Perfect Parry.</summary>
        public int ShadowGainPerfectParry = 150;

        /// <summary>Shadow gained per Perfect Dodge.</summary>
        public int ShadowGainPerfectDodge = 200;

        /// <summary>Ultimate gained per block (defender).</summary>
        public int UltimateGainOnBlock = 15;

        // ----- Modes -----

        /// <summary>Ember Rage duration.</summary>
        public int RageDurationFrames = 480;

        /// <summary>Ember Rage damage multiplier (permille).</summary>
        public int RageDamagePermille = 1200;

        /// <summary>Armor hits granted to Heavy moves during Ember Rage.</summary>
        public int RageHeavyArmorHits = 1;

        /// <summary>Umbral Shadow duration.</summary>
        public int ShadowDurationFrames = 360;

        /// <summary>Delay before a shadow echo strikes.</summary>
        public int ShadowEchoDelayFrames = 10;

        /// <summary>Echo damage (permille of the triggering hit).</summary>
        public int ShadowEchoDamagePermille = 300;

        /// <summary>Hitstun added to a victim still in hitstun when an echo lands.</summary>
        public int ShadowEchoHitstunBonus = 3;

        /// <summary>Victim hitstop of an echo.</summary>
        public int ShadowEchoHitstop = 4;

        // ----- Executions -----

        /// <summary>Health threshold (permille of max) under which a staggered or hitstunned target is executable.</summary>
        public int ExecutionHealthPermille = 150;

        /// <summary>Maximum distance for an execution (metres).</summary>
        public Fixed ExecutionRange = Fixed.FromMilli(1800);
    }
}

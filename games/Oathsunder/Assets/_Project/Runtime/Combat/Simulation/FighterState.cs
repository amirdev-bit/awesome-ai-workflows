using Oathsunder.Core.Hashing;
using Oathsunder.Core.Mathematics;

namespace Oathsunder.Combat.Simulation
{
    /// <summary>
    /// Complete mutable state of one fighter. A plain value type with no references, so a rollback snapshot is a
    /// memberwise copy and the checksum covers everything.
    /// </summary>
    public struct FighterState
    {
        // ----- Identity -----

        /// <summary>Whether this slot is used.</summary>
        public bool Active;

        /// <summary>Team (0 or 1).</summary>
        public int Team;

        /// <summary>Current target fighter index (-1 when none).</summary>
        public int TargetIndex;

        // ----- Kinematics -----

        /// <summary>Position of the feet (metres).</summary>
        public FixedVector2 Position;

        /// <summary>Velocity (metres per frame).</summary>
        public FixedVector2 Velocity;

        /// <summary>+1 facing right, -1 facing left.</summary>
        public int Facing;

        /// <summary>True when standing on the ground.</summary>
        public bool Grounded;

        /// <summary>Gravity multiplier for the current move frame (permille).</summary>
        public int MoveGravityPermille;

        // ----- Action -----

        /// <summary>Current action.</summary>
        public FighterAction Action;

        /// <summary>Frames spent in the current action (1-based for moves).</summary>
        public int ActionFrame;

        /// <summary>Current move index when <see cref="Action"/> is <see cref="FighterAction.Move"/>, else -1.</summary>
        public int MoveIndex;

        /// <summary>Unique id of the current move instance (hit-once bookkeeping, events, rollback-safe VFX keys).</summary>
        public int MoveInstance;

        /// <summary>Contact result of the current move.</summary>
        public MoveContact Contact;

        /// <summary>Bit (group * 4 + victim) set once a hit group connected with a victim.</summary>
        public uint HitRegistry;

        /// <summary>Armor hits absorbed during the current move.</summary>
        public int ArmorHitsTaken;

        /// <summary>Remaining hitstun / blockstun / stagger / landing frames.</summary>
        public int StunRemaining;

        /// <summary>Remaining hitstop (freeze) frames.</summary>
        public int HitstopRemaining;

        /// <summary>Frames this fighter has actually simulated (excludes hitstop and time-dilated skips).</summary>
        public int LocalFrame;

        /// <summary>World frame on which this fighter's logic last ran.</summary>
        public int LastLogicWorldFrame;

        /// <summary>Air actions started from neutral during the current jump.</summary>
        public int AirActionsUsed;

        /// <summary>Guard is held low (crouch guard) — also used in blockstun.</summary>
        public bool CrouchGuard;

        /// <summary>Stagger reason when <see cref="Action"/> is <see cref="FighterAction.Staggered"/>.</summary>
        public StaggerKind Stagger;

        /// <summary>Knockdown is hard (untechable).</summary>
        public bool HardKnockdown;

        /// <summary>Remaining generic throw invulnerability.</summary>
        public int ThrowInvulnerableFrames;

        // ----- Guard / parry -----

        /// <summary>Last local frame on which the Perfect Parry window is open.</summary>
        public int ParryWindowEndLocal;

        /// <summary>Local frame of the previous Guard press (anti-mash).</summary>
        public int LastGuardPressLocal;

        /// <summary>Instance id of the last attack this fighter perfectly dodged.</summary>
        public int LastEvadedInstance;

        // ----- Resources -----

        /// <summary>Health.</summary>
        public int Health;

        /// <summary>Maximum health.</summary>
        public int MaxHealth;

        /// <summary>Posture (0 = fresh, max = Guard Break).</summary>
        public int Posture;

        /// <summary>Maximum posture.</summary>
        public int MaxPosture;

        /// <summary>Frames before posture regenerates.</summary>
        public int PostureRegenDelay;

        /// <summary>Ember Rage meter.</summary>
        public int Rage;

        /// <summary>Umbral Shadow meter.</summary>
        public int Shadow;

        /// <summary>Ultimate meter.</summary>
        public int Ultimate;

        /// <summary>Remaining Ember Rage frames (0 = inactive).</summary>
        public int RageFrames;

        /// <summary>Remaining Umbral Shadow frames (0 = inactive).</summary>
        public int ShadowFrames;

        // ----- Combo (as victim) -----

        /// <summary>Hits taken in the current combo.</summary>
        public int ComboHits;

        /// <summary>Damage taken in the current combo.</summary>
        public int ComboDamage;

        /// <summary>Starter proration of the current combo (permille).</summary>
        public int ComboStarterPermille;

        /// <summary>Juggle points spent in the current combo.</summary>
        public int JugglePoints;

        /// <summary>Fighter index that started the current combo.</summary>
        public int ComboAttacker;

        /// <summary>A wall bounce happened in this combo.</summary>
        public bool WallBounceUsed;

        /// <summary>A ground bounce happened in this combo.</summary>
        public bool GroundBounceUsed;

        /// <summary>An off-the-ground hit happened in this combo.</summary>
        public bool OffTheGroundUsed;

        /// <summary>The next wall contact bounces.</summary>
        public bool PendingWallBounce;

        /// <summary>The next ground contact bounces.</summary>
        public bool PendingGroundBounce;

        /// <summary>Fighter who last struck this one (for corner pushback transfer; -1 for projectiles).</summary>
        public int LastAttackerIndex;

        // ----- Paired actions -----

        /// <summary>Partner fighter index in a paired action (-1 when none).</summary>
        public int PairedPartner;

        /// <summary>Remaining tech frames for a paired victim.</summary>
        public int PairedTechRemaining;

        /// <summary>Victim offset while held (facing-relative to the attacker).</summary>
        public FixedVector2 PairedOffset;

        // ----- Time dilation -----

        /// <summary>Time scale (permille; 1000 = normal).</summary>
        public int TimeScalePermille;

        /// <summary>World frames remaining at the current time scale.</summary>
        public int TimeScaleFrames;

        /// <summary>Fractional tick accumulator.</summary>
        public int TimeAccumulator;

        /// <summary>Transient: whether the fighter simulated this world frame.</summary>
        public bool AdvancedThisFrame;

        // ----- Outcome -----

        /// <summary>Knocked out this round.</summary>
        public bool IsKnockedOut;

        /// <summary>True when Ember Rage is active.</summary>
        public bool InRage => RageFrames > 0;

        /// <summary>True when Umbral Shadow is active.</summary>
        public bool InShadow => ShadowFrames > 0;

        /// <summary>Adds every field to a checksum.</summary>
        public void AppendHash(ref StateHasher h)
        {
            h.Add(Active);
            h.Add(Team);
            h.Add(TargetIndex);
            h.Add(Position);
            h.Add(Velocity);
            h.Add(Facing);
            h.Add(Grounded);
            h.Add(MoveGravityPermille);
            h.Add((int)Action);
            h.Add(ActionFrame);
            h.Add(MoveIndex);
            h.Add(MoveInstance);
            h.Add((int)Contact);
            h.Add(HitRegistry);
            h.Add(ArmorHitsTaken);
            h.Add(StunRemaining);
            h.Add(HitstopRemaining);
            h.Add(LocalFrame);
            h.Add(LastLogicWorldFrame);
            h.Add(AirActionsUsed);
            h.Add(CrouchGuard);
            h.Add((int)Stagger);
            h.Add(HardKnockdown);
            h.Add(ThrowInvulnerableFrames);
            h.Add(ParryWindowEndLocal);
            h.Add(LastGuardPressLocal);
            h.Add(LastEvadedInstance);
            h.Add(Health);
            h.Add(MaxHealth);
            h.Add(Posture);
            h.Add(MaxPosture);
            h.Add(PostureRegenDelay);
            h.Add(Rage);
            h.Add(Shadow);
            h.Add(Ultimate);
            h.Add(RageFrames);
            h.Add(ShadowFrames);
            h.Add(ComboHits);
            h.Add(ComboDamage);
            h.Add(ComboStarterPermille);
            h.Add(JugglePoints);
            h.Add(ComboAttacker);
            h.Add(WallBounceUsed);
            h.Add(GroundBounceUsed);
            h.Add(OffTheGroundUsed);
            h.Add(PendingWallBounce);
            h.Add(PendingGroundBounce);
            h.Add(LastAttackerIndex);
            h.Add(PairedPartner);
            h.Add(PairedTechRemaining);
            h.Add(PairedOffset);
            h.Add(TimeScalePermille);
            h.Add(TimeScaleFrames);
            h.Add(TimeAccumulator);
            h.Add(IsKnockedOut);
        }
    }
}

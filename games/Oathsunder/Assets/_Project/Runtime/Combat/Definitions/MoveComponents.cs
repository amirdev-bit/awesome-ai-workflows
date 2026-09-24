using Oathsunder.Combat.Input;
using Oathsunder.Core.Mathematics;

namespace Oathsunder.Combat.Definitions
{
    /// <summary>An active hitbox over a frame window.</summary>
    public sealed class HitboxSpec
    {
        /// <summary>Frames on which the hitbox is active.</summary>
        public FrameWindow Window;

        /// <summary>Facing-relative box.</summary>
        public FixedAabb Box;

        /// <summary>Index into <see cref="MoveDefinition.Attacks"/>.</summary>
        public int AttackIndex;

        /// <summary>
        /// Hit group (0-7). Each group can hit each victim once per move instance, so multi-hit moves use one
        /// group per hit.
        /// </summary>
        public int Group;
    }

    /// <summary>Frame-ranged hurtbox override.</summary>
    public sealed class HurtboxWindow
    {
        /// <summary>Frames covered.</summary>
        public FrameWindow Window;

        /// <summary>Facing-relative boxes.</summary>
        public FixedAabb[] Boxes = new FixedAabb[0];

        /// <summary>True to replace the stance hurtboxes, false to add to them (extended limbs).</summary>
        public bool Replace;
    }

    /// <summary>Frame-ranged invulnerability.</summary>
    public sealed class InvulnerabilityWindow
    {
        /// <summary>Frames covered.</summary>
        public FrameWindow Window;

        /// <summary>What the fighter is immune to.</summary>
        public InvulnerabilityMask Mask;
    }

    /// <summary>Frame-ranged armor (absorbs hits without stun).</summary>
    public sealed class ArmorWindow
    {
        /// <summary>Frames covered.</summary>
        public FrameWindow Window;

        /// <summary>How many hits are absorbed per move instance.</summary>
        public int Hits = 1;

        /// <summary>Damage taken while armored (permille).</summary>
        public int DamagePermille = 500;
    }

    /// <summary>Scripted velocity over a frame window (root motion in the deterministic domain).</summary>
    public sealed class MotionSegment
    {
        /// <summary>Frames covered.</summary>
        public FrameWindow Window;

        /// <summary>Facing-relative velocity in metres per frame.</summary>
        public FixedVector2 Velocity;

        /// <summary>Whether X is written.</summary>
        public bool SetX;

        /// <summary>Whether Y is written.</summary>
        public bool SetY;

        /// <summary>Gravity multiplier while this segment is active (permille; 0 = float).</summary>
        public int GravityPermille = 1000;
    }

    /// <summary>Spawns a projectile on a frame.</summary>
    public sealed class ProjectileSpawn
    {
        /// <summary>Spawn frame.</summary>
        public int Frame;

        /// <summary>Projectile id.</summary>
        public string ProjectileId = "";

        /// <summary>Resolved projectile index in the blueprint.</summary>
        public int ProjectileIndex = -1;

        /// <summary>Facing-relative spawn offset from the fighter origin.</summary>
        public FixedVector2 Offset;

        /// <summary>Copy with its own resolved index.</summary>
        public ProjectileSpawn Clone() => (ProjectileSpawn)MemberwiseClone();
    }

    /// <summary>Fires a gameplay effect on a frame.</summary>
    public sealed class EffectCue
    {
        /// <summary>Frame.</summary>
        public int Frame;

        /// <summary>Effect.</summary>
        public MoveEffect Effect;
    }

    /// <summary>Presentation-only cue (SFX, VFX, camera, haptics) emitted as an event on a frame.</summary>
    public sealed class PresentationCue
    {
        /// <summary>Frame.</summary>
        public int Frame;

        /// <summary>Cue name, e.g. "sfx.katana.swing.light".</summary>
        public string Name = "";

        /// <summary>Index in <see cref="FighterBlueprint.CueNames"/>.</summary>
        public int CueIndex = -1;

        /// <summary>Copy with its own resolved index.</summary>
        public PresentationCue Clone() => (PresentationCue)MemberwiseClone();
    }

    /// <summary>Counter stance: strikes landing in the window are nullified and answered with a counter move.</summary>
    public sealed class CounterStanceSpec
    {
        /// <summary>Frames on which the stance catches attacks.</summary>
        public FrameWindow Window;

        /// <summary>What the stance catches (strikes and/or projectiles).</summary>
        public InvulnerabilityMask Catches = InvulnerabilityMask.Strike;

        /// <summary>Counter move id.</summary>
        public string CounterMoveId = "";

        /// <summary>Resolved counter move index.</summary>
        public int CounterMoveIndex = -1;

        /// <summary>Copy with its own resolved index.</summary>
        public CounterStanceSpec Clone() => (CounterStanceSpec)MemberwiseClone();
    }

    /// <summary>Scripted damage during a paired action.</summary>
    public sealed class PairedHit
    {
        /// <summary>Frame of the attacker's move.</summary>
        public int Frame;

        /// <summary>Damage (subject to combo scaling and stats).</summary>
        public int Damage;

        /// <summary>Posture damage.</summary>
        public int PostureDamage;

        /// <summary>Ignore combo scaling (cinematic damage stays predictable).</summary>
        public bool Unscaled;
    }

    /// <summary>
    /// Paired (synchronised two-fighter) action: throws, executions, ultimate cinematics. The victim is locked
    /// to the attacker at <see cref="VictimOffset"/> until <see cref="ReleaseFrame"/>.
    /// </summary>
    public sealed class PairedActionSpec
    {
        /// <summary>Facing-relative victim position while held.</summary>
        public FixedVector2 VictimOffset;

        /// <summary>Frames after the grab in which the victim can tech (0 = untechable).</summary>
        public int TechWindow;

        /// <summary>Scripted damage.</summary>
        public PairedHit[] Hits = new PairedHit[0];

        /// <summary>Frame on which the victim is released.</summary>
        public int ReleaseFrame;

        /// <summary>Attack key describing the release reaction (launch, knockdown, …).</summary>
        public string ReleaseAttackKey = "";

        /// <summary>Resolved release attack index in the move's attacks.</summary>
        public int ReleaseAttackIndex = -1;

        /// <summary>The attacker turns around on release (back throws).</summary>
        public bool AttackerTurnsAround;

        /// <summary>Facing-relative victim position at release (after any turn-around).</summary>
        public FixedVector2 ReleaseOffset;

        /// <summary>Presentation should treat this as a cinematic (camera takeover, letterbox).</summary>
        public bool Cinematic;

        /// <summary>Animation the victim plays.</summary>
        public string VictimAnimation = "";
    }

    /// <summary>Resource cost paid when a move starts.</summary>
    public sealed class ResourceCost
    {
        /// <summary>Rage meter cost.</summary>
        public int Rage;

        /// <summary>Shadow meter cost.</summary>
        public int Shadow;

        /// <summary>Ultimate meter cost.</summary>
        public int Ultimate;

        /// <summary>True when there is no cost.</summary>
        public bool IsFree => Rage == 0 && Shadow == 0 && Ultimate == 0;
    }

    /// <summary>One way to perform a move.</summary>
    public sealed class MoveTrigger
    {
        /// <summary>States the trigger works in.</summary>
        public TriggerStance Stance = TriggerStance.Grounded;

        /// <summary>Button chord (None for motion-only triggers such as dashes).</summary>
        public InputButtons Buttons;

        /// <summary>Accepted facing-relative directions on the press frame.</summary>
        public DirectionMask Direction = DirectionMask.Any;

        /// <summary>Motion command (null for none).</summary>
        public MotionCommand Motion;

        /// <summary>Control schemes that use this trigger.</summary>
        public ControlSchemeMask Schemes = ControlSchemeMask.All;

        /// <summary>Extra conditions.</summary>
        public TriggerConditions Conditions;

        /// <summary>Priority when several moves match the same press (higher wins).</summary>
        public int Priority;
    }

    /// <summary>A window during which the current move can be cancelled into others.</summary>
    public sealed class CancelWindow
    {
        /// <summary>Frames the window is open.</summary>
        public FrameWindow Window;

        /// <summary>Contact requirement.</summary>
        public CancelCondition Condition = CancelCondition.Always;

        /// <summary>How the window is entered.</summary>
        public CancelInput Input = CancelInput.Trigger;

        /// <summary>Button for <see cref="CancelInput.Held"/> / <see cref="CancelInput.Released"/>.</summary>
        public InputButtons InputButton;

        /// <summary>Explicit target move ids.</summary>
        public string[] TargetIds = new string[0];

        /// <summary>Any move carrying one of these tags (and not chain-only) is also a target.</summary>
        public MoveTags TargetTags;

        /// <summary>Allows a jump cancel.</summary>
        public bool AllowJump;

        /// <summary>Resolved candidate move indices in declaration order (built by the blueprint builder).</summary>
        public int[] Candidates = new int[0];

        /// <summary>Copy with its own candidate list.</summary>
        public CancelWindow Clone()
        {
            var clone = (CancelWindow)MemberwiseClone();
            clone.Candidates = new int[0];
            return clone;
        }
    }
}

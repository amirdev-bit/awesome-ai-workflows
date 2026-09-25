using System.Collections.Generic;
using Oathsunder.Combat.Definitions;
using Oathsunder.Core.Mathematics;
using Oathsunder.Presentation.Cues;

namespace Oathsunder.Presentation.Animation
{
    /// <summary>How the root travels during a move, and therefore how the clip must be authored.</summary>
    public enum RootMotionKind : byte
    {
        /// <summary>The root does not move. Author in place.</summary>
        InPlace,

        /// <summary>Scripted motion segments move the root. Author in place and match foot contacts to the captured curve.</summary>
        Scripted,

        /// <summary>Airborne: the root follows jump physics (gravity scaled by the move). Author in place relative to the root.</summary>
        Ballistic,

        /// <summary>Paired action: the attacker holds position and the victim is locked at the victim offset.</summary>
        Paired,
    }

    /// <summary>Phase of a move frame.</summary>
    public enum MovePhase : byte
    {
        /// <summary>Before the first active frame.</summary>
        Startup,

        /// <summary>Inside a hitbox window.</summary>
        Active,

        /// <summary>Between two active windows of a multi-hit move.</summary>
        Gap,

        /// <summary>After the last active frame.</summary>
        Recovery,

        /// <summary>Moves without hitboxes (movement, stances, cinematics).</summary>
        Action,
    }

    /// <summary>A named pose the animator must hit on an exact frame.</summary>
    public sealed class KeyPose
    {
        /// <summary>1-based move frame.</summary>
        public int Frame;

        /// <summary>Pose name, e.g. "Contact".</summary>
        public string Name = "";

        /// <summary>What the pose must show.</summary>
        public string Purpose = "";
    }

    /// <summary>Captured root position at the end of a move frame, relative to the start, facing right.</summary>
    public struct RootSample
    {
        /// <summary>1-based move frame.</summary>
        public int Frame;

        /// <summary>Offset from the start position (metres; +X is forward).</summary>
        public FixedVector2 Offset;

        /// <summary>Velocity during the frame (metres per frame; +X is forward).</summary>
        public FixedVector2 Velocity;

        /// <summary>True when the fighter is on the ground at the end of the frame.</summary>
        public bool Grounded;
    }

    /// <summary>Defensive state of one move frame, as contact detection sees it.</summary>
    public sealed class HurtState
    {
        /// <summary>1-based move frame.</summary>
        public int Frame;

        /// <summary>Stance box set: standing, crouching, airborne or knockdown.</summary>
        public string Stance = "";

        /// <summary>Hurtboxes relative to the root (metres; facing right).</summary>
        public FixedAabb[] Boxes = new FixedAabb[0];

        /// <summary>Invulnerability this frame.</summary>
        public InvulnerabilityMask Invulnerable;

        /// <summary>Armor absorbs hits this frame.</summary>
        public bool Armor;

        /// <summary>Attacks overlapping this frame trigger a perfect dodge.</summary>
        public bool PerfectEvade;

        /// <summary>The counter stance catches attacks this frame.</summary>
        public bool CounterStance;

        /// <summary>True when every defensive property equals <paramref name="other"/>'s.</summary>
        public bool SameAs(HurtState other)
        {
            if (other == null || Stance != other.Stance || Invulnerable != other.Invulnerable || Armor != other.Armor ||
                PerfectEvade != other.PerfectEvade || CounterStance != other.CounterStance || Boxes.Length != other.Boxes.Length)
            {
                return false;
            }

            for (int i = 0; i < Boxes.Length; i++)
            {
                if (!Boxes[i].Equals(other.Boxes[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>A presentation cue on a move frame.</summary>
    public struct TimedCue
    {
        /// <summary>1-based move frame.</summary>
        public int Frame;

        /// <summary>Cue name.</summary>
        public string Name;

        /// <summary>Channel.</summary>
        public CueChannel Channel;
    }

    /// <summary>Feedback fired by the simulation when one hitbox group connects.</summary>
    public sealed class ContactFeedback
    {
        /// <summary>Hit group.</summary>
        public int Group;

        /// <summary>First and last frame the group can connect.</summary>
        public FrameWindow Window;

        /// <summary>Attack key in the move data.</summary>
        public string Attack = "";

        /// <summary>Impact cue on a normal hit.</summary>
        public string OnHit = "";

        /// <summary>Impact cue on a counter hit.</summary>
        public string OnCounterHit = "";

        /// <summary>Impact cue when blocked.</summary>
        public string OnBlock = "";

        /// <summary>Hitstop frames (both fighters freeze; the pose holds).</summary>
        public int Hitstop;
    }

    /// <summary>
    /// The complete animation production specification of one move, captured from the running simulation: the
    /// timing contract (startup/active/recovery), key poses, the root-motion curve, hitbox and hurtbox timelines,
    /// and every VFX, sound, voice, camera and haptic trigger. Clips must match it frame for frame.
    /// </summary>
    public sealed class MoveAnimationSpec
    {
        /// <summary>The move.</summary>
        public MoveDefinition Move;

        /// <summary>Clip name (<c>A_</c> prefix).</summary>
        public string Clip = "";

        /// <summary>Frames before the first active frame (0 when the move has no hitbox).</summary>
        public int Startup;

        /// <summary>Frames from the first to the last active frame, inclusive.</summary>
        public int ActiveSpan;

        /// <summary>Frames after the last active frame.</summary>
        public int Recovery;

        /// <summary>How the root travels.</summary>
        public RootMotionKind RootMotion;

        /// <summary>Phase of each frame (index 0 = frame 1).</summary>
        public MovePhase[] Phases = new MovePhase[0];

        /// <summary>Root samples, one per frame.</summary>
        public RootSample[] Root = new RootSample[0];

        /// <summary>Defensive state, one per frame.</summary>
        public HurtState[] Hurt = new HurtState[0];

        /// <summary>Key poses in frame order.</summary>
        public List<KeyPose> KeyPoses = new List<KeyPose>();

        /// <summary>Move-authored cues in frame order.</summary>
        public List<TimedCue> Cues = new List<TimedCue>();

        /// <summary>Event-driven feedback per hitbox group.</summary>
        public List<ContactFeedback> Contacts = new List<ContactFeedback>();

        /// <summary>Frames on which projectiles spawn.</summary>
        public List<int> ProjectileFrames = new List<int>();

        /// <summary>Production notes (paired victim clip, air behaviour, loops).</summary>
        public List<string> Notes = new List<string>();

        /// <summary>Clip length in seconds at 60 fps.</summary>
        public double Seconds => Move.TotalFrames / 60.0;

        /// <summary>Root offset at the end of the move.</summary>
        public FixedVector2 FinalOffset => Root.Length == 0 ? FixedVector2.Zero : Root[Root.Length - 1].Offset;
    }
}

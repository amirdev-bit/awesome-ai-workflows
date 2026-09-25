using System;
using System.Collections.Generic;
using Oathsunder.Combat.Definitions;
using Oathsunder.Combat.Simulation;

namespace Oathsunder.Presentation.Animation
{
    /// <summary>Which clip to show and where in it: the frame-locked pose of a fighter.</summary>
    public readonly struct PoseSample
    {
        /// <summary>Creates a sample.</summary>
        public PoseSample(string clip, float normalizedTime)
        {
            Clip = clip;
            NormalizedTime = normalizedTime;
        }

        /// <summary>Clip (Animator state) name.</summary>
        public string Clip { get; }

        /// <summary>0..1 position in the clip.</summary>
        public float NormalizedTime { get; }
    }

    /// <summary>How a fighter-state clip is timed.</summary>
    public enum ClipDriver : byte
    {
        /// <summary>Loops with a fixed length.</summary>
        Loop,

        /// <summary>Plays once over a fixed duration from the fighter data.</summary>
        FixedDuration,

        /// <summary>Plays once over the stun the hit applied (variable).</summary>
        StunDuration,

        /// <summary>Sampled by vertical velocity: rise → apex → fall.</summary>
        VerticalVelocity,

        /// <summary>Synchronised to the holder's paired move.</summary>
        PairedHolder,
    }

    /// <summary>Production specification of one non-move clip.</summary>
    public sealed class StateClipSpec
    {
        /// <summary>Clip name.</summary>
        public string Clip = "";

        /// <summary>Fighter action(s) that play it.</summary>
        public string Actions = "";

        /// <summary>Timing driver.</summary>
        public ClipDriver Driver;

        /// <summary>Authored length in frames (for stun-driven clips: the reference length to author at).</summary>
        public int Frames;

        /// <summary>What the clip must show.</summary>
        public string Content = "";
    }

    /// <summary>
    /// The frame-locked playback contract for every fighter state: moves play their own clip over their frame
    /// count, stuns play over the stun the hit applied, jumps are sampled by vertical velocity, and paired victims
    /// play the holder's victim clip on the holder's frame. The pose is a pure function of simulation state, so
    /// hitstop, rollback and replays look exact. Engine-free so it is unit-tested with the simulation.
    /// </summary>
    public static class FighterPoseSampler
    {
        /// <summary>Idle loop length (frames).</summary>
        public const int IdleLoopFrames = 120;

        /// <summary>Walk loop length (frames): four steps.</summary>
        public const int WalkLoopFrames = 60;

        /// <summary>Crouch loop length (frames).</summary>
        public const int CrouchLoopFrames = 120;

        /// <summary>Guard loop length (frames).</summary>
        public const int GuardLoopFrames = 90;

        /// <summary>Authored length of velocity-sampled clips (rise → apex → fall).</summary>
        public const int AirborneClipFrames = 30;

        /// <summary>Reference length for stun-driven reaction clips (time-warped to the actual stun).</summary>
        public const int ReactionReferenceFrames = 20;

        /// <summary>Samples the pose of a fighter for rendering.</summary>
        /// <param name="world">Running encounter.</param>
        /// <param name="index">Fighter index.</param>
        /// <param name="alpha">Render interpolation (0..1) between the previous and current tick.</param>
        public static PoseSample Sample(CombatWorld world, int index, float alpha)
        {
            ref FighterState f = ref world.State.Fighters[index];
            var blueprint = world.BlueprintOf(index);
            var body = blueprint.Body;
            float a = f.HitstopRemaining > 0 ? 0f : Clamp01(alpha);
            switch (f.Action)
            {
                case FighterAction.Move when f.MoveIndex >= 0:
                    var move = blueprint.Moves[f.MoveIndex];
                    return new PoseSample(ClipOf(move), Over(f.ActionFrame - 1 + a, move.TotalFrames));
                case FighterAction.Idle:
                    return Loop("A_Fighter_Idle", f.ActionFrame, a, IdleLoopFrames);
                case FighterAction.WalkForward:
                    return Loop("A_Fighter_WalkForward", f.ActionFrame, a, WalkLoopFrames);
                case FighterAction.WalkBackward:
                    return Loop("A_Fighter_WalkBackward", f.ActionFrame, a, WalkLoopFrames);
                case FighterAction.Crouch:
                    return Loop("A_Fighter_Crouch", f.ActionFrame, a, CrouchLoopFrames);
                case FighterAction.GuardStand:
                    return Loop("A_Fighter_GuardStand", f.ActionFrame, a, GuardLoopFrames);
                case FighterAction.GuardCrouch:
                    return Loop("A_Fighter_GuardCrouch", f.ActionFrame, a, GuardLoopFrames);
                case FighterAction.PreJump:
                    return new PoseSample("A_Fighter_PreJump", Over(f.ActionFrame + a, body.PreJumpFrames));
                case FighterAction.Airborne:
                    return new PoseSample("A_Fighter_Airborne", Vertical(ref f, body));
                case FighterAction.Launched:
                    return new PoseSample("A_Fighter_Launched", Vertical(ref f, body));
                case FighterAction.Landing:
                    return new PoseSample("A_Fighter_Landing", Stun(ref f, a));
                case FighterAction.ParryRecovery:
                    return new PoseSample("A_Fighter_ParryRecovery", Over(f.ActionFrame + a, world.Setup.Tuning.ParryRecoveryFrames));
                case FighterAction.Blockstun:
                    return new PoseSample(f.CrouchGuard ? "A_Fighter_BlockstunCrouch" : "A_Fighter_Blockstun", Stun(ref f, a));
                case FighterAction.Hitstun:
                    return new PoseSample("A_Fighter_Hitstun", Stun(ref f, a));
                case FighterAction.AirHitstun:
                    return new PoseSample("A_Fighter_AirHitstun", Stun(ref f, a));
                case FighterAction.Staggered:
                    return new PoseSample(f.Stagger == StaggerKind.Parried ? "A_Fighter_Stagger_Parried" : "A_Fighter_Stagger_GuardBreak", Stun(ref f, a));
                case FighterAction.Knockdown:
                    return f.HardKnockdown
                        ? new PoseSample("A_Fighter_Knockdown_Hard", Over(f.ActionFrame + a, body.HardKnockdownFrames))
                        : new PoseSample("A_Fighter_Knockdown_Soft", Over(f.ActionFrame + a, body.SoftKnockdownFrames));
                case FighterAction.WakeUp:
                    return new PoseSample("A_Fighter_WakeUp", Over(f.ActionFrame + a, body.WakeUpFrames));
                case FighterAction.PairedVictim:
                    return PairedVictim(world, ref f, a);
                case FighterAction.KnockedOut:
                    return new PoseSample("A_Fighter_KnockedOut", Over(f.ActionFrame + a, world.Setup.Rules.KnockoutHoldFrames));
                default:
                    return Loop("A_Fighter_Idle", f.ActionFrame, a, IdleLoopFrames);
            }
        }

        /// <summary>Clip name of a move (its animation key, or its id when none is set).</summary>
        public static string ClipOf(MoveDefinition move) => string.IsNullOrEmpty(move.Animation) ? move.Id : move.Animation;

        /// <summary>The non-move clip library a fighter needs, with lengths from its body data.</summary>
        public static List<StateClipSpec> StateClips(FighterDefinition body, CombatTuning tuning, MatchRules rules)
        {
            return new List<StateClipSpec>
            {
                Spec("A_Fighter_Idle", "Idle", ClipDriver.Loop, IdleLoopFrames, "Breathing combat guard, weapon ready; loop seamless; the pose every move starts and ends on."),
                Spec("A_Fighter_WalkForward", "WalkForward", ClipDriver.Loop, WalkLoopFrames, $"Four guarded steps covering {Metres(body.WalkForwardSpeed, WalkLoopFrames)} m (walk speed × loop), feet locked to the floor."),
                Spec("A_Fighter_WalkBackward", "WalkBackward", ClipDriver.Loop, WalkLoopFrames, $"Four retreating steps covering {Metres(body.WalkBackwardSpeed, WalkLoopFrames)} m, guard kept high."),
                Spec("A_Fighter_Crouch", "Crouch", ClipDriver.Loop, CrouchLoopFrames, "Low stance under the crouching hurtbox height; loop."),
                Spec("A_Fighter_GuardStand", "GuardStand", ClipDriver.Loop, GuardLoopFrames, "Standing block: weapon across the body, braced; loop."),
                Spec("A_Fighter_GuardCrouch", "GuardCrouch", ClipDriver.Loop, GuardLoopFrames, "Crouching block: low guard covering the legs; loop."),
                Spec("A_Fighter_PreJump", "PreJump", ClipDriver.FixedDuration, body.PreJumpFrames, "Knees load before take-off; last frame is the push-off."),
                Spec("A_Fighter_Airborne", "Airborne", ClipDriver.VerticalVelocity, AirborneClipFrames, "Jump strip sampled by vertical velocity: frame 1 = take-off, middle = apex tuck, last = falling ready to land."),
                Spec("A_Fighter_Landing", "Landing", ClipDriver.FixedDuration, body.LandingFrames, "Landing absorb (also used, time-warped, for the longer land-cancel recoveries)."),
                Spec("A_Fighter_ParryRecovery", "ParryRecovery", ClipDriver.FixedDuration, tuning.ParryRecoveryFrames, "Deflection follow-through after a perfect parry; returns to guard."),
                Spec("A_Fighter_Blockstun", "Blockstun", ClipDriver.StunDuration, ReactionReferenceFrames, "Standing block impact: recoil in the first 4 frames, hold, recover to guard at the end."),
                Spec("A_Fighter_BlockstunCrouch", "Blockstun (crouch guard)", ClipDriver.StunDuration, ReactionReferenceFrames, "Crouching block impact."),
                Spec("A_Fighter_Hitstun", "Hitstun", ClipDriver.StunDuration, ReactionReferenceFrames, "Grounded hit reaction: snap in 3 frames, hold, recover; Phase 7 layers height/weight variants on top."),
                Spec("A_Fighter_AirHitstun", "AirHitstun", ClipDriver.StunDuration, ReactionReferenceFrames, "Hit in the air: body jolts, recovers to the airborne fall pose."),
                Spec("A_Fighter_Launched", "Launched", ClipDriver.VerticalVelocity, AirborneClipFrames, "Juggle tumble sampled by vertical velocity: rising spin, apex, falling face-up."),
                Spec("A_Fighter_Stagger_GuardBreak", "Staggered (guard break)", ClipDriver.StunDuration, ReactionReferenceFrames, "Guard shattered: weapon knocked wide, off-balance, open to an execution."),
                Spec("A_Fighter_Stagger_Parried", "Staggered (parried heavy)", ClipDriver.StunDuration, ReactionReferenceFrames, "Heavy attack deflected: weapon bounced back, body twisted open."),
                Spec("A_Fighter_Knockdown_Soft", "Knockdown (soft)", ClipDriver.FixedDuration, body.SoftKnockdownFrames, "Fall and lie; the first frames overlap the tech-roll window, so the body must read as 'can still roll'."),
                Spec("A_Fighter_Knockdown_Hard", "Knockdown (hard)", ClipDriver.FixedDuration, body.HardKnockdownFrames, "Slammed flat, longer lie; no tech possible."),
                Spec("A_Fighter_WakeUp", "WakeUp", ClipDriver.FixedDuration, body.WakeUpFrames, "Rise to guard (fully invulnerable); last frame matches A_Fighter_Idle frame 1."),
                Spec("A_Fighter_PairedVictim", "PairedVictim (fallback)", ClipDriver.Loop, 60, "Generic held pose used only if a paired move has no victim clip."),
                Spec("A_Fighter_KnockedOut", "KnockedOut", ClipDriver.FixedDuration, rules.KnockoutHoldFrames, "Defeat collapse played over the knock-out hold."),
            };
        }

        private static PoseSample PairedVictim(CombatWorld world, ref FighterState f, float a)
        {
            int holder = f.PairedPartner;
            if (holder >= 0 && holder < world.FighterCount)
            {
                ref FighterState h = ref world.State.Fighters[holder];
                if (h.Action == FighterAction.Move && h.MoveIndex >= 0)
                {
                    var move = world.BlueprintOf(holder).Moves[h.MoveIndex];
                    if (move.Paired != null && !string.IsNullOrEmpty(move.Paired.VictimAnimation))
                    {
                        float ha = h.HitstopRemaining > 0 ? 0f : a;
                        return new PoseSample(move.Paired.VictimAnimation, Over(h.ActionFrame - 1 + ha, move.TotalFrames));
                    }
                }
            }

            return Loop("A_Fighter_PairedVictim", f.ActionFrame, a, 60);
        }

        private static float Stun(ref FighterState f, float a)
        {
            int total = f.ActionFrame + f.StunRemaining + 1;
            return Over(f.ActionFrame + a, total);
        }

        private static float Vertical(ref FighterState f, FighterDefinition body)
        {
            double up = body.JumpVelocityY.ToDouble();
            double down = body.MaxFallSpeed.ToDouble();
            if (up + down <= 0)
            {
                return 0f;
            }

            return Clamp01((float)((up - f.Velocity.Y.ToDouble()) / (up + down)));
        }

        private static PoseSample Loop(string clip, int frame, float a, int length) =>
            new PoseSample(clip, ((frame % length) + a) / length % 1f);

        private static float Over(float frame, int length) => length <= 0 ? 1f : Clamp01(frame / length);

        private static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;

        private static StateClipSpec Spec(string clip, string actions, ClipDriver driver, int frames, string content) =>
            new StateClipSpec { Clip = clip, Actions = actions, Driver = driver, Frames = Math.Max(1, frames), Content = content };

        private static string Metres(Oathsunder.Core.Mathematics.Fixed perFrame, int frames) =>
            (perFrame.ToDouble() * frames).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
    }
}

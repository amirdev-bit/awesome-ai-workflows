using System;
using System.Collections.Generic;
using Oathsunder.Combat.Definitions;
using Oathsunder.Combat.Input;
using Oathsunder.Combat.Simulation;
using Oathsunder.Core.Mathematics;
using Oathsunder.Presentation.Cues;

namespace Oathsunder.Presentation.Animation
{
    /// <summary>
    /// Builds <see cref="MoveAnimationSpec"/>s by running each move in an isolated simulation and recording what
    /// the engine actually does frame by frame, so the animation contract can never drift from gameplay.
    /// </summary>
    /// <remarks>
    /// The subject stands at the origin facing right with a passive dummy 40 m away on an unbounded capture
    /// stage. Air moves start from the apex of a neutral jump. Buttons the move waits to see released (charge
    /// holds) are held for the whole capture. Paired moves are captured without a partner: the attacker's root
    /// is what the clip needs, and the victim is locked to the data's victim offset.
    /// </remarks>
    public static class MoveAnimationCapture
    {
        private const int DummyDistance = 40;
        private const int MaxJumpFrames = 90;

        /// <summary>Captures every move of a blueprint, in blueprint order.</summary>
        public static List<MoveAnimationSpec> CaptureAll(FighterBlueprint blueprint, CombatTuning tuning)
        {
            var specs = new List<MoveAnimationSpec>(blueprint.Moves.Length);
            foreach (var move in blueprint.Moves)
            {
                specs.Add(Capture(blueprint, tuning, move.Id));
            }

            return specs;
        }

        /// <summary>Captures one move.</summary>
        /// <exception cref="ArgumentException">The blueprint has no such move.</exception>
        public static MoveAnimationSpec Capture(FighterBlueprint blueprint, CombatTuning tuning, string moveId)
        {
            var move = blueprint.FindMove(moveId) ?? throw new ArgumentException($"No move '{moveId}'.", nameof(moveId));
            var world = CreateWorld(blueprint, tuning);
            var inputs = new[] { InputFrame.Neutral, InputFrame.Neutral };
            world.Step(inputs);

            ref FighterState subject = ref world.State.Fighters[0];
            ref FighterState dummy = ref world.State.Fighters[1];
            subject.Position = FixedVector2.Zero;
            subject.Facing = 1;
            dummy.Position = new FixedVector2(Fixed.FromInt(DummyDistance), Fixed.Zero);
            dummy.Facing = -1;

            bool air = StartsAirborne(move);
            if (air)
            {
                inputs[0] = new InputFrame(InputButtons.Jump, 0, 0);
                world.Step(inputs);
                inputs[0] = InputFrame.Neutral;
                for (int i = 0; i < MaxJumpFrames; i++)
                {
                    world.Step(inputs);
                    ref FighterState f = ref world.State.Fighters[0];
                    if (!f.Grounded && f.Velocity.Y <= Fixed.Zero)
                    {
                        break;
                    }
                }
            }

            inputs[0] = new InputFrame(HeldThroughout(move), 0, 0);
            FixedVector2 start = world.State.Fighters[0].Position;
            var spec = new MoveAnimationSpec
            {
                Move = move,
                Clip = string.IsNullOrEmpty(move.Animation) ? move.Id : move.Animation,
            };

            var root = new List<RootSample>(move.TotalFrames);
            var hurt = new List<HurtState>(move.TotalFrames);
            var boxes = new FixedAabb[16];
            world.ForceMove(0, move.Id);
            string endedBy = null;
            for (int frame = 1; frame <= move.TotalFrames; frame++)
            {
                world.Step(inputs);
                ref FighterState f = ref world.State.Fighters[0];
                if (f.Action != FighterAction.Move || f.MoveIndex != move.Index)
                {
                    endedBy = f.Action == FighterAction.Move ? $"cancels into {blueprint.Moves[f.MoveIndex].Id}" : $"ends in {f.Action}";
                    spec.Notes.Add($"Capture: on frame {frame} the move {endedBy} (clip still covers all {move.TotalFrames} frames).");
                    break;
                }

                root.Add(new RootSample
                {
                    Frame = f.ActionFrame,
                    Offset = f.Position - start,
                    Velocity = f.Velocity,
                    Grounded = f.Grounded,
                });
                hurt.Add(CaptureHurt(world, move, ref f, boxes));
            }

            spec.Root = root.ToArray();
            spec.Hurt = hurt.ToArray();
            spec.RootMotion = move.Paired != null ? RootMotionKind.Paired
                : air ? RootMotionKind.Ballistic
                : Moves(spec.Root) ? RootMotionKind.Scripted
                : RootMotionKind.InPlace;
            FillTiming(spec);
            FillCues(spec);
            FillContacts(spec);
            FillKeyPoses(spec);
            FillNotes(spec, air, blueprint);
            return spec;
        }

        private static CombatWorld CreateWorld(FighterBlueprint blueprint, CombatTuning tuning)
        {
            var setup = new CombatSetup
            {
                Stage = new StageDefinition
                {
                    Id = "stage.capture",
                    LeftWall = Fixed.FromInt(-200),
                    RightWall = Fixed.FromInt(200),
                    MaxSeparation = Fixed.FromInt(400),
                    WallBounceEnabled = false,
                },
                Tuning = tuning ?? new CombatTuning(),
                Rules = new MatchRules
                {
                    Id = "rules.capture",
                    RoundsToWin = 1,
                    RoundTimerFrames = 0,
                    PreRoundFrames = 0,
                    UseRpgStats = false,
                },
            };
            setup.Combatants.Add(new CombatantSetup(blueprint, team: 0));
            setup.Combatants.Add(new CombatantSetup(blueprint, team: 1));
            return new CombatWorld(setup);
        }

        private static bool StartsAirborne(MoveDefinition move)
        {
            if (move.HasTag(MoveTags.Air))
            {
                return true;
            }

            if (move.Triggers.Length == 0)
            {
                return false;
            }

            foreach (var trigger in move.Triggers)
            {
                if ((trigger.Stance & TriggerStance.Grounded) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static InputButtons HeldThroughout(MoveDefinition move)
        {
            var held = InputButtons.None;
            foreach (var cancel in move.Cancels)
            {
                if (cancel.Input == CancelInput.Released)
                {
                    held |= cancel.InputButton;
                }
            }

            return held;
        }

        private static bool Moves(RootSample[] root)
        {
            foreach (var sample in root)
            {
                if (sample.Offset != FixedVector2.Zero)
                {
                    return true;
                }
            }

            return false;
        }

        private static HurtState CaptureHurt(CombatWorld world, MoveDefinition move, ref FighterState f, FixedAabb[] buffer)
        {
            int count = world.GetHurtboxes(0, buffer);
            var state = new HurtState
            {
                Frame = f.ActionFrame,
                Stance = !f.Grounded ? "airborne" : move.HasFlag(MoveFlags.CrouchingHurtbox) ? "crouching" : "standing",
                Boxes = new FixedAabb[count],
                PerfectEvade = move.PerfectEvade.HasValue && move.PerfectEvade.Value.Contains(f.ActionFrame),
                CounterStance = move.Counter != null && move.Counter.Window.Contains(f.ActionFrame),
            };

            for (int i = 0; i < count; i++)
            {
                state.Boxes[i] = new FixedAabb(buffer[i].Min - f.Position, buffer[i].Max - f.Position);
            }

            foreach (var mask in new[] { InvulnerabilityMask.Strike, InvulnerabilityMask.Throw, InvulnerabilityMask.Projectile })
            {
                if (world.IsInvulnerable(0, mask))
                {
                    state.Invulnerable |= mask;
                }
            }

            foreach (var armor in move.Armor)
            {
                state.Armor |= armor.Window.Contains(f.ActionFrame);
            }

            return state;
        }

        private static void FillTiming(MoveAnimationSpec spec)
        {
            var move = spec.Move;
            spec.Phases = new MovePhase[move.TotalFrames];
            for (int frame = 1; frame <= move.TotalFrames; frame++)
            {
                MovePhase phase;
                if (!move.IsAttack)
                {
                    phase = MovePhase.Action;
                }
                else if (frame < move.FirstActiveFrame)
                {
                    phase = MovePhase.Startup;
                }
                else if (frame > move.LastActiveFrame)
                {
                    phase = MovePhase.Recovery;
                }
                else
                {
                    phase = MovePhase.Gap;
                    foreach (var hitbox in move.Hitboxes)
                    {
                        if (hitbox.Window.Contains(frame))
                        {
                            phase = MovePhase.Active;
                            break;
                        }
                    }
                }

                spec.Phases[frame - 1] = phase;
            }

            if (move.IsAttack)
            {
                spec.Startup = move.FirstActiveFrame - 1;
                spec.ActiveSpan = move.LastActiveFrame - move.FirstActiveFrame + 1;
                spec.Recovery = move.TotalFrames - move.LastActiveFrame;
            }
        }

        private static void FillCues(MoveAnimationSpec spec)
        {
            foreach (var cue in spec.Move.Cues)
            {
                if (CueNames.TryParse(cue.Name, out var channel))
                {
                    spec.Cues.Add(new TimedCue { Frame = cue.Frame, Name = cue.Name, Channel = channel });
                }
            }

            spec.Cues.Sort((a, b) => a.Frame.CompareTo(b.Frame));
        }

        private static void FillContacts(MoveAnimationSpec spec)
        {
            var move = spec.Move;
            var groups = new SortedDictionary<int, ContactFeedback>();
            foreach (var hitbox in move.Hitboxes)
            {
                var attack = move.Attacks[hitbox.AttackIndex];
                if (!groups.TryGetValue(hitbox.Group, out var contact))
                {
                    bool grab = attack.Has(AttackFlags.Grab | AttackFlags.AirGrab | AttackFlags.Execution);
                    bool heavy = move.IsHeavyAttack(attack);
                    contact = new ContactFeedback
                    {
                        Group = hitbox.Group,
                        Window = hitbox.Window,
                        Attack = attack.Key,
                        OnHit = grab ? EventCueMap.Grab : heavy ? EventCueMap.HitHeavy : EventCueMap.HitLight,
                        OnCounterHit = grab ? "" : EventCueMap.HitCounter,
                        OnBlock = grab ? "" : heavy ? EventCueMap.BlockHeavy : EventCueMap.Block,
                        Hitstop = attack.Hitstop,
                    };
                    groups.Add(hitbox.Group, contact);
                }
                else
                {
                    contact.Window = new FrameWindow(Math.Min(contact.Window.Start, hitbox.Window.Start), Math.Max(contact.Window.End, hitbox.Window.End));
                }
            }

            spec.Contacts.AddRange(groups.Values);
            foreach (var spawn in move.Projectiles)
            {
                spec.ProjectileFrames.Add(spawn.Frame);
            }
        }

        private static void FillKeyPoses(MoveAnimationSpec spec)
        {
            var move = spec.Move;
            var poses = new List<KeyPose>();
            void Add(int frame, string name, string purpose)
            {
                if (frame >= 1 && frame <= move.TotalFrames)
                {
                    poses.Add(new KeyPose { Frame = frame, Name = name, Purpose = purpose });
                }
            }

            Add(1, "Start", "First frame of the move; readable from neutral (blend-in is at most 2 frames).");
            int previousEnd = 0;
            foreach (var contact in spec.Contacts)
            {
                int windup = contact.Window.Start - previousEnd - 1;
                if (windup >= 2)
                {
                    int anticipation = previousEnd + 1 + (int)Math.Round(windup * 0.6, MidpointRounding.AwayFromZero);
                    Add(Math.Min(anticipation, contact.Window.Start - 1), contact.Group == 0 ? "Anticipation" : $"Anticipation {contact.Group + 1}",
                        "Wind-up extreme: weapon fully loaded, weight on the back foot; the silhouette telegraphs the attack.");
                }

                Add(contact.Window.Start, contact.Group == 0 ? "Contact" : $"Contact {contact.Group + 1}",
                    "Strike pose: the weapon edge fills the hitbox on this frame; this pose is frozen during hitstop.");
                if (contact.Window.End + 1 <= move.TotalFrames)
                {
                    Add(contact.Window.End + 1, contact.Group == 0 ? "Follow-through" : $"Follow-through {contact.Group + 1}",
                        "Overshoot past the target; the hitbox is gone, the pose sells momentum.");
                }

                previousEnd = contact.Window.End;
            }

            if (move.IsAttack && spec.Recovery >= 4)
            {
                Add(move.LastActiveFrame + 1 + (int)Math.Round(spec.Recovery * 0.6, MidpointRounding.AwayFromZero), "Recovery hold",
                    "Vulnerable recovery: open guard, readable at 3 m so opponents see the punish window.");
            }

            foreach (var segment in move.Motion)
            {
                if (segment.SetX && segment.Window.Start > 1)
                {
                    Add(segment.Window.Start, "Push-off", "Root motion starts: foot drives off the floor.");
                }

                if (segment.SetX && segment.Window.End < move.TotalFrames)
                {
                    Add(segment.Window.End + 1, "Plant", "Root motion ends: foot plants and slides to rest.");
                }
            }

            foreach (int frame in spec.ProjectileFrames)
            {
                Add(frame, "Release", "Projectile leaves the weapon tip on this frame.");
            }

            if (move.Counter != null)
            {
                Add(move.Counter.Window.Start, "Stance set", "Counter stance is live from this frame.");
                Add(move.Counter.Window.End, "Stance end", "Last catching frame; relax the stance after it.");
            }

            if (move.PerfectEvade.HasValue)
            {
                var window = move.PerfectEvade.Value;
                Add(window.Start + (window.Length - 1) / 2, "Evade extreme", "Body at its furthest from the attack line (perfect-dodge window).");
            }

            if (move.Paired != null)
            {
                int n = 1;
                foreach (var hit in move.Paired.Hits)
                {
                    Add(hit.Frame, move.Paired.Hits.Length == 1 ? "Impact" : $"Strike {n}", "Scripted hit on the locked victim.");
                    n++;
                }

                Add(move.Paired.ReleaseFrame, "Release", "Victim is released and takes the release attack.");
            }

            Add(move.TotalFrames, "End", "Settles into the idle guard; the last frame blends to A_Fighter_Idle frame 1.");

            poses.Sort((a, b) => a.Frame.CompareTo(b.Frame));
            for (int i = 0; i < poses.Count; i++)
            {
                if (i > 0 && poses[i].Frame == spec.KeyPoses[spec.KeyPoses.Count - 1].Frame)
                {
                    var last = spec.KeyPoses[spec.KeyPoses.Count - 1];
                    last.Name += " / " + poses[i].Name;
                    last.Purpose += " " + poses[i].Purpose;
                    continue;
                }

                spec.KeyPoses.Add(poses[i]);
            }
        }

        private static void FillNotes(MoveAnimationSpec spec, bool air, FighterBlueprint blueprint)
        {
            var move = spec.Move;
            if (air)
            {
                spec.Notes.Add("Air move captured from the apex of a neutral jump; the root follows jump physics, so author in place relative to the root.");
            }

            if (move.HasFlag(MoveFlags.LandCancel))
            {
                spec.Notes.Add($"Land-cancel: touching the floor ends the clip early and plays landing recovery (+{move.LandingRecovery} frames).");
            }

            if (move.HasFlag(MoveFlags.CrouchingHurtbox))
            {
                spec.Notes.Add("Crouching hurtbox: the pose must stay below the crouch box height for the whole move.");
            }

            if (move.Paired != null)
            {
                var paired = move.Paired;
                spec.Notes.Add($"Paired: victim clip `{paired.VictimAnimation}` has the same {move.TotalFrames} frames and is authored relative to the attacker root at offset ({paired.VictimOffset.X}, {paired.VictimOffset.Y}) m; release on frame {paired.ReleaseFrame}.");
                if (paired.Cinematic)
                {
                    spec.Notes.Add("Cinematic: an authored camera sequence plays; world time is frozen for everyone else.");
                }
            }

            int earliestCancel = int.MaxValue;
            foreach (var cancel in move.Cancels)
            {
                earliestCancel = Math.Min(earliestCancel, cancel.Window.Start);
            }

            if (earliestCancel != int.MaxValue)
            {
                spec.Notes.Add($"Earliest cancel on frame {earliestCancel}: from here the clip may be cut, so poses must transition cleanly.");
            }

            foreach (var spawn in move.Projectiles)
            {
                var projectile = blueprint.Projectiles[spawn.ProjectileIndex];
                spec.Notes.Add($"Projectile `{projectile.Id}` ({projectile.Visual}) spawns on frame {spawn.Frame} at ({spawn.Offset.X}, {spawn.Offset.Y}) m from the root.");
            }
        }
    }
}

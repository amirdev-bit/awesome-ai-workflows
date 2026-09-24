using System;
using System.Collections.Generic;
using System.Globalization;
using Oathsunder.Combat.Definitions;
using Oathsunder.Combat.Input;
using Oathsunder.Core.Mathematics;
using Oathsunder.Core.Serialization;

namespace Oathsunder.Combat.Content
{
    /// <summary>
    /// Converts combat JSON documents into definitions. Designer-facing units are SI (metres, metres per second,
    /// metres per second squared) and are converted exactly to simulation units (per 60 Hz frame) without floats.
    /// Frames are 1-based and inclusive. See Documentation/05-CoreCombat.md for the full schema.
    /// </summary>
    public static class CombatContentParser
    {
        /// <summary>Simulation ticks per second.</summary>
        public const int TicksPerSecond = 60;

        private const int TicksPerSecondSquared = TicksPerSecond * TicksPerSecond;

        /// <summary>Parses a fighter body document.</summary>
        public static FighterDefinition ParseFighter(string json, string sourceName) => ParseFighter(JsonReader.Parse(json, sourceName));

        /// <summary>Parses a fighter body node.</summary>
        public static FighterDefinition ParseFighter(JsonNode root)
        {
            root.RequireKind(JsonKind.Object);
            var fighter = new FighterDefinition
            {
                Id = root.Require("id").AsString(),
                Name = root.GetString("name", ""),
                MaxHealth = root.GetInt("maxHealth", 1000),
                MaxPosture = root.GetInt("maxPosture", 1000),
                WalkForwardSpeed = Speed(root.Require("walkForwardSpeed")),
                WalkBackwardSpeed = Speed(root.Require("walkBackwardSpeed")),
                PreJumpFrames = root.GetInt("preJumpFrames", 4),
                JumpVelocityY = Speed(root.Require("jumpVelocity")),
                JumpForwardVelocityX = Speed(root.Require("jumpForwardSpeed")),
                JumpBackwardVelocityX = Speed(root.Require("jumpBackwardSpeed")),
                Gravity = Acceleration(root.Require("gravity")),
                MaxFallSpeed = Speed(root.Require("maxFallSpeed")),
                GroundFriction = Acceleration(root.Require("groundFriction")),
                LandingFrames = root.GetInt("landingFrames", 3),
                SoftKnockdownFrames = root.GetInt("softKnockdownFrames", 30),
                HardKnockdownFrames = root.GetInt("hardKnockdownFrames", 50),
                WakeUpFrames = root.GetInt("wakeUpFrames", 20),
                KnockbackPermille = root.GetInt("knockbackPermille", 1000),
            };

            var stances = root.Require("stances");
            fighter.Standing = ParseStance(stances.Require("standing"));
            fighter.Crouching = ParseStance(stances.Require("crouching"));
            fighter.Airborne = ParseStance(stances.Require("airborne"));
            fighter.Knockdown = ParseStance(stances.Require("knockdown"));
            return fighter;
        }

        /// <summary>Parses a move-set document.</summary>
        public static MoveSetDefinition ParseMoveSet(string json, string sourceName) => ParseMoveSet(JsonReader.Parse(json, sourceName));

        /// <summary>Parses a move-set node.</summary>
        public static MoveSetDefinition ParseMoveSet(JsonNode root)
        {
            root.RequireKind(JsonKind.Object);
            var set = new MoveSetDefinition
            {
                Id = root.Require("id").AsString(),
                DisplayName = root.GetString("name", ""),
            };

            var projectiles = root.Get("projectiles");
            if (projectiles != null)
            {
                projectiles.RequireKind(JsonKind.Array);
                foreach (var node in projectiles.Items)
                {
                    set.Projectiles.Add(ParseProjectile(node));
                }
            }

            var moves = root.Require("moves");
            moves.RequireKind(JsonKind.Array);
            foreach (var node in moves.Items)
            {
                set.Moves.Add(ParseMove(node));
            }

            return set;
        }

        /// <summary>Parses a stage document.</summary>
        public static StageDefinition ParseStage(string json, string sourceName)
        {
            var root = JsonReader.Parse(json, sourceName);
            root.RequireKind(JsonKind.Object);
            var stage = new StageDefinition
            {
                Id = root.Require("id").AsString(),
                Name = root.GetString("name", ""),
            };
            stage.LeftWall = root.GetFixed("leftWall", stage.LeftWall);
            stage.RightWall = root.GetFixed("rightWall", stage.RightWall);
            stage.MaxSeparation = root.GetFixed("maxSeparation", stage.MaxSeparation);
            stage.SpawnOffset = root.GetFixed("spawnOffset", stage.SpawnOffset);
            stage.WallBounceEnabled = root.GetBool("wallBounce", stage.WallBounceEnabled);
            if (stage.RightWall <= stage.LeftWall)
            {
                throw new JsonContentException(root.Path, "rightWall must be greater than leftWall");
            }

            return stage;
        }

        /// <summary>Parses a match-rules document.</summary>
        public static MatchRules ParseRules(string json, string sourceName)
        {
            var root = JsonReader.Parse(json, sourceName);
            root.RequireKind(JsonKind.Object);
            var rules = new MatchRules { Id = root.Require("id").AsString() };
            rules.RoundsToWin = root.GetInt("roundsToWin", rules.RoundsToWin);
            rules.RoundTimerFrames = root.GetInt("roundTimerSeconds", rules.RoundTimerFrames / TicksPerSecond) * TicksPerSecond;
            rules.PreRoundFrames = root.GetInt("preRoundFrames", rules.PreRoundFrames);
            rules.KnockoutHoldFrames = root.GetInt("knockoutHoldFrames", rules.KnockoutHoldFrames);
            rules.PerfectDodgeSlowPermille = OptionalPermille(root, "perfectDodgeTimeScale", rules.PerfectDodgeSlowPermille);
            rules.PerfectDodgeSlowFrames = root.GetInt("perfectDodgeSlowFrames", rules.PerfectDodgeSlowFrames);
            rules.ExecutionsEnabled = root.GetBool("executions", rules.ExecutionsEnabled);
            rules.RageBurstEnabled = root.GetBool("rageBurst", rules.RageBurstEnabled);
            rules.UseRpgStats = root.GetBool("rpgStats", rules.UseRpgStats);
            rules.CarryUltimateMeter = root.GetBool("carryUltimateMeter", rules.CarryUltimateMeter);
            rules.FriendlyFire = root.GetBool("friendlyFire", rules.FriendlyFire);
            if (rules.RoundsToWin < 1)
            {
                throw new JsonContentException(root.Path + ".roundsToWin", "must be at least 1");
            }

            return rules;
        }

        /// <summary>Parses a tuning document. Absent fields keep their shipped defaults.</summary>
        public static CombatTuning ParseTuning(string json, string sourceName)
        {
            var root = JsonReader.Parse(json, sourceName);
            root.RequireKind(JsonKind.Object);
            var t = new CombatTuning();
            t.InputBufferFrames = root.GetInt("inputBufferFrames", t.InputBufferFrames);
            t.ChordWindowFrames = root.GetInt("chordWindowFrames", t.ChordWindowFrames);
            t.SimplifiedStartupPenalty = root.GetInt("simplifiedStartupPenalty", t.SimplifiedStartupPenalty);
            t.ParryWindowFrames = root.GetInt("parryWindowFrames", t.ParryWindowFrames);
            t.ParryWindowMashFrames = root.GetInt("parryWindowMashFrames", t.ParryWindowMashFrames);
            t.ParryMashThresholdFrames = root.GetInt("parryMashThresholdFrames", t.ParryMashThresholdFrames);
            t.ParryRecoveryFrames = root.GetInt("parryRecoveryFrames", t.ParryRecoveryFrames);
            t.ParryHitstop = root.GetInt("parryHitstop", t.ParryHitstop);
            t.ParryPostureDamagePermille = root.GetInt("parryPostureDamagePermille", t.ParryPostureDamagePermille);
            t.ParryStaggerFrames = root.GetInt("parryStaggerFrames", t.ParryStaggerFrames);
            t.GuardBreakStaggerFrames = root.GetInt("guardBreakStaggerFrames", t.GuardBreakStaggerFrames);
            t.BlockHitstopPermille = root.GetInt("blockHitstopPermille", t.BlockHitstopPermille);
            t.PostureOnHitPermille = root.GetInt("postureOnHitPermille", t.PostureOnHitPermille);
            t.PostureRegenDelayFrames = root.GetInt("postureRegenDelayFrames", t.PostureRegenDelayFrames);
            t.PostureRegenPerFrame = root.GetInt("postureRegenPerFrame", t.PostureRegenPerFrame);
            t.ThrowInvulnerableAfterStunFrames = root.GetInt("throwInvulnerableAfterStunFrames", t.ThrowInvulnerableAfterStunFrames);
            t.ThrowTechPushback = root.GetFixedScaled("throwTechPushback", TicksPerSecond, t.ThrowTechPushback);
            t.ThrowTechRecoveryFrames = root.GetInt("throwTechRecoveryFrames", t.ThrowTechRecoveryFrames);
            t.CounterStanceHitstop = root.GetInt("counterStanceHitstop", t.CounterStanceHitstop);
            t.ArmorHitstop = root.GetInt("armorHitstop", t.ArmorHitstop);
            t.KnockoutHitstop = root.GetInt("knockoutHitstop", t.KnockoutHitstop);
            t.CounterHitDamagePermille = root.GetInt("counterHitDamagePermille", t.CounterHitDamagePermille);
            t.CounterHitHitstunBonus = root.GetInt("counterHitHitstunBonus", t.CounterHitHitstunBonus);
            t.CounterHitHitstopBonus = root.GetInt("counterHitHitstopBonus", t.CounterHitHitstopBonus);
            t.PunishDamagePermille = root.GetInt("punishDamagePermille", t.PunishDamagePermille);
            t.PunishHitstunBonus = root.GetInt("punishHitstunBonus", t.PunishHitstunBonus);
            t.ComboScalingStepPermille = root.GetInt("comboScalingStepPermille", t.ComboScalingStepPermille);
            t.MinComboScalingPermille = root.GetInt("minComboScalingPermille", t.MinComboScalingPermille);
            t.HitstunDecayStartHit = root.GetInt("hitstunDecayStartHit", t.HitstunDecayStartHit);
            t.HitstunDecayEveryHits = Math.Max(1, root.GetInt("hitstunDecayEveryHits", t.HitstunDecayEveryHits));
            t.MinHitstun = root.GetInt("minHitstun", t.MinHitstun);
            t.MaxJugglePoints = root.GetInt("maxJugglePoints", t.MaxJugglePoints);
            t.JuggleGravityPerHitPermille = root.GetInt("juggleGravityPerHitPermille", t.JuggleGravityPerHitPermille);
            t.MaxJuggleGravityPermille = root.GetInt("maxJuggleGravityPermille", t.MaxJuggleGravityPermille);
            t.GroundBounceVelocity = root.GetFixedScaled("groundBounceVelocity", TicksPerSecond, t.GroundBounceVelocity);
            t.WallBounceRestitutionPermille = root.GetInt("wallBounceRestitutionPermille", t.WallBounceRestitutionPermille);
            t.WallBounceUpVelocity = root.GetFixedScaled("wallBounceUpVelocity", TicksPerSecond, t.WallBounceUpVelocity);
            if (root.Has("tripVelocity"))
            {
                t.TripVelocity = Velocity(root.Get("tripVelocity"));
            }

            if (root.Has("knockoutLaunchVelocity"))
            {
                t.KnockoutLaunchVelocity = Velocity(root.Get("knockoutLaunchVelocity"));
            }

            t.AirResetLandingFrames = root.GetInt("airResetLandingFrames", t.AirResetLandingFrames);
            t.TechWindowFrames = root.GetInt("techWindowFrames", t.TechWindowFrames);
            t.MaxAirActions = root.GetInt("maxAirActions", t.MaxAirActions);
            t.MeterMax = root.GetInt("meterMax", t.MeterMax);
            t.UltimateGainDealtPermille = root.GetInt("ultimateGainDealtPermille", t.UltimateGainDealtPermille);
            t.UltimateGainTakenPermille = root.GetInt("ultimateGainTakenPermille", t.UltimateGainTakenPermille);
            t.RageGainTakenPermille = root.GetInt("rageGainTakenPermille", t.RageGainTakenPermille);
            t.RageGainDealtPermille = root.GetInt("rageGainDealtPermille", t.RageGainDealtPermille);
            t.ShadowGainPerHit = root.GetInt("shadowGainPerHit", t.ShadowGainPerHit);
            t.ShadowGainPerfectParry = root.GetInt("shadowGainPerfectParry", t.ShadowGainPerfectParry);
            t.ShadowGainPerfectDodge = root.GetInt("shadowGainPerfectDodge", t.ShadowGainPerfectDodge);
            t.UltimateGainOnBlock = root.GetInt("ultimateGainOnBlock", t.UltimateGainOnBlock);
            t.RageDurationFrames = root.GetInt("rageDurationFrames", t.RageDurationFrames);
            t.RageDamagePermille = root.GetInt("rageDamagePermille", t.RageDamagePermille);
            t.RageHeavyArmorHits = root.GetInt("rageHeavyArmorHits", t.RageHeavyArmorHits);
            t.ShadowDurationFrames = root.GetInt("shadowDurationFrames", t.ShadowDurationFrames);
            t.ShadowEchoDelayFrames = root.GetInt("shadowEchoDelayFrames", t.ShadowEchoDelayFrames);
            t.ShadowEchoDamagePermille = root.GetInt("shadowEchoDamagePermille", t.ShadowEchoDamagePermille);
            t.ShadowEchoHitstunBonus = root.GetInt("shadowEchoHitstunBonus", t.ShadowEchoHitstunBonus);
            t.ShadowEchoHitstop = root.GetInt("shadowEchoHitstop", t.ShadowEchoHitstop);
            t.ExecutionHealthPermille = root.GetInt("executionHealthPermille", t.ExecutionHealthPermille);
            t.ExecutionRange = root.GetFixed("executionRange", t.ExecutionRange);
            return t;
        }

        private static StanceBoxes ParseStance(JsonNode node)
        {
            node.RequireKind(JsonKind.Object);
            var hurtboxes = node.Require("hurtboxes");
            hurtboxes.RequireKind(JsonKind.Array);
            var boxes = new FixedAabb[hurtboxes.Count];
            for (int i = 0; i < boxes.Length; i++)
            {
                boxes[i] = Box(hurtboxes.Items[i]);
            }

            return new StanceBoxes { Hurtboxes = boxes, Pushbox = Box(node.Require("pushbox")) };
        }

        private static ProjectileDefinition ParseProjectile(JsonNode node)
        {
            node.RequireKind(JsonKind.Object);
            var projectile = new ProjectileDefinition
            {
                Id = node.Require("id").AsString(),
                Lifetime = node.GetInt("lifetime", 60),
                Velocity = Velocity(node.Require("velocity")),
                Box = Box(node.Require("box")),
                Hits = node.GetInt("hits", 1),
                RehitInterval = node.GetInt("rehitInterval", 0),
                Visual = node.GetString("visual", ""),
                Attack = ParseAttack("projectile", node.Require("attack")),
            };
            return projectile;
        }

        private static MoveDefinition ParseMove(JsonNode node)
        {
            node.RequireKind(JsonKind.Object);
            var move = new MoveDefinition
            {
                Id = node.Require("id").AsString(),
                Name = node.GetString("name", ""),
                TotalFrames = node.Require("frames").AsInt(),
                LandingRecovery = node.GetInt("landingRecovery", 0),
                Animation = node.GetString("animation", ""),
                Tags = ParseFlags<MoveTags>(node.Get("tags")),
                Flags = ParseFlags<MoveFlags>(node.Get("flags")),
            };

            var attackKeys = new List<string>();
            var attacks = new List<AttackSpec>();
            var attacksNode = node.Get("attacks");
            if (attacksNode != null)
            {
                attacksNode.RequireKind(JsonKind.Object);
                foreach (var member in attacksNode.Members)
                {
                    attackKeys.Add(member.Key);
                    attacks.Add(ParseAttack(member.Key, member.Value));
                }
            }

            move.Attacks = attacks.ToArray();

            move.Triggers = ParseArray(node.Get("triggers"), ParseTrigger);
            move.Hitboxes = ParseArray(node.Get("hitboxes"), h => ParseHitbox(h, attackKeys));
            move.Hurtboxes = ParseArray(node.Get("hurtboxes"), ParseHurtboxWindow);
            move.Invulnerability = ParseArray(node.Get("invulnerable"), n => new InvulnerabilityWindow
            {
                Window = Window(n.Require("frames")),
                Mask = ParseFlags<InvulnerabilityMask>(n.Require("against")),
            });
            move.Armor = ParseArray(node.Get("armor"), n => new ArmorWindow
            {
                Window = Window(n.Require("frames")),
                Hits = n.GetInt("hits", 1),
                DamagePermille = n.GetInt("damagePermille", 500),
            });
            move.Cancels = ParseArray(node.Get("cancels"), ParseCancel);
            move.Motion = ParseArray(node.Get("motion"), ParseMotionSegment);
            move.Projectiles = ParseArray(node.Get("spawn"), n => new ProjectileSpawn
            {
                Frame = n.Require("frame").AsInt(),
                ProjectileId = n.Require("projectile").AsString(),
                Offset = n.Has("offset") ? Vector(n.Get("offset")) : FixedVector2.Zero,
            });
            move.Effects = ParseArray(node.Get("effects"), n => new EffectCue
            {
                Frame = n.Require("frame").AsInt(),
                Effect = ParseEnum<MoveEffect>(n.Require("effect")),
            });
            move.Cues = ParseArray(node.Get("cues"), n => new PresentationCue
            {
                Frame = n.Require("frame").AsInt(),
                Name = n.Require("name").AsString(),
            });

            var counter = node.Get("counter");
            if (counter != null)
            {
                move.Counter = new CounterStanceSpec
                {
                    Window = Window(counter.Require("frames")),
                    Catches = counter.Has("catches") ? ParseFlags<InvulnerabilityMask>(counter.Get("catches")) : InvulnerabilityMask.Strike,
                    CounterMoveId = counter.Require("move").AsString(),
                };
            }

            var paired = node.Get("paired");
            if (paired != null)
            {
                move.Paired = ParsePaired(paired, attackKeys);
            }

            var evade = node.Get("perfectEvade");
            if (evade != null)
            {
                move.PerfectEvade = Window(evade);
            }

            var cost = node.Get("cost");
            if (cost != null)
            {
                move.Cost = new ResourceCost
                {
                    Rage = cost.GetInt("rage", 0),
                    Shadow = cost.GetInt("shadow", 0),
                    Ultimate = cost.GetInt("ultimate", 0),
                };
            }

            return move;
        }

        private static AttackSpec ParseAttack(string key, JsonNode node)
        {
            node.RequireKind(JsonKind.Object);
            var attack = new AttackSpec
            {
                Key = key,
                Damage = node.GetInt("damage", 0),
                ChipDamage = node.GetInt("chip", 0),
                PostureDamage = node.GetInt("posture", 0),
                Hitstun = node.GetInt("hitstun", 0),
                Blockstun = node.GetInt("blockstun", 0),
                Hitstop = node.GetInt("hitstop", 0),
                Height = node.Has("height") ? ParseEnum<AttackHeight>(node.Get("height")) : AttackHeight.Mid,
                Flags = ParseFlags<AttackFlags>(node.Get("flags")),
                Element = node.Has("element") ? ParseEnum<DamageElement>(node.Get("element")) : DamageElement.Physical,
                Knockback = node.GetFixedScaled("knockback", TicksPerSecond, Fixed.Zero),
                BlockPushback = node.GetFixedScaled("pushback", TicksPerSecond, Fixed.Zero),
                LaunchVelocity = node.Has("launch") ? Velocity(node.Get("launch")) : FixedVector2.Zero,
                AirKnockback = node.Has("airKnockback") ? Velocity(node.Get("airKnockback")) : FixedVector2.Zero,
                JuggleCost = node.GetInt("juggleCost", 1),
                ProrationPermille = node.GetInt("proration", 1000),
                OnHitMoveId = node.GetString("onHit", ""),
            };
            return attack;
        }

        private static MoveTrigger ParseTrigger(JsonNode node)
        {
            node.RequireKind(JsonKind.Object);
            var trigger = new MoveTrigger
            {
                Stance = node.Has("stance") ? ParseFlags<TriggerStance>(node.Get("stance")) : TriggerStance.Grounded,
                Buttons = node.Has("buttons") ? ParseFlags<InputButtons>(node.Get("buttons")) : InputButtons.None,
                Direction = node.Has("direction") ? ParseDirection(node.Get("direction")) : DirectionMask.Any,
                Schemes = node.Has("schemes") ? ParseFlags<ControlSchemeMask>(node.Get("schemes")) : ControlSchemeMask.All,
                Conditions = ParseFlags<TriggerConditions>(node.Get("conditions")),
                Priority = node.GetInt("priority", 0),
            };

            var motion = node.Get("motion");
            if (motion != null)
            {
                string motionId = motion.AsString();
                if (!StandardMotions.TryGet(motionId, out var command))
                {
                    throw new JsonContentException(motion.Path, $"unknown motion '{motionId}'");
                }

                trigger.Motion = command;
            }

            if (trigger.Buttons == InputButtons.None && trigger.Motion == null)
            {
                throw new JsonContentException(node.Path, "a trigger needs buttons, a motion, or both");
            }

            return trigger;
        }

        private static HitboxSpec ParseHitbox(JsonNode node, List<string> attackKeys)
        {
            node.RequireKind(JsonKind.Object);
            var attackNode = node.Require("attack");
            int attackIndex = attackKeys.IndexOf(attackNode.AsString());
            if (attackIndex < 0)
            {
                throw new JsonContentException(attackNode.Path, $"unknown attack '{attackNode.AsString()}'");
            }

            return new HitboxSpec
            {
                Window = Window(node.Require("frames")),
                Box = Box(node.Require("box")),
                AttackIndex = attackIndex,
                Group = node.GetInt("group", 0),
            };
        }

        private static HurtboxWindow ParseHurtboxWindow(JsonNode node)
        {
            node.RequireKind(JsonKind.Object);
            var boxes = node.Require("boxes");
            boxes.RequireKind(JsonKind.Array);
            var result = new FixedAabb[boxes.Count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Box(boxes.Items[i]);
            }

            return new HurtboxWindow
            {
                Window = Window(node.Require("frames")),
                Boxes = result,
                Replace = node.GetBool("replace", false),
            };
        }

        private static CancelWindow ParseCancel(JsonNode node)
        {
            node.RequireKind(JsonKind.Object);
            var window = new CancelWindow
            {
                Window = Window(node.Require("frames")),
                Condition = node.Has("on") ? ParseCancelCondition(node.Get("on")) : CancelCondition.Always,
                Input = node.Has("input") ? ParseEnum<CancelInput>(node.Get("input")) : CancelInput.Trigger,
                InputButton = node.Has("button") ? ParseFlags<InputButtons>(node.Get("button")) : InputButtons.None,
                TargetTags = ParseFlags<MoveTags>(node.Get("tags")),
                AllowJump = node.GetBool("jump", false),
            };

            var targets = node.Get("to");
            if (targets != null)
            {
                targets.RequireKind(JsonKind.Array);
                window.TargetIds = new string[targets.Count];
                for (int i = 0; i < targets.Count; i++)
                {
                    window.TargetIds[i] = targets.Items[i].AsString();
                }
            }

            if ((window.Input == CancelInput.Held || window.Input == CancelInput.Released) && window.InputButton == InputButtons.None)
            {
                throw new JsonContentException(node.Path, "held/released cancels need a 'button'");
            }

            return window;
        }

        private static MotionSegment ParseMotionSegment(JsonNode node)
        {
            node.RequireKind(JsonKind.Object);
            var segment = new MotionSegment { Window = Window(node.Require("frames")) };
            if (node.Has("velocity"))
            {
                segment.Velocity = Velocity(node.Get("velocity"));
                segment.SetX = true;
                segment.SetY = true;
            }
            else
            {
                Fixed x = Fixed.Zero;
                Fixed y = Fixed.Zero;
                if (node.Has("velocityX"))
                {
                    x = Speed(node.Get("velocityX"));
                    segment.SetX = true;
                }

                if (node.Has("velocityY"))
                {
                    y = Speed(node.Get("velocityY"));
                    segment.SetY = true;
                }

                segment.Velocity = new FixedVector2(x, y);
            }

            segment.GravityPermille = OptionalPermille(node, "gravity", 1000);
            return segment;
        }

        private static PairedActionSpec ParsePaired(JsonNode node, List<string> attackKeys)
        {
            node.RequireKind(JsonKind.Object);
            var spec = new PairedActionSpec
            {
                VictimOffset = Vector(node.Require("victimOffset")),
                TechWindow = node.GetInt("techWindow", 0),
                ReleaseFrame = node.Require("releaseFrame").AsInt(),
                ReleaseAttackKey = node.Require("releaseAttack").AsString(),
                AttackerTurnsAround = node.GetBool("turnAround", false),
                ReleaseOffset = node.Has("releaseOffset") ? Vector(node.Get("releaseOffset")) : Vector(node.Require("victimOffset")),
                Cinematic = node.GetBool("cinematic", false),
                VictimAnimation = node.GetString("victimAnimation", ""),
                Hits = ParseArray(node.Get("hits"), n => new PairedHit
                {
                    Frame = n.Require("frame").AsInt(),
                    Damage = n.GetInt("damage", 0),
                    PostureDamage = n.GetInt("posture", 0),
                    Unscaled = n.GetBool("unscaled", false),
                }),
            };
            spec.ReleaseAttackIndex = attackKeys.IndexOf(spec.ReleaseAttackKey);
            if (spec.ReleaseAttackIndex < 0)
            {
                throw new JsonContentException(node.Path + ".releaseAttack", $"unknown attack '{spec.ReleaseAttackKey}'");
            }

            return spec;
        }

        /// <summary>Converts a JSON number in m/s into m/frame.</summary>
        public static Fixed Speed(JsonNode node) => node.AsFixedScaled(TicksPerSecond);

        /// <summary>Converts a JSON number in m/s² into m/frame².</summary>
        public static Fixed Acceleration(JsonNode node) => node.AsFixedScaled(TicksPerSecondSquared);

        /// <summary>Converts a JSON [x, y] array in m/s into m/frame.</summary>
        public static FixedVector2 Velocity(JsonNode node)
        {
            node.RequireKind(JsonKind.Array);
            if (node.Count != 2)
            {
                throw new JsonContentException(node.Path, "expected [x, y]");
            }

            return new FixedVector2(Speed(node.Items[0]), Speed(node.Items[1]));
        }

        private static FixedVector2 Vector(JsonNode node)
        {
            node.RequireKind(JsonKind.Array);
            if (node.Count != 2)
            {
                throw new JsonContentException(node.Path, "expected [x, y]");
            }

            return new FixedVector2(node.Items[0].AsFixed(), node.Items[1].AsFixed());
        }

        private static FixedAabb Box(JsonNode node)
        {
            node.RequireKind(JsonKind.Object);
            var box = FixedAabb.FromMinSize(
                node.Require("x").AsFixed(),
                node.Require("y").AsFixed(),
                node.Require("w").AsFixed(),
                node.Require("h").AsFixed());
            if (!box.IsValid)
            {
                throw new JsonContentException(node.Path, "box width and height must be positive");
            }

            return box;
        }

        private static FrameWindow Window(JsonNode node)
        {
            if (node.Kind == JsonKind.Number)
            {
                int frame = node.AsInt();
                return new FrameWindow(frame, frame);
            }

            node.RequireKind(JsonKind.Array);
            if (node.Count != 2)
            {
                throw new JsonContentException(node.Path, "expected [start, end]");
            }

            return new FrameWindow(node.Items[0].AsInt(), node.Items[1].AsInt());
        }

        private static DirectionMask ParseDirection(JsonNode node)
        {
            try
            {
                return DirectionUtility.ParseMask(node.Kind == JsonKind.Number ? node.RawText : node.AsString());
            }
            catch (FormatException exception)
            {
                throw new JsonContentException(node.Path, exception.Message);
            }
        }

        private static int OptionalPermille(JsonNode parent, string name, int fallback)
        {
            var node = parent.Get(name);
            if (node == null)
            {
                return fallback;
            }

            node.RequireKind(JsonKind.Number);
            decimal value = decimal.Parse(node.RawText, NumberStyles.Float, CultureInfo.InvariantCulture);
            return (int)decimal.Round(value * 1000m, 0, MidpointRounding.AwayFromZero);
        }

        private static T[] ParseArray<T>(JsonNode node, Func<JsonNode, T> parse)
        {
            if (node == null)
            {
                return new T[0];
            }

            node.RequireKind(JsonKind.Array);
            var result = new T[node.Count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = parse(node.Items[i]);
            }

            return result;
        }

        private static T ParseEnum<T>(JsonNode node)
            where T : struct
        {
            string text = node.AsString();
            if (Enum.TryParse(text, true, out T value) && Enum.IsDefined(typeof(T), value))
            {
                return value;
            }

            throw new JsonContentException(node.Path, $"'{text}' is not a valid {typeof(T).Name}");
        }

        private static CancelCondition ParseCancelCondition(JsonNode node)
        {
            switch (node.AsString().ToLowerInvariant())
            {
                case "always": return CancelCondition.Always;
                case "hit": return CancelCondition.OnHit;
                case "block": return CancelCondition.OnBlock;
                case "contact": return CancelCondition.OnContact;
                case "whiff": return CancelCondition.OnWhiff;
                case "evade": return CancelCondition.OnEvade;
                default:
                    throw new JsonContentException(node.Path, $"'{node.AsString()}' is not one of always, hit, block, contact, whiff, evade");
            }
        }

        private static T ParseFlags<T>(JsonNode node)
            where T : struct
        {
            if (node == null)
            {
                return default;
            }

            if (node.Kind == JsonKind.String)
            {
                return ParseEnum<T>(node);
            }

            node.RequireKind(JsonKind.Array);
            ulong bits = 0;
            foreach (var item in node.Items)
            {
                bits |= Convert.ToUInt64(ParseEnum<T>(item), CultureInfo.InvariantCulture);
            }

            return (T)Enum.ToObject(typeof(T), bits);
        }
    }
}

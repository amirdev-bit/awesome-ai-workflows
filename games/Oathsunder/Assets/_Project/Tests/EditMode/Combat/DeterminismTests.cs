using System;
using System.Collections.Generic;
using Oathsunder.Combat.Definitions;
using Oathsunder.Combat.Events;
using Oathsunder.Combat.Input;
using Oathsunder.Combat.Simulation;
using Oathsunder.Core.Mathematics;
using Oathsunder.Tests.Support;
using NUnit.Framework;

namespace Oathsunder.Tests.Combat
{
    /// <summary>
    /// Proves the properties rollback netcode, replays and anti-cheat verification rely on.
    /// </summary>
    [TestFixture]
    public sealed class DeterminismTests
    {
        private const int Frames = 6000;

        private static CombatWorld CreateWorld(ulong seed = 99)
        {
            var setup = new CombatSetup
            {
                Stage = TestContent.Stage("stage.training.dojo"),
                Tuning = TestContent.Tuning(),
                Rules = TestContent.Rules("rules.ranked"),
                Seed = seed,
            };
            setup.Rules.RoundTimerFrames = 30 * 60;
            setup.Combatants.Add(new CombatantSetup(TestContent.RhenKatana(), 0));
            setup.Combatants.Add(new CombatantSetup(TestContent.RhenKatana(), 1) { Scheme = ControlScheme.Simplified });
            var world = new CombatWorld(setup);

            // Start every fighter with meters so modes, bursts and ultimates are exercised by the fuzzers.
            for (int i = 0; i < world.FighterCount; i++)
            {
                world.State.Fighters[i].Rage = 1000;
                world.State.Fighters[i].Shadow = 1000;
                world.State.Fighters[i].Ultimate = 1000;
            }

            return world;
        }

        [Test]
        public void IdenticalInputsProduceIdenticalChecksums()
        {
            var a = CreateWorld();
            var b = CreateWorld();
            var inputA = new[] { new RandomInputGenerator(1), new RandomInputGenerator(2) };
            var inputB = new[] { new RandomInputGenerator(1), new RandomInputGenerator(2) };
            var frameA = new InputFrame[2];
            var frameB = new InputFrame[2];
            var coverage = new HashSet<CombatEventType>();
            for (int frame = 0; frame < Frames; frame++)
            {
                frameA[0] = inputA[0].Next(a.State.Fighters[0].Facing);
                frameA[1] = inputA[1].Next(a.State.Fighters[1].Facing);
                frameB[0] = inputB[0].Next(b.State.Fighters[0].Facing);
                frameB[1] = inputB[1].Next(b.State.Fighters[1].Facing);
                a.Step(frameA);
                b.Step(frameB);
                Assert.AreEqual(a.Checksum(), b.Checksum(), $"desync at frame {frame}");
                for (int e = 0; e < a.Events.Count; e++)
                {
                    coverage.Add(a.Events[e].Type);
                }
            }

            Assert.AreEqual(0, a.DroppedContacts);
            Assert.AreEqual(0, a.DroppedProjectileSpawns);
            foreach (var required in new[]
            {
                CombatEventType.Hit, CombatEventType.Block, CombatEventType.MoveStart, CombatEventType.Knockdown,
                CombatEventType.Parry, CombatEventType.GrabConnect, CombatEventType.ProjectileSpawn, CombatEventType.ComboEnd,
            })
            {
                Assert.IsTrue(coverage.Contains(required), $"fuzz run never produced {required}");
            }
        }

        [Test]
        public void DifferentInputsDiverge()
        {
            var a = CreateWorld();
            var b = CreateWorld();
            var ga = new RandomInputGenerator(1);
            var gb = new RandomInputGenerator(3);
            var neutral = new RandomInputGenerator(2);
            for (int frame = 0; frame < 600; frame++)
            {
                var n = neutral.Next();
                a.Step(new[] { ga.Next(), n });
                b.Step(new[] { gb.Next(), n });
            }

            Assert.AreNotEqual(a.Checksum(), b.Checksum(), "the checksum must actually observe gameplay state");
        }

        /// <summary>
        /// GGPO-style sync test: every frame, roll back up to 8 frames, re-simulate with the same inputs, and require
        /// the result to be bit-identical to the straight run. This is the contract rollback netcode depends on.
        /// </summary>
        [Test]
        public void RollbackAndResimulationIsBitExact()
        {
            const int maxRollback = 8;
            var reference = CreateWorld();
            var rolled = CreateWorld();
            var snapshots = new CombatWorldState[maxRollback + 1];
            for (int i = 0; i < snapshots.Length; i++)
            {
                snapshots[i] = new CombatWorldState();
            }

            var history = new List<InputFrame[]>();
            var generators = new[] { new RandomInputGenerator(11), new RandomInputGenerator(12) };
            for (int frame = 0; frame < 3000; frame++)
            {
                var inputs = new[] { generators[0].Next(reference.State.Fighters[0].Facing), generators[1].Next(reference.State.Fighters[1].Facing) };
                history.Add(inputs);
                reference.Step(inputs);

                rolled.SaveState(snapshots[frame % snapshots.Length]);
                rolled.Step(inputs);

                int depth = 1 + (frame % maxRollback);
                if (frame >= depth)
                {
                    int restoreFrame = frame + 1 - depth;
                    rolled.LoadState(snapshots[restoreFrame % snapshots.Length]);
                    for (int f = restoreFrame; f <= frame; f++)
                    {
                        rolled.Step(history[f]);
                    }
                }

                Assert.AreEqual(reference.Checksum(), rolled.Checksum(), $"rollback desync at frame {frame} (depth {depth})");
            }
        }

        /// <summary>Events regenerated during re-simulation must match the originals (presentation dedupe relies on it).</summary>
        [Test]
        public void ResimulatedEventsMatch()
        {
            var world = CreateWorld();
            var snapshot = new CombatWorldState();
            var generators = new[] { new RandomInputGenerator(21), new RandomInputGenerator(22) };
            for (int frame = 0; frame < 1500; frame++)
            {
                var inputs = new[] { generators[0].Next(world.State.Fighters[0].Facing), generators[1].Next(world.State.Fighters[1].Facing) };
                world.SaveState(snapshot);
                world.Step(inputs);
                var original = Capture(world.Events);
                world.LoadState(snapshot);
                world.Step(inputs);
                CollectionAssert.AreEqual(original, Capture(world.Events), $"frame {frame}");
            }
        }

        /// <summary>
        /// Playing the same match mirrored left-to-right must produce the exact mirror image. Guarantees neither
        /// side of the stage has an advantage (and validates odd-symmetric fixed-point rounding end to end).
        /// </summary>
        [Test]
        public void MirroredMatchIsAnExactMirrorImage()
        {
            var normal = CreateWorld();
            var mirror = CreateWorld();
            for (int i = 0; i < 2; i++)
            {
                ref FighterState m = ref mirror.State.Fighters[i];
                m.Position = new FixedVector2(-m.Position.X, m.Position.Y);
                m.Facing = -m.Facing;
            }

            var generators = new[] { new RandomInputGenerator(31), new RandomInputGenerator(32) };
            var inputs = new InputFrame[2];
            var mirrored = new InputFrame[2];
            int round = normal.State.Round.RoundNumber;
            for (int frame = 0; frame < Frames; frame++)
            {
                inputs[0] = generators[0].Next(normal.State.Fighters[0].Facing);
                inputs[1] = generators[1].Next(normal.State.Fighters[1].Facing);
                mirrored[0] = inputs[0].Mirrored();
                mirrored[1] = inputs[1].Mirrored();
                normal.Step(inputs);
                mirror.Step(mirrored);
                if (normal.State.Round.RoundNumber != round)
                {
                    // Round resets place team 0 on the left by rule; re-mirror the mirrored world.
                    round = normal.State.Round.RoundNumber;
                    for (int i = 0; i < 2; i++)
                    {
                        ref FighterState m = ref mirror.State.Fighters[i];
                        m.Position = new FixedVector2(-m.Position.X, m.Position.Y);
                        m.Facing = -m.Facing;
                    }
                }
                for (int i = 0; i < 2; i++)
                {
                    ref FighterState a = ref normal.State.Fighters[i];
                    ref FighterState b = ref mirror.State.Fighters[i];
                    string where = $"frame {frame}, fighter {i}";
                    Assert.AreEqual(-a.Position.X.Raw, b.Position.X.Raw, where + " x");
                    Assert.AreEqual(a.Position.Y.Raw, b.Position.Y.Raw, where + " y");
                    Assert.AreEqual(-a.Velocity.X.Raw, b.Velocity.X.Raw, where + " vx");
                    Assert.AreEqual(-a.Facing, b.Facing, where + " facing");
                    Assert.AreEqual(a.Action, b.Action, where + " action");
                    Assert.AreEqual(a.ActionFrame, b.ActionFrame, where + " action frame");
                    Assert.AreEqual(a.MoveIndex, b.MoveIndex, where + " move");
                    Assert.AreEqual(a.Health, b.Health, where + " health");
                    Assert.AreEqual(a.Posture, b.Posture, where + " posture");
                }
            }
        }

        [Test]
        public void SteppingDoesNotAllocate()
        {
            var world = CreateWorld();
            var generators = new[] { new RandomInputGenerator(41), new RandomInputGenerator(42) };
            var inputs = new InputFrame[2];
            var snapshot = new CombatWorldState();
            for (int frame = 0; frame < 600; frame++)
            {
                inputs[0] = generators[0].Next(world.State.Fighters[0].Facing);
                inputs[1] = generators[1].Next(world.State.Fighters[1].Facing);
                world.Step(inputs);
            }

            var prepared = new InputFrame[4000][];
            for (int frame = 0; frame < prepared.Length; frame++)
            {
                prepared[frame] = new[] { generators[0].Next(frame % 2 == 0 ? 1 : -1), generators[1].Next() };
            }

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int frame = 0; frame < prepared.Length; frame++)
            {
                world.Step(prepared[frame]);
                if (frame % 8 == 0)
                {
                    world.SaveState(snapshot);
                    world.LoadState(snapshot);
                    world.Checksum();
                }
            }

            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.AreEqual(0, allocated, "the simulation, snapshots and checksums must be allocation-free");
        }

        private static List<string> Capture(CombatEventBuffer events)
        {
            var list = new List<string>(events.Count);
            for (int i = 0; i < events.Count; i++)
            {
                list.Add(events[i].ToString() + " " + events[i].Instance + " " + events[i].Position);
            }

            return list;
        }
    }
}

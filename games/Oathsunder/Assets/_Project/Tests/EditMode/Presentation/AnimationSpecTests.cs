using System;
using System.Linq;
using Oathsunder.Combat.Definitions;
using Oathsunder.Combat.Events;
using Oathsunder.Combat.Input;
using Oathsunder.Combat.Simulation;
using Oathsunder.Core.Mathematics;
using Oathsunder.Core.Serialization;
using Oathsunder.Presentation.Animation;
using Oathsunder.Tests.Support;
using NUnit.Framework;

namespace Oathsunder.Tests.Presentation
{
    [TestFixture]
    public sealed class AnimationSpecTests
    {
        private static MoveAnimationSpec Capture(string moveId) =>
            MoveAnimationCapture.Capture(TestContent.RhenKatana(), TestContent.Tuning(), moveId);

        [Test]
        public void EveryMoveIsCapturedForItsWholeClipOrExplainsWhyNot()
        {
            foreach (var spec in MoveAnimationCapture.CaptureAll(TestContent.RhenKatana(), TestContent.Tuning()))
            {
                Assert.AreEqual(spec.Move.TotalFrames, spec.Phases.Length, spec.Move.Id);
                if (spec.Root.Length < spec.Move.TotalFrames)
                {
                    Assert.IsTrue(spec.Notes.Any(n => n.StartsWith("Capture:", StringComparison.Ordinal)), $"{spec.Move.Id} ended early without a note");
                }

                for (int i = 0; i < spec.Root.Length; i++)
                {
                    Assert.AreEqual(i + 1, spec.Root[i].Frame, $"{spec.Move.Id} root sample {i}");
                    Assert.AreEqual(i + 1, spec.Hurt[i].Frame, $"{spec.Move.Id} hurt sample {i}");
                }
            }
        }

        [Test]
        public void GroundedRootMotionMatchesTheMotionSegmentsExactly()
        {
            // universal.dash: 13 m/s for frames 1-12, then 4 m/s for frames 13-16.
            var dash = Capture("universal.dash");
            Assert.AreEqual(RootMotionKind.Scripted, dash.RootMotion);
            Fixed expected = Fixed.FromRatio(13, 60) * Fixed.FromInt(12) + Fixed.FromRatio(4, 60) * Fixed.FromInt(4);
            Assert.AreEqual(expected, dash.FinalOffset.X);
            Assert.AreEqual(Fixed.FromRatio(13, 60), dash.Root[0].Offset.X, "frame 1 already moves");
            Assert.IsTrue(dash.Root.All(r => r.Grounded));

            var light = Capture("katana.l1");
            Assert.AreEqual(Fixed.FromRatio(15, 600) * Fixed.FromInt(4), light.FinalOffset.X, "1.5 m/s for 4 frames");
        }

        [Test]
        public void StationaryAndPairedMovesAreClassified()
        {
            Assert.AreEqual(RootMotionKind.InPlace, Capture("katana.dl").RootMotion);
            Assert.AreEqual(RootMotionKind.Paired, Capture("universal.throwforward.hit").RootMotion);
            Assert.AreEqual(RootMotionKind.Ballistic, Capture("katana.jl").RootMotion);
        }

        [Test]
        public void AirMovesStartAtTheJumpApexAndFall()
        {
            var jumping = Capture("katana.jh");
            Assert.IsFalse(jumping.Root[0].Grounded);
            Assert.Less(jumping.Root[jumping.Root.Length - 1].Offset.Y.Raw, 0, "falls from the apex");
            Assert.IsTrue(jumping.Notes.Any(n => n.Contains("apex")));
        }

        [Test]
        public void ChargeHoldIsCapturedUntilItsAutomaticRelease()
        {
            var charge = Capture("katana.hcharge");
            Assert.AreEqual(59, charge.Root.Length, "held for the whole charge, auto-released into the full charge on frame 60");
            StringAssert.Contains("katana.hchargedfull", string.Join(" ", charge.Notes));
        }

        [Test]
        public void DefensiveTimelineMatchesTheMoveData()
        {
            var dragon = Capture("katana.dps");
            for (int frame = 1; frame <= 8; frame++)
            {
                Assert.AreEqual(InvulnerabilityMask.Strike, dragon.Hurt[frame - 1].Invulnerable & InvulnerabilityMask.Strike, $"dps frame {frame}");
            }

            Assert.AreEqual(InvulnerabilityMask.None, dragon.Hurt[8].Invulnerable & InvulnerabilityMask.Strike, "dps frame 9");

            var dodge = Capture("universal.dodge");
            Assert.IsFalse(dodge.Hurt[0].PerfectEvade);
            Assert.IsTrue(dodge.Hurt[1].PerfectEvade && dodge.Hurt[6].PerfectEvade);
            Assert.IsFalse(dodge.Hurt[7].PerfectEvade);

            var stance = Capture("katana.s");
            Assert.IsFalse(stance.Hurt[2].CounterStance);
            Assert.IsTrue(stance.Hurt[3].CounterStance && stance.Hurt[23].CounterStance);
            Assert.IsFalse(stance.Hurt[24].CounterStance);
        }

        [Test]
        public void ExtendedHurtboxesAppearOnlyOnTheirFrames()
        {
            var light = Capture("katana.l1");
            int stance = TestContent.Rhen.Standing.Hurtboxes.Length;
            Assert.AreEqual(stance, light.Hurt[4].Boxes.Length, "frame 5");
            Assert.AreEqual(stance + 1, light.Hurt[5].Boxes.Length, "frame 6 adds the extended arm");
            Assert.AreEqual(stance + 1, light.Hurt[10].Boxes.Length, "frame 11");
            Assert.AreEqual(stance, light.Hurt[11].Boxes.Length, "frame 12");
            Assert.AreEqual(3, AnimationSpecJson.HurtRanges(light).Count, "stance, extended arm, stance");
        }

        [Test]
        public void TimingAndKeyPosesFollowTheFrameData()
        {
            foreach (var spec in MoveAnimationCapture.CaptureAll(TestContent.RhenKatana(), TestContent.Tuning()))
            {
                var move = spec.Move;
                CollectionAssert.IsOrdered(spec.KeyPoses.Select(p => p.Frame).ToList(), move.Id);
                Assert.AreEqual(spec.KeyPoses.Count, spec.KeyPoses.Select(p => p.Frame).Distinct().Count(), $"{move.Id}: one entry per key frame");
                Assert.AreEqual(1, spec.KeyPoses[0].Frame, move.Id);
                Assert.AreEqual(move.TotalFrames, spec.KeyPoses[spec.KeyPoses.Count - 1].Frame, move.Id);
                if (!move.IsAttack)
                {
                    continue;
                }

                Assert.AreEqual(move.TotalFrames, spec.Startup + spec.ActiveSpan + spec.Recovery, move.Id);
                Assert.AreEqual(MovePhase.Active, spec.Phases[move.FirstActiveFrame - 1], move.Id);
                Assert.IsTrue(spec.KeyPoses.Any(p => p.Frame == move.FirstActiveFrame && p.Name.Contains("Contact")), $"{move.Id}: contact pose on the first active frame");
                Assert.IsNotEmpty(spec.Contacts, move.Id);
            }
        }

        [Test]
        public void ContactFeedbackUsesTheSimulationsHeavyRule()
        {
            var light = Capture("katana.l1").Contacts.Single();
            Assert.AreEqual("impact.hit.light", light.OnHit);
            Assert.AreEqual("impact.block", light.OnBlock);
            var heavy = Capture("katana.h1").Contacts.Single();
            Assert.AreEqual("impact.hit.heavy", heavy.OnHit);
            Assert.AreEqual("impact.block.heavy", heavy.OnBlock);
            var grab = Capture("universal.throwforward").Contacts.Single();
            Assert.AreEqual("impact.grab", grab.OnHit);
            Assert.AreEqual("", grab.OnBlock);
        }

        [Test]
        public void CaptureIsDeterministic()
        {
            string a = AnimationSpecJson.Write("fighter.rhen", "weapon.katana", MoveAnimationCapture.CaptureAll(TestContent.RhenKatana(), TestContent.Tuning()), TestContent.Cues);
            string b = AnimationSpecJson.Write("fighter.rhen", "weapon.katana", MoveAnimationCapture.CaptureAll(TestContent.RhenKatana(), TestContent.Tuning()), TestContent.Cues);
            Assert.AreEqual(a, b);
        }

        [Test]
        public void JsonExportIsValidAndComplete()
        {
            var blueprint = TestContent.RhenKatana();
            string json = AnimationSpecJson.Write("fighter.rhen", "weapon.katana", MoveAnimationCapture.CaptureAll(blueprint, TestContent.Tuning()), TestContent.Cues);
            var root = JsonReader.Parse(json, "animspec.json");
            Assert.AreEqual(AnimationSpecJson.Version, root.Require("version").AsInt());
            var clips = root.Require("clips");
            Assert.AreEqual(blueprint.Moves.Length, clips.Count);
            var l1 = clips.Items.Single(c => c.Require("move").AsString() == "katana.l1");
            Assert.AreEqual(19, l1.Require("frames").AsInt());
            Assert.AreEqual(19, l1.Require("root").Count);
            Assert.AreEqual("SFX_Katana_Swing_Light", l1.Require("events").Items.Single(e => e.Require("cue").AsString() == "sfx.katana.swing.light").Require("asset").AsString());
        }

        [Test]
        public void ShippedAnimationSpecFileIsUpToDate()
        {
            string path = System.IO.Path.Combine(TestContent.Root, "..", "..", "Animation", "Specs", "animspec.rhen.katana.json");
            string committed = System.IO.File.ReadAllText(path).Replace("\r\n", "\n");
            string current = AnimationSpecJson.Write("fighter.rhen", "weapon.katana", MoveAnimationCapture.CaptureAll(TestContent.RhenKatana(), TestContent.Tuning()), TestContent.Cues);
            Assert.AreEqual(current, committed, "Regenerate: dotnet run -c Release --project Tools/DotNet/Oathsunder.Tools.FrameData -- --write");
        }

        [Test]
        public void ForceMoveStartsOnTheNextStepAndLoadStateCancelsIt()
        {
            var h = new CombatHarness();
            h.PlaceApart(3);
            var snapshot = new CombatWorldState();
            h.World.SaveState(snapshot);
            h.World.ForceMove(0, "katana.h1");
            Assert.AreEqual(FighterAction.Idle, h.P0.Action, "nothing happens until the next step");
            h.Step();
            Assert.AreEqual("katana.h1", h.MoveId(0));
            Assert.AreEqual(1, h.P0.ActionFrame);
            Assert.IsTrue(h.Has(CombatEventType.MoveStart));

            h.World.ForceMove(0, "katana.l1");
            h.World.LoadState(snapshot);
            h.Step();
            Assert.AreNotEqual(FighterAction.Move, h.P0.Action, "a restored state drops pending forced moves");
            Assert.Throws<ArgumentException>(() => h.World.ForceMove(0, "katana.unknown"));
        }

        [Test]
        public void WorldHurtboxQueryMatchesContactDetection()
        {
            var h = new CombatHarness();
            h.PlaceApart(3);
            var boxes = new FixedAabb[16];
            int idle = h.World.GetHurtboxes(0, boxes);
            Assert.AreEqual(TestContent.Rhen.Standing.Hurtboxes.Length, idle);
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.P0.ActionFrame == 6, 10, "katana.l1 frame 6");
            Assert.AreEqual(idle + 1, h.World.GetHurtboxes(0, boxes));
            Assert.IsFalse(h.World.IsInvulnerable(0, InvulnerabilityMask.Strike));
        }
    }
}

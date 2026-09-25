using System;
using System.Linq;
using Oathsunder.Combat.Events;
using Oathsunder.Combat.Input;
using Oathsunder.Combat.Simulation;
using Oathsunder.Presentation.Animation;
using Oathsunder.Tests.Support;
using NUnit.Framework;

namespace Oathsunder.Tests.Presentation
{
    [TestFixture]
    public sealed class PoseSamplerTests
    {
        [Test]
        public void MovesPlayTheirClipOverTheirFrameCountAndFreezeInHitstop()
        {
            var whiff = new CombatHarness();
            whiff.PlaceApart(4);
            whiff.Tap(0, InputButtons.Light);
            whiff.RunUntil(() => whiff.P0.ActionFrame == 6, 10, "katana.l1 frame 6");
            var sample = FighterPoseSampler.Sample(whiff.World, 0, 0.5f);
            Assert.AreEqual("A_Katana_L1", sample.Clip);
            Assert.AreEqual(5.5f / 19f, sample.NormalizedTime, 1e-5f, "frame 6 of 19, half-way to the next tick");

            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.P0.HitstopRemaining > 0, 10, "hitstop");
            float frozen = FighterPoseSampler.Sample(h.World, 0, 0f).NormalizedTime;
            Assert.AreEqual(frozen, FighterPoseSampler.Sample(h.World, 0, 0.9f).NormalizedTime, "interpolation is ignored during hitstop");
        }

        [Test]
        public void StunReactionsPlayOverTheStunTheHitApplied()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.P1.Action == FighterAction.Hitstun, 10, "hitstun");
            float previous = -1f;
            while (h.P1.Action == FighterAction.Hitstun)
            {
                var sample = FighterPoseSampler.Sample(h.World, 1, 0f);
                Assert.AreEqual("A_Fighter_Hitstun", sample.Clip);
                Assert.GreaterOrEqual(sample.NormalizedTime, previous, "never runs backwards");
                Assert.LessOrEqual(sample.NormalizedTime, 1f);
                previous = sample.NormalizedTime;
                h.Step();
            }

            Assert.Greater(previous, 0.85f, "reaches the end of the clip as the stun ends");
        }

        [Test]
        public void JumpsAreSampledByVerticalVelocity()
        {
            var h = new CombatHarness();
            h.PlaceApart(4);
            h.Tap(0, InputButtons.Jump);
            h.RunUntil(() => h.P0.Action == FighterAction.Airborne, 10, "take-off");
            float takeOff = FighterPoseSampler.Sample(h.World, 0, 0f).NormalizedTime;
            h.RunUntil(() => h.P0.Velocity.Y.Raw <= 0, 60, "apex");
            float apex = FighterPoseSampler.Sample(h.World, 0, 0f).NormalizedTime;
            h.Wait(8);
            float falling = FighterPoseSampler.Sample(h.World, 0, 0f).NormalizedTime;
            Assert.AreEqual("A_Fighter_Airborne", FighterPoseSampler.Sample(h.World, 0, 0f).Clip);
            Assert.Less(takeOff, apex);
            Assert.Less(apex, falling);
        }

        [Test]
        public void PairedVictimsPlayTheHoldersVictimClipInSync()
        {
            var h = new CombatHarness();
            h.PlaceApart(0.9);
            h.Tap(0, InputButtons.Grab);
            h.RunUntil(() => h.P1.Action == FighterAction.PairedVictim, 15, "grab connects");
            h.Wait(10);
            var holder = FighterPoseSampler.Sample(h.World, 0, 0f);
            var victim = FighterPoseSampler.Sample(h.World, 1, 0f);
            Assert.AreEqual("A_Universal_Throw_Forward", holder.Clip);
            Assert.AreEqual("A_Victim_Throw_Forward", victim.Clip);
            Assert.AreEqual(holder.NormalizedTime, victim.NormalizedTime, 1e-6f, "same frame of the same paired action");
        }

        [Test]
        public void EveryFighterStateHasAClipInTheLibrary()
        {
            var clips = FighterPoseSampler.StateClips(TestContent.Rhen, TestContent.Tuning(), TestContent.Rules("rules.story"));
            Assert.AreEqual(clips.Count, clips.Select(c => c.Clip).Distinct().Count(), "unique clip names");
            foreach (FighterAction action in Enum.GetValues(typeof(FighterAction)))
            {
                if (action == FighterAction.Move)
                {
                    continue;
                }

                Assert.IsTrue(clips.Any(c => c.Actions.StartsWith(action.ToString(), StringComparison.Ordinal)), $"{action} has no clip");
            }

            var knockdown = clips.Single(c => c.Clip == "A_Fighter_Knockdown_Hard");
            Assert.AreEqual(TestContent.Rhen.HardKnockdownFrames, knockdown.Frames, "durations come from the fighter data");
        }

        [Test]
        public void EverySampledClipIsSpecified()
        {
            var specified = FighterPoseSampler.StateClips(TestContent.Rhen, TestContent.Tuning(), TestContent.Rules("rules.story")).Select(c => c.Clip)
                .Concat(TestContent.RhenKatana().Moves.Select(FighterPoseSampler.ClipOf))
                .Concat(TestContent.RhenKatana().Moves.Where(m => m.Paired != null).Select(m => m.Paired.VictimAnimation))
                .ToHashSet(StringComparer.Ordinal);

            var generator = new RandomInputGenerator(0x5EED);
            var h = new CombatHarness();
            h.PlaceApart(1.5);
            for (int frame = 0; frame < 3000; frame++)
            {
                for (int fighter = 0; fighter < 2; fighter++)
                {
                    string clip = FighterPoseSampler.Sample(h.World, fighter, 0.5f).Clip;
                    Assert.IsTrue(specified.Contains(clip), $"frame {frame}: {clip} is not in the animation specification");
                }

                h.World.Step(new[] { generator.Next(h.P0.Facing), generator.Next(h.P1.Facing) });
                if (h.World.State.Round.Phase == RoundPhase.MatchOver)
                {
                    break;
                }
            }
        }
    }
}

using Oathsunder.Combat.Events;
using Oathsunder.Combat.Input;
using Oathsunder.Combat.Simulation;
using Oathsunder.Tests.Support;
using NUnit.Framework;

namespace Oathsunder.Tests.Combat
{
    [TestFixture]
    public sealed class RoundTests
    {
        private static CombatHarness BestOfThree(int holdFrames = 30) => new CombatHarness(setup =>
        {
            setup.Rules.RoundsToWin = 2;
            setup.Rules.KnockoutHoldFrames = holdFrames;
            setup.Rules.PreRoundFrames = 0;
        });

        [Test]
        public void KnockoutEndsTheRoundAndResetsTheNext()
        {
            var h = BestOfThree();
            h.PlaceApart(1.2);
            h.P1.Health = 1;
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.Has(CombatEventType.KnockOut), 10, "KO");
            Assert.IsTrue(h.P1.IsKnockedOut);
            Assert.AreEqual(RoundPhase.RoundEnding, h.World.State.Round.Phase);
            Assert.AreEqual(0, h.First(CombatEventType.RoundEnd).Value, "team 0 wins the round");

            h.RunUntil(() => h.World.State.Round.RoundNumber == 2, 60, "round 2");
            Assert.AreEqual(1, h.World.State.Round.Team0Wins);
            Assert.AreEqual(1000, h.P1.Health);
            Assert.IsFalse(h.P1.IsKnockedOut);
            Assert.AreEqual(FighterAction.Idle, h.P1.Action);
            Assert.AreEqual(-CombatHarness.M(1.6).Raw, h.P0.Position.X.Raw, "positions reset");
        }

        [Test]
        public void MatchEndsWhenATeamHasEnoughRounds()
        {
            var h = BestOfThree();
            for (int round = 1; round <= 2; round++)
            {
                h.RunUntil(() => h.World.State.Round.Phase == RoundPhase.Fighting, 60, "fight");
                h.PlaceApart(1.2);
                h.P1.Health = 1;
                h.Tap(0, InputButtons.Light);
                h.RunUntil(() => h.World.State.Round.Phase != RoundPhase.Fighting, 20, "KO");
            }

            h.RunUntil(() => h.World.State.Round.Phase == RoundPhase.MatchOver, 60, "match over");
            Assert.AreEqual(0, h.World.State.Round.MatchWinner);
            Assert.AreEqual(0, h.First(CombatEventType.MatchEnd).Value);
            int frame = h.Frame;
            h.Tap(0, InputButtons.Light);
            Assert.AreEqual(frame + 1, h.Frame, "stepping after the match is harmless");
        }

        [Test]
        public void TimeOverAwardsTheHealthierTeam()
        {
            var h = new CombatHarness(setup => setup.Rules.RoundTimerFrames = 120);
            h.P0.Health = 900;
            h.RunUntil(() => h.Has(CombatEventType.TimeOver), 130, "time over");
            Assert.AreEqual(1, h.First(CombatEventType.RoundEnd).Value);
        }

        [Test]
        public void DoubleKnockoutIsADraw()
        {
            var h = BestOfThree();
            h.PlaceApart(1.2);
            h.P0.Health = 1;
            h.P1.Health = 1;
            h.Step(InputButtons.Light, InputButtons.Light);
            h.RunUntil(() => h.Count(CombatEventType.KnockOut) == 2, 10, "double KO");
            Assert.AreEqual(-2, h.First(CombatEventType.RoundEnd).Value);
            h.RunUntil(() => h.World.State.Round.RoundNumber == 2, 60, "next round");
            Assert.AreEqual(0, h.World.State.Round.Team0Wins);
            Assert.AreEqual(0, h.World.State.Round.Team1Wins);
        }

        [Test]
        public void CountdownIgnoresInput()
        {
            var h = new CombatHarness(setup => setup.Rules.PreRoundFrames = 30);
            h.PlaceApart(1.2);
            h.Tap(0, InputButtons.Light);
            Assert.AreEqual(FighterAction.Idle, h.P0.Action);
            h.RunUntil(() => h.Has(CombatEventType.RoundFight), 40, "fight");
            h.Tap(0, InputButtons.Light);
            Assert.AreEqual("katana.l1", h.MoveId(0));
        }

        [Test]
        public void ExecutionKillIsAFinisher()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.P1.Health = 100;
            h.P1.Action = FighterAction.Staggered;
            h.P1.Stagger = StaggerKind.GuardBreak;
            h.P1.StunRemaining = 60;
            h.Tap(0, InputButtons.Execute);
            h.RunUntil(() => h.Has(CombatEventType.KnockOut), 60, "execution kill");
            Assert.IsTrue(h.First(CombatEventType.KnockOut).Has(CombatEventFlags.Finisher));
        }
    }
}

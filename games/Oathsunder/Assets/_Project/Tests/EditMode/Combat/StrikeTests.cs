using Oathsunder.Combat.Events;
using Oathsunder.Combat.Input;
using Oathsunder.Combat.Simulation;
using Oathsunder.Tests.Support;
using NUnit.Framework;

namespace Oathsunder.Tests.Combat
{
    [TestFixture]
    public sealed class StrikeTests
    {
        [Test]
        public void LightStartsOnThePressFrameAndHitsOnFrameSix()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Tap(0, InputButtons.Light);
            int pressFrame = h.Frame;
            Assert.AreEqual("katana.l1", h.MoveId(0), "zero added input latency");
            Assert.AreEqual(1, h.P0.ActionFrame);
            h.RunUntil(() => h.Has(CombatEventType.Hit), 10, "hit");
            var hit = h.First(CombatEventType.Hit);
            Assert.AreEqual(pressFrame + 5, hit.Frame, "startup 6");
            Assert.AreEqual(40, hit.Value);
            Assert.AreEqual(960, h.P1.Health);
        }

        [Test]
        public void HitstopFreezesBothFighters()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.Has(CombatEventType.Hit), 10, "hit");
            Assert.AreEqual(8, h.P0.HitstopRemaining);
            Assert.AreEqual(8, h.P1.HitstopRemaining);
            var p0 = h.P0.Position;
            var p1 = h.P1.Position;
            int frame0 = h.P0.ActionFrame;
            h.Wait(8);
            Assert.AreEqual(p0, h.P0.Position);
            Assert.AreEqual(p1, h.P1.Position);
            Assert.AreEqual(frame0, h.P0.ActionFrame);
            h.Step();
            Assert.AreEqual(frame0 + 1, h.P0.ActionFrame);
        }

        [Test]
        public void WhiffedLightRecoversAfterTotalFrames()
        {
            var h = new CombatHarness();
            h.PlaceApart(3.0);
            h.Tap(0, InputButtons.Light);
            h.Wait(18);
            Assert.AreEqual(FighterAction.Move, h.P0.Action, "frame 19 is still the move");
            h.Step();
            Assert.AreEqual(FighterAction.Idle, h.P0.Action, "actionable on frame 20");
            Assert.AreEqual(0, h.Count(CombatEventType.Hit));
        }

        [Test]
        public void MeasuredFrameAdvantageMatchesFrameData()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.Has(CombatEventType.Hit), 10, "hit");
            int attackerFree = h.RunUntil(() => h.P0.Action != FighterAction.Move, 60, "attacker recovers");
            int victimFree = attackerFree + h.RunUntil(() => h.P1.Action != FighterAction.Hitstun, 60, "victim recovers");
            Assert.AreEqual(4, victimFree - attackerFree, "katana.l1 is +4 on hit");
        }

        [Test]
        public void LightStringChainsWithBufferedPresses()
        {
            // The player confirms each hit and presses during hitstop, which the local-frame buffer keeps fresh.
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Tap(0, InputButtons.Light);
            for (int hits = 1; hits <= 3; hits++)
            {
                int expected = hits;
                h.RunUntil(() => h.Count(CombatEventType.Hit) == expected, 40, $"hit {expected}");
                h.Wait(2);
                h.Tap(0, InputButtons.Light);
            }

            h.Wait(80);
            Assert.AreEqual(4, h.Count(CombatEventType.Hit), "L1 > L2 > L3 > L4");
            Assert.IsTrue(h.Has(CombatEventType.Knockdown), "Oathseal knocks down");
            var combo = h.First(CombatEventType.ComboEnd);
            Assert.AreEqual(4, combo.Value2);
            Assert.AreEqual(40 + 36 + 39 + 57, combo.Value, "starter full, then 80% proration × per-hit scaling");
        }

        [TestCase(InputButtons.Light, InputButtons.Heavy, "katana.l3")]
        [TestCase(InputButtons.Heavy, InputButtons.Light, "katana.l2h")]
        public void EarliestBufferedPressWins(InputButtons first, InputButtons second, string expectedMove)
        {
            // Both follow-ups of Falling Petal are buffered during its hitstop: the one pressed first is performed.
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.Count(CombatEventType.Hit) == 1, 10, "L1 hit");
            h.Wait(2);
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.Count(CombatEventType.Hit) == 2, 40, "L2 hit");
            h.Tap(0, first);
            h.Step();
            h.Tap(0, second);
            h.RunUntil(() => h.MoveId(0) != "katana.l2", 30, "cancel out of L2");
            Assert.AreEqual(expectedMove, h.MoveId(0));
        }

        [Test]
        public void CounterHitAddsDamageAndHitstun()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.3);
            h.Tap(1, InputButtons.Heavy);
            h.Wait(3);
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.Has(CombatEventType.Hit), 10, "hit");
            var hit = h.First(CombatEventType.Hit);
            Assert.AreEqual(0, hit.Actor);
            Assert.IsTrue(hit.Has(CombatEventFlags.Counter));
            Assert.AreEqual(48, hit.Value, "40 × 1.2");
            Assert.AreEqual(17 + 4, h.P1.StunRemaining);
        }

        [Test]
        public void PunishingRecoveryIsFlagged()
        {
            var h = new CombatHarness();
            h.PlaceApart(2.2);
            h.Tap(1, InputButtons.Heavy, 2);
            Assert.AreEqual("katana.dh", h.MoveId(1));
            h.Wait(15);
            h.Tap(0, InputButtons.Light, 6);
            Assert.AreEqual("katana.fl", h.MoveId(0));
            h.RunUntil(() => h.Has(CombatEventType.Hit), 16, "punish");
            var hit = h.First(CombatEventType.Hit);
            Assert.AreEqual(0, hit.Actor, "the sweep whiffed");
            Assert.IsTrue(hit.Has(CombatEventFlags.Punish));
            Assert.AreEqual(66, hit.Value, "60 × 1.1");
        }

        [Test]
        public void TradesResolveSymmetrically()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Step(InputButtons.Light, InputButtons.Light);
            h.RunUntil(() => h.Has(CombatEventType.Hit), 10, "trade");
            Assert.AreEqual(2, h.Count(CombatEventType.Hit));
            Assert.AreEqual(h.P0.Health, h.P1.Health);
            Assert.AreEqual(FighterAction.Hitstun, h.P0.Action);
            Assert.AreEqual(FighterAction.Hitstun, h.P1.Action);
        }

        [Test]
        public void EachHitGroupHitsOncePerMove()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Hold(0, InputButtons.None);
            h.Tap(0, InputButtons.Heavy);
            h.Wait(5);
            h.Tap(0, InputButtons.Heavy);
            h.Wait(60);
            Assert.AreEqual(3, h.Count(CombatEventType.Hit), "Iron Draw (1) + Twin Moon (2 groups)");
        }
    }
}

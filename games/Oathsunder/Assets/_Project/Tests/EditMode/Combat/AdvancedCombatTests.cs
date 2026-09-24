using Oathsunder.Combat.Definitions;
using Oathsunder.Combat.Events;
using Oathsunder.Combat.Input;
using Oathsunder.Combat.Simulation;
using Oathsunder.Tests.Support;
using NUnit.Framework;

namespace Oathsunder.Tests.Combat
{
    [TestFixture]
    public sealed class AdvancedCombatTests
    {
        /// <summary>L, L, H launcher, jump cancel, j.L, j.L, j.H spike, ground bounce — the Katana air route.</summary>
        [Test]
        public void LauncherAirComboRouteConnects()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.Count(CombatEventType.Hit) == 1, 10, "L1");
            h.Wait(2);
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.Count(CombatEventType.Hit) == 2, 30, "L2");
            h.Wait(2);
            h.Tap(0, InputButtons.Heavy);
            h.RunUntil(() => h.Has(CombatEventType.Launch), 30, "launcher");
            Assert.AreEqual(FighterAction.Launched, h.P1.Action);

            h.Wait(2);
            h.HoldDirection(0, 9);
            h.Tap(0, InputButtons.Jump);
            h.RunUntil(() => !h.P0.Grounded, 20, "jump cancel");
            h.HoldDirection(0, 5);

            h.Wait(3);
            h.Tap(0, InputButtons.Light);
            Assert.AreEqual("katana.jl", h.MoveId(0));
            h.RunUntil(() => h.Count(CombatEventType.Hit) == 4, 20, "j.L");
            h.Wait(2);
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.Count(CombatEventType.Hit) == 5, 25, "j.L2");
            h.Wait(2);
            h.Tap(0, InputButtons.Heavy);
            h.RunUntil(() => h.Count(CombatEventType.Hit) == 6, 25, "j.H spike");
            h.RunUntil(() => h.Has(CombatEventType.GroundBounce), 40, "ground bounce");

            h.RunUntil(() => h.Has(CombatEventType.ComboEnd), 200, "combo end");
            var combo = h.First(CombatEventType.ComboEnd);
            Assert.AreEqual(6, combo.Value2, "six-hit route");
            Assert.Greater(combo.Value, 200);
        }

        [Test]
        public void JuggleLimitMakesFurtherHitsWhiff()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.Count(CombatEventType.Hit) == 1, 10, "L1");
            h.Wait(2);
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.Count(CombatEventType.Hit) == 2, 30, "L2");
            h.Wait(2);
            h.Tap(0, InputButtons.Heavy);
            h.RunUntil(() => h.Has(CombatEventType.Launch), 30, "launcher");
            h.P1.JugglePoints = TestContent.Tuning().MaxJugglePoints;

            h.Wait(2);
            h.HoldDirection(0, 9);
            h.Tap(0, InputButtons.Jump);
            h.RunUntil(() => !h.P0.Grounded, 20, "jump cancel");
            h.HoldDirection(0, 5);
            h.Wait(3);
            h.Tap(0, InputButtons.Light);
            h.Wait(20);
            Assert.AreEqual(3, h.Count(CombatEventType.Hit), "no juggle points left");
        }

        [Test]
        public void OathsealWallBouncesOncePerCombo()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2, centre: 9.5);
            h.Tap(0, InputButtons.Light);
            for (int hits = 1; hits <= 3; hits++)
            {
                int expected = hits;
                h.RunUntil(() => h.Count(CombatEventType.Hit) == expected, 40, $"hit {expected}");
                h.Wait(2);
                h.Tap(0, InputButtons.Light);
            }

            h.RunUntil(() => h.Has(CombatEventType.WallBounce), 60, "wall bounce");
            h.Wait(120);
            Assert.AreEqual(1, h.Count(CombatEventType.WallBounce));
        }

        [Test]
        public void EmberRageBoostsDamage()
        {
            var h = new CombatHarness();
            h.PlaceApart(3);
            h.P0.Rage = 1000;
            h.Tap(0, InputButtons.Rage);
            Assert.AreEqual("universal.rage", h.MoveId(0));
            Assert.IsTrue(h.Has(CombatEventType.RageStart));
            Assert.AreEqual(0, h.P0.Rage, "meter spent");
            h.RunUntil(() => h.P0.Action == FighterAction.Idle, 40, "activation ends");
            Assert.IsTrue(h.P0.InRage);

            h.PlaceApart(1.2);
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.Has(CombatEventType.Hit), 10, "hit");
            var hit = h.First(CombatEventType.Hit);
            Assert.AreEqual(48, hit.Value, "40 × 1.2");
            Assert.IsTrue(hit.Has(CombatEventFlags.Rage));
        }

        [Test]
        public void RageCannotBeActivatedWithoutMeter()
        {
            var h = new CombatHarness();
            h.P0.Rage = 999;
            h.Tap(0, InputButtons.Rage);
            Assert.AreEqual(FighterAction.Idle, h.P0.Action);
        }

        [Test]
        public void RageBurstBreaksACombo()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.P1.Rage = 1000;
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.Has(CombatEventType.Hit), 10, "hit");
            h.Tap(1, InputButtons.Rage);
            h.RunUntil(() => h.MoveId(1) == "universal.rage", 20, "burst");
            h.RunUntil(() => h.P0.Action == FighterAction.Launched || h.P0.Action == FighterAction.Knockdown, 20, "attacker blown away");
            Assert.IsTrue(h.Has(CombatEventType.ComboEnd));
        }

        [Test]
        public void RageBurstCanBeDisabledByRules()
        {
            var h = new CombatHarness(setup => setup.Rules.RageBurstEnabled = false);
            h.PlaceApart(1.2);
            h.P1.Rage = 1000;
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.Has(CombatEventType.Hit), 10, "hit");
            h.Tap(1, InputButtons.Rage);
            h.Wait(10);
            Assert.AreNotEqual("universal.rage", h.MoveId(1));
        }

        [Test]
        public void UmbralShadowEchoesEveryHit()
        {
            var h = new CombatHarness();
            h.PlaceApart(3);
            h.P0.Shadow = 1000;
            h.Tap(0, InputButtons.Shadow);
            Assert.IsTrue(h.Has(CombatEventType.ShadowStart));
            h.RunUntil(() => h.P0.Action == FighterAction.Idle, 40, "activation ends");

            h.PlaceApart(1.2);
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.Has(CombatEventType.Hit), 10, "hit");
            int hitFrame = h.First(CombatEventType.Hit).Frame;
            h.RunUntil(() => h.Has(CombatEventType.ShadowEcho), 20, "echo");
            var echo = h.First(CombatEventType.ShadowEcho);
            Assert.AreEqual(hitFrame + 10, echo.Frame);
            Assert.AreEqual(12, echo.Value, "30% of 40");
            Assert.AreEqual(1000 - 40 - 12, h.P1.Health);
        }

        [Test]
        public void UltimateCinematicDealsScriptedDamage()
        {
            var h = new CombatHarness();
            h.PlaceApart(2.0);
            h.P0.Ultimate = 1000;
            h.Tap(0, InputButtons.Ultimate);
            Assert.AreEqual("katana.ultimate", h.MoveId(0));
            h.RunUntil(() => h.Has(CombatEventType.UltimateCinematic), 30, "cinematic");
            Assert.AreEqual(FighterAction.PairedVictim, h.P1.Action);
            h.RunUntil(() => h.Has(CombatEventType.PairedRelease), 180, "release");
            Assert.AreEqual(7, h.Count(CombatEventType.PairedHit));
            Assert.AreEqual(1000 - 60 - 280, h.P1.Health);
            h.RunUntil(() => h.P1.Action == FighterAction.Knockdown, 60, "knockdown");
            Assert.IsTrue(h.P1.HardKnockdown);
        }

        [Test]
        public void UltimateIsInvincibleOnStartup()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.P1.Ultimate = 1000;
            h.Tap(0, InputButtons.Light);
            h.Wait(3);
            h.Tap(1, InputButtons.Ultimate);
            h.Wait(20);
            Assert.AreEqual(1000, h.P1.Health, "the jab passes through the ultimate's invincibility");
            Assert.IsTrue(h.Has(CombatEventType.UltimateCinematic));
        }

        [Test]
        public void StillwaterCountersAStrike()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Tap(0, InputButtons.Special);
            Assert.AreEqual("katana.s", h.MoveId(0));
            h.Tap(1, InputButtons.Light);
            h.RunUntil(() => h.Has(CombatEventType.CounterStance), 15, "counter");
            Assert.AreEqual("katana.sreprisal", h.MoveId(0));
            h.RunUntil(() => h.Has(CombatEventType.Hit), 15, "reprisal");
            var reprisal = h.First(CombatEventType.Hit);
            Assert.AreEqual(0, reprisal.Actor);
            Assert.IsTrue(reprisal.Has(CombatEventFlags.Counter), "the caught attacker is still mid-attack");
            Assert.AreEqual(1000, h.P0.Health);
            Assert.AreEqual(1000 - 132, h.P1.Health, "110 × 1.2 counter hit");
        }

        [Test]
        public void StillwaterRecoveryIsPunishable()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Tap(0, InputButtons.Special);
            h.Wait(26);
            h.Tap(1, InputButtons.Light);
            h.RunUntil(() => h.Has(CombatEventType.Hit), 10, "punish");
            Assert.AreEqual(1, h.First(CombatEventType.Hit).Actor);
            Assert.IsTrue(h.First(CombatEventType.Hit).Has(CombatEventFlags.Punish));
        }

        [Test]
        public void SeveringWindTravelsAndHits()
        {
            var h = new CombatHarness();
            h.PlaceApart(5);
            h.Motion(0, "214", InputButtons.Special);
            Assert.AreEqual("katana.qcbs", h.MoveId(0));
            h.RunUntil(() => h.Has(CombatEventType.ProjectileSpawn), 20, "spawn");
            h.RunUntil(() => h.Has(CombatEventType.Hit), 60, "projectile hit");
            var hit = h.First(CombatEventType.Hit);
            Assert.IsTrue(hit.Has(CombatEventFlags.Projectile));
            Assert.AreEqual(55, hit.Value);
            Assert.AreEqual(0, h.P0.HitstopRemaining, "projectile hits never freeze the thrower");
            h.Step();
            Assert.IsTrue(h.Has(CombatEventType.ProjectileEnd));
        }

        [Test]
        public void ProjectilesClash()
        {
            var h = new CombatHarness();
            h.PlaceApart(6);
            h.Step(direction0: 2, direction1: 2);
            h.Step(direction0: 1, direction1: 1);
            h.Step(InputButtons.Special, InputButtons.Special, 4, 4);
            Assert.AreEqual("katana.qcbs", h.MoveId(0));
            Assert.AreEqual("katana.qcbs", h.MoveId(1));
            h.RunUntil(() => h.Has(CombatEventType.ProjectileClash), 60, "clash");
            h.Wait(60);
            Assert.AreEqual(0, h.Count(CombatEventType.Hit));
            Assert.AreEqual(2, h.Count(CombatEventType.ProjectileEnd));
        }

        [Test]
        public void HoldingHeavyChargesAndFullChargeCrushesGuard()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.4);
            h.Hold(1, InputButtons.Guard);
            h.Hold(0, InputButtons.Heavy);
            h.Wait(10);
            Assert.AreEqual("katana.hcharge", h.MoveId(0));
            h.Wait(35);
            h.Release(0, InputButtons.Heavy);
            h.Step();
            Assert.AreEqual("katana.hchargedfull", h.MoveId(0));
            h.RunUntil(() => h.Has(CombatEventType.GuardBreak), 30, "guard crush");
            Assert.AreEqual(FighterAction.Staggered, h.P1.Action);
        }

        [Test]
        public void EarlyReleaseGivesThePartialCharge()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.4);
            h.Hold(0, InputButtons.Heavy);
            h.Wait(15);
            h.Release(0, InputButtons.Heavy);
            h.Step();
            Assert.AreEqual("katana.hcharged", h.MoveId(0));
        }

        [Test]
        public void TappingHeavyGivesIronDraw()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.4);
            h.Tap(0, InputButtons.Heavy);
            h.Wait(12);
            Assert.AreEqual("katana.h1", h.MoveId(0));
        }

        [Test]
        public void SimplifiedSpecialsCostTwoFramesOfStartup()
        {
            var classic = new CombatHarness();
            classic.PlaceApart(3);
            classic.Motion(0, "236", InputButtons.Special);
            int classicStart = classic.Frame;
            classic.RunUntil(() => classic.Has(CombatEventType.Hit), 40, "classic hit");
            int classicStartup = classic.First(CombatEventType.Hit).Frame - classicStart;

            var simplified = new CombatHarness(setup => setup.Combatants[0].Scheme = ControlScheme.Simplified);
            simplified.PlaceApart(3);
            simplified.Tap(0, InputButtons.Special, 6);
            Assert.AreEqual("katana.qcfs", simplified.MoveId(0));
            int simplifiedStart = simplified.Frame;
            simplified.RunUntil(() => simplified.Has(CombatEventType.Hit), 40, "simplified hit");
            int simplifiedStartup = simplified.First(CombatEventType.Hit).Frame - simplifiedStart;

            Assert.AreEqual(classicStartup + 2, simplifiedStartup);
        }

        [Test]
        public void ClassicSchemeCannotUseSimplifiedShortcuts()
        {
            var h = new CombatHarness();
            h.PlaceApart(3);
            h.Tap(0, InputButtons.Special, 6);
            Assert.AreEqual("katana.s", h.MoveId(0), "6+S is just Stillwater in Classic");
        }

        [Test]
        public void AscendingDragonIsAnInvincibleReversal()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Tap(1, InputButtons.Light);
            h.Wait(1);
            h.Step(direction0: 6);
            h.Step(direction0: 2);
            h.Step(InputButtons.Special, InputButtons.None, 3, 0);
            Assert.AreEqual("katana.dps", h.MoveId(0));
            h.RunUntil(() => h.Has(CombatEventType.Hit), 20, "reversal");
            Assert.AreEqual(0, h.First(CombatEventType.Hit).Actor);
            Assert.AreEqual(1000, h.P0.Health);
        }

        [Test]
        public void DashCancelsIntoAttacks()
        {
            var h = new CombatHarness();
            h.PlaceApart(6);
            h.Tap(0, InputButtons.Dodge, 6);
            h.Wait(9);
            h.Tap(0, InputButtons.Light);
            Assert.AreEqual("katana.l1", h.MoveId(0));
        }

        [Test]
        public void SoftKnockdownCanBeTechRolled()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Tap(0, InputButtons.Heavy, 2);
            h.RunUntil(() => h.P1.Action == FighterAction.Knockdown, 60, "sweep knockdown");
            h.Tap(1, InputButtons.Dodge);
            Assert.IsTrue(h.Has(CombatEventType.TechRoll));
            Assert.AreEqual("universal.techroll", h.MoveId(1));
        }

        [Test]
        public void WakeUpIsInvulnerable()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Tap(0, InputButtons.Heavy, 2);
            h.RunUntil(() => h.P1.Action == FighterAction.WakeUp, 120, "wake up");
            h.ClearEvents();
            h.PlaceApart(1.0);
            h.Tap(0, InputButtons.Light);
            h.Wait(10);
            Assert.AreEqual(0, h.Count(CombatEventType.Hit));
        }
    }
}

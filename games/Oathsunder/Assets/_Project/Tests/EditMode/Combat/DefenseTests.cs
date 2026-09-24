using Oathsunder.Combat.Events;
using Oathsunder.Combat.Input;
using Oathsunder.Combat.Simulation;
using Oathsunder.Tests.Support;
using NUnit.Framework;

namespace Oathsunder.Tests.Combat
{
    [TestFixture]
    public sealed class DefenseTests
    {
        [Test]
        public void StandingGuardBlocksMidsAndBuildsPosture()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Hold(1, InputButtons.Guard);
            h.Wait(10);
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.Has(CombatEventType.Block), 10, "block");
            Assert.AreEqual(FighterAction.Blockstun, h.P1.Action);
            Assert.AreEqual(13, h.P1.StunRemaining);
            Assert.AreEqual(1000, h.P1.Health, "no chip on normals");
            Assert.AreEqual(60, h.P1.Posture);
            Assert.AreEqual(0, h.Count(CombatEventType.Hit));
        }

        [Test]
        public void BlockAdvantageMatchesFrameData()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Hold(1, InputButtons.Guard);
            h.Wait(10);
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.Has(CombatEventType.Block), 10, "block");
            h.Release(1, InputButtons.Guard);
            int attackerFree = h.RunUntil(() => h.P0.Action != FighterAction.Move, 60, "attacker recovers");
            int defenderFree = attackerFree + h.RunUntil(() => h.P1.Action != FighterAction.Blockstun, 60, "defender recovers");
            Assert.AreEqual(0, defenderFree - attackerFree, "katana.l1 is 0 on block");
        }

        [Test]
        public void LowsBeatStandingGuardAndLoseToCrouchGuard()
        {
            var standing = new CombatHarness();
            standing.PlaceApart(1.2);
            standing.Hold(1, InputButtons.Guard);
            standing.Wait(10);
            standing.Tap(0, InputButtons.Light, 2);
            standing.RunUntil(() => standing.Has(CombatEventType.Hit), 10, "low hits");
            Assert.IsTrue(standing.First(CombatEventType.Hit).Has(CombatEventFlags.Low));

            var crouching = new CombatHarness();
            crouching.PlaceApart(1.2);
            crouching.Hold(1, InputButtons.Guard);
            crouching.HoldDirection(1, 2);
            crouching.Wait(10);
            Assert.AreEqual(FighterAction.GuardCrouch, crouching.P1.Action);
            crouching.Tap(0, InputButtons.Light, 2);
            crouching.RunUntil(() => crouching.Has(CombatEventType.Block), 10, "low blocked");
        }

        [Test]
        public void OverheadsBeatCrouchGuardAndLoseToStandingGuard()
        {
            var crouching = new CombatHarness();
            crouching.PlaceApart(1.2);
            crouching.Hold(1, InputButtons.Guard);
            crouching.HoldDirection(1, 2);
            crouching.Step();
            crouching.Tap(0, InputButtons.Heavy, 4);
            Assert.AreEqual("katana.bh", crouching.MoveId(0));
            crouching.RunUntil(() => crouching.Has(CombatEventType.Hit), 30, "overhead hits");
            Assert.IsTrue(crouching.First(CombatEventType.Hit).Has(CombatEventFlags.Overhead));

            var standing = new CombatHarness();
            standing.PlaceApart(1.2);
            standing.Hold(1, InputButtons.Guard);
            standing.Step();
            standing.Tap(0, InputButtons.Heavy, 4);
            standing.RunUntil(() => standing.Has(CombatEventType.Block), 30, "overhead blocked");
        }

        [Test]
        public void PerfectParryDeflectsAndPunishesPosture()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Tap(0, InputButtons.Light);
            h.Wait(2);
            h.Tap(1, InputButtons.Guard);
            h.RunUntil(() => h.Has(CombatEventType.Parry), 10, "parry");
            var parry = h.First(CombatEventType.Parry);
            Assert.AreEqual(1, parry.Actor);
            Assert.AreEqual(0, parry.Target);
            Assert.AreEqual(FighterAction.ParryRecovery, h.P1.Action);
            Assert.AreEqual(1000, h.P1.Health);
            Assert.AreEqual(90, h.P0.Posture, "60 posture × 1.5");
            Assert.AreEqual(150, h.P1.Shadow);
            Assert.AreEqual(0, h.Count(CombatEventType.Hit));
        }

        [Test]
        public void ParryDeniesTheAttackersCancel()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Tap(0, InputButtons.Light);
            h.Wait(2);
            h.Tap(1, InputButtons.Guard);
            h.RunUntil(() => h.Has(CombatEventType.Parry), 10, "parry");
            h.Tap(0, InputButtons.Light);
            h.Wait(20);
            Assert.AreNotEqual("katana.l2", h.MoveId(0));
            Assert.AreEqual(0, h.Count(CombatEventType.Hit));
        }

        [Test]
        public void GuardPressedTooEarlyIsOnlyABlock()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Hold(1, InputButtons.Guard);
            h.Step();
            h.Wait(8);
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.Has(CombatEventType.Block), 10, "block");
            Assert.AreEqual(0, h.Count(CombatEventType.Parry));
        }

        [Test]
        public void MashingGuardShrinksTheParryWindow()
        {
            // A clean press 3 frames before contact parries…
            var clean = new CombatHarness();
            clean.PlaceApart(1.2);
            clean.Tap(0, InputButtons.Light);
            clean.Wait(1);
            clean.Tap(1, InputButtons.Guard);
            clean.RunUntil(() => clean.Has(CombatEventType.Parry) || clean.Has(CombatEventType.Hit), 10, "contact");
            Assert.IsTrue(clean.Has(CombatEventType.Parry));

            // …but the same press shortly after another one only gets the 2-frame mash window.
            var mashed = new CombatHarness();
            mashed.PlaceApart(1.2);
            mashed.Tap(1, InputButtons.Guard);
            mashed.Wait(8);
            mashed.Tap(0, InputButtons.Light);
            mashed.Wait(1);
            mashed.Tap(1, InputButtons.Guard);
            mashed.RunUntil(() => mashed.Has(CombatEventType.Parry) || mashed.Has(CombatEventType.Hit), 10, "contact");
            Assert.IsFalse(mashed.Has(CombatEventType.Parry));
            Assert.IsTrue(mashed.Has(CombatEventType.Hit));
        }

        [Test]
        public void PerfectDodgeTriggersShadowTime()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Tap(0, InputButtons.Light);
            h.Wait(1);
            h.Tap(1, InputButtons.Dodge);
            Assert.AreEqual("universal.dodge", h.MoveId(1));
            h.RunUntil(() => h.Has(CombatEventType.PerfectDodge), 10, "perfect dodge");
            Assert.AreEqual(0, h.Count(CombatEventType.Hit));
            Assert.AreEqual(350, h.P0.TimeScalePermille);
            Assert.AreEqual(200, h.P1.Shadow);
            Assert.IsTrue(h.Has(CombatEventType.TimeDilationStart));

            int localBefore = h.P0.LocalFrame;
            h.Wait(40);
            int advanced = h.P0.LocalFrame - localBefore;
            Assert.That(advanced, Is.InRange(13, 15), "attacker runs at 35% speed");
            Assert.AreEqual(0, h.Count(CombatEventType.Hit), "a perfectly dodged attack is spent");
        }

        [Test]
        public void PerfectDodgeOpensARiposteCancel()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Tap(0, InputButtons.Light);
            h.Wait(1);
            h.Tap(1, InputButtons.Dodge);
            h.RunUntil(() => h.Has(CombatEventType.PerfectDodge), 10, "perfect dodge");
            h.RunUntil(() => h.P1.ActionFrame >= 8, 10, "cancel window");
            h.Tap(1, InputButtons.Light);
            Assert.AreEqual("katana.l1", h.MoveId(1));
        }

        [Test]
        public void GuardBreakOpensAnExecution()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Hold(1, InputButtons.Guard);
            h.Wait(10);
            h.P1.Posture = 950;
            h.P1.PostureRegenDelay = 90;
            h.Tap(0, InputButtons.Light);
            h.RunUntil(() => h.Has(CombatEventType.GuardBreak), 10, "guard break");
            Assert.AreEqual(FighterAction.Staggered, h.P1.Action);
            Assert.AreEqual(StaggerKind.GuardBreak, h.P1.Stagger);
            h.Release(1, InputButtons.Guard);

            h.RunUntil(() => h.P0.Action != FighterAction.Move, 30, "attacker recovers");
            h.Tap(0, InputButtons.Execute);
            Assert.AreEqual("katana.execution", h.MoveId(0));
            h.RunUntil(() => h.Has(CombatEventType.ExecutionStart), 10, "execution grabs");
            Assert.AreEqual(FighterAction.PairedVictim, h.P1.Action);
            h.RunUntil(() => h.Has(CombatEventType.PairedRelease), 130, "release");
            Assert.AreEqual(700, h.P1.Health, "execution deals 120 + 180 unscaled");
            h.RunUntil(() => h.P1.Action == FighterAction.Knockdown, 60, "hard knockdown");
            Assert.IsTrue(h.P1.HardKnockdown);
        }

        [Test]
        public void ExecuteDoesNothingWhenTargetIsHealthy()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Tap(0, InputButtons.Execute);
            Assert.AreEqual(FighterAction.Idle, h.P0.Action);
        }

        [Test]
        public void ChipDamageCannotKillUnlessFlagged()
        {
            var h = new CombatHarness();
            h.PlaceApart(4);
            h.P1.Health = 1;
            h.Hold(1, InputButtons.Guard);
            h.Wait(10);
            h.Motion(0, "214", InputButtons.Special);
            Assert.AreEqual("katana.qcbs", h.MoveId(0));
            h.RunUntil(() => h.Has(CombatEventType.Block), 60, "projectile blocked");
            Assert.AreEqual(1, h.P1.Health);
            Assert.IsFalse(h.P1.IsKnockedOut);
        }

        [Test]
        public void PostureRegeneratesAfterTheDelay()
        {
            var h = new CombatHarness();
            h.P1.Posture = 500;
            h.P1.PostureRegenDelay = 90;
            h.Wait(90);
            Assert.AreEqual(500, h.P1.Posture);
            h.Wait(10);
            Assert.AreEqual(500 - (10 * 3), h.P1.Posture, "3 posture per frame once the delay has elapsed");
        }
    }

    [TestFixture]
    public sealed class ThrowTests
    {
        [Test]
        public void ForwardThrowConnectsDamagesAndKnocksDown()
        {
            var h = new CombatHarness();
            h.PlaceApart(0.9);
            h.Tap(0, InputButtons.Grab);
            Assert.AreEqual("universal.throwforward", h.MoveId(0));
            h.RunUntil(() => h.Has(CombatEventType.GrabConnect), 10, "grab");
            Assert.AreEqual(FighterAction.PairedVictim, h.P1.Action);
            h.RunUntil(() => h.Has(CombatEventType.PairedRelease), 40, "release");
            Assert.AreEqual(900, h.P1.Health);
            h.RunUntil(() => h.P1.Action == FighterAction.Knockdown, 60, "knockdown");
            Assert.Greater(h.P1.Position.X.Raw, h.P0.Position.X.Raw, "thrown forward");
        }

        [Test]
        public void ThrowsCanBeTeched()
        {
            var h = new CombatHarness();
            h.PlaceApart(0.9);
            h.Tap(0, InputButtons.Grab);
            h.RunUntil(() => h.Has(CombatEventType.GrabConnect), 10, "grab");
            h.Wait(2);
            h.Tap(1, InputButtons.Grab);
            h.RunUntil(() => h.Has(CombatEventType.ThrowTech), 10, "tech");
            h.Wait(60);
            Assert.AreEqual(1000, h.P1.Health);
            Assert.AreEqual(0, h.Count(CombatEventType.PairedHit));
            Assert.Greater((h.P1.Position.X - h.P0.Position.X).Raw, CombatHarness.M(0.9).Raw, "pushed apart");
        }

        [Test]
        public void TechWindowExpires()
        {
            var h = new CombatHarness();
            h.PlaceApart(0.9);
            h.Tap(0, InputButtons.Grab);
            h.RunUntil(() => h.Has(CombatEventType.GrabConnect), 10, "grab");
            h.Wait(12);
            h.Tap(1, InputButtons.Grab);
            h.RunUntil(() => h.Has(CombatEventType.PairedHit), 30, "throw damage");
            Assert.AreEqual(0, h.Count(CombatEventType.ThrowTech));
        }

        [Test]
        public void StunnedFightersCannotBeThrown()
        {
            var h = new CombatHarness();
            h.PlaceApart(0.9);
            h.P1.Action = FighterAction.Blockstun;
            h.P1.StunRemaining = 30;
            h.Tap(0, InputButtons.Grab);
            h.Wait(10);
            Assert.AreEqual(0, h.Count(CombatEventType.GrabConnect));
        }

        [Test]
        public void BackThrowSwitchesSides()
        {
            var h = new CombatHarness();
            h.PlaceApart(0.9);
            h.Tap(0, InputButtons.Grab, 4);
            Assert.AreEqual("universal.throwback", h.MoveId(0));
            h.RunUntil(() => h.Has(CombatEventType.PairedRelease), 50, "release");
            h.RunUntil(() => h.P1.Action == FighterAction.Knockdown, 60, "knockdown");
            Assert.Less(h.P1.Position.X.Raw, h.P0.Position.X.Raw, "victim ends up behind");
            Assert.AreEqual(890, h.P1.Health);
        }

        [Test]
        public void SimultaneousThrowsBreak()
        {
            var h = new CombatHarness();
            h.PlaceApart(0.9);
            h.Step(InputButtons.Grab, InputButtons.Grab);
            h.RunUntil(() => h.Has(CombatEventType.ThrowTech), 10, "throw clash");
            h.Wait(40);
            Assert.AreEqual(1000, h.P0.Health);
            Assert.AreEqual(1000, h.P1.Health);
        }
    }
}

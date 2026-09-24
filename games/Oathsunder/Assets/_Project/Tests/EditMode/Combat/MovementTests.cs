using Oathsunder.Combat.Events;
using Oathsunder.Combat.Input;
using Oathsunder.Combat.Simulation;
using Oathsunder.Core.Mathematics;
using Oathsunder.Tests.Support;
using NUnit.Framework;

namespace Oathsunder.Tests.Combat
{
    [TestFixture]
    public sealed class MovementTests
    {
        [Test]
        public void IdleFightersDoNotDrift()
        {
            var h = new CombatHarness();
            var p0 = h.P0.Position;
            var p1 = h.P1.Position;
            h.Wait(300);
            Assert.AreEqual(p0, h.P0.Position);
            Assert.AreEqual(p1, h.P1.Position);
            Assert.AreEqual(FighterAction.Idle, h.P0.Action);
            Assert.AreEqual(1, h.P0.Facing);
            Assert.AreEqual(-1, h.P1.Facing);
        }

        [Test]
        public void WalkSpeedsAreExact()
        {
            var h = new CombatHarness();
            h.PlaceApart(6);
            var start = h.P0.Position.X;
            h.HoldDirection(0, 6);
            h.Wait(60);
            Assert.AreEqual(FighterAction.WalkForward, h.P0.Action);
            Assert.AreEqual(start + TestContent.Rhen.WalkForwardSpeed * 60, h.P0.Position.X);

            h.HoldDirection(0, 4);
            var before = h.P0.Position.X;
            h.Wait(30);
            Assert.AreEqual(FighterAction.WalkBackward, h.P0.Action);
            Assert.AreEqual(before - TestContent.Rhen.WalkBackwardSpeed * 30, h.P0.Position.X);
        }

        [Test]
        public void JumpHasDesignedStartupArcAndLanding()
        {
            var h = new CombatHarness();
            h.Tap(0, InputButtons.Jump);
            Assert.AreEqual(FighterAction.PreJump, h.P0.Action);
            // The press frame is pre-jump frame 1; the fighter is airborne on frame PreJumpFrames + 1.
            int framesAfterPress = h.RunUntil(() => !h.P0.Grounded, 10, "leave ground");
            Assert.AreEqual(TestContent.Rhen.PreJumpFrames, framesAfterPress, "pre-jump frames");

            Fixed apex = Fixed.Zero;
            int air = h.RunUntil(() =>
            {
                if (h.P0.Position.Y > apex)
                {
                    apex = h.P0.Position.Y;
                }

                return h.P0.Grounded;
            }, 60, "land");
            Assert.That(apex.ToDouble(), Is.InRange(1.6, 1.85), "apex height");
            Assert.That(air, Is.InRange(34, 38), "air time");
            Assert.AreEqual(FighterAction.Landing, h.P0.Action);
            h.Wait(TestContent.Rhen.LandingFrames);
            Assert.AreEqual(FighterAction.Landing, h.P0.Action, "landing lasts exactly LandingFrames");
            h.Step();
            Assert.AreEqual(FighterAction.Idle, h.P0.Action);
        }

        [Test]
        public void BodiesCannotOverlap()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.HoldDirection(0, 6);
            h.Wait(90);
            Fixed gap = h.P1.Position.X - h.P0.Position.X;
            Assert.GreaterOrEqual(gap.ToDouble(), 0.56 - 1e-4, "pushboxes (0.28 half-width each) must not overlap");
            Assert.Greater(h.P1.Position.X.ToDouble(), 0.6, "the walker pushes the opponent back");
        }

        [Test]
        public void WallsStopFighters()
        {
            var h = new CombatHarness();
            h.PlaceApart(5, centre: -7.5);
            h.HoldDirection(0, 4);
            h.Wait(300);
            var stage = TestContent.Stage("stage.training.dojo");
            Assert.AreEqual(stage.LeftWall + CombatHarness.M(0.28), h.P0.Position.X);
        }

        [Test]
        public void CameraSeparationIsLimited()
        {
            var h = new CombatHarness();
            h.HoldDirection(0, 4);
            h.HoldDirection(1, 4);
            h.Wait(400);
            Fixed separation = h.P1.Position.X - h.P0.Position.X;
            Assert.LessOrEqual(separation.ToDouble(), 8.0 + 1e-4);
            Assert.Greater(separation.ToDouble(), 7.9);
        }

        [Test]
        public void JumpingOverTheOpponentSwitchesSides()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.4);
            h.HoldDirection(0, 9);
            h.Tap(0, InputButtons.Jump, 9);
            h.RunUntil(() => h.P0.Grounded && h.P0.Action != FighterAction.PreJump, 80, "land after cross-up");
            h.ReleaseAll();
            h.Wait(5);
            Assert.IsTrue(h.P0.Position.X > h.P1.Position.X, "player 0 landed on the far side");
            Assert.AreEqual(-1, h.P0.Facing);
            Assert.AreEqual(1, h.P1.Facing);
        }

        [Test]
        public void DashTravelsTheDesignedDistance()
        {
            var h = new CombatHarness();
            h.PlaceApart(8);
            var start = h.P0.Position.X;
            h.Tap(0, InputButtons.Dodge, 6);
            Assert.AreEqual("universal.dash", h.MoveId(0));
            h.Wait(25);
            double travelled = (h.P0.Position.X - start).ToDouble();
            Assert.AreEqual(13.0 / 60 * 12 + 4.0 / 60 * 4, travelled, 1e-3);
        }

        [Test]
        public void DoubleTapForwardDashes()
        {
            var h = new CombatHarness();
            h.PlaceApart(8);
            h.Step(direction0: 6);
            h.Step(direction0: 5);
            h.Step(direction0: 6);
            Assert.AreEqual("universal.dash", h.MoveId(0));
        }

        [Test]
        public void BackstepIsInvulnerableEarly()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.2);
            h.Tap(1, InputButtons.Light);
            h.Wait(2);
            h.Tap(0, InputButtons.Dodge, 4);
            Assert.AreEqual("universal.backstep", h.MoveId(0));
            h.Wait(20);
            Assert.AreEqual(0, h.Count(CombatEventType.Hit), "the light attack passes through the backstep");
        }

        [Test]
        public void RollPassesThroughTheOpponent()
        {
            var h = new CombatHarness();
            h.PlaceApart(1.0);
            h.Tap(0, InputButtons.Dodge, 3);
            Assert.AreEqual("universal.rollforward", h.MoveId(0));
            h.Wait(40);
            Assert.IsTrue(h.P0.Position.X > h.P1.Position.X, "rolled through");
        }
    }
}

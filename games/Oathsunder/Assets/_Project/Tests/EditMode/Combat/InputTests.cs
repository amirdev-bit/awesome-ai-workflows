using System;
using Oathsunder.Combat.Input;
using NUnit.Framework;

namespace Oathsunder.Tests.Combat
{
    [TestFixture]
    public sealed class InputTests
    {
        private static InputFrame Buttons(InputButtons b) => new InputFrame(b, 0, 0);

        private static InputFrame Dir(int numpad, InputButtons b = InputButtons.None)
        {
            int x = ((numpad - 1) % 3) - 1;
            int y = ((numpad - 1) / 3) - 1;
            return new InputFrame(b, x, y);
        }

        [Test]
        public void PackRoundTripsEveryCombination()
        {
            for (int buttons = 0; buttons <= (int)InputButtons.All; buttons += 37)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        var frame = new InputFrame((InputButtons)buttons, x, y);
                        Assert.AreEqual(frame, InputFrame.Unpack(frame.Pack()));
                        Assert.Less(frame.Pack(), 1 << 15);
                    }
                }
            }
        }

        [Test]
        public void DirectionsAreFacingRelative()
        {
            var right = new InputFrame(InputButtons.None, 1, 0);
            Assert.AreEqual(NumpadDirection.Forward, right.Direction(1));
            Assert.AreEqual(NumpadDirection.Back, right.Direction(-1));
            Assert.AreEqual(NumpadDirection.DownBack, new InputFrame(InputButtons.None, -1, -1).Direction(1));
            Assert.AreEqual(NumpadDirection.UpForward, new InputFrame(InputButtons.None, -1, 1).Direction(-1));
        }

        [Test]
        public void ParseMaskSupportsDigitsAndNames()
        {
            Assert.AreEqual(DirectionMask.AnyDown, DirectionUtility.ParseMask("123"));
            Assert.AreEqual(DirectionMask.AnyDown, DirectionUtility.ParseMask("down"));
            Assert.AreEqual(DirectionMask.NotDown, DirectionUtility.ParseMask("notdown"));
            Assert.Throws<FormatException>(() => DirectionUtility.ParseMask("0"));
        }

        [Test]
        public void RecordRequiresConsecutiveFrames()
        {
            var history = new InputHistory();
            history.Record(1, 1, InputFrame.Neutral);
            Assert.Throws<InvalidOperationException>(() => history.Record(3, 3, InputFrame.Neutral));
        }

        [Test]
        public void PressIsBufferedForTheWindowThenExpires()
        {
            var history = new InputHistory();
            history.Record(1, 1, Buttons(InputButtons.Light));
            for (int frame = 2; frame <= 9; frame++)
            {
                history.Record(frame, frame, InputFrame.Neutral);
                Assert.AreEqual(1, history.FindBufferedPress(InputButtons.Light, frame, 8, 3), $"frame {frame}");
            }

            history.Record(10, 10, InputFrame.Neutral);
            Assert.AreEqual(-1, history.FindBufferedPress(InputButtons.Light, 10, 8, 3));
        }

        [Test]
        public void HitstopDoesNotAgeTheBuffer()
        {
            // Local frames stop while the fighter is frozen: a press during a 20-frame hitstop stays fresh.
            var history = new InputHistory();
            history.Record(1, 1, InputFrame.Neutral);
            history.Record(2, 1, Buttons(InputButtons.Heavy));
            for (int frame = 3; frame <= 22; frame++)
            {
                history.Record(frame, 1, InputFrame.Neutral);
            }

            history.Record(23, 2, InputFrame.Neutral);
            Assert.AreEqual(2, history.FindBufferedPress(InputButtons.Heavy, 2, 8, 3));
        }

        [Test]
        public void ConsumptionKeepsNewerPressesInOrder()
        {
            var history = new InputHistory();
            history.Record(1, 1, Buttons(InputButtons.Light));
            history.Record(2, 2, InputFrame.Neutral);
            history.Record(3, 3, Buttons(InputButtons.Heavy));
            Assert.AreEqual(1, history.FindBufferedPress(InputButtons.Light, 3, 8, 3));
            history.ConsumeThrough(1);
            Assert.AreEqual(-1, history.FindBufferedPress(InputButtons.Light, 3, 8, 3));
            Assert.AreEqual(3, history.FindBufferedPress(InputButtons.Heavy, 3, 8, 3));
            history.ConsumeThrough(3);
            Assert.AreEqual(-1, history.FindBufferedPress(InputButtons.Heavy, 3, 8, 3));
        }

        [Test]
        public void HoldingIsNotPressing()
        {
            var history = new InputHistory();
            history.Record(1, 1, Buttons(InputButtons.Light));
            history.Record(2, 2, Buttons(InputButtons.Light));
            history.ConsumeThrough(1);
            Assert.AreEqual(-1, history.FindBufferedPress(InputButtons.Light, 2, 8, 3));
            Assert.IsTrue(history.IsHeld(2, InputButtons.Light));
        }

        [Test]
        public void ChordsCompleteWithinTheWindow()
        {
            var history = new InputHistory();
            history.Record(1, 1, Buttons(InputButtons.Heavy));
            history.Record(2, 2, Buttons(InputButtons.Heavy));
            history.Record(3, 3, Buttons(InputButtons.Heavy | InputButtons.Special));
            Assert.AreEqual(3, history.FindBufferedPress(InputButtons.Heavy | InputButtons.Special, 3, 8, 3));

            var late = new InputHistory();
            late.Record(1, 1, Buttons(InputButtons.Heavy));
            for (int f = 2; f <= 5; f++)
            {
                late.Record(f, f, InputFrame.Neutral);
            }

            late.Record(6, 6, Buttons(InputButtons.Special));
            Assert.AreEqual(-1, late.FindBufferedPress(InputButtons.Heavy | InputButtons.Special, 6, 8, 3));
        }

        [Test]
        public void CopyFromSnapshotsEverything()
        {
            var a = new InputHistory();
            a.Record(1, 1, Dir(6, InputButtons.Light));
            a.ConsumeThrough(1);
            var b = new InputHistory();
            b.CopyFrom(a);
            Assert.AreEqual(a.LatestFrame, b.LatestFrame);
            Assert.AreEqual(a.ConsumedThroughFrame, b.ConsumedThroughFrame);
            Assert.AreEqual(a.Get(1), b.Get(1));
        }

        [Test]
        public void RingBufferWrapsSafely()
        {
            var history = new InputHistory();
            for (int frame = 1; frame <= InputHistory.Capacity * 3; frame++)
            {
                history.Record(frame, frame, frame % 17 == 0 ? Buttons(InputButtons.Light) : InputFrame.Neutral);
            }

            int latest = history.LatestFrame;
            Assert.IsFalse(history.Contains(latest - InputHistory.Capacity));
            Assert.IsTrue(history.Contains(latest - InputHistory.Capacity + 1));
            Assert.AreEqual(InputFrame.Neutral, history.Get(1));
        }
    }

    [TestFixture]
    public sealed class MotionCommandTests
    {
        private static InputHistory Feed(params int[] numpads)
        {
            var history = new InputHistory();
            for (int i = 0; i < numpads.Length; i++)
            {
                int n = numpads[i];
                history.Record(i + 1, i + 1, new InputFrame(InputButtons.None, ((n - 1) % 3) - 1, ((n - 1) / 3) - 1));
            }

            return history;
        }

        [TestCase(new[] { 5, 2, 3, 6 }, true)]
        [TestCase(new[] { 2, 2, 3, 3, 6, 6 }, true)]
        [TestCase(new[] { 1, 2, 3, 6 }, true)]
        [TestCase(new[] { 2, 3, 6, 5, 5 }, true)]
        [TestCase(new[] { 2, 6 }, false)]
        [TestCase(new[] { 3, 6 }, false)]
        [TestCase(new[] { 6, 3, 2 }, false)]
        public void QuarterCircleForward(int[] sequence, bool expected)
        {
            var history = Feed(sequence);
            Assert.AreEqual(expected, StandardMotions.QuarterCircleForward.MatchesBefore(history, history.LatestFrame, 1, 1));
        }

        [Test]
        public void MotionIsFacingRelative()
        {
            // 2-1-4 in absolute terms is a quarter circle forward for a fighter facing left.
            var history = Feed(2, 1, 4);
            Assert.IsTrue(StandardMotions.QuarterCircleForward.MatchesBefore(history, 3, -1, 1));
            Assert.IsFalse(StandardMotions.QuarterCircleForward.MatchesBefore(history, 3, 1, 1));
            Assert.IsTrue(StandardMotions.QuarterCircleBack.MatchesBefore(history, 3, 1, 1));
        }

        [Test]
        public void DragonMotionAndShortcut()
        {
            var classic = Feed(6, 2, 3);
            Assert.IsTrue(StandardMotions.DragonForward.MatchesBefore(classic, 3, 1, 1));
            var shortcut = Feed(3, 2, 3);
            Assert.IsTrue(StandardMotions.DragonForward.MatchesBefore(shortcut, 3, 1, 1));
        }

        [Test]
        public void WalkingIntoAQuarterCircleIsNotADragon()
        {
            var history = Feed(6, 6, 2, 3, 6);
            Assert.IsFalse(StandardMotions.DragonForward.MatchesBefore(history, history.LatestFrame, 1, 1));
            Assert.IsTrue(StandardMotions.QuarterCircleForward.MatchesBefore(history, history.LatestFrame, 1, 1));
        }

        [Test]
        public void MotionTooSlowFails()
        {
            var history = Feed(2, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 3, 6);
            Assert.IsFalse(StandardMotions.QuarterCircleForward.MatchesBefore(history, history.LatestFrame, 1, 1));
        }

        [Test]
        public void DoubleTapNeedsARelease()
        {
            var dash = Feed(6, 5, 6);
            Assert.IsTrue(StandardMotions.DoubleTapForward.CompletesOn(dash, 3, 1, 1));
            var held = Feed(6, 6, 6);
            Assert.IsFalse(StandardMotions.DoubleTapForward.CompletesOn(held, 3, 1, 1));
            var slow = Feed(6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 6);
            Assert.IsFalse(StandardMotions.DoubleTapForward.CompletesOn(slow, slow.LatestFrame, 1, 1));
        }

        [Test]
        public void MotionFloorPreventsReusingConsumedDirections()
        {
            var history = Feed(2, 3, 6, 5, 5);
            Assert.IsTrue(StandardMotions.QuarterCircleForward.MatchesBefore(history, 4, 1, 1));
            Assert.IsFalse(StandardMotions.QuarterCircleForward.MatchesBefore(history, 4, 1, 3));
        }
    }
}

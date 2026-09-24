using System;
using Oathsunder.Core.Hashing;

namespace Oathsunder.Combat.Input
{
    /// <summary>
    /// Per-fighter ring buffer of recent inputs, plus input-consumption bookkeeping. Part of the rollback state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every entry stores the world frame it was recorded on and the fighter's <i>local</i> frame at that moment.
    /// Local frames only advance while the fighter is actually simulating (not during hitstop or skipped
    /// time-dilated ticks), so the input buffer "pauses" during hitstop: a button pressed during a 12-frame
    /// hitstop is still fresh when the fighter resumes. This is what makes buffered combo links feel reliable.
    /// </para>
    /// <para>
    /// Consumption is a single high-water mark: when a command fires from a press on world frame <c>F</c>, every
    /// press at or before <c>F</c> is consumed. Newer presses stay buffered, which preserves the player's input
    /// order (L then H performs L2 then the launcher, never the reverse).
    /// </para>
    /// </remarks>
    public sealed class InputHistory
    {
        /// <summary>Number of frames kept. Must be a power of two.</summary>
        public const int Capacity = 128;

        private const int IndexMask = Capacity - 1;

        private readonly ushort[] _packed = new ushort[Capacity];
        private readonly int[] _localFrames = new int[Capacity];

        /// <summary>World frame of the newest entry (0 when empty).</summary>
        public int LatestFrame { get; private set; }

        /// <summary>Presses on or before this world frame have been consumed.</summary>
        public int ConsumedThroughFrame { get; private set; }

        /// <summary>Oldest world frame still stored.</summary>
        public int OldestFrame => LatestFrame == 0 ? 0 : Math.Max(1, LatestFrame - Capacity + 1);

        /// <summary>
        /// Earliest frame on which a press edge can be detected. Before the ring wraps, frame 1 qualifies (the
        /// match starts from neutral); afterwards the oldest stored frame has no known predecessor.
        /// </summary>
        public int EarliestEdgeFrame => LatestFrame < Capacity ? 1 : OldestFrame + 1;

        /// <summary>Clears all history.</summary>
        public void Reset()
        {
            Array.Clear(_packed, 0, Capacity);
            Array.Clear(_localFrames, 0, Capacity);
            LatestFrame = 0;
            ConsumedThroughFrame = 0;
        }

        /// <summary>Records the input of a world frame. Frames must be recorded consecutively.</summary>
        /// <exception cref="InvalidOperationException">A frame was skipped or repeated.</exception>
        public void Record(int worldFrame, int localFrame, InputFrame input)
        {
            if (LatestFrame != 0 && worldFrame != LatestFrame + 1)
            {
                throw new InvalidOperationException($"Input frames must be consecutive (expected {LatestFrame + 1}, got {worldFrame}).");
            }

            _packed[worldFrame & IndexMask] = input.Pack();
            _localFrames[worldFrame & IndexMask] = localFrame;
            LatestFrame = worldFrame;
        }

        /// <summary>True when the given world frame is stored.</summary>
        public bool Contains(int worldFrame) => worldFrame >= OldestFrame && worldFrame <= LatestFrame && worldFrame >= 1;

        /// <summary>Input of a world frame, or neutral when not stored.</summary>
        public InputFrame Get(int worldFrame) => Contains(worldFrame) ? InputFrame.Unpack(_packed[worldFrame & IndexMask]) : InputFrame.Neutral;

        /// <summary>The fighter's local frame when the given world frame was recorded.</summary>
        public int LocalFrameAt(int worldFrame) => Contains(worldFrame) ? _localFrames[worldFrame & IndexMask] : int.MinValue / 2;

        /// <summary>True when every button in <paramref name="buttons"/> was held on the frame.</summary>
        public bool IsHeld(int worldFrame, InputButtons buttons) => Get(worldFrame).IsHeld(buttons);

        /// <summary>True when <paramref name="button"/> went from released to held on the frame.</summary>
        public bool WasPressed(int worldFrame, InputButtons button)
        {
            if (!Contains(worldFrame))
            {
                return false;
            }

            if ((Get(worldFrame).Buttons & button) == 0)
            {
                return false;
            }

            return (Get(worldFrame - 1).Buttons & button) == 0;
        }

        /// <summary>
        /// Earliest world frame within the local-frame <paramref name="bufferWindow"/> that is not consumed.
        /// Returns <see cref="int.MaxValue"/> when nothing is available.
        /// </summary>
        public int EarliestBufferedFrame(int currentLocalFrame, int bufferWindow)
        {
            if (LatestFrame == 0)
            {
                return int.MaxValue;
            }

            int earliestAllowed = Math.Max(ConsumedThroughFrame + 1, EarliestEdgeFrame);
            int frame = LatestFrame;
            if (frame < earliestAllowed)
            {
                return int.MaxValue;
            }

            while (frame - 1 >= earliestAllowed && currentLocalFrame - LocalFrameAt(frame - 1) <= bufferWindow)
            {
                frame--;
            }

            return currentLocalFrame - LocalFrameAt(frame) <= bufferWindow ? frame : int.MaxValue;
        }

        /// <summary>
        /// Finds the earliest unconsumed, still-buffered frame on which the button chord was completed.
        /// A chord is complete on frame <c>F</c> when one of its buttons is pressed on <c>F</c> and every other
        /// button was pressed within <paramref name="chordWindow"/> frames before <c>F</c>.
        /// </summary>
        /// <returns>The world frame, or -1 when no buffered press exists.</returns>
        public int FindBufferedPress(InputButtons chord, int currentLocalFrame, int bufferWindow, int chordWindow)
        {
            if (chord == InputButtons.None)
            {
                return -1;
            }

            int start = EarliestBufferedFrame(currentLocalFrame, bufferWindow);
            if (start == int.MaxValue)
            {
                return -1;
            }

            for (int frame = start; frame <= LatestFrame; frame++)
            {
                if (IsChordCompletedOn(frame, chord, chordWindow))
                {
                    return frame;
                }
            }

            return -1;
        }

        /// <summary>
        /// True when the chord was completed on <paramref name="frame"/> using only unconsumed presses: one of its
        /// buttons is pressed on the frame and every other button was pressed within <paramref name="chordWindow"/>
        /// frames before it.
        /// </summary>
        public bool IsChordCompletedOn(int frame, InputButtons chord, int chordWindow)
        {
            int earliestAllowed = Math.Max(ConsumedThroughFrame + 1, EarliestEdgeFrame);
            if (chord == InputButtons.None || frame < earliestAllowed)
            {
                return false;
            }

            return IsChordCompletedOn(frame, chord, chordWindow, earliestAllowed);
        }

        /// <summary>
        /// Every button with at least one unconsumed press inside the buffer window. One pass over the window; the
        /// move selector uses it to skip triggers whose buttons were not pressed, which is the common case.
        /// </summary>
        public InputButtons BufferedPresses(int currentLocalFrame, int bufferWindow)
        {
            int start = EarliestBufferedFrame(currentLocalFrame, bufferWindow);
            if (start == int.MaxValue)
            {
                return InputButtons.None;
            }

            int earliestAllowed = Math.Max(ConsumedThroughFrame + 1, EarliestEdgeFrame);
            if (start < earliestAllowed)
            {
                start = earliestAllowed;
            }

            ushort previous = start - 1 >= 1 && Contains(start - 1) ? _packed[(start - 1) & IndexMask] : (ushort)0;
            ushort pressed = 0;
            for (int frame = start; frame <= LatestFrame; frame++)
            {
                ushort current = _packed[frame & IndexMask];
                pressed |= (ushort)(current & ~previous);
                previous = current;
            }

            return (InputButtons)(pressed & (ushort)InputButtons.All);
        }

        /// <summary>Marks all presses up to and including <paramref name="worldFrame"/> as consumed.</summary>
        public void ConsumeThrough(int worldFrame)
        {
            if (worldFrame > ConsumedThroughFrame)
            {
                ConsumedThroughFrame = Math.Min(worldFrame, LatestFrame);
            }
        }

        /// <summary>Copies another history into this one (rollback snapshot / restore).</summary>
        public void CopyFrom(InputHistory other)
        {
            Array.Copy(other._packed, _packed, Capacity);
            Array.Copy(other._localFrames, _localFrames, Capacity);
            LatestFrame = other.LatestFrame;
            ConsumedThroughFrame = other.ConsumedThroughFrame;
        }

        /// <summary>Adds the rollback-relevant content (the live window only) to a checksum.</summary>
        public void AppendHash(ref StateHasher hasher)
        {
            hasher.Add(LatestFrame);
            hasher.Add(ConsumedThroughFrame);
            for (int frame = OldestFrame; frame <= LatestFrame && frame >= 1; frame++)
            {
                hasher.Add((uint)_packed[frame & IndexMask]);
                hasher.Add(_localFrames[frame & IndexMask]);
            }
        }

        private bool IsChordCompletedOn(int frame, InputButtons chord, int chordWindow, int earliestAllowed)
        {
            bool anyPressedNow = false;
            ushort bits = (ushort)chord;
            while (bits != 0)
            {
                ushort lowest = (ushort)(bits & -bits);
                bits = (ushort)(bits & (bits - 1));
                var button = (InputButtons)lowest;
                if (WasPressed(frame, button))
                {
                    anyPressedNow = true;
                    continue;
                }

                bool pressedEarlier = false;
                int from = Math.Max(earliestAllowed, frame - chordWindow);
                for (int earlier = frame - 1; earlier >= from; earlier--)
                {
                    if (WasPressed(earlier, button))
                    {
                        pressedEarlier = true;
                        break;
                    }
                }

                if (!pressedEarlier)
                {
                    return false;
                }
            }

            return anyPressedNow;
        }
    }
}

using Oathsunder.Combat.Input;

namespace Oathsunder.Controls
{
    /// <summary>
    /// Collects device samples taken at render rate (60–240 Hz) and turns them into exactly one
    /// <see cref="InputFrame"/> per 60 Hz simulation tick without losing anything a player did in between.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>A button pressed and released between two ticks is reported as held for one tick, so the
    /// simulation still sees the press edge.</item>
    /// <item>A button held when the tick is built stays held.</item>
    /// <item>The direction is the most recent sample, except that a direction tapped and released inside one
    /// tick is kept for that tick so quick taps (dash double-taps, motion inputs) survive high frame rates.</item>
    /// </list>
    /// </remarks>
    public sealed class InputLatch
    {
        private InputButtons _heldNow;
        private InputButtons _pressedSinceTick;
        private int _x;
        private int _y;
        private int _latchedX;
        private int _latchedY;
        private bool _directionTapped;

        /// <summary>Number of samples received since the last tick (diagnostics).</summary>
        public int SamplesSinceTick { get; private set; }

        /// <summary>Records one device sample.</summary>
        /// <param name="held">Logical buttons currently held.</param>
        /// <param name="x">Digital horizontal direction (-1, 0, 1, absolute).</param>
        /// <param name="y">Digital vertical direction (-1, 0, 1).</param>
        public void Sample(InputButtons held, int x, int y)
        {
            _pressedSinceTick |= held & ~_heldNow;
            _heldNow = held;

            if ((x != 0 || y != 0) && (x != _x || y != _y))
            {
                // Remember the newest non-neutral direction in case it is released before the tick.
                _latchedX = x;
                _latchedY = y;
                _directionTapped = true;
            }

            _x = x;
            _y = y;
            SamplesSinceTick++;
        }

        /// <summary>Builds the frame for the next simulation tick and starts a new accumulation window.</summary>
        public InputFrame ConsumeTick()
        {
            InputButtons buttons = _heldNow | _pressedSinceTick;
            int x = _x;
            int y = _y;
            if (x == 0 && y == 0 && _directionTapped)
            {
                x = _latchedX;
                y = _latchedY;
            }

            _pressedSinceTick = InputButtons.None;
            _directionTapped = false;
            SamplesSinceTick = 0;
            return new InputFrame(buttons, x, y);
        }

        /// <summary>Clears everything (device lost, pause menu opened).</summary>
        public void Reset()
        {
            _heldNow = InputButtons.None;
            _pressedSinceTick = InputButtons.None;
            _x = _y = _latchedX = _latchedY = 0;
            _directionTapped = false;
            SamplesSinceTick = 0;
        }
    }
}

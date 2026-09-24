using Oathsunder.Combat.Input;

namespace Oathsunder.Gameplay.Combat
{
    /// <summary>
    /// Supplies one <see cref="InputFrame"/> per fighter per simulation tick. Every controller of a fighter —
    /// local player (touch, pad, keyboard), AI, network peer, replay, training dummy — implements this, so the
    /// simulation treats them all identically. AI therefore plays by the same rules and frame data as humans.
    /// </summary>
    public interface ICombatInputSource
    {
        /// <summary>Returns the input for <paramref name="fighterIndex"/> on simulation frame <paramref name="frame"/>.</summary>
        /// <remarks>
        /// Called exactly once per tick, in fighter order, before the tick is simulated. Implementations must latch
        /// presses that happened between ticks so a tap shorter than 16.7 ms is never lost.
        /// </remarks>
        InputFrame Sample(int fighterIndex, int frame);
    }

    /// <summary>Always neutral (idle training dummy, spectator slots).</summary>
    public sealed class NeutralInputSource : ICombatInputSource
    {
        /// <summary>Shared instance.</summary>
        public static readonly NeutralInputSource Instance = new NeutralInputSource();

        /// <inheritdoc />
        public InputFrame Sample(int fighterIndex, int frame) => InputFrame.Neutral;
    }

    /// <summary>Plays back a recorded sequence of inputs (replays, training-mode recordings, PlayMode tests).</summary>
    public sealed class RecordedInputSource : ICombatInputSource
    {
        private readonly InputFrame[] _frames;
        private readonly bool _loop;

        /// <summary>Creates a playback source.</summary>
        public RecordedInputSource(InputFrame[] frames, bool loop)
        {
            _frames = frames ?? new InputFrame[0];
            _loop = loop;
        }

        /// <inheritdoc />
        public InputFrame Sample(int fighterIndex, int frame)
        {
            if (_frames.Length == 0)
            {
                return InputFrame.Neutral;
            }

            int index = frame - 1;
            if (_loop)
            {
                index %= _frames.Length;
            }

            return index >= 0 && index < _frames.Length ? _frames[index] : InputFrame.Neutral;
        }
    }
}

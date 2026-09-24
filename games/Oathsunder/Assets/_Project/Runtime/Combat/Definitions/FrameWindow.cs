using System;

namespace Oathsunder.Combat.Definitions
{
    /// <summary>
    /// Inclusive range of move frames. Frames are 1-based: frame 1 is the first frame of a move, and "startup N"
    /// means the first active hitbox is on frame N.
    /// </summary>
    [Serializable]
    public readonly struct FrameWindow : IEquatable<FrameWindow>
    {
        /// <summary>First frame (inclusive).</summary>
        public readonly int Start;

        /// <summary>Last frame (inclusive).</summary>
        public readonly int End;

        /// <summary>Creates a window.</summary>
        public FrameWindow(int start, int end)
        {
            Start = start;
            End = end;
        }

        /// <summary>Number of frames covered.</summary>
        public int Length => End - Start + 1;

        /// <summary>True when <paramref name="frame"/> is inside the window.</summary>
        public bool Contains(int frame) => frame >= Start && frame <= End;

        /// <summary>True when the window is well-formed within a move of <paramref name="totalFrames"/> frames.</summary>
        public bool IsValidWithin(int totalFrames) => Start >= 1 && End >= Start && End <= totalFrames;

        /// <inheritdoc />
        public bool Equals(FrameWindow other) => Start == other.Start && End == other.End;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is FrameWindow other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => (Start * 397) ^ End;

        /// <inheritdoc />
        public override string ToString() => Start == End ? $"{Start}" : $"{Start}-{End}";
    }
}

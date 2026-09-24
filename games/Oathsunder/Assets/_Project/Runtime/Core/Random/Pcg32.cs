using System;

namespace Oathsunder.Core.Random
{
    /// <summary>
    /// PCG-XSH-RR 32-bit deterministic random generator (O'Neill, 2014). Pure integer arithmetic, so a given
    /// seed produces the same sequence on every platform. Value type: copying it snapshots it, which is what
    /// rollback needs.
    /// </summary>
    [Serializable]
    public struct Pcg32
    {
        private const ulong Multiplier = 6364136223846793005UL;

        /// <summary>Internal state.</summary>
        public ulong State;

        /// <summary>Stream selector (always odd).</summary>
        public ulong Increment;

        /// <summary>Creates a generator for a seed and stream.</summary>
        public Pcg32(ulong seed, ulong stream = 0xDA3E39CB94B95BDBUL)
        {
            State = 0;
            Increment = (stream << 1) | 1UL;
            NextUInt();
            State += seed;
            NextUInt();
        }

        /// <summary>Next uniformly distributed 32-bit value.</summary>
        public uint NextUInt()
        {
            ulong old = State;
            State = unchecked(old * Multiplier + Increment);
            uint xorShifted = (uint)(((old >> 18) ^ old) >> 27);
            int rotation = (int)(old >> 59);
            return (xorShifted >> rotation) | (xorShifted << ((-rotation) & 31));
        }

        /// <summary>Unbiased integer in [0, <paramref name="maxExclusive"/>).</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxExclusive"/> is not positive.</exception>
        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), "Upper bound must be positive.");
            }

            uint bound = (uint)maxExclusive;
            // (2^32 - bound) % bound, computed in 64 bits: unary minus on uint would promote to long in C#.
            uint threshold = (uint)((0x1_0000_0000UL - bound) % bound);
            while (true)
            {
                uint r = NextUInt();
                if (r >= threshold)
                {
                    return (int)(r % bound);
                }
            }
        }

        /// <summary>Unbiased integer in [<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>).</summary>
        public int NextInt(int minInclusive, int maxExclusive) => minInclusive + NextInt(maxExclusive - minInclusive);

        /// <summary>True with probability <paramref name="permille"/>/1000.</summary>
        public bool ChancePermille(int permille) => permille >= 1000 || (permille > 0 && NextInt(1000) < permille);
    }
}

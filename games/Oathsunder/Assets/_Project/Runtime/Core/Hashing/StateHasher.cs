using Oathsunder.Core.Mathematics;

namespace Oathsunder.Core.Hashing
{
    /// <summary>
    /// Incremental 64-bit hash used to checksum simulation state for desync detection, replay validation and
    /// rollback sync-tests. Not cryptographic. Order-sensitive by design.
    /// </summary>
    public struct StateHasher
    {
        private const ulong Offset = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;

        private ulong _hash;
        private bool _started;

        /// <summary>Creates a hasher already primed with the FNV offset basis.</summary>
        public static StateHasher Create()
        {
            var hasher = new StateHasher();
            hasher._hash = Offset;
            hasher._started = true;
            return hasher;
        }

        /// <summary>Mixes a 64-bit value.</summary>
        public void Add(ulong value)
        {
            if (!_started)
            {
                _hash = Offset;
                _started = true;
            }

            unchecked
            {
                _hash = (_hash ^ (value & 0xFFFFFFFFUL)) * Prime;
                _hash = (_hash ^ (value >> 32)) * Prime;
            }
        }

        /// <summary>Mixes a 64-bit signed value.</summary>
        public void Add(long value) => Add(unchecked((ulong)value));

        /// <summary>Mixes a 32-bit value.</summary>
        public void Add(int value) => Add(unchecked((ulong)(uint)value));

        /// <summary>Mixes a 32-bit unsigned value.</summary>
        public void Add(uint value) => Add((ulong)value);

        /// <summary>Mixes a boolean.</summary>
        public void Add(bool value) => Add(value ? 1UL : 0UL);

        /// <summary>Mixes a fixed-point value.</summary>
        public void Add(Fixed value) => Add(value.Raw);

        /// <summary>Mixes a fixed-point vector.</summary>
        public void Add(FixedVector2 value)
        {
            Add(value.X.Raw);
            Add(value.Y.Raw);
        }

        /// <summary>Final avalanche-mixed hash value.</summary>
        public ulong Finish()
        {
            ulong h = _started ? _hash : Offset;
            unchecked
            {
                h ^= h >> 33;
                h *= 0xFF51AFD7ED558CCDUL;
                h ^= h >> 33;
                h *= 0xC4CEB9FE1A85EC53UL;
                h ^= h >> 33;
            }

            return h;
        }
    }
}

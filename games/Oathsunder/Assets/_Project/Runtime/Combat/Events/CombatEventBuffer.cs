using System;
using Oathsunder.Core.Mathematics;

namespace Oathsunder.Combat.Events
{
    /// <summary>
    /// Fixed-capacity list of events produced by one simulation step. Cleared at the start of every step.
    /// Not part of the rollback state: events are outputs, and re-simulated frames regenerate them.
    /// </summary>
    public sealed class CombatEventBuffer
    {
        /// <summary>Capacity per step.</summary>
        public const int Capacity = 128;

        private readonly CombatEvent[] _events = new CombatEvent[Capacity];

        /// <summary>Events recorded this step.</summary>
        public int Count { get; private set; }

        /// <summary>Events dropped because the buffer was full (should always be 0; asserted by tests).</summary>
        public int Dropped { get; private set; }

        /// <summary>Event at an index.</summary>
        public CombatEvent this[int index]
        {
            get
            {
                if ((uint)index >= (uint)Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return _events[index];
            }
        }

        /// <summary>Clears the buffer.</summary>
        public void Clear()
        {
            Count = 0;
            Dropped = 0;
        }

        /// <summary>Appends an event.</summary>
        public void Add(int frame, CombatEventType type, int actor, int target, int instance, int value, int value2, FixedVector2 position, CombatEventFlags flags)
        {
            if (Count >= Capacity)
            {
                Dropped++;
                return;
            }

            _events[Count++] = new CombatEvent(frame, type, actor, target, instance, value, value2, position, flags);
        }

        /// <summary>Appends an event with no values.</summary>
        public void Add(int frame, CombatEventType type, int actor, int target, FixedVector2 position)
        {
            Add(frame, type, actor, target, 0, 0, 0, position, CombatEventFlags.None);
        }

        /// <summary>Counts events of a type (tests and diagnostics).</summary>
        public int CountOf(CombatEventType type)
        {
            int count = 0;
            for (int i = 0; i < Count; i++)
            {
                if (_events[i].Type == type)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>Finds the first event of a type, if any.</summary>
        public bool TryFind(CombatEventType type, out CombatEvent found)
        {
            for (int i = 0; i < Count; i++)
            {
                if (_events[i].Type == type)
                {
                    found = _events[i];
                    return true;
                }
            }

            found = default;
            return false;
        }
    }
}

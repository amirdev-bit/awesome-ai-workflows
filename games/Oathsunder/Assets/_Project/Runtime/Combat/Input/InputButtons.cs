using System;

namespace Oathsunder.Combat.Input
{
    /// <summary>
    /// Logical combat buttons (canon §7.1). Physical devices (touch, gamepad, keyboard) are mapped onto these by
    /// the presentation layer; the simulation never sees device-specific input. Eleven bits: the whole set fits
    /// in the 16-bit packed network input together with the direction.
    /// </summary>
    [Flags]
    public enum InputButtons : ushort
    {
        /// <summary>No button.</summary>
        None = 0,

        /// <summary>Light attack.</summary>
        Light = 1 << 0,

        /// <summary>Heavy attack (hold to charge where the weapon allows).</summary>
        Heavy = 1 << 1,

        /// <summary>Special skill.</summary>
        Special = 1 << 2,

        /// <summary>Guard. A fresh press opens the Perfect Parry window.</summary>
        Guard = 1 << 3,

        /// <summary>Dodge / evasive movement.</summary>
        Dodge = 1 << 4,

        /// <summary>Jump.</summary>
        Jump = 1 << 5,

        /// <summary>Grab / throw; also throw tech.</summary>
        Grab = 1 << 6,

        /// <summary>Execution (contextual).</summary>
        Execute = 1 << 7,

        /// <summary>Ultimate ability.</summary>
        Ultimate = 1 << 8,

        /// <summary>Ember Rage activation.</summary>
        Rage = 1 << 9,

        /// <summary>Umbral Shadow activation.</summary>
        Shadow = 1 << 10,

        /// <summary>Every logical button.</summary>
        All = 0x07FF,
    }

    /// <summary>Helpers for <see cref="InputButtons"/>.</summary>
    public static class InputButtonsExtensions
    {
        /// <summary>Number of logical buttons.</summary>
        public const int Count = 11;

        /// <summary>True when every button in <paramref name="required"/> is set.</summary>
        public static bool HasAll(this InputButtons value, InputButtons required) => (value & required) == required;

        /// <summary>True when any button in <paramref name="mask"/> is set.</summary>
        public static bool HasAny(this InputButtons value, InputButtons mask) => (value & mask) != 0;
    }
}

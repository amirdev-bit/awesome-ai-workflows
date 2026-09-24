using System;

namespace Oathsunder.Combat.Input
{
    /// <summary>
    /// One simulation tick of input for one fighter: held buttons plus an <b>absolute</b> stick direction
    /// (X: -1 left / +1 right, Y: -1 down / +1 up). Directions are converted to facing-relative numpad notation
    /// by the simulation, never by the device layer, so side switches cannot corrupt buffered commands.
    /// </summary>
    /// <remarks>Packs into 15 bits (<see cref="Pack"/>) — the unit sent over the wire by rollback netcode and stored in replays.</remarks>
    [Serializable]
    public readonly struct InputFrame : IEquatable<InputFrame>
    {
        private const int ButtonBits = 11;

        /// <summary>Held buttons.</summary>
        public readonly InputButtons Buttons;

        /// <summary>Horizontal axis: -1, 0 or 1 (absolute, not facing-relative).</summary>
        public readonly sbyte X;

        /// <summary>Vertical axis: -1, 0 or 1.</summary>
        public readonly sbyte Y;

        /// <summary>Creates an input frame. Axes are clamped to -1..1.</summary>
        public InputFrame(InputButtons buttons, int x, int y)
        {
            Buttons = buttons & InputButtons.All;
            X = (sbyte)Math.Sign(x);
            Y = (sbyte)Math.Sign(y);
        }

        /// <summary>No buttons, stick centred.</summary>
        public static readonly InputFrame Neutral = new InputFrame(InputButtons.None, 0, 0);

        /// <summary>True when all buttons in <paramref name="buttons"/> are held.</summary>
        public bool IsHeld(InputButtons buttons) => (Buttons & buttons) == buttons && buttons != InputButtons.None;

        /// <summary>Facing-relative numpad direction.</summary>
        public NumpadDirection Direction(int facingSign) => DirectionUtility.ToNumpad(X, Y, facingSign);

        /// <summary>Returns the horizontally mirrored input (used by mirror-symmetry tests and side-swapped replays).</summary>
        public InputFrame Mirrored() => new InputFrame(Buttons, -X, Y);

        /// <summary>Packs into 15 bits: buttons (11) | x+1 (2) | y+1 (2).</summary>
        public ushort Pack() => (ushort)((ushort)Buttons | ((X + 1) << ButtonBits) | ((Y + 1) << (ButtonBits + 2)));

        /// <summary>Inverse of <see cref="Pack"/>.</summary>
        public static InputFrame Unpack(ushort packed) => new InputFrame(
            (InputButtons)(packed & (ushort)InputButtons.All),
            ((packed >> ButtonBits) & 0x3) - 1,
            ((packed >> (ButtonBits + 2)) & 0x3) - 1);

        /// <inheritdoc />
        public bool Equals(InputFrame other) => Buttons == other.Buttons && X == other.X && Y == other.Y;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is InputFrame other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Pack();

        /// <summary>Equality.</summary>
        public static bool operator ==(InputFrame a, InputFrame b) => a.Equals(b);

        /// <summary>Inequality.</summary>
        public static bool operator !=(InputFrame a, InputFrame b) => !a.Equals(b);

        /// <inheritdoc />
        public override string ToString() => $"[{Direction(1)} {Buttons}]";
    }
}

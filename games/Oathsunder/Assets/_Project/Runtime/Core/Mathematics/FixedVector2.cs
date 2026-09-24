using System;

namespace Oathsunder.Core.Mathematics
{
    /// <summary>
    /// Deterministic 2D vector. The combat plane of a 2.5D game is two-dimensional: X runs along the stage,
    /// Y is height above the ground. Depth (Z) is presentation-only.
    /// </summary>
    [Serializable]
    public readonly struct FixedVector2 : IEquatable<FixedVector2>
    {
        /// <summary>Horizontal component (metres, positive = stage right).</summary>
        public readonly Fixed X;

        /// <summary>Vertical component (metres, positive = up).</summary>
        public readonly Fixed Y;

        /// <summary>Creates a vector.</summary>
        public FixedVector2(Fixed x, Fixed y)
        {
            X = x;
            Y = y;
        }

        /// <summary>(0, 0).</summary>
        public static readonly FixedVector2 Zero = new FixedVector2(Fixed.Zero, Fixed.Zero);

        /// <summary>Returns a copy with a new X.</summary>
        public FixedVector2 WithX(Fixed x) => new FixedVector2(x, Y);

        /// <summary>Returns a copy with a new Y.</summary>
        public FixedVector2 WithY(Fixed y) => new FixedVector2(X, y);

        /// <summary>Mirrors X by a facing sign (+1 keeps, -1 mirrors). Exact.</summary>
        public FixedVector2 Facing(int facingSign) => facingSign >= 0 ? this : new FixedVector2(-X, Y);

        /// <summary>Squared length.</summary>
        public Fixed SqrMagnitude => X * X + Y * Y;

        /// <summary>Length.</summary>
        public Fixed Magnitude => Fixed.Sqrt(SqrMagnitude);

        /// <summary>Addition.</summary>
        public static FixedVector2 operator +(FixedVector2 a, FixedVector2 b) => new FixedVector2(a.X + b.X, a.Y + b.Y);

        /// <summary>Subtraction.</summary>
        public static FixedVector2 operator -(FixedVector2 a, FixedVector2 b) => new FixedVector2(a.X - b.X, a.Y - b.Y);

        /// <summary>Negation.</summary>
        public static FixedVector2 operator -(FixedVector2 a) => new FixedVector2(-a.X, -a.Y);

        /// <summary>Scale.</summary>
        public static FixedVector2 operator *(FixedVector2 a, Fixed s) => new FixedVector2(a.X * s, a.Y * s);

        /// <summary>Integer scale.</summary>
        public static FixedVector2 operator *(FixedVector2 a, int s) => new FixedVector2(a.X * s, a.Y * s);

        /// <summary>Equality.</summary>
        public static bool operator ==(FixedVector2 a, FixedVector2 b) => a.X == b.X && a.Y == b.Y;

        /// <summary>Inequality.</summary>
        public static bool operator !=(FixedVector2 a, FixedVector2 b) => !(a == b);

        /// <inheritdoc />
        public bool Equals(FixedVector2 other) => this == other;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is FixedVector2 other && this == other;

        /// <inheritdoc />
        public override int GetHashCode() => (X.GetHashCode() * 397) ^ Y.GetHashCode();

        /// <inheritdoc />
        public override string ToString() => $"({X}, {Y})";
    }
}

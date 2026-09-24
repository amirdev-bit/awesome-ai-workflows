using System;

namespace Oathsunder.Core.Mathematics
{
    /// <summary>
    /// Deterministic axis-aligned box used for hitboxes, hurtboxes and pushboxes.
    /// </summary>
    /// <remarks>
    /// Boxes authored in data are <b>facing-relative</b>: +X points in front of the fighter. Use
    /// <see cref="ToWorld"/> to place a local box in the world for a given origin and facing.
    /// Touching edges do not count as overlap.
    /// </remarks>
    [Serializable]
    public readonly struct FixedAabb : IEquatable<FixedAabb>
    {
        /// <summary>Minimum corner.</summary>
        public readonly FixedVector2 Min;

        /// <summary>Maximum corner.</summary>
        public readonly FixedVector2 Max;

        /// <summary>Creates a box from two corners. The corners are normalised.</summary>
        public FixedAabb(FixedVector2 min, FixedVector2 max)
        {
            Min = new FixedVector2(Fixed.Min(min.X, max.X), Fixed.Min(min.Y, max.Y));
            Max = new FixedVector2(Fixed.Max(min.X, max.X), Fixed.Max(min.Y, max.Y));
        }

        /// <summary>Creates a box from its minimum corner and size.</summary>
        public static FixedAabb FromMinSize(Fixed x, Fixed y, Fixed width, Fixed height) =>
            new FixedAabb(new FixedVector2(x, y), new FixedVector2(x + width, y + height));

        /// <summary>Width of the box.</summary>
        public Fixed Width => Max.X - Min.X;

        /// <summary>Height of the box.</summary>
        public Fixed Height => Max.Y - Min.Y;

        /// <summary>Centre point (rounded toward the minimum corner by at most one raw unit).</summary>
        public FixedVector2 Center => new FixedVector2(
            Fixed.FromRaw((Min.X.Raw + Max.X.Raw) >> 1),
            Fixed.FromRaw((Min.Y.Raw + Max.Y.Raw) >> 1));

        /// <summary>True when the box has positive area.</summary>
        public bool IsValid => Max.X > Min.X && Max.Y > Min.Y;

        /// <summary>Places a facing-relative box into world space.</summary>
        public FixedAabb ToWorld(FixedVector2 origin, int facingSign)
        {
            if (facingSign >= 0)
            {
                return new FixedAabb(origin + Min, origin + Max);
            }

            return new FixedAabb(
                new FixedVector2(origin.X - Max.X, origin.Y + Min.Y),
                new FixedVector2(origin.X - Min.X, origin.Y + Max.Y));
        }

        /// <summary>True when the interiors of the boxes intersect.</summary>
        public bool Overlaps(in FixedAabb other) =>
            Min.X < other.Max.X && other.Min.X < Max.X &&
            Min.Y < other.Max.Y && other.Min.Y < Max.Y;

        /// <summary>True when the X ranges intersect.</summary>
        public bool OverlapsX(in FixedAabb other) => Min.X < other.Max.X && other.Min.X < Max.X;

        /// <summary>True when the Y ranges intersect.</summary>
        public bool OverlapsY(in FixedAabb other) => Min.Y < other.Max.Y && other.Min.Y < Max.Y;

        /// <summary>Intersection box. Only meaningful when <see cref="Overlaps"/> is true.</summary>
        public FixedAabb Intersection(in FixedAabb other) => new FixedAabb(
            new FixedVector2(Fixed.Max(Min.X, other.Min.X), Fixed.Max(Min.Y, other.Min.Y)),
            new FixedVector2(Fixed.Min(Max.X, other.Max.X), Fixed.Min(Max.Y, other.Max.Y)));

        /// <inheritdoc />
        public bool Equals(FixedAabb other) => Min == other.Min && Max == other.Max;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is FixedAabb other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => (Min.GetHashCode() * 397) ^ Max.GetHashCode();

        /// <inheritdoc />
        public override string ToString() => $"[{Min} .. {Max}]";
    }
}

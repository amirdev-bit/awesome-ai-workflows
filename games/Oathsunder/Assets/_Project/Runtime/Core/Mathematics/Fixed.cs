using System;
using System.Globalization;

namespace Oathsunder.Core.Mathematics
{
    /// <summary>
    /// Signed Q47.16 fixed-point number used by every deterministic simulation in the project.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Floating point is not bit-identical across CPUs, compilers and IL2CPP builds, which makes it unusable
    /// for rollback netcode, replays and lockstep verification. All gameplay simulation therefore runs on
    /// <see cref="Fixed"/>. Presentation converts to <c>float</c> only at the very edge via <see cref="ToFloat"/>.
    /// </para>
    /// <para>
    /// Multiplication and division are <b>odd-symmetric</b>: <c>(-a) * b == -(a * b)</c> exactly. This guarantees that a
    /// fighter on the left side of the stage behaves as the exact mirror image of the same fighter on the right,
    /// which is a hard requirement for competitive fairness.
    /// </para>
    /// <para>Safe range for products: operands with magnitude below roughly 46,000 units.</para>
    /// </remarks>
    [Serializable]
    public readonly struct Fixed : IEquatable<Fixed>, IComparable<Fixed>
    {
        /// <summary>Number of fractional bits.</summary>
        public const int FractionalBits = 16;

        /// <summary>Raw representation of 1.0.</summary>
        public const long OneRaw = 1L << FractionalBits;

        private const long HalfRaw = OneRaw >> 1;

        /// <summary>The raw scaled integer value.</summary>
        public readonly long Raw;

        private Fixed(long raw)
        {
            Raw = raw;
        }

        /// <summary>0.</summary>
        public static readonly Fixed Zero = new Fixed(0);

        /// <summary>1.</summary>
        public static readonly Fixed One = new Fixed(OneRaw);

        /// <summary>-1.</summary>
        public static readonly Fixed MinusOne = new Fixed(-OneRaw);

        /// <summary>0.5.</summary>
        public static readonly Fixed Half = new Fixed(HalfRaw);

        /// <summary>Smallest positive representable value (1/65536).</summary>
        public static readonly Fixed Epsilon = new Fixed(1);

        /// <summary>Largest representable value.</summary>
        public static readonly Fixed MaxValue = new Fixed(long.MaxValue);

        /// <summary>Smallest representable value.</summary>
        public static readonly Fixed MinValue = new Fixed(long.MinValue);

        /// <summary>Creates a value from its raw representation.</summary>
        public static Fixed FromRaw(long raw) => new Fixed(raw);

        /// <summary>Creates a value from an integer.</summary>
        public static Fixed FromInt(int value) => new Fixed((long)value << FractionalBits);

        /// <summary>Creates <c>numerator / denominator</c>, rounded to the nearest raw unit (ties away from zero).</summary>
        public static Fixed FromRatio(long numerator, long denominator)
        {
            if (denominator == 0)
            {
                throw new DivideByZeroException("Fixed.FromRatio denominator is zero.");
            }

            return new Fixed(DivideRounded(numerator * OneRaw, denominator));
        }

        /// <summary>Creates a value from thousandths, e.g. <c>FromMilli(1250) == 1.25</c>.</summary>
        public static Fixed FromMilli(int milli) => FromRatio(milli, 1000);

        /// <summary>
        /// Parses an invariant-culture decimal string (optionally with exponent) exactly, rounding to the nearest
        /// raw unit. Parsing never touches floating point, so data files produce bit-identical values everywhere.
        /// </summary>
        /// <exception cref="FormatException">The text is not a valid number.</exception>
        /// <exception cref="OverflowException">The value is outside the representable range.</exception>
        public static Fixed Parse(string text) => ParseScaled(text, 1);

        /// <summary>
        /// Parses a decimal string and divides it by <paramref name="divisor"/> before rounding. Used to convert
        /// designer-facing units (metres per second) into simulation units (metres per frame) without any
        /// intermediate rounding error.
        /// </summary>
        public static Fixed ParseScaled(string text, int divisor)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (divisor <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(divisor), "Divisor must be positive.");
            }

            decimal value = decimal.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
            decimal scaled = value * OneRaw / divisor;
            decimal rounded = decimal.Round(scaled, 0, MidpointRounding.AwayFromZero);
            if (rounded > long.MaxValue || rounded < long.MinValue)
            {
                throw new OverflowException($"'{text}' is outside the Fixed range.");
            }

            return new Fixed((long)rounded);
        }

        /// <summary>Tries to parse a decimal string. See <see cref="Parse"/>.</summary>
        public static bool TryParse(string text, out Fixed value)
        {
            try
            {
                value = Parse(text);
                return true;
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
            }
            catch (ArgumentNullException)
            {
            }

            value = Zero;
            return false;
        }

        /// <summary>
        /// Converts from a float. <b>Only</b> for editor tooling and presentation; never feed the result of a runtime
        /// float computation back into the simulation.
        /// </summary>
        public static Fixed FromFloatUnsafe(float value) => new Fixed((long)Math.Round(value * (double)OneRaw, MidpointRounding.AwayFromZero));

        /// <summary>Converts to float for presentation.</summary>
        public float ToFloat() => (float)(Raw / (double)OneRaw);

        /// <summary>Converts to double for tooling and reports.</summary>
        public double ToDouble() => Raw / (double)OneRaw;

        /// <summary>Largest integer less than or equal to this value.</summary>
        public int FloorToInt() => (int)(Raw >> FractionalBits);

        /// <summary>Smallest integer greater than or equal to this value.</summary>
        public int CeilToInt() => (int)((Raw + OneRaw - 1) >> FractionalBits);

        /// <summary>Nearest integer, ties away from zero.</summary>
        public int RoundToInt() => (int)(Raw >= 0 ? (Raw + HalfRaw) >> FractionalBits : -((-Raw + HalfRaw) >> FractionalBits));

        /// <summary>True when the value is exactly zero.</summary>
        public bool IsZero => Raw == 0;

        /// <summary>-1, 0 or 1.</summary>
        public int Sign => Raw > 0 ? 1 : (Raw < 0 ? -1 : 0);

        /// <summary>Addition.</summary>
        public static Fixed operator +(Fixed a, Fixed b) => new Fixed(a.Raw + b.Raw);

        /// <summary>Subtraction.</summary>
        public static Fixed operator -(Fixed a, Fixed b) => new Fixed(a.Raw - b.Raw);

        /// <summary>Negation.</summary>
        public static Fixed operator -(Fixed a) => new Fixed(-a.Raw);

        /// <summary>Odd-symmetric multiplication rounded to nearest (ties away from zero).</summary>
        public static Fixed operator *(Fixed a, Fixed b)
        {
            long product = a.Raw * b.Raw;
            return new Fixed(product >= 0 ? (product + HalfRaw) >> FractionalBits : -((-product + HalfRaw) >> FractionalBits));
        }

        /// <summary>Multiplication by an integer (exact).</summary>
        public static Fixed operator *(Fixed a, int b) => new Fixed(a.Raw * b);

        /// <summary>Multiplication by an integer (exact).</summary>
        public static Fixed operator *(int a, Fixed b) => new Fixed(a * b.Raw);

        /// <summary>Odd-symmetric division rounded to nearest (ties away from zero).</summary>
        /// <exception cref="DivideByZeroException"><paramref name="b"/> is zero.</exception>
        public static Fixed operator /(Fixed a, Fixed b)
        {
            if (b.Raw == 0)
            {
                throw new DivideByZeroException("Fixed division by zero.");
            }

            return new Fixed(DivideRounded(a.Raw << FractionalBits, b.Raw));
        }

        /// <summary>Division by an integer, rounded to nearest (ties away from zero).</summary>
        public static Fixed operator /(Fixed a, int b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException("Fixed division by zero.");
            }

            return new Fixed(DivideRounded(a.Raw, b));
        }

        /// <summary>Equality.</summary>
        public static bool operator ==(Fixed a, Fixed b) => a.Raw == b.Raw;

        /// <summary>Inequality.</summary>
        public static bool operator !=(Fixed a, Fixed b) => a.Raw != b.Raw;

        /// <summary>Less than.</summary>
        public static bool operator <(Fixed a, Fixed b) => a.Raw < b.Raw;

        /// <summary>Greater than.</summary>
        public static bool operator >(Fixed a, Fixed b) => a.Raw > b.Raw;

        /// <summary>Less than or equal.</summary>
        public static bool operator <=(Fixed a, Fixed b) => a.Raw <= b.Raw;

        /// <summary>Greater than or equal.</summary>
        public static bool operator >=(Fixed a, Fixed b) => a.Raw >= b.Raw;

        /// <summary>Scales by <paramref name="permille"/>/1000, truncating toward zero (odd-symmetric).</summary>
        public Fixed MulPermille(int permille) => new Fixed(Raw * permille / 1000);

        /// <summary>Absolute value.</summary>
        public static Fixed Abs(Fixed value) => value.Raw < 0 ? new Fixed(-value.Raw) : value;

        /// <summary>Minimum of two values.</summary>
        public static Fixed Min(Fixed a, Fixed b) => a.Raw <= b.Raw ? a : b;

        /// <summary>Maximum of two values.</summary>
        public static Fixed Max(Fixed a, Fixed b) => a.Raw >= b.Raw ? a : b;

        /// <summary>Clamps a value into [<paramref name="min"/>, <paramref name="max"/>].</summary>
        public static Fixed Clamp(Fixed value, Fixed min, Fixed max)
        {
            if (value.Raw < min.Raw)
            {
                return min;
            }

            return value.Raw > max.Raw ? max : value;
        }

        /// <summary>Linear interpolation, <paramref name="t"/> is not clamped.</summary>
        public static Fixed Lerp(Fixed a, Fixed b, Fixed t) => a + (b - a) * t;

        /// <summary>Moves <paramref name="current"/> toward <paramref name="target"/> by at most <paramref name="maxDelta"/>.</summary>
        public static Fixed MoveTowards(Fixed current, Fixed target, Fixed maxDelta)
        {
            long delta = target.Raw - current.Raw;
            if (delta > maxDelta.Raw)
            {
                return new Fixed(current.Raw + maxDelta.Raw);
            }

            if (delta < -maxDelta.Raw)
            {
                return new Fixed(current.Raw - maxDelta.Raw);
            }

            return target;
        }

        /// <summary>Square root of a non-negative value (bitwise integer square root, fully deterministic).</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
        public static Fixed Sqrt(Fixed value)
        {
            if (value.Raw < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Cannot take the square root of a negative number.");
            }

            ulong radicand = (ulong)value.Raw << FractionalBits;
            ulong result = 0;
            ulong bit = 1UL << 62;
            while (bit > radicand)
            {
                bit >>= 2;
            }

            while (bit != 0)
            {
                if (radicand >= result + bit)
                {
                    radicand -= result + bit;
                    result = (result >> 1) + bit;
                }
                else
                {
                    result >>= 1;
                }

                bit >>= 2;
            }

            return new Fixed((long)result);
        }

        /// <inheritdoc />
        public bool Equals(Fixed other) => Raw == other.Raw;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is Fixed other && other.Raw == Raw;

        /// <inheritdoc />
        public override int GetHashCode() => Raw.GetHashCode();

        /// <inheritdoc />
        public int CompareTo(Fixed other) => Raw.CompareTo(other.Raw);

        /// <summary>Invariant decimal representation with up to five fractional digits.</summary>
        public override string ToString() => ToDouble().ToString("0.#####", CultureInfo.InvariantCulture);

        private static long DivideRounded(long numerator, long denominator)
        {
            bool negative = (numerator < 0) != (denominator < 0);
            ulong n = numerator < 0 ? (ulong)(-numerator) : (ulong)numerator;
            ulong d = denominator < 0 ? (ulong)(-denominator) : (ulong)denominator;
            ulong q = (n + (d >> 1)) / d;
            return negative ? -(long)q : (long)q;
        }
    }
}

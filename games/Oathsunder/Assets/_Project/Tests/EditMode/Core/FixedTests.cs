using System;
using Oathsunder.Core.Mathematics;
using NUnit.Framework;

namespace Oathsunder.Tests.Core
{
    [TestFixture]
    public sealed class FixedTests
    {
        [Test]
        public void IntegerRoundTrip()
        {
            for (int i = -1000; i <= 1000; i += 7)
            {
                Assert.AreEqual(i, Fixed.FromInt(i).FloorToInt());
                Assert.AreEqual(i, Fixed.FromInt(i).RoundToInt());
            }
        }

        [TestCase("0", 0L)]
        [TestCase("1", 65536L)]
        [TestCase("-1", -65536L)]
        [TestCase("0.5", 32768L)]
        [TestCase("1.25", 81920L)]
        [TestCase("-2.75", -180224L)]
        [TestCase("1e-1", 6554L)]
        [TestCase("3.2E0", 209715L)]
        public void ParseIsExactAndRoundsToNearest(string text, long raw)
        {
            Assert.AreEqual(raw, Fixed.Parse(text).Raw);
        }

        [Test]
        public void ParseScaledConvertsMetresPerSecondToMetresPerFrameWithoutIntermediateRounding()
        {
            // 3.2 m/s at 60 Hz = 0.053333 m/frame = 3495.25 raw -> 3495.
            Assert.AreEqual(3495L, Fixed.ParseScaled("3.2", 60).Raw);

            // 37.8 m/s² at 60 Hz = 0.0105 m/frame² = 688.128 raw -> 688.
            Assert.AreEqual(688L, Fixed.ParseScaled("37.8", 3600).Raw);
        }

        [Test]
        public void ParseRejectsGarbage()
        {
            Assert.Throws<FormatException>(() => Fixed.Parse("abc"));
            Assert.IsFalse(Fixed.TryParse("1.2.3", out _));
            Assert.IsTrue(Fixed.TryParse("-0.001", out var small));
            Assert.AreEqual(-66L, small.Raw);
        }

        [Test]
        public void MultiplicationAndDivisionAreOddSymmetric()
        {
            // Mirror symmetry of the whole simulation depends on (-a) * b == -(a * b) exactly.
            var random = new System.Random(1234);
            for (int i = 0; i < 20000; i++)
            {
                var a = Fixed.FromRaw(random.Next(-5_000_000, 5_000_000));
                var b = Fixed.FromRaw(random.Next(-5_000_000, 5_000_000));
                Assert.AreEqual(-(a * b), (-a) * b);
                Assert.AreEqual(-(a * b), a * -b);
                if (b.Raw != 0)
                {
                    Assert.AreEqual(-(a / b), (-a) / b);
                }

                Assert.AreEqual(-a.MulPermille(733), (-a).MulPermille(733));
            }
        }

        [Test]
        public void MultiplicationRoundsToNearest()
        {
            var third = Fixed.FromRatio(1, 3);
            Assert.AreEqual(21845L, third.Raw);
            Assert.AreEqual(Fixed.FromInt(6), Fixed.FromInt(2) * Fixed.FromInt(3));
            Assert.AreEqual(Fixed.Half, Fixed.One * Fixed.Half);
            Assert.AreEqual(Fixed.FromMilli(1500), Fixed.FromInt(3) / 2);
        }

        [Test]
        public void DivideByZeroThrows()
        {
            Assert.Throws<DivideByZeroException>(() => { var _ = Fixed.One / Fixed.Zero; });
            Assert.Throws<DivideByZeroException>(() => Fixed.FromRatio(1, 0));
        }

        [Test]
        public void SqrtIsAccurate()
        {
            Assert.AreEqual(Fixed.FromInt(3), Fixed.Sqrt(Fixed.FromInt(9)));
            Assert.AreEqual(Fixed.Zero, Fixed.Sqrt(Fixed.Zero));
            double two = Fixed.Sqrt(Fixed.FromInt(2)).ToDouble();
            Assert.AreEqual(Math.Sqrt(2), two, 1.0 / 65536 * 2);
            Assert.Throws<ArgumentOutOfRangeException>(() => Fixed.Sqrt(Fixed.MinusOne));
        }

        [Test]
        public void HelpersBehave()
        {
            Assert.AreEqual(Fixed.FromInt(2), Fixed.Abs(Fixed.FromInt(-2)));
            Assert.AreEqual(Fixed.FromInt(1), Fixed.Clamp(Fixed.FromInt(5), Fixed.Zero, Fixed.One));
            Assert.AreEqual(Fixed.FromInt(4), Fixed.MoveTowards(Fixed.FromInt(5), Fixed.Zero, Fixed.One));
            Assert.AreEqual(Fixed.Zero, Fixed.MoveTowards(Fixed.Half, Fixed.Zero, Fixed.One));
            Assert.AreEqual(Fixed.FromMilli(2500), Fixed.Lerp(Fixed.FromInt(2), Fixed.FromInt(3), Fixed.Half));
            Assert.AreEqual(-1, Fixed.FromMilli(-1).Sign);
            Assert.AreEqual("1.25", Fixed.FromMilli(1250).ToString());
        }

        [Test]
        public void AabbFacingFlipMirrorsExactly()
        {
            var local = FixedAabb.FromMinSize(Fixed.FromMilli(200), Fixed.FromMilli(800), Fixed.FromMilli(1200), Fixed.FromMilli(550));
            var origin = new FixedVector2(Fixed.FromInt(3), Fixed.Zero);
            var right = local.ToWorld(origin, 1);
            var left = local.ToWorld(new FixedVector2(-origin.X, Fixed.Zero), -1);
            Assert.AreEqual(-right.Max.X, left.Min.X);
            Assert.AreEqual(-right.Min.X, left.Max.X);
            Assert.AreEqual(right.Min.Y, left.Min.Y);
            Assert.AreEqual(right.Max.Y, left.Max.Y);
        }

        [Test]
        public void AabbOverlapExcludesTouchingEdges()
        {
            var a = FixedAabb.FromMinSize(Fixed.Zero, Fixed.Zero, Fixed.One, Fixed.One);
            var touching = FixedAabb.FromMinSize(Fixed.One, Fixed.Zero, Fixed.One, Fixed.One);
            var overlapping = FixedAabb.FromMinSize(Fixed.Half, Fixed.Half, Fixed.One, Fixed.One);
            Assert.IsFalse(a.Overlaps(touching));
            Assert.IsTrue(a.Overlaps(overlapping));
            var intersection = a.Intersection(overlapping);
            Assert.AreEqual(Fixed.Half, intersection.Width);
        }
    }
}

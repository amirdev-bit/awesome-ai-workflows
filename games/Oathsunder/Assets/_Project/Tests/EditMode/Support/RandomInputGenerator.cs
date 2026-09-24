using Oathsunder.Combat.Input;
using Oathsunder.Core.Random;

namespace Oathsunder.Tests.Support
{
    /// <summary>
    /// Produces plausible, reproducible player input: directions held for a while, buttons tapped or held,
    /// occasional motion inputs. Used by fuzz, determinism, rollback and performance tests.
    /// </summary>
    public sealed class RandomInputGenerator
    {
        private static readonly InputButtons[] Buttons =
        {
            InputButtons.Light, InputButtons.Heavy, InputButtons.Special, InputButtons.Guard, InputButtons.Dodge,
            InputButtons.Jump, InputButtons.Grab, InputButtons.Execute, InputButtons.Ultimate, InputButtons.Rage, InputButtons.Shadow,
        };

        private Pcg32 _random;
        private int _x;
        private int _y;
        private int _directionFrames;
        private InputButtons _held;
        private int _holdFrames;

        /// <summary>Creates a generator.</summary>
        public RandomInputGenerator(ulong seed)
        {
            _random = new Pcg32(seed);
        }

        /// <summary>Next frame of input (absolute directions).</summary>
        /// <param name="facing">
        /// The fighter's current facing (+1/-1), or 0 when unknown. When known, the generator approaches the opponent
        /// often, which keeps the fighters in range so the fuzz run exercises contact, guard and throw systems.
        /// </param>
        public InputFrame Next(int facing = 0)
        {
            if (_directionFrames <= 0)
            {
                _x = facing != 0 && _random.ChancePermille(450) ? facing : _random.NextInt(3) - 1;
                _y = _random.ChancePermille(250) ? _random.NextInt(3) - 1 : 0;
                _directionFrames = _random.NextInt(1, 20);
            }

            _directionFrames--;
            if (_holdFrames <= 0)
            {
                _held = InputButtons.None;
                if (_random.ChancePermille(220))
                {
                    if (_random.ChancePermille(250))
                    {
                        // Sustained guarding (blocks, parries, guard breaks).
                        _held = InputButtons.Guard;
                        _holdFrames = _random.NextInt(8, 60);
                    }
                    else
                    {
                        _held = Buttons[_random.NextInt(Buttons.Length)];
                        _holdFrames = _random.ChancePermille(200) ? _random.NextInt(5, 50) : 1;
                    }
                }
            }

            _holdFrames--;
            return new InputFrame(_held, _x, _y);
        }
    }
}

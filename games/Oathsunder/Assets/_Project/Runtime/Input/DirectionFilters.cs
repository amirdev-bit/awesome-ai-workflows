using System;

namespace Oathsunder.Controls
{
    /// <summary>
    /// Converts an analog stick into the 8-way digital direction a fighting game needs. Cardinal directions get
    /// wider sectors than diagonals (default 60° vs 30°) so "hold back to walk" never turns into an accidental
    /// crouch, while deliberate diagonals for motion inputs still register.
    /// </summary>
    public readonly struct DigitalStick
    {
        /// <summary>Creates a quantiser.</summary>
        /// <param name="deadzone">Radius below which the stick is neutral (0..1).</param>
        /// <param name="diagonalSectorDegrees">Angular width of each diagonal sector (the rest goes to cardinals).</param>
        public DigitalStick(float deadzone, float diagonalSectorDegrees)
        {
            if (deadzone < 0f || deadzone >= 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(deadzone), "Deadzone must be in [0, 1).");
            }

            if (diagonalSectorDegrees <= 0f || diagonalSectorDegrees >= 90f)
            {
                throw new ArgumentOutOfRangeException(nameof(diagonalSectorDegrees), "Diagonal sector must be in (0, 90).");
            }

            Deadzone = deadzone;
            DiagonalSectorDegrees = diagonalSectorDegrees;
        }

        /// <summary>Shipped default: 0.35 deadzone, 30° diagonals.</summary>
        public static readonly DigitalStick Default = new DigitalStick(0.35f, 30f);

        /// <summary>Radius below which the stick is neutral.</summary>
        public float Deadzone { get; }

        /// <summary>Angular width of each diagonal sector.</summary>
        public float DiagonalSectorDegrees { get; }

        /// <summary>Quantises an analog vector into (x, y) each in {-1, 0, 1}.</summary>
        public void Quantize(float analogX, float analogY, out int x, out int y)
        {
            float magnitudeSquared = analogX * analogX + analogY * analogY;
            if (magnitudeSquared <= Deadzone * Deadzone)
            {
                x = 0;
                y = 0;
                return;
            }

            double angle = Math.Atan2(analogY, analogX) * (180.0 / Math.PI);
            if (angle < 0)
            {
                angle += 360.0;
            }

            // Sector centres: 0 (right), 45, 90 (up) ... Diagonals span ±half their width around 45°·k (k odd).
            double half = DiagonalSectorDegrees * 0.5;
            int octant = -1;
            for (int k = 1; k < 8; k += 2)
            {
                double centre = 45.0 * k;
                if (angle >= centre - half && angle <= centre + half)
                {
                    octant = k;
                    break;
                }
            }

            if (octant < 0)
            {
                // Cardinal: nearest multiple of 90°.
                octant = ((int)Math.Round(angle / 90.0) % 4) * 2;
            }

            switch (octant)
            {
                case 0: x = 1; y = 0; break;
                case 1: x = 1; y = 1; break;
                case 2: x = 0; y = 1; break;
                case 3: x = -1; y = 1; break;
                case 4: x = -1; y = 0; break;
                case 5: x = -1; y = -1; break;
                case 6: x = 0; y = -1; break;
                default: x = 1; y = -1; break;
            }
        }
    }

    /// <summary>How simultaneous opposite directions (keyboard, hitbox-style controllers) are resolved.</summary>
    public enum SocdMode : byte
    {
        /// <summary>Left + right = neutral; up + down = up (tournament standard).</summary>
        NeutralHorizontalUpPriority,

        /// <summary>Left + right = neutral; up + down = neutral.</summary>
        NeutralBoth,

        /// <summary>The most recently pressed of two opposites wins (keyboard players often prefer this).</summary>
        LastInputWins,
    }

    /// <summary>Simultaneous Opposite Cardinal Direction cleaner for digital devices.</summary>
    public sealed class SocdResolver
    {
        private bool _leftWasHeld;
        private bool _rightWasHeld;
        private bool _upWasHeld;
        private bool _downWasHeld;
        private int _lastHorizontal;
        private int _lastVertical;

        /// <summary>Creates a resolver.</summary>
        public SocdResolver(SocdMode mode)
        {
            Mode = mode;
        }

        /// <summary>Resolution mode.</summary>
        public SocdMode Mode { get; set; }

        /// <summary>Resolves four digital direction switches into (x, y).</summary>
        public void Resolve(bool left, bool right, bool up, bool down, out int x, out int y)
        {
            if (left && !_leftWasHeld)
            {
                _lastHorizontal = -1;
            }

            if (right && !_rightWasHeld)
            {
                _lastHorizontal = 1;
            }

            if (up && !_upWasHeld)
            {
                _lastVertical = 1;
            }

            if (down && !_downWasHeld)
            {
                _lastVertical = -1;
            }

            _leftWasHeld = left;
            _rightWasHeld = right;
            _upWasHeld = up;
            _downWasHeld = down;

            if (left && right)
            {
                x = Mode == SocdMode.LastInputWins ? _lastHorizontal : 0;
            }
            else
            {
                x = left ? -1 : (right ? 1 : 0);
            }

            if (up && down)
            {
                switch (Mode)
                {
                    case SocdMode.NeutralHorizontalUpPriority:
                        y = 1;
                        break;
                    case SocdMode.NeutralBoth:
                        y = 0;
                        break;
                    default:
                        y = _lastVertical;
                        break;
                }
            }
            else
            {
                y = down ? -1 : (up ? 1 : 0);
            }
        }
    }
}

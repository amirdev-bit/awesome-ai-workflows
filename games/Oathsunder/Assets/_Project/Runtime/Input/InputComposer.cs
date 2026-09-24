using Oathsunder.Combat.Input;

namespace Oathsunder.Controls
{
    /// <summary>Raw device state for one render frame, already mapped to logical buttons by the platform layer.</summary>
    public struct DeviceSnapshot
    {
        /// <summary>Logical buttons held on any device (keyboard, gamepad, touch).</summary>
        public InputButtons Held;

        /// <summary>Analog stick (gamepad left stick), -1..1.</summary>
        public float StickX;

        /// <summary>Analog stick (gamepad left stick), -1..1.</summary>
        public float StickY;

        /// <summary>Digital directions (keyboard WASD / arrows, gamepad d-pad, arcade stick).</summary>
        public bool Left;

        /// <summary>Digital direction.</summary>
        public bool Right;

        /// <summary>Digital direction.</summary>
        public bool Up;

        /// <summary>Digital direction.</summary>
        public bool Down;

        /// <summary>True when the touch stick is being used (it takes priority over the gamepad stick).</summary>
        public bool TouchStickActive;

        /// <summary>Touch stick vector, -1..1.</summary>
        public float TouchStickX;

        /// <summary>Touch stick vector, -1..1.</summary>
        public float TouchStickY;
    }

    /// <summary>
    /// Turns a <see cref="DeviceSnapshot"/> into logical input according to a <see cref="ControlProfile"/>:
    /// digital directions win over analog (SOCD-cleaned), the touch stick wins over the gamepad stick, analog
    /// is quantised to 8 ways with cardinal bias, and profile conveniences (Up-to-Jump, Heavy+Special = Execute)
    /// are applied. The result feeds an <see cref="InputLatch"/>.
    /// </summary>
    public sealed class InputComposer
    {
        private readonly SocdResolver _socd;
        private DigitalStick _stick;
        private ControlProfile _profile;

        /// <summary>Creates a composer for a profile.</summary>
        public InputComposer(ControlProfile profile)
        {
            _socd = new SocdResolver(profile.Socd);
            Apply(profile);
        }

        /// <summary>Applies (possibly edited) profile settings.</summary>
        public void Apply(ControlProfile profile)
        {
            _profile = profile;
            _socd.Mode = profile.Socd;
            _stick = new DigitalStick(profile.StickDeadzone, profile.DiagonalSectorDegrees);
        }

        /// <summary>Composes the logical buttons and digital direction of one snapshot.</summary>
        public void Compose(in DeviceSnapshot snapshot, out InputButtons buttons, out int x, out int y)
        {
            buttons = snapshot.Held & InputButtons.All;

            if (snapshot.Left || snapshot.Right || snapshot.Up || snapshot.Down)
            {
                _socd.Resolve(snapshot.Left, snapshot.Right, snapshot.Up, snapshot.Down, out x, out y);
            }
            else
            {
                _socd.Resolve(false, false, false, false, out _, out _);
                if (snapshot.TouchStickActive)
                {
                    _stick.Quantize(snapshot.TouchStickX, snapshot.TouchStickY, out x, out y);
                }
                else
                {
                    _stick.Quantize(snapshot.StickX, snapshot.StickY, out x, out y);
                }
            }

            if (_profile.UpToJump && y > 0)
            {
                buttons |= InputButtons.Jump;
            }

            if (_profile.ExecuteChord && (buttons & (InputButtons.Heavy | InputButtons.Special)) == (InputButtons.Heavy | InputButtons.Special))
            {
                buttons |= InputButtons.Execute;
            }
        }
    }
}

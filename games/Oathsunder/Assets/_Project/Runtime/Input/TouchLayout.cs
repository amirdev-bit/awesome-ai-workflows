using System;
using System.Collections.Generic;
using Oathsunder.Combat.Input;

namespace Oathsunder.Controls
{
    /// <summary>A circular touch button in normalised screen space (origin bottom-left, x in 0..aspect, y in 0..1).</summary>
    public readonly struct TouchButtonRegion
    {
        /// <summary>Creates a region. Coordinates are measured in screen heights so layouts keep their shape on any aspect ratio.</summary>
        public TouchButtonRegion(InputButtons button, float centerFromRight, float centerY, float radius, string label)
        {
            Button = button;
            CenterFromRight = centerFromRight;
            CenterY = centerY;
            Radius = radius;
            Label = label;
        }

        /// <summary>Logical button.</summary>
        public InputButtons Button { get; }

        /// <summary>Distance of the centre from the right edge, in screen heights.</summary>
        public float CenterFromRight { get; }

        /// <summary>Height of the centre, in screen heights.</summary>
        public float CenterY { get; }

        /// <summary>Radius, in screen heights.</summary>
        public float Radius { get; }

        /// <summary>Short label drawn on the button.</summary>
        public string Label { get; }
    }

    /// <summary>
    /// Touch control layout: a floating stick zone on the left and a cluster of buttons on the right.
    /// Measured in screen heights so the same layout works from 16:9 phones to 4:3 tablets.
    /// </summary>
    public sealed class TouchLayout
    {
        /// <summary>Phone layout id.</summary>
        public const string PhoneId = "touch.phone";

        /// <summary>Tablet layout id.</summary>
        public const string TabletId = "touch.tablet";

        /// <summary>Creates a layout.</summary>
        public TouchLayout(string id, float stickZoneWidth, float stickZoneHeight, float stickRadius, IReadOnlyList<TouchButtonRegion> buttons)
        {
            Id = id;
            StickZoneWidth = stickZoneWidth;
            StickZoneHeight = stickZoneHeight;
            StickRadius = stickRadius;
            _buttons = new TouchButtonRegion[buttons.Count];
            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttons[i] = buttons[i];
            }
        }

        private readonly TouchButtonRegion[] _buttons;

        /// <summary>Layout id.</summary>
        public string Id { get; }

        /// <summary>Width of the floating-stick zone from the left edge, in screen heights.</summary>
        public float StickZoneWidth { get; }

        /// <summary>Height of the floating-stick zone from the bottom, in screen heights.</summary>
        public float StickZoneHeight { get; }

        /// <summary>Stick throw radius, in screen heights.</summary>
        public float StickRadius { get; }

        /// <summary>Buttons.</summary>
        public IReadOnlyList<TouchButtonRegion> Buttons => _buttons;

        /// <summary>Number of buttons.</summary>
        public int ButtonCount => _buttons.Length;

        /// <summary>Button by index (allocation-free access for per-frame code).</summary>
        public TouchButtonRegion ButtonAt(int index) => _buttons[index];

        /// <summary>Phone layout: thumb arc of six primary buttons plus meter buttons along the top of the cluster.</summary>
        public static TouchLayout Phone() => new TouchLayout(PhoneId, 0.8f, 0.75f, 0.11f, new[]
        {
            new TouchButtonRegion(InputButtons.Light, 0.36f, 0.16f, 0.085f, "L"),
            new TouchButtonRegion(InputButtons.Heavy, 0.20f, 0.26f, 0.085f, "H"),
            new TouchButtonRegion(InputButtons.Special, 0.36f, 0.36f, 0.075f, "S"),
            new TouchButtonRegion(InputButtons.Guard, 0.52f, 0.13f, 0.075f, "G"),
            new TouchButtonRegion(InputButtons.Dodge, 0.12f, 0.11f, 0.07f, "D"),
            new TouchButtonRegion(InputButtons.Jump, 0.52f, 0.29f, 0.065f, "J"),
            new TouchButtonRegion(InputButtons.Grab, 0.12f, 0.43f, 0.06f, "T"),
            new TouchButtonRegion(InputButtons.Execute, 0.27f, 0.52f, 0.06f, "X"),
            new TouchButtonRegion(InputButtons.Ultimate, 0.12f, 0.62f, 0.06f, "U"),
            new TouchButtonRegion(InputButtons.Rage, 0.46f, 0.52f, 0.05f, "R"),
            new TouchButtonRegion(InputButtons.Shadow, 0.60f, 0.46f, 0.05f, "Sh"),
        });

        /// <summary>Tablet layout: same topology, smaller relative sizes and more spacing.</summary>
        public static TouchLayout Tablet() => new TouchLayout(TabletId, 0.75f, 0.65f, 0.09f, new[]
        {
            new TouchButtonRegion(InputButtons.Light, 0.30f, 0.14f, 0.068f, "L"),
            new TouchButtonRegion(InputButtons.Heavy, 0.16f, 0.22f, 0.068f, "H"),
            new TouchButtonRegion(InputButtons.Special, 0.30f, 0.31f, 0.06f, "S"),
            new TouchButtonRegion(InputButtons.Guard, 0.44f, 0.11f, 0.06f, "G"),
            new TouchButtonRegion(InputButtons.Dodge, 0.10f, 0.09f, 0.056f, "D"),
            new TouchButtonRegion(InputButtons.Jump, 0.44f, 0.25f, 0.052f, "J"),
            new TouchButtonRegion(InputButtons.Grab, 0.10f, 0.37f, 0.048f, "T"),
            new TouchButtonRegion(InputButtons.Execute, 0.23f, 0.45f, 0.048f, "X"),
            new TouchButtonRegion(InputButtons.Ultimate, 0.10f, 0.53f, 0.048f, "U"),
            new TouchButtonRegion(InputButtons.Rage, 0.39f, 0.45f, 0.04f, "R"),
            new TouchButtonRegion(InputButtons.Shadow, 0.51f, 0.40f, 0.04f, "Sh"),
        });

        /// <summary>Layout by id (falls back to the phone layout).</summary>
        public static TouchLayout ById(string id) => id == TabletId ? Tablet() : Phone();

        /// <summary>
        /// Checks that no two buttons overlap at scale 1 and every button is fully on screen for aspect ratios down
        /// to <paramref name="minAspect"/>. Returns the problems found (empty when valid).
        /// </summary>
        public List<string> Validate(float minAspect)
        {
            var problems = new List<string>();
            for (int i = 0; i < Buttons.Count; i++)
            {
                var a = Buttons[i];
                if (a.CenterY - a.Radius < 0f || a.CenterY + a.Radius > 1f || a.CenterFromRight - a.Radius < 0f)
                {
                    problems.Add($"{Id}: {a.Button} leaves the screen");
                }

                if (minAspect - a.CenterFromRight - a.Radius < StickZoneWidth)
                {
                    problems.Add($"{Id}: {a.Button} overlaps the stick zone at aspect {minAspect}");
                }

                for (int j = i + 1; j < Buttons.Count; j++)
                {
                    var b = Buttons[j];
                    float dx = a.CenterFromRight - b.CenterFromRight;
                    float dy = a.CenterY - b.CenterY;
                    float min = a.Radius + b.Radius;
                    if (dx * dx + dy * dy < min * min)
                    {
                        problems.Add($"{Id}: {a.Button} overlaps {b.Button}");
                    }
                }
            }

            return problems;
        }
    }

    /// <summary>Phase of a touch, mirroring the platform's touch phases.</summary>
    public enum TouchPhaseKind : byte
    {
        /// <summary>Finger down.</summary>
        Began,

        /// <summary>Finger moved or held still.</summary>
        Moved,

        /// <summary>Finger lifted or the touch was cancelled.</summary>
        Ended,
    }

    /// <summary>
    /// Resolves raw touches into held buttons and a stick vector for a <see cref="TouchLayout"/>.
    /// A touch that starts in the stick zone becomes a floating stick anchored where it landed; a touch on a
    /// button presses it and may slide onto a neighbouring button (slide-press, used for fast chains).
    /// </summary>
    public sealed class TouchControlResolver
    {
        private const float HitForgiveness = 1.15f;

        private readonly Dictionary<int, InputButtons> _buttonTouches = new Dictionary<int, InputButtons>();
        private TouchLayout _layout;
        private int _stickTouch = -1;
        private float _originX;
        private float _originY;

        /// <summary>Creates a resolver.</summary>
        public TouchControlResolver(TouchLayout layout, float buttonScale = 1f)
        {
            SetLayout(layout, buttonScale);
        }

        /// <summary>Button scale multiplier.</summary>
        public float ButtonScale { get; private set; } = 1f;

        /// <summary>Stick vector (-1..1 each axis, magnitude ≤ 1).</summary>
        public float StickX { get; private set; }

        /// <summary>Stick vector (-1..1 each axis, magnitude ≤ 1).</summary>
        public float StickY { get; private set; }

        /// <summary>True while a finger drives the stick.</summary>
        public bool StickActive => _stickTouch >= 0;

        /// <summary>Screen-pixel anchor of the floating stick (for drawing).</summary>
        public float StickOriginX => _originX;

        /// <summary>Screen-pixel anchor of the floating stick (for drawing).</summary>
        public float StickOriginY => _originY;

        /// <summary>Buttons held by touches.</summary>
        public InputButtons Held
        {
            get
            {
                InputButtons held = InputButtons.None;
                foreach (var pair in _buttonTouches)
                {
                    held |= pair.Value;
                }

                return held;
            }
        }

        /// <summary>Changes the layout (settings menu) and releases every touch.</summary>
        public void SetLayout(TouchLayout layout, float buttonScale)
        {
            _layout = layout ?? throw new ArgumentNullException(nameof(layout));
            ButtonScale = buttonScale;
            Reset();
        }

        /// <summary>Releases everything.</summary>
        public void Reset()
        {
            _buttonTouches.Clear();
            _stickTouch = -1;
            StickX = 0f;
            StickY = 0f;
        }

        /// <summary>Feeds one touch update (pixel coordinates, origin bottom-left).</summary>
        public void Update(int touchId, TouchPhaseKind phase, float x, float y, float screenWidth, float screenHeight)
        {
            if (phase == TouchPhaseKind.Ended)
            {
                _buttonTouches.Remove(touchId);
                if (touchId == _stickTouch)
                {
                    _stickTouch = -1;
                    StickX = 0f;
                    StickY = 0f;
                }

                return;
            }

            float h = screenHeight;
            float nx = x / h;
            float ny = y / h;
            float aspect = screenWidth / h;

            if (phase == TouchPhaseKind.Began)
            {
                if (_stickTouch < 0 && nx <= _layout.StickZoneWidth && ny <= _layout.StickZoneHeight)
                {
                    _stickTouch = touchId;
                    _originX = x;
                    _originY = y;
                    StickX = 0f;
                    StickY = 0f;
                    return;
                }

                var hit = HitTest(nx, ny, aspect);
                if (hit != InputButtons.None)
                {
                    _buttonTouches[touchId] = hit;
                }

                return;
            }

            if (touchId == _stickTouch)
            {
                float radiusPixels = _layout.StickRadius * h;
                float dx = (x - _originX) / radiusPixels;
                float dy = (y - _originY) / radiusPixels;
                float magnitude = (float)Math.Sqrt(dx * dx + dy * dy);
                if (magnitude > 1f)
                {
                    dx /= magnitude;
                    dy /= magnitude;
                }

                StickX = dx;
                StickY = dy;
                return;
            }

            if (_buttonTouches.ContainsKey(touchId))
            {
                var hit = HitTest(nx, ny, aspect);
                if (hit != InputButtons.None)
                {
                    _buttonTouches[touchId] = hit;
                }
            }
        }

        /// <summary>The button under a normalised point (nearest centre wins), or None.</summary>
        public InputButtons HitTest(float nx, float ny, float aspect)
        {
            InputButtons best = InputButtons.None;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < _layout.ButtonCount; i++)
            {
                var region = _layout.ButtonAt(i);
                float cx = aspect - region.CenterFromRight;
                float dx = nx - cx;
                float dy = ny - region.CenterY;
                float distance = dx * dx + dy * dy;
                float reach = region.Radius * ButtonScale * HitForgiveness;
                if (distance <= reach * reach && distance < bestDistance)
                {
                    bestDistance = distance;
                    best = region.Button;
                }
            }

            return best;
        }
    }
}

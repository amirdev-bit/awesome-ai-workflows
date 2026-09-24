using System.Collections.Generic;
using Oathsunder.Combat.Definitions;
using Oathsunder.Combat.Input;
using Oathsunder.Controls;
using Oathsunder.Gameplay.Combat;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using ETouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Oathsunder.Gameplay.Controls
{
    /// <summary>
    /// The local player's <see cref="ICombatInputSource"/>: reads keyboard, gamepad and touch through the Unity
    /// Input System every rendered frame and hands the simulation one latched <see cref="InputFrame"/> per tick.
    /// </summary>
    /// <remarks>
    /// Runs before <see cref="CombatSimulationRunner"/> (execution order −200 vs −100), so the newest device
    /// state of a rendered frame reaches the tick simulated in that same frame: no extra frame of input lag.
    /// </remarks>
    [DefaultExecutionOrder(-200)]
    public sealed class PlayerInputSource : MonoBehaviour, ICombatInputSource
    {
        [SerializeField] private CombatSimulationRunner _runner;
        [SerializeField, Range(0, 3)] private int _fighterIndex;
        [Tooltip("Profile slot used for persistence (one per local player).")]
        [SerializeField, Range(0, 3)] private int _profileSlot;
        [SerializeField] private bool _enableTouch = true;

        private readonly Dictionary<InputButtons, InputAction> _buttonActions = new Dictionary<InputButtons, InputAction>();
        private readonly InputLatch _latch = new InputLatch();
        private InputAction _stick;
        private InputAction _left;
        private InputAction _right;
        private InputAction _up;
        private InputAction _down;
        private InputComposer _composer;
        private TouchControlResolver _touch;

        /// <summary>Active control profile.</summary>
        public ControlProfile Profile { get; private set; }

        /// <summary>Touch layout in use.</summary>
        public TouchLayout TouchLayout { get; private set; }

        /// <summary>Touch resolver (read by the on-screen controls overlay).</summary>
        public TouchControlResolver Touch => _touch;

        /// <summary>True when touch was the most recent input device (shows the on-screen controls).</summary>
        public bool TouchIsActiveDevice { get; private set; }

        /// <summary>Logical buttons currently held (for input display in training mode).</summary>
        public InputButtons CurrentButtons { get; private set; }

        /// <summary>The action bound to a logical button (used by the rebinding UI).</summary>
        public InputAction ActionFor(InputButtons button) => _buttonActions.TryGetValue(button, out var action) ? action : null;

        private void Awake()
        {
            Profile = ControlProfileStore.Load(_profileSlot);
            _composer = new InputComposer(Profile);
            TouchLayout = TouchLayout.ById(Profile.TouchLayoutId);
            _touch = new TouchControlResolver(TouchLayout, Profile.TouchButtonScale);
            BuildActions();
        }

        private void OnEnable()
        {
            if (_enableTouch)
            {
                EnhancedTouchSupport.Enable();
            }

            SetActionsEnabled(true);
            if (_runner != null)
            {
                _runner.SetInputSource(_fighterIndex, this);
            }
        }

        private void OnDisable()
        {
            SetActionsEnabled(false);
            _latch.Reset();
            _touch.Reset();
            if (_runner != null)
            {
                _runner.SetInputSource(_fighterIndex, NeutralInputSource.Instance);
            }
        }

        private void OnDestroy()
        {
            DisposeActions();
        }

        /// <summary>Applies an edited profile (settings or remap screen) and persists it.</summary>
        public void ApplyProfile(ControlProfile profile)
        {
            Profile = profile;
            _composer.Apply(profile);
            TouchLayout = TouchLayout.ById(profile.TouchLayoutId);
            _touch.SetLayout(TouchLayout, profile.TouchButtonScale);
            DisposeActions();
            BuildActions();
            SetActionsEnabled(isActiveAndEnabled);
            ControlProfileStore.Save(_profileSlot, profile);
        }

        /// <inheritdoc />
        public InputFrame Sample(int fighterIndex, int frame) => _latch.ConsumeTick();

        private void Update()
        {
            var snapshot = new DeviceSnapshot();
            foreach (var pair in _buttonActions)
            {
                if (pair.Value.IsPressed())
                {
                    snapshot.Held |= pair.Key;
                }
            }

            Vector2 stick = _stick.ReadValue<Vector2>();
            snapshot.StickX = stick.x;
            snapshot.StickY = stick.y;
            snapshot.Left = _left.IsPressed();
            snapshot.Right = _right.IsPressed();
            snapshot.Up = _up.IsPressed();
            snapshot.Down = _down.IsPressed();

            if (_enableTouch)
            {
                ReadTouches();
                snapshot.Held |= _touch.Held;
                snapshot.TouchStickActive = _touch.StickActive;
                snapshot.TouchStickX = _touch.StickX;
                snapshot.TouchStickY = _touch.StickY;
            }

            if (snapshot.Held != InputButtons.None || snapshot.Left || snapshot.Right || snapshot.Up || snapshot.Down ||
                stick.sqrMagnitude > 0.1f)
            {
                TouchIsActiveDevice = _touch.StickActive || _touch.Held != InputButtons.None;
            }

            _composer.Compose(snapshot, out var buttons, out int x, out int y);
            CurrentButtons = buttons;
            _latch.Sample(buttons, x, y);
        }

        private void ReadTouches()
        {
            float width = Screen.width;
            float height = Screen.height;
            foreach (var touch in ETouch.activeTouches)
            {
                TouchPhaseKind phase;
                switch (touch.phase)
                {
                    case ETouchPhase.Began:
                        phase = TouchPhaseKind.Began;
                        break;
                    case ETouchPhase.Ended:
                    case ETouchPhase.Canceled:
                        phase = TouchPhaseKind.Ended;
                        break;
                    default:
                        phase = TouchPhaseKind.Moved;
                        break;
                }

                Vector2 position = touch.screenPosition;
                _touch.Update(touch.touchId, phase, position.x, position.y, width, height);
            }
        }

        private void BuildActions()
        {
            foreach (var button in ControlProfile.RemappableButtons)
            {
                var action = new InputAction(button.ToString(), InputActionType.Button);
                foreach (var path in Profile.BindingsOf(button))
                {
                    action.AddBinding(path);
                }

                _buttonActions[button] = action;
            }

            _stick = new InputAction("Stick", InputActionType.Value, "<Gamepad>/leftStick");
            _left = Direction("Left", "<Keyboard>/a", "<Keyboard>/leftArrow", "<Gamepad>/dpad/left");
            _right = Direction("Right", "<Keyboard>/d", "<Keyboard>/rightArrow", "<Gamepad>/dpad/right");
            _up = Direction("Up", "<Keyboard>/w", "<Keyboard>/upArrow", "<Gamepad>/dpad/up");
            _down = Direction("Down", "<Keyboard>/s", "<Keyboard>/downArrow", "<Gamepad>/dpad/down");
        }

        private static InputAction Direction(string name, params string[] paths)
        {
            var action = new InputAction(name, InputActionType.Button);
            foreach (var path in paths)
            {
                action.AddBinding(path);
            }

            return action;
        }

        private void SetActionsEnabled(bool enabled)
        {
            foreach (var action in AllActions())
            {
                if (action == null)
                {
                    continue;
                }

                if (enabled)
                {
                    action.Enable();
                }
                else
                {
                    action.Disable();
                }
            }
        }

        private void DisposeActions()
        {
            foreach (var action in AllActions())
            {
                action?.Dispose();
            }

            _buttonActions.Clear();
        }

        private IEnumerable<InputAction> AllActions()
        {
            foreach (var action in _buttonActions.Values)
            {
                yield return action;
            }

            yield return _stick;
            yield return _left;
            yield return _right;
            yield return _up;
            yield return _down;
        }
    }
}

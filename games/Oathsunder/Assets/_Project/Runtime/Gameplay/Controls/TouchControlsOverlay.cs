using System.Collections.Generic;
using Oathsunder.Combat.Input;
using Oathsunder.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace Oathsunder.Gameplay.Controls
{
    /// <summary>
    /// Draws the on-screen touch controls with UI Toolkit from the same <see cref="TouchLayout"/> the resolver
    /// hit-tests, so what the player sees is exactly what responds. Hidden while a gamepad or keyboard is in use.
    /// Styling (brush-stroke art, colour-blind palettes) is layered on in Phase 12 via USS classes.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class TouchControlsOverlay : MonoBehaviour
    {
        private const string ButtonClass = "os-touch-button";
        private const string HeldClass = "os-touch-button--held";

        private static readonly Color Idle = new Color(1f, 1f, 1f, 0.18f);
        private static readonly Color Held = new Color(1f, 0.62f, 0.25f, 0.55f);

        [SerializeField] private PlayerInputSource _source;
        [SerializeField, Range(0f, 1f)] private float _opacity = 0.85f;

        private readonly Dictionary<InputButtons, VisualElement> _buttons = new Dictionary<InputButtons, VisualElement>();
        private VisualElement _root;
        private VisualElement _stickBase;
        private VisualElement _stickKnob;
        private TouchLayout _builtFor;
        private float _builtHeight;

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
        }

        private void LateUpdate()
        {
            if (_source == null || _root == null)
            {
                return;
            }

            float height = _root.resolvedStyle.height;
            float width = _root.resolvedStyle.width;
            if (float.IsNaN(height) || height <= 0f)
            {
                return;
            }

            if (_builtFor != _source.TouchLayout || !Mathf.Approximately(_builtHeight, height))
            {
                Build(_source.TouchLayout, width, height);
            }

            _root.style.display = _source.TouchIsActiveDevice || Application.isMobilePlatform ? DisplayStyle.Flex : DisplayStyle.None;
            var held = _source.Touch.Held;
            foreach (var pair in _buttons)
            {
                bool isHeld = (held & pair.Key) != 0;
                pair.Value.EnableInClassList(HeldClass, isHeld);
                pair.Value.style.backgroundColor = isHeld ? Held : Idle;
            }

            var touch = _source.Touch;
            _stickBase.style.display = touch.StickActive ? DisplayStyle.Flex : DisplayStyle.None;
            if (touch.StickActive)
            {
                float scale = height / Screen.height;
                float radius = _source.TouchLayout.StickRadius * height;
                float cx = touch.StickOriginX * scale;
                float cy = height - touch.StickOriginY * scale;
                Place(_stickBase, cx, cy, radius);
                Place(_stickKnob, cx + touch.StickX * radius, cy - touch.StickY * radius, radius * 0.45f);
            }
        }

        private void Build(TouchLayout layout, float width, float height)
        {
            _root.Clear();
            _buttons.Clear();
            _root.style.opacity = _opacity;
            _root.pickingMode = PickingMode.Ignore;
            float aspect = width / height;
            float scale = _source.Profile.TouchButtonScale;
            foreach (var region in layout.Buttons)
            {
                var element = new VisualElement { name = "touch-" + region.Button };
                element.AddToClassList(ButtonClass);
                element.pickingMode = PickingMode.Ignore;
                var label = new Label(region.Label) { pickingMode = PickingMode.Ignore };
                label.style.unityTextAlign = TextAnchor.MiddleCenter;
                label.style.flexGrow = 1f;
                element.Add(label);
                float radius = region.Radius * scale * height;
                Place(element, (aspect - region.CenterFromRight) * height, height - region.CenterY * height, radius);
                _root.Add(element);
                _buttons[region.Button] = element;
            }

            _stickBase = Circle("touch-stick-base", new Color(1f, 1f, 1f, 0.12f));
            _stickKnob = Circle("touch-stick-knob", new Color(1f, 1f, 1f, 0.35f));
            _root.Add(_stickBase);
            _stickBase.style.display = DisplayStyle.None;
            _root.Add(_stickKnob);
            _builtFor = layout;
            _builtHeight = height;
        }

        private static VisualElement Circle(string name, Color color)
        {
            var element = new VisualElement { name = name, pickingMode = PickingMode.Ignore };
            element.style.backgroundColor = color;
            return element;
        }

        private static void Place(VisualElement element, float centerX, float centerY, float radius)
        {
            element.style.position = Position.Absolute;
            element.style.left = centerX - radius;
            element.style.top = centerY - radius;
            element.style.width = radius * 2f;
            element.style.height = radius * 2f;
            var round = new Length(50f, LengthUnit.Percent);
            element.style.borderTopLeftRadius = round;
            element.style.borderTopRightRadius = round;
            element.style.borderBottomLeftRadius = round;
            element.style.borderBottomRightRadius = round;
        }
    }
}

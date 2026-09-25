// Minimal compile-only stand-ins for the UnityEngine / UnityEditor members used by the project.
// Signatures mirror Unity 6; bodies are intentionally empty. Never shipped, never referenced by Unity.
#pragma warning disable
using System;
using System.Collections;

namespace UnityEngine
{
    public class Object
    {
        public string name { get; set; }
        public static T Instantiate<T>(T original, Transform parent) where T : Object => original;
        public static void Destroy(Object obj) { }
        public static void DestroyImmediate(Object obj) { }
        public static implicit operator bool(Object obj) => obj != null;
    }

    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject => default;
    }

    public class Component : Object
    {
        public Transform transform => null;
        public GameObject gameObject => null;
        public T GetComponent<T>() => default;
    }

    public class Behaviour : Component
    {
        public bool enabled { get; set; }
        public bool isActiveAndEnabled => true;
    }

    public class MonoBehaviour : Behaviour { }

    public sealed class GameObject : Object
    {
        public GameObject() { }
        public GameObject(string name) { }
        public Transform transform => null;
        public void SetActive(bool value) { }
        public T AddComponent<T>() where T : Component => default;
    }

    public class Transform : Component
    {
        public Vector3 position { get; set; }
        public Transform parent { get; set; }
        public Vector3 localPosition { get; set; }
        public Quaternion rotation { get; set; }
        public Quaternion localRotation { get; set; }
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation) { }
    }

    public struct Vector2
    {
        public float x, y;
        public float sqrMagnitude => x * x + y * y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 operator *(Vector2 a, float d) => a;
        public static implicit operator Vector3(Vector2 v) => new Vector3(v.x, v.y, 0f);
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 LerpUnclamped(Vector3 a, Vector3 b, float t) => a;
        public static Vector3 operator +(Vector3 a, Vector3 b) => a;
        public static Vector3 operator -(Vector3 a, Vector3 b) => a;
        public static Vector3 zero => default;
        public float magnitude => 0f;
        public static Vector3 up => default;
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => a;
    }

    public struct Quaternion
    {
        public static Quaternion identity => default;
        public static Quaternion Euler(float x, float y, float z) => default;
        public static Quaternion LookRotation(Vector3 forward, Vector3 upwards) => default;
    }

    public struct Color
    {
        public Color(float r, float g, float b) { }
        public Color(float r, float g, float b, float a) { }
        public static Color white => default;
    }

    public struct Bounds
    {
        public Bounds(Vector3 center, Vector3 size) { this.center = center; this.size = size; }
        public Vector3 center { get; set; }
        public Vector3 size { get; set; }
    }

    public struct Rect
    {
        public Rect(float x, float y, float width, float height) { this.x = x; this.y = y; this.width = width; this.height = height; }
        public float x { get; set; }
        public float y { get; set; }
        public float width { get; set; }
        public float height { get; set; }
        public float xMax => x + width;
        public float yMax => y + height;
    }

    public static class Mathf
    {
        public const float Deg2Rad = 0.0174532924f;
        public static float Min(float a, float b) => a;
        public static float Lerp(float a, float b, float t) => a;
        public static float Exp(float power) => power;
        public static float Tan(float f) => f;
        public static float Sqrt(float f) => f;
        public static bool Approximately(float a, float b) => true;
        public static float Clamp(float v, float min, float max) => v;
        public static float Max(float a, float b) => a;
        public static int Max(int a, int b) => a;
        public static float Clamp01(float v) => v;
        public static int Clamp(int v, int min, int max) => v;
        public static float MoveTowards(float current, float target, float maxDelta) => current;
        public static float MoveTowardsAngle(float current, float target, float maxDelta) => current;
        public static float Pow(float f, float p) => f;
        public static int RoundToInt(float f) => 0;
        public static float Abs(float f) => f;
        public static float PerlinNoise(float x, float y) => x;
        public static float SmoothStep(float from, float to, float t) => from;
    }

    public static class Time
    {
        public static float deltaTime => 0f;
        public static float unscaledDeltaTime => 0f;
        public static float unscaledTime => 0f;
    }

    public static class Debug
    {
        public static void Log(object message) { }
        public static void LogError(object message) { }
        public static void LogWarning(object message) { }
        public static void LogError(object message, Object context) { }
    }

    public static class Random
    {
        public static Vector2 insideUnitCircle => default;
        public static float value => 0f;
    }

    public static class Handheld
    {
        public static void Vibrate() { }
    }

    public static class Application
    {
        public static string dataPath => "";
        public static string persistentDataPath => "";
        public static bool isMobilePlatform => false;
    }

    public static class Screen
    {
        public static int width => 0;
        public static int height => 0;
        public static float dpi => 0f;
    }

    public class Camera : Behaviour
    {
        public float fieldOfView { get; set; }
        public float aspect { get; set; }
    }

    public enum TextAnchor { UpperLeft, UpperCenter, UpperRight, MiddleLeft, MiddleCenter, MiddleRight, LowerLeft, LowerCenter, LowerRight }

    [AttributeUsage(AttributeTargets.Class)] public sealed class RequireComponent : Attribute { public RequireComponent(Type type) { } }
    [AttributeUsage(AttributeTargets.Field)] public sealed class HeaderAttribute : PropertyAttribute { public HeaderAttribute(string header) { } }

    public class Animator : Behaviour
    {
        public float speed { get; set; }
        public bool applyRootMotion { get; set; }
        public void Play(int stateNameHash, int layer, float normalizedTime) { }
        public void Update(float deltaTime) { }
        public static int StringToHash(string name) => 0;
    }

    public class AudioClip : Object { }

    public class Motion : Object
    {
        public Vector3 averageSpeed => default;
        public bool isLooping => false;
    }

    public sealed class AnimationClip : Motion
    {
        public float length => 0f;
        public float frameRate { get; set; }
    }

    public class AudioSource : Behaviour
    {
        public AudioClip clip { get; set; }
        public float volume { get; set; }
        public float pitch { get; set; }
        public float spatialBlend { get; set; }
        public bool playOnAwake { get; set; }
        public bool isPlaying => false;
        public Audio.AudioMixerGroup outputAudioMixerGroup { get; set; }
        public void Play() { }
        public void Stop() { }
        public void PlayOneShot(AudioClip clip, float volumeScale) { }
    }

    public class TextAsset : Object
    {
        public string text => "";
    }

    public static class Gizmos
    {
        public static Color color { get; set; }
        public static void DrawLine(Vector3 from, Vector3 to) { }
        public static void DrawWireCube(Vector3 center, Vector3 size) { }
    }

    public class GUIStyle { }

    public sealed class GUILayoutOption { }

    public static class GUILayout
    {
        public static void Label(string text, GUIStyle style, params GUILayoutOption[] options) { }
        public static GUILayoutOption Width(float width) => null;
        public static float HorizontalSlider(float value, float left, float right, params GUILayoutOption[] options) => value;
        public static void FlexibleSpace() { }
        public static bool Button(string text, GUIStyle style, params GUILayoutOption[] options) => false;
    }

    public static class GUILayoutUtility
    {
        public static Rect GetRect(float width, float height) => default;
        public static Rect GetRect(float width, float height, params GUILayoutOption[] options) => default;
    }

    public static class GUI
    {
        public static void Label(Rect position, string text, GUIStyle style) { }
    }

    public class PropertyAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field)] public sealed class SerializeField : Attribute { }
    [AttributeUsage(AttributeTargets.Field)] public sealed class TooltipAttribute : PropertyAttribute { public TooltipAttribute(string tooltip) { } }
    [AttributeUsage(AttributeTargets.Field)] public sealed class RangeAttribute : PropertyAttribute { public RangeAttribute(float min, float max) { } }
    [AttributeUsage(AttributeTargets.Field)] public sealed class MinAttribute : PropertyAttribute { public MinAttribute(float min) { } }
    [AttributeUsage(AttributeTargets.Class)] public sealed class CreateAssetMenuAttribute : Attribute { public string fileName { get; set; } public string menuName { get; set; } public int order { get; set; } }
    [AttributeUsage(AttributeTargets.Class)] public sealed class DefaultExecutionOrder : Attribute { public DefaultExecutionOrder(int order) { } }
}

namespace UnityEngine.Audio
{
    public class AudioMixerGroup : Object { }
}

namespace UnityEngine.TestTools
{
    [AttributeUsage(AttributeTargets.Method)] public sealed class UnityTestAttribute : Attribute { }
}

namespace UnityEditor
{
    using UnityEngine;

    public static class AssetDatabase
    {
        public static string[] FindAssets(string filter) => new string[0];
        public static string[] FindAssets(string filter, string[] searchInFolders) => new string[0];
        public static string GUIDToAssetPath(string guid) => "";
        public static T LoadAssetAtPath<T>(string path) where T : Object => default;
        public static Object[] LoadAllAssetsAtPath(string assetPath) => new Object[0];
        public static void SaveAssets() { }
    }

    public static class EditorApplication
    {
        public static void Exit(int returnValue) { }
    }

    public static class EditorUtility
    {
        public static void SetDirty(Object target) { }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MenuItem : Attribute
    {
        public MenuItem(string itemName) { }
        public int priority { get; set; }
    }

    public class AssetPostprocessor
    {
        public string assetPath => "";
    }

    public class EditorWindow : ScriptableObject
    {
        public Rect position { get; set; }
        public static T GetWindow<T>(string title) where T : EditorWindow => default;
        public void Repaint() { }
    }

    public enum MessageType { None, Info, Warning, Error }

    public static class EditorStyles
    {
        public static GUIStyle toolbar => null;
        public static GUIStyle toolbarPopup => null;
        public static GUIStyle toolbarButton => null;
        public static GUIStyle miniLabel => null;
    }

    public static class EditorGUI
    {
        public static void DrawRect(Rect rect, Color color) { }
    }

    public static class EditorGUIUtility
    {
        public static string systemCopyBuffer { get; set; }
    }

    public static class EditorGUILayout
    {
        public sealed class HorizontalScope : IDisposable
        {
            public HorizontalScope(params GUILayoutOption[] options) { }
            public HorizontalScope(GUIStyle style, params GUILayoutOption[] options) { }
            public void Dispose() { }
        }

        public static int Popup(int selectedIndex, string[] displayedOptions, GUIStyle style, params GUILayoutOption[] options) => selectedIndex;
        public static void HelpBox(string message, MessageType type) { }
        public static Vector2 BeginScrollView(Vector2 scrollPosition, params GUILayoutOption[] options) => scrollPosition;
        public static void EndScrollView() { }
    }
}

namespace UnityEngine.InputSystem.Utilities
{
    public struct ReadOnlyArray<T> : System.Collections.Generic.IEnumerable<T>
    {
        public int Count => 0;
        public T this[int index] => default;
        public System.Collections.Generic.IEnumerator<T> GetEnumerator() { yield break; }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

namespace UnityEngine.InputSystem
{
    using System;
    using UnityEngine.InputSystem.Utilities;

    public enum InputActionType { Value, Button, PassThrough }

    public enum TouchPhase { None, Began, Moved, Ended, Canceled, Stationary }

    public struct InputBinding
    {
        public string effectivePath => "";
        public string path { get; set; }
    }

    public struct BindingSyntax { }

    public sealed class InputAction : IDisposable
    {
        public InputAction(string name = null, InputActionType type = InputActionType.Value, string binding = null) { }
        public ReadOnlyArray<InputBinding> bindings => default;
        public BindingSyntax AddBinding(string path) => default;
        public void Enable() { }
        public void Disable() { }
        public void Dispose() { }
        public bool IsPressed() => false;
        public TValue ReadValue<TValue>() where TValue : struct => default;
    }

    public static class InputActionRebindingExtensions
    {
        public sealed class RebindingOperation : IDisposable
        {
            public RebindingOperation WithControlsExcluding(string path) => this;
            public RebindingOperation WithCancelingThrough(string path) => this;
            public RebindingOperation OnMatchWaitForAnother(float seconds) => this;
            public RebindingOperation OnComplete(Action<RebindingOperation> callback) => this;
            public RebindingOperation OnCancel(Action<RebindingOperation> callback) => this;
            public RebindingOperation Start() => this;
            public void Cancel() { }
            public void Dispose() { }
        }

        public static RebindingOperation PerformInteractiveRebinding(this InputAction action, int bindingIndex = -1) => new RebindingOperation();
    }

    public class InputDevice { }

    public class Gamepad : InputDevice
    {
        public static Gamepad current => null;
        public void SetMotorSpeeds(float lowFrequency, float highFrequency) { }
        public void ResetHaptics() { }
    }
}

namespace UnityEngine.InputSystem.EnhancedTouch
{
    using UnityEngine.InputSystem.Utilities;

    public static class EnhancedTouchSupport
    {
        public static void Enable() { }
    }

    public struct Touch
    {
        public static ReadOnlyArray<Touch> activeTouches => default;
        public int touchId => 0;
        public UnityEngine.InputSystem.TouchPhase phase => default;
        public Vector2 screenPosition => default;
    }
}

namespace UnityEngine.UIElements
{
    public enum DisplayStyle { Flex, None }
    public enum Position { Relative, Absolute }
    public enum PickingMode { Position, Ignore }
    public enum LengthUnit { Pixel, Percent }

    public struct Length
    {
        public Length(float value, LengthUnit unit) { }
    }

    public struct StyleLength
    {
        public static implicit operator StyleLength(float v) => default;
        public static implicit operator StyleLength(Length v) => default;
    }

    public struct StyleFloat { public static implicit operator StyleFloat(float v) => default; }
    public struct StyleColor { public static implicit operator StyleColor(Color v) => default; }
    public struct StyleEnum<T> where T : struct { public static implicit operator StyleEnum<T>(T v) => default; }

    public interface IStyle
    {
        StyleEnum<Position> position { get; set; }
        StyleLength left { get; set; }
        StyleLength top { get; set; }
        StyleLength width { get; set; }
        StyleLength height { get; set; }
        StyleLength borderTopLeftRadius { get; set; }
        StyleLength borderTopRightRadius { get; set; }
        StyleLength borderBottomLeftRadius { get; set; }
        StyleLength borderBottomRightRadius { get; set; }
        StyleColor backgroundColor { get; set; }
        StyleEnum<DisplayStyle> display { get; set; }
        StyleFloat opacity { get; set; }
        StyleFloat flexGrow { get; set; }
        StyleEnum<TextAnchor> unityTextAlign { get; set; }
    }

    public interface IResolvedStyle
    {
        float width { get; }
        float height { get; }
    }

    public class VisualElement
    {
        public string name { get; set; }
        public PickingMode pickingMode { get; set; }
        public IStyle style => null;
        public IResolvedStyle resolvedStyle => null;
        public void Add(VisualElement child) { }
        public void Clear() { }
        public void AddToClassList(string className) { }
        public void EnableInClassList(string className, bool enable) { }
    }

    public class Label : VisualElement
    {
        public Label(string text) { }
    }

    public sealed class UIDocument : MonoBehaviour
    {
        public VisualElement rootVisualElement => null;
    }
}

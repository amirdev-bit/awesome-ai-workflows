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
        public Vector3 localPosition { get; set; }
        public Quaternion rotation { get; set; }
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation) { }
    }

    public struct Vector2
    {
        public float x, y;
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
    }

    public struct Quaternion
    {
        public static Quaternion identity => default;
        public static Quaternion Euler(float x, float y, float z) => default;
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
        public static float Max(float a, float b) => a;
        public static int Max(int a, int b) => a;
        public static float Clamp01(float v) => v;
        public static int Clamp(int v, int min, int max) => v;
        public static float MoveTowards(float current, float target, float maxDelta) => current;
        public static float MoveTowardsAngle(float current, float target, float maxDelta) => current;
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
        public static void LogError(object message, Object context) { }
    }

    public static class Random
    {
        public static Vector2 insideUnitCircle => default;
    }

    public static class Handheld
    {
        public static void Vibrate() { }
    }

    public static class Application
    {
        public static string dataPath => "";
    }

    public class Animator : Behaviour
    {
        public float speed { get; set; }
        public bool applyRootMotion { get; set; }
        public void Play(int stateNameHash, int layer, float normalizedTime) { }
        public void Update(float deltaTime) { }
        public static int StringToHash(string name) => 0;
    }

    public class AudioClip : Object { }

    public class AudioSource : Behaviour
    {
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

    public class AssetPostprocessor { }

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

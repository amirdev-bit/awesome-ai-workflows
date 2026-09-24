using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Oathsunder.Combat.Definitions;
using Oathsunder.Combat.Input;
using Oathsunder.Core.Serialization;

namespace Oathsunder.Controls
{
    /// <summary>
    /// A player's control settings: scheme, direction handling, and remappable bindings for every logical
    /// button. Binding paths use Unity Input System control-path syntax (e.g. <c>&lt;Gamepad&gt;/buttonWest</c>) but
    /// the profile itself is engine-free, so it can be validated, diffed and synced to the cloud save.
    /// </summary>
    public sealed class ControlProfile
    {
        /// <summary>Current format version of the serialized profile.</summary>
        public const int FormatVersion = 1;

        /// <summary>Every remappable logical button, in display order.</summary>
        public static readonly InputButtons[] RemappableButtons =
        {
            InputButtons.Light, InputButtons.Heavy, InputButtons.Special, InputButtons.Guard, InputButtons.Dodge,
            InputButtons.Jump, InputButtons.Grab, InputButtons.Execute, InputButtons.Ultimate, InputButtons.Rage,
            InputButtons.Shadow,
        };

        private readonly Dictionary<InputButtons, List<string>> _bindings = new Dictionary<InputButtons, List<string>>();

        /// <summary>Control scheme.</summary>
        public ControlScheme Scheme { get; set; } = ControlScheme.Classic;

        /// <summary>Pushing the stick/keys up also presses Jump (Classic players on stick often want this).</summary>
        public bool UpToJump { get; set; }

        /// <summary>Opposite-direction resolution for digital devices.</summary>
        public SocdMode Socd { get; set; } = SocdMode.NeutralHorizontalUpPriority;

        /// <summary>Analog stick deadzone (0..1).</summary>
        public float StickDeadzone { get; set; } = 0.35f;

        /// <summary>Angular width of the diagonal sectors in degrees.</summary>
        public float DiagonalSectorDegrees { get; set; } = 30f;

        /// <summary>Heavy + Special pressed together also sends Execute.</summary>
        public bool ExecuteChord { get; set; } = true;

        /// <summary>Touch layout id (see <see cref="TouchLayout"/>).</summary>
        public string TouchLayoutId { get; set; } = TouchLayout.PhoneId;

        /// <summary>Touch button scale multiplier (accessibility).</summary>
        public float TouchButtonScale { get; set; } = 1f;

        /// <summary>Binding paths of a logical button (empty list when unbound).</summary>
        public IReadOnlyList<string> BindingsOf(InputButtons button) =>
            _bindings.TryGetValue(button, out var list) ? list : (IReadOnlyList<string>)Array.Empty<string>();

        /// <summary>Replaces every binding of a button.</summary>
        public void SetBindings(InputButtons button, params string[] paths)
        {
            ValidateButton(button);
            var list = new List<string>();
            foreach (var path in paths)
            {
                if (!string.IsNullOrWhiteSpace(path) && !list.Contains(path))
                {
                    list.Add(path);
                }
            }

            _bindings[button] = list;
        }

        /// <summary>
        /// Binds <paramref name="path"/> to <paramref name="button"/> and removes it from any other button of the
        /// same device family, returning the button that lost it (or <see cref="InputButtons.None"/>).
        /// </summary>
        public InputButtons Rebind(InputButtons button, string path, string replacedPath = null)
        {
            ValidateButton(button);
            InputButtons displaced = InputButtons.None;
            foreach (var pair in _bindings)
            {
                if (pair.Key != button && pair.Value.Remove(path))
                {
                    displaced = pair.Key;
                }
            }

            if (!_bindings.TryGetValue(button, out var list))
            {
                list = new List<string>();
                _bindings[button] = list;
            }

            if (replacedPath != null)
            {
                list.Remove(replacedPath);
            }

            if (!list.Contains(path))
            {
                list.Add(path);
            }

            return displaced;
        }

        /// <summary>Paths bound to more than one logical button (shown as warnings in the remap screen).</summary>
        public List<string> FindConflicts()
        {
            var owners = new Dictionary<string, InputButtons>(StringComparer.Ordinal);
            var conflicts = new List<string>();
            foreach (var pair in _bindings)
            {
                foreach (var path in pair.Value)
                {
                    if (owners.TryGetValue(path, out var owner) && owner != pair.Key)
                    {
                        if (!conflicts.Contains(path))
                        {
                            conflicts.Add(path);
                        }
                    }
                    else
                    {
                        owners[path] = pair.Key;
                    }
                }
            }

            return conflicts;
        }

        /// <summary>Shipped defaults for gamepad and keyboard.</summary>
        public static ControlProfile CreateDefault(ControlScheme scheme)
        {
            var profile = new ControlProfile { Scheme = scheme };
            profile.SetBindings(InputButtons.Light, "<Gamepad>/buttonWest", "<Keyboard>/j");
            profile.SetBindings(InputButtons.Heavy, "<Gamepad>/buttonNorth", "<Keyboard>/k");
            profile.SetBindings(InputButtons.Special, "<Gamepad>/buttonEast", "<Keyboard>/l");
            profile.SetBindings(InputButtons.Jump, "<Gamepad>/buttonSouth", "<Keyboard>/space");
            profile.SetBindings(InputButtons.Guard, "<Gamepad>/rightShoulder", "<Keyboard>/i");
            profile.SetBindings(InputButtons.Dodge, "<Gamepad>/leftShoulder", "<Keyboard>/u");
            profile.SetBindings(InputButtons.Grab, "<Gamepad>/leftTrigger", "<Keyboard>/o");
            profile.SetBindings(InputButtons.Execute, "<Gamepad>/rightStickPress", "<Keyboard>/h");
            profile.SetBindings(InputButtons.Ultimate, "<Gamepad>/rightTrigger", "<Keyboard>/semicolon");
            profile.SetBindings(InputButtons.Rage, "<Gamepad>/leftStickPress", "<Keyboard>/q");
            profile.SetBindings(InputButtons.Shadow, "<Gamepad>/select", "<Keyboard>/e");
            return profile;
        }

        /// <summary>Serializes to JSON (stored in the player's save and cloud profile).</summary>
        public string ToJson()
        {
            var text = new StringBuilder(1024);
            text.Append('{');
            Property(text, "version", FormatVersion.ToString(CultureInfo.InvariantCulture)).Append(',');
            Property(text, "scheme", Quote(Scheme.ToString())).Append(',');
            Property(text, "upToJump", UpToJump ? "true" : "false").Append(',');
            Property(text, "socd", Quote(Socd.ToString())).Append(',');
            Property(text, "stickDeadzone", StickDeadzone.ToString("0.###", CultureInfo.InvariantCulture)).Append(',');
            Property(text, "diagonalSector", DiagonalSectorDegrees.ToString("0.###", CultureInfo.InvariantCulture)).Append(',');
            Property(text, "executeChord", ExecuteChord ? "true" : "false").Append(',');
            Property(text, "touchLayout", Quote(TouchLayoutId)).Append(',');
            Property(text, "touchButtonScale", TouchButtonScale.ToString("0.###", CultureInfo.InvariantCulture)).Append(',');
            text.Append("\"bindings\":{");
            bool first = true;
            foreach (var button in RemappableButtons)
            {
                if (!first)
                {
                    text.Append(',');
                }

                first = false;
                text.Append(Quote(button.ToString())).Append(":[");
                var list = BindingsOf(button);
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0)
                    {
                        text.Append(',');
                    }

                    text.Append(Quote(list[i]));
                }

                text.Append(']');
            }

            text.Append("}}");
            return text.ToString();
        }

        /// <summary>Parses a profile. Unknown buttons are rejected; missing fields keep their defaults.</summary>
        /// <exception cref="JsonSyntaxException">Malformed JSON.</exception>
        /// <exception cref="JsonContentException">Invalid values.</exception>
        public static ControlProfile FromJson(string json)
        {
            var root = JsonReader.Parse(json, "controls.json");
            root.RequireKind(JsonKind.Object);
            int version = root.GetInt("version", FormatVersion);
            if (version > FormatVersion)
            {
                throw new JsonContentException(root.Path + ".version", $"profile version {version} is newer than {FormatVersion}");
            }

            var profile = CreateDefault(ControlScheme.Classic);
            profile.Scheme = ParseEnum<ControlScheme>(root, "scheme", ControlScheme.Classic);
            profile.UpToJump = root.GetBool("upToJump", false);
            profile.Socd = ParseEnum<SocdMode>(root, "socd", SocdMode.NeutralHorizontalUpPriority);
            profile.StickDeadzone = ClampFloat(root, "stickDeadzone", 0.35f, 0f, 0.9f);
            profile.DiagonalSectorDegrees = ClampFloat(root, "diagonalSector", 30f, 5f, 85f);
            profile.ExecuteChord = root.GetBool("executeChord", true);
            profile.TouchLayoutId = root.GetString("touchLayout", TouchLayout.PhoneId);
            profile.TouchButtonScale = ClampFloat(root, "touchButtonScale", 1f, 0.6f, 1.8f);
            var bindings = root.Get("bindings");
            if (bindings != null)
            {
                bindings.RequireKind(JsonKind.Object);
                foreach (var member in bindings.Members)
                {
                    if (!Enum.TryParse(member.Key, false, out InputButtons button) || Array.IndexOf(RemappableButtons, button) < 0)
                    {
                        throw new JsonContentException(member.Value.Path, $"'{member.Key}' is not a remappable button");
                    }

                    member.Value.RequireKind(JsonKind.Array);
                    var paths = new string[member.Value.Count];
                    for (int i = 0; i < paths.Length; i++)
                    {
                        paths[i] = member.Value.Items[i].AsString();
                    }

                    profile.SetBindings(button, paths);
                }
            }

            return profile;
        }

        private static void ValidateButton(InputButtons button)
        {
            if (Array.IndexOf(RemappableButtons, button) < 0)
            {
                throw new ArgumentException($"{button} is not a single remappable button.", nameof(button));
            }
        }

        private static T ParseEnum<T>(JsonNode root, string name, T fallback)
            where T : struct
        {
            var node = root.Get(name);
            if (node == null)
            {
                return fallback;
            }

            if (Enum.TryParse(node.AsString(), false, out T value) && Enum.IsDefined(typeof(T), value))
            {
                return value;
            }

            throw new JsonContentException(node.Path, $"'{node.AsString()}' is not a valid {typeof(T).Name}");
        }

        private static float ClampFloat(JsonNode root, string name, float fallback, float min, float max)
        {
            var node = root.Get(name);
            if (node == null)
            {
                return fallback;
            }

            node.RequireKind(JsonKind.Number);
            float value = float.Parse(node.RawText, NumberStyles.Float, CultureInfo.InvariantCulture);
            return value < min ? min : (value > max ? max : value);
        }

        private static StringBuilder Property(StringBuilder text, string name, string value) =>
            text.Append('"').Append(name).Append("\":").Append(value);

        private static string Quote(string value)
        {
            var text = new StringBuilder(value.Length + 2);
            text.Append('"');
            foreach (char c in value)
            {
                switch (c)
                {
                    case '"': text.Append("\\\""); break;
                    case '\\': text.Append("\\\\"); break;
                    default:
                        if (c < 0x20)
                        {
                            text.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            text.Append(c);
                        }

                        break;
                }
            }

            return text.Append('"').ToString();
        }
    }
}

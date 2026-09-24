using System;
using System.Collections.Generic;
using Oathsunder.Core.Mathematics;

namespace Oathsunder.Core.Serialization
{
    /// <summary>Kind of a parsed JSON value.</summary>
    public enum JsonKind
    {
        /// <summary>JSON null.</summary>
        Null,

        /// <summary>true / false.</summary>
        Boolean,

        /// <summary>Number (kept as source text for exact fixed-point conversion).</summary>
        Number,

        /// <summary>String.</summary>
        String,

        /// <summary>Array.</summary>
        Array,

        /// <summary>Object.</summary>
        Object,
    }

    /// <summary>
    /// Immutable JSON document node produced by <see cref="JsonReader"/>. Every node knows its JSONPath-style
    /// location so content errors can point designers at the exact field (e.g. <c>$.moves[3].hitboxes[0]</c>).
    /// Numbers keep their source text, so conversion to <see cref="Fixed"/> is exact and float-free.
    /// </summary>
    public sealed class JsonNode
    {
        private static readonly IReadOnlyList<JsonNode> EmptyItems = new JsonNode[0];

        private readonly string _text;
        private readonly bool _bool;
        private readonly List<JsonNode> _items;
        private readonly List<KeyValuePair<string, JsonNode>> _members;
        private readonly Dictionary<string, JsonNode> _lookup;

        private JsonNode(JsonKind kind, string path, string text, bool boolean, List<JsonNode> items, List<KeyValuePair<string, JsonNode>> members)
        {
            Kind = kind;
            Path = path;
            _text = text;
            _bool = boolean;
            _items = items;
            _members = members;
            if (members != null)
            {
                _lookup = new Dictionary<string, JsonNode>(members.Count, StringComparer.Ordinal);
                foreach (var member in members)
                {
                    _lookup[member.Key] = member.Value;
                }
            }
        }

        /// <summary>Value kind.</summary>
        public JsonKind Kind { get; }

        /// <summary>Location in the document, e.g. <c>$.moves[2].id</c>.</summary>
        public string Path { get; }

        internal static JsonNode CreateNull(string path) => new JsonNode(JsonKind.Null, path, null, false, null, null);

        internal static JsonNode CreateBoolean(string path, bool value) => new JsonNode(JsonKind.Boolean, path, null, value, null, null);

        internal static JsonNode CreateNumber(string path, string text) => new JsonNode(JsonKind.Number, path, text, false, null, null);

        internal static JsonNode CreateString(string path, string text) => new JsonNode(JsonKind.String, path, text, false, null, null);

        internal static JsonNode CreateArray(string path, List<JsonNode> items) => new JsonNode(JsonKind.Array, path, null, false, items, null);

        internal static JsonNode CreateObject(string path, List<KeyValuePair<string, JsonNode>> members) => new JsonNode(JsonKind.Object, path, null, false, null, members);

        /// <summary>Array items (empty for non-arrays).</summary>
        public IReadOnlyList<JsonNode> Items => _items ?? EmptyItems;

        /// <summary>Object members in document order (empty for non-objects).</summary>
        public IReadOnlyList<KeyValuePair<string, JsonNode>> Members => (IReadOnlyList<KeyValuePair<string, JsonNode>>)_members ?? new KeyValuePair<string, JsonNode>[0];

        /// <summary>Number of array items or object members.</summary>
        public int Count => _items?.Count ?? _members?.Count ?? 0;

        /// <summary>True when this object has a member with the given name.</summary>
        public bool Has(string name) => _lookup != null && _lookup.ContainsKey(name);

        /// <summary>Gets a member, or null when absent.</summary>
        public JsonNode Get(string name)
        {
            if (_lookup != null && _lookup.TryGetValue(name, out var node))
            {
                return node;
            }

            return null;
        }

        /// <summary>Gets a required member.</summary>
        /// <exception cref="JsonContentException">The node is not an object or the member is missing.</exception>
        public JsonNode Require(string name)
        {
            RequireKind(JsonKind.Object);
            var node = Get(name);
            if (node == null)
            {
                throw new JsonContentException(Path, $"missing required field '{name}'");
            }

            return node;
        }

        /// <summary>Returns this node as a string.</summary>
        public string AsString()
        {
            RequireKind(JsonKind.String);
            return _text;
        }

        /// <summary>Returns this node as a boolean.</summary>
        public bool AsBool()
        {
            RequireKind(JsonKind.Boolean);
            return _bool;
        }

        /// <summary>Returns this number as a 32-bit integer.</summary>
        public int AsInt()
        {
            RequireKind(JsonKind.Number);
            if (!int.TryParse(_text, System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out int value))
            {
                throw new JsonContentException(Path, $"expected an integer but found '{_text}'");
            }

            return value;
        }

        /// <summary>Returns this number as an exact fixed-point value.</summary>
        public Fixed AsFixed() => AsFixedScaled(1);

        /// <summary>Returns this number divided by <paramref name="divisor"/> as an exact fixed-point value.</summary>
        public Fixed AsFixedScaled(int divisor)
        {
            RequireKind(JsonKind.Number);
            try
            {
                return Fixed.ParseScaled(_text, divisor);
            }
            catch (Exception exception) when (exception is FormatException || exception is OverflowException)
            {
                throw new JsonContentException(Path, $"'{_text}' is not a valid fixed-point number");
            }
        }

        /// <summary>Raw source text of a number or string.</summary>
        public string RawText => _text;

        /// <summary>Optional string member with default.</summary>
        public string GetString(string name, string fallback)
        {
            var node = Get(name);
            return node == null || node.Kind == JsonKind.Null ? fallback : node.AsString();
        }

        /// <summary>Optional integer member with default.</summary>
        public int GetInt(string name, int fallback)
        {
            var node = Get(name);
            return node == null || node.Kind == JsonKind.Null ? fallback : node.AsInt();
        }

        /// <summary>Optional boolean member with default.</summary>
        public bool GetBool(string name, bool fallback)
        {
            var node = Get(name);
            return node == null || node.Kind == JsonKind.Null ? fallback : node.AsBool();
        }

        /// <summary>Optional fixed-point member with default.</summary>
        public Fixed GetFixed(string name, Fixed fallback)
        {
            var node = Get(name);
            return node == null || node.Kind == JsonKind.Null ? fallback : node.AsFixed();
        }

        /// <summary>Optional fixed-point member divided by <paramref name="divisor"/>, with default.</summary>
        public Fixed GetFixedScaled(string name, int divisor, Fixed fallback)
        {
            var node = Get(name);
            return node == null || node.Kind == JsonKind.Null ? fallback : node.AsFixedScaled(divisor);
        }

        /// <summary>Throws a content error when this node is not of the expected kind.</summary>
        public void RequireKind(JsonKind kind)
        {
            if (Kind != kind)
            {
                throw new JsonContentException(Path, $"expected {Describe(kind)} but found {Describe(Kind)}");
            }
        }

        private static string Describe(JsonKind kind)
        {
            switch (kind)
            {
                case JsonKind.Null: return "null";
                case JsonKind.Boolean: return "a boolean";
                case JsonKind.Number: return "a number";
                case JsonKind.String: return "a string";
                case JsonKind.Array: return "an array";
                default: return "an object";
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Oathsunder.Core.Serialization
{
    /// <summary>
    /// Strict RFC 8259 JSON parser with line/column diagnostics. Engine-independent (no UnityEngine,
    /// no System.Text.Json) so the same content pipeline runs in the Unity editor, player builds, dedicated
    /// servers and command-line tools.
    /// </summary>
    public static class JsonReader
    {
        private const int MaxDepth = 64;

        /// <summary>Parses a complete JSON document.</summary>
        /// <param name="text">Document text.</param>
        /// <param name="sourceName">Name used in error messages (usually the file name).</param>
        /// <exception cref="JsonSyntaxException">The document is not valid JSON.</exception>
        public static JsonNode Parse(string text, string sourceName = "<json>")
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var parser = new Parser(text, sourceName);
            parser.SkipWhitespace();
            JsonNode root = parser.ParseValue("$", 0);
            parser.SkipWhitespace();
            if (!parser.AtEnd)
            {
                throw parser.Error("unexpected trailing content");
            }

            return root;
        }

        private sealed class Parser
        {
            private readonly string _text;
            private readonly string _source;
            private readonly StringBuilder _builder = new StringBuilder(64);
            private int _position;

            public Parser(string text, string source)
            {
                // Tolerate a UTF-8 byte order mark written by some editors.
                _position = text.Length > 0 && text[0] == '﻿' ? 1 : 0;
                _text = text;
                _source = source;
            }

            public bool AtEnd => _position >= _text.Length;

            public JsonSyntaxException Error(string message)
            {
                int line = 1;
                int column = 1;
                for (int i = 0; i < _position && i < _text.Length; i++)
                {
                    if (_text[i] == '\n')
                    {
                        line++;
                        column = 1;
                    }
                    else
                    {
                        column++;
                    }
                }

                return new JsonSyntaxException(_source, line, column, message);
            }

            public void SkipWhitespace()
            {
                while (_position < _text.Length)
                {
                    char c = _text[_position];
                    if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
                    {
                        _position++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            public JsonNode ParseValue(string path, int depth)
            {
                if (depth > MaxDepth)
                {
                    throw Error("maximum nesting depth exceeded");
                }

                if (AtEnd)
                {
                    throw Error("unexpected end of document");
                }

                char c = _text[_position];
                switch (c)
                {
                    case '{':
                        return ParseObject(path, depth);
                    case '[':
                        return ParseArray(path, depth);
                    case '"':
                        return JsonNode.CreateString(path, ParseString());
                    case 't':
                        ExpectLiteral("true");
                        return JsonNode.CreateBoolean(path, true);
                    case 'f':
                        ExpectLiteral("false");
                        return JsonNode.CreateBoolean(path, false);
                    case 'n':
                        ExpectLiteral("null");
                        return JsonNode.CreateNull(path);
                    default:
                        if (c == '-' || (c >= '0' && c <= '9'))
                        {
                            return JsonNode.CreateNumber(path, ParseNumber());
                        }

                        throw Error($"unexpected character '{c}'");
                }
            }

            private JsonNode ParseObject(string path, int depth)
            {
                _position++; // {
                var members = new List<KeyValuePair<string, JsonNode>>();
                var seen = new HashSet<string>(StringComparer.Ordinal);
                SkipWhitespace();
                if (Peek() == '}')
                {
                    _position++;
                    return JsonNode.CreateObject(path, members);
                }

                while (true)
                {
                    SkipWhitespace();
                    if (Peek() != '"')
                    {
                        throw Error("expected a string key");
                    }

                    string key = ParseString();
                    if (!seen.Add(key))
                    {
                        throw Error($"duplicate key '{key}'");
                    }

                    SkipWhitespace();
                    Expect(':');
                    SkipWhitespace();
                    JsonNode value = ParseValue(path + "." + key, depth + 1);
                    members.Add(new KeyValuePair<string, JsonNode>(key, value));
                    SkipWhitespace();
                    char next = Peek();
                    if (next == ',')
                    {
                        _position++;
                        continue;
                    }

                    if (next == '}')
                    {
                        _position++;
                        return JsonNode.CreateObject(path, members);
                    }

                    throw Error("expected ',' or '}'");
                }
            }

            private JsonNode ParseArray(string path, int depth)
            {
                _position++; // [
                var items = new List<JsonNode>();
                SkipWhitespace();
                if (Peek() == ']')
                {
                    _position++;
                    return JsonNode.CreateArray(path, items);
                }

                while (true)
                {
                    SkipWhitespace();
                    items.Add(ParseValue(path + "[" + items.Count.ToString(CultureInfo.InvariantCulture) + "]", depth + 1));
                    SkipWhitespace();
                    char next = Peek();
                    if (next == ',')
                    {
                        _position++;
                        continue;
                    }

                    if (next == ']')
                    {
                        _position++;
                        return JsonNode.CreateArray(path, items);
                    }

                    throw Error("expected ',' or ']'");
                }
            }

            private string ParseString()
            {
                _position++; // opening quote
                _builder.Clear();
                while (true)
                {
                    if (AtEnd)
                    {
                        throw Error("unterminated string");
                    }

                    char c = _text[_position++];
                    if (c == '"')
                    {
                        return _builder.ToString();
                    }

                    if (c < 0x20)
                    {
                        throw Error("control character in string");
                    }

                    if (c != '\\')
                    {
                        _builder.Append(c);
                        continue;
                    }

                    if (AtEnd)
                    {
                        throw Error("unterminated escape sequence");
                    }

                    char escape = _text[_position++];
                    switch (escape)
                    {
                        case '"': _builder.Append('"'); break;
                        case '\\': _builder.Append('\\'); break;
                        case '/': _builder.Append('/'); break;
                        case 'b': _builder.Append('\b'); break;
                        case 'f': _builder.Append('\f'); break;
                        case 'n': _builder.Append('\n'); break;
                        case 'r': _builder.Append('\r'); break;
                        case 't': _builder.Append('\t'); break;
                        case 'u':
                            if (_position + 4 > _text.Length)
                            {
                                throw Error("truncated unicode escape");
                            }

                            if (!ushort.TryParse(_text.Substring(_position, 4), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out ushort code))
                            {
                                throw Error("invalid unicode escape");
                            }

                            _builder.Append((char)code);
                            _position += 4;
                            break;
                        default:
                            throw Error($"invalid escape '\\{escape}'");
                    }
                }
            }

            private string ParseNumber()
            {
                int start = _position;
                if (PeekOrEnd() == '-')
                {
                    _position++;
                }

                if (PeekOrEnd() == '0')
                {
                    _position++;
                }
                else if (IsDigit(PeekOrEnd()))
                {
                    while (IsDigit(PeekOrEnd()))
                    {
                        _position++;
                    }
                }
                else
                {
                    throw Error("invalid number");
                }

                if (PeekOrEnd() == '.')
                {
                    _position++;
                    if (!IsDigit(PeekOrEnd()))
                    {
                        throw Error("expected digits after decimal point");
                    }

                    while (IsDigit(PeekOrEnd()))
                    {
                        _position++;
                    }
                }

                char e = PeekOrEnd();
                if (e == 'e' || e == 'E')
                {
                    _position++;
                    char sign = PeekOrEnd();
                    if (sign == '+' || sign == '-')
                    {
                        _position++;
                    }

                    if (!IsDigit(PeekOrEnd()))
                    {
                        throw Error("expected exponent digits");
                    }

                    while (IsDigit(PeekOrEnd()))
                    {
                        _position++;
                    }
                }

                return _text.Substring(start, _position - start);
            }

            private void ExpectLiteral(string literal)
            {
                if (string.CompareOrdinal(_text, _position, literal, 0, literal.Length) != 0)
                {
                    throw Error($"expected '{literal}'");
                }

                _position += literal.Length;
            }

            private void Expect(char c)
            {
                if (Peek() != c)
                {
                    throw Error($"expected '{c}'");
                }

                _position++;
            }

            private char Peek()
            {
                if (_position >= _text.Length)
                {
                    throw Error("unexpected end of document");
                }

                return _text[_position];
            }

            private char PeekOrEnd() => _position < _text.Length ? _text[_position] : '\0';

            private static bool IsDigit(char c) => c >= '0' && c <= '9';
        }
    }
}

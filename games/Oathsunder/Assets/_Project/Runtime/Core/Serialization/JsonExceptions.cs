using System;

namespace Oathsunder.Core.Serialization
{
    /// <summary>Thrown when a document is not syntactically valid JSON.</summary>
    public sealed class JsonSyntaxException : Exception
    {
        /// <summary>Creates the exception.</summary>
        public JsonSyntaxException(string source, int line, int column, string message)
            : base($"{source}({line},{column}): {message}")
        {
            Source = source;
            Line = line;
            Column = column;
        }

        /// <summary>1-based line of the error.</summary>
        public int Line { get; }

        /// <summary>1-based column of the error.</summary>
        public int Column { get; }
    }

    /// <summary>Thrown when valid JSON does not match the expected content schema.</summary>
    public sealed class JsonContentException : Exception
    {
        /// <summary>Creates the exception.</summary>
        public JsonContentException(string path, string message)
            : base($"{path}: {message}")
        {
            JsonPath = path;
        }

        /// <summary>Location of the offending value.</summary>
        public string JsonPath { get; }
    }
}

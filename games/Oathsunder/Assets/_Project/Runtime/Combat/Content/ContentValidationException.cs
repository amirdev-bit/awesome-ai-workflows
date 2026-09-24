using System;
using System.Collections.Generic;

namespace Oathsunder.Combat.Content
{
    /// <summary>Thrown when combat content fails validation. Lists every problem found, not just the first.</summary>
    public sealed class ContentValidationException : Exception
    {
        /// <summary>Creates the exception.</summary>
        public ContentValidationException(string subject, IReadOnlyList<string> errors)
            : base(Format(subject, errors))
        {
            Subject = subject;
            Errors = errors;
        }

        /// <summary>What was being validated (fighter id, file name).</summary>
        public string Subject { get; }

        /// <summary>Every validation error.</summary>
        public IReadOnlyList<string> Errors { get; }

        private static string Format(string subject, IReadOnlyList<string> errors)
        {
            var text = new System.Text.StringBuilder();
            text.Append("Combat content '").Append(subject).Append("' is invalid (").Append(errors.Count).Append(" error(s)):");
            foreach (var error in errors)
            {
                text.Append("\n  - ").Append(error);
            }

            return text.ToString();
        }
    }
}

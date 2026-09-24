using System;

namespace Oathsunder.Combat.Input
{
    /// <summary>
    /// Facing-relative direction in fighting-game numpad notation:
    /// <code>
    /// 7 8 9      up-back   up   up-forward
    /// 4 5 6  =   back    neutral  forward
    /// 1 2 3      down-back down down-forward
    /// </code>
    /// </summary>
    public enum NumpadDirection : byte
    {
        /// <summary>Invalid / unknown.</summary>
        None = 0,

        /// <summary>1 — down-back.</summary>
        DownBack = 1,

        /// <summary>2 — down.</summary>
        Down = 2,

        /// <summary>3 — down-forward.</summary>
        DownForward = 3,

        /// <summary>4 — back.</summary>
        Back = 4,

        /// <summary>5 — neutral.</summary>
        Neutral = 5,

        /// <summary>6 — forward.</summary>
        Forward = 6,

        /// <summary>7 — up-back.</summary>
        UpBack = 7,

        /// <summary>8 — up.</summary>
        Up = 8,

        /// <summary>9 — up-forward.</summary>
        UpForward = 9,
    }

    /// <summary>Set of numpad directions. Bit <c>n</c> represents numpad direction <c>n</c>.</summary>
    [Flags]
    public enum DirectionMask : ushort
    {
        /// <summary>Matches nothing.</summary>
        None = 0,

        /// <summary>1.</summary>
        DownBack = 1 << 1,

        /// <summary>2.</summary>
        Down = 1 << 2,

        /// <summary>3.</summary>
        DownForward = 1 << 3,

        /// <summary>4.</summary>
        Back = 1 << 4,

        /// <summary>5.</summary>
        Neutral = 1 << 5,

        /// <summary>6.</summary>
        Forward = 1 << 6,

        /// <summary>7.</summary>
        UpBack = 1 << 7,

        /// <summary>8.</summary>
        Up = 1 << 8,

        /// <summary>9.</summary>
        UpForward = 1 << 9,

        /// <summary>1, 2, 3.</summary>
        AnyDown = DownBack | Down | DownForward,

        /// <summary>7, 8, 9.</summary>
        AnyUp = UpBack | Up | UpForward,

        /// <summary>3, 6, 9.</summary>
        AnyForward = DownForward | Forward | UpForward,

        /// <summary>1, 4, 7.</summary>
        AnyBack = DownBack | Back | UpBack,

        /// <summary>4, 5, 6, 7, 8, 9.</summary>
        NotDown = Back | Neutral | Forward | UpBack | Up | UpForward,

        /// <summary>Every direction.</summary>
        Any = AnyDown | Back | Neutral | Forward | AnyUp,
    }

    /// <summary>Helpers for directions.</summary>
    public static class DirectionUtility
    {
        /// <summary>Converts absolute stick axes (-1..1) into a facing-relative numpad direction.</summary>
        public static NumpadDirection ToNumpad(int x, int y, int facingSign)
        {
            int relativeX = Math.Sign(x) * (facingSign >= 0 ? 1 : -1);
            int vertical = Math.Sign(y);
            return (NumpadDirection)(5 + relativeX + 3 * vertical);
        }

        /// <summary>Mask bit for a single direction.</summary>
        public static DirectionMask ToMask(this NumpadDirection direction) =>
            direction == NumpadDirection.None ? DirectionMask.None : (DirectionMask)(1 << (int)direction);

        /// <summary>True when <paramref name="direction"/> is in <paramref name="mask"/>.</summary>
        public static bool Matches(this DirectionMask mask, NumpadDirection direction) => (mask & direction.ToMask()) != 0;

        /// <summary>
        /// Parses numpad notation such as <c>"6"</c>, <c>"123"</c>, <c>"down"</c>, <c>"any"</c>, <c>"notdown"</c>.
        /// Digits may be combined ("369" = any forward).
        /// </summary>
        /// <exception cref="FormatException">The text is not a recognised direction set.</exception>
        public static DirectionMask ParseMask(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new FormatException("Empty direction.");
            }

            switch (text.ToLowerInvariant())
            {
                case "any": return DirectionMask.Any;
                case "down": return DirectionMask.AnyDown;
                case "up": return DirectionMask.AnyUp;
                case "forward": return DirectionMask.AnyForward;
                case "back": return DirectionMask.AnyBack;
                case "notdown": return DirectionMask.NotDown;
                case "neutral": return DirectionMask.Neutral;
            }

            DirectionMask mask = DirectionMask.None;
            foreach (char c in text)
            {
                if (c < '1' || c > '9')
                {
                    throw new FormatException($"'{text}' is not a numpad direction set.");
                }

                mask |= (DirectionMask)(1 << (c - '0'));
            }

            return mask;
        }
    }
}

using System;

namespace Oathsunder.Presentation.Cues
{
    /// <summary>
    /// The presentation channel a cue drives, taken from the first segment of its name
    /// (<c>sfx.katana.swing.light</c> is <see cref="Sfx"/>).
    /// </summary>
    public enum CueChannel : byte
    {
        /// <summary>One-shot sound effect (weapons, movement, modes).</summary>
        Sfx,

        /// <summary>Visual effect prefab (trails, slashes, dust, auras).</summary>
        Vfx,

        /// <summary>Camera impulse, push, tilt, zoom, slow motion or authored sequence.</summary>
        Cam,

        /// <summary>Voice line resolved per character (<c>{Character}</c> in the asset name).</summary>
        Vo,

        /// <summary>Body and cloth foley.</summary>
        Foley,

        /// <summary>Controller rumble or phone haptics.</summary>
        Haptic,

        /// <summary>Composite cue fired by a combat event (hit, block, parry…) that plays several layers.</summary>
        Impact,
    }

    /// <summary>Cue name grammar: <c>channel.segment[.segment…]</c>, lowercase ASCII letters and digits per segment.</summary>
    public static class CueNames
    {
        /// <summary>Parses the channel of a cue name.</summary>
        /// <returns>False when the name does not follow the grammar or names an unknown channel.</returns>
        public static bool TryParse(string name, out CueChannel channel)
        {
            channel = default;
            if (!IsWellFormed(name))
            {
                return false;
            }

            int dot = name.IndexOf('.');
            switch (name.Substring(0, dot))
            {
                case "sfx": channel = CueChannel.Sfx; return true;
                case "vfx": channel = CueChannel.Vfx; return true;
                case "cam": channel = CueChannel.Cam; return true;
                case "vo": channel = CueChannel.Vo; return true;
                case "foley": channel = CueChannel.Foley; return true;
                case "haptic": channel = CueChannel.Haptic; return true;
                case "impact": channel = CueChannel.Impact; return true;
                default: return false;
            }
        }

        /// <summary>True when the name has at least two non-empty segments of <c>[a-z0-9]</c>.</summary>
        public static bool IsWellFormed(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            int segments = 1;
            int segmentLength = 0;
            foreach (char c in name)
            {
                if (c == '.')
                {
                    if (segmentLength == 0)
                    {
                        return false;
                    }

                    segments++;
                    segmentLength = 0;
                    continue;
                }

                if (!((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')))
                {
                    return false;
                }

                segmentLength++;
            }

            return segments >= 2 && segmentLength > 0;
        }

        /// <summary>Channel of a cue name.</summary>
        /// <exception cref="ArgumentException">The name is not a valid cue name.</exception>
        public static CueChannel ChannelOf(string name)
        {
            if (!TryParse(name, out var channel))
            {
                throw new ArgumentException($"'{name}' is not a valid cue name (expected sfx|vfx|cam|vo|foley|haptic|impact followed by dot-separated [a-z0-9] segments)", nameof(name));
            }

            return channel;
        }

        /// <summary>Lowercase prefix used in names for a channel.</summary>
        public static string Prefix(CueChannel channel)
        {
            switch (channel)
            {
                case CueChannel.Sfx: return "sfx";
                case CueChannel.Vfx: return "vfx";
                case CueChannel.Cam: return "cam";
                case CueChannel.Vo: return "vo";
                case CueChannel.Foley: return "foley";
                case CueChannel.Haptic: return "haptic";
                default: return "impact";
            }
        }
    }
}

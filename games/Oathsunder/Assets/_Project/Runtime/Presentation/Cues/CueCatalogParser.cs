using System;
using System.Collections.Generic;
using System.Globalization;
using Oathsunder.Combat.Content;
using Oathsunder.Core.Serialization;

namespace Oathsunder.Presentation.Cues
{
    /// <summary>
    /// Parses and validates a cue catalog document (<c>Content/Presentation/cues.*.json</c>). Schema errors throw
    /// <see cref="JsonContentException"/> with the JSON path; production-rule violations are collected and thrown
    /// together as one <see cref="ContentValidationException"/>. See Documentation/Production/03-CueCatalog.md.
    /// </summary>
    public static class CueCatalogParser
    {
        /// <summary>Mixer buses a cue may route to.</summary>
        public static readonly IReadOnlyList<string> Buses = new[]
        {
            "sfx.weapons", "sfx.impacts", "sfx.movement", "sfx.modes", "sfx.cinematic", "sfx.ui", "foley", "vo.combat", "vo.announcer",
        };

        /// <summary>Minimum recorded variations for sound effects and foley (Audio README rule).</summary>
        public const int MinSfxVariations = 4;

        /// <summary>Minimum recorded variations for voice cues.</summary>
        public const int MinVoVariations = 3;

        /// <summary>Particle ceiling on <see cref="QualityTier.MobileLow"/> (VFX README rule).</summary>
        public const int MobileLowParticleLimit = 32;

        /// <summary>Peak opacity allowed for a flash (photosensitivity guideline).</summary>
        public const float MaxFlashOpacity = 0.6f;

        private static readonly string[] CommonFields = { "name", "desc", "asset" };
        private static readonly string[] AudioFields = { "variations", "bus", "volume", "pitchJitter", "chance" };
        private static readonly string[] VfxFields = { "attach", "lifetime", "particles", "distortion", "light", "gore" };
        private static readonly string[] CameraFields = { "kind", "amplitude", "frequency", "frames", "timeScale", "sequence" };
        private static readonly string[] HapticFields = { "amplitude", "ms" };
        private static readonly string[] ImpactFields = { "layers" };

        /// <summary>Parses a catalog document.</summary>
        /// <exception cref="JsonSyntaxException">The text is not valid JSON.</exception>
        /// <exception cref="JsonContentException">The document does not match the schema.</exception>
        /// <exception cref="ContentValidationException">One or more cues break a production rule.</exception>
        public static CueCatalog Parse(string json, string sourceName) => Parse(JsonReader.Parse(json, sourceName), sourceName);

        /// <summary>Parses a catalog node.</summary>
        public static CueCatalog Parse(JsonNode root, string sourceName)
        {
            root.RequireKind(JsonKind.Object);
            var catalog = new CueCatalog(root.Require("id").AsString());
            var errors = new List<string>();
            var cues = root.Require("cues");
            cues.RequireKind(JsonKind.Array);
            foreach (var node in cues.Items)
            {
                var cue = ParseCue(node, errors);
                if (cue != null && !catalog.Add(cue))
                {
                    errors.Add($"{node.Path}: duplicate cue '{cue.Name}'");
                }
            }

            foreach (var cue in catalog.Cues)
            {
                if (cue.Channel != CueChannel.Impact)
                {
                    continue;
                }

                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (var layer in cue.Layers)
                {
                    if (!seen.Add(layer))
                    {
                        errors.Add($"{cue.Name}: layer '{layer}' is listed twice");
                    }
                    else if (!catalog.TryGet(layer, out var target))
                    {
                        errors.Add($"{cue.Name}: layer '{layer}' is not in the catalog");
                    }
                    else if (target.Channel == CueChannel.Impact)
                    {
                        errors.Add($"{cue.Name}: layer '{layer}' is an impact cue (impact cues cannot nest)");
                    }
                }
            }

            if (errors.Count > 0)
            {
                throw new ContentValidationException(sourceName ?? catalog.Id, errors);
            }

            return catalog;
        }

        private static CueDefinition ParseCue(JsonNode node, List<string> errors)
        {
            node.RequireKind(JsonKind.Object);
            string name = node.Require("name").AsString();
            if (!CueNames.TryParse(name, out var channel))
            {
                errors.Add($"{node.Path}: '{name}' is not a valid cue name");
                return null;
            }

            var cue = new CueDefinition
            {
                Name = name,
                Channel = channel,
                Description = node.Require("desc").AsString(),
                Asset = node.GetString("asset", ""),
            };

            if (cue.Description.Trim().Length < 12)
            {
                errors.Add($"{name}: 'desc' must describe what to produce (at least 12 characters)");
            }

            switch (channel)
            {
                case CueChannel.Sfx:
                case CueChannel.Foley:
                case CueChannel.Vo:
                    CheckFields(node, errors, AudioFields);
                    cue.Audio = ParseAudio(node, cue, errors);
                    break;
                case CueChannel.Vfx:
                    CheckFields(node, errors, VfxFields);
                    cue.Vfx = ParseVfx(node, cue, errors);
                    break;
                case CueChannel.Cam:
                    CheckFields(node, errors, CameraFields);
                    cue.Camera = ParseCamera(node, cue, errors);
                    break;
                case CueChannel.Haptic:
                    CheckFields(node, errors, HapticFields);
                    cue.Haptic = new HapticCueParams
                    {
                        Amplitude = Float(node.Require("amplitude")),
                        DurationMs = node.Require("ms").AsInt(),
                    };
                    if (cue.Haptic.Amplitude <= 0f || cue.Haptic.Amplitude > 1f)
                    {
                        errors.Add($"{name}: haptic amplitude must be in (0, 1]");
                    }

                    if (cue.Haptic.DurationMs < 1 || cue.Haptic.DurationMs > 500)
                    {
                        errors.Add($"{name}: haptic duration must be 1–500 ms");
                    }

                    break;
                default:
                    CheckFields(node, errors, ImpactFields);
                    var layers = node.Require("layers");
                    layers.RequireKind(JsonKind.Array);
                    cue.Layers = new string[layers.Count];
                    for (int i = 0; i < layers.Count; i++)
                    {
                        cue.Layers[i] = layers.Items[i].AsString();
                    }

                    if (cue.Layers.Length == 0)
                    {
                        errors.Add($"{name}: an impact cue needs at least one layer");
                    }

                    break;
            }

            RequireAsset(cue, errors);
            return cue;
        }

        private static AudioCueParams ParseAudio(JsonNode node, CueDefinition cue, List<string> errors)
        {
            var audio = new AudioCueParams
            {
                Variations = node.Require("variations").AsInt(),
                Bus = node.Require("bus").AsString(),
                VolumeDb = node.Has("volume") ? Float(node.Get("volume")) : 0f,
                PitchJitterCents = node.GetInt("pitchJitter", 0),
                ChancePercent = node.GetInt("chance", 100),
            };

            int minimum = cue.Channel == CueChannel.Vo ? MinVoVariations : MinSfxVariations;
            if (audio.Variations < minimum)
            {
                errors.Add($"{cue.Name}: needs at least {minimum} variations (has {audio.Variations})");
            }

            if (!Contains(Buses, audio.Bus))
            {
                errors.Add($"{cue.Name}: unknown bus '{audio.Bus}'");
            }

            if (audio.VolumeDb > 0f || audio.VolumeDb < -48f)
            {
                errors.Add($"{cue.Name}: volume must be between -48 and 0 dB");
            }

            if (audio.PitchJitterCents < 0 || audio.PitchJitterCents > 300)
            {
                errors.Add($"{cue.Name}: pitchJitter must be 0–300 cents");
            }

            if (audio.ChancePercent < 1 || audio.ChancePercent > 100)
            {
                errors.Add($"{cue.Name}: chance must be 1–100");
            }

            return audio;
        }

        private static VfxCueParams ParseVfx(JsonNode node, CueDefinition cue, List<string> errors)
        {
            var vfx = new VfxCueParams
            {
                Attach = ParseEnum<VfxAttach>(node.Require("attach")),
                LifetimeFrames = node.Require("lifetime").AsInt(),
                Distortion = node.GetBool("distortion", false),
                Light = node.GetBool("light", false),
                Gore = node.GetBool("gore", false),
            };

            var particles = node.Require("particles");
            particles.RequireKind(JsonKind.Array);
            if (particles.Count != 4)
            {
                throw new JsonContentException(particles.Path, "expected 4 particle budgets [mobileLow, mobileMid, mobileHigh, pc]");
            }

            for (int i = 0; i < 4; i++)
            {
                vfx.Particles[i] = particles.Items[i].AsInt();
                if (vfx.Particles[i] < 0 || (i > 0 && vfx.Particles[i] < vfx.Particles[i - 1]))
                {
                    errors.Add($"{cue.Name}: particle budgets must be non-negative and must not decrease with tier");
                    break;
                }
            }

            if (vfx.Particles[(int)QualityTier.MobileLow] > MobileLowParticleLimit)
            {
                errors.Add($"{cue.Name}: mobile-low budget {vfx.Particles[0]} exceeds {MobileLowParticleLimit} particles");
            }

            if (vfx.LifetimeFrames < 1 || vfx.LifetimeFrames > 600)
            {
                errors.Add($"{cue.Name}: lifetime must be 1–600 frames");
            }

            return vfx;
        }

        private static CameraCueParams ParseCamera(JsonNode node, CueDefinition cue, List<string> errors)
        {
            var camera = new CameraCueParams
            {
                Kind = ParseEnum<CameraCueKind>(node.Require("kind")),
                Amplitude = node.Has("amplitude") ? Float(node.Get("amplitude")) : 0f,
                Frequency = node.Has("frequency") ? Float(node.Get("frequency")) : 0f,
                Frames = node.Require("frames").AsInt(),
                TimeScale = node.Has("timeScale") ? Float(node.Get("timeScale")) : 1f,
                Sequence = node.GetString("sequence", ""),
            };

            if (camera.Frames < 1 || camera.Frames > 600)
            {
                errors.Add($"{cue.Name}: frames must be 1–600");
            }

            switch (camera.Kind)
            {
                case CameraCueKind.Shake:
                    if (camera.Amplitude <= 0f || camera.Amplitude > 0.5f)
                    {
                        errors.Add($"{cue.Name}: shake amplitude must be in (0, 0.5] m");
                    }

                    if (camera.Frequency <= 0f || camera.Frequency > 60f)
                    {
                        errors.Add($"{cue.Name}: shake frequency must be in (0, 60] Hz");
                    }

                    break;
                case CameraCueKind.Push:
                    if (camera.Amplitude <= 0f || camera.Amplitude > 0.5f)
                    {
                        errors.Add($"{cue.Name}: push amplitude must be a fraction of distance in (0, 0.5]");
                    }

                    break;
                case CameraCueKind.Tilt:
                case CameraCueKind.Zoom:
                    if (camera.Amplitude == 0f || Math.Abs(camera.Amplitude) > 20f)
                    {
                        errors.Add($"{cue.Name}: {camera.Kind} amplitude must be non-zero and at most 20 degrees");
                    }

                    break;
                case CameraCueKind.SlowMotion:
                    if (camera.TimeScale <= 0f || camera.TimeScale >= 1f)
                    {
                        errors.Add($"{cue.Name}: slow motion timeScale must be in (0, 1)");
                    }

                    break;
                case CameraCueKind.Flash:
                    if (camera.Amplitude <= 0f || camera.Amplitude > MaxFlashOpacity)
                    {
                        errors.Add($"{cue.Name}: flash opacity must be in (0, {MaxFlashOpacity.ToString(CultureInfo.InvariantCulture)}] (photosensitivity)");
                    }

                    if (camera.Frames > 12)
                    {
                        errors.Add($"{cue.Name}: a flash may last at most 12 frames");
                    }

                    break;
                case CameraCueKind.Sequence:
                    if (!camera.Sequence.StartsWith("TL_", StringComparison.Ordinal))
                    {
                        errors.Add($"{cue.Name}: a sequence cue needs a 'sequence' Timeline asset with the TL_ prefix");
                    }

                    break;
            }

            return camera;
        }

        private static void RequireAsset(CueDefinition cue, List<string> errors)
        {
            string prefix;
            switch (cue.Channel)
            {
                case CueChannel.Sfx:
                case CueChannel.Foley:
                    prefix = "SFX_";
                    break;
                case CueChannel.Vfx:
                    prefix = "VFX_";
                    break;
                case CueChannel.Vo:
                    prefix = "VO_";
                    break;
                default:
                    if (cue.Asset.Length > 0)
                    {
                        errors.Add($"{cue.Name}: {CueNames.Prefix(cue.Channel)} cues have no asset");
                    }

                    return;
            }

            if (!cue.Asset.StartsWith(prefix, StringComparison.Ordinal))
            {
                errors.Add($"{cue.Name}: asset '{cue.Asset}' must start with {prefix}");
            }

            bool announcer = cue.Name.StartsWith("vo.announcer.", StringComparison.Ordinal);
            if (cue.Channel == CueChannel.Vo && !announcer && cue.Asset.IndexOf("{Character}", StringComparison.Ordinal) < 0)
            {
                errors.Add($"{cue.Name}: character voice assets must contain {{Character}}");
            }
        }

        private static void CheckFields(JsonNode node, List<string> errors, string[] allowed)
        {
            foreach (var member in node.Members)
            {
                if (!Contains(CommonFields, member.Key) && !Contains(allowed, member.Key))
                {
                    errors.Add($"{node.Path}: unknown field '{member.Key}'");
                }
            }
        }

        private static bool Contains(IReadOnlyList<string> values, string value)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (string.Equals(values[i], value, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static float Float(JsonNode node)
        {
            node.RequireKind(JsonKind.Number);
            if (!double.TryParse(node.RawText, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                throw new JsonContentException(node.Path, $"'{node.RawText}' is not a number");
            }

            return (float)value;
        }

        private static T ParseEnum<T>(JsonNode node)
            where T : struct
        {
            string text = node.AsString();
            if (!Enum.TryParse(text, false, out T value) || !Enum.IsDefined(typeof(T), value) || char.IsDigit(text[0]))
            {
                throw new JsonContentException(node.Path, $"'{text}' is not one of: {string.Join(", ", Enum.GetNames(typeof(T)))}");
            }

            return value;
        }
    }
}

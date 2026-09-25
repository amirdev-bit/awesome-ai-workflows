namespace Oathsunder.Presentation.Cues
{
    /// <summary>Rendering quality tiers (Phase 15). Index order is used for per-tier arrays.</summary>
    public enum QualityTier : byte
    {
        /// <summary>Android low / older iOS, 60 FPS target.</summary>
        MobileLow,

        /// <summary>Mid-range phones, 90 FPS target.</summary>
        MobileMid,

        /// <summary>Flagship phones and tablets, 120 FPS target.</summary>
        MobileHigh,

        /// <summary>PC and consoles, up to 240 FPS.</summary>
        Pc,
    }

    /// <summary>Where a visual effect is spawned.</summary>
    public enum VfxAttach : byte
    {
        /// <summary>Fighter root on the floor (dust, auras that follow the body).</summary>
        Root,

        /// <summary>Along the weapon blade from <c>weapon_r</c> to <c>weapon_tip</c> (trails).</summary>
        Weapon,

        /// <summary>At the weapon tip socket (thrust glints, projectile launch).</summary>
        WeaponTip,

        /// <summary>Left hand socket.</summary>
        HandL,

        /// <summary>Right hand socket.</summary>
        HandR,

        /// <summary>Chest socket (mode auras, charge glow).</summary>
        Chest,

        /// <summary>The contact point reported by the combat event.</summary>
        Hit,

        /// <summary>A world position, not following anything.</summary>
        World,
    }

    /// <summary>Kind of camera cue.</summary>
    public enum CameraCueKind : byte
    {
        /// <summary>Positional noise: amplitude in metres, frequency in Hz.</summary>
        Shake,

        /// <summary>Dolly toward the actor: amplitude is the fraction of camera distance removed.</summary>
        Push,

        /// <summary>Pitch the camera: amplitude in degrees (positive looks up).</summary>
        Tilt,

        /// <summary>Change the field of view: amplitude in degrees (negative zooms in).</summary>
        Zoom,

        /// <summary>Presentation time scale (runner pacing only, never simulation content).</summary>
        SlowMotion,

        /// <summary>Full-screen flash: amplitude is the peak white opacity.</summary>
        Flash,

        /// <summary>Authored camera sequence (Timeline asset, <c>TL_</c> prefix).</summary>
        Sequence,
    }

    /// <summary>Parameters of an audio cue (sfx, foley, vo).</summary>
    public sealed class AudioCueParams
    {
        /// <summary>Number of recorded variations (random container, no immediate repeats).</summary>
        public int Variations;

        /// <summary>Mixer bus, e.g. <c>sfx.weapons</c>.</summary>
        public string Bus = "";

        /// <summary>Playback gain in dBFS relative to the bus (≤ 0).</summary>
        public float VolumeDb;

        /// <summary>Random pitch range (± cents).</summary>
        public int PitchJitterCents;

        /// <summary>Probability of playing, 1–100 (voice efforts use &lt; 100 to avoid fatigue).</summary>
        public int ChancePercent = 100;
    }

    /// <summary>Parameters of a visual effect cue.</summary>
    public sealed class VfxCueParams
    {
        /// <summary>Spawn location.</summary>
        public VfxAttach Attach;

        /// <summary>Frames before the pooled instance is recycled.</summary>
        public int LifetimeFrames;

        /// <summary>Maximum live particles per <see cref="QualityTier"/> (index = tier).</summary>
        public int[] Particles = new int[4];

        /// <summary>Uses screen-space distortion (enabled on MobileHigh and Pc only).</summary>
        public bool Distortion;

        /// <summary>Spawns a realtime light (Pc only).</summary>
        public bool Light;

        /// <summary>Blood or gore: not spawned when the player disables blood.</summary>
        public bool Gore;
    }

    /// <summary>Parameters of a camera cue.</summary>
    public sealed class CameraCueParams
    {
        /// <summary>Kind.</summary>
        public CameraCueKind Kind;

        /// <summary>Kind-specific amplitude (see <see cref="CameraCueKind"/>).</summary>
        public float Amplitude;

        /// <summary>Shake frequency in Hz.</summary>
        public float Frequency;

        /// <summary>Duration in 60 Hz frames.</summary>
        public int Frames;

        /// <summary>Time scale for <see cref="CameraCueKind.SlowMotion"/> (0–1).</summary>
        public float TimeScale = 1f;

        /// <summary>Timeline asset for <see cref="CameraCueKind.Sequence"/>.</summary>
        public string Sequence = "";
    }

    /// <summary>Parameters of a haptic cue.</summary>
    public sealed class HapticCueParams
    {
        /// <summary>Intensity 0–1.</summary>
        public float Amplitude;

        /// <summary>Duration in milliseconds.</summary>
        public int DurationMs;
    }

    /// <summary>
    /// One entry of the cue catalog: the production specification of a presentation cue. Only the parameter
    /// block matching <see cref="Channel"/> is set.
    /// </summary>
    public sealed class CueDefinition
    {
        /// <summary>Cue name, e.g. <c>vfx.trail.katana.light</c>.</summary>
        public string Name = "";

        /// <summary>Channel parsed from the name.</summary>
        public CueChannel Channel;

        /// <summary>What the artist or sound designer must make, in one or two sentences.</summary>
        public string Description = "";

        /// <summary>Asset name (<c>SFX_</c>, <c>VFX_</c>, <c>VO_</c> prefixes); may contain <c>{Character}</c> or <c>{Weapon}</c>.</summary>
        public string Asset = "";

        /// <summary>Audio parameters (sfx, foley, vo).</summary>
        public AudioCueParams Audio;

        /// <summary>Visual effect parameters.</summary>
        public VfxCueParams Vfx;

        /// <summary>Camera parameters.</summary>
        public CameraCueParams Camera;

        /// <summary>Haptic parameters.</summary>
        public HapticCueParams Haptic;

        /// <summary>Layers of an impact cue (names of non-impact cues).</summary>
        public string[] Layers = new string[0];
    }
}

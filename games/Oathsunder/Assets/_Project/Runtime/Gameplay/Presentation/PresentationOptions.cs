using System;
using Oathsunder.Presentation.Cues;
using UnityEngine;

namespace Oathsunder.Gameplay.Presentation
{
    /// <summary>
    /// Player-facing presentation settings (Options → Display / Accessibility, Phase 12). They change what is
    /// shown and heard, never the simulation, so they are safe to differ between online peers.
    /// </summary>
    [Serializable]
    public sealed class PresentationOptions
    {
        [Tooltip("Ink-blood sprays and decals. Off removes every gore-flagged VFX layer; gameplay feedback is unchanged.")]
        public bool BloodEnabled = true;

        [Tooltip("Scales every camera shake, push, tilt and zoom (0 = no camera motion; accessibility).")]
        [Range(0f, 1f)] public float CameraMotionScale = 1f;

        [Tooltip("Full-screen flashes (ultimate activation). Off replaces them with nothing (photosensitivity).")]
        public bool FlashesEnabled = true;

        [Tooltip("Controller rumble and phone haptics.")]
        public bool HapticsEnabled = true;

        [Tooltip("Rendering tier used to pick VFX variants and particle budgets.")]
        public QualityTier Tier = QualityTier.Pc;

        /// <summary>Raised after a setting changes so listeners can re-apply.</summary>
        public event Action Changed;

        /// <summary>Call after editing fields from the options screen.</summary>
        public void NotifyChanged() => Changed?.Invoke();
    }
}

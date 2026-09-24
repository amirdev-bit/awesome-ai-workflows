using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oathsunder.Gameplay.Combat
{
    /// <summary>What a presentation cue does.</summary>
    [Serializable]
    public sealed class CombatCueEntry
    {
        [Tooltip("Cue name from move data, e.g. sfx.katana.swing.light, vfx.trail.katana.heavy, cam.shake.heavy")]
        public string Cue = "";

        [Tooltip("Optional pooled VFX prefab spawned at the fighter or contact point.")]
        public GameObject VfxPrefab;

        [Tooltip("Optional sound. Phase 13 routes this through the adaptive audio system.")]
        public AudioClip Sound;

        [Range(0f, 1f)] public float Volume = 1f;

        [Tooltip("Camera shake amplitude in metres (0 = none).")]
        [Min(0f)] public float CameraShake;

        [Tooltip("Haptic intensity on supported devices (0 = none).")]
        [Range(0f, 1f)] public float Haptics;

        [Tooltip("Seconds before a pooled VFX instance is recycled.")]
        [Min(0.05f)] public float Lifetime = 1.5f;
    }

    /// <summary>
    /// Maps data cue names (and synthesised impact cues such as <c>impact.hit.heavy</c>) to presentation assets.
    /// Keeping this out of move data lets art and audio iterate without touching frame data.
    /// </summary>
    [CreateAssetMenu(fileName = "DA_CombatCues", menuName = "Oathsunder/Combat/Cue Library", order = 2)]
    public sealed class CombatCueLibrary : ScriptableObject
    {
        [SerializeField] private List<CombatCueEntry> _entries = new List<CombatCueEntry>();

        private Dictionary<string, CombatCueEntry> _lookup;

        /// <summary>Finds a cue; returns false for unknown cues (they are simply not played).</summary>
        public bool TryGet(string cue, out CombatCueEntry entry)
        {
            if (_lookup == null)
            {
                _lookup = new Dictionary<string, CombatCueEntry>(StringComparer.Ordinal);
                foreach (var e in _entries)
                {
                    if (e != null && !string.IsNullOrEmpty(e.Cue))
                    {
                        _lookup[e.Cue] = e;
                    }
                }
            }

            return _lookup.TryGetValue(cue, out entry);
        }

#if UNITY_EDITOR
        private void OnValidate() => _lookup = null;
#endif
    }
}

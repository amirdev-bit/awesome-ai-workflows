using System;
using System.Collections.Generic;
using Oathsunder.Presentation.Cues;
using UnityEngine;

namespace Oathsunder.Gameplay.Presentation
{
    /// <summary>
    /// Binds one catalog cue to engine assets. <see cref="Variant"/> selects per-character voice banks
    /// (<c>{Character}</c>, e.g. <c>rhen</c>) and per-weapon impact families (<c>{Weapon}</c>, e.g. <c>katana</c>);
    /// an empty variant is the fallback used when no specific binding exists.
    /// </summary>
    [Serializable]
    public sealed class CombatCueBinding
    {
        [Tooltip("Catalog cue name, e.g. sfx.katana.swing.light.")]
        public string Cue = "";

        [Tooltip("Character id (rhen) for {Character} assets, weapon class (katana) for {Weapon} assets, empty for the default.")]
        public string Variant = "";

        [Tooltip("VFX prefab for MobileMid and above.")]
        public GameObject Vfx;

        [Tooltip("Optional cheaper prefab for MobileLow (≤ 32 particles, no lights, no distortion). Falls back to Vfx.")]
        public GameObject VfxMobileLow;

        [Tooltip("Recorded variations for sfx, foley and vo cues.")]
        public AudioClip[] Clips = new AudioClip[0];

        [Tooltip("Prefab with a PlayableDirector for camera Sequence cues (TL_ assets).")]
        public GameObject Sequence;
    }

    /// <summary>
    /// The engine side of the cue catalog: the catalog JSON (production parameters) plus asset bindings.
    /// Parameters live in the catalog so sound and VFX designers tune one file that CI validates; this asset only
    /// says which prefab or clip realises each cue. "Oathsunder/Presentation/Sync Cue Library" adds a binding row
    /// for every catalog cue and reports the ones still missing assets.
    /// </summary>
    [CreateAssetMenu(fileName = "DA_CombatCues", menuName = "Oathsunder/Presentation/Cue Library", order = 2)]
    public sealed class CombatCueLibrary : ScriptableObject
    {
        [SerializeField] private TextAsset _catalog;
        [SerializeField] private List<CombatCueBinding> _bindings = new List<CombatCueBinding>();

        private CueCatalog _parsed;
        private bool _parseFailed;
        private Dictionary<string, CombatCueBinding> _lookup;

        /// <summary>The parsed catalog (null when no catalog is assigned or it is invalid; errors are logged once).</summary>
        public CueCatalog Catalog
        {
            get
            {
                if (_parsed == null && _catalog != null && !_parseFailed)
                {
                    try
                    {
                        _parsed = CueCatalogParser.Parse(_catalog.text, _catalog.name);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError($"[Cues] {_catalog.name}: {exception.Message}", this);
                        _parseFailed = true;
                    }
                }

                return _parsed;
            }
        }

        /// <summary>All bindings (editor tooling).</summary>
        public List<CombatCueBinding> Bindings => _bindings;

        /// <summary>Assigns the catalog JSON (editor tooling).</summary>
        public void SetCatalog(TextAsset catalog)
        {
            _catalog = catalog;
            Invalidate();
        }

        /// <summary>Finds the binding for a cue and variant, falling back to the default variant.</summary>
        public bool TryGetBinding(string cue, string variant, out CombatCueBinding binding)
        {
            if (_lookup == null)
            {
                _lookup = new Dictionary<string, CombatCueBinding>(StringComparer.Ordinal);
                foreach (var b in _bindings)
                {
                    if (b != null && !string.IsNullOrEmpty(b.Cue))
                    {
                        _lookup[Key(b.Cue, b.Variant)] = b;
                    }
                }
            }

            if (!string.IsNullOrEmpty(variant) && _lookup.TryGetValue(Key(cue, variant), out binding))
            {
                return true;
            }

            return _lookup.TryGetValue(Key(cue, ""), out binding);
        }

        /// <summary>Drops cached lookups after bindings or the catalog change.</summary>
        public void Invalidate()
        {
            _parsed = null;
            _parseFailed = false;
            _lookup = null;
        }

        private static string Key(string cue, string variant) => string.IsNullOrEmpty(variant) ? cue : cue + "|" + variant;

#if UNITY_EDITOR
        private void OnValidate() => Invalidate();
#endif
    }
}

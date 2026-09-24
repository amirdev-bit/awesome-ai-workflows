using System.Collections.Generic;
using Oathsunder.Combat.Content;
using UnityEngine;

namespace Oathsunder.Gameplay.Combat
{
    /// <summary>
    /// Project asset listing every combat JSON file that ships in a build. The deterministic core never touches
    /// UnityEngine; this asset is the bridge that feeds it TextAssets.
    /// </summary>
    [CreateAssetMenu(fileName = "DA_CombatContentLibrary", menuName = "Oathsunder/Combat/Content Library", order = 0)]
    public sealed class CombatContentLibrary : ScriptableObject
    {
        [Tooltip("Every fighter.*, moveset.*, stage.*, rules.* and tuning.* JSON file shipped in this build.")]
        [SerializeField]
        private List<TextAsset> _files = new List<TextAsset>();

        private CombatContentSet _cached;

        /// <summary>The JSON files referenced by this library.</summary>
        public IReadOnlyList<TextAsset> Files => _files;

        /// <summary>
        /// Parses (once) and returns the content set. Problems are logged with the file name and JSON path so the
        /// Console points designers straight at the broken field.
        /// </summary>
        public CombatContentSet Load()
        {
            if (_cached != null)
            {
                return _cached;
            }

            var set = new CombatContentSet();
            foreach (var file in _files)
            {
                if (file == null)
                {
                    Debug.LogError($"{name}: the file list contains an empty slot.", this);
                    continue;
                }

                set.Add(file.name + ".json", file.text);
            }

            foreach (var error in set.Errors)
            {
                Debug.LogError($"[Combat content] {error}", this);
            }

            _cached = set;
            return set;
        }

        /// <summary>Drops the parsed cache (the editor calls this after content files change).</summary>
        public void Invalidate() => _cached = null;

#if UNITY_EDITOR
        /// <summary>Editor-only: replaces the file list (used by the content importer).</summary>
        public void SetFiles(IEnumerable<TextAsset> files)
        {
            _files = new List<TextAsset>(files);
            Invalidate();
        }

        private void OnValidate() => Invalidate();
#endif
    }
}

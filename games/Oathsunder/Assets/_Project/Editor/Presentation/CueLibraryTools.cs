using System;
using System.Collections.Generic;
using System.Linq;
using Oathsunder.Combat.Content;
using Oathsunder.Core.Serialization;
using Oathsunder.Gameplay.Presentation;
using Oathsunder.Presentation.Cues;
using UnityEditor;
using UnityEngine;

namespace Oathsunder.Editor.Presentation
{
    /// <summary>
    /// Editor side of the cue catalog: validates the catalog and the presentation Definition of Done for every
    /// fighter × weapon (called from the combat content validator, so CI covers it), and keeps every
    /// <see cref="CombatCueLibrary"/> in sync with the catalog.
    /// </summary>
    public static class CueLibraryTools
    {
        /// <summary>Folder holding presentation content.</summary>
        public const string PresentationFolder = "Assets/_Project/Content/Presentation";

        /// <summary>The combat cue catalog.</summary>
        public const string CatalogPath = PresentationFolder + "/cues.combat.json";

        /// <summary>Loads and parses the catalog; parse problems are appended to <paramref name="errors"/>.</summary>
        public static CueCatalog LoadCatalog(List<string> errors)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(CatalogPath);
            if (asset == null)
            {
                errors.Add($"{CatalogPath}: missing");
                return null;
            }

            try
            {
                return CueCatalogParser.Parse(asset.text, CatalogPath);
            }
            catch (ContentValidationException exception)
            {
                errors.AddRange(exception.Errors.Select(e => $"{CatalogPath}: {e}"));
            }
            catch (Exception exception) when (exception is JsonSyntaxException || exception is JsonContentException)
            {
                errors.Add(exception.Message);
            }

            return null;
        }

        /// <summary>Checks the catalog, every blueprint's cue coverage, and the event map.</summary>
        public static List<string> Validate(CombatContentSet set, IEnumerable<(string Fighter, string Weapon)> combinations)
        {
            var errors = new List<string>();
            var catalog = LoadCatalog(errors);
            if (catalog == null)
            {
                return errors;
            }

            foreach (string name in EventCueMap.All)
            {
                if (!catalog.Contains(name))
                {
                    errors.Add($"{CatalogPath}: event cue '{name}' is not declared");
                }
            }

            foreach (var (fighter, weapon) in combinations)
            {
                try
                {
                    var blueprint = set.BuildBlueprint(fighter, Gameplay.Combat.CombatMatchConfig.UniversalMoveSetId, weapon);
                    errors.AddRange(CueCoverage.Check(blueprint, catalog).Select(e => $"{fighter} + {weapon}: {e}"));
                }
                catch (ContentValidationException)
                {
                    // Reported by the combat validator already.
                }
                catch (KeyNotFoundException)
                {
                    // Reported by the combat validator already.
                }
            }

            return errors;
        }

        [MenuItem("Oathsunder/Presentation/Sync Cue Library", priority = 10)]
        private static void SyncFromMenu()
        {
            var errors = new List<string>();
            var catalog = LoadCatalog(errors);
            if (catalog == null)
            {
                errors.ForEach(e => Debug.LogError("[Cues] " + e));
                return;
            }

            var catalogAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(CatalogPath);
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(CombatCueLibrary)))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var library = AssetDatabase.LoadAssetAtPath<CombatCueLibrary>(path);
                int added = Sync(library, catalog);
                library.SetCatalog(catalogAsset);
                EditorUtility.SetDirty(library);
                var missing = MissingAssets(library, catalog);
                Debug.Log($"[Cues] {path}: {added} binding row(s) added, {missing.Count} cue(s) still need assets.");
                foreach (string cue in missing)
                {
                    Debug.LogWarning($"[Cues] {path}: '{cue}' has no asset bound yet.");
                }
            }

            AssetDatabase.SaveAssets();
        }

        /// <summary>Adds a default-variant binding row for every catalog cue that needs assets.</summary>
        /// <returns>Rows added.</returns>
        public static int Sync(CombatCueLibrary library, CueCatalog catalog)
        {
            var existing = new HashSet<string>(library.Bindings.Where(b => b != null && string.IsNullOrEmpty(b.Variant)).Select(b => b.Cue), StringComparer.Ordinal);
            int added = 0;
            foreach (var cue in catalog.Cues)
            {
                if (NeedsAsset(cue) && existing.Add(cue.Name))
                {
                    library.Bindings.Add(new CombatCueBinding { Cue = cue.Name });
                    added++;
                }
            }

            library.Bindings.Sort((a, b) =>
            {
                int byCue = string.CompareOrdinal(a.Cue, b.Cue);
                return byCue != 0 ? byCue : string.CompareOrdinal(a.Variant, b.Variant);
            });
            library.Invalidate();
            return added;
        }

        /// <summary>Catalog cues that need assets but have no default binding with one.</summary>
        public static List<string> MissingAssets(CombatCueLibrary library, CueCatalog catalog)
        {
            var missing = new List<string>();
            foreach (var cue in catalog.Cues)
            {
                if (!NeedsAsset(cue))
                {
                    continue;
                }

                bool bound = library.Bindings.Any(b => b != null && b.Cue == cue.Name && HasAsset(cue, b));
                if (!bound)
                {
                    missing.Add(cue.Name);
                }
            }

            return missing;
        }

        private static bool NeedsAsset(CueDefinition cue) =>
            cue.Channel == CueChannel.Vfx || cue.Channel == CueChannel.Sfx || cue.Channel == CueChannel.Foley || cue.Channel == CueChannel.Vo ||
            (cue.Channel == CueChannel.Cam && cue.Camera.Kind == CameraCueKind.Sequence);

        private static bool HasAsset(CueDefinition cue, CombatCueBinding binding)
        {
            switch (cue.Channel)
            {
                case CueChannel.Vfx: return binding.Vfx != null;
                case CueChannel.Cam: return binding.Sequence != null;
                default: return binding.Clips != null && binding.Clips.Any(c => c != null);
            }
        }
    }
}

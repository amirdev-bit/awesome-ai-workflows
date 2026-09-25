using System;
using System.Collections.Generic;
using System.Linq;
using Oathsunder.Combat.Content;
using Oathsunder.Editor.Animation;
using Oathsunder.Editor.Presentation;
using Oathsunder.Gameplay.Combat;
using UnityEditor;
using UnityEngine;

namespace Oathsunder.Editor.Combat
{
    /// <summary>
    /// Validates every combat JSON file and every fighter × weapon combination. Runs from the menu, automatically
    /// after content is imported, and in CI through <see cref="ValidateFromCommandLine"/>:
    /// <c>Unity -batchmode -quit -projectPath . -executeMethod Oathsunder.Editor.Combat.CombatContentValidator.ValidateFromCommandLine</c>
    /// </summary>
    public static class CombatContentValidator
    {
        /// <summary>Folder holding the combat content.</summary>
        public const string ContentFolder = "Assets/_Project/Content/Combat";

        /// <summary>Loads every JSON under the content folder into a content set.</summary>
        public static CombatContentSet LoadAll(out List<TextAsset> files)
        {
            files = new List<TextAsset>();
            var set = new CombatContentSet();
            foreach (string guid in AssetDatabase.FindAssets("t:TextAsset", new[] { ContentFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                files.Add(asset);
                set.Add(path, asset.text);
            }

            return set;
        }

        /// <summary>
        /// Validates all combat content, the cue catalog and the presentation Definition of Done; returns every
        /// problem found.
        /// </summary>
        public static List<string> Validate()
        {
            var set = LoadAll(out _);
            var errors = new List<string>(set.Errors);
            var weapons = set.MoveSetIds.Where(id => id != CombatMatchConfig.UniversalMoveSetId && id.StartsWith("weapon.", StringComparison.Ordinal)).ToList();
            var combinations = set.FighterIds.SelectMany(f => weapons.Select(w => (f, w))).ToList();
            foreach (string fighter in set.FighterIds)
            {
                foreach (string weapon in weapons)
                {
                    try
                    {
                        set.BuildBlueprint(fighter, CombatMatchConfig.UniversalMoveSetId, weapon);
                    }
                    catch (ContentValidationException exception)
                    {
                        errors.AddRange(exception.Errors.Select(e => $"{fighter} + {weapon}: {e}"));
                    }
                    catch (KeyNotFoundException exception)
                    {
                        errors.Add(exception.Message);
                    }
                }
            }

            errors.AddRange(CueLibraryTools.Validate(set, combinations));
            return errors;
        }

        [MenuItem("Oathsunder/Combat/Validate Content", priority = 1)]
        private static void ValidateFromMenu()
        {
            var errors = Validate();
            if (errors.Count == 0)
            {
                Debug.Log("[Combat content] All combat content is valid.");
                return;
            }

            foreach (string error in errors)
            {
                Debug.LogError("[Combat content] " + error);
            }
        }

        /// <summary>Batch-mode entry point for CI. Exits with code 1 on any error.</summary>
        public static void ValidateFromCommandLine()
        {
            var errors = Validate();
            foreach (string error in errors)
            {
                Console.Error.WriteLine("[Combat content] " + error);
            }

            Console.WriteLine($"[Combat content] {errors.Count} error(s).");
            EditorApplication.Exit(errors.Count == 0 ? 0 : 1);
        }

        [MenuItem("Oathsunder/Combat/Sync Content Libraries", priority = 2)]
        private static void SyncLibraries()
        {
            LoadAll(out var files);
            files.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(CombatContentLibrary)))
            {
                var library = AssetDatabase.LoadAssetAtPath<CombatContentLibrary>(AssetDatabase.GUIDToAssetPath(guid));
                library.SetFiles(files);
                EditorUtility.SetDirty(library);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Combat content] Synced {files.Count} files into every content library.");
        }
    }

    /// <summary>Re-validates combat content whenever a JSON file under the content folder is imported.</summary>
    public sealed class CombatContentPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            bool touched = imported.Concat(deleted).Concat(moved).Any(IsCombatContent);
            if (!touched)
            {
                return;
            }

            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(CombatContentLibrary)))
            {
                AssetDatabase.LoadAssetAtPath<CombatContentLibrary>(AssetDatabase.GUIDToAssetPath(guid))?.Invalidate();
            }

            AnimationClipSpecValidator.Invalidate();

            foreach (string error in CombatContentValidator.Validate())
            {
                Debug.LogError("[Combat content] " + error);
            }
        }

        private static bool IsCombatContent(string path) =>
            (path.StartsWith(CombatContentValidator.ContentFolder, StringComparison.Ordinal) ||
             path.StartsWith(CueLibraryTools.PresentationFolder, StringComparison.Ordinal)) &&
            path.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
    }
}

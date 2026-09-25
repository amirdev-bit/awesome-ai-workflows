using System;
using System.Collections.Generic;
using System.Linq;
using Oathsunder.Combat.Content;
using Oathsunder.Editor.Combat;
using Oathsunder.Gameplay.Combat;
using Oathsunder.Presentation.Animation;
using UnityEditor;
using UnityEngine;

namespace Oathsunder.Editor.Animation
{
    /// <summary>
    /// Checks imported animation clips against the specification captured from the simulation
    /// (<see cref="MoveAnimationCapture"/>): 60 fps, exact frame count, authored in place, not looping. Runs on
    /// every clip import and from "Oathsunder/Animation/Validate Clips Against Spec", which also reports which
    /// specified clips have not been delivered yet.
    /// </summary>
    public sealed class AnimationClipSpecValidator : AssetPostprocessor
    {
        /// <summary>Root speed above which a clip is considered to carry root motion (m/s).</summary>
        public const float InPlaceTolerance = 0.05f;

        private const string AnimationFolder = "Assets/_Project/Animation";

        private static Dictionary<string, MoveAnimationSpec> _specs;

        /// <summary>Specs by clip name for every fighter × weapon in the content (cached until content changes).</summary>
        public static Dictionary<string, MoveAnimationSpec> Specs
        {
            get
            {
                if (_specs == null)
                {
                    _specs = new Dictionary<string, MoveAnimationSpec>(StringComparer.Ordinal);
                    var set = CombatContentValidator.LoadAll(out _);
                    foreach (string fighter in set.FighterIds)
                    {
                        foreach (string weapon in set.MoveSetIds.Where(id => id.StartsWith("weapon.", StringComparison.Ordinal)))
                        {
                            try
                            {
                                var blueprint = set.BuildBlueprint(fighter, CombatMatchConfig.UniversalMoveSetId, weapon);
                                foreach (var spec in MoveAnimationCapture.CaptureAll(blueprint, set.Tuning))
                                {
                                    if (!_specs.ContainsKey(spec.Clip))
                                    {
                                        _specs.Add(spec.Clip, spec);
                                    }
                                }
                            }
                            catch (ContentValidationException)
                            {
                                // Reported by the content validator.
                            }
                        }
                    }
                }

                return _specs;
            }
        }

        /// <summary>Drops the cached specs (content changed).</summary>
        public static void Invalidate() => _specs = null;

        /// <summary>Problems with one clip, or an empty list when it matches its spec (or has none).</summary>
        public static List<string> Check(AnimationClip clip, string path)
        {
            var problems = new List<string>();
            if (clip == null || !Specs.TryGetValue(clip.name, out var spec))
            {
                return problems;
            }

            if (!Mathf.Approximately(clip.frameRate, 60f))
            {
                problems.Add($"{path} [{clip.name}]: authored at {clip.frameRate} fps; combat clips are 60 fps.");
            }

            int frames = Mathf.RoundToInt(clip.length * 60f);
            if (frames != spec.Move.TotalFrames)
            {
                problems.Add($"{path} [{clip.name}]: {frames} frames, the spec for {spec.Move.Id} needs exactly {spec.Move.TotalFrames} ({spec.Seconds:0.000} s).");
            }

            if (clip.isLooping)
            {
                problems.Add($"{path} [{clip.name}]: move clips must not loop.");
            }

            float speed = clip.averageSpeed.magnitude;
            if (speed > InPlaceTolerance)
            {
                problems.Add($"{path} [{clip.name}]: root moves at {speed:0.00} m/s; author in place, the simulation owns root motion ({spec.RootMotion}, travel {spec.FinalOffset.X} m).");
            }

            return problems;
        }

        private void OnPostprocessAnimation(GameObject root, AnimationClip clip)
        {
            if (!assetPath.StartsWith(AnimationFolder, StringComparison.Ordinal))
            {
                return;
            }

            foreach (string problem in Check(clip, assetPath))
            {
                Debug.LogError("[Animation spec] " + problem);
            }
        }

        [MenuItem("Oathsunder/Animation/Validate Clips Against Spec", priority = 20)]
        private static void ValidateAll()
        {
            Invalidate();
            var delivered = new HashSet<string>(StringComparer.Ordinal);
            int problems = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:AnimationClip", new[] { AnimationFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                foreach (var clip in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>())
                {
                    if (clip.name.StartsWith("__preview__", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    delivered.Add(clip.name);
                    foreach (string problem in Check(clip, path))
                    {
                        Debug.LogError("[Animation spec] " + problem);
                        problems++;
                    }
                }
            }

            var missing = Specs.Keys.Where(k => !delivered.Contains(k)).OrderBy(k => k, StringComparer.Ordinal).ToList();
            Debug.Log($"[Animation spec] {Specs.Count - missing.Count}/{Specs.Count} specified clips delivered, {problems} problem(s).");
            foreach (string clip in missing)
            {
                Debug.LogWarning($"[Animation spec] Not delivered yet: {clip} ({Specs[clip].Move.Id}, {Specs[clip].Move.TotalFrames} frames).");
            }
        }
    }
}

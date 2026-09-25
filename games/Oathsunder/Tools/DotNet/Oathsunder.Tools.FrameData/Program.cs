using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Oathsunder.Combat.Content;
using Oathsunder.Combat.Definitions;
using Oathsunder.Presentation.Animation;

namespace Oathsunder.Tools
{
    /// <summary>
    /// Generates every data-derived production document from the shipped content, so specs can never drift
    /// from gameplay:
    /// <list type="bullet">
    /// <item><c>Documentation/05-CoreCombat/Generated-FrameData.md</c> — frame data, clip list, cue usage.</item>
    /// <item><c>Documentation/Production/03-Animation/Generated-AnimationLibrary.md</c> — one production sheet per move.</item>
    /// <item><c>Documentation/Production/Generated-CueCatalog.md</c> — every VFX, sound, voice, camera and haptic cue.</item>
    /// <item><c>Assets/_Project/Animation/Specs/animspec.&lt;fighter&gt;.&lt;weapon&gt;.json</c> — machine-readable animation specs.</item>
    /// </list>
    /// Usage: <c>dotnet run -c Release -- --write</c> (update the repository), <c>-- --check</c> (exit 1 when any
    /// committed file is stale; CI), or <c>-- &lt;file.md&gt;</c> (frame data only, for the CI artifact).
    /// </summary>
    internal static class Program
    {
        private const string Fighter = "fighter.rhen";

        private static int Main(string[] args)
        {
            var content = ContentLocator.Load();
            var catalog = ContentLocator.LoadCueCatalog();
            var blueprints = new List<(string Weapon, FighterBlueprint Blueprint)>();
            foreach (var setId in content.MoveSetIds.Where(id => id.StartsWith("weapon.", StringComparison.Ordinal)).OrderBy(id => id, StringComparer.Ordinal))
            {
                blueprints.Add((setId, content.BuildBlueprint(Fighter, "moveset.universal", setId)));
            }

            var specs = blueprints.Select(b => (b.Weapon, b.Blueprint, Specs: MoveAnimationCapture.CaptureAll(b.Blueprint, content.Tuning))).ToList();
            var outputs = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["Documentation/05-CoreCombat/Generated-FrameData.md"] = FrameDataReport.Build(content, specs.Select(s => (s.Weapon, s.Blueprint, s.Specs)).ToList()),
                ["Documentation/Production/03-Animation/Generated-AnimationLibrary.md"] = AnimationLibraryReport.Build(content, specs.Select(s => (s.Weapon, s.Blueprint, s.Specs)).ToList(), catalog),
                ["Documentation/Production/Generated-CueCatalog.md"] = CueCatalogReport.Build(catalog, specs.Select(s => s.Blueprint).ToList()),
            };
            foreach (var (weapon, _, list) in specs)
            {
                string weaponName = weapon.Substring("weapon.".Length);
                outputs[$"Assets/_Project/Animation/Specs/animspec.{Fighter.Substring("fighter.".Length)}.{weaponName}.json"] =
                    AnimationSpecJson.Write(Fighter, weapon, list, catalog);
            }

            string root = ContentLocator.ProjectRoot();
            if (args.Length == 1 && args[0] == "--write")
            {
                foreach (var pair in outputs)
                {
                    string path = Path.Combine(root, pair.Key);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllText(path, pair.Value);
                    Console.WriteLine($"Wrote {pair.Key}");
                }

                return 0;
            }

            if (args.Length == 1 && args[0] == "--check")
            {
                int stale = 0;
                foreach (var pair in outputs)
                {
                    string path = Path.Combine(root, pair.Key);
                    string committed = File.Exists(path) ? File.ReadAllText(path).Replace("\r\n", "\n") : null;
                    if (committed != pair.Value)
                    {
                        Console.Error.WriteLine($"STALE: {pair.Key} (run: dotnet run -c Release --project Oathsunder.Tools.FrameData -- --write)");
                        stale++;
                    }
                }

                Console.WriteLine(stale == 0 ? $"All {outputs.Count} generated files are up to date." : $"{stale} generated file(s) are stale.");
                return stale == 0 ? 0 : 1;
            }

            string frameData = outputs["Documentation/05-CoreCombat/Generated-FrameData.md"];
            if (args.Length == 1)
            {
                File.WriteAllText(args[0], frameData);
                Console.WriteLine($"Wrote {args[0]}");
            }
            else
            {
                Console.Write(frameData);
            }

            return 0;
        }
    }
}

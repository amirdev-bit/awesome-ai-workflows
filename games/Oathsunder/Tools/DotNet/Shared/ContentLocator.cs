using System;
using System.IO;
using Oathsunder.Combat.Content;
using Oathsunder.Presentation.Cues;

namespace Oathsunder.Tools
{
    /// <summary>Loads the Unity project's combat content from disk for command-line tools.</summary>
    internal static class ContentLocator
    {
        private const string Relative = "Assets/_Project/Content/Combat";

        public static string FindRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                string candidate = Path.Combine(directory.FullName, Relative);
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException($"Could not find {Relative} above {AppContext.BaseDirectory}.");
        }

        /// <summary>The Oathsunder project root (the folder holding Assets/ and Documentation/).</summary>
        public static string ProjectRoot() => Path.GetFullPath(Path.Combine(FindRoot(), "..", "..", "..", ".."));

        /// <summary>Loads the shipped combat cue catalog.</summary>
        public static CueCatalog LoadCueCatalog()
        {
            const string file = "cues.combat.json";
            string path = Path.Combine(FindRoot(), "..", "Presentation", file);
            return CueCatalogParser.Parse(File.ReadAllText(path), file);
        }

        public static CombatContentSet Load()
        {
            var set = new CombatContentSet();
            foreach (var file in Directory.GetFiles(FindRoot(), "*.json", SearchOption.AllDirectories))
            {
                set.Add(file, File.ReadAllText(file));
            }

            if (set.Errors.Count > 0)
            {
                throw new InvalidDataException("Content errors:\n" + string.Join("\n", set.Errors));
            }

            return set;
        }
    }
}

using System;
using System.IO;
using Oathsunder.Combat.Content;
using Oathsunder.Combat.Definitions;
using NUnit.Framework;

namespace Oathsunder.Tests.Support
{
    /// <summary>
    /// Loads the shipped combat content from disk. Works in the Unity Test Runner (current directory is the
    /// project root) and under <c>dotnet test</c> (walks up from the test binary).
    /// </summary>
    public static class TestContent
    {
        private const string ContentRelativePath = "Assets/_Project/Content/Combat";

        private static string _root;
        private static FighterDefinition _rhen;
        private static MoveSetDefinition _universal;
        private static MoveSetDefinition _katana;

        /// <summary>Absolute path of the combat content folder.</summary>
        public static string Root => _root ?? (_root = FindRoot());

        /// <summary>Reads a content file.</summary>
        public static string Read(string relativePath) => File.ReadAllText(Path.Combine(Root, relativePath));

        /// <summary>Rhen's body.</summary>
        public static FighterDefinition Rhen => _rhen ?? (_rhen = CombatContentParser.ParseFighter(Read("Fighters/fighter.rhen.json"), "fighter.rhen.json"));

        /// <summary>Universal move set.</summary>
        public static MoveSetDefinition Universal => _universal ?? (_universal = CombatContentParser.ParseMoveSet(Read("MoveSets/moveset.universal.json"), "moveset.universal.json"));

        /// <summary>Katana move set.</summary>
        public static MoveSetDefinition Katana => _katana ?? (_katana = CombatContentParser.ParseMoveSet(Read("MoveSets/moveset.katana.json"), "moveset.katana.json"));

        /// <summary>Shipped tuning.</summary>
        public static CombatTuning Tuning() => CombatContentParser.ParseTuning(Read("Tuning/tuning.combat.json"), "tuning.combat.json");

        /// <summary>A shipped stage.</summary>
        public static StageDefinition Stage(string id) => CombatContentParser.ParseStage(Read($"Stages/{id}.json"), id);

        /// <summary>A shipped rules file.</summary>
        public static MatchRules Rules(string id) => CombatContentParser.ParseRules(Read($"Rules/{id}.json"), id);

        /// <summary>Rhen wielding the katana (a fresh blueprint each call).</summary>
        public static FighterBlueprint RhenKatana() => FighterBlueprintBuilder.Build(Rhen, Universal, Katana);

        private static string FindRoot()
        {
            foreach (var start in new[] { Directory.GetCurrentDirectory(), TestContext.CurrentContext.TestDirectory, AppContext.BaseDirectory })
            {
                var directory = new DirectoryInfo(start);
                while (directory != null)
                {
                    string candidate = Path.Combine(directory.FullName, ContentRelativePath);
                    if (Directory.Exists(candidate))
                    {
                        return candidate;
                    }

                    candidate = Path.Combine(directory.FullName, "games", "Oathsunder", ContentRelativePath);
                    if (Directory.Exists(candidate))
                    {
                        return candidate;
                    }

                    directory = directory.Parent;
                }
            }

            throw new DirectoryNotFoundException($"Could not locate '{ContentRelativePath}'.");
        }
    }
}

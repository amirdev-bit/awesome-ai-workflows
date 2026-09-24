using System;
using System.Collections.Generic;
using Oathsunder.Combat.Definitions;
using Oathsunder.Core.Serialization;

namespace Oathsunder.Combat.Content
{
    /// <summary>
    /// Registry of parsed combat content, fed with (file name, JSON text) pairs. The source is up to the caller:
    /// Unity feeds TextAssets or Addressables, command-line tools feed files, servers feed a content bundle.
    /// Files are classified by prefix: <c>fighter.</c>, <c>moveset.</c>, <c>stage.</c>, <c>rules.</c>, <c>tuning.</c>.
    /// </summary>
    public sealed class CombatContentSet
    {
        private readonly Dictionary<string, FighterDefinition> _fighters = new Dictionary<string, FighterDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, MoveSetDefinition> _moveSets = new Dictionary<string, MoveSetDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, StageDefinition> _stages = new Dictionary<string, StageDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, MatchRules> _rules = new Dictionary<string, MatchRules>(StringComparer.Ordinal);
        private readonly List<string> _errors = new List<string>();

        /// <summary>Combat tuning (shipped defaults until a tuning file is added).</summary>
        public CombatTuning Tuning { get; private set; } = new CombatTuning();

        /// <summary>Errors collected while adding files.</summary>
        public IReadOnlyList<string> Errors => _errors;

        /// <summary>Fighter ids.</summary>
        public IEnumerable<string> FighterIds => _fighters.Keys;

        /// <summary>Move-set ids.</summary>
        public IEnumerable<string> MoveSetIds => _moveSets.Keys;

        /// <summary>Stage ids.</summary>
        public IEnumerable<string> StageIds => _stages.Keys;

        /// <summary>Rules ids.</summary>
        public IEnumerable<string> RulesIds => _rules.Keys;

        /// <summary>
        /// Parses and registers one content file. Problems are recorded in <see cref="Errors"/> instead of thrown,
        /// so a validation pass can report every broken file at once.
        /// </summary>
        /// <returns>True when the file was added.</returns>
        public bool Add(string fileName, string json)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            string name = fileName.Replace('\\', '/');
            int slash = name.LastIndexOf('/');
            if (slash >= 0)
            {
                name = name.Substring(slash + 1);
            }

            try
            {
                if (name.StartsWith("fighter.", StringComparison.Ordinal))
                {
                    var fighter = CombatContentParser.ParseFighter(json, name);
                    return Register(_fighters, fighter.Id, fighter, name);
                }

                if (name.StartsWith("moveset.", StringComparison.Ordinal))
                {
                    var set = CombatContentParser.ParseMoveSet(json, name);
                    return Register(_moveSets, set.Id, set, name);
                }

                if (name.StartsWith("stage.", StringComparison.Ordinal))
                {
                    var stage = CombatContentParser.ParseStage(json, name);
                    return Register(_stages, stage.Id, stage, name);
                }

                if (name.StartsWith("rules.", StringComparison.Ordinal))
                {
                    var rules = CombatContentParser.ParseRules(json, name);
                    return Register(_rules, rules.Id, rules, name);
                }

                if (name.StartsWith("tuning.", StringComparison.Ordinal))
                {
                    Tuning = CombatContentParser.ParseTuning(json, name);
                    return true;
                }

                _errors.Add($"{name}: unknown content type (expected fighter., moveset., stage., rules. or tuning. prefix)");
                return false;
            }
            catch (Exception exception) when (exception is JsonSyntaxException || exception is JsonContentException)
            {
                _errors.Add(exception.Message);
                return false;
            }
        }

        /// <summary>Fighter body by id.</summary>
        public FighterDefinition Fighter(string id) => Find(_fighters, id, "fighter");

        /// <summary>Move set by id.</summary>
        public MoveSetDefinition MoveSet(string id) => Find(_moveSets, id, "move set");

        /// <summary>Stage by id.</summary>
        public StageDefinition Stage(string id) => Find(_stages, id, "stage");

        /// <summary>
        /// Rules by id. Returns a fresh copy each call so callers (tests, practice-mode options) can tweak it
        /// without affecting other matches.
        /// </summary>
        public MatchRules Rules(string id)
        {
            var source = Find(_rules, id, "rules");
            return new MatchRules
            {
                Id = source.Id,
                RoundsToWin = source.RoundsToWin,
                RoundTimerFrames = source.RoundTimerFrames,
                PreRoundFrames = source.PreRoundFrames,
                KnockoutHoldFrames = source.KnockoutHoldFrames,
                PerfectDodgeSlowPermille = source.PerfectDodgeSlowPermille,
                PerfectDodgeSlowFrames = source.PerfectDodgeSlowFrames,
                ExecutionsEnabled = source.ExecutionsEnabled,
                RageBurstEnabled = source.RageBurstEnabled,
                UseRpgStats = source.UseRpgStats,
                CarryUltimateMeter = source.CarryUltimateMeter,
                FriendlyFire = source.FriendlyFire,
            };
        }

        /// <summary>Builds a validated blueprint: the body plus move sets merged in order (universal first).</summary>
        /// <exception cref="ContentValidationException">The merged content is invalid.</exception>
        public FighterBlueprint BuildBlueprint(string fighterId, params string[] moveSetIds)
        {
            var sets = new MoveSetDefinition[moveSetIds.Length];
            for (int i = 0; i < sets.Length; i++)
            {
                sets[i] = MoveSet(moveSetIds[i]);
            }

            return FighterBlueprintBuilder.Build(Fighter(fighterId), sets);
        }

        private bool Register<T>(Dictionary<string, T> table, string id, T value, string file)
        {
            if (table.ContainsKey(id))
            {
                _errors.Add($"{file}: duplicate id '{id}'");
                return false;
            }

            table.Add(id, value);
            return true;
        }

        private static T Find<T>(Dictionary<string, T> table, string id, string kind)
        {
            if (id != null && table.TryGetValue(id, out var value))
            {
                return value;
            }

            throw new KeyNotFoundException($"Unknown {kind} '{id}'.");
        }
    }
}

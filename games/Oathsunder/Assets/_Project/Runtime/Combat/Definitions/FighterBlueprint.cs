using System.Collections.Generic;

namespace Oathsunder.Combat.Definitions
{
    /// <summary>
    /// Fully resolved, immutable description of one combatant for a match: body + universal moves + weapon
    /// moves, with every cross-reference resolved to an index. Built once by
    /// <see cref="Content.FighterBlueprintBuilder"/>; shared by simulation, AI, training-mode tooling and UI.
    /// </summary>
    public sealed class FighterBlueprint
    {
        private readonly Dictionary<string, int> _moveIndexById;
        private readonly Dictionary<string, int> _projectileIndexById;

        internal FighterBlueprint(
            FighterDefinition body,
            MoveDefinition[] moves,
            ProjectileDefinition[] projectiles,
            string[] cueNames,
            int[] neutralCandidates,
            Dictionary<string, int> moveIndexById,
            Dictionary<string, int> projectileIndexById,
            string[] moveSetIds)
        {
            Body = body;
            Moves = moves;
            Projectiles = projectiles;
            CueNames = cueNames;
            NeutralCandidates = neutralCandidates;
            _moveIndexById = moveIndexById;
            _projectileIndexById = projectileIndexById;
            MoveSetIds = moveSetIds;
            TechRollMoveIndex = FindMoveIndex(WellKnownMoves.TechRoll);
        }

        /// <summary>Body definition.</summary>
        public FighterDefinition Body { get; }

        /// <summary>All moves, indexed by <see cref="MoveDefinition.Index"/>.</summary>
        public MoveDefinition[] Moves { get; }

        /// <summary>All projectiles, indexed by <see cref="ProjectileDefinition.Index"/>.</summary>
        public ProjectileDefinition[] Projectiles { get; }

        /// <summary>Presentation cue names, indexed by cue index.</summary>
        public string[] CueNames { get; }

        /// <summary>Moves startable from neutral (not chain-only, with triggers), in declaration order.</summary>
        public int[] NeutralCandidates { get; }

        /// <summary>Ids of the merged move sets, in merge order.</summary>
        public string[] MoveSetIds { get; }

        /// <summary>Index of the tech-roll move (-1 if the content has none).</summary>
        public int TechRollMoveIndex { get; }

        /// <summary>Index of a move by id, or -1.</summary>
        public int FindMoveIndex(string id) => id != null && _moveIndexById.TryGetValue(id, out int index) ? index : -1;

        /// <summary>Index of a projectile by id, or -1.</summary>
        public int FindProjectileIndex(string id) => id != null && _projectileIndexById.TryGetValue(id, out int index) ? index : -1;

        /// <summary>Move by id, or null.</summary>
        public MoveDefinition FindMove(string id)
        {
            int index = FindMoveIndex(id);
            return index >= 0 ? Moves[index] : null;
        }
    }

    /// <summary>Ids the simulation looks up by name.</summary>
    public static class WellKnownMoves
    {
        /// <summary>Soft-knockdown tech roll.</summary>
        public const string TechRoll = "universal.techroll";
    }
}

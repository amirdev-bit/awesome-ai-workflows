using System;
using System.Collections.Generic;
using Oathsunder.Combat.Definitions;

namespace Oathsunder.Combat.Simulation
{
    /// <summary>One combatant in an encounter.</summary>
    public sealed class CombatantSetup
    {
        /// <summary>Creates a combatant.</summary>
        public CombatantSetup(FighterBlueprint blueprint, int team)
        {
            Blueprint = blueprint ?? throw new ArgumentNullException(nameof(blueprint));
            Team = team;
        }

        /// <summary>Resolved fighter.</summary>
        public FighterBlueprint Blueprint { get; }

        /// <summary>Team (0 or 1).</summary>
        public int Team { get; }

        /// <summary>RPG-derived stats (ignored when the rules normalise stats).</summary>
        public CombatStats Stats { get; set; } = CombatStats.Normalized;

        /// <summary>Control scheme of the player (or AI) driving this fighter.</summary>
        public ControlScheme Scheme { get; set; } = ControlScheme.Classic;
    }

    /// <summary>Everything needed to start a deterministic encounter. Both peers of a netplay match build an identical setup.</summary>
    public sealed class CombatSetup
    {
        /// <summary>Arena bounds.</summary>
        public StageDefinition Stage { get; set; } = new StageDefinition();

        /// <summary>Mechanics constants.</summary>
        public CombatTuning Tuning { get; set; } = new CombatTuning();

        /// <summary>Mode rules.</summary>
        public MatchRules Rules { get; set; } = new MatchRules();

        /// <summary>Combatants (at most <see cref="CombatWorldState.MaxFighters"/>).</summary>
        public List<CombatantSetup> Combatants { get; } = new List<CombatantSetup>();

        /// <summary>Seed of the deterministic random stream.</summary>
        public ulong Seed { get; set; } = 0x0A7B5D3EUL;

        /// <summary>Validates the setup.</summary>
        /// <exception cref="InvalidOperationException">The setup cannot start a match.</exception>
        public void Validate()
        {
            if (Combatants.Count < 2 || Combatants.Count > CombatWorldState.MaxFighters)
            {
                throw new InvalidOperationException($"An encounter needs 2..{CombatWorldState.MaxFighters} combatants (got {Combatants.Count}).");
            }

            bool team0 = false;
            bool team1 = false;
            foreach (var combatant in Combatants)
            {
                if (combatant.Team == 0)
                {
                    team0 = true;
                }
                else if (combatant.Team == 1)
                {
                    team1 = true;
                }
                else
                {
                    throw new InvalidOperationException($"Team must be 0 or 1 (got {combatant.Team}).");
                }
            }

            if (!team0 || !team1)
            {
                throw new InvalidOperationException("Both teams need at least one combatant.");
            }

            if (Stage == null || Tuning == null || Rules == null)
            {
                throw new InvalidOperationException("Stage, tuning and rules are required.");
            }
        }
    }
}

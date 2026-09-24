using System;
using System.Collections.Generic;
using Oathsunder.Combat.Definitions;
using Oathsunder.Combat.Simulation;
using UnityEngine;

namespace Oathsunder.Gameplay.Combat
{
    /// <summary>One combatant slot of a match configuration.</summary>
    [Serializable]
    public sealed class CombatantSlot
    {
        [Tooltip("Fighter body id, e.g. fighter.rhen")]
        public string FighterId = "fighter.rhen";

        [Tooltip("Weapon move set id, e.g. weapon.katana")]
        public string WeaponMoveSetId = "weapon.katana";

        [Tooltip("Team 0 starts on the left, team 1 on the right.")]
        [Range(0, 1)]
        public int Team;

        [Tooltip("Control scheme of whoever drives this fighter.")]
        public ControlScheme Scheme = ControlScheme.Classic;
    }

    /// <summary>
    /// Designer-facing description of an encounter (story duel, boss fight, training session). Converted into a
    /// deterministic <see cref="CombatSetup"/> by <see cref="CreateSetup"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "DA_Match_New", menuName = "Oathsunder/Combat/Match Config", order = 1)]
    public sealed class CombatMatchConfig : ScriptableObject
    {
        /// <summary>Move set merged into every fighter before the weapon.</summary>
        public const string UniversalMoveSetId = "moveset.universal";

        [SerializeField] private string _stageId = "stage.emberfall.belltower";
        [SerializeField] private string _rulesId = "rules.story";
        [SerializeField] private List<CombatantSlot> _combatants = new List<CombatantSlot>
        {
            new CombatantSlot { Team = 0 },
            new CombatantSlot { Team = 1 },
        };

        /// <summary>Stage id.</summary>
        public string StageId => _stageId;

        /// <summary>Rules id.</summary>
        public string RulesId => _rulesId;

        /// <summary>Combatants.</summary>
        public IReadOnlyList<CombatantSlot> Combatants => _combatants;

        /// <summary>Builds the deterministic setup. Stats default to normalised; the RPG layer overrides them.</summary>
        public CombatSetup CreateSetup(CombatContentLibrary library, ulong seed)
        {
            var content = library.Load();
            var setup = new CombatSetup
            {
                Stage = content.Stage(_stageId),
                Rules = content.Rules(_rulesId),
                Tuning = content.Tuning,
                Seed = seed,
            };

            foreach (var slot in _combatants)
            {
                var blueprint = content.BuildBlueprint(slot.FighterId, UniversalMoveSetId, slot.WeaponMoveSetId);
                setup.Combatants.Add(new CombatantSetup(blueprint, slot.Team) { Scheme = slot.Scheme, Stats = CombatStats.Normalized });
            }

            return setup;
        }
    }
}

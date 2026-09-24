namespace Oathsunder.Combat.Definitions
{
    /// <summary>
    /// Combat-facing summary of a fighter's RPG build (level, attributes, gear, runes, mastery). The RPG layer
    /// computes this once per match; the simulation only ever sees these integers. Ranked PvP uses
    /// <see cref="Normalized"/>, so gear can never buy an advantage there.
    /// </summary>
    public readonly struct CombatStats
    {
        /// <summary>Outgoing damage multiplier (permille).</summary>
        public readonly int AttackPermille;

        /// <summary>Incoming damage multiplier (permille; lower is tankier).</summary>
        public readonly int DamageTakenPermille;

        /// <summary>Outgoing posture damage multiplier (permille).</summary>
        public readonly int PostureDamagePermille;

        /// <summary>Flat bonus to maximum health.</summary>
        public readonly int BonusHealth;

        /// <summary>Meter gain multiplier (permille).</summary>
        public readonly int MeterGainPermille;

        /// <summary>Creates stats.</summary>
        public CombatStats(int attackPermille, int damageTakenPermille, int postureDamagePermille, int bonusHealth, int meterGainPermille)
        {
            AttackPermille = attackPermille;
            DamageTakenPermille = damageTakenPermille;
            PostureDamagePermille = postureDamagePermille;
            BonusHealth = bonusHealth;
            MeterGainPermille = meterGainPermille;
        }

        /// <summary>Neutral stats (ranked PvP, training, tests).</summary>
        public static readonly CombatStats Normalized = new CombatStats(1000, 1000, 1000, 0, 1000);
    }
}

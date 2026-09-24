namespace Oathsunder.Combat.Definitions
{
    /// <summary>
    /// Mode-level rules. Story, Ranked, Training, Boss and Raid each ship a rules file; the combat code is the
    /// same everywhere and only these numbers differ.
    /// </summary>
    public sealed class MatchRules
    {
        /// <summary>Stable id, e.g. "rules.ranked".</summary>
        public string Id = "rules.default";

        /// <summary>Rounds a team must win to take the match.</summary>
        public int RoundsToWin = 2;

        /// <summary>Round timer in frames (0 = no timer).</summary>
        public int RoundTimerFrames = 99 * 60;

        /// <summary>Countdown before control is given ("Ready… Fight!").</summary>
        public int PreRoundFrames = 90;

        /// <summary>Frames the knockout is held before the round ends (slow-motion finisher plays here).</summary>
        public int KnockoutHoldFrames = 150;

        /// <summary>Time-scale applied to the attacker after a Perfect Dodge (permille; 1000 disables Shadow Time).</summary>
        public int PerfectDodgeSlowPermille = 350;

        /// <summary>Shadow Time duration in world frames.</summary>
        public int PerfectDodgeSlowFrames = 90;

        /// <summary>Whether executions are allowed.</summary>
        public bool ExecutionsEnabled = true;

        /// <summary>Whether Ember Rage can be activated from hitstun as a burst.</summary>
        public bool RageBurstEnabled = true;

        /// <summary>Whether RPG stats apply (false = normalized, used by ranked PvP).</summary>
        public bool UseRpgStats = true;

        /// <summary>Whether ultimate meter carries over between rounds.</summary>
        public bool CarryUltimateMeter = true;

        /// <summary>Whether members of the same team can hit each other.</summary>
        public bool FriendlyFire;
    }
}

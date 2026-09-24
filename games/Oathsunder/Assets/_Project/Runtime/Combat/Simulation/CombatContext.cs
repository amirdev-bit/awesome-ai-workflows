using Oathsunder.Combat.Definitions;
using Oathsunder.Combat.Events;
using Oathsunder.Core.Mathematics;

namespace Oathsunder.Combat.Simulation
{
    /// <summary>
    /// Shared, read-mostly context of one encounter: immutable configuration plus the mutable state and event
    /// output. Systems receive it once at construction, which keeps every system allocation-free per tick.
    /// </summary>
    internal sealed class CombatContext
    {
        public CombatContext(CombatSetup setup, CombatWorldState state, CombatEventBuffer events)
        {
            Stage = setup.Stage;
            Tuning = setup.Tuning;
            Rules = setup.Rules;
            State = state;
            Events = events;
            int count = setup.Combatants.Count;
            Blueprints = new FighterBlueprint[count];
            Stats = new CombatStats[count];
            Schemes = new ControlSchemeMask[count];
            for (int i = 0; i < count; i++)
            {
                var combatant = setup.Combatants[i];
                Blueprints[i] = combatant.Blueprint;
                Stats[i] = setup.Rules.UseRpgStats ? combatant.Stats : CombatStats.Normalized;
                Schemes[i] = combatant.Scheme == ControlScheme.Classic ? ControlSchemeMask.Classic : ControlSchemeMask.Simplified;
            }

            PreLogic = new FighterState[CombatWorldState.MaxFighters];
        }

        public StageDefinition Stage { get; }

        public CombatTuning Tuning { get; }

        public MatchRules Rules { get; }

        public CombatWorldState State { get; }

        public CombatEventBuffer Events { get; }

        public FighterBlueprint[] Blueprints { get; }

        public CombatStats[] Stats { get; }

        public ControlSchemeMask[] Schemes { get; }

        /// <summary>
        /// Copy of every fighter taken before logic runs. Cross-fighter reads during logic use it, so the order in
        /// which fighters are processed can never favour player 1 over player 2.
        /// </summary>
        public FighterState[] PreLogic { get; }

        public FighterController Controller { get; set; }

        public HitSystem Hits { get; set; }

        public ProjectileSystem Projectiles { get; set; }

        public bool IsFighting => State.Round.Phase == RoundPhase.Fighting;

        public int Frame => State.Frame;

        public void Emit(CombatEventType type, int actor, int target, int instance, int value, int value2, FixedVector2 position, CombatEventFlags flags)
        {
            Events.Add(State.Frame, type, actor, target, instance, value, value2, position, flags);
        }

        public void Emit(CombatEventType type, int actor, int target, FixedVector2 position)
        {
            Events.Add(State.Frame, type, actor, target, position);
        }

        public MoveDefinition MoveOf(int fighterIndex)
        {
            ref FighterState fighter = ref State.Fighters[fighterIndex];
            return fighter.Action == FighterAction.Move && fighter.MoveIndex >= 0 ? Blueprints[fighterIndex].Moves[fighter.MoveIndex] : null;
        }

        public void AddMeter(ref int meter, int amount, int fighterIndex)
        {
            if (amount <= 0)
            {
                return;
            }

            long scaled = (long)amount * Stats[fighterIndex].MeterGainPermille / 1000;
            long total = meter + scaled;
            meter = total > Tuning.MeterMax ? Tuning.MeterMax : (int)total;
        }

        public bool CanTarget(int attacker, int victim)
        {
            if (attacker == victim)
            {
                return false;
            }

            ref FighterState v = ref State.Fighters[victim];
            if (!v.Active)
            {
                return false;
            }

            return Rules.FriendlyFire || State.Fighters[attacker].Team != v.Team;
        }

        /// <summary>
        /// True when <paramref name="target"/> can be executed: guard broken, or low on health while staggered or
        /// in hitstun (canon §7.2).
        /// </summary>
        public bool IsExecutable(in FighterState target)
        {
            if (!target.Active || target.IsKnockedOut || !target.Grounded)
            {
                return false;
            }

            if (target.Action == FighterAction.Staggered && target.Stagger == StaggerKind.GuardBreak)
            {
                return true;
            }

            bool lowHealth = (long)target.Health * 1000 <= (long)target.MaxHealth * Tuning.ExecutionHealthPermille;
            return lowHealth && (target.Action == FighterAction.Staggered || target.Action == FighterAction.Hitstun);
        }

        public StanceBoxes StanceOf(int fighterIndex)
        {
            ref FighterState f = ref State.Fighters[fighterIndex];
            var body = Blueprints[fighterIndex].Body;
            switch (f.Action)
            {
                case FighterAction.Knockdown:
                case FighterAction.KnockedOut when f.Grounded:
                    return body.Knockdown;
            }

            if (!f.Grounded)
            {
                return body.Airborne;
            }

            switch (f.Action)
            {
                case FighterAction.Crouch:
                case FighterAction.GuardCrouch:
                    return body.Crouching;
                case FighterAction.Blockstun:
                    return f.CrouchGuard ? body.Crouching : body.Standing;
                case FighterAction.Move:
                    return Blueprints[fighterIndex].Moves[f.MoveIndex].HasFlag(MoveFlags.CrouchingHurtbox) ? body.Crouching : body.Standing;
                default:
                    return body.Standing;
            }
        }

        public FixedAabb PushboxOf(int fighterIndex)
        {
            ref FighterState f = ref State.Fighters[fighterIndex];
            return StanceOf(fighterIndex).Pushbox.ToWorld(f.Position, f.Facing);
        }

        /// <summary>Writes the fighter's world-space hurtboxes into <paramref name="buffer"/> and returns the count.</summary>
        public int GetHurtboxes(int fighterIndex, FixedAabb[] buffer)
        {
            ref FighterState f = ref State.Fighters[fighterIndex];
            var stance = StanceOf(fighterIndex);
            int count = 0;
            var move = MoveOf(fighterIndex);
            if (move != null)
            {
                foreach (var window in move.Hurtboxes)
                {
                    if (window.Replace && window.Window.Contains(f.ActionFrame))
                    {
                        foreach (var box in window.Boxes)
                        {
                            if (count < buffer.Length)
                            {
                                buffer[count++] = box.ToWorld(f.Position, f.Facing);
                            }
                        }

                        return count;
                    }
                }
            }

            foreach (var box in stance.Hurtboxes)
            {
                if (count < buffer.Length)
                {
                    buffer[count++] = box.ToWorld(f.Position, f.Facing);
                }
            }

            if (move != null)
            {
                foreach (var window in move.Hurtboxes)
                {
                    if (!window.Replace && window.Window.Contains(f.ActionFrame))
                    {
                        foreach (var box in window.Boxes)
                        {
                            if (count < buffer.Length)
                            {
                                buffer[count++] = box.ToWorld(f.Position, f.Facing);
                            }
                        }
                    }
                }
            }

            return count;
        }

        /// <summary>True when the fighter's current move makes it immune to <paramref name="mask"/>.</summary>
        public bool IsMoveInvulnerable(int fighterIndex, InvulnerabilityMask mask)
        {
            var move = MoveOf(fighterIndex);
            if (move == null)
            {
                return false;
            }

            int frame = State.Fighters[fighterIndex].ActionFrame;
            foreach (var window in move.Invulnerability)
            {
                if ((window.Mask & mask) != 0 && window.Window.Contains(frame))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

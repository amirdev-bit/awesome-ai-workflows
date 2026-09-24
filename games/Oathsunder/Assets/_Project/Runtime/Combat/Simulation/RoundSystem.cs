using Oathsunder.Combat.Events;
using Oathsunder.Core.Mathematics;

namespace Oathsunder.Combat.Simulation
{
    /// <summary>Round flow: countdown, fight, knockout / time-over, round reset and match resolution.</summary>
    internal sealed class RoundSystem
    {
        private const int Draw = -2;
        private const int Undecided = -1;

        private readonly CombatContext _ctx;

        public RoundSystem(CombatContext context)
        {
            _ctx = context;
        }

        /// <summary>Initialises round 1 and places the fighters.</summary>
        public void Begin()
        {
            ref RoundState round = ref _ctx.State.Round;
            round.Phase = RoundPhase.PreRound;
            round.PhaseFrame = 0;
            round.RoundNumber = 1;
            round.TimerFrames = _ctx.Rules.RoundTimerFrames;
            round.Team0Wins = 0;
            round.Team1Wins = 0;
            round.RoundWinner = Undecided;
            round.MatchWinner = Undecided;
            ResetFighters(firstRound: true);
        }

        /// <summary>Runs before inputs are processed: advances the countdown.</summary>
        public void PreStep()
        {
            ref RoundState round = ref _ctx.State.Round;
            if (round.Phase != RoundPhase.PreRound)
            {
                return;
            }

            if (round.PhaseFrame == 0)
            {
                _ctx.Emit(CombatEventType.RoundStart, -1, -1, round.RoundNumber, round.RoundNumber, 0, FixedVector2.Zero, CombatEventFlags.None);
            }

            round.PhaseFrame++;
            if (round.PhaseFrame > _ctx.Rules.PreRoundFrames)
            {
                round.Phase = RoundPhase.Fighting;
                round.PhaseFrame = 0;
                _ctx.Emit(CombatEventType.RoundFight, -1, -1, round.RoundNumber, round.RoundNumber, 0, FixedVector2.Zero, CombatEventFlags.None);
            }
        }

        /// <summary>Runs after combat resolution: knockouts, timer and transitions.</summary>
        public void PostStep()
        {
            ref RoundState round = ref _ctx.State.Round;
            switch (round.Phase)
            {
                case RoundPhase.Fighting:
                    round.PhaseFrame++;
                    bool team0Alive = TeamAlive(0);
                    bool team1Alive = TeamAlive(1);
                    if (!team0Alive || !team1Alive)
                    {
                        EndRound(team0Alive ? 0 : (team1Alive ? 1 : Draw));
                        return;
                    }

                    if (_ctx.Rules.RoundTimerFrames > 0)
                    {
                        round.TimerFrames--;
                        if (round.TimerFrames <= 0)
                        {
                            round.TimerFrames = 0;
                            _ctx.Emit(CombatEventType.TimeOver, -1, -1, FixedVector2.Zero);
                            EndRound(WinnerByHealth());
                        }
                    }

                    break;
                case RoundPhase.RoundEnding:
                    round.PhaseFrame++;
                    if (round.PhaseFrame >= _ctx.Rules.KnockoutHoldFrames)
                    {
                        FinishRound();
                    }

                    break;
            }
        }

        private void EndRound(int winner)
        {
            ref RoundState round = ref _ctx.State.Round;
            round.Phase = RoundPhase.RoundEnding;
            round.PhaseFrame = 0;
            round.RoundWinner = winner;
            _ctx.Emit(CombatEventType.RoundEnd, -1, -1, round.RoundNumber, winner, round.RoundNumber, FixedVector2.Zero, CombatEventFlags.None);
        }

        private void FinishRound()
        {
            ref RoundState round = ref _ctx.State.Round;
            if (round.RoundWinner == 0)
            {
                round.Team0Wins++;
            }
            else if (round.RoundWinner == 1)
            {
                round.Team1Wins++;
            }

            int needed = _ctx.Rules.RoundsToWin;
            int maxRounds = needed * 2 + 1;
            if (round.Team0Wins >= needed || round.Team1Wins >= needed || round.RoundNumber >= maxRounds)
            {
                round.Phase = RoundPhase.MatchOver;
                round.PhaseFrame = 0;
                round.MatchWinner = round.Team0Wins >= needed ? 0 : (round.Team1Wins >= needed ? 1 : Draw);
                _ctx.Emit(CombatEventType.MatchEnd, -1, -1, 0, round.MatchWinner, round.RoundNumber, FixedVector2.Zero, CombatEventFlags.None);
                return;
            }

            round.RoundNumber++;
            round.Phase = RoundPhase.PreRound;
            round.PhaseFrame = 0;
            round.TimerFrames = _ctx.Rules.RoundTimerFrames;
            round.RoundWinner = Undecided;
            ResetFighters(firstRound: false);
        }

        private bool TeamAlive(int team)
        {
            var state = _ctx.State;
            for (int i = 0; i < state.FighterCount; i++)
            {
                ref FighterState f = ref state.Fighters[i];
                if (f.Active && f.Team == team && !f.IsKnockedOut)
                {
                    return true;
                }
            }

            return false;
        }

        private int WinnerByHealth()
        {
            long team0 = 0;
            long team1 = 0;
            var state = _ctx.State;
            for (int i = 0; i < state.FighterCount; i++)
            {
                ref FighterState f = ref state.Fighters[i];
                if (!f.Active)
                {
                    continue;
                }

                long permille = (long)f.Health * 1000 / (f.MaxHealth > 0 ? f.MaxHealth : 1);
                if (f.Team == 0)
                {
                    team0 += permille;
                }
                else
                {
                    team1 += permille;
                }
            }

            return team0 > team1 ? 0 : (team1 > team0 ? 1 : Draw);
        }

        /// <summary>Places fighters on their marks and restores per-round resources.</summary>
        public void ResetFighters(bool firstRound)
        {
            var state = _ctx.State;
            _ctx.Projectiles.Clear();
            for (int e = 0; e < state.Echoes.Length; e++)
            {
                state.Echoes[e] = default;
            }

            int team0Slot = 0;
            int team1Slot = 0;
            for (int i = 0; i < state.FighterCount; i++)
            {
                ref FighterState f = ref state.Fighters[i];
                var body = _ctx.Blueprints[i].Body;
                int facing = f.Team == 0 ? 1 : -1;
                int slot = f.Team == 0 ? team0Slot++ : team1Slot++;
                Fixed x = (_ctx.Stage.SpawnOffset + Fixed.FromInt(slot)) * -facing;

                int ultimate = firstRound || !_ctx.Rules.CarryUltimateMeter ? 0 : f.Ultimate;
                int shadow = firstRound || !_ctx.Rules.CarryUltimateMeter ? 0 : f.Shadow;
                int localFrame = f.LocalFrame;
                int lastLogic = f.LastLogicWorldFrame;
                int team = f.Team;

                f = default;
                f.Active = true;
                f.Team = team;
                f.TargetIndex = -1;
                f.Position = new FixedVector2(x, Fixed.Zero);
                f.Facing = facing;
                f.Grounded = true;
                f.MoveGravityPermille = 1000;
                f.Action = FighterAction.Idle;
                f.MoveIndex = -1;
                f.LocalFrame = localFrame;
                f.LastLogicWorldFrame = lastLogic;
                f.ParryWindowEndLocal = int.MinValue / 2;
                f.LastGuardPressLocal = int.MinValue / 2;
                f.MaxHealth = body.MaxHealth + _ctx.Stats[i].BonusHealth;
                f.Health = f.MaxHealth;
                f.MaxPosture = body.MaxPosture;
                f.Ultimate = ultimate;
                f.Shadow = shadow;
                f.ComboStarterPermille = 1000;
                f.ComboAttacker = -1;
                f.LastAttackerIndex = -1;
                f.PairedPartner = -1;
                f.TimeScalePermille = 1000;

                // Drop anything buffered during the previous round.
                state.Inputs[i].ConsumeThrough(state.Inputs[i].LatestFrame);
            }
        }
    }
}

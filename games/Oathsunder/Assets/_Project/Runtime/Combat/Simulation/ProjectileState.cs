using Oathsunder.Core.Hashing;
using Oathsunder.Core.Mathematics;

namespace Oathsunder.Combat.Simulation
{
    /// <summary>State of one live projectile (value type; part of the rollback state).</summary>
    public struct ProjectileState
    {
        /// <summary>Slot in use.</summary>
        public bool Active;

        /// <summary>Owner fighter index.</summary>
        public int Owner;

        /// <summary>Owner team.</summary>
        public int Team;

        /// <summary>Projectile index in the owner's blueprint.</summary>
        public int DefinitionIndex;

        /// <summary>Unique instance id.</summary>
        public int Instance;

        /// <summary>Position.</summary>
        public FixedVector2 Position;

        /// <summary>Velocity (world space).</summary>
        public FixedVector2 Velocity;

        /// <summary>+1 / -1.</summary>
        public int Facing;

        /// <summary>Frames alive.</summary>
        public int Age;

        /// <summary>Remaining durability.</summary>
        public int HitsRemaining;

        /// <summary>Bit per victim index already hit.</summary>
        public uint HitRegistry;

        /// <summary>Frames until the registry clears (rehit interval).</summary>
        public int RehitTimer;

        /// <summary>Remaining hitstop frames.</summary>
        public int HitstopRemaining;

        /// <summary>Adds every field to a checksum.</summary>
        public void AppendHash(ref StateHasher h)
        {
            h.Add(Active);
            h.Add(Owner);
            h.Add(Team);
            h.Add(DefinitionIndex);
            h.Add(Instance);
            h.Add(Position);
            h.Add(Velocity);
            h.Add(Facing);
            h.Add(Age);
            h.Add(HitsRemaining);
            h.Add(HitRegistry);
            h.Add(RehitTimer);
            h.Add(HitstopRemaining);
        }
    }

    /// <summary>A delayed Umbral Shadow echo strike (value type; part of the rollback state).</summary>
    public struct EchoHitState
    {
        /// <summary>Slot in use.</summary>
        public bool Active;

        /// <summary>World frame on which the echo lands.</summary>
        public int TriggerFrame;

        /// <summary>Attacker fighter index.</summary>
        public int Attacker;

        /// <summary>Victim fighter index.</summary>
        public int Victim;

        /// <summary>Damage.</summary>
        public int Damage;

        /// <summary>Posture damage.</summary>
        public int PostureDamage;

        /// <summary>Contact point for presentation.</summary>
        public FixedVector2 Position;

        /// <summary>Adds every field to a checksum.</summary>
        public void AppendHash(ref StateHasher h)
        {
            h.Add(Active);
            h.Add(TriggerFrame);
            h.Add(Attacker);
            h.Add(Victim);
            h.Add(Damage);
            h.Add(PostureDamage);
            h.Add(Position);
        }
    }

    /// <summary>Round and match progress (value type; part of the rollback state).</summary>
    public struct RoundState
    {
        /// <summary>Phase.</summary>
        public RoundPhase Phase;

        /// <summary>Frames spent in the phase.</summary>
        public int PhaseFrame;

        /// <summary>1-based round number.</summary>
        public int RoundNumber;

        /// <summary>Remaining round time in frames (ignored when the rules have no timer).</summary>
        public int TimerFrames;

        /// <summary>Round wins of team 0.</summary>
        public int Team0Wins;

        /// <summary>Round wins of team 1.</summary>
        public int Team1Wins;

        /// <summary>Winner of the current round once decided: 0, 1, -1 undecided, -2 draw.</summary>
        public int RoundWinner;

        /// <summary>Winner of the match once over: 0, 1, -1 undecided, -2 draw.</summary>
        public int MatchWinner;

        /// <summary>Adds every field to a checksum.</summary>
        public void AppendHash(ref StateHasher h)
        {
            h.Add((int)Phase);
            h.Add(PhaseFrame);
            h.Add(RoundNumber);
            h.Add(TimerFrames);
            h.Add(Team0Wins);
            h.Add(Team1Wins);
            h.Add(RoundWinner);
            h.Add(MatchWinner);
        }
    }
}

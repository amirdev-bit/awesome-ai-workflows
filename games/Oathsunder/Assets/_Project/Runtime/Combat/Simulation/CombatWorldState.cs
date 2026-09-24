using System;
using Oathsunder.Combat.Input;
using Oathsunder.Core.Hashing;
using Oathsunder.Core.Random;

namespace Oathsunder.Combat.Simulation
{
    /// <summary>
    /// The entire rollback-relevant state of a combat encounter. Preallocated, fixed-size and reference-free
    /// except for the input histories, so saving and restoring is a handful of array copies with zero allocation.
    /// </summary>
    public sealed class CombatWorldState
    {
        /// <summary>Maximum fighters per encounter (1v1 duels, 2v2 clan wars, 1v3 story gauntlets).</summary>
        public const int MaxFighters = 4;

        /// <summary>Maximum simultaneous projectiles.</summary>
        public const int MaxProjectiles = 16;

        /// <summary>Maximum pending shadow echoes.</summary>
        public const int MaxEchoes = 16;

        /// <summary>Creates an empty state.</summary>
        public CombatWorldState()
        {
            for (int i = 0; i < MaxFighters; i++)
            {
                Inputs[i] = new InputHistory();
            }
        }

        /// <summary>Last simulated world frame (0 before the first step).</summary>
        public int Frame;

        /// <summary>Number of fighters in use (slots 0..FighterCount-1).</summary>
        public int FighterCount;

        /// <summary>Round progress.</summary>
        public RoundState Round;

        /// <summary>Deterministic random stream (reserved for PvE variance; unused in PvP rules).</summary>
        public Pcg32 Random;

        /// <summary>Last issued move/projectile instance id.</summary>
        public int LastInstanceId;

        /// <summary>Fighters.</summary>
        public readonly FighterState[] Fighters = new FighterState[MaxFighters];

        /// <summary>Per-fighter input histories.</summary>
        public readonly InputHistory[] Inputs = new InputHistory[MaxFighters];

        /// <summary>Projectiles.</summary>
        public readonly ProjectileState[] Projectiles = new ProjectileState[MaxProjectiles];

        /// <summary>Pending shadow echoes.</summary>
        public readonly EchoHitState[] Echoes = new EchoHitState[MaxEchoes];

        /// <summary>Copies another state into this one without allocating.</summary>
        public void CopyFrom(CombatWorldState other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Frame = other.Frame;
            FighterCount = other.FighterCount;
            Round = other.Round;
            Random = other.Random;
            LastInstanceId = other.LastInstanceId;
            Array.Copy(other.Fighters, Fighters, MaxFighters);
            Array.Copy(other.Projectiles, Projectiles, MaxProjectiles);
            Array.Copy(other.Echoes, Echoes, MaxEchoes);
            for (int i = 0; i < MaxFighters; i++)
            {
                Inputs[i].CopyFrom(other.Inputs[i]);
            }
        }

        /// <summary>64-bit checksum of the whole state (desync detection, replay validation, sync tests).</summary>
        public ulong ComputeChecksum()
        {
            var h = StateHasher.Create();
            h.Add(Frame);
            h.Add(FighterCount);
            Round.AppendHash(ref h);
            h.Add(Random.State);
            h.Add(Random.Increment);
            h.Add(LastInstanceId);
            for (int i = 0; i < MaxFighters; i++)
            {
                Fighters[i].AppendHash(ref h);
                Inputs[i].AppendHash(ref h);
            }

            for (int i = 0; i < MaxProjectiles; i++)
            {
                Projectiles[i].AppendHash(ref h);
            }

            for (int i = 0; i < MaxEchoes; i++)
            {
                Echoes[i].AppendHash(ref h);
            }

            return h.Finish();
        }

        /// <summary>Issues a new unique instance id.</summary>
        public int NextInstanceId() => ++LastInstanceId;
    }
}

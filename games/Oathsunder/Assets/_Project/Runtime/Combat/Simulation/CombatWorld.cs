using System;
using Oathsunder.Combat.Definitions;
using Oathsunder.Combat.Events;
using Oathsunder.Combat.Input;
using Oathsunder.Core.Mathematics;
using Oathsunder.Core.Random;

namespace Oathsunder.Combat.Simulation
{
    /// <summary>
    /// Public facade of the deterministic combat simulation. One instance runs one encounter at a fixed 60 Hz.
    /// </summary>
    /// <remarks>
    /// <para><b>Contract.</b> Given the same <see cref="CombatSetup"/> and the same sequence of inputs, every call
    /// to <see cref="Step"/> produces bit-identical state on every platform. There is no floating point, no
    /// wall-clock time, no unordered iteration and no allocation after construction.</para>
    /// <para><b>Rollback.</b> <see cref="SaveState"/> / <see cref="LoadState"/> copy the whole state into a
    /// preallocated <see cref="CombatWorldState"/>; <see cref="Checksum"/> hashes it for desync detection.</para>
    /// <para><b>Tick order.</b> round countdown → record inputs → targets → time dilation and hitstop →
    /// fighter logic → integrate → bounds and bodies → paired locks → facing → projectiles → contacts → meters → round result.</para>
    /// </remarks>
    public sealed class CombatWorld
    {
        private readonly CombatContext _ctx;
        private readonly FighterController _controller;
        private readonly PhysicsSystem _physics;
        private readonly HitSystem _hits;
        private readonly ProjectileSystem _projectiles;
        private readonly ResourceSystem _resources;
        private readonly RoundSystem _round;
        private readonly int[] _forcedMoves = NoForcedMoves();

        /// <summary>Creates an encounter.</summary>
        /// <exception cref="InvalidOperationException">The setup is invalid.</exception>
        public CombatWorld(CombatSetup setup)
        {
            if (setup == null)
            {
                throw new ArgumentNullException(nameof(setup));
            }

            setup.Validate();
            State = new CombatWorldState();
            Events = new CombatEventBuffer();
            _ctx = new CombatContext(setup, State, Events);
            _controller = new FighterController(_ctx);
            _physics = new PhysicsSystem(_ctx);
            _hits = new HitSystem(_ctx);
            _projectiles = new ProjectileSystem(_ctx);
            _resources = new ResourceSystem(_ctx);
            _round = new RoundSystem(_ctx);
            _ctx.Controller = _controller;
            _ctx.Hits = _hits;
            _ctx.Projectiles = _projectiles;

            State.FighterCount = setup.Combatants.Count;
            State.Random = new Pcg32(setup.Seed);
            for (int i = 0; i < State.FighterCount; i++)
            {
                State.Fighters[i].Team = setup.Combatants[i].Team;
            }

            _round.Begin();
            UpdateTargets();
            Setup = setup;
        }

        /// <summary>The setup this encounter was created from.</summary>
        public CombatSetup Setup { get; }

        /// <summary>Live state. Treat as read-only outside of rollback tooling.</summary>
        public CombatWorldState State { get; }

        /// <summary>Events produced by the most recent <see cref="Step"/>.</summary>
        public CombatEventBuffer Events { get; }

        /// <summary>Number of fighters.</summary>
        public int FighterCount => State.FighterCount;

        /// <summary>Resolved blueprint of a fighter.</summary>
        public FighterBlueprint BlueprintOf(int fighterIndex) => _ctx.Blueprints[fighterIndex];

        /// <summary>Contacts dropped because the per-frame contact buffer was full (should always be 0).</summary>
        public int DroppedContacts => _hits.DroppedContacts;

        /// <summary>Projectiles not spawned because the pool was full (should always be 0).</summary>
        public int DroppedProjectileSpawns => _projectiles.DroppedSpawns;

        /// <summary>
        /// Writes the fighter's world-space hurtboxes for the current frame (stance boxes plus the active move's
        /// added or replacing boxes, exactly as contact detection sees them) and returns how many were written.
        /// Used by debug drawing and production tooling.
        /// </summary>
        public int GetHurtboxes(int fighterIndex, FixedAabb[] buffer) => _ctx.GetHurtboxes(fighterIndex, buffer);

        /// <summary>True when the fighter's current move frame is immune to any of <paramref name="mask"/>.</summary>
        public bool IsInvulnerable(int fighterIndex, InvulnerabilityMask mask) => _ctx.IsMoveInvulnerable(fighterIndex, mask);

        /// <summary>
        /// Queues a move to start on the fighter's next simulated frame, bypassing input, meter and cancel rules.
        /// For tooling and training-mode scenarios (animation capture, dummy playback); match flow never calls it.
        /// The request is not part of <see cref="CombatWorldState"/>: it is consumed by the next <see cref="Step"/>
        /// and cleared by <see cref="LoadState"/>, so rollback sessions must not use it.
        /// </summary>
        /// <exception cref="ArgumentException">The fighter does not have the move.</exception>
        public void ForceMove(int fighterIndex, string moveId)
        {
            if (fighterIndex < 0 || fighterIndex >= State.FighterCount)
            {
                throw new ArgumentOutOfRangeException(nameof(fighterIndex));
            }

            int moveIndex = _ctx.Blueprints[fighterIndex].FindMoveIndex(moveId);
            if (moveIndex < 0)
            {
                throw new ArgumentException($"Fighter {fighterIndex} has no move '{moveId}'.", nameof(moveId));
            }

            _forcedMoves[fighterIndex] = moveIndex;
        }

        /// <summary>Advances the simulation by one 60 Hz frame.</summary>
        /// <param name="inputs">One input per fighter, indexed like the setup's combatants.</param>
        /// <exception cref="ArgumentException">The number of inputs does not match the number of fighters.</exception>
        public void Step(InputFrame[] inputs)
        {
            if (inputs == null || inputs.Length < State.FighterCount)
            {
                throw new ArgumentException($"Expected {State.FighterCount} inputs.", nameof(inputs));
            }

            Events.Clear();
            State.Frame++;
            _round.PreStep();

            if (State.Round.Phase == RoundPhase.MatchOver)
            {
                for (int i = 0; i < State.FighterCount; i++)
                {
                    State.Inputs[i].Record(State.Frame, State.Fighters[i].LocalFrame, inputs[i]);
                }

                return;
            }

            UpdateTargets();
            AdvanceClocks(inputs);

            Array.Copy(State.Fighters, _ctx.PreLogic, State.FighterCount);
            for (int i = 0; i < State.FighterCount; i++)
            {
                if (State.Fighters[i].Active && State.Fighters[i].AdvancedThisFrame)
                {
                    if (_forcedMoves[i] >= 0)
                    {
                        _controller.ForceStart(i, _forcedMoves[i]);
                        _forcedMoves[i] = -1;
                    }
                    else
                    {
                        _controller.Update(i);
                    }
                }
            }

            _physics.Integrate();
            _physics.ResolveBounds();
            _physics.SnapPairedVictims();
            _physics.UpdateFacing();
            _projectiles.Update();
            _hits.Run();
            _resources.Update();
            _round.PostStep();
        }

        /// <summary>Copies the current state into <paramref name="target"/> (no allocation).</summary>
        public void SaveState(CombatWorldState target) => target.CopyFrom(State);

        /// <summary>Restores a previously saved state (no allocation).</summary>
        public void LoadState(CombatWorldState source)
        {
            State.CopyFrom(source);
            for (int i = 0; i < _forcedMoves.Length; i++)
            {
                _forcedMoves[i] = -1;
            }
        }

        /// <summary>64-bit checksum of the current state.</summary>
        public ulong Checksum() => State.ComputeChecksum();

        private static int[] NoForcedMoves()
        {
            var moves = new int[CombatWorldState.MaxFighters];
            for (int i = 0; i < moves.Length; i++)
            {
                moves[i] = -1;
            }

            return moves;
        }

        private void AdvanceClocks(InputFrame[] inputs)
        {
            for (int i = 0; i < State.FighterCount; i++)
            {
                ref FighterState f = ref State.Fighters[i];
                f.AdvancedThisFrame = false;
                if (f.Active)
                {
                    bool tick = true;
                    if (f.TimeScalePermille < 1000)
                    {
                        f.TimeAccumulator += f.TimeScalePermille;
                        tick = f.TimeAccumulator >= 1000;
                        if (tick)
                        {
                            f.TimeAccumulator -= 1000;
                        }
                    }

                    if (tick)
                    {
                        if (f.HitstopRemaining > 0)
                        {
                            f.HitstopRemaining--;
                        }
                        else
                        {
                            f.LocalFrame++;
                            f.AdvancedThisFrame = true;
                        }
                    }
                }

                State.Inputs[i].Record(State.Frame, f.LocalFrame, inputs[i]);
            }
        }

        private void UpdateTargets()
        {
            for (int i = 0; i < State.FighterCount; i++)
            {
                ref FighterState f = ref State.Fighters[i];
                int best = -1;
                Fixed bestDistance = Fixed.MaxValue;
                for (int j = 0; j < State.FighterCount; j++)
                {
                    ref FighterState other = ref State.Fighters[j];
                    if (j == i || !other.Active || other.Team == f.Team || other.IsKnockedOut)
                    {
                        continue;
                    }

                    Fixed distance = Fixed.Abs(other.Position.X - f.Position.X);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        best = j;
                    }
                }

                if (best >= 0 || f.TargetIndex < 0 || State.Fighters[f.TargetIndex].IsKnockedOut)
                {
                    f.TargetIndex = best;
                }
            }
        }
    }
}

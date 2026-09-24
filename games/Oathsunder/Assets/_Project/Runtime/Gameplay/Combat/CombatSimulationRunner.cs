using System;
using System.Collections.Generic;
using Oathsunder.Combat.Events;
using Oathsunder.Combat.Input;
using Oathsunder.Combat.Simulation;
using Oathsunder.Core.Mathematics;
using UnityEngine;

namespace Oathsunder.Gameplay.Combat
{
    /// <summary>Receives combat events after each simulation tick.</summary>
    public interface ICombatEventListener
    {
        /// <summary>Called once per event, in emission order, after the tick that produced it.</summary>
        void OnCombatEvent(CombatSimulationRunner runner, in CombatEvent combatEvent);
    }

    /// <summary>
    /// Drives a <see cref="CombatWorld"/> at a fixed 60 Hz from Unity's variable frame rate and exposes an
    /// interpolation factor so presentation renders smoothly at 90/120/240 FPS.
    /// </summary>
    /// <remarks>
    /// <para>Uses unscaled time: Unity's <c>Time.timeScale</c> never touches the simulation. Offline modes may
    /// set <see cref="TickRateScale"/> for cinematic slow motion; networked sessions must leave it at 1 because
    /// both peers have to tick at the same rate.</para>
    /// <para>At most <see cref="MaxTicksPerFrame"/> ticks run per rendered frame so a hitch cannot spiral.</para>
    /// </remarks>
    [DefaultExecutionOrder(-100)]
    public sealed class CombatSimulationRunner : MonoBehaviour
    {
        /// <summary>Simulation rate.</summary>
        public const int TicksPerSecond = 60;

        /// <summary>Seconds per tick.</summary>
        public const double TickDuration = 1.0 / TicksPerSecond;

        /// <summary>Upper bound of catch-up ticks per rendered frame.</summary>
        public const int MaxTicksPerFrame = 4;

        [SerializeField] private CombatContentLibrary _content;
        [SerializeField] private CombatMatchConfig _match;
        [SerializeField] private bool _startOnAwake = true;
        [SerializeField] private ulong _seed = 0xC0FFEE;

        private readonly List<ICombatEventListener> _listeners = new List<ICombatEventListener>();
        private readonly ICombatInputSource[] _sources = new ICombatInputSource[CombatWorldState.MaxFighters];
        private InputFrame[] _inputs = new InputFrame[0];
        private FixedVector2[] _previousPositions = new FixedVector2[0];
        private double _accumulator;

        /// <summary>The running encounter (null before <see cref="StartMatch"/>).</summary>
        public CombatWorld World { get; private set; }

        /// <summary>Render interpolation between the previous and current tick, 0..1.</summary>
        public float Alpha { get; private set; }

        /// <summary>Simulation speed multiplier for offline cinematic slow motion (1 = real time).</summary>
        public float TickRateScale { get; set; } = 1f;

        /// <summary>Pauses ticking (pause menu, cutscenes); presentation keeps rendering the last tick.</summary>
        public bool Paused { get; set; }

        /// <summary>Raised after every tick (after events were dispatched).</summary>
        public event Action<CombatSimulationRunner> Ticked;

        private void Awake()
        {
            if (_startOnAwake && _content != null && _match != null)
            {
                StartMatch(_match.CreateSetup(_content, _seed));
            }
        }

        /// <summary>Starts a new encounter.</summary>
        public void StartMatch(CombatSetup setup)
        {
            World = new CombatWorld(setup);
            _inputs = new InputFrame[World.FighterCount];
            _previousPositions = new FixedVector2[World.FighterCount];
            CapturePositions();
            _accumulator = 0;
            Alpha = 0;
        }

        /// <summary>Assigns who controls a fighter (player, AI, network, replay).</summary>
        public void SetInputSource(int fighterIndex, ICombatInputSource source)
        {
            _sources[fighterIndex] = source ?? NeutralInputSource.Instance;
        }

        /// <summary>Registers an event listener.</summary>
        public void AddListener(ICombatEventListener listener)
        {
            if (listener != null && !_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        /// <summary>Unregisters an event listener.</summary>
        public void RemoveListener(ICombatEventListener listener) => _listeners.Remove(listener);

        /// <summary>Render position of a fighter, interpolated between ticks.</summary>
        public Vector3 GetRenderPosition(int fighterIndex, float depth = 0f) =>
            FixedConversions.Interpolate(_previousPositions[fighterIndex], World.State.Fighters[fighterIndex].Position, Alpha, depth);

        /// <summary>Advances exactly one tick (also used by tests and frame-step debugging).</summary>
        public void Tick()
        {
            if (World == null)
            {
                return;
            }

            CapturePositions();
            int nextFrame = World.State.Frame + 1;
            for (int i = 0; i < _inputs.Length; i++)
            {
                var source = _sources[i] ?? NeutralInputSource.Instance;
                _inputs[i] = source.Sample(i, nextFrame);
            }

            World.Step(_inputs);
            var events = World.Events;
            for (int e = 0; e < events.Count; e++)
            {
                var combatEvent = events[e];
                for (int l = 0; l < _listeners.Count; l++)
                {
                    _listeners[l].OnCombatEvent(this, combatEvent);
                }
            }

            Ticked?.Invoke(this);
        }

        private void Update()
        {
            if (World == null || Paused)
            {
                return;
            }

            _accumulator += Time.unscaledDeltaTime * Mathf.Max(0f, TickRateScale);
            int ticks = 0;
            while (_accumulator >= TickDuration && ticks < MaxTicksPerFrame)
            {
                Tick();
                _accumulator -= TickDuration;
                ticks++;
            }

            if (ticks == MaxTicksPerFrame && _accumulator > TickDuration)
            {
                // Too far behind (breakpoint, hitch): drop the backlog instead of fast-forwarding the fight.
                _accumulator = 0;
            }

            Alpha = (float)(_accumulator / TickDuration);
        }

        private void CapturePositions()
        {
            for (int i = 0; i < _previousPositions.Length; i++)
            {
                _previousPositions[i] = World.State.Fighters[i].Position;
            }
        }
    }
}

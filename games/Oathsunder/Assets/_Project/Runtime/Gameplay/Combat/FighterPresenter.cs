using System.Collections.Generic;
using Oathsunder.Combat.Definitions;
using Oathsunder.Combat.Simulation;
using UnityEngine;

namespace Oathsunder.Gameplay.Combat
{
    /// <summary>
    /// Renders one simulated fighter: interpolated transform, facing, and a pose that is <b>frame-locked</b> to
    /// the simulation (the Animator never advances on its own). Frame locking is what makes hitstop, rollback
    /// and replays look exact: the pose is a pure function of the simulation state.
    /// </summary>
    /// <remarks>
    /// Animator state names follow the canon: a move plays the state named by its <c>animation</c> key
    /// (e.g. <c>A_Katana_L1</c>); non-move actions play <c>A_Fighter_{Action}</c> (e.g. <c>A_Fighter_Idle</c>).
    /// Phase 7 replaces the Animator path with the Playables-based animation graph; this component keeps the
    /// same public contract.
    /// </remarks>
    public sealed class FighterPresenter : MonoBehaviour
    {
        private const int BaseLayer = 0;

        [SerializeField] private CombatSimulationRunner _runner;
        [SerializeField, Min(0)] private int _fighterIndex;
        [SerializeField] private Animator _animator;
        [Tooltip("Depth of the combat plane in world space.")]
        [SerializeField] private float _planeDepth;
        [Tooltip("Model yaw when facing right (degrees). Facing left mirrors it.")]
        [SerializeField] private float _rightFacingYaw = 90f;
        [Tooltip("Frames used to turn around visually (0 = instant).")]
        [SerializeField, Range(0f, 8f)] private float _turnFrames = 3f;
        [Tooltip("Frame length of looping locomotion states (idle, walk).")]
        [SerializeField, Min(1)] private int _loopFrames = 60;

        private readonly Dictionary<string, int> _stateHashes = new Dictionary<string, int>();
        private float _yaw;

        /// <summary>Index of the fighter this presenter renders.</summary>
        public int FighterIndex => _fighterIndex;

        /// <summary>Binds the presenter at runtime (spawned fighters).</summary>
        public void Bind(CombatSimulationRunner runner, int fighterIndex, Animator animator)
        {
            _runner = runner;
            _fighterIndex = fighterIndex;
            _animator = animator;
            PrepareAnimator();
        }

        private void Awake()
        {
            PrepareAnimator();
            _yaw = _rightFacingYaw;
        }

        private void PrepareAnimator()
        {
            if (_animator != null)
            {
                _animator.speed = 0f;
                _animator.applyRootMotion = false;
            }
        }

        private void LateUpdate()
        {
            if (_runner == null || _runner.World == null || _fighterIndex >= _runner.World.FighterCount)
            {
                return;
            }

            ref FighterState fighter = ref _runner.World.State.Fighters[_fighterIndex];
            transform.position = _runner.GetRenderPosition(_fighterIndex, _planeDepth);

            float targetYaw = fighter.Facing >= 0 ? _rightFacingYaw : -_rightFacingYaw;
            float step = _turnFrames <= 0f ? 360f : 180f / _turnFrames * Time.deltaTime * CombatSimulationRunner.TicksPerSecond;
            _yaw = Mathf.MoveTowardsAngle(_yaw, targetYaw, step);
            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);

            if (_animator != null && _animator.isActiveAndEnabled)
            {
                ApplyPose(ref fighter);
            }
        }

        private void ApplyPose(ref FighterState fighter)
        {
            string state;
            float normalizedTime;
            if (fighter.Action == FighterAction.Move && fighter.MoveIndex >= 0)
            {
                MoveDefinition move = _runner.World.BlueprintOf(_fighterIndex).Moves[fighter.MoveIndex];
                state = string.IsNullOrEmpty(move.Animation) ? move.Id : move.Animation;
                float frame = Mathf.Max(0, fighter.ActionFrame - 1) + (fighter.HitstopRemaining > 0 ? 0f : _runner.Alpha);
                normalizedTime = Mathf.Clamp01(frame / Mathf.Max(1, move.TotalFrames));
            }
            else
            {
                state = ActionStateNames.Get(fighter.Action);
                normalizedTime = (fighter.ActionFrame % _loopFrames) / (float)_loopFrames;
            }

            _animator.Play(Hash(state), BaseLayer, normalizedTime);
            _animator.Update(0f);
        }

        private int Hash(string state)
        {
            if (!_stateHashes.TryGetValue(state, out int hash))
            {
                hash = Animator.StringToHash(state);
                _stateHashes.Add(state, hash);
            }

            return hash;
        }

        /// <summary>Animator state names for non-move actions (precomputed; no per-frame allocation).</summary>
        private static class ActionStateNames
        {
            private static readonly string[] Names = Build();

            public static string Get(FighterAction action) => Names[(int)action];

            private static string[] Build()
            {
                var values = (FighterAction[])System.Enum.GetValues(typeof(FighterAction));
                var names = new string[values.Length];
                foreach (var value in values)
                {
                    names[(int)value] = "A_Fighter_" + value;
                }

                return names;
            }
        }
    }
}

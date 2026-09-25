using System.Collections.Generic;
using Oathsunder.Combat.Definitions;
using Oathsunder.Combat.Simulation;
using Oathsunder.Presentation.Animation;
using Oathsunder.Presentation.Cues;
using UnityEngine;

namespace Oathsunder.Gameplay.Combat
{
    /// <summary>
    /// Renders one simulated fighter: interpolated transform, facing, and a pose that is <b>frame-locked</b> to
    /// the simulation (the Animator never advances on its own). Frame locking is what makes hitstop, rollback
    /// and replays look exact: the pose is a pure function of the simulation state.
    /// </summary>
    /// <remarks>
    /// Which state plays and where is decided by <see cref="FighterPoseSampler"/> (engine-free and unit-tested):
    /// a move plays the state named by its <c>animation</c> key over its frame count, stuns play over the stun the
    /// hit applied, jumps are sampled by vertical velocity, and a paired victim plays the holder's victim clip on
    /// the holder's frame. Animator state names therefore equal clip names (<c>A_Katana_L1</c>,
    /// <c>A_Fighter_Hitstun</c>…); see Documentation/Production/03-Animation.
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

        [Header("VFX sockets (rig bones or child transforms; see Production/02-Characters/00-CharacterStandards.md)")]
        [SerializeField] private Transform _weaponSocket;
        [SerializeField] private Transform _weaponTipSocket;
        [SerializeField] private Transform _handLeftSocket;
        [SerializeField] private Transform _handRightSocket;
        [SerializeField] private Transform _chestSocket;

        private readonly Dictionary<string, int> _stateHashes = new Dictionary<string, int>();
        private float _yaw;

        /// <summary>Index of the fighter this presenter renders.</summary>
        public int FighterIndex => _fighterIndex;

        /// <summary>Binds the presenter at runtime (spawned fighters).</summary>
        public void Bind(CombatSimulationRunner runner, int fighterIndex, Animator animator)
        {
            if (_runner != null)
            {
                _runner.UnregisterPresenter(this);
            }

            _runner = runner;
            _fighterIndex = fighterIndex;
            _animator = animator;
            PrepareAnimator();
            if (isActiveAndEnabled && _runner != null)
            {
                _runner.RegisterPresenter(this);
            }
        }

        /// <summary>Where a VFX cue with the given attach point spawns (falls back to the fighter root).</summary>
        public Transform GetSocket(VfxAttach attach)
        {
            Transform socket;
            switch (attach)
            {
                case VfxAttach.Weapon: socket = _weaponSocket; break;
                case VfxAttach.WeaponTip: socket = _weaponTipSocket != null ? _weaponTipSocket : _weaponSocket; break;
                case VfxAttach.HandL: socket = _handLeftSocket; break;
                case VfxAttach.HandR: socket = _handRightSocket; break;
                case VfxAttach.Chest: socket = _chestSocket; break;
                default: socket = null; break;
            }

            return socket != null ? socket : transform;
        }

        private void OnEnable()
        {
            if (_runner != null)
            {
                _runner.RegisterPresenter(this);
            }
        }

        private void OnDisable()
        {
            if (_runner != null)
            {
                _runner.UnregisterPresenter(this);
            }
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
                ApplyPose();
            }
        }

        private void ApplyPose()
        {
            var sample = FighterPoseSampler.Sample(_runner.World, _fighterIndex, _runner.Alpha);
            _animator.Play(Hash(sample.Clip), BaseLayer, sample.NormalizedTime);
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
    }
}

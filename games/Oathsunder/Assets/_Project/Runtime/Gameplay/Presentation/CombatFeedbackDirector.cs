using System;
using System.Collections.Generic;
using Oathsunder.Combat.Events;
using Oathsunder.Combat.Simulation;
using Oathsunder.Gameplay.Cameras;
using Oathsunder.Gameplay.Combat;
using Oathsunder.Presentation.Cues;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Oathsunder.Gameplay.Presentation
{
    /// <summary>
    /// Turns combat events into "game feel". Move-authored cues (<see cref="CombatEventType.Cue"/>) and
    /// event-driven impact cues (<see cref="EventCueMap"/>) are expanded through the cue catalog and dispatched
    /// per channel: pooled VFX at sockets or contact points, audio variations on mixer buses, camera effects,
    /// slow motion, authored camera sequences and haptics. Hitstop is not handled here: it is part of the
    /// simulation, so frozen poses are exact and rollback-safe.
    /// </summary>
    /// <remarks>
    /// <para>Voice layers speak as the affected fighter: <c>vo.hurt.*</c> as the event target (victim), every
    /// other voice cue as the actor. <c>{Character}</c> bindings use the speaker's fighter id without the
    /// <c>fighter.</c> prefix; <c>{Weapon}</c> bindings use the attacker's weapon class.</para>
    /// <para>Netplay (Phase 14) must feed this listener confirmed events only, or de-duplicate predicted events
    /// by their stable key (frame, type, actor, target, instance), so a rollback never replays a cue.</para>
    /// </remarks>
    public sealed class CombatFeedbackDirector : MonoBehaviour, ICombatEventListener
    {
        private const float FramesPerSecond = 60f;

        [SerializeField] private CombatSimulationRunner _runner;
        [SerializeField] private CombatCueLibrary _library;
        [SerializeField] private CombatAudioPlayer _audio;
        [SerializeField] private CombatCameraEffects _cameraEffects;
        [SerializeField] private CombatCameraRig _cameraRig;
        [Tooltip("Depth of the combat plane in world space.")]
        [SerializeField] private float _planeDepth;
        [Tooltip("Only offline modes may slow the simulation pacing; online peers must tick at the same rate.")]
        [SerializeField] private bool _allowSlowMotion = true;
        [Tooltip("Fighter whose events drive haptics (-1 = every fighter, e.g. local versus).")]
        [SerializeField] private int _localFighter;
        [SerializeField] private PresentationOptions _options = new PresentationOptions();

        private readonly Dictionary<GameObject, Queue<GameObject>> _pool = new Dictionary<GameObject, Queue<GameObject>>();
        private readonly List<ActiveVfx> _active = new List<ActiveVfx>();
        private readonly List<CueDefinition> _expanded = new List<CueDefinition>(8);
        private readonly string[] _characterVariants = new string[CombatWorldState.MaxFighters];
        private readonly string[] _weaponVariants = new string[CombatWorldState.MaxFighters];
        private CombatWorld _variantsFor;
        private int _slowMotionTicks;
        private bool _slowMotionActive;
        private float _scaleBeforeSlowMotion = 1f;
        private float _cinematicUntil;
        private GameObject _cinematic;
        private float _hapticsUntil;

        /// <summary>Player presentation settings (blood, camera motion, flashes, haptics, tier).</summary>
        public PresentationOptions Options
        {
            get => _options;
            set
            {
                _options = value ?? new PresentationOptions();
                ApplyOptions();
            }
        }

        /// <summary>Fighter whose events drive haptics (-1 = every fighter).</summary>
        public int LocalFighter
        {
            get => _localFighter;
            set => _localFighter = value;
        }

        private void OnEnable()
        {
            if (_runner != null)
            {
                _runner.AddListener(this);
                _runner.Ticked += OnTicked;
            }

            ApplyOptions();
        }

        private void OnDisable()
        {
            if (_runner != null)
            {
                _runner.RemoveListener(this);
                _runner.Ticked -= OnTicked;
            }

            EndSlowMotion();
            StopHaptics();
        }

        /// <inheritdoc />
        public void OnCombatEvent(CombatSimulationRunner runner, in CombatEvent e)
        {
            if (_library == null || _library.Catalog == null)
            {
                return;
            }

            if (e.Type == CombatEventType.RoundStart)
            {
                ResetPresentation();
            }

            string cue = e.Type == CombatEventType.Cue ? runner.World.BlueprintOf(e.Actor).CueNames[e.Value] : EventCueMap.ForEvent(e);
            if (cue == null)
            {
                return;
            }

            RefreshVariants(runner.World);
            _expanded.Clear();
            _library.Catalog.Expand(cue, _expanded);
            for (int i = 0; i < _expanded.Count; i++)
            {
                Dispatch(runner, _expanded[i], e);
            }
        }

        private void Dispatch(CombatSimulationRunner runner, CueDefinition cue, in CombatEvent e)
        {
            switch (cue.Channel)
            {
                case CueChannel.Vfx:
                    SpawnVfx(runner, cue, e);
                    break;
                case CueChannel.Sfx:
                case CueChannel.Foley:
                case CueChannel.Vo:
                    PlayAudio(runner, cue, e);
                    break;
                case CueChannel.Cam:
                    PlayCamera(runner, cue, e);
                    break;
                case CueChannel.Haptic:
                    PlayHaptic(cue, e);
                    break;
            }
        }

        // ------------------------------------------------------------------ VFX

        private void SpawnVfx(CombatSimulationRunner runner, CueDefinition cue, in CombatEvent e)
        {
            if (cue.Vfx.Gore && !_options.BloodEnabled)
            {
                return;
            }

            int owner = e.Actor >= 0 ? e.Actor : e.Target;
            if (!_library.TryGetBinding(cue.Name, _weaponVariants[Clamp(owner)], out var binding))
            {
                return;
            }

            var prefab = _options.Tier == QualityTier.MobileLow && binding.VfxMobileLow != null ? binding.VfxMobileLow : binding.Vfx;
            if (prefab == null)
            {
                return;
            }

            Transform parent = null;
            Vector3 position;
            Quaternion rotation = Quaternion.identity;
            switch (cue.Vfx.Attach)
            {
                case VfxAttach.Hit:
                case VfxAttach.World:
                    position = e.Position.ToWorld(_planeDepth);
                    break;
                case VfxAttach.Root:
                    position = owner >= 0 ? runner.GetRenderPosition(owner, _planeDepth) : e.Position.ToWorld(_planeDepth);
                    rotation = FacingRotation(runner, owner);
                    break;
                default:
                    var presenter = runner.GetPresenter(owner);
                    parent = presenter != null ? presenter.GetSocket(cue.Vfx.Attach) : null;
                    position = parent != null ? parent.position : e.Position.ToWorld(_planeDepth);
                    rotation = parent != null ? parent.rotation : Quaternion.identity;
                    break;
            }

            var instance = Rent(prefab);
            instance.transform.parent = parent != null ? parent : transform;
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
            _active.Add(new ActiveVfx(prefab, instance, Time.unscaledTime + cue.Vfx.LifetimeFrames / FramesPerSecond));
        }

        private static Quaternion FacingRotation(CombatSimulationRunner runner, int fighter)
        {
            if (fighter < 0 || runner.World == null)
            {
                return Quaternion.identity;
            }

            return runner.World.State.Fighters[fighter].Facing >= 0 ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);
        }

        // ------------------------------------------------------------------ audio

        private void PlayAudio(CombatSimulationRunner runner, CueDefinition cue, in CombatEvent e)
        {
            if (_audio == null)
            {
                return;
            }

            bool victimVoice = cue.Channel == CueChannel.Vo && cue.Name.StartsWith("vo.hurt.", StringComparison.Ordinal);
            int speaker = victimVoice ? (e.Target >= 0 ? e.Target : e.Actor) : (e.Actor >= 0 ? e.Actor : e.Target);
            string variant = cue.Asset.IndexOf("{Character}", StringComparison.Ordinal) >= 0 ? _characterVariants[Clamp(speaker)]
                : cue.Asset.IndexOf("{Weapon}", StringComparison.Ordinal) >= 0 ? _weaponVariants[Clamp(e.Actor >= 0 ? e.Actor : e.Target)]
                : "";
            if (!_library.TryGetBinding(cue.Name, variant, out var binding))
            {
                return;
            }

            Vector3 position = e.Type == CombatEventType.Cue && speaker >= 0
                ? runner.GetRenderPosition(speaker, _planeDepth)
                : e.Position.ToWorld(_planeDepth);
            _audio.Play(cue, binding, position);
        }

        // ------------------------------------------------------------------ camera

        private void PlayCamera(CombatSimulationRunner runner, CueDefinition cue, in CombatEvent e)
        {
            var camera = cue.Camera;
            switch (camera.Kind)
            {
                case CameraCueKind.SlowMotion:
                    StartSlowMotion(runner, camera.TimeScale, camera.Frames);
                    break;
                case CameraCueKind.Sequence:
                    StartSequence(runner, cue, e.Actor);
                    break;
                default:
                    if (_cameraEffects != null)
                    {
                        _cameraEffects.Play(camera, _options.FlashesEnabled);
                    }

                    break;
            }
        }

        private void StartSlowMotion(CombatSimulationRunner runner, float timeScale, int frames)
        {
            if (!_allowSlowMotion)
            {
                return;
            }

            if (!_slowMotionActive)
            {
                _scaleBeforeSlowMotion = runner.TickRateScale;
                _slowMotionActive = true;
            }

            runner.TickRateScale = Mathf.Min(_scaleBeforeSlowMotion, timeScale);
            _slowMotionTicks = Mathf.Max(_slowMotionTicks, frames);
        }

        private void OnTicked(CombatSimulationRunner runner)
        {
            if (_slowMotionTicks > 0 && --_slowMotionTicks == 0)
            {
                EndSlowMotion();
            }
        }

        private void EndSlowMotion()
        {
            if (_slowMotionActive && _runner != null)
            {
                _runner.TickRateScale = _scaleBeforeSlowMotion;
            }

            _slowMotionActive = false;
            _slowMotionTicks = 0;
        }

        private void StartSequence(CombatSimulationRunner runner, CueDefinition cue, int actor)
        {
            if (!_library.TryGetBinding(cue.Name, "", out var binding) || binding.Sequence == null)
            {
                return;
            }

            if (_cinematic != null)
            {
                Destroy(_cinematic);
            }

            var presenter = runner.GetPresenter(actor);
            _cinematic = Instantiate(binding.Sequence, presenter != null ? presenter.transform : transform);
            _cinematicUntil = Time.unscaledTime + cue.Camera.Frames / FramesPerSecond / Mathf.Max(0.05f, runner.TickRateScale);
            if (_cameraRig != null)
            {
                _cameraRig.CinematicOverride = true;
            }

            if (_cameraEffects != null)
            {
                _cameraEffects.Clear();
            }
        }

        // ------------------------------------------------------------------ haptics

        private void PlayHaptic(CueDefinition cue, in CombatEvent e)
        {
            if (!_options.HapticsEnabled || (_localFighter >= 0 && e.Actor != _localFighter && e.Target != _localFighter))
            {
                return;
            }

            float amplitude = cue.Haptic.Amplitude;
            var pad = Gamepad.current;
            if (pad != null)
            {
                pad.SetMotorSpeeds(amplitude * 0.6f, amplitude);
                _hapticsUntil = Mathf.Max(_hapticsUntil, Time.unscaledTime + cue.Haptic.DurationMs / 1000f);
            }
#if UNITY_ANDROID || UNITY_IOS
            else if (amplitude >= 0.5f)
            {
                Handheld.Vibrate();
            }
#endif
        }

        private void StopHaptics()
        {
            Gamepad.current?.ResetHaptics();
            _hapticsUntil = 0f;
        }

        // ------------------------------------------------------------------ lifecycle

        private void LateUpdate()
        {
            float now = Time.unscaledTime;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                if (now >= _active[i].ReturnTime)
                {
                    Return(_active[i]);
                    _active.RemoveAt(i);
                }
            }

            if (_hapticsUntil > 0f && now >= _hapticsUntil)
            {
                StopHaptics();
            }

            if (_cinematic != null && now >= _cinematicUntil)
            {
                Destroy(_cinematic);
                _cinematic = null;
                if (_cameraRig != null)
                {
                    _cameraRig.CinematicOverride = false;
                }
            }
        }

        private void ResetPresentation()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Return(_active[i]);
            }

            _active.Clear();
            if (_audio != null)
            {
                _audio.StopAll();
            }

            if (_cameraEffects != null)
            {
                _cameraEffects.Clear();
            }

            EndSlowMotion();
            StopHaptics();
        }

        private void ApplyOptions()
        {
            if (_cameraEffects != null)
            {
                _cameraEffects.MotionScale = _options.CameraMotionScale;
            }
        }

        private void RefreshVariants(CombatWorld world)
        {
            if (world == _variantsFor)
            {
                return;
            }

            _variantsFor = world;
            for (int i = 0; i < _characterVariants.Length; i++)
            {
                _characterVariants[i] = "";
                _weaponVariants[i] = "";
                if (i >= world.FighterCount)
                {
                    continue;
                }

                var blueprint = world.BlueprintOf(i);
                _characterVariants[i] = StripPrefix(blueprint.Body.Id, "fighter.");
                foreach (string set in blueprint.MoveSetIds)
                {
                    if (set.StartsWith("weapon.", StringComparison.Ordinal))
                    {
                        _weaponVariants[i] = StripPrefix(set, "weapon.");
                        break;
                    }
                }
            }
        }

        private static string StripPrefix(string id, string prefix) =>
            id != null && id.StartsWith(prefix, StringComparison.Ordinal) ? id.Substring(prefix.Length) : id ?? "";

        private static int Clamp(int fighter) => fighter < 0 ? 0 : fighter >= CombatWorldState.MaxFighters ? CombatWorldState.MaxFighters - 1 : fighter;

        private GameObject Rent(GameObject prefab)
        {
            if (_pool.TryGetValue(prefab, out var queue) && queue.Count > 0)
            {
                return queue.Dequeue();
            }

            var instance = Instantiate(prefab, transform);
            instance.SetActive(false);
            return instance;
        }

        private void Return(ActiveVfx vfx)
        {
            if (vfx.Instance == null)
            {
                return;
            }

            vfx.Instance.SetActive(false);
            vfx.Instance.transform.parent = transform;
            if (!_pool.TryGetValue(vfx.Prefab, out var queue))
            {
                queue = new Queue<GameObject>();
                _pool.Add(vfx.Prefab, queue);
            }

            queue.Enqueue(vfx.Instance);
        }

        private readonly struct ActiveVfx
        {
            public ActiveVfx(GameObject prefab, GameObject instance, float returnTime)
            {
                Prefab = prefab;
                Instance = instance;
                ReturnTime = returnTime;
            }

            public GameObject Prefab { get; }

            public GameObject Instance { get; }

            public float ReturnTime { get; }
        }
    }
}

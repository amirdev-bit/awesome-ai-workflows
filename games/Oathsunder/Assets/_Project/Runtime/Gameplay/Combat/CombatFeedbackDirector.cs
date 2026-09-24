using System.Collections.Generic;
using Oathsunder.Combat.Events;
using UnityEngine;

namespace Oathsunder.Gameplay.Combat
{
    /// <summary>
    /// Turns combat events into "game feel": pooled VFX, one-shot sounds, camera shake and haptics. Hitstop is
    /// not handled here — it is part of the simulation, so frozen poses are exact and rollback-safe.
    /// </summary>
    /// <remarks>
    /// Impact cues are synthesised from event flags so every weapon gets consistent feedback without authoring
    /// it per move: <c>impact.hit.light</c>, <c>impact.hit.heavy</c>, <c>impact.hit.counter</c>, <c>impact.block</c>,
    /// <c>impact.parry</c>, <c>impact.perfectdodge</c>, <c>impact.guardbreak</c>, <c>impact.knockout</c>,
    /// <c>impact.finisher</c>, <c>impact.wallbounce</c>, <c>impact.groundbounce</c>, <c>impact.echo</c>.
    /// </remarks>
    public sealed class CombatFeedbackDirector : MonoBehaviour, ICombatEventListener
    {
        [SerializeField] private CombatSimulationRunner _runner;
        [SerializeField] private CombatCueLibrary _cues;
        [SerializeField] private Transform _cameraRig;
        [SerializeField] private AudioSource _audio;
        [Tooltip("Shake decay per second.")]
        [SerializeField, Min(0.1f)] private float _shakeDecay = 9f;
        [SerializeField] private float _planeDepth;

        private readonly Dictionary<GameObject, Queue<GameObject>> _pool = new Dictionary<GameObject, Queue<GameObject>>();
        private readonly List<ActiveVfx> _active = new List<ActiveVfx>();
        private float _shake;
        private Vector3 _rigRestPosition;

        private void OnEnable()
        {
            if (_runner != null)
            {
                _runner.AddListener(this);
            }

            if (_cameraRig != null)
            {
                _rigRestPosition = _cameraRig.localPosition;
            }
        }

        private void OnDisable()
        {
            if (_runner != null)
            {
                _runner.RemoveListener(this);
            }
        }

        /// <inheritdoc />
        public void OnCombatEvent(CombatSimulationRunner runner, in CombatEvent e)
        {
            switch (e.Type)
            {
                case CombatEventType.Cue:
                    Play(runner.World.BlueprintOf(e.Actor).CueNames[e.Value], runner.GetRenderPosition(e.Actor, _planeDepth));
                    break;
                case CombatEventType.Hit:
                    string hit = e.Has(CombatEventFlags.Counter) ? "impact.hit.counter" : (e.Has(CombatEventFlags.Heavy) ? "impact.hit.heavy" : "impact.hit.light");
                    Play(hit, e.Position.ToWorld(_planeDepth));
                    break;
                case CombatEventType.Block:
                    Play("impact.block", e.Position.ToWorld(_planeDepth));
                    break;
                case CombatEventType.Parry:
                    Play("impact.parry", e.Position.ToWorld(_planeDepth));
                    break;
                case CombatEventType.PerfectDodge:
                    Play("impact.perfectdodge", e.Position.ToWorld(_planeDepth));
                    break;
                case CombatEventType.GuardBreak:
                    Play("impact.guardbreak", e.Position.ToWorld(_planeDepth));
                    break;
                case CombatEventType.WallBounce:
                    Play("impact.wallbounce", e.Position.ToWorld(_planeDepth));
                    break;
                case CombatEventType.GroundBounce:
                    Play("impact.groundbounce", e.Position.ToWorld(_planeDepth));
                    break;
                case CombatEventType.ShadowEcho:
                    Play("impact.echo", e.Position.ToWorld(_planeDepth));
                    break;
                case CombatEventType.KnockOut:
                    Play(e.Has(CombatEventFlags.Finisher) ? "impact.finisher" : "impact.knockout", e.Position.ToWorld(_planeDepth));
                    break;
            }
        }

        private void Play(string cue, Vector3 position)
        {
            if (_cues == null || !_cues.TryGet(cue, out var entry))
            {
                return;
            }

            if (entry.VfxPrefab != null)
            {
                var instance = Rent(entry.VfxPrefab);
                instance.transform.SetPositionAndRotation(position, Quaternion.identity);
                instance.SetActive(true);
                _active.Add(new ActiveVfx(entry.VfxPrefab, instance, Time.unscaledTime + entry.Lifetime));
            }

            if (entry.Sound != null && _audio != null)
            {
                _audio.PlayOneShot(entry.Sound, entry.Volume);
            }

            if (entry.CameraShake > _shake)
            {
                _shake = entry.CameraShake;
            }

#if UNITY_ANDROID || UNITY_IOS
            if (entry.Haptics >= 0.5f)
            {
                Handheld.Vibrate();
            }
#endif
        }

        private void LateUpdate()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                if (Time.unscaledTime >= _active[i].ReturnTime)
                {
                    Return(_active[i]);
                    _active.RemoveAt(i);
                }
            }

            if (_cameraRig == null)
            {
                return;
            }

            if (_shake > 0.0005f)
            {
                _cameraRig.localPosition = _rigRestPosition + (Vector3)(Random.insideUnitCircle * _shake);
                _shake = Mathf.MoveTowards(_shake, 0f, _shakeDecay * _shake * Time.unscaledDeltaTime + 0.0001f);
            }
            else
            {
                _shake = 0f;
                _cameraRig.localPosition = _rigRestPosition;
            }
        }

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
            vfx.Instance.SetActive(false);
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

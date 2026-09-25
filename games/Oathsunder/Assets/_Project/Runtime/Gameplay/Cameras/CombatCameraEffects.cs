using System;
using Oathsunder.Presentation.Cues;
using UnityEngine;

namespace Oathsunder.Gameplay.Cameras
{
    /// <summary>
    /// Additive camera cues on a child of <see cref="CombatCameraRig"/>: shake (Perlin noise), push (dolly
    /// toward the focus), tilt (pitch), zoom (field of view) and full-screen flash requests. The rig owns framing;
    /// this component only offsets its child, so framing and effects never fight. All motion runs on unscaled
    /// time and is multiplied by <see cref="MotionScale"/> (accessibility).
    /// </summary>
    public sealed class CombatCameraEffects : MonoBehaviour
    {
        private const int MaxActive = 16;
        private const float FramesPerSecond = 60f;

        [SerializeField] private UnityEngine.Camera _camera;

        private readonly Active[] _active = new Active[MaxActive];
        private float _baseFieldOfView;
        private bool _hasBaseFieldOfView;

        /// <summary>Global multiplier for shake, push, tilt and zoom (0 disables camera motion).</summary>
        public float MotionScale { get; set; } = 1f;

        /// <summary>Raised for flash cues: peak opacity (0–1) and duration in seconds. The HUD draws the flash.</summary>
        public event Action<float, float> FlashRequested;

        /// <summary>Starts a camera cue. Sequence and slow-motion cues are handled by the feedback director.</summary>
        public void Play(CameraCueParams cue, bool flashesEnabled)
        {
            if (cue == null)
            {
                return;
            }

            if (cue.Kind == CameraCueKind.Flash)
            {
                if (flashesEnabled)
                {
                    FlashRequested?.Invoke(cue.Amplitude, cue.Frames / FramesPerSecond);
                }

                return;
            }

            if (cue.Kind == CameraCueKind.Sequence || cue.Kind == CameraCueKind.SlowMotion)
            {
                return;
            }

            int slot = 0;
            float soonest = float.MaxValue;
            for (int i = 0; i < MaxActive; i++)
            {
                float remaining = _active[i].Duration - _active[i].Elapsed;
                if (remaining < soonest)
                {
                    soonest = remaining;
                    slot = i;
                }
            }

            _active[slot] = new Active
            {
                Kind = cue.Kind,
                Amplitude = cue.Amplitude,
                Frequency = cue.Frequency,
                Duration = Mathf.Max(1, cue.Frames) / FramesPerSecond,
                Elapsed = 0f,
                Seed = UnityEngine.Random.value * 100f,
            };
        }

        /// <summary>Cancels every running effect (round reset, cinematic takeover).</summary>
        public void Clear()
        {
            for (int i = 0; i < MaxActive; i++)
            {
                _active[i] = default;
            }
        }

        private void LateUpdate()
        {
            if (_camera != null && !_hasBaseFieldOfView)
            {
                _baseFieldOfView = _camera.fieldOfView;
                _hasBaseFieldOfView = true;
            }

            float dt = Time.unscaledDeltaTime;
            Vector3 offset = Vector3.zero;
            float pitch = 0f;
            float fov = 0f;
            for (int i = 0; i < MaxActive; i++)
            {
                ref Active a = ref _active[i];
                if (a.Duration <= 0f || a.Elapsed >= a.Duration)
                {
                    continue;
                }

                a.Elapsed += dt;
                float t = Mathf.Clamp01(a.Elapsed / a.Duration);
                switch (a.Kind)
                {
                    case CameraCueKind.Shake:
                        float decay = (1f - t) * (1f - t);
                        float phase = a.Elapsed * a.Frequency;
                        offset.x += (Mathf.PerlinNoise(a.Seed, phase) * 2f - 1f) * a.Amplitude * decay;
                        offset.y += (Mathf.PerlinNoise(a.Seed + 17.3f, phase) * 2f - 1f) * a.Amplitude * decay;
                        break;
                    case CameraCueKind.Push:
                        offset.z += a.Amplitude * Envelope(t) * DistanceToFocus();
                        break;
                    case CameraCueKind.Tilt:
                        pitch -= a.Amplitude * Envelope(t);
                        break;
                    case CameraCueKind.Zoom:
                        fov += a.Amplitude * Envelope(t);
                        break;
                }
            }

            float scale = Mathf.Clamp01(MotionScale);
            transform.localPosition = new Vector3(offset.x * scale, offset.y * scale, offset.z * scale);
            transform.localRotation = Quaternion.Euler(pitch * scale, 0f, 0f);
            if (_camera != null && _hasBaseFieldOfView)
            {
                _camera.fieldOfView = _baseFieldOfView + fov * scale;
            }
        }

        /// <summary>Ease in over the first quarter, hold, ease out over the last half.</summary>
        private static float Envelope(float t)
        {
            if (t < 0.25f)
            {
                return Mathf.SmoothStep(0f, 1f, t / 0.25f);
            }

            return t < 0.5f ? 1f : Mathf.SmoothStep(1f, 0f, (t - 0.5f) / 0.5f);
        }

        private float DistanceToFocus()
        {
            var parent = transform.parent;
            return parent != null ? Mathf.Abs(parent.position.z) : 8f;
        }

        private struct Active
        {
            public CameraCueKind Kind;
            public float Amplitude;
            public float Frequency;
            public float Duration;
            public float Elapsed;
            public float Seed;
        }
    }
}

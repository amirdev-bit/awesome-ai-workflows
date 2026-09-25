using System;
using System.Collections.Generic;
using Oathsunder.Presentation.Cues;
using UnityEngine;
using UnityEngine.Audio;

namespace Oathsunder.Gameplay.Presentation
{
    /// <summary>Routes a catalog bus name (e.g. <c>sfx.weapons</c>) to a mixer group.</summary>
    [Serializable]
    public sealed class CombatAudioBus
    {
        [Tooltip("Bus name used by the cue catalog.")]
        public string Bus = "";

        [Tooltip("Mixer group in AM_Master.")]
        public AudioMixerGroup Group;
    }

    /// <summary>
    /// Plays combat one-shots from a fixed pool of voices: random-container variation without immediate repeats,
    /// pitch jitter and play chance from the catalog, mixer-bus routing, and voice stealing (oldest voice) when
    /// the pool is full. Presentation randomness uses its own generator and never touches the simulation.
    /// Phase 13 layers the adaptive music and ambience systems next to this player.
    /// </summary>
    public sealed class CombatAudioPlayer : MonoBehaviour
    {
        [SerializeField, Range(4, 64)] private int _voices = 24;
        [Tooltip("0 = 2D, 1 = fully positional. The side-on camera wants mostly-2D mixing with a little panning.")]
        [SerializeField, Range(0f, 1f)] private float _spatialBlend = 0.35f;
        [SerializeField] private List<CombatAudioBus> _buses = new List<CombatAudioBus>();

        private readonly Dictionary<string, AudioMixerGroup> _busLookup = new Dictionary<string, AudioMixerGroup>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _lastVariation = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly System.Random _random = new System.Random(0x0A7B5);
        private AudioSource[] _pool;
        private float[] _startedAt;

        private void Awake()
        {
            _pool = new AudioSource[_voices];
            _startedAt = new float[_voices];
            for (int i = 0; i < _voices; i++)
            {
                var voice = new GameObject("Voice " + i);
                voice.transform.parent = transform;
                _pool[i] = voice.AddComponent<AudioSource>();
                _pool[i].playOnAwake = false;
                _pool[i].spatialBlend = _spatialBlend;
            }

            foreach (var bus in _buses)
            {
                if (bus != null && !string.IsNullOrEmpty(bus.Bus))
                {
                    _busLookup[bus.Bus] = bus.Group;
                }
            }
        }

        /// <summary>Plays one variation of an audio cue at a world position.</summary>
        /// <returns>False when the chance roll skipped it or no clip is bound.</returns>
        public bool Play(CueDefinition cue, CombatCueBinding binding, Vector3 position)
        {
            if (_pool == null || cue.Audio == null || binding == null || binding.Clips == null || binding.Clips.Length == 0)
            {
                return false;
            }

            var audio = cue.Audio;
            if (audio.ChancePercent < 100 && _random.Next(100) >= audio.ChancePercent)
            {
                return false;
            }

            AudioClip clip = binding.Clips[PickVariation(cue.Name + "|" + binding.Variant, binding.Clips.Length)];
            if (clip == null)
            {
                return false;
            }

            AudioSource voice = RentVoice();
            voice.transform.position = position;
            voice.clip = clip;
            voice.volume = Mathf.Pow(10f, audio.VolumeDb / 20f);
            float cents = audio.PitchJitterCents == 0 ? 0f : (float)(_random.NextDouble() * 2.0 - 1.0) * audio.PitchJitterCents;
            voice.pitch = Mathf.Pow(2f, cents / 1200f);
            voice.outputAudioMixerGroup = _busLookup.TryGetValue(audio.Bus, out var group) ? group : null;
            voice.Play();
            return true;
        }

        /// <summary>Stops every voice (round reset, pause-to-menu).</summary>
        public void StopAll()
        {
            if (_pool == null)
            {
                return;
            }

            foreach (var voice in _pool)
            {
                voice.Stop();
            }
        }

        private int PickVariation(string key, int count)
        {
            if (count == 1)
            {
                return 0;
            }

            int previous = _lastVariation.TryGetValue(key, out int last) ? last : -1;
            int pick = _random.Next(count - (previous >= 0 ? 1 : 0));
            if (previous >= 0 && pick >= previous)
            {
                pick++;
            }

            _lastVariation[key] = pick;
            return pick;
        }

        private AudioSource RentVoice()
        {
            int oldest = 0;
            for (int i = 0; i < _pool.Length; i++)
            {
                if (!_pool[i].isPlaying)
                {
                    _startedAt[i] = Time.unscaledTime;
                    return _pool[i];
                }

                if (_startedAt[i] < _startedAt[oldest])
                {
                    oldest = i;
                }
            }

            _pool[oldest].Stop();
            _startedAt[oldest] = Time.unscaledTime;
            return _pool[oldest];
        }
    }
}

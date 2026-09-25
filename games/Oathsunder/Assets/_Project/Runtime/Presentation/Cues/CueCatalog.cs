using System;
using System.Collections.Generic;

namespace Oathsunder.Presentation.Cues
{
    /// <summary>
    /// Every presentation cue the game can fire, with its production specification. Move data and the
    /// <see cref="EventCueMap"/> reference cues by name; the Unity cue library binds each name to assets.
    /// </summary>
    public sealed class CueCatalog
    {
        private readonly List<CueDefinition> _cues = new List<CueDefinition>();
        private readonly Dictionary<string, CueDefinition> _byName = new Dictionary<string, CueDefinition>(StringComparer.Ordinal);

        /// <summary>Creates an empty catalog.</summary>
        public CueCatalog(string id)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        /// <summary>Catalog id, e.g. <c>cues.combat</c>.</summary>
        public string Id { get; }

        /// <summary>Cues in declaration order.</summary>
        public IReadOnlyList<CueDefinition> Cues => _cues;

        /// <summary>Number of cues.</summary>
        public int Count => _cues.Count;

        /// <summary>Adds a cue.</summary>
        /// <returns>False when a cue with the same name already exists.</returns>
        public bool Add(CueDefinition cue)
        {
            if (cue == null)
            {
                throw new ArgumentNullException(nameof(cue));
            }

            if (_byName.ContainsKey(cue.Name))
            {
                return false;
            }

            _byName.Add(cue.Name, cue);
            _cues.Add(cue);
            return true;
        }

        /// <summary>Finds a cue by name.</summary>
        public bool TryGet(string name, out CueDefinition cue)
        {
            if (name == null)
            {
                cue = null;
                return false;
            }

            return _byName.TryGetValue(name, out cue);
        }

        /// <summary>True when the catalog declares the cue.</summary>
        public bool Contains(string name) => name != null && _byName.ContainsKey(name);

        /// <summary>
        /// Appends the cues that actually play for <paramref name="name"/>: the cue itself, or the layers of an
        /// impact cue. Unknown names append nothing.
        /// </summary>
        public void Expand(string name, List<CueDefinition> output)
        {
            if (!TryGet(name, out var cue))
            {
                return;
            }

            if (cue.Channel != CueChannel.Impact)
            {
                output.Add(cue);
                return;
            }

            foreach (var layer in cue.Layers)
            {
                if (TryGet(layer, out var layerCue) && layerCue.Channel != CueChannel.Impact)
                {
                    output.Add(layerCue);
                }
            }
        }

        /// <summary>Cues of one channel, in declaration order.</summary>
        public IEnumerable<CueDefinition> OfChannel(CueChannel channel)
        {
            foreach (var cue in _cues)
            {
                if (cue.Channel == channel)
                {
                    yield return cue;
                }
            }
        }
    }
}

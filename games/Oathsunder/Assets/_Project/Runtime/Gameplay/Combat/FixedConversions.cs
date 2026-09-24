using Oathsunder.Core.Mathematics;
using UnityEngine;

namespace Oathsunder.Gameplay.Combat
{
    /// <summary>
    /// The one-way door from deterministic simulation space to Unity space. Presentation may read simulation
    /// values as floats; nothing computed in floats may ever flow back into the simulation.
    /// </summary>
    public static class FixedConversions
    {
        /// <summary>Converts a simulation position to a world position on the combat plane at <paramref name="depth"/>.</summary>
        public static Vector3 ToWorld(this FixedVector2 value, float depth = 0f) => new Vector3(value.X.ToFloat(), value.Y.ToFloat(), depth);

        /// <summary>Interpolates two simulation positions for rendering between ticks.</summary>
        public static Vector3 Interpolate(FixedVector2 previous, FixedVector2 current, float alpha, float depth = 0f) =>
            Vector3.LerpUnclamped(previous.ToWorld(depth), current.ToWorld(depth), alpha);

        /// <summary>Converts a simulation box to Unity bounds on the combat plane (debug drawing).</summary>
        public static Bounds ToBounds(this FixedAabb box, float depth = 0f, float thickness = 0.05f)
        {
            var center = box.Center.ToWorld(depth);
            return new Bounds(center, new Vector3(box.Width.ToFloat(), box.Height.ToFloat(), thickness));
        }
    }
}

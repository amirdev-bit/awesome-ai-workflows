using Oathsunder.Combat.Simulation;
using Oathsunder.Gameplay.Combat;
using UnityEngine;

namespace Oathsunder.Gameplay.Cameras
{
    /// <summary>
    /// Side-on 2.5D combat camera: keeps every fighter framed, zooms out as they separate, follows jumps and
    /// juggles partially, never shows past the stage walls, and damps motion frame-rate independently.
    /// Camera shake is applied by <see cref="CombatFeedbackDirector"/> to a child transform, so the two never fight.
    /// </summary>
    public sealed class CombatCameraRig : MonoBehaviour
    {
        [SerializeField] private CombatSimulationRunner _runner;
        [SerializeField] private UnityEngine.Camera _camera;

        [Header("Framing")]
        [Tooltip("Camera distance from the combat plane when fighters are close.")]
        [SerializeField, Min(1f)] private float _nearDistance = 6.5f;
        [Tooltip("Camera distance when fighters are at maximum separation.")]
        [SerializeField, Min(1f)] private float _farDistance = 10.5f;
        [Tooltip("Separation (m) at which the far distance is reached.")]
        [SerializeField, Min(0.5f)] private float _separationForFar = 8f;
        [SerializeField] private float _lookHeight = 1.25f;
        [SerializeField] private float _cameraHeightOffset = 0.35f;
        [Tooltip("How much of an airborne fighter's height the camera follows (0..1).")]
        [SerializeField, Range(0f, 1f)] private float _airFollow = 0.4f;
        [Tooltip("Metres of stage wall kept in view.")]
        [SerializeField, Min(0f)] private float _wallMargin = 0.75f;

        [Header("Motion")]
        [SerializeField, Min(0.1f)] private float _positionDamping = 9f;
        [SerializeField, Min(0.1f)] private float _zoomDamping = 5f;

        private float _distance;
        private Vector3 _focus;
        private bool _initialised;

        /// <summary>When set, cinematic sequences (ultimates, executions, boss intros) own the camera.</summary>
        public bool CinematicOverride { get; set; }

        private void LateUpdate()
        {
            if (_runner == null || _runner.World == null || _camera == null || CinematicOverride)
            {
                return;
            }

            var world = _runner.World;
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float maxAir = 0f;
            int counted = 0;
            for (int i = 0; i < world.FighterCount; i++)
            {
                ref FighterState fighter = ref world.State.Fighters[i];
                if (!fighter.Active)
                {
                    continue;
                }

                Vector3 position = _runner.GetRenderPosition(i);
                minX = Mathf.Min(minX, position.x);
                maxX = Mathf.Max(maxX, position.x);
                maxAir = Mathf.Max(maxAir, position.y);
                counted++;
            }

            if (counted == 0)
            {
                return;
            }

            float separation = maxX - minX;
            float targetDistance = Mathf.Lerp(_nearDistance, _farDistance, Mathf.Clamp01(separation / _separationForFar));
            var targetFocus = new Vector3((minX + maxX) * 0.5f, _lookHeight + maxAir * _airFollow, 0f);

            // Keep the view inside the walls.
            float halfWidth = HalfVisibleWidth(targetDistance);
            float left = world.Setup.Stage.LeftWall.ToFloat() - _wallMargin + halfWidth;
            float right = world.Setup.Stage.RightWall.ToFloat() + _wallMargin - halfWidth;
            targetFocus.x = left <= right ? Mathf.Clamp(targetFocus.x, left, right) : 0.5f * (left + right);

            float dt = Time.unscaledDeltaTime;
            if (!_initialised)
            {
                _distance = targetDistance;
                _focus = targetFocus;
                _initialised = true;
            }
            else
            {
                _distance = Mathf.Lerp(_distance, targetDistance, 1f - Mathf.Exp(-_zoomDamping * dt));
                _focus = Vector3.Lerp(_focus, targetFocus, 1f - Mathf.Exp(-_positionDamping * dt));
            }

            transform.position = new Vector3(_focus.x, _focus.y + _cameraHeightOffset, -_distance);
            transform.rotation = Quaternion.LookRotation(_focus - transform.position, Vector3.up);
        }

        private float HalfVisibleWidth(float distance)
        {
            float halfFov = _camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
            return Mathf.Tan(halfFov) * distance * _camera.aspect;
        }
    }
}

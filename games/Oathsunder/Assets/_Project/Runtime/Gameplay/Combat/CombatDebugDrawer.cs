using Oathsunder.Combat.Simulation;
using Oathsunder.Core.Mathematics;
using UnityEngine;

namespace Oathsunder.Gameplay.Combat
{
    /// <summary>
    /// Scene-view visualisation of the simulation: hurtboxes (blue), active hitboxes (red), pushboxes (yellow),
    /// projectiles (magenta) and stage walls. The same data drives the in-game training-mode overlay (Phase 12).
    /// </summary>
    public sealed class CombatDebugDrawer : MonoBehaviour
    {
        private static readonly Color Hurt = new Color(0.25f, 0.55f, 1f, 0.9f);
        private static readonly Color HitColor = new Color(1f, 0.2f, 0.2f, 0.95f);
        private static readonly Color Push = new Color(1f, 0.85f, 0.2f, 0.6f);
        private static readonly Color ProjectileColor = new Color(1f, 0.3f, 1f, 0.95f);

        [SerializeField] private CombatSimulationRunner _runner;
        [SerializeField] private bool _drawHurtboxes = true;
        [SerializeField] private bool _drawHitboxes = true;
        [SerializeField] private bool _drawPushboxes = true;
        [SerializeField] private float _planeDepth;

        private void OnDrawGizmos()
        {
            if (_runner == null || _runner.World == null)
            {
                return;
            }

            var world = _runner.World;
            var stage = world.Setup.Stage;
            Gizmos.color = Color.white;
            Gizmos.DrawLine(new Vector3(stage.LeftWall.ToFloat(), 0f, _planeDepth), new Vector3(stage.LeftWall.ToFloat(), 6f, _planeDepth));
            Gizmos.DrawLine(new Vector3(stage.RightWall.ToFloat(), 0f, _planeDepth), new Vector3(stage.RightWall.ToFloat(), 6f, _planeDepth));

            for (int i = 0; i < world.FighterCount; i++)
            {
                ref FighterState f = ref world.State.Fighters[i];
                var body = world.BlueprintOf(i).Body;
                var stance = f.Grounded ? body.Standing : body.Airborne;
                if (f.Action == FighterAction.Crouch || f.Action == FighterAction.GuardCrouch)
                {
                    stance = body.Crouching;
                }

                if (_drawHurtboxes)
                {
                    foreach (var box in stance.Hurtboxes)
                    {
                        Draw(box.ToWorld(f.Position, f.Facing), Hurt);
                    }
                }

                if (_drawPushboxes)
                {
                    Draw(stance.Pushbox.ToWorld(f.Position, f.Facing), Push);
                }

                if (_drawHitboxes && f.Action == FighterAction.Move && f.MoveIndex >= 0)
                {
                    foreach (var hitbox in world.BlueprintOf(i).Moves[f.MoveIndex].Hitboxes)
                    {
                        if (hitbox.Window.Contains(f.ActionFrame))
                        {
                            Draw(hitbox.Box.ToWorld(f.Position, f.Facing), HitColor);
                        }
                    }
                }
            }

            for (int p = 0; p < world.State.Projectiles.Length; p++)
            {
                ref ProjectileState projectile = ref world.State.Projectiles[p];
                if (projectile.Active)
                {
                    var definition = world.BlueprintOf(projectile.Owner).Projectiles[projectile.DefinitionIndex];
                    Draw(definition.Box.ToWorld(projectile.Position, projectile.Facing), ProjectileColor);
                }
            }
        }

        private void Draw(FixedAabb box, Color color)
        {
            Gizmos.color = color;
            var bounds = box.ToBounds(_planeDepth);
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}

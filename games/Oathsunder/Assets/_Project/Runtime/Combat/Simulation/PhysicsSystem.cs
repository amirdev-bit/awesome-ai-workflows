using Oathsunder.Combat.Events;
using Oathsunder.Core.Mathematics;

namespace Oathsunder.Combat.Simulation
{
    /// <summary>
    /// Deterministic 2.5D kinematics: gravity, landing, stage walls (with wall bounces and corner pushback
    /// transfer), camera separation limit, body (pushbox) collision, paired-victim locking and auto-facing.
    /// Every rule is mirror-symmetric so both sides of the stage behave identically.
    /// </summary>
    internal sealed class PhysicsSystem
    {
        private readonly CombatContext _ctx;

        public PhysicsSystem(CombatContext context)
        {
            _ctx = context;
        }

        /// <summary>Integrates velocity for every fighter that simulated this frame.</summary>
        public void Integrate()
        {
            var state = _ctx.State;
            for (int i = 0; i < state.FighterCount; i++)
            {
                ref FighterState f = ref state.Fighters[i];
                if (!f.Active || !f.AdvancedThisFrame || f.Action == FighterAction.PairedVictim)
                {
                    continue;
                }

                var body = _ctx.Blueprints[i].Body;
                var velocity = f.Velocity;
                if (!f.Grounded)
                {
                    int gravityPermille = GravityPermille(ref f);
                    Fixed vy = velocity.Y - body.Gravity.MulPermille(gravityPermille);
                    if (vy < -body.MaxFallSpeed)
                    {
                        vy = -body.MaxFallSpeed;
                    }

                    velocity = velocity.WithY(vy);
                }
                else if (UsesFriction(f.Action))
                {
                    velocity = new FixedVector2(Fixed.MoveTowards(velocity.X, Fixed.Zero, body.GroundFriction), Fixed.Zero);
                }

                f.Velocity = velocity;
                f.Position += velocity;

                if (f.Grounded)
                {
                    f.Position = f.Position.WithY(Fixed.Zero);
                }
                else if (f.Position.Y.Raw <= 0 && f.Velocity.Y.Raw <= 0)
                {
                    _ctx.Controller.OnLanded(i);
                }
            }
        }

        /// <summary>Applies walls, the separation limit and body collision.</summary>
        public void ResolveBounds()
        {
            var state = _ctx.State;
            for (int i = 0; i < state.FighterCount; i++)
            {
                if (state.Fighters[i].Active && state.Fighters[i].Action != FighterAction.PairedVictim)
                {
                    ResolveWall(i);
                }
            }

            ResolveSeparation();
            ResolveBodies();
        }

        /// <summary>Locks every paired victim to its attacker.</summary>
        public void SnapPairedVictims()
        {
            var state = _ctx.State;
            for (int i = 0; i < state.FighterCount; i++)
            {
                ref FighterState victim = ref state.Fighters[i];
                if (!victim.Active || victim.Action != FighterAction.PairedVictim || victim.PairedPartner < 0)
                {
                    continue;
                }

                ref FighterState attacker = ref state.Fighters[victim.PairedPartner];
                victim.Position = attacker.Position + victim.PairedOffset.Facing(attacker.Facing);
                victim.Velocity = FixedVector2.Zero;
                victim.Facing = -attacker.Facing;

                // A victim held against a wall pushes the holder out instead of clipping into the wall.
                var push = _ctx.PushboxOf(i);
                Fixed shift = Fixed.Zero;
                if (push.Min.X < _ctx.Stage.LeftWall)
                {
                    shift = _ctx.Stage.LeftWall - push.Min.X;
                }
                else if (push.Max.X > _ctx.Stage.RightWall)
                {
                    shift = _ctx.Stage.RightWall - push.Max.X;
                }

                if (shift.Raw != 0)
                {
                    attacker.Position = attacker.Position.WithX(attacker.Position.X + shift);
                    victim.Position = victim.Position.WithX(victim.Position.X + shift);
                }

                victim.Grounded = victim.Position.Y.Raw <= 0;
                if (victim.Grounded)
                {
                    victim.Position = victim.Position.WithY(Fixed.Zero);
                }
            }
        }

        /// <summary>Turns fighters in neutral-like states toward their targets.</summary>
        public void UpdateFacing()
        {
            var state = _ctx.State;
            for (int i = 0; i < state.FighterCount; i++)
            {
                ref FighterState f = ref state.Fighters[i];
                if (!f.Active || f.IsKnockedOut || f.TargetIndex < 0 || !FacesTargetAutomatically(f.Action) || !f.Grounded)
                {
                    continue;
                }

                Fixed dx = state.Fighters[f.TargetIndex].Position.X - f.Position.X;
                if (dx.Raw != 0)
                {
                    f.Facing = dx.Sign;
                }
            }
        }

        private int GravityPermille(ref FighterState f)
        {
            switch (f.Action)
            {
                case FighterAction.Launched:
                    if (f.IsKnockedOut)
                    {
                        return 1000;
                    }

                    int scaled = 1000 + _ctx.Tuning.JuggleGravityPerHitPermille * f.ComboHits;
                    return scaled > _ctx.Tuning.MaxJuggleGravityPermille ? _ctx.Tuning.MaxJuggleGravityPermille : scaled;
                case FighterAction.Move:
                    return f.MoveGravityPermille;
                default:
                    return 1000;
            }
        }

        private static bool UsesFriction(FighterAction action)
        {
            switch (action)
            {
                case FighterAction.Hitstun:
                case FighterAction.Blockstun:
                case FighterAction.Staggered:
                case FighterAction.Knockdown:
                case FighterAction.KnockedOut:
                case FighterAction.Landing:
                case FighterAction.ParryRecovery:
                case FighterAction.WakeUp:
                    return true;
                default:
                    return false;
            }
        }

        private static bool FacesTargetAutomatically(FighterAction action)
        {
            switch (action)
            {
                case FighterAction.Idle:
                case FighterAction.WalkForward:
                case FighterAction.WalkBackward:
                case FighterAction.Crouch:
                case FighterAction.GuardStand:
                case FighterAction.GuardCrouch:
                case FighterAction.Blockstun:
                case FighterAction.ParryRecovery:
                case FighterAction.Landing:
                case FighterAction.PreJump:
                case FighterAction.WakeUp:
                    return true;
                default:
                    return false;
            }
        }

        private void ResolveWall(int index)
        {
            ref FighterState f = ref _ctx.State.Fighters[index];
            var stage = _ctx.Stage;
            var push = _ctx.PushboxOf(index);
            Fixed overflow;
            int wallSide;
            if (push.Min.X < stage.LeftWall)
            {
                overflow = stage.LeftWall - push.Min.X;
                wallSide = -1;
                f.Position = f.Position.WithX(f.Position.X + overflow);
            }
            else if (push.Max.X > stage.RightWall)
            {
                overflow = push.Max.X - stage.RightWall;
                wallSide = 1;
                f.Position = f.Position.WithX(f.Position.X - overflow);
            }
            else
            {
                return;
            }

            bool movingIntoWall = f.Velocity.X.Sign == wallSide;
            if (!movingIntoWall)
            {
                return;
            }

            if (f.Action == FighterAction.Launched && f.PendingWallBounce && stage.WallBounceEnabled)
            {
                f.PendingWallBounce = false;
                f.WallBounceUsed = true;
                Fixed vy = Fixed.Max(f.Velocity.Y, _ctx.Tuning.WallBounceUpVelocity);
                f.Velocity = new FixedVector2(-f.Velocity.X.MulPermille(_ctx.Tuning.WallBounceRestitutionPermille), vy);
                _ctx.Emit(CombatEventType.WallBounce, f.ComboAttacker, index, f.Position);
                return;
            }

            if ((f.Action == FighterAction.Hitstun || f.Action == FighterAction.Blockstun) && f.LastAttackerIndex >= 0)
            {
                // Corner pushback: the victim cannot travel, so the attacker is pushed away instead.
                ref FighterState attacker = ref _ctx.State.Fighters[f.LastAttackerIndex];
                if (attacker.Active && attacker.Grounded && !attacker.IsKnockedOut && attacker.Action != FighterAction.PairedVictim)
                {
                    attacker.Position = attacker.Position.WithX(attacker.Position.X - overflow * wallSide);
                }

                return;
            }

            f.Velocity = f.Velocity.WithX(Fixed.Zero);
        }

        private void ResolveSeparation()
        {
            var state = _ctx.State;
            if (state.FighterCount != 2)
            {
                return;
            }

            ref FighterState a = ref state.Fighters[0];
            ref FighterState b = ref state.Fighters[1];
            if (!a.Active || !b.Active || a.IsKnockedOut || b.IsKnockedOut)
            {
                return;
            }

            bool aIsLeft = a.Position.X <= b.Position.X;
            ref FighterState left = ref aIsLeft ? ref a : ref b;
            ref FighterState right = ref aIsLeft ? ref b : ref a;
            Fixed excess = right.Position.X - left.Position.X - _ctx.Stage.MaxSeparation;
            if (excess.Raw <= 0)
            {
                return;
            }

            bool leftOutward = left.Velocity.X.Raw < 0;
            bool rightOutward = right.Velocity.X.Raw > 0;
            if (leftOutward && !rightOutward)
            {
                left.Position = left.Position.WithX(left.Position.X + excess);
            }
            else if (rightOutward && !leftOutward)
            {
                right.Position = right.Position.WithX(right.Position.X - excess);
            }
            else
            {
                Fixed half = Fixed.FromRaw((excess.Raw + 1) / 2);
                left.Position = left.Position.WithX(left.Position.X + half);
                right.Position = right.Position.WithX(right.Position.X - half);
            }
        }

        private void ResolveBodies()
        {
            var state = _ctx.State;
            for (int i = 0; i < state.FighterCount; i++)
            {
                for (int j = i + 1; j < state.FighterCount; j++)
                {
                    ResolvePair(i, j);
                }
            }
        }

        private void ResolvePair(int i, int j)
        {
            var state = _ctx.State;
            ref FighterState a = ref state.Fighters[i];
            ref FighterState b = ref state.Fighters[j];
            if (!a.Active || !b.Active || a.IsKnockedOut || b.IsKnockedOut ||
                a.Action == FighterAction.PairedVictim || b.Action == FighterAction.PairedVictim)
            {
                return;
            }

            if (IsPassingThrough(i) || IsPassingThrough(j))
            {
                return;
            }

            var boxA = _ctx.PushboxOf(i);
            var boxB = _ctx.PushboxOf(j);
            if (!boxA.Overlaps(boxB))
            {
                return;
            }

            Fixed overlap = Fixed.Min(boxA.Max.X, boxB.Max.X) - Fixed.Max(boxA.Min.X, boxB.Min.X);
            int directionA;
            if (a.Position.X < b.Position.X)
            {
                directionA = -1;
            }
            else if (a.Position.X > b.Position.X)
            {
                directionA = 1;
            }
            else
            {
                directionA = a.Facing >= 0 ? -1 : 1;
            }

            Fixed half = Fixed.FromRaw((overlap.Raw + 1) / 2);
            a.Position = a.Position.WithX(a.Position.X + half * directionA);
            b.Position = b.Position.WithX(b.Position.X - half * directionA);

            // A fighter pinned against a wall cannot give ground: the other one takes the whole push.
            Fixed correctionA = ClampToWalls(i);
            if (correctionA.Raw != 0)
            {
                b.Position = b.Position.WithX(b.Position.X + correctionA);
            }

            Fixed correctionB = ClampToWalls(j);
            if (correctionB.Raw != 0)
            {
                a.Position = a.Position.WithX(a.Position.X + correctionB);
            }
        }

        private bool IsPassingThrough(int index)
        {
            var move = _ctx.MoveOf(index);
            return move != null && move.HasFlag(Oathsunder.Combat.Definitions.MoveFlags.PassThrough);
        }

        private Fixed ClampToWalls(int index)
        {
            ref FighterState f = ref _ctx.State.Fighters[index];
            var push = _ctx.PushboxOf(index);
            if (push.Min.X < _ctx.Stage.LeftWall)
            {
                Fixed correction = _ctx.Stage.LeftWall - push.Min.X;
                f.Position = f.Position.WithX(f.Position.X + correction);
                return correction;
            }

            if (push.Max.X > _ctx.Stage.RightWall)
            {
                Fixed correction = _ctx.Stage.RightWall - push.Max.X;
                f.Position = f.Position.WithX(f.Position.X + correction);
                return correction;
            }

            return Fixed.Zero;
        }
    }
}

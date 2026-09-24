using Oathsunder.Combat.Definitions;
using Oathsunder.Combat.Events;
using Oathsunder.Combat.Input;
using Oathsunder.Core.Mathematics;

namespace Oathsunder.Combat.Simulation
{
    /// <summary>
    /// Per-fighter state machine: reads buffered input and decides what the fighter does this frame. Movement is
    /// integrated afterwards by <see cref="PhysicsSystem"/>, and contacts are resolved by <see cref="HitSystem"/>.
    /// </summary>
    /// <remarks>
    /// Move selection rule (applies from neutral and in cancel windows): among every trigger whose input is
    /// satisfied, pick the one completed on the <b>earliest</b> press frame (preserving the player's input order);
    /// ties go to the higher trigger priority, then to declaration order.
    /// </remarks>
    internal sealed class FighterController
    {
        private const int NoFrame = int.MinValue / 2;

        private readonly CombatContext _ctx;

        public FighterController(CombatContext context)
        {
            _ctx = context;
        }

        /// <summary>Runs one frame of logic for a fighter that is simulating this frame (not frozen).</summary>
        public void Update(int index)
        {
            ref FighterState f = ref _ctx.State.Fighters[index];
            ScanGuardPresses(index, ref f);
            f.LastLogicWorldFrame = _ctx.Frame;

            if (f.IsKnockedOut)
            {
                return;
            }

            switch (f.Action)
            {
                case FighterAction.Idle:
                case FighterAction.WalkForward:
                case FighterAction.WalkBackward:
                case FighterAction.Crouch:
                case FighterAction.GuardStand:
                case FighterAction.GuardCrouch:
                    UpdateGroundNeutral(index, ref f);
                    break;
                case FighterAction.ParryRecovery:
                    UpdateParryRecovery(index, ref f);
                    break;
                case FighterAction.PreJump:
                    UpdatePreJump(index, ref f);
                    break;
                case FighterAction.Airborne:
                    UpdateAirborne(index, ref f);
                    break;
                case FighterAction.Landing:
                    UpdateLanding(index, ref f);
                    break;
                case FighterAction.Move:
                    UpdateMove(index, ref f);
                    break;
                case FighterAction.Blockstun:
                    UpdateBlockstun(index, ref f);
                    break;
                case FighterAction.Hitstun:
                    UpdateHitstun(index, ref f);
                    break;
                case FighterAction.AirHitstun:
                    UpdateAirHitstun(index, ref f);
                    break;
                case FighterAction.Launched:
                    TryBurst(index, ref f);
                    break;
                case FighterAction.Knockdown:
                    UpdateKnockdown(index, ref f);
                    break;
                case FighterAction.WakeUp:
                    UpdateWakeUp(index, ref f);
                    break;
                case FighterAction.Staggered:
                    UpdateStaggered(index, ref f);
                    break;
                case FighterAction.PairedVictim:
                    UpdatePairedVictim(index, ref f);
                    break;
            }
        }

        // ------------------------------------------------------------------ neutral

        private void UpdateGroundNeutral(int index, ref FighterState f)
        {
            if (!_ctx.IsFighting)
            {
                SetAction(ref f, FighterAction.Idle);
                f.Velocity = FixedVector2.Zero;
                return;
            }

            if (TryStartFromNeutral(index, ref f, TriggerStance.Grounded))
            {
                return;
            }

            var input = _ctx.State.Inputs[index];
            int jumpPress = input.FindBufferedPress(InputButtons.Jump, f.LocalFrame, _ctx.Tuning.InputBufferFrames, _ctx.Tuning.ChordWindowFrames);
            if (jumpPress >= 0)
            {
                input.ConsumeThrough(jumpPress);
                SetAction(ref f, FighterAction.PreJump);
                f.Velocity = FixedVector2.Zero;
                return;
            }

            var current = input.Get(_ctx.Frame);
            var direction = current.Direction(f.Facing);
            bool down = DirectionMask.AnyDown.Matches(direction);
            var body = _ctx.Blueprints[index].Body;
            if (current.IsHeld(InputButtons.Guard))
            {
                f.CrouchGuard = down;
                SetAction(ref f, down ? FighterAction.GuardCrouch : FighterAction.GuardStand);
                f.Velocity = FixedVector2.Zero;
            }
            else if (down)
            {
                SetAction(ref f, FighterAction.Crouch);
                f.Velocity = FixedVector2.Zero;
            }
            else if (DirectionMask.AnyForward.Matches(direction))
            {
                SetAction(ref f, FighterAction.WalkForward);
                f.Velocity = new FixedVector2(body.WalkForwardSpeed * f.Facing, Fixed.Zero);
            }
            else if (DirectionMask.AnyBack.Matches(direction))
            {
                SetAction(ref f, FighterAction.WalkBackward);
                f.Velocity = new FixedVector2(-body.WalkBackwardSpeed * f.Facing, Fixed.Zero);
            }
            else
            {
                SetAction(ref f, FighterAction.Idle);
                f.Velocity = FixedVector2.Zero;
            }
        }

        private void UpdateParryRecovery(int index, ref FighterState f)
        {
            f.ActionFrame++;
            if (f.ActionFrame > _ctx.Tuning.ParryRecoveryFrames)
            {
                SetAction(ref f, FighterAction.Idle);
                UpdateGroundNeutral(index, ref f);
                return;
            }

            f.Velocity = FixedVector2.Zero;
            var current = _ctx.State.Inputs[index].Get(_ctx.Frame);
            f.CrouchGuard = DirectionMask.AnyDown.Matches(current.Direction(f.Facing));
            if (_ctx.IsFighting)
            {
                TryStartFromNeutral(index, ref f, TriggerStance.Grounded);
            }
        }

        private void UpdatePreJump(int index, ref FighterState f)
        {
            f.ActionFrame++;
            var body = _ctx.Blueprints[index].Body;
            if (f.ActionFrame < body.PreJumpFrames)
            {
                return;
            }

            var direction = _ctx.State.Inputs[index].Get(_ctx.Frame).Direction(f.Facing);
            LaunchJump(index, ref f, direction);
        }

        private void LaunchJump(int index, ref FighterState f, NumpadDirection direction)
        {
            var body = _ctx.Blueprints[index].Body;
            Fixed vx = Fixed.Zero;
            if (DirectionMask.AnyForward.Matches(direction))
            {
                vx = body.JumpForwardVelocityX * f.Facing;
            }
            else if (DirectionMask.AnyBack.Matches(direction))
            {
                vx = -body.JumpBackwardVelocityX * f.Facing;
            }

            f.Velocity = new FixedVector2(vx, body.JumpVelocityY);
            f.Grounded = false;
            f.AirActionsUsed = 0;
            SetAction(ref f, FighterAction.Airborne);
            _ctx.Emit(CombatEventType.Jump, index, -1, f.Position);
        }

        private void UpdateAirborne(int index, ref FighterState f)
        {
            f.ActionFrame++;
            if (_ctx.IsFighting && f.AirActionsUsed < _ctx.Tuning.MaxAirActions && TryStartFromNeutral(index, ref f, TriggerStance.Airborne))
            {
                f.AirActionsUsed++;
            }
        }

        private void UpdateLanding(int index, ref FighterState f)
        {
            f.ActionFrame++;
            f.Velocity = f.Velocity.WithX(Fixed.Zero);
            if (f.ActionFrame > f.StunRemaining)
            {
                f.StunRemaining = 0;
                SetAction(ref f, FighterAction.Idle);
                UpdateGroundNeutral(index, ref f);
            }
        }

        // ------------------------------------------------------------------ moves

        private void UpdateMove(int index, ref FighterState f)
        {
            var move = _ctx.Blueprints[index].Moves[f.MoveIndex];
            f.ActionFrame++;
            if (f.ActionFrame > move.TotalFrames)
            {
                EndMove(index, ref f);
                return;
            }

            if (TryCancel(index, ref f, move))
            {
                return;
            }

            ApplyMoveFrame(index, ref f, move);
        }

        private void EndMove(int index, ref FighterState f)
        {
            var move = _ctx.Blueprints[index].Moves[f.MoveIndex];
            if (f.PairedPartner >= 0 && move.Paired != null)
            {
                ReleasePartner(index, ref f, move);
            }

            f.MoveIndex = -1;
            f.MoveGravityPermille = 1000;
            if (f.Grounded)
            {
                var direction = _ctx.State.Inputs[index].Get(_ctx.Frame).Direction(f.Facing);
                bool crouch = move.HasFlag(MoveFlags.EndsCrouched) && DirectionMask.AnyDown.Matches(direction);
                SetAction(ref f, crouch ? FighterAction.Crouch : FighterAction.Idle);
                UpdateGroundNeutral(index, ref f);
            }
            else
            {
                SetAction(ref f, FighterAction.Airborne);
            }
        }

        /// <summary>Starts a move on a fighter (used by neutral, cancels, counters, throws and cinematics).</summary>
        public void StartMove(int index, ref FighterState f, int moveIndex, bool autoFace, int startupPenalty = 0)
        {
            var move = _ctx.Blueprints[index].Moves[moveIndex];
            if (f.PairedPartner >= 0 && f.Action == FighterAction.Move)
            {
                var previous = _ctx.Blueprints[index].Moves[f.MoveIndex];
                if (previous.Paired != null)
                {
                    ReleasePartner(index, ref f, previous);
                }
            }

            f.Rage -= move.Cost.Rage;
            f.Shadow -= move.Cost.Shadow;
            f.Ultimate -= move.Cost.Ultimate;

            if (IsStunned(f.Action))
            {
                EndCombo(index, ref f);
            }

            if (autoFace && f.Grounded && !move.HasFlag(MoveFlags.NoAutoFace))
            {
                FaceTarget(index, ref f);
            }

            f.Action = FighterAction.Move;
            f.MoveIndex = moveIndex;
            f.ActionFrame = 1 - startupPenalty;
            f.Contact = MoveContact.None;
            f.HitRegistry = 0;
            f.ArmorHitsTaken = 0;
            f.StunRemaining = 0;
            f.Stagger = StaggerKind.None;
            f.MoveGravityPermille = 1000;
            f.MoveInstance = _ctx.State.NextInstanceId();
            if (f.Grounded && !move.HasFlag(MoveFlags.KeepMomentum))
            {
                f.Velocity = FixedVector2.Zero;
            }

            _ctx.Emit(CombatEventType.MoveStart, index, f.TargetIndex, f.MoveInstance, moveIndex, (int)move.Tags, f.Position, CombatEventFlags.None);
            ApplyMoveFrame(index, ref f, move);
        }

        private void ApplyMoveFrame(int index, ref FighterState f, MoveDefinition move)
        {
            int frame = f.ActionFrame;
            bool covered = false;
            f.MoveGravityPermille = 1000;
            foreach (var segment in move.Motion)
            {
                if (!segment.Window.Contains(frame))
                {
                    continue;
                }

                covered = true;
                var velocity = f.Velocity;
                if (segment.SetX)
                {
                    velocity = velocity.WithX(segment.Velocity.X * f.Facing);
                }

                if (segment.SetY)
                {
                    velocity = velocity.WithY(segment.Velocity.Y);
                    if (segment.Velocity.Y.Raw > 0)
                    {
                        f.Grounded = false;
                    }
                }

                f.Velocity = velocity;
                f.MoveGravityPermille = segment.GravityPermille;
                break;
            }

            if (!covered && f.Grounded && !move.HasFlag(MoveFlags.KeepMomentum))
            {
                f.Velocity = f.Velocity.WithX(Fixed.Zero);
            }

            foreach (var effect in move.Effects)
            {
                if (effect.Frame == frame)
                {
                    ApplyEffect(index, ref f, effect.Effect);
                }
            }

            foreach (var spawn in move.Projectiles)
            {
                if (spawn.Frame == frame)
                {
                    _ctx.Projectiles.Spawn(index, spawn);
                }
            }

            foreach (var cue in move.Cues)
            {
                if (cue.Frame == frame)
                {
                    _ctx.Emit(CombatEventType.Cue, index, f.TargetIndex, f.MoveInstance, cue.CueIndex, move.Index, f.Position, CombatEventFlags.None);
                }
            }

            if (move.Paired != null && f.PairedPartner >= 0)
            {
                ApplyPairedFrame(index, ref f, move);
            }
        }

        private void ApplyEffect(int index, ref FighterState f, MoveEffect effect)
        {
            switch (effect)
            {
                case MoveEffect.ActivateRage:
                    f.RageFrames = _ctx.Tuning.RageDurationFrames;
                    _ctx.Emit(CombatEventType.RageStart, index, -1, f.Position);
                    break;
                case MoveEffect.ActivateShadow:
                    f.ShadowFrames = _ctx.Tuning.ShadowDurationFrames;
                    _ctx.Emit(CombatEventType.ShadowStart, index, -1, f.Position);
                    break;
                case MoveEffect.FaceTarget:
                    FaceTarget(index, ref f);
                    break;
            }
        }

        private bool TryCancel(int index, ref FighterState f, MoveDefinition move)
        {
            if (!_ctx.IsFighting)
            {
                return false;
            }

            var input = _ctx.State.Inputs[index];
            int frame = f.ActionFrame;
            var stance = f.Grounded ? TriggerStance.Grounded : TriggerStance.Airborne;
            var best = Selection.None;
            int jumpPress = -1;
            var pressed = input.BufferedPresses(f.LocalFrame, _ctx.Tuning.InputBufferFrames);

            foreach (var window in move.Cancels)
            {
                if (!window.Window.Contains(frame) || !IsConditionMet(window.Condition, f.Contact))
                {
                    continue;
                }

                switch (window.Input)
                {
                    case CancelInput.Auto:
                        StartMove(index, ref f, window.Candidates[0], autoFace: false);
                        return true;
                    case CancelInput.Held:
                        if (input.IsHeld(_ctx.Frame, window.InputButton))
                        {
                            StartMove(index, ref f, window.Candidates[0], autoFace: false);
                            return true;
                        }

                        continue;
                    case CancelInput.Released:
                        if (!input.IsHeld(_ctx.Frame, window.InputButton))
                        {
                            StartMove(index, ref f, window.Candidates[0], autoFace: false);
                            return true;
                        }

                        continue;
                }

                foreach (int candidate in window.Candidates)
                {
                    EvaluateMove(index, ref f, candidate, stance, pressed, ref best);
                }

                if (window.AllowJump && f.Grounded && jumpPress < 0)
                {
                    jumpPress = input.FindBufferedPress(InputButtons.Jump, f.LocalFrame, _ctx.Tuning.InputBufferFrames, _ctx.Tuning.ChordWindowFrames);
                }
            }

            // A jump cancel wins only when its press came strictly before any matching move press.
            if (jumpPress >= 0 && (best.Move < 0 || jumpPress < best.PressFrame))
            {
                input.ConsumeThrough(jumpPress);
                f.MoveIndex = -1;
                var direction = input.Get(_ctx.Frame).Direction(f.Facing);
                LaunchJump(index, ref f, direction);
                return true;
            }

            if (best.Move < 0)
            {
                return false;
            }

            input.ConsumeThrough(best.PressFrame);
            StartMove(index, ref f, best.Move, autoFace: false, best.StartupPenalty);
            return true;
        }

        private static bool IsConditionMet(CancelCondition condition, MoveContact contact)
        {
            switch (condition)
            {
                case CancelCondition.OnHit:
                    return (contact & MoveContact.Hit) != 0;
                case CancelCondition.OnBlock:
                    return (contact & MoveContact.Blocked) != 0;
                case CancelCondition.OnContact:
                    return (contact & (MoveContact.Hit | MoveContact.Blocked | MoveContact.Armored)) != 0;
                case CancelCondition.OnWhiff:
                    return (contact & (MoveContact.Hit | MoveContact.Blocked | MoveContact.Armored | MoveContact.Parried)) == 0;
                case CancelCondition.OnEvade:
                    return (contact & MoveContact.Evaded) != 0;
                default:
                    return true;
            }
        }

        private bool TryStartFromNeutral(int index, ref FighterState f, TriggerStance stance)
        {
            var best = Selection.None;
            var pressed = _ctx.State.Inputs[index].BufferedPresses(f.LocalFrame, _ctx.Tuning.InputBufferFrames);
            foreach (int candidate in _ctx.Blueprints[index].NeutralCandidates)
            {
                EvaluateMove(index, ref f, candidate, stance, pressed, ref best);
            }

            if (best.Move < 0)
            {
                return false;
            }

            _ctx.State.Inputs[index].ConsumeThrough(best.PressFrame);
            StartMove(index, ref f, best.Move, autoFace: true, best.StartupPenalty);
            return true;
        }

        private void TryBurst(int index, ref FighterState f)
        {
            if (_ctx.IsFighting && _ctx.Rules.RageBurstEnabled)
            {
                TryStartFromNeutral(index, ref f, TriggerStance.Stunned);
            }
        }

        private void EvaluateMove(int index, ref FighterState f, int moveIndex, TriggerStance stance, InputButtons pressed, ref Selection best)
        {
            var move = _ctx.Blueprints[index].Moves[moveIndex];
            if (f.Rage < move.Cost.Rage || f.Shadow < move.Cost.Shadow || f.Ultimate < move.Cost.Ultimate)
            {
                return;
            }

            foreach (var trigger in move.Triggers)
            {
                if ((trigger.Stance & stance) == 0 || (trigger.Schemes & _ctx.Schemes[index]) == 0)
                {
                    continue;
                }

                // Fast reject: a button trigger needs every one of its buttons freshly pressed inside the buffer.
                if (trigger.Buttons != InputButtons.None && (trigger.Buttons & ~pressed) != 0)
                {
                    continue;
                }

                if (!AreConditionsMet(index, ref f, trigger.Conditions))
                {
                    continue;
                }

                int press = MatchTrigger(index, ref f, trigger);
                if (press < 0)
                {
                    continue;
                }

                if (best.Move < 0 || press < best.PressFrame || (press == best.PressFrame && trigger.Priority > best.Priority))
                {
                    bool penalised = trigger.Schemes == ControlSchemeMask.Simplified && move.HasTag(MoveTags.Special);
                    best = new Selection(moveIndex, press, trigger.Priority, penalised ? _ctx.Tuning.SimplifiedStartupPenalty : 0);
                }
            }
        }

        private int MatchTrigger(int index, ref FighterState f, MoveTrigger trigger)
        {
            var input = _ctx.State.Inputs[index];
            int start = input.EarliestBufferedFrame(f.LocalFrame, _ctx.Tuning.InputBufferFrames);
            if (start == int.MaxValue)
            {
                return -1;
            }

            int motionFloor = System.Math.Max(input.OldestFrame, input.ConsumedThroughFrame + 1);
            for (int frame = start; frame <= input.LatestFrame; frame++)
            {
                if (trigger.Buttons != InputButtons.None)
                {
                    if (!input.IsChordCompletedOn(frame, trigger.Buttons, _ctx.Tuning.ChordWindowFrames))
                    {
                        continue;
                    }

                    if (!trigger.Direction.Matches(input.Get(frame).Direction(f.Facing)))
                    {
                        continue;
                    }

                    if (trigger.Motion != null && !trigger.Motion.MatchesBefore(input, frame, f.Facing, motionFloor))
                    {
                        continue;
                    }

                    return frame;
                }

                if (trigger.Motion.CompletesOn(input, frame, f.Facing, motionFloor) && trigger.Direction.Matches(input.Get(frame).Direction(f.Facing)))
                {
                    return frame;
                }
            }

            return -1;
        }

        private bool AreConditionsMet(int index, ref FighterState f, TriggerConditions conditions)
        {
            if (conditions == TriggerConditions.None)
            {
                return true;
            }

            if ((conditions & TriggerConditions.RageActive) != 0 && !f.InRage)
            {
                return false;
            }

            if ((conditions & TriggerConditions.RageInactive) != 0 && f.InRage)
            {
                return false;
            }

            if ((conditions & TriggerConditions.ShadowActive) != 0 && !f.InShadow)
            {
                return false;
            }

            if ((conditions & TriggerConditions.ShadowInactive) != 0 && f.InShadow)
            {
                return false;
            }

            if ((conditions & TriggerConditions.TargetExecutable) != 0)
            {
                if (!_ctx.Rules.ExecutionsEnabled || f.TargetIndex < 0 || !f.Grounded)
                {
                    return false;
                }

                ref FighterState target = ref _ctx.PreLogic[f.TargetIndex];
                if (!_ctx.IsExecutable(target))
                {
                    return false;
                }

                if (Fixed.Abs(target.Position.X - f.Position.X) > _ctx.Tuning.ExecutionRange)
                {
                    return false;
                }
            }

            return true;
        }

        // ------------------------------------------------------------------ stun states

        private void UpdateBlockstun(int index, ref FighterState f)
        {
            if (_ctx.IsFighting && _ctx.Rules.RageBurstEnabled && TryStartFromNeutral(index, ref f, TriggerStance.Stunned))
            {
                return;
            }

            var current = _ctx.State.Inputs[index].Get(_ctx.Frame);
            f.CrouchGuard = DirectionMask.AnyDown.Matches(current.Direction(f.Facing));
            f.ActionFrame++;

            // "Blockstun N" = N full frames of stun; the fighter acts on frame N + 1.
            if (f.StunRemaining > 0)
            {
                f.StunRemaining--;
                return;
            }

            f.ThrowInvulnerableFrames = _ctx.Tuning.ThrowInvulnerableAfterStunFrames;
            SetAction(ref f, FighterAction.Idle);
            UpdateGroundNeutral(index, ref f);
        }

        private void UpdateHitstun(int index, ref FighterState f)
        {
            if (_ctx.IsFighting && _ctx.Rules.RageBurstEnabled && TryStartFromNeutral(index, ref f, TriggerStance.Stunned))
            {
                return;
            }

            f.ActionFrame++;

            // "Hitstun N" = N full frames of stun; the fighter acts on frame N + 1.
            if (f.StunRemaining > 0)
            {
                f.StunRemaining--;
                return;
            }

            EndCombo(index, ref f);
            f.ThrowInvulnerableFrames = _ctx.Tuning.ThrowInvulnerableAfterStunFrames;
            SetAction(ref f, FighterAction.Idle);
            UpdateGroundNeutral(index, ref f);
        }

        private void UpdateAirHitstun(int index, ref FighterState f)
        {
            if (_ctx.IsFighting && _ctx.Rules.RageBurstEnabled && TryStartFromNeutral(index, ref f, TriggerStance.Stunned))
            {
                return;
            }

            f.ActionFrame++;
            if (f.StunRemaining > 0)
            {
                f.StunRemaining--;
                return;
            }

            EndCombo(index, ref f);
            SetAction(ref f, FighterAction.Airborne);
            f.AirActionsUsed = _ctx.Tuning.MaxAirActions;
        }

        private void UpdateKnockdown(int index, ref FighterState f)
        {
            f.ActionFrame++;
            var blueprint = _ctx.Blueprints[index];
            if (!f.HardKnockdown && _ctx.IsFighting && f.ActionFrame <= _ctx.Tuning.TechWindowFrames && blueprint.TechRollMoveIndex >= 0)
            {
                var input = _ctx.State.Inputs[index];
                int press = input.FindBufferedPress(InputButtons.Dodge, f.LocalFrame, _ctx.Tuning.InputBufferFrames, _ctx.Tuning.ChordWindowFrames);
                if (press >= 0)
                {
                    input.ConsumeThrough(press);
                    EndCombo(index, ref f);
                    _ctx.Emit(CombatEventType.TechRoll, index, -1, f.Position);
                    StartMove(index, ref f, blueprint.TechRollMoveIndex, autoFace: false);
                    return;
                }
            }

            int duration = f.HardKnockdown ? blueprint.Body.HardKnockdownFrames : blueprint.Body.SoftKnockdownFrames;
            if (f.ActionFrame > duration)
            {
                EndCombo(index, ref f);
                SetAction(ref f, FighterAction.WakeUp);
                _ctx.Emit(CombatEventType.WakeUp, index, -1, f.Position);
            }
        }

        private void UpdateWakeUp(int index, ref FighterState f)
        {
            f.ActionFrame++;
            if (f.ActionFrame > _ctx.Blueprints[index].Body.WakeUpFrames)
            {
                f.ThrowInvulnerableFrames = _ctx.Tuning.ThrowInvulnerableAfterStunFrames;
                SetAction(ref f, FighterAction.Idle);
                UpdateGroundNeutral(index, ref f);
            }
        }

        private void UpdateStaggered(int index, ref FighterState f)
        {
            f.ActionFrame++;
            if (f.StunRemaining > 0)
            {
                f.StunRemaining--;
                return;
            }

            if (f.Stagger == StaggerKind.GuardBreak)
            {
                f.Posture = 0;
            }

            f.Stagger = StaggerKind.None;
            EndCombo(index, ref f);
            SetAction(ref f, FighterAction.Idle);
            UpdateGroundNeutral(index, ref f);
        }

        private void UpdatePairedVictim(int index, ref FighterState f)
        {
            f.ActionFrame++;
            if (f.PairedPartner < 0 || !IsHolding(f.PairedPartner, index))
            {
                // The holder was interrupted without releasing (safety net): fall free.
                f.PairedPartner = -1;
                f.PairedTechRemaining = 0;
                SetAction(ref f, f.Grounded ? FighterAction.Idle : FighterAction.Launched);
                return;
            }

            if (f.PairedTechRemaining <= 0)
            {
                return;
            }

            f.PairedTechRemaining--;
            if (!_ctx.IsFighting)
            {
                return;
            }

            var input = _ctx.State.Inputs[index];
            int press = input.FindBufferedPress(InputButtons.Grab, f.LocalFrame, _ctx.Tuning.InputBufferFrames, _ctx.Tuning.ChordWindowFrames);
            if (press >= 0)
            {
                input.ConsumeThrough(press);
                ThrowTech(f.PairedPartner, index);
            }
        }

        // ------------------------------------------------------------------ paired actions

        private bool IsHolding(int holderIndex, int victimIndex)
        {
            ref FighterState holder = ref _ctx.State.Fighters[holderIndex];
            return holder.Active && holder.Action == FighterAction.Move && holder.PairedPartner == victimIndex;
        }

        private void ApplyPairedFrame(int index, ref FighterState f, MoveDefinition move)
        {
            int victimIndex = f.PairedPartner;
            ref FighterState victim = ref _ctx.State.Fighters[victimIndex];
            if (victim.Action != FighterAction.PairedVictim || victim.PairedPartner != index)
            {
                f.PairedPartner = -1;
                return;
            }

            foreach (var hit in move.Paired.Hits)
            {
                if (hit.Frame == f.ActionFrame)
                {
                    _ctx.Hits.ApplyPairedHit(index, victimIndex, hit, move);
                }
            }

            if (f.ActionFrame == move.Paired.ReleaseFrame)
            {
                ReleasePartner(index, ref f, move);
            }
        }

        /// <summary>Releases a held victim using the move's release attack.</summary>
        public void ReleasePartner(int index, ref FighterState f, MoveDefinition move)
        {
            int victimIndex = f.PairedPartner;
            f.PairedPartner = -1;
            if (victimIndex < 0)
            {
                return;
            }

            ref FighterState victim = ref _ctx.State.Fighters[victimIndex];
            if (victim.Action != FighterAction.PairedVictim || victim.PairedPartner != index)
            {
                return;
            }

            var spec = move.Paired;
            victim.PairedPartner = -1;
            victim.PairedTechRemaining = 0;
            if (spec.AttackerTurnsAround)
            {
                f.Facing = -f.Facing;
            }

            victim.Position = f.Position + spec.ReleaseOffset.Facing(f.Facing);
            victim.Grounded = victim.Position.Y.Raw <= 0;
            if (victim.Grounded)
            {
                victim.Position = victim.Position.WithY(Fixed.Zero);
            }

            victim.Facing = -f.Facing;
            _ctx.Emit(CombatEventType.PairedRelease, index, victimIndex, f.MoveInstance, 0, 0, victim.Position, CombatEventFlags.None);
            var release = move.Attacks[spec.ReleaseAttackIndex];
            _ctx.Hits.ApplyReaction(index, f.Facing, victimIndex, release, counter: false, hitstun: release.Hitstun, lethal: victim.IsKnockedOut);
        }

        /// <summary>Breaks a grab: both fighters are pushed apart into a short neutral recovery.</summary>
        public void ThrowTech(int attackerIndex, int victimIndex)
        {
            ref FighterState attacker = ref _ctx.State.Fighters[attackerIndex];
            ref FighterState victim = ref _ctx.State.Fighters[victimIndex];
            attacker.PairedPartner = -1;
            victim.PairedPartner = -1;
            victim.PairedTechRemaining = 0;
            var point = new FixedVector2(
                Fixed.FromRaw((attacker.Position.X.Raw + victim.Position.X.Raw) / 2),
                Fixed.FromInt(1));
            int attackerSide = attacker.Position.X < victim.Position.X ? -1 : (attacker.Position.X > victim.Position.X ? 1 : -attacker.Facing);
            EnterTechRecovery(ref attacker, attackerSide);
            EnterTechRecovery(ref victim, -attackerSide);
            _ctx.Emit(CombatEventType.ThrowTech, victimIndex, attackerIndex, point);
        }

        private void EnterTechRecovery(ref FighterState f, int awaySign)
        {
            f.MoveIndex = -1;
            f.Grounded = true;
            f.Position = f.Position.WithY(Fixed.Zero);
            f.Action = FighterAction.Blockstun;
            f.ActionFrame = 0;
            f.StunRemaining = _ctx.Tuning.ThrowTechRecoveryFrames;
            f.Velocity = new FixedVector2(_ctx.Tuning.ThrowTechPushback * awaySign, Fixed.Zero);
            f.CrouchGuard = false;
        }

        // ------------------------------------------------------------------ landing

        /// <summary>Called by physics when an airborne fighter touches the ground.</summary>
        public void OnLanded(int index)
        {
            ref FighterState f = ref _ctx.State.Fighters[index];
            f.Grounded = true;
            f.Position = f.Position.WithY(Fixed.Zero);
            f.Velocity = f.Velocity.WithY(Fixed.Zero);
            f.AirActionsUsed = 0;

            switch (f.Action)
            {
                case FighterAction.Airborne:
                    EnterLanding(index, ref f, _ctx.Blueprints[index].Body.LandingFrames);
                    break;
                case FighterAction.AirHitstun:
                    EndCombo(index, ref f);
                    EnterLanding(index, ref f, _ctx.Tuning.AirResetLandingFrames);
                    break;
                case FighterAction.Launched:
                    LandFromLaunch(index, ref f);
                    break;
                case FighterAction.Move:
                    var move = _ctx.Blueprints[index].Moves[f.MoveIndex];
                    if (move.HasFlag(MoveFlags.LandCancel))
                    {
                        f.MoveIndex = -1;
                        EnterLanding(index, ref f, move.LandingRecovery);
                    }

                    break;
            }
        }

        private void EnterLanding(int index, ref FighterState f, int recovery)
        {
            SetAction(ref f, FighterAction.Landing);
            f.StunRemaining = recovery;
            f.Velocity = FixedVector2.Zero;
            _ctx.Emit(CombatEventType.Land, index, -1, f.Position);
        }

        private void LandFromLaunch(int index, ref FighterState f)
        {
            if (f.PendingGroundBounce && !f.GroundBounceUsed)
            {
                f.PendingGroundBounce = false;
                f.GroundBounceUsed = true;
                f.Grounded = false;
                f.Velocity = new FixedVector2(f.Velocity.X / 2, _ctx.Tuning.GroundBounceVelocity);
                _ctx.Emit(CombatEventType.GroundBounce, f.ComboAttacker, index, f.Position);
                return;
            }

            f.PendingWallBounce = false;
            f.PendingGroundBounce = false;
            f.Velocity = FixedVector2.Zero;
            var flags = f.IsKnockedOut ? CombatEventFlags.Lethal : CombatEventFlags.None;
            SetAction(ref f, f.IsKnockedOut ? FighterAction.KnockedOut : FighterAction.Knockdown);
            _ctx.Emit(CombatEventType.Knockdown, f.ComboAttacker, index, 0, f.HardKnockdown ? 1 : 0, 0, f.Position, flags);
        }

        // ------------------------------------------------------------------ helpers

        private void ScanGuardPresses(int index, ref FighterState f)
        {
            var input = _ctx.State.Inputs[index];
            bool pressed = false;
            int from = System.Math.Max(f.LastLogicWorldFrame + 1, input.EarliestEdgeFrame);
            for (int frame = from; frame <= _ctx.Frame; frame++)
            {
                if (input.WasPressed(frame, InputButtons.Guard))
                {
                    pressed = true;
                }
            }

            if (!pressed)
            {
                return;
            }

            int local = f.LocalFrame;
            bool mashing = local - f.LastGuardPressLocal <= _ctx.Tuning.ParryMashThresholdFrames;
            f.LastGuardPressLocal = local;
            if (!_ctx.IsFighting || !IsParryCapable(f.Action) || f.IsKnockedOut)
            {
                return;
            }

            int window = mashing ? _ctx.Tuning.ParryWindowMashFrames : _ctx.Tuning.ParryWindowFrames;
            f.ParryWindowEndLocal = local + window - 1;
        }

        /// <summary>True for states in which a Guard press opens a Perfect Parry window.</summary>
        public static bool IsParryCapable(FighterAction action)
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
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsStunned(FighterAction action) =>
            action == FighterAction.Hitstun || action == FighterAction.Blockstun || action == FighterAction.AirHitstun || action == FighterAction.Launched;

        /// <summary>Ends the fighter's current combo (as victim) and reports it.</summary>
        public void EndCombo(int index, ref FighterState f)
        {
            if (f.ComboHits > 0)
            {
                _ctx.Emit(CombatEventType.ComboEnd, f.ComboAttacker, index, 0, f.ComboDamage, f.ComboHits, f.Position, CombatEventFlags.None);
            }

            f.ComboHits = 0;
            f.ComboDamage = 0;
            f.JugglePoints = 0;
            f.ComboStarterPermille = 1000;
            f.WallBounceUsed = false;
            f.GroundBounceUsed = false;
            f.OffTheGroundUsed = false;
            f.PendingWallBounce = false;
            f.PendingGroundBounce = false;
        }

        /// <summary>Turns the fighter toward its target.</summary>
        public void FaceTarget(int index, ref FighterState f)
        {
            if (f.TargetIndex < 0)
            {
                return;
            }

            Fixed dx = _ctx.PreLogic[f.TargetIndex].Position.X - f.Position.X;
            if (dx.Raw != 0)
            {
                f.Facing = dx.Sign;
            }
        }

        private static void SetAction(ref FighterState f, FighterAction action)
        {
            if (f.Action != action)
            {
                f.Action = action;
                f.ActionFrame = 0;
            }
            else
            {
                f.ActionFrame++;
            }

            if (action != FighterAction.Move)
            {
                f.MoveIndex = -1;
            }
        }

        private readonly struct Selection
        {
            public static readonly Selection None = new Selection(-1, int.MaxValue, int.MinValue, 0);

            public readonly int Move;
            public readonly int PressFrame;
            public readonly int Priority;
            public readonly int StartupPenalty;

            public Selection(int move, int pressFrame, int priority, int startupPenalty)
            {
                Move = move;
                PressFrame = pressFrame;
                Priority = priority;
                StartupPenalty = startupPenalty;
            }
        }
    }
}

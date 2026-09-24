using System;
using System.Collections.Generic;
using Oathsunder.Combat.Definitions;
using Oathsunder.Combat.Events;
using Oathsunder.Combat.Input;
using Oathsunder.Combat.Simulation;
using Oathsunder.Core.Mathematics;
using NUnit.Framework;

namespace Oathsunder.Tests.Support
{
    /// <summary>
    /// Drives a 1v1 encounter frame by frame for gameplay tests. Player 0 starts on the left facing right,
    /// player 1 on the right facing left. Directions passed to helpers are <b>facing-relative</b> numpad values.
    /// </summary>
    public sealed class CombatHarness
    {
        private readonly InputFrame[] _inputs = new InputFrame[2];
        private readonly InputButtons[] _held = new InputButtons[2];
        private readonly int[] _heldDirection = { 5, 5 };

        /// <summary>Creates a harness with shipped content and training rules (no countdown, no timer).</summary>
        public CombatHarness(Action<CombatSetup> configure = null)
        {
            var setup = new CombatSetup
            {
                Stage = TestContent.Stage("stage.training.dojo"),
                Tuning = TestContent.Tuning(),
                Rules = TestContent.Rules("rules.training"),
            };
            setup.Combatants.Add(new CombatantSetup(TestContent.RhenKatana(), team: 0));
            setup.Combatants.Add(new CombatantSetup(TestContent.RhenKatana(), team: 1));
            configure?.Invoke(setup);
            World = new CombatWorld(setup);
            Step();
        }

        /// <summary>The encounter.</summary>
        public CombatWorld World { get; }

        /// <summary>Every event since the harness was created (or since <see cref="ClearEvents"/>).</summary>
        public List<CombatEvent> Events { get; } = new List<CombatEvent>();

        /// <summary>Player 0.</summary>
        public ref FighterState P0 => ref World.State.Fighters[0];

        /// <summary>Player 1.</summary>
        public ref FighterState P1 => ref World.State.Fighters[1];

        /// <summary>Current world frame.</summary>
        public int Frame => World.State.Frame;

        /// <summary>Metres to fixed.</summary>
        public static Fixed M(double metres) => Fixed.FromRatio((long)Math.Round(metres * 1000), 1000);

        /// <summary>Places both fighters (feet on the ground) at a distance, centred, facing each other.</summary>
        public void PlaceApart(double distance, double centre = 0)
        {
            ref FighterState a = ref P0;
            ref FighterState b = ref P1;
            a.Position = new FixedVector2(M(centre - distance / 2), Fixed.Zero);
            b.Position = new FixedVector2(M(centre + distance / 2), Fixed.Zero);
            a.Facing = 1;
            b.Facing = -1;
        }

        /// <summary>Holds a facing-relative direction (numpad) for a player until changed.</summary>
        public void HoldDirection(int player, int numpad) => _heldDirection[player] = numpad;

        /// <summary>Holds buttons for a player until released.</summary>
        public void Hold(int player, InputButtons buttons) => _held[player] |= buttons;

        /// <summary>Releases held buttons.</summary>
        public void Release(int player, InputButtons buttons) => _held[player] &= ~buttons;

        /// <summary>Releases everything for both players.</summary>
        public void ReleaseAll()
        {
            _held[0] = _held[1] = InputButtons.None;
            _heldDirection[0] = _heldDirection[1] = 5;
        }

        /// <summary>Steps one frame with the held state plus optional extra buttons / direction overrides.</summary>
        public void Step(InputButtons extra0 = InputButtons.None, InputButtons extra1 = InputButtons.None, int direction0 = 0, int direction1 = 0)
        {
            _inputs[0] = Build(0, extra0, direction0);
            _inputs[1] = Build(1, extra1, direction1);
            World.Step(_inputs);
            for (int i = 0; i < World.Events.Count; i++)
            {
                Events.Add(World.Events[i]);
            }

            Assert.AreEqual(0, World.Events.Dropped, "event buffer overflow");
        }

        /// <summary>Steps <paramref name="frames"/> frames with only held input.</summary>
        public void Wait(int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                Step();
            }
        }

        /// <summary>Presses a button for exactly one frame (then it is released), with an optional direction.</summary>
        public void Tap(int player, InputButtons buttons, int direction = 0)
        {
            if (player == 0)
            {
                Step(buttons, InputButtons.None, direction, 0);
            }
            else
            {
                Step(InputButtons.None, buttons, 0, direction);
            }
        }

        /// <summary>Feeds a sequence of facing-relative directions (one per frame) then presses a button with the last one.</summary>
        public void Motion(int player, string numpadSequence, InputButtons button)
        {
            for (int i = 0; i < numpadSequence.Length - 1; i++)
            {
                StepDirection(player, numpadSequence[i] - '0', InputButtons.None);
            }

            StepDirection(player, numpadSequence[numpadSequence.Length - 1] - '0', button);
        }

        /// <summary>Steps until the predicate holds; fails after <paramref name="maxFrames"/>.</summary>
        public int RunUntil(Func<bool> predicate, int maxFrames, string what)
        {
            for (int i = 0; i < maxFrames; i++)
            {
                if (predicate())
                {
                    return i;
                }

                Step();
            }

            if (predicate())
            {
                return maxFrames;
            }

            Assert.Fail($"Timed out after {maxFrames} frames waiting for: {what}");
            return -1;
        }

        /// <summary>Clears the recorded event history.</summary>
        public void ClearEvents() => Events.Clear();

        /// <summary>Number of recorded events of a type.</summary>
        public int Count(CombatEventType type)
        {
            int count = 0;
            foreach (var e in Events)
            {
                if (e.Type == type)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>First recorded event of a type (fails when absent).</summary>
        public CombatEvent First(CombatEventType type)
        {
            foreach (var e in Events)
            {
                if (e.Type == type)
                {
                    return e;
                }
            }

            Assert.Fail($"No {type} event recorded.");
            return default;
        }

        /// <summary>True when an event of the type was recorded.</summary>
        public bool Has(CombatEventType type) => Count(type) > 0;

        /// <summary>Id of the fighter's current move ("" when not in a move).</summary>
        public string MoveId(int player)
        {
            ref FighterState f = ref World.State.Fighters[player];
            return f.Action == FighterAction.Move ? World.BlueprintOf(player).Moves[f.MoveIndex].Id : "";
        }

        /// <summary>Index of a move in a fighter's blueprint.</summary>
        public int MoveIndex(int player, string id)
        {
            int index = World.BlueprintOf(player).FindMoveIndex(id);
            Assert.GreaterOrEqual(index, 0, $"move {id} not found");
            return index;
        }

        private void StepDirection(int player, int numpad, InputButtons button)
        {
            if (player == 0)
            {
                Step(button, InputButtons.None, numpad, 0);
            }
            else
            {
                Step(InputButtons.None, button, 0, numpad);
            }
        }

        private InputFrame Build(int player, InputButtons extra, int directionOverride)
        {
            int numpad = directionOverride != 0 ? directionOverride : _heldDirection[player];
            int relativeX = ((numpad - 1) % 3) - 1;
            int y = ((numpad - 1) / 3) - 1;
            int facing = World == null ? (player == 0 ? 1 : -1) : World.State.Fighters[player].Facing;
            return new InputFrame(_held[player] | extra, relativeX * facing, y);
        }
    }
}

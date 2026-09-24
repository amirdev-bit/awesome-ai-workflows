using System;
using Oathsunder.Combat.Definitions;
using Oathsunder.Combat.Events;
using Oathsunder.Combat.Input;
using Oathsunder.Controls;
using Oathsunder.Core.Serialization;
using Oathsunder.Tests.Support;
using NUnit.Framework;

namespace Oathsunder.Tests.Controls
{
    [TestFixture]
    public sealed class InputLatchTests
    {
        [Test]
        public void TapShorterThanATickIsNotLost()
        {
            var latch = new InputLatch();
            latch.Sample(InputButtons.None, 0, 0);
            latch.Sample(InputButtons.Light, 0, 0);
            latch.Sample(InputButtons.None, 0, 0);
            latch.Sample(InputButtons.None, 0, 0);
            Assert.IsTrue(latch.ConsumeTick().IsHeld(InputButtons.Light), "a 4 ms tap at 240 FPS still reaches the simulation");
            Assert.IsFalse(latch.ConsumeTick().IsHeld(InputButtons.Light), "…and is released on the next tick");
        }

        [Test]
        public void HeldButtonsStayHeld()
        {
            var latch = new InputLatch();
            latch.Sample(InputButtons.Guard, 0, 0);
            Assert.IsTrue(latch.ConsumeTick().IsHeld(InputButtons.Guard));
            latch.Sample(InputButtons.Guard, 0, 0);
            Assert.IsTrue(latch.ConsumeTick().IsHeld(InputButtons.Guard));
        }

        [Test]
        public void DirectionTapInsideATickSurvives()
        {
            var latch = new InputLatch();
            latch.Sample(InputButtons.None, 1, 0);
            latch.Sample(InputButtons.None, 0, 0);
            var frame = latch.ConsumeTick();
            Assert.AreEqual(1, frame.X);
            Assert.AreEqual(0, latch.ConsumeTick().X, "a released direction is neutral on the following tick");
        }

        [Test]
        public void LatestDirectionWinsWhenHeld()
        {
            var latch = new InputLatch();
            latch.Sample(InputButtons.None, 0, -1);
            latch.Sample(InputButtons.None, 1, -1);
            latch.Sample(InputButtons.None, 1, 0);
            var frame = latch.ConsumeTick();
            Assert.AreEqual(1, frame.X);
            Assert.AreEqual(0, frame.Y);
        }

        [Test]
        public void TicksWithoutSamplesRepeatTheLastState()
        {
            var latch = new InputLatch();
            latch.Sample(InputButtons.Heavy, -1, 0);
            latch.ConsumeTick();
            var frame = latch.ConsumeTick();
            Assert.IsTrue(frame.IsHeld(InputButtons.Heavy));
            Assert.AreEqual(-1, frame.X);
        }
    }

    [TestFixture]
    public sealed class DirectionFilterTests
    {
        [TestCase(0.2f, 0.1f, 0, 0)]
        [TestCase(1f, 0f, 1, 0)]
        [TestCase(-1f, 0f, -1, 0)]
        [TestCase(0f, -1f, 0, -1)]
        [TestCase(0.71f, 0.71f, 1, 1)]
        [TestCase(-0.71f, -0.71f, -1, -1)]
        [TestCase(0.9f, 0.3f, 1, 0)]
        [TestCase(-0.95f, -0.3f, -1, 0)]
        public void StickQuantisesWithCardinalBias(float ax, float ay, int x, int y)
        {
            DigitalStick.Default.Quantize(ax, ay, out int qx, out int qy);
            Assert.AreEqual(x, qx, "x");
            Assert.AreEqual(y, qy, "y");
        }

        [Test]
        public void StickRejectsInvalidSettings()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DigitalStick(1f, 30f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DigitalStick(0.2f, 90f));
        }

        [Test]
        public void SocdNeutralisesHorizontalAndPrefersUp()
        {
            var socd = new SocdResolver(SocdMode.NeutralHorizontalUpPriority);
            socd.Resolve(true, true, true, true, out int x, out int y);
            Assert.AreEqual(0, x);
            Assert.AreEqual(1, y);
        }

        [Test]
        public void SocdLastInputWins()
        {
            var socd = new SocdResolver(SocdMode.LastInputWins);
            socd.Resolve(true, false, false, false, out int x, out _);
            Assert.AreEqual(-1, x);
            socd.Resolve(true, true, false, false, out x, out _);
            Assert.AreEqual(1, x, "right was pressed last");
            socd.Resolve(false, true, false, false, out x, out _);
            Assert.AreEqual(1, x);
        }

        [Test]
        public void SocdNeutralBoth()
        {
            var socd = new SocdResolver(SocdMode.NeutralBoth);
            socd.Resolve(false, false, true, true, out _, out int y);
            Assert.AreEqual(0, y);
        }
    }

    [TestFixture]
    public sealed class ControlProfileTests
    {
        [Test]
        public void DefaultsBindEveryButtonWithoutConflicts()
        {
            var profile = ControlProfile.CreateDefault(ControlScheme.Classic);
            foreach (var button in ControlProfile.RemappableButtons)
            {
                Assert.GreaterOrEqual(profile.BindingsOf(button).Count, 2, $"{button} needs gamepad and keyboard defaults");
            }

            Assert.IsEmpty(profile.FindConflicts());
        }

        [Test]
        public void JsonRoundTripPreservesEverything()
        {
            var profile = ControlProfile.CreateDefault(ControlScheme.Simplified);
            profile.UpToJump = true;
            profile.Socd = SocdMode.LastInputWins;
            profile.StickDeadzone = 0.25f;
            profile.TouchLayoutId = TouchLayout.TabletId;
            profile.TouchButtonScale = 1.3f;
            profile.Rebind(InputButtons.Light, "<Gamepad>/buttonSouth", "<Gamepad>/buttonWest");

            var copy = ControlProfile.FromJson(profile.ToJson());
            Assert.AreEqual(profile.ToJson(), copy.ToJson());
            Assert.AreEqual(ControlScheme.Simplified, copy.Scheme);
            Assert.IsTrue(copy.UpToJump);
            Assert.AreEqual(TouchLayout.TabletId, copy.TouchLayoutId);
            CollectionAssert.Contains(copy.BindingsOf(InputButtons.Light), "<Gamepad>/buttonSouth");
        }

        [Test]
        public void RebindingStealsTheControlFromItsPreviousOwner()
        {
            var profile = ControlProfile.CreateDefault(ControlScheme.Classic);
            var displaced = profile.Rebind(InputButtons.Light, "<Gamepad>/buttonSouth", "<Gamepad>/buttonWest");
            Assert.AreEqual(InputButtons.Jump, displaced);
            CollectionAssert.DoesNotContain(profile.BindingsOf(InputButtons.Jump), "<Gamepad>/buttonSouth");
            Assert.IsEmpty(profile.FindConflicts());
        }

        [Test]
        public void MalformedProfilesAreRejectedWithPaths()
        {
            Assert.Throws<JsonSyntaxException>(() => ControlProfile.FromJson("{"));
            var exception = Assert.Throws<JsonContentException>(() => ControlProfile.FromJson("{\"bindings\":{\"Kick\":[]}}"));
            StringAssert.Contains("Kick", exception.Message);
            Assert.Throws<JsonContentException>(() => ControlProfile.FromJson("{\"version\":99}"));
        }

        [Test]
        public void OutOfRangeSettingsAreClamped()
        {
            var profile = ControlProfile.FromJson("{\"stickDeadzone\":5,\"touchButtonScale\":0.1}");
            Assert.AreEqual(0.9f, profile.StickDeadzone);
            Assert.AreEqual(0.6f, profile.TouchButtonScale);
        }

        [Test]
        public void ComposerAppliesProfileConveniences()
        {
            var profile = ControlProfile.CreateDefault(ControlScheme.Classic);
            profile.UpToJump = true;
            var composer = new InputComposer(profile);
            var snapshot = new DeviceSnapshot { Held = InputButtons.Heavy | InputButtons.Special, Up = true };
            composer.Compose(snapshot, out var buttons, out int x, out int y);
            Assert.IsTrue(buttons.HasAll(InputButtons.Execute), "Heavy + Special chord sends Execute");
            Assert.IsTrue(buttons.HasAll(InputButtons.Jump), "up-to-jump");
            Assert.AreEqual(0, x);
            Assert.AreEqual(1, y);
        }

        [Test]
        public void DigitalBeatsTouchBeatsAnalog()
        {
            var composer = new InputComposer(ControlProfile.CreateDefault(ControlScheme.Simplified));
            var snapshot = new DeviceSnapshot { StickX = -1f, TouchStickActive = true, TouchStickX = 1f };
            composer.Compose(snapshot, out _, out int x, out _);
            Assert.AreEqual(1, x, "touch stick wins over the gamepad stick");
            snapshot.Left = true;
            composer.Compose(snapshot, out _, out x, out _);
            Assert.AreEqual(-1, x, "digital input wins over sticks");
        }
    }

    [TestFixture]
    public sealed class TouchControlTests
    {
        [TestCase(16f / 9f)]
        [TestCase(19.5f / 9f)]
        [TestCase(4f / 3f)]
        public void ShippedLayoutsAreValid(float aspect)
        {
            // Phones ship at 16:10 or wider; tablets go down to 4:3.
            CollectionAssert.IsEmpty(TouchLayout.Phone().Validate(aspect < 1.6f ? 1.6f : aspect), "phone");
            CollectionAssert.IsEmpty(TouchLayout.Tablet().Validate(aspect), "tablet");
        }

        [Test]
        public void EveryCombatButtonHasATouchControl()
        {
            foreach (var layout in new[] { TouchLayout.Phone(), TouchLayout.Tablet() })
            {
                foreach (var button in ControlProfile.RemappableButtons)
                {
                    bool found = false;
                    foreach (var region in layout.Buttons)
                    {
                        found |= region.Button == button;
                    }

                    Assert.IsTrue(found, $"{layout.Id} has no {button} button");
                }
            }
        }

        [Test]
        public void FloatingStickAnchorsWhereTheThumbLands()
        {
            var resolver = new TouchControlResolver(TouchLayout.Phone());
            const float width = 2400f;
            const float height = 1080f;
            resolver.Update(1, TouchPhaseKind.Began, 300f, 300f, width, height);
            Assert.IsTrue(resolver.StickActive);
            resolver.Update(1, TouchPhaseKind.Moved, 300f + 0.11f * height, 300f, width, height);
            Assert.AreEqual(1f, resolver.StickX, 1e-3f);
            Assert.AreEqual(0f, resolver.StickY, 1e-3f);
            resolver.Update(1, TouchPhaseKind.Moved, 300f + 5f * height, 300f, width, height);
            Assert.AreEqual(1f, resolver.StickX, 1e-3f, "clamped to the throw radius");
            resolver.Update(1, TouchPhaseKind.Ended, 0f, 0f, width, height);
            Assert.IsFalse(resolver.StickActive);
            Assert.AreEqual(0f, resolver.StickX);
        }

        [Test]
        public void ButtonsPressAndSlide()
        {
            var layout = TouchLayout.Phone();
            var resolver = new TouchControlResolver(layout);
            const float width = 2400f;
            const float height = 1080f;
            float aspect = width / height;
            var light = Find(layout, InputButtons.Light);
            var heavy = Find(layout, InputButtons.Heavy);

            resolver.Update(7, TouchPhaseKind.Began, (aspect - light.CenterFromRight) * height, light.CenterY * height, width, height);
            Assert.AreEqual(InputButtons.Light, resolver.Held);
            resolver.Update(7, TouchPhaseKind.Moved, (aspect - heavy.CenterFromRight) * height, heavy.CenterY * height, width, height);
            Assert.AreEqual(InputButtons.Heavy, resolver.Held, "sliding onto a neighbour presses it");
            resolver.Update(8, TouchPhaseKind.Began, 300f, 300f, width, height);
            Assert.AreEqual(InputButtons.Heavy, resolver.Held, "the stick finger does not press buttons");
            resolver.Update(7, TouchPhaseKind.Ended, 0f, 0f, width, height);
            Assert.AreEqual(InputButtons.None, resolver.Held);
        }

        [Test]
        public void TouchInputDrivesTheSimulation()
        {
            // Simplified touch player: forward on the floating stick + Special = Crescent Rush.
            var harness = new CombatHarness(setup => setup.Combatants[0].Scheme = ControlScheme.Simplified);
            harness.PlaceApart(3);
            var layout = TouchLayout.Phone();
            var resolver = new TouchControlResolver(layout);
            var profile = ControlProfile.CreateDefault(ControlScheme.Simplified);
            var composer = new InputComposer(profile);
            var latch = new InputLatch();
            const float width = 2400f;
            const float height = 1080f;
            var special = Find(layout, InputButtons.Special);

            resolver.Update(1, TouchPhaseKind.Began, 300f, 300f, width, height);
            resolver.Update(1, TouchPhaseKind.Moved, 300f + 0.1f * height, 300f, width, height);
            resolver.Update(2, TouchPhaseKind.Began, (width / height - special.CenterFromRight) * height, special.CenterY * height, width, height);
            var snapshot = new DeviceSnapshot
            {
                Held = resolver.Held,
                TouchStickActive = resolver.StickActive,
                TouchStickX = resolver.StickX,
                TouchStickY = resolver.StickY,
            };
            composer.Compose(snapshot, out var buttons, out int x, out int y);
            latch.Sample(buttons, x, y);
            var frame = latch.ConsumeTick();

            harness.World.Step(new[] { frame, InputFrame.Neutral });
            Assert.AreEqual("katana.qcfs", harness.MoveId(0));
        }

        [Test]
        public void InputPipelineDoesNotAllocate()
        {
            var composer = new InputComposer(ControlProfile.CreateDefault(ControlScheme.Classic));
            var latch = new InputLatch();
            var resolver = new TouchControlResolver(TouchLayout.Phone());
            RunPipeline(composer, latch, resolver, 0, 200_000);

            // A per-frame allocation would show up in every window (≥ 100,000 frames each); a test host's one-off
            // bookkeeping allocation on this thread cannot, so the quietest window must be exactly zero.
            long quietest = long.MaxValue;
            for (int window = 0; window < 3; window++)
            {
                long before = GC.GetAllocatedBytesForCurrentThread();
                RunPipeline(composer, latch, resolver, 200_000 + window * 100_000, 100_000);
                quietest = Math.Min(quietest, GC.GetAllocatedBytesForCurrentThread() - before);
            }

            Assert.AreEqual(0, quietest, "per-frame input handling must not allocate");
        }

        private static void RunPipeline(InputComposer composer, InputLatch latch, TouchControlResolver resolver, int start, int count)
        {
            var snapshot = new DeviceSnapshot { StickX = 0.7f, StickY = -0.7f, Left = true };
            for (int i = start; i < start + count; i++)
            {
                resolver.Update(i & 3, (i & 7) == 0 ? TouchPhaseKind.Began : TouchPhaseKind.Moved, 300f + (i & 63), 300f + (i & 31) * 20f, 2400f, 1080f);
                snapshot.Held = resolver.Held | InputButtons.Light;
                composer.Compose(snapshot, out var buttons, out int x, out int y);
                latch.Sample(buttons, x, y);
                if ((i & 3) == 3)
                {
                    latch.ConsumeTick();
                }
            }
        }

        private static TouchButtonRegion Find(TouchLayout layout, InputButtons button)
        {
            foreach (var region in layout.Buttons)
            {
                if (region.Button == button)
                {
                    return region;
                }
            }

            throw new InvalidOperationException(button.ToString());
        }
    }
}

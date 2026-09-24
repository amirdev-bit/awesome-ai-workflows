using System.Collections;
using System.IO;
using Oathsunder.Combat.Content;
using Oathsunder.Combat.Events;
using Oathsunder.Combat.Input;
using Oathsunder.Combat.Simulation;
using Oathsunder.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Oathsunder.Tests.PlayMode
{
    /// <summary>Integration tests of the Unity bridge: real frame loop, fixed-tick runner, event dispatch.</summary>
    public sealed class CombatRunnerPlayModeTests
    {
        private static CombatSetup CreateSetup()
        {
            var content = new CombatContentSet();
            string root = Path.Combine(Application.dataPath, "_Project/Content/Combat");
            foreach (string file in Directory.GetFiles(root, "*.json", SearchOption.AllDirectories))
            {
                content.Add(file, File.ReadAllText(file));
            }

            Assert.IsEmpty(content.Errors);
            var setup = new CombatSetup
            {
                Stage = content.Stage("stage.training.dojo"),
                Rules = content.Rules("rules.training"),
                Tuning = content.Tuning,
            };
            setup.Combatants.Add(new CombatantSetup(content.BuildBlueprint("fighter.rhen", "moveset.universal", "weapon.katana"), 0));
            setup.Combatants.Add(new CombatantSetup(content.BuildBlueprint("fighter.rhen", "moveset.universal", "weapon.katana"), 1));
            return setup;
        }

        [UnityTest]
        public IEnumerator RunnerTicksAtFixedRateAndDispatchesEvents()
        {
            var host = new GameObject("CombatRunner");
            var runner = host.AddComponent<CombatSimulationRunner>();
            runner.StartMatch(CreateSetup());
            var listener = new CountingListener();
            runner.AddListener(listener);

            var walkForward = new InputFrame[600];
            for (int i = 0; i < walkForward.Length; i++)
            {
                walkForward[i] = new InputFrame(InputButtons.None, 1, 0);
            }

            runner.SetInputSource(0, new RecordedInputSource(walkForward, loop: false));
            float startX = runner.World.State.Fighters[0].Position.X.ToFloat();

            for (int frame = 0; frame < 45; frame++)
            {
                yield return null;
            }

            Assert.Greater(runner.World.State.Frame, 0, "the runner ticks from Update");
            Assert.Greater(runner.World.State.Fighters[0].Position.X.ToFloat(), startX, "player 0 walked forward");
            Assert.Greater(listener.Count, 0, "events reach listeners");
            Assert.That(runner.Alpha, Is.InRange(0f, 1f));
            Object.Destroy(host);
        }

        [Test]
        public void ManualTickIsDeterministic()
        {
            var a = new GameObject("A").AddComponent<CombatSimulationRunner>();
            var b = new GameObject("B").AddComponent<CombatSimulationRunner>();
            a.StartMatch(CreateSetup());
            b.StartMatch(CreateSetup());
            for (int i = 0; i < 300; i++)
            {
                a.Tick();
                b.Tick();
            }

            Assert.AreEqual(a.World.Checksum(), b.World.Checksum());
            Object.DestroyImmediate(a.gameObject);
            Object.DestroyImmediate(b.gameObject);
        }

        private sealed class CountingListener : ICombatEventListener
        {
            public int Count { get; private set; }

            public void OnCombatEvent(CombatSimulationRunner runner, in CombatEvent combatEvent) => Count++;
        }
    }
}

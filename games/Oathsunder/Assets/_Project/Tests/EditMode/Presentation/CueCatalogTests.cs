using System;
using System.Collections.Generic;
using System.Linq;
using Oathsunder.Combat.Content;
using Oathsunder.Combat.Definitions;
using Oathsunder.Combat.Events;
using Oathsunder.Combat.Input;
using Oathsunder.Core.Mathematics;
using Oathsunder.Presentation.Cues;
using Oathsunder.Tests.Support;
using NUnit.Framework;

namespace Oathsunder.Tests.Presentation
{
    [TestFixture]
    public sealed class CueCatalogTests
    {
        [Test]
        public void ShippedCatalogParsesAndCoversEveryChannel()
        {
            var catalog = TestContent.Cues;
            Assert.AreEqual("cues.combat", catalog.Id);
            foreach (CueChannel channel in Enum.GetValues(typeof(CueChannel)))
            {
                Assert.IsTrue(catalog.OfChannel(channel).Any(), $"no {channel} cues in the catalog");
            }
        }

        [Test]
        public void EveryShippedMovePassesThePresentationDefinitionOfDone()
        {
            var errors = CueCoverage.Check(TestContent.RhenKatana(), TestContent.Cues);
            Assert.IsEmpty(errors, string.Join("\n", errors));
        }

        [Test]
        public void EveryCatalogCueIsUsedByContentOrEvents()
        {
            var catalog = TestContent.Cues;
            var used = new HashSet<string>(StringComparer.Ordinal);
            foreach (var move in TestContent.RhenKatana().Moves)
            {
                foreach (var cue in move.Cues)
                {
                    used.Add(cue.Name);
                }
            }

            foreach (var name in EventCueMap.All)
            {
                used.Add(name);
            }

            foreach (var cue in catalog.Cues.Where(c => c.Channel == CueChannel.Impact))
            {
                used.UnionWith(cue.Layers);
            }

            var orphans = catalog.Cues.Select(c => c.Name).Where(n => !used.Contains(n)).ToList();
            Assert.IsEmpty(orphans, "catalog cues nothing fires (dead production work): " + string.Join(", ", orphans));
        }

        [Test]
        public void EventCuesAreDeclaredWithTheirChannel()
        {
            var catalog = TestContent.Cues;
            foreach (var name in EventCueMap.All)
            {
                Assert.IsTrue(catalog.TryGet(name, out var cue), $"{name} missing from the catalog");
                Assert.AreEqual(CueNames.ChannelOf(name), cue.Channel);
            }
        }

        [Test]
        public void ImpactCuesExpandToTheirLayers()
        {
            var output = new List<CueDefinition>();
            TestContent.Cues.Expand(EventCueMap.HitCounter, output);
            CollectionAssert.AreEqual(
                new[] { "vfx.hit.counter", "vfx.blood.heavy", "sfx.impact.counter", "vo.hurt.heavy", "cam.shake.medium", "cam.zoom.counter", "haptic.medium" },
                output.Select(c => c.Name).ToArray());
            output.Clear();
            TestContent.Cues.Expand("sfx.parry", output);
            Assert.AreEqual(1, output.Count);
            output.Clear();
            TestContent.Cues.Expand("sfx.unknown", output);
            Assert.AreEqual(0, output.Count);
        }

        [Test]
        public void BloodIsAlwaysAGoreLayerSoTheToggleRemovesIt()
        {
            var blood = TestContent.Cues.Cues.Where(c => c.Name.StartsWith("vfx.blood.", StringComparison.Ordinal)).ToList();
            Assert.IsNotEmpty(blood);
            Assert.IsTrue(blood.All(c => c.Vfx.Gore));
            Assert.IsTrue(TestContent.Cues.Cues.Where(c => c.Channel == CueChannel.Vfx && c.Vfx.Gore).All(c => c.Name.StartsWith("vfx.blood.", StringComparison.Ordinal)),
                "only blood layers may be gore, so disabling blood never removes gameplay feedback");
        }

        [TestCase("sfx.katana.swing.light", CueChannel.Sfx)]
        [TestCase("vfx.trail.katana.heavy", CueChannel.Vfx)]
        [TestCase("impact.hit.light", CueChannel.Impact)]
        [TestCase("haptic.light", CueChannel.Haptic)]
        public void CueNamesParseTheirChannel(string name, CueChannel expected)
        {
            Assert.IsTrue(CueNames.TryParse(name, out var channel));
            Assert.AreEqual(expected, channel);
        }

        [TestCase("")]
        [TestCase("sfx")]
        [TestCase("sfx.")]
        [TestCase(".sfx.swing")]
        [TestCase("sfx..swing")]
        [TestCase("SFX.swing")]
        [TestCase("sfx.Swing")]
        [TestCase("sfx.swing-light")]
        [TestCase("music.theme")]
        public void MalformedCueNamesAreRejected(string name)
        {
            Assert.IsFalse(CueNames.TryParse(name, out _));
        }

        [TestCase(@"{""name"":""sfx.a.b"",""desc"":""A proper description."",""asset"":""SFX_A"",""variations"":3,""bus"":""sfx.weapons""}", "at least 4 variations")]
        [TestCase(@"{""name"":""sfx.a.b"",""desc"":""A proper description."",""asset"":""SFX_A"",""variations"":4,""bus"":""sfx.nowhere""}", "unknown bus")]
        [TestCase(@"{""name"":""sfx.a.b"",""desc"":""A proper description."",""asset"":""VFX_A"",""variations"":4,""bus"":""sfx.weapons""}", "must start with SFX_")]
        [TestCase(@"{""name"":""sfx.a.b"",""desc"":""A proper description."",""asset"":""SFX_A"",""variations"":4,""bus"":""sfx.weapons"",""volume"":3}", "volume must be")]
        [TestCase(@"{""name"":""sfx.a.b"",""desc"":""short"",""asset"":""SFX_A"",""variations"":4,""bus"":""sfx.weapons""}", "must describe")]
        [TestCase(@"{""name"":""sfx.a.b"",""desc"":""A proper description."",""asset"":""SFX_A"",""variations"":4,""bus"":""sfx.weapons"",""particles"":[1,1,1,1]}", "unknown field 'particles'")]
        [TestCase(@"{""name"":""vo.effort.x"",""desc"":""A proper description."",""asset"":""VO_Rhen_X"",""variations"":3,""bus"":""vo.combat""}", "must contain {Character}")]
        [TestCase(@"{""name"":""vfx.a.b"",""desc"":""A proper description."",""asset"":""VFX_A"",""attach"":""Hit"",""lifetime"":10,""particles"":[40,40,40,40]}", "exceeds 32 particles")]
        [TestCase(@"{""name"":""vfx.a.b"",""desc"":""A proper description."",""asset"":""VFX_A"",""attach"":""Hit"",""lifetime"":10,""particles"":[8,4,16,32]}", "must not decrease")]
        [TestCase(@"{""name"":""cam.a.b"",""desc"":""A proper description."",""kind"":""Flash"",""amplitude"":0.9,""frames"":4}", "photosensitivity")]
        [TestCase(@"{""name"":""cam.a.b"",""desc"":""A proper description."",""kind"":""Shake"",""amplitude"":0.9,""frequency"":20,""frames"":4}", "shake amplitude")]
        [TestCase(@"{""name"":""cam.a.b"",""desc"":""A proper description."",""kind"":""Sequence"",""frames"":60,""sequence"":""Cutscene""}", "TL_ prefix")]
        [TestCase(@"{""name"":""cam.a.b"",""desc"":""A proper description."",""kind"":""SlowMotion"",""frames"":60,""timeScale"":1}", "timeScale")]
        [TestCase(@"{""name"":""haptic.a"",""desc"":""A proper description."",""amplitude"":2,""ms"":10}", "haptic amplitude")]
        [TestCase(@"{""name"":""impact.a"",""desc"":""A proper description."",""layers"":[""sfx.missing""]}", "not in the catalog")]
        [TestCase(@"{""name"":""impact.a"",""desc"":""A proper description."",""layers"":[""impact.a""]}", "cannot nest")]
        [TestCase(@"{""name"":""music.a"",""desc"":""A proper description.""}", "not a valid cue name")]
        public void ProductionRulesAreEnforced(string cueJson, string expected)
        {
            string json = @"{""id"":""cues.test"",""cues"":[" + cueJson + "]}";
            var exception = Assert.Throws<ContentValidationException>(() => CueCatalogParser.Parse(json, "cues.test.json"));
            StringAssert.Contains(expected, exception.Message);
        }

        [Test]
        public void DuplicateCuesAreRejected()
        {
            const string cue = @"{""name"":""haptic.a"",""desc"":""A proper description."",""amplitude"":0.5,""ms"":10}";
            string json = @"{""id"":""cues.test"",""cues"":[" + cue + "," + cue + "]}";
            var exception = Assert.Throws<ContentValidationException>(() => CueCatalogParser.Parse(json, "cues.test.json"));
            StringAssert.Contains("duplicate cue 'haptic.a'", exception.Message);
        }

        [Test]
        public void UnknownEnumValuesPointAtTheJsonPath()
        {
            const string json = @"{""id"":""cues.test"",""cues"":[{""name"":""vfx.a.b"",""desc"":""A proper description."",""asset"":""VFX_A"",""attach"":""Elbow"",""lifetime"":10,""particles"":[1,1,1,1]}]}";
            var exception = Assert.Throws<Oathsunder.Core.Serialization.JsonContentException>(() => CueCatalogParser.Parse(json, "cues.test.json"));
            StringAssert.Contains("$.cues[0].attach", exception.Message);
        }

        [Test]
        public void CoverageFlagsMissingVoiceCameraAndLateAnticipation()
        {
            var blueprint = TestContent.RhenKatana();
            var light = blueprint.FindMove("katana.l1");
            light.Cues = light.Cues.Where(c => !c.Name.StartsWith("vo.", StringComparison.Ordinal)).ToArray();
            var heavy = blueprint.FindMove("katana.h1");
            heavy.Cues = heavy.Cues.Where(c => !c.Name.StartsWith("cam.", StringComparison.Ordinal)).ToArray();
            var low = blueprint.FindMove("katana.dl");
            foreach (var cue in low.Cues)
            {
                cue.Frame = low.FirstActiveFrame + 1;
            }

            var errors = CueCoverage.Check(blueprint, TestContent.Cues);
            Assert.IsTrue(errors.Any(e => e.StartsWith("katana.l1: attacks need a voice cue", StringComparison.Ordinal)), string.Join("\n", errors));
            Assert.IsTrue(errors.Any(e => e.StartsWith("katana.h1:", StringComparison.Ordinal) && e.Contains("camera cue")), string.Join("\n", errors));
            Assert.IsTrue(errors.Any(e => e.StartsWith("katana.dl: first sound", StringComparison.Ordinal)), string.Join("\n", errors));
            Assert.IsTrue(errors.Any(e => e.StartsWith("katana.dl: first visual", StringComparison.Ordinal)), string.Join("\n", errors));
            Assert.AreEqual(4, errors.Count, string.Join("\n", errors));
        }

        [Test]
        public void MoveDataCannotFireEventDrivenImpactCues()
        {
            var blueprint = TestContent.RhenKatana();
            var move = blueprint.FindMove("katana.l1");
            move.Cues = move.Cues.Concat(new[] { new PresentationCue { Frame = 6, Name = EventCueMap.HitHeavy } }).ToArray();
            var errors = CueCoverage.Check(blueprint, TestContent.Cues);
            Assert.AreEqual(1, errors.Count, string.Join("\n", errors));
            StringAssert.Contains("event-driven", errors[0]);
        }

        [Test]
        public void EventsMapToTheirCues()
        {
            Assert.AreEqual(EventCueMap.HitLight, EventCueMap.ForEvent(Event(CombatEventType.Hit, CombatEventFlags.None)));
            Assert.AreEqual(EventCueMap.HitHeavy, EventCueMap.ForEvent(Event(CombatEventType.Hit, CombatEventFlags.Heavy)));
            Assert.AreEqual(EventCueMap.HitCounter, EventCueMap.ForEvent(Event(CombatEventType.Hit, CombatEventFlags.Heavy | CombatEventFlags.Counter)));
            Assert.AreEqual(EventCueMap.BlockHeavy, EventCueMap.ForEvent(Event(CombatEventType.Block, CombatEventFlags.Heavy)));
            Assert.AreEqual(EventCueMap.KnockdownHard, EventCueMap.ForEvent(Event(CombatEventType.Knockdown, CombatEventFlags.None, value: 1)));
            Assert.AreEqual(EventCueMap.KnockdownSoft, EventCueMap.ForEvent(Event(CombatEventType.Knockdown, CombatEventFlags.None)));
            Assert.AreEqual(EventCueMap.Finisher, EventCueMap.ForEvent(Event(CombatEventType.KnockOut, CombatEventFlags.Finisher)));
            Assert.IsNull(EventCueMap.ForEvent(Event(CombatEventType.MoveStart, CombatEventFlags.None)));
            Assert.IsNull(EventCueMap.ForEvent(Event(CombatEventType.Cue, CombatEventFlags.None)));
        }

        [Test]
        public void BlockedHeavyAttacksCarryTheHeavyFlag()
        {
            Assert.IsTrue(BlockFlags(InputButtons.Heavy).HasFlag(CombatEventFlags.Heavy), "katana.h1 is a Heavy move");
            Assert.IsFalse(BlockFlags(InputButtons.Light).HasFlag(CombatEventFlags.Heavy), "katana.l1 is a Light move");
        }

        private static CombatEventFlags BlockFlags(InputButtons button)
        {
            var h = new CombatHarness();
            h.PlaceApart(1.4);
            h.Hold(1, InputButtons.Guard);
            h.Wait(10);
            h.Tap(0, button);
            h.RunUntil(() => h.Has(CombatEventType.Block), 30, "block");
            return h.First(CombatEventType.Block).Flags;
        }

        private static CombatEvent Event(CombatEventType type, CombatEventFlags flags, int value = 0) =>
            new CombatEvent(1, type, 0, 1, 0, value, 0, FixedVector2.Zero, flags);
    }
}

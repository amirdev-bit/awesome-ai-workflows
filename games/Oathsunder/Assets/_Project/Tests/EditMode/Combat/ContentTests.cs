using System.IO;
using Oathsunder.Combat.Content;
using Oathsunder.Combat.Definitions;
using Oathsunder.Core.Mathematics;
using Oathsunder.Core.Serialization;
using Oathsunder.Tests.Support;
using NUnit.Framework;

namespace Oathsunder.Tests.Combat
{
    [TestFixture]
    public sealed class ContentTests
    {
        [Test]
        public void EveryShippedFileParses()
        {
            foreach (var file in Directory.GetFiles(TestContent.Root, "*.json", SearchOption.AllDirectories))
            {
                string json = File.ReadAllText(file);
                string name = Path.GetFileName(file);
                Assert.DoesNotThrow(() => JsonReader.Parse(json, name), name);
                if (name.StartsWith("moveset."))
                {
                    Assert.DoesNotThrow(() => CombatContentParser.ParseMoveSet(json, name), name);
                }
                else if (name.StartsWith("fighter."))
                {
                    Assert.DoesNotThrow(() => CombatContentParser.ParseFighter(json, name), name);
                }
                else if (name.StartsWith("stage."))
                {
                    Assert.DoesNotThrow(() => CombatContentParser.ParseStage(json, name), name);
                }
                else if (name.StartsWith("rules."))
                {
                    Assert.DoesNotThrow(() => CombatContentParser.ParseRules(json, name), name);
                }
                else if (name.StartsWith("tuning."))
                {
                    Assert.DoesNotThrow(() => CombatContentParser.ParseTuning(json, name), name);
                }
                else
                {
                    Assert.Fail($"Unrecognised content file {name}: names must start with a known prefix.");
                }
            }
        }

        [Test]
        public void RhenKatanaBlueprintValidates()
        {
            var blueprint = TestContent.RhenKatana();
            Assert.Greater(blueprint.Moves.Length, 30);
            Assert.GreaterOrEqual(blueprint.TechRollMoveIndex, 0);
            Assert.AreEqual(1, blueprint.Projectiles.Length);
            CollectionAssert.AreEqual(new[] { "moveset.universal", "weapon.katana" }, blueprint.MoveSetIds);
        }

        [Test]
        public void ShippedTuningMatchesCodeDefaults()
        {
            // The JSON is the designer-facing source of truth; the code defaults must never drift from it.
            var fromJson = TestContent.Tuning();
            var defaults = new CombatTuning();
            foreach (var field in typeof(CombatTuning).GetFields())
            {
                Assert.AreEqual(field.GetValue(defaults), field.GetValue(fromJson), field.Name);
            }
        }

        [Test]
        public void UnitsConvertExactly()
        {
            var rhen = TestContent.Rhen;
            Assert.AreEqual(Fixed.ParseScaled("3.2", 60), rhen.WalkForwardSpeed);
            Assert.AreEqual(Fixed.ParseScaled("37.8", 3600), rhen.Gravity);
            var rules = TestContent.Rules("rules.ranked");
            Assert.AreEqual(99 * 60, rules.RoundTimerFrames);
            Assert.AreEqual(500, rules.PerfectDodgeSlowPermille);
            Assert.IsFalse(rules.UseRpgStats, "ranked must normalise stats (no pay-to-win)");
        }

        [Test]
        public void KatanaFrameDataIsAsDesigned()
        {
            var blueprint = TestContent.RhenKatana();
            AssertFrameData(blueprint, "katana.l1", startup: 6, active: 3, recovery: 11, onHit: 4, onBlock: 0);
            AssertFrameData(blueprint, "katana.l2", startup: 7, active: 3, recovery: 12, onHit: 4, onBlock: 0);
            AssertFrameData(blueprint, "katana.dl", startup: 5, active: 3, recovery: 10, onHit: 3, onBlock: -1);
            AssertFrameData(blueprint, "katana.h1", startup: 12, active: 3, recovery: 16, onHit: 4, onBlock: 0);
            AssertFrameData(blueprint, "katana.fl", startup: 12, active: 3, recovery: 14, onHit: 4, onBlock: -1);
            AssertFrameData(blueprint, "katana.bh", startup: 20, active: 3, recovery: 16, onHit: 6, onBlock: -2);
            Assert.AreEqual(AttackHeight.Overhead, blueprint.FindMove("katana.bh").Attacks[0].Height);
            Assert.AreEqual(AttackHeight.Low, blueprint.FindMove("katana.dl").Attacks[0].Height);
        }

        [Test]
        public void FrameDataReportCoversEveryAttack()
        {
            var blueprint = TestContent.RhenKatana();
            var rows = FrameDataCalculator.Calculate(blueprint);
            foreach (var move in blueprint.Moves)
            {
                bool listed = false;
                foreach (var row in rows)
                {
                    listed |= row.MoveId == move.Id;
                }

                Assert.AreEqual(move.IsAttack, listed, move.Id);
            }

            StringAssert.Contains("| katana.l1 | Rising Petal |", FrameDataCalculator.ToMarkdown(rows));
        }

        [Test]
        public void BlueprintsDoNotShareResolvedState()
        {
            // Two fighters merging the same parsed move sets must resolve independently.
            var universal = TestContent.Universal;
            var first = FighterBlueprintBuilder.Build(TestContent.Rhen, universal, TestContent.Katana);
            var onlyUniversal = FighterBlueprintBuilder.Build(TestContent.Rhen, universal);
            var dashA = first.FindMove("universal.dash");
            var dashB = onlyUniversal.FindMove("universal.dash");
            Assert.AreNotSame(dashA, dashB);
            Assert.Greater(dashA.Cancels[0].Candidates.Length, dashB.Cancels[0].Candidates.Length);
            foreach (int candidate in dashA.Cancels[0].Candidates)
            {
                Assert.Less(candidate, first.Moves.Length);
            }

            Assert.AreEqual(-1, universal.Moves[0].Index, "parsed definitions must stay untouched");
        }

        [Test]
        public void ValidationReportsEveryProblem()
        {
            const string json = @"{
              ""id"": ""weapon.broken"",
              ""moves"": [
                { ""id"": ""broken.a"", ""frames"": 10,
                  ""triggers"": [ { ""buttons"": [""Light""] } ],
                  ""attacks"": { ""hit"": { ""damage"": 10, ""proration"": 1500 } },
                  ""hitboxes"": [ { ""frames"": [8, 12], ""box"": { ""x"": 0, ""y"": 0, ""w"": 1, ""h"": 1 }, ""attack"": ""hit"" } ],
                  ""cancels"": [ { ""frames"": [5, 6], ""to"": [""broken.missing""] } ] },
                { ""id"": ""broken.a"", ""frames"": 5, ""triggers"": [ { ""buttons"": [""Heavy""] } ] }
              ]
            }";
            var set = CombatContentParser.ParseMoveSet(json, "broken.json");
            var exception = Assert.Throws<ContentValidationException>(() => FighterBlueprintBuilder.Build(TestContent.Rhen, set));
            Assert.GreaterOrEqual(exception.Errors.Count, 4);
            StringAssert.Contains("duplicate move id 'broken.a'", exception.Message);
            StringAssert.Contains("hitbox window 8-12 is outside 1..10", exception.Message);
            StringAssert.Contains("proration must be within 1..1000", exception.Message);
            StringAssert.Contains("unknown move 'broken.missing'", exception.Message);
        }

        [Test]
        public void SchemaErrorsPointAtTheField()
        {
            const string json = @"{ ""id"": ""x"", ""moves"": [ { ""id"": ""x.a"", ""frames"": 10, ""tags"": [""Nonsense""] } ] }";
            var exception = Assert.Throws<JsonContentException>(() => CombatContentParser.ParseMoveSet(json, "x.json"));
            Assert.AreEqual("$.moves[0].tags[0]", exception.JsonPath);
        }

        private static void AssertFrameData(FighterBlueprint blueprint, string id, int startup, int active, int recovery, int onHit, int onBlock)
        {
            var move = blueprint.FindMove(id);
            Assert.IsNotNull(move, id);
            var row = FrameDataCalculator.Calculate(move);
            Assert.AreEqual(startup, row.Startup, id + " startup");
            Assert.AreEqual(active, row.Active, id + " active");
            Assert.AreEqual(recovery, row.Recovery, id + " recovery");
            Assert.AreEqual(onHit, row.OnHit, id + " on hit");
            Assert.AreEqual(onBlock, row.OnBlock, id + " on block");
        }
    }
}

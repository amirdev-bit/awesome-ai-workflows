using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Oathsunder.Combat.Definitions;
using Oathsunder.Core.Mathematics;
using Oathsunder.Presentation.Cues;

namespace Oathsunder.Presentation.Animation
{
    /// <summary>
    /// Writes animation specs as stable, diff-friendly JSON (<c>Animation/Specs/animspec.&lt;fighter&gt;.&lt;weapon&gt;.json</c>)
    /// for DCC import scripts (root-motion locator, key-pose markers, cue markers) and CI freshness checks.
    /// Distances are metres with 4 decimals, frames are 1-based, +X is forward.
    /// </summary>
    public static class AnimationSpecJson
    {
        /// <summary>Format version.</summary>
        public const int Version = 1;

        /// <summary>Serialises a fighter's specs.</summary>
        public static string Write(string fighterId, string weaponId, IReadOnlyList<MoveAnimationSpec> specs, CueCatalog catalog)
        {
            var w = new StringBuilder(64 * 1024);
            w.Append("{\n");
            w.Append("  \"version\": ").Append(Version).Append(",\n");
            w.Append("  \"fighter\": ").Append(Quote(fighterId)).Append(",\n");
            w.Append("  \"weapon\": ").Append(Quote(weaponId)).Append(",\n");
            w.Append("  \"fps\": 60,\n");
            w.Append("  \"clips\": [\n");
            for (int i = 0; i < specs.Count; i++)
            {
                WriteClip(w, specs[i], catalog);
                w.Append(i + 1 < specs.Count ? ",\n" : "\n");
            }

            w.Append("  ]\n}\n");
            return w.ToString();
        }

        private static void WriteClip(StringBuilder w, MoveAnimationSpec spec, CueCatalog catalog)
        {
            var move = spec.Move;
            w.Append("    {\n");
            Field(w, "clip", Quote(spec.Clip));
            Field(w, "move", Quote(move.Id));
            Field(w, "name", Quote(move.Name));
            Field(w, "frames", Int(move.TotalFrames));
            Field(w, "rootMotion", Quote(spec.RootMotion.ToString()));
            if (move.IsAttack)
            {
                Field(w, "startup", Int(spec.Startup));
                Field(w, "active", $"[{Int(move.FirstActiveFrame)}, {Int(move.LastActiveFrame)}]");
                Field(w, "recovery", Int(spec.Recovery));
            }

            var poses = new List<string>();
            foreach (var pose in spec.KeyPoses)
            {
                poses.Add($"{{ \"frame\": {Int(pose.Frame)}, \"pose\": {Quote(pose.Name)} }}");
            }

            FieldList(w, "keyPoses", poses);

            var root = new List<string>();
            foreach (var sample in spec.Root)
            {
                root.Add($"[{Metres(sample.Offset.X)}, {Metres(sample.Offset.Y)}]");
            }

            FieldInline(w, "root", root);

            var hitboxes = new List<string>();
            foreach (var hitbox in move.Hitboxes)
            {
                hitboxes.Add($"{{ \"frames\": [{Int(hitbox.Window.Start)}, {Int(hitbox.Window.End)}], \"group\": {Int(hitbox.Group)}, \"attack\": {Quote(move.Attacks[hitbox.AttackIndex].Key)}, \"box\": {Box(hitbox.Box)} }}");
            }

            FieldList(w, "hitboxes", hitboxes);

            var hurt = new List<string>();
            foreach (var range in HurtRanges(spec))
            {
                var state = range.State;
                var boxes = new List<string>();
                foreach (var box in state.Boxes)
                {
                    boxes.Add(Box(box));
                }

                hurt.Add($"{{ \"frames\": [{Int(range.Start)}, {Int(range.End)}], \"stance\": {Quote(state.Stance)}, \"invulnerable\": {Quote(state.Invulnerable.ToString())}, \"armor\": {Bool(state.Armor)}, \"perfectEvade\": {Bool(state.PerfectEvade)}, \"counterStance\": {Bool(state.CounterStance)}, \"boxes\": [{string.Join(", ", boxes)}] }}");
            }

            FieldList(w, "hurt", hurt);

            var events = new List<string>();
            foreach (var cue in spec.Cues)
            {
                string asset = catalog != null && catalog.TryGet(cue.Name, out var def) ? def.Asset : "";
                events.Add($"{{ \"frame\": {Int(cue.Frame)}, \"channel\": {Quote(CueNames.Prefix(cue.Channel))}, \"cue\": {Quote(cue.Name)}, \"asset\": {Quote(asset)} }}");
            }

            FieldList(w, "events", events);

            var contacts = new List<string>();
            foreach (var contact in spec.Contacts)
            {
                contacts.Add($"{{ \"group\": {Int(contact.Group)}, \"frames\": [{Int(contact.Window.Start)}, {Int(contact.Window.End)}], \"attack\": {Quote(contact.Attack)}, \"hitstop\": {Int(contact.Hitstop)}, \"onHit\": {Quote(contact.OnHit)}, \"onCounterHit\": {Quote(contact.OnCounterHit)}, \"onBlock\": {Quote(contact.OnBlock)} }}");
            }

            FieldList(w, "contacts", contacts);
            if (move.Paired != null)
            {
                var paired = move.Paired;
                var hits = new List<string>();
                foreach (var hit in paired.Hits)
                {
                    hits.Add(Int(hit.Frame));
                }

                Field(w, "paired", $"{{ \"victimClip\": {Quote(paired.VictimAnimation)}, \"victimOffset\": [{Metres(paired.VictimOffset.X)}, {Metres(paired.VictimOffset.Y)}], \"hits\": [{string.Join(", ", hits)}], \"release\": {Int(paired.ReleaseFrame)}, \"cinematic\": {Bool(paired.Cinematic)} }}");
            }

            var notes = new List<string>();
            foreach (var note in spec.Notes)
            {
                notes.Add(Quote(note));
            }

            FieldList(w, "notes", notes, last: true);
            w.Append("    }");
        }

        /// <summary>A run of consecutive frames with identical defensive state.</summary>
        public readonly struct HurtRange
        {
            /// <summary>Creates a range.</summary>
            public HurtRange(int start, int end, HurtState state)
            {
                Start = start;
                End = end;
                State = state;
            }

            /// <summary>First frame.</summary>
            public int Start { get; }

            /// <summary>Last frame.</summary>
            public int End { get; }

            /// <summary>The shared state.</summary>
            public HurtState State { get; }
        }

        /// <summary>Collapses the per-frame hurt timeline into ranges of identical state.</summary>
        public static List<HurtRange> HurtRanges(MoveAnimationSpec spec)
        {
            var ranges = new List<HurtRange>();
            int start = 0;
            for (int i = 1; i <= spec.Hurt.Length; i++)
            {
                if (i == spec.Hurt.Length || !spec.Hurt[i].SameAs(spec.Hurt[start]))
                {
                    ranges.Add(new HurtRange(spec.Hurt[start].Frame, spec.Hurt[i - 1].Frame, spec.Hurt[start]));
                    start = i;
                }
            }

            return ranges;
        }

        /// <summary>Metres with 4 decimals, invariant culture.</summary>
        public static string Metres(Fixed value) => value.ToDouble().ToString("0.0000", CultureInfo.InvariantCulture);

        private static string Box(FixedAabb box) =>
            $"[{Metres(box.Min.X)}, {Metres(box.Min.Y)}, {Metres(box.Width)}, {Metres(box.Height)}]";

        private static string Int(int value) => value.ToString(CultureInfo.InvariantCulture);

        private static string Bool(bool value) => value ? "true" : "false";

        private static void Field(StringBuilder w, string name, string value) =>
            w.Append("      ").Append(Quote(name)).Append(": ").Append(value).Append(",\n");

        private static void FieldInline(StringBuilder w, string name, List<string> items) =>
            w.Append("      ").Append(Quote(name)).Append(": [").Append(string.Join(", ", items)).Append("],\n");

        private static void FieldList(StringBuilder w, string name, List<string> items, bool last = false)
        {
            w.Append("      ").Append(Quote(name)).Append(": [");
            if (items.Count == 0)
            {
                w.Append(']');
            }
            else
            {
                w.Append('\n');
                for (int i = 0; i < items.Count; i++)
                {
                    w.Append("        ").Append(items[i]).Append(i + 1 < items.Count ? ",\n" : "\n");
                }

                w.Append("      ]");
            }

            w.Append(last ? "\n" : ",\n");
        }

        private static string Quote(string text)
        {
            var q = new StringBuilder(text.Length + 2);
            q.Append('"');
            foreach (char c in text)
            {
                switch (c)
                {
                    case '"': q.Append("\\\""); break;
                    case '\\': q.Append("\\\\"); break;
                    case '\n': q.Append("\\n"); break;
                    default:
                        if (c < ' ')
                        {
                            q.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            q.Append(c);
                        }

                        break;
                }
            }

            return q.Append('"').ToString();
        }
    }
}

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Oathsunder.Combat.Definitions;

namespace Oathsunder.Combat.Content
{
    /// <summary>Frame data of one attack, in standard fighting-game notation.</summary>
    public readonly struct FrameDataRow
    {
        /// <summary>Creates a row.</summary>
        public FrameDataRow(string moveId, string name, int startup, int active, int recovery, int total, int? onHit, int? onBlock, int damage, string notes, bool isGrab = false)
        {
            IsGrab = isGrab;
            MoveId = moveId;
            Name = name;
            Startup = startup;
            Active = active;
            Recovery = recovery;
            Total = total;
            OnHit = onHit;
            OnBlock = onBlock;
            Damage = damage;
            Notes = notes;
        }

        /// <summary>Move id.</summary>
        public string MoveId { get; }

        /// <summary>Display name.</summary>
        public string Name { get; }

        /// <summary>Frame of the first active hitbox.</summary>
        public int Startup { get; }

        /// <summary>Active frames (span from first to last active frame).</summary>
        public int Active { get; }

        /// <summary>Frames after the last active frame.</summary>
        public int Recovery { get; }

        /// <summary>Total frames.</summary>
        public int Total { get; }

        /// <summary>Frame advantage on hit when the last hit connects on its first active frame (null: knockdown/launch).</summary>
        public int? OnHit { get; }

        /// <summary>Frame advantage on block (null when unblockable or a grab).</summary>
        public int? OnBlock { get; }

        /// <summary>Total base damage of all hitbox attacks (distinct hit groups).</summary>
        public int Damage { get; }

        /// <summary>Notes (KD, Launch, Low, Overhead, Wall Bounce…).</summary>
        public string Notes { get; }

        /// <summary>True for throws and executions (their result is a paired action, not hitstun).</summary>
        public bool IsGrab { get; }
    }

    /// <summary>
    /// Derives frame data from move definitions. Used by training mode, the move-list UI, the design docs
    /// generator and balance tests. Hitstop freezes both fighters equally, so it does not change advantage.
    /// </summary>
    public static class FrameDataCalculator
    {
        /// <summary>Frame data of every attack in a blueprint, in blueprint order.</summary>
        public static List<FrameDataRow> Calculate(FighterBlueprint blueprint)
        {
            var rows = new List<FrameDataRow>();
            foreach (var move in blueprint.Moves)
            {
                if (move.IsAttack)
                {
                    rows.Add(Calculate(move));
                }
            }

            return rows;
        }

        /// <summary>Frame data of a single attack move.</summary>
        public static FrameDataRow Calculate(MoveDefinition move)
        {
            move.ComputeDerivedData();
            HitboxSpec last = null;
            var damageByGroup = new Dictionary<int, int>();
            foreach (var hitbox in move.Hitboxes)
            {
                if (last == null || hitbox.Window.Start > last.Window.Start)
                {
                    last = hitbox;
                }

                int damage = move.Attacks[hitbox.AttackIndex].Damage;
                if (!damageByGroup.TryGetValue(hitbox.Group, out int existing) || damage > existing)
                {
                    damageByGroup[hitbox.Group] = damage;
                }
            }

            int totalDamage = 0;
            foreach (var pair in damageByGroup)
            {
                totalDamage += pair.Value;
            }

            if (move.Paired != null)
            {
                foreach (var hit in move.Paired.Hits)
                {
                    totalDamage += hit.Damage;
                }
            }

            int startup = move.FirstActiveFrame;
            int active = move.LastActiveFrame - move.FirstActiveFrame + 1;
            int recovery = move.TotalFrames - move.LastActiveFrame;
            int? onHit = null;
            int? onBlock = null;
            bool isGrab = false;
            var notes = new List<string>();
            if (last != null)
            {
                var attack = move.Attacks[last.AttackIndex];
                isGrab = attack.Has(AttackFlags.Grab);
                int remaining = move.TotalFrames - last.Window.Start;
                bool reaction = attack.Has(AttackFlags.Launch | AttackFlags.Knockdown | AttackFlags.HardKnockdown);
                if (!reaction && !attack.Has(AttackFlags.Grab))
                {
                    onHit = attack.Hitstun - remaining;
                }

                if (!attack.Has(AttackFlags.Grab) && attack.Height != AttackHeight.Unblockable && !attack.Has(AttackFlags.GuardCrush))
                {
                    onBlock = attack.Blockstun - remaining;
                }

                Describe(attack, notes);
            }

            if (move.PerfectEvade.HasValue)
            {
                notes.Add("Perfect Evade " + move.PerfectEvade.Value);
            }

            foreach (var window in move.Invulnerability)
            {
                notes.Add($"Invuln {window.Mask} {window.Window}");
            }

            foreach (var armor in move.Armor)
            {
                notes.Add($"Armor x{armor.Hits} {armor.Window}");
            }

            return new FrameDataRow(move.Id, move.Name, startup, active, recovery, move.TotalFrames, onHit, onBlock, totalDamage, string.Join(", ", notes), isGrab);
        }

        /// <summary>Formats rows as a Markdown table.</summary>
        public static string ToMarkdown(IReadOnlyList<FrameDataRow> rows)
        {
            var text = new StringBuilder();
            text.AppendLine("| Move | Name | Damage | Startup | Active | Recovery | Total | On Hit | On Block | Notes |");
            text.AppendLine("|---|---|---:|---:|---:|---:|---:|---:|---:|---|");
            foreach (var row in rows)
            {
                text.Append("| ").Append(row.MoveId)
                    .Append(" | ").Append(row.Name)
                    .Append(" | ").Append(row.Damage.ToString(CultureInfo.InvariantCulture))
                    .Append(" | ").Append(row.Startup.ToString(CultureInfo.InvariantCulture))
                    .Append(" | ").Append(row.Active.ToString(CultureInfo.InvariantCulture))
                    .Append(" | ").Append(row.Recovery.ToString(CultureInfo.InvariantCulture))
                    .Append(" | ").Append(row.Total.ToString(CultureInfo.InvariantCulture))
                    .Append(" | ").Append(FormatAdvantage(row.OnHit, row.IsGrab ? "Grab" : "KD/Launch"))
                    .Append(" | ").Append(FormatAdvantage(row.OnBlock, "—"))
                    .Append(" | ").Append(row.Notes)
                    .AppendLine(" |");
            }

            return text.ToString();
        }

        private static string FormatAdvantage(int? value, string fallback)
        {
            if (!value.HasValue)
            {
                return fallback;
            }

            return value.Value > 0 ? "+" + value.Value.ToString(CultureInfo.InvariantCulture) : value.Value.ToString(CultureInfo.InvariantCulture);
        }

        private static void Describe(AttackSpec attack, List<string> notes)
        {
            if (attack.Height != AttackHeight.Mid)
            {
                notes.Add(attack.Height.ToString());
            }

            if (attack.Has(AttackFlags.Grab))
            {
                notes.Add(attack.Has(AttackFlags.Execution) ? "Execution" : "Throw");
            }

            if (attack.Has(AttackFlags.Launch))
            {
                notes.Add("Launch");
            }

            if (attack.Has(AttackFlags.HardKnockdown))
            {
                notes.Add("Hard KD");
            }
            else if (attack.Has(AttackFlags.Knockdown))
            {
                notes.Add("KD");
            }

            if (attack.Has(AttackFlags.WallBounce))
            {
                notes.Add("Wall Bounce");
            }

            if (attack.Has(AttackFlags.GroundBounce))
            {
                notes.Add("Ground Bounce");
            }

            if (attack.Has(AttackFlags.GuardCrush))
            {
                notes.Add("Guard Crush");
            }

            if (attack.Has(AttackFlags.LaunchOnCounter))
            {
                notes.Add("Launch on CH");
            }

            if (attack.Has(AttackFlags.Spike))
            {
                notes.Add("Spike");
            }

            if (!string.IsNullOrEmpty(attack.OnHitMoveId))
            {
                notes.Add("→ " + attack.OnHitMoveId);
            }
        }
    }
}

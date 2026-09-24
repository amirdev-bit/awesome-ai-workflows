using System;
using System.Collections.Generic;
using Oathsunder.Combat.Definitions;
using Oathsunder.Combat.Input;

namespace Oathsunder.Combat.Content
{
    /// <summary>
    /// Merges a fighter body with one or more move sets (universal + weapon), resolves every string reference to
    /// an index, precomputes cancel candidate lists, and validates the result. All problems are collected and
    /// reported together so designers fix a file in one pass. Parsed move sets are never mutated: each blueprint
    /// resolves its own clones, so one parsed set can be merged into any number of fighters.
    /// </summary>
    public static class FighterBlueprintBuilder
    {
        /// <summary>Maximum hit groups per move (bounded by the 32-bit hit registry: 8 groups × 4 victims).</summary>
        public const int MaxHitGroups = 8;

        /// <summary>Builds and validates a blueprint.</summary>
        /// <exception cref="ContentValidationException">The content is invalid.</exception>
        public static FighterBlueprint Build(FighterDefinition body, params MoveSetDefinition[] moveSets)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            var errors = new List<string>();
            var moves = new List<MoveDefinition>();
            var projectiles = new List<ProjectileDefinition>();
            var moveIndexById = new Dictionary<string, int>(StringComparer.Ordinal);
            var projectileIndexById = new Dictionary<string, int>(StringComparer.Ordinal);
            var setIds = new string[moveSets.Length];

            for (int s = 0; s < moveSets.Length; s++)
            {
                var set = moveSets[s];
                setIds[s] = set.Id;
                foreach (var sourceProjectile in set.Projectiles)
                {
                    var projectile = sourceProjectile.CloneForResolution();
                    if (projectileIndexById.ContainsKey(projectile.Id))
                    {
                        errors.Add($"{set.Id}: duplicate projectile id '{projectile.Id}'");
                        continue;
                    }

                    projectile.Index = projectiles.Count;
                    projectileIndexById.Add(projectile.Id, projectile.Index);
                    projectiles.Add(projectile);
                }

                foreach (var sourceMove in set.Moves)
                {
                    var move = sourceMove.CloneForResolution();
                    if (moveIndexById.ContainsKey(move.Id))
                    {
                        errors.Add($"{set.Id}: duplicate move id '{move.Id}'");
                        continue;
                    }

                    move.Index = moves.Count;
                    moveIndexById.Add(move.Id, move.Index);
                    moves.Add(move);
                }
            }

            var cueNames = new List<string>();
            var cueIndex = new Dictionary<string, int>(StringComparer.Ordinal);
            var moveArray = moves.ToArray();

            foreach (var move in moveArray)
            {
                move.ComputeDerivedData();
                ValidateMove(move, errors);
                ResolveMove(move, moveArray, moveIndexById, projectileIndexById, cueNames, cueIndex, errors);
            }

            foreach (var projectile in projectiles)
            {
                ValidateProjectile(projectile, errors);
            }

            ValidateBody(body, errors);

            var neutral = new List<int>();
            foreach (var move in moveArray)
            {
                if (!move.HasFlag(MoveFlags.ChainOnly) && move.Triggers.Length > 0)
                {
                    neutral.Add(move.Index);
                }
            }

            if (errors.Count > 0)
            {
                throw new ContentValidationException(body.Id + " + " + string.Join(" + ", setIds), errors);
            }

            return new FighterBlueprint(
                body,
                moveArray,
                projectiles.ToArray(),
                cueNames.ToArray(),
                neutral.ToArray(),
                moveIndexById,
                projectileIndexById,
                setIds);
        }

        private static void ResolveMove(
            MoveDefinition move,
            MoveDefinition[] moves,
            Dictionary<string, int> moveIndexById,
            Dictionary<string, int> projectileIndexById,
            List<string> cueNames,
            Dictionary<string, int> cueIndex,
            List<string> errors)
        {
            foreach (var attack in move.Attacks)
            {
                attack.OnHitMoveIndex = -1;
                if (!string.IsNullOrEmpty(attack.OnHitMoveId))
                {
                    if (moveIndexById.TryGetValue(attack.OnHitMoveId, out int index))
                    {
                        attack.OnHitMoveIndex = index;
                    }
                    else
                    {
                        errors.Add($"{move.Id}: attack '{attack.Key}' onHit references unknown move '{attack.OnHitMoveId}'");
                    }
                }
            }

            foreach (var window in move.Cancels)
            {
                var candidates = new List<int>();
                foreach (var id in window.TargetIds)
                {
                    if (moveIndexById.TryGetValue(id, out int index))
                    {
                        if (!candidates.Contains(index))
                        {
                            candidates.Add(index);
                        }
                    }
                    else
                    {
                        errors.Add($"{move.Id}: cancel window {window.Window} targets unknown move '{id}'");
                    }
                }

                if (window.TargetTags != MoveTags.None)
                {
                    foreach (var candidate in moves)
                    {
                        if (candidate != move && candidate.HasTag(window.TargetTags) && !candidate.HasFlag(MoveFlags.ChainOnly) &&
                            candidate.Triggers.Length > 0 && !candidates.Contains(candidate.Index))
                        {
                            candidates.Add(candidate.Index);
                        }
                    }
                }

                if (window.Input != CancelInput.Trigger && candidates.Count != 1)
                {
                    errors.Add($"{move.Id}: automatic cancel window {window.Window} must have exactly one explicit target");
                }

                // Tag-only windows may legitimately match nothing until a weapon set is merged in.
                if (window.Input == CancelInput.Trigger && window.TargetIds.Length == 0 && window.TargetTags == MoveTags.None && !window.AllowJump)
                {
                    errors.Add($"{move.Id}: cancel window {window.Window} declares no targets");
                }

                if (window.Input == CancelInput.Trigger)
                {
                    foreach (int candidate in candidates)
                    {
                        if (moves[candidate].Triggers.Length == 0)
                        {
                            errors.Add($"{move.Id}: cancel target '{moves[candidate].Id}' has no triggers (use an automatic cancel)");
                        }
                    }
                }

                window.Candidates = candidates.ToArray();
            }

            if (move.Counter != null)
            {
                move.Counter.CounterMoveIndex = moveIndexById.TryGetValue(move.Counter.CounterMoveId, out int index) ? index : -1;
                if (move.Counter.CounterMoveIndex < 0)
                {
                    errors.Add($"{move.Id}: counter stance references unknown move '{move.Counter.CounterMoveId}'");
                }
            }

            foreach (var spawn in move.Projectiles)
            {
                spawn.ProjectileIndex = projectileIndexById.TryGetValue(spawn.ProjectileId, out int index) ? index : -1;
                if (spawn.ProjectileIndex < 0)
                {
                    errors.Add($"{move.Id}: spawns unknown projectile '{spawn.ProjectileId}'");
                }
            }

            foreach (var cue in move.Cues)
            {
                if (!cueIndex.TryGetValue(cue.Name, out int index))
                {
                    index = cueNames.Count;
                    cueNames.Add(cue.Name);
                    cueIndex.Add(cue.Name, index);
                }

                cue.CueIndex = index;
            }
        }

        private static void ValidateMove(MoveDefinition move, List<string> errors)
        {
            string id = move.Id;
            if (string.IsNullOrEmpty(id))
            {
                errors.Add("a move has an empty id");
                return;
            }

            if (move.TotalFrames < 1)
            {
                errors.Add($"{id}: frames must be at least 1");
                return;
            }

            int total = move.TotalFrames;
            foreach (var hitbox in move.Hitboxes)
            {
                CheckWindow(errors, id, "hitbox", hitbox.Window, total);
                if (hitbox.Group < 0 || hitbox.Group >= MaxHitGroups)
                {
                    errors.Add($"{id}: hitbox group {hitbox.Group} outside 0..{MaxHitGroups - 1}");
                }

                if (hitbox.AttackIndex < 0 || hitbox.AttackIndex >= move.Attacks.Length)
                {
                    errors.Add($"{id}: hitbox references a missing attack");
                }
            }

            foreach (var attack in move.Attacks)
            {
                ValidateAttack(errors, id, attack);
            }

            foreach (var hurtbox in move.Hurtboxes)
            {
                CheckWindow(errors, id, "hurtbox", hurtbox.Window, total);
            }

            foreach (var invulnerability in move.Invulnerability)
            {
                CheckWindow(errors, id, "invulnerability", invulnerability.Window, total);
                if (invulnerability.Mask == InvulnerabilityMask.None)
                {
                    errors.Add($"{id}: invulnerability window {invulnerability.Window} protects against nothing");
                }
            }

            foreach (var armor in move.Armor)
            {
                CheckWindow(errors, id, "armor", armor.Window, total);
                if (armor.Hits < 1)
                {
                    errors.Add($"{id}: armor window {armor.Window} must absorb at least one hit");
                }
            }

            foreach (var window in move.Cancels)
            {
                CheckWindow(errors, id, "cancel", window.Window, total);
            }

            for (int i = 0; i < move.Motion.Length; i++)
            {
                CheckWindow(errors, id, "motion", move.Motion[i].Window, total);
                for (int j = i + 1; j < move.Motion.Length; j++)
                {
                    var a = move.Motion[i].Window;
                    var b = move.Motion[j].Window;
                    if (a.Start <= b.End && b.Start <= a.End)
                    {
                        errors.Add($"{id}: motion segments {a} and {b} overlap");
                    }
                }
            }

            foreach (var spawn in move.Projectiles)
            {
                CheckFrame(errors, id, "spawn", spawn.Frame, total);
            }

            foreach (var effect in move.Effects)
            {
                CheckFrame(errors, id, "effect", effect.Frame, total);
            }

            foreach (var cue in move.Cues)
            {
                CheckFrame(errors, id, "cue", cue.Frame, total);
            }

            if (move.PerfectEvade.HasValue)
            {
                CheckWindow(errors, id, "perfectEvade", move.PerfectEvade.Value, total);
            }

            if (move.Counter != null)
            {
                CheckWindow(errors, id, "counter", move.Counter.Window, total);
            }

            if (move.Paired != null)
            {
                CheckFrame(errors, id, "paired.releaseFrame", move.Paired.ReleaseFrame, total);
                foreach (var hit in move.Paired.Hits)
                {
                    CheckFrame(errors, id, "paired hit", hit.Frame, total);
                    if (hit.Frame > move.Paired.ReleaseFrame)
                    {
                        errors.Add($"{id}: paired hit on frame {hit.Frame} is after the release frame {move.Paired.ReleaseFrame}");
                    }
                }

                if (move.Paired.TechWindow > 0 && move.Paired.Hits.Length > 0 && move.Paired.Hits[0].Frame <= move.Paired.TechWindow)
                {
                    errors.Add($"{id}: the first paired hit lands inside the tech window");
                }
            }

            if (move.Cost.Rage < 0 || move.Cost.Shadow < 0 || move.Cost.Ultimate < 0)
            {
                errors.Add($"{id}: costs cannot be negative");
            }

            foreach (var trigger in move.Triggers)
            {
                if (trigger.Stance == TriggerStance.None)
                {
                    errors.Add($"{id}: trigger has no stance");
                }

                if (trigger.Direction == DirectionMask.None)
                {
                    errors.Add($"{id}: trigger accepts no direction");
                }
            }
        }

        private static void ValidateAttack(List<string> errors, string owner, AttackSpec attack)
        {
            if (attack.Damage < 0 || attack.ChipDamage < 0 || attack.PostureDamage < 0)
            {
                errors.Add($"{owner}: attack '{attack.Key}' has negative damage");
            }

            if (attack.Hitstun < 0 || attack.Blockstun < 0 || attack.Hitstop < 0)
            {
                errors.Add($"{owner}: attack '{attack.Key}' has negative stun");
            }

            if (attack.ProrationPermille < 1 || attack.ProrationPermille > 1000)
            {
                errors.Add($"{owner}: attack '{attack.Key}' proration must be within 1..1000");
            }

            if (attack.JuggleCost < 0)
            {
                errors.Add($"{owner}: attack '{attack.Key}' juggle cost cannot be negative");
            }

            if (attack.Has(AttackFlags.Grab) && string.IsNullOrEmpty(attack.OnHitMoveId))
            {
                errors.Add($"{owner}: grab attack '{attack.Key}' needs an onHit move");
            }
        }

        private static void ValidateProjectile(ProjectileDefinition projectile, List<string> errors)
        {
            if (projectile.Lifetime < 1)
            {
                errors.Add($"{projectile.Id}: lifetime must be at least 1");
            }

            if (projectile.Hits < 1)
            {
                errors.Add($"{projectile.Id}: hits must be at least 1");
            }

            ValidateAttack(errors, projectile.Id, projectile.Attack);
        }

        private static void ValidateBody(FighterDefinition body, List<string> errors)
        {
            if (body.MaxHealth < 1)
            {
                errors.Add($"{body.Id}: maxHealth must be positive");
            }

            if (body.MaxPosture < 1)
            {
                errors.Add($"{body.Id}: maxPosture must be positive");
            }

            if (body.Gravity.Raw <= 0)
            {
                errors.Add($"{body.Id}: gravity must be positive");
            }

            CheckStance(errors, body.Id, "standing", body.Standing);
            CheckStance(errors, body.Id, "crouching", body.Crouching);
            CheckStance(errors, body.Id, "airborne", body.Airborne);
            CheckStance(errors, body.Id, "knockdown", body.Knockdown);
        }

        private static void CheckStance(List<string> errors, string owner, string name, StanceBoxes stance)
        {
            if (stance.Hurtboxes.Length == 0)
            {
                errors.Add($"{owner}: {name} stance has no hurtboxes");
            }

            if (!stance.Pushbox.IsValid)
            {
                errors.Add($"{owner}: {name} stance pushbox is empty");
            }
        }

        private static void CheckWindow(List<string> errors, string owner, string what, FrameWindow window, int total)
        {
            if (!window.IsValidWithin(total))
            {
                errors.Add($"{owner}: {what} window {window} is outside 1..{total}");
            }
        }

        private static void CheckFrame(List<string> errors, string owner, string what, int frame, int total)
        {
            if (frame < 1 || frame > total)
            {
                errors.Add($"{owner}: {what} frame {frame} is outside 1..{total}");
            }
        }
    }
}

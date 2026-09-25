using System.Collections.Generic;
using Oathsunder.Combat.Definitions;

namespace Oathsunder.Presentation.Cues
{
    /// <summary>
    /// Presentation Definition-of-Done rules for moves. A move is not shippable until every rule passes:
    /// <list type="number">
    /// <item><b>Sound</b>: at least one <c>sfx</c> or <c>foley</c> cue.</item>
    /// <item><b>Visual</b>: at least one <c>vfx</c> cue.</item>
    /// <item><b>Camera</b>: Heavy, Launcher, Special, Ultimate, Execution, Charged, ModeActivation and Cinematic
    /// moves, and moves with a paired action, have at least one <c>cam</c> cue. Light attacks get their camera
    /// impulse from the event-driven impact cues on contact.</item>
    /// <item><b>Voice</b>: moves that can hurt (hitboxes, projectiles, paired action) or catch (counter stance)
    /// have at least one <c>vo</c> cue.</item>
    /// <item><b>Anticipation</b>: on moves with hitboxes the first sound and the first visual fire no later than
    /// the first active frame, so the player hears and sees the attack before it can connect.</item>
    /// <item><b>Catalog</b>: every cue is declared in the catalog, and move data never fires an event-driven
    /// <c>impact</c> cue directly.</item>
    /// <item><b>Projectiles</b>: every projectile names a <c>VFX_</c> visual.</item>
    /// </list>
    /// </summary>
    public static class CueCoverage
    {
        /// <summary>Tags whose moves must move the camera.</summary>
        public const MoveTags CameraTags = MoveTags.Heavy | MoveTags.Launcher | MoveTags.Special | MoveTags.Ultimate | MoveTags.Execution
            | MoveTags.Charged | MoveTags.ModeActivation | MoveTags.Cinematic;

        /// <summary>Checks every move and projectile of a blueprint.</summary>
        /// <returns>One message per violation; empty when the blueprint passes.</returns>
        public static List<string> Check(FighterBlueprint blueprint, CueCatalog catalog)
        {
            var errors = new List<string>();
            foreach (var move in blueprint.Moves)
            {
                CheckMove(move, catalog, errors);
            }

            foreach (var projectile in blueprint.Projectiles)
            {
                if (projectile.Visual == null || !projectile.Visual.StartsWith("VFX_", System.StringComparison.Ordinal))
                {
                    errors.Add($"{projectile.Id}: projectile visual '{projectile.Visual}' must be a VFX_ asset");
                }
            }

            return errors;
        }

        /// <summary>Checks one move, appending violations to <paramref name="errors"/>.</summary>
        public static void CheckMove(MoveDefinition move, CueCatalog catalog, List<string> errors)
        {
            bool sound = false, visual = false, camera = false, voice = false;
            int firstSound = int.MaxValue, firstVisual = int.MaxValue;
            foreach (var cue in move.Cues)
            {
                if (!CueNames.TryParse(cue.Name, out var channel))
                {
                    errors.Add($"{move.Id}: cue '{cue.Name}' (frame {cue.Frame}) is not a valid cue name");
                    continue;
                }

                if (!catalog.Contains(cue.Name))
                {
                    errors.Add($"{move.Id}: cue '{cue.Name}' (frame {cue.Frame}) is not in catalog '{catalog.Id}'");
                }

                switch (channel)
                {
                    case CueChannel.Sfx:
                    case CueChannel.Foley:
                        sound = true;
                        firstSound = cue.Frame < firstSound ? cue.Frame : firstSound;
                        break;
                    case CueChannel.Vfx:
                        visual = true;
                        firstVisual = cue.Frame < firstVisual ? cue.Frame : firstVisual;
                        break;
                    case CueChannel.Cam:
                        camera = true;
                        break;
                    case CueChannel.Vo:
                        voice = true;
                        break;
                    case CueChannel.Impact:
                        errors.Add($"{move.Id}: cue '{cue.Name}' is event-driven and cannot be fired from move data");
                        break;
                }
            }

            if (!sound)
            {
                errors.Add($"{move.Id}: needs a sound cue (sfx.* or foley.*)");
            }

            if (!visual)
            {
                errors.Add($"{move.Id}: needs a visual cue (vfx.*)");
            }

            if (!camera && (move.HasTag(CameraTags) || move.Paired != null))
            {
                errors.Add($"{move.Id}: {move.Tags} moves need a camera cue (cam.*)");
            }

            bool dangerous = move.Hitboxes.Length > 0 || move.Projectiles.Length > 0 || move.Paired != null || move.Counter != null;
            if (!voice && dangerous)
            {
                errors.Add($"{move.Id}: attacks need a voice cue (vo.*)");
            }

            if (move.Hitboxes.Length > 0 && move.FirstActiveFrame > 0)
            {
                if (sound && firstSound > move.FirstActiveFrame)
                {
                    errors.Add($"{move.Id}: first sound on frame {firstSound} comes after the first active frame {move.FirstActiveFrame}");
                }

                if (visual && firstVisual > move.FirstActiveFrame)
                {
                    errors.Add($"{move.Id}: first visual on frame {firstVisual} comes after the first active frame {move.FirstActiveFrame}");
                }
            }
        }
    }
}

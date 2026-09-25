using System.Collections.Generic;
using Oathsunder.Combat.Events;

namespace Oathsunder.Presentation.Cues
{
    /// <summary>
    /// Maps simulation events to the cue that presents them, so every weapon gets consistent contact feedback
    /// without authoring it per move. Move-authored cues arrive separately as <see cref="CombatEventType.Cue"/>.
    /// </summary>
    public static class EventCueMap
    {
        /// <summary>Normal hit.</summary>
        public const string HitLight = "impact.hit.light";

        /// <summary>Hit by a Heavy-tagged attack.</summary>
        public const string HitHeavy = "impact.hit.heavy";

        /// <summary>Counter hit.</summary>
        public const string HitCounter = "impact.hit.counter";

        /// <summary>Scripted hit inside a throw, execution or ultimate.</summary>
        public const string HitPaired = "impact.hit.paired";

        /// <summary>Guarded attack.</summary>
        public const string Block = "impact.block";

        /// <summary>Guarded Heavy-tagged attack.</summary>
        public const string BlockHeavy = "impact.block.heavy";

        /// <summary>Perfect parry.</summary>
        public const string Parry = "impact.parry";

        /// <summary>Perfect dodge (Shadow Time starts).</summary>
        public const string PerfectDodge = "impact.perfectdodge";

        /// <summary>Counter stance caught a strike.</summary>
        public const string CounterStance = "impact.counterstance";

        /// <summary>Hit absorbed by armor.</summary>
        public const string Armor = "impact.armor";

        /// <summary>Posture broken.</summary>
        public const string GuardBreak = "impact.guardbreak";

        /// <summary>Victim launched into a juggle.</summary>
        public const string Launch = "impact.launch";

        /// <summary>Wall bounce.</summary>
        public const string WallBounce = "impact.wallbounce";

        /// <summary>Ground bounce.</summary>
        public const string GroundBounce = "impact.groundbounce";

        /// <summary>Soft knockdown body fall.</summary>
        public const string KnockdownSoft = "impact.knockdown.soft";

        /// <summary>Hard knockdown body slam.</summary>
        public const string KnockdownHard = "impact.knockdown.hard";

        /// <summary>Take-off from the ground.</summary>
        public const string Jump = "impact.jump";

        /// <summary>Landing.</summary>
        public const string Land = "impact.land";

        /// <summary>A grab connected.</summary>
        public const string Grab = "impact.grab";

        /// <summary>Throw tech (both fighters pushed apart).</summary>
        public const string ThrowTech = "impact.throwtech";

        /// <summary>Two projectiles cancelled each other.</summary>
        public const string Clash = "impact.clash";

        /// <summary>A projectile expired or finished hitting.</summary>
        public const string ProjectileEnd = "impact.projectile.end";

        /// <summary>Ember Rage ran out.</summary>
        public const string RageEnd = "impact.rage.end";

        /// <summary>Umbral Shadow ran out.</summary>
        public const string ShadowEnd = "impact.shadow.end";

        /// <summary>Umbral Shadow echo strike.</summary>
        public const string Echo = "impact.echo";

        /// <summary>Knock-out.</summary>
        public const string KnockOut = "impact.knockout";

        /// <summary>Knock-out by a finisher.</summary>
        public const string Finisher = "impact.finisher";

        /// <summary>Fighter rises from a knockdown.</summary>
        public const string WakeUp = "foley.body.rise";

        /// <summary>Announcer: round number.</summary>
        public const string RoundStart = "vo.announcer.round";

        /// <summary>Announcer: fight.</summary>
        public const string RoundFight = "vo.announcer.fight";

        /// <summary>Announcer: time over.</summary>
        public const string TimeOver = "vo.announcer.timeover";

        /// <summary>Every cue name this map can return.</summary>
        public static readonly IReadOnlyList<string> All = new[]
        {
            HitLight, HitHeavy, HitCounter, HitPaired, Block, BlockHeavy, Parry, PerfectDodge, CounterStance, Armor,
            GuardBreak, Launch, WallBounce, GroundBounce, KnockdownSoft, KnockdownHard, Jump, Land, Grab, ThrowTech,
            Clash, ProjectileEnd, RageEnd, ShadowEnd, Echo, KnockOut, Finisher, WakeUp, RoundStart, RoundFight, TimeOver,
        };

        /// <summary>
        /// The cue for an event, or null when the event has no event-driven presentation (move cues, round flow
        /// handled by the UI, or events presented by other systems such as projectiles).
        /// </summary>
        public static string ForEvent(in CombatEvent e)
        {
            switch (e.Type)
            {
                case CombatEventType.Hit:
                    return e.Has(CombatEventFlags.Counter) ? HitCounter : e.Has(CombatEventFlags.Heavy) ? HitHeavy : HitLight;
                case CombatEventType.PairedHit:
                    return HitPaired;
                case CombatEventType.Block:
                    return e.Has(CombatEventFlags.Heavy) ? BlockHeavy : Block;
                case CombatEventType.Parry:
                    return Parry;
                case CombatEventType.PerfectDodge:
                    return PerfectDodge;
                case CombatEventType.CounterStance:
                    return CounterStance;
                case CombatEventType.ArmorHit:
                    return Armor;
                case CombatEventType.GuardBreak:
                    return GuardBreak;
                case CombatEventType.Launch:
                    return Launch;
                case CombatEventType.WallBounce:
                    return WallBounce;
                case CombatEventType.GroundBounce:
                    return GroundBounce;
                case CombatEventType.Knockdown:
                    return e.Value != 0 ? KnockdownHard : KnockdownSoft;
                case CombatEventType.Jump:
                    return Jump;
                case CombatEventType.Land:
                    return Land;
                case CombatEventType.GrabConnect:
                    return Grab;
                case CombatEventType.ThrowTech:
                    return ThrowTech;
                case CombatEventType.ProjectileClash:
                    return Clash;
                case CombatEventType.ProjectileEnd:
                    return ProjectileEnd;
                case CombatEventType.RageEnd:
                    return RageEnd;
                case CombatEventType.ShadowEnd:
                    return ShadowEnd;
                case CombatEventType.ShadowEcho:
                    return Echo;
                case CombatEventType.KnockOut:
                    return e.Has(CombatEventFlags.Finisher) ? Finisher : KnockOut;
                case CombatEventType.WakeUp:
                    return WakeUp;
                case CombatEventType.RoundStart:
                    return RoundStart;
                case CombatEventType.RoundFight:
                    return RoundFight;
                case CombatEventType.TimeOver:
                    return TimeOver;
                default:
                    return null;
            }
        }
    }
}

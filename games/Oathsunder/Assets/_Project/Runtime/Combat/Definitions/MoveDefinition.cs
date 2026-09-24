namespace Oathsunder.Combat.Definitions
{
    /// <summary>
    /// A complete, data-authored move: frame data, boxes, cancels, motion, resources and presentation cues.
    /// Loaded from JSON by <see cref="Content.CombatContentParser"/> and immutable at runtime, so it can be shared
    /// between every fighter, rollback snapshot and replay.
    /// </summary>
    public sealed class MoveDefinition
    {
        /// <summary>Stable content id, e.g. "katana.l1".</summary>
        public string Id = "";

        /// <summary>Display name, e.g. "Rising Petal".</summary>
        public string Name = "";

        /// <summary>Index in the owning <see cref="FighterBlueprint"/>.</summary>
        public int Index = -1;

        /// <summary>Classification.</summary>
        public MoveTags Tags;

        /// <summary>Structural flags.</summary>
        public MoveFlags Flags;

        /// <summary>Total duration in frames.</summary>
        public int TotalFrames;

        /// <summary>Extra landing recovery when a land-cancel move touches down.</summary>
        public int LandingRecovery;

        /// <summary>Animation key used by the presentation layer.</summary>
        public string Animation = "";

        /// <summary>Ways to perform the move.</summary>
        public MoveTrigger[] Triggers = new MoveTrigger[0];

        /// <summary>Attack payloads referenced by hitboxes.</summary>
        public AttackSpec[] Attacks = new AttackSpec[0];

        /// <summary>Hitboxes.</summary>
        public HitboxSpec[] Hitboxes = new HitboxSpec[0];

        /// <summary>Hurtbox overrides.</summary>
        public HurtboxWindow[] Hurtboxes = new HurtboxWindow[0];

        /// <summary>Invulnerability windows.</summary>
        public InvulnerabilityWindow[] Invulnerability = new InvulnerabilityWindow[0];

        /// <summary>Armor windows.</summary>
        public ArmorWindow[] Armor = new ArmorWindow[0];

        /// <summary>Cancel windows.</summary>
        public CancelWindow[] Cancels = new CancelWindow[0];

        /// <summary>Scripted motion.</summary>
        public MotionSegment[] Motion = new MotionSegment[0];

        /// <summary>Projectile spawns.</summary>
        public ProjectileSpawn[] Projectiles = new ProjectileSpawn[0];

        /// <summary>Gameplay effects.</summary>
        public EffectCue[] Effects = new EffectCue[0];

        /// <summary>Presentation cues.</summary>
        public PresentationCue[] Cues = new PresentationCue[0];

        /// <summary>Counter stance (null for none).</summary>
        public CounterStanceSpec Counter;

        /// <summary>Paired action run while this move is active (null for none).</summary>
        public PairedActionSpec Paired;

        /// <summary>Perfect-evade window: attacks overlapping the fighter here trigger a Perfect Dodge.</summary>
        public FrameWindow? PerfectEvade;

        /// <summary>Resource cost.</summary>
        public ResourceCost Cost = new ResourceCost();

        /// <summary>First frame with an active hitbox (0 when the move has none). Derived.</summary>
        public int FirstActiveFrame;

        /// <summary>Last frame with an active hitbox (0 when the move has none). Derived.</summary>
        public int LastActiveFrame;

        /// <summary>True when any tag in <paramref name="tags"/> is present.</summary>
        public bool HasTag(MoveTags tags) => (Tags & tags) != 0;

        /// <summary>True when any flag in <paramref name="flags"/> is present.</summary>
        public bool HasFlag(MoveFlags flags) => (Flags & flags) != 0;

        /// <summary>True when the move has at least one hitbox.</summary>
        public bool IsAttack => Hitboxes.Length > 0;

        /// <summary>Recomputes derived frame data. Called by the blueprint builder.</summary>
        public void ComputeDerivedData()
        {
            FirstActiveFrame = 0;
            LastActiveFrame = 0;
            foreach (var hitbox in Hitboxes)
            {
                if (FirstActiveFrame == 0 || hitbox.Window.Start < FirstActiveFrame)
                {
                    FirstActiveFrame = hitbox.Window.Start;
                }

                if (hitbox.Window.End > LastActiveFrame)
                {
                    LastActiveFrame = hitbox.Window.End;
                }
            }
        }

        /// <summary>
        /// Copy whose resolvable parts (indices, cancel candidates, cue indices) are independent of this instance.
        /// Blueprints resolve clones so one parsed move set can be merged into many fighters safely.
        /// </summary>
        public MoveDefinition CloneForResolution()
        {
            var clone = (MoveDefinition)MemberwiseClone();
            clone.Index = -1;
            clone.Attacks = new AttackSpec[Attacks.Length];
            for (int i = 0; i < Attacks.Length; i++)
            {
                clone.Attacks[i] = Attacks[i].Clone();
            }

            clone.Cancels = new CancelWindow[Cancels.Length];
            for (int i = 0; i < Cancels.Length; i++)
            {
                clone.Cancels[i] = Cancels[i].Clone();
            }

            clone.Projectiles = new ProjectileSpawn[Projectiles.Length];
            for (int i = 0; i < Projectiles.Length; i++)
            {
                clone.Projectiles[i] = Projectiles[i].Clone();
            }

            clone.Cues = new PresentationCue[Cues.Length];
            for (int i = 0; i < Cues.Length; i++)
            {
                clone.Cues[i] = Cues[i].Clone();
            }

            clone.Counter = Counter?.Clone();
            return clone;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Id} ({Name})";
    }
}

using Oathsunder.Combat.Definitions;
using Oathsunder.Combat.Events;

namespace Oathsunder.Combat.Simulation
{
    /// <summary>Spawns, moves, clashes and retires projectiles. Pool-based: no allocation during a match.</summary>
    internal sealed class ProjectileSystem
    {
        private static readonly Oathsunder.Core.Mathematics.Fixed OutOfBoundsMargin = Oathsunder.Core.Mathematics.Fixed.FromInt(2);

        private readonly CombatContext _ctx;

        public ProjectileSystem(CombatContext context)
        {
            _ctx = context;
        }

        /// <summary>Projectiles not spawned because every slot was in use (diagnostic; expected 0).</summary>
        public int DroppedSpawns { get; private set; }

        public void Spawn(int ownerIndex, ProjectileSpawn spawn)
        {
            var projectiles = _ctx.State.Projectiles;
            for (int slot = 0; slot < projectiles.Length; slot++)
            {
                if (projectiles[slot].Active)
                {
                    continue;
                }

                ref FighterState owner = ref _ctx.State.Fighters[ownerIndex];
                var definition = _ctx.Blueprints[ownerIndex].Projectiles[spawn.ProjectileIndex];
                projectiles[slot] = new ProjectileState
                {
                    Active = true,
                    Owner = ownerIndex,
                    Team = owner.Team,
                    DefinitionIndex = spawn.ProjectileIndex,
                    Instance = _ctx.State.NextInstanceId(),
                    Position = owner.Position + spawn.Offset.Facing(owner.Facing),
                    Velocity = definition.Velocity.Facing(owner.Facing),
                    Facing = owner.Facing,
                    Age = 0,
                    HitsRemaining = definition.Hits,
                    HitRegistry = 0,
                    RehitTimer = 0,
                    HitstopRemaining = 0,
                };
                _ctx.Emit(CombatEventType.ProjectileSpawn, ownerIndex, -1, projectiles[slot].Instance, spawn.ProjectileIndex, slot, projectiles[slot].Position, CombatEventFlags.Projectile);
                return;
            }

            DroppedSpawns++;
        }

        /// <summary>Moves projectiles and retires expired ones.</summary>
        public void Update()
        {
            var projectiles = _ctx.State.Projectiles;
            for (int slot = 0; slot < projectiles.Length; slot++)
            {
                ref ProjectileState p = ref projectiles[slot];
                if (!p.Active)
                {
                    continue;
                }

                if (p.HitstopRemaining > 0)
                {
                    p.HitstopRemaining--;
                    continue;
                }

                p.Age++;
                p.Position += p.Velocity;
                if (p.RehitTimer > 0 && --p.RehitTimer == 0)
                {
                    p.HitRegistry = 0;
                }

                var definition = _ctx.Blueprints[p.Owner].Projectiles[p.DefinitionIndex];
                bool outOfBounds = p.Position.X < _ctx.Stage.LeftWall - OutOfBoundsMargin || p.Position.X > _ctx.Stage.RightWall + OutOfBoundsMargin;
                if (p.Age >= definition.Lifetime || outOfBounds)
                {
                    Destroy(slot);
                }
            }
        }

        /// <summary>Opposing projectiles that overlap cancel one hit of durability each.</summary>
        public void ResolveClashes()
        {
            var projectiles = _ctx.State.Projectiles;
            for (int a = 0; a < projectiles.Length; a++)
            {
                if (!projectiles[a].Active)
                {
                    continue;
                }

                for (int b = a + 1; b < projectiles.Length; b++)
                {
                    ref ProjectileState pa = ref projectiles[a];
                    ref ProjectileState pb = ref projectiles[b];
                    if (!pa.Active || !pb.Active || pa.Team == pb.Team)
                    {
                        continue;
                    }

                    var boxA = _ctx.Blueprints[pa.Owner].Projectiles[pa.DefinitionIndex].Box.ToWorld(pa.Position, pa.Facing);
                    var boxB = _ctx.Blueprints[pb.Owner].Projectiles[pb.DefinitionIndex].Box.ToWorld(pb.Position, pb.Facing);
                    if (!boxA.Overlaps(boxB))
                    {
                        continue;
                    }

                    var point = boxA.Intersection(boxB).Center;
                    _ctx.Emit(CombatEventType.ProjectileClash, pa.Owner, pb.Owner, pa.Instance, pb.Instance, 0, point, CombatEventFlags.Projectile);
                    ConsumeHit(a);
                    ConsumeHit(b);
                }
            }
        }

        /// <summary>Removes one hit of durability; destroys the projectile at zero.</summary>
        public void ConsumeHit(int slot)
        {
            ref ProjectileState p = ref _ctx.State.Projectiles[slot];
            if (!p.Active)
            {
                return;
            }

            p.HitsRemaining--;
            if (p.HitsRemaining <= 0)
            {
                Destroy(slot);
            }
        }

        /// <summary>Retires a projectile immediately.</summary>
        public void Destroy(int slot)
        {
            ref ProjectileState p = ref _ctx.State.Projectiles[slot];
            if (!p.Active)
            {
                return;
            }

            p.Active = false;
            _ctx.Emit(CombatEventType.ProjectileEnd, p.Owner, -1, p.Instance, p.DefinitionIndex, slot, p.Position, CombatEventFlags.Projectile);
        }

        /// <summary>Clears every projectile (round reset).</summary>
        public void Clear()
        {
            var projectiles = _ctx.State.Projectiles;
            for (int slot = 0; slot < projectiles.Length; slot++)
            {
                projectiles[slot] = default;
            }
        }
    }
}

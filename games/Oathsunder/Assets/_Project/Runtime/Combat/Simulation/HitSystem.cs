using Oathsunder.Combat.Definitions;
using Oathsunder.Combat.Events;
using Oathsunder.Combat.Input;
using Oathsunder.Core.Mathematics;

namespace Oathsunder.Combat.Simulation
{
    /// <summary>
    /// Detects every hitbox/hurtbox contact of the frame first, then resolves them in a fixed order, so trades
    /// (both fighters hitting on the same frame) resolve symmetrically. Resolution precedence for strikes:
    /// counter stance → perfect parry → guard → armor → hit.
    /// </summary>
    internal sealed class HitSystem
    {
        private const int MaxContacts = 64;
        private const int NoFrame = int.MinValue / 2;

        private readonly CombatContext _ctx;
        private readonly Contact[] _contacts = new Contact[MaxContacts];
        private readonly FixedAabb[] _hurtboxes = new FixedAabb[16];
        private int _contactCount;

        public HitSystem(CombatContext context)
        {
            _ctx = context;
        }

        /// <summary>Contacts dropped because the per-frame buffer was full (diagnostic; expected 0).</summary>
        public int DroppedContacts { get; private set; }

        /// <summary>Runs detection and resolution for the current frame, then due shadow echoes.</summary>
        public void Run()
        {
            _contactCount = 0;
            DetectFighterContacts();
            _ctx.Projectiles.ResolveClashes();
            DetectProjectileContacts();
            ResolveContacts();
            ProcessEchoes();
        }

        // ================================================================== detection

        private void DetectFighterContacts()
        {
            var state = _ctx.State;
            for (int attackerIndex = 0; attackerIndex < state.FighterCount; attackerIndex++)
            {
                ref FighterState attacker = ref state.Fighters[attackerIndex];
                if (!attacker.Active || attacker.IsKnockedOut || attacker.Action != FighterAction.Move || attacker.HitstopRemaining > 0)
                {
                    continue;
                }

                var move = _ctx.Blueprints[attackerIndex].Moves[attacker.MoveIndex];
                int frame = attacker.ActionFrame;
                if (frame < move.FirstActiveFrame || frame > move.LastActiveFrame)
                {
                    continue;
                }

                for (int h = 0; h < move.Hitboxes.Length; h++)
                {
                    var hitbox = move.Hitboxes[h];
                    if (!hitbox.Window.Contains(frame))
                    {
                        continue;
                    }

                    var attack = move.Attacks[hitbox.AttackIndex];
                    var worldBox = hitbox.Box.ToWorld(attacker.Position, attacker.Facing);
                    for (int victimIndex = 0; victimIndex < state.FighterCount; victimIndex++)
                    {
                        if (!_ctx.CanTarget(attackerIndex, victimIndex))
                        {
                            continue;
                        }

                        uint bit = RegistryBit(hitbox.Group, victimIndex);
                        if ((attacker.HitRegistry & bit) != 0 || IsQueued(attackerIndex, -1, victimIndex, hitbox.Group))
                        {
                            continue;
                        }

                        if (!OverlapsHurtboxes(victimIndex, worldBox, out var point))
                        {
                            continue;
                        }

                        if (attack.Has(AttackFlags.Grab))
                        {
                            if (CanBeGrabbed(victimIndex, attack))
                            {
                                Queue(attackerIndex, -1, victimIndex, hitbox.AttackIndex, hitbox.Group, point, grab: true,
                                    attack, attacker.Facing, attacker.MoveInstance, attacker.MoveIndex);
                            }

                            continue;
                        }

                        if (IsStrikeTargetable(attackerIndex, -1, victimIndex, attack, attacker.MoveInstance, point))
                        {
                            Queue(attackerIndex, -1, victimIndex, hitbox.AttackIndex, hitbox.Group, point, grab: false,
                                attack, attacker.Facing, attacker.MoveInstance, attacker.MoveIndex);
                        }
                    }
                }
            }
        }

        private void DetectProjectileContacts()
        {
            var state = _ctx.State;
            for (int slot = 0; slot < CombatWorldState.MaxProjectiles; slot++)
            {
                ref ProjectileState projectile = ref state.Projectiles[slot];
                if (!projectile.Active || projectile.HitstopRemaining > 0)
                {
                    continue;
                }

                var definition = _ctx.Blueprints[projectile.Owner].Projectiles[projectile.DefinitionIndex];
                var worldBox = definition.Box.ToWorld(projectile.Position, projectile.Facing);
                for (int victimIndex = 0; victimIndex < state.FighterCount; victimIndex++)
                {
                    if (victimIndex == projectile.Owner || !state.Fighters[victimIndex].Active)
                    {
                        continue;
                    }

                    if (!_ctx.Rules.FriendlyFire && state.Fighters[victimIndex].Team == projectile.Team)
                    {
                        continue;
                    }

                    if ((projectile.HitRegistry & (1u << victimIndex)) != 0)
                    {
                        continue;
                    }

                    if (!OverlapsHurtboxes(victimIndex, worldBox, out var point))
                    {
                        continue;
                    }

                    if (IsStrikeTargetable(projectile.Owner, slot, victimIndex, definition.Attack, projectile.Instance, point))
                    {
                        Queue(projectile.Owner, slot, victimIndex, -1, 0, point, grab: false,
                            definition.Attack, projectile.Facing, projectile.Instance, -1);
                    }
                }
            }
        }

        private bool IsStrikeTargetable(int attackerIndex, int projectileSlot, int victimIndex, AttackSpec attack, int instance, FixedVector2 point)
        {
            bool projectile = projectileSlot >= 0;
            ref FighterState victim = ref _ctx.State.Fighters[victimIndex];
            if (victim.IsKnockedOut || victim.Action == FighterAction.PairedVictim || victim.Action == FighterAction.WakeUp)
            {
                return false;
            }

            var mask = projectile ? InvulnerabilityMask.Projectile : InvulnerabilityMask.Strike;
            if (_ctx.IsMoveInvulnerable(victimIndex, mask))
            {
                TryPerfectDodge(victimIndex, attackerIndex, projectileSlot, instance, point);
                return false;
            }

            if (victim.Action == FighterAction.Knockdown && (!attack.Has(AttackFlags.OffTheGround) || victim.OffTheGroundUsed))
            {
                return false;
            }

            if (victim.Action == FighterAction.Launched && !attack.Has(AttackFlags.IgnoreJuggleLimit) &&
                victim.JugglePoints + attack.JuggleCost > _ctx.Tuning.MaxJugglePoints)
            {
                return false;
            }

            return true;
        }

        private bool CanBeGrabbed(int victimIndex, AttackSpec attack)
        {
            ref FighterState victim = ref _ctx.State.Fighters[victimIndex];
            if (victim.IsKnockedOut || victim.Action == FighterAction.PairedVictim)
            {
                return false;
            }

            if (attack.Has(AttackFlags.Execution))
            {
                return _ctx.IsExecutable(victim);
            }

            if (!victim.Grounded && !attack.Has(AttackFlags.AirGrab))
            {
                return false;
            }

            if (victim.ThrowInvulnerableFrames > 0 || _ctx.IsMoveInvulnerable(victimIndex, InvulnerabilityMask.Throw))
            {
                return false;
            }

            switch (victim.Action)
            {
                case FighterAction.Hitstun:
                case FighterAction.Blockstun:
                case FighterAction.AirHitstun:
                case FighterAction.Launched:
                case FighterAction.Knockdown:
                case FighterAction.WakeUp:
                case FighterAction.Staggered:
                    return false;
                default:
                    return true;
            }
        }

        private void TryPerfectDodge(int dodgerIndex, int attackerIndex, int projectileSlot, int instance, FixedVector2 point)
        {
            bool projectile = projectileSlot >= 0;
            ref FighterState dodger = ref _ctx.State.Fighters[dodgerIndex];
            var move = _ctx.MoveOf(dodgerIndex);
            if (move == null || !move.PerfectEvade.HasValue || !move.PerfectEvade.Value.Contains(dodger.ActionFrame))
            {
                return;
            }

            if (dodger.LastEvadedInstance == instance)
            {
                return;
            }

            dodger.LastEvadedInstance = instance;
            dodger.Contact |= MoveContact.Evaded;

            // A perfectly dodged attack is spent against the dodger, even if Shadow Time keeps it active
            // after the dodge's invulnerability ends.
            if (projectile)
            {
                _ctx.State.Projectiles[projectileSlot].HitRegistry |= 1u << dodgerIndex;
            }
            else
            {
                for (int group = 0; group < Content.FighterBlueprintBuilder.MaxHitGroups; group++)
                {
                    _ctx.State.Fighters[attackerIndex].HitRegistry |= RegistryBit(group, dodgerIndex);
                }
            }
            _ctx.AddMeter(ref dodger.Shadow, _ctx.Tuning.ShadowGainPerfectDodge, dodgerIndex);
            var flags = projectile ? CombatEventFlags.Projectile : CombatEventFlags.None;
            _ctx.Emit(CombatEventType.PerfectDodge, dodgerIndex, attackerIndex, instance, 0, 0, point, flags);

            if (_ctx.Rules.PerfectDodgeSlowPermille < 1000 && _ctx.Rules.PerfectDodgeSlowFrames > 0)
            {
                ref FighterState attacker = ref _ctx.State.Fighters[attackerIndex];
                attacker.TimeScalePermille = _ctx.Rules.PerfectDodgeSlowPermille;
                attacker.TimeScaleFrames = _ctx.Rules.PerfectDodgeSlowFrames;
                attacker.TimeAccumulator = 0;
                _ctx.Emit(CombatEventType.TimeDilationStart, dodgerIndex, attackerIndex, instance, _ctx.Rules.PerfectDodgeSlowPermille, _ctx.Rules.PerfectDodgeSlowFrames, point, CombatEventFlags.None);
            }
        }

        private bool OverlapsHurtboxes(int victimIndex, in FixedAabb box, out FixedVector2 point)
        {
            int count = _ctx.GetHurtboxes(victimIndex, _hurtboxes);
            for (int i = 0; i < count; i++)
            {
                if (box.Overlaps(_hurtboxes[i]))
                {
                    point = box.Intersection(_hurtboxes[i]).Center;
                    return true;
                }
            }

            point = FixedVector2.Zero;
            return false;
        }

        private static uint RegistryBit(int group, int victimIndex) => 1u << (group * CombatWorldState.MaxFighters + victimIndex);

        private bool IsQueued(int attacker, int projectileSlot, int victim, int group)
        {
            for (int i = 0; i < _contactCount; i++)
            {
                ref Contact c = ref _contacts[i];
                if (c.Attacker == attacker && c.ProjectileSlot == projectileSlot && c.Victim == victim && c.Group == group)
                {
                    return true;
                }
            }

            return false;
        }

        private void Queue(int attacker, int projectileSlot, int victim, int attackIndex, int group, FixedVector2 point, bool grab,
            AttackSpec attack, int attackerFacing, int instance, int sourceMove)
        {
            if (_contactCount >= MaxContacts)
            {
                DroppedContacts++;
                return;
            }

            _contacts[_contactCount++] = new Contact
            {
                Attacker = attacker,
                ProjectileSlot = projectileSlot,
                Victim = victim,
                AttackIndex = attackIndex,
                Group = group,
                Point = point,
                Grab = grab,
                Consumed = false,
                Attack = attack,
                AttackerFacing = attackerFacing,
                Instance = instance,
                SourceMove = sourceMove,
            };
        }

        // ================================================================== resolution

        private void ResolveContacts()
        {
            for (int i = 0; i < _contactCount; i++)
            {
                ref Contact contact = ref _contacts[i];
                if (contact.Consumed)
                {
                    continue;
                }

                contact.Consumed = true;
                if (contact.Grab)
                {
                    ResolveGrab(i);
                }
                else
                {
                    ResolveStrike(ref contact);
                }
            }
        }

        private void ResolveGrab(int contactIndex)
        {
            ref Contact contact = ref _contacts[contactIndex];
            var state = _ctx.State;
            ref FighterState attacker = ref state.Fighters[contact.Attacker];
            ref FighterState victim = ref state.Fighters[contact.Victim];
            if (attacker.Action != FighterAction.Move || victim.Action == FighterAction.PairedVictim || victim.IsKnockedOut)
            {
                return;
            }

            attacker.HitRegistry |= RegistryBit(contact.Group, contact.Victim);
            var attack = contact.Attack;

            // Simultaneous throws break each other.
            if (!attack.Has(AttackFlags.Execution))
            {
                for (int j = contactIndex + 1; j < _contactCount; j++)
                {
                    ref Contact other = ref _contacts[j];
                    if (!other.Consumed && other.Grab && other.Attacker == contact.Victim && other.Victim == contact.Attacker)
                    {
                        other.Consumed = true;
                        _ctx.Controller.ThrowTech(contact.Attacker, contact.Victim);
                        return;
                    }
                }
            }

            attacker.Contact |= MoveContact.Hit;
            StartPairedAction(contact.Attacker, contact.Victim, attack.OnHitMoveIndex);
        }

        private void ResolveStrike(ref Contact contact)
        {
            var state = _ctx.State;
            ref FighterState victim = ref state.Fighters[contact.Victim];
            if (victim.IsKnockedOut || victim.Action == FighterAction.PairedVictim)
            {
                return;
            }

            bool projectile = contact.ProjectileSlot >= 0;
            var attack = contact.Attack;
            int attackerFacing = contact.AttackerFacing;
            int instance = contact.Instance;
            int sourceMove = contact.SourceMove;
            if (projectile)
            {
                ref ProjectileState p = ref state.Projectiles[contact.ProjectileSlot];
                if (!p.Active || p.HitsRemaining <= 0)
                {
                    return;
                }

                p.HitRegistry |= 1u << contact.Victim;
                int rehit = _ctx.Blueprints[p.Owner].Projectiles[p.DefinitionIndex].RehitInterval;
                if (rehit > 0)
                {
                    p.RehitTimer = rehit;
                }
            }
            else
            {
                // The attacker may already have been hit earlier this frame (a trade); its attack still lands.
                ref FighterState attacker = ref state.Fighters[contact.Attacker];
                if (attacker.MoveInstance == instance)
                {
                    attacker.HitRegistry |= RegistryBit(contact.Group, contact.Victim);
                }
            }

            var catchMask = projectile ? InvulnerabilityMask.Projectile : InvulnerabilityMask.Strike;
            var victimMove = _ctx.MoveOf(contact.Victim);
            if (victimMove != null && victimMove.Counter != null && victimMove.Counter.Window.Contains(victim.ActionFrame) &&
                (victimMove.Counter.Catches & catchMask) != 0)
            {
                ResolveCounterStance(ref contact, victimMove, instance);
                return;
            }

            if (CanParry(ref victim, attack))
            {
                ResolveParry(ref contact, attack, attackerFacing, instance);
                return;
            }

            if (CanBlock(contact.Victim, ref victim, attack))
            {
                ResolveBlock(ref contact, attack, attackerFacing, instance);
                return;
            }

            if (TryGetArmor(contact.Victim, ref victim, attack, out int armorPermille))
            {
                ResolveArmor(ref contact, attack, instance, armorPermille);
                return;
            }

            ResolveHit(ref contact, attack, attackerFacing, instance, sourceMove);
        }

        private static bool CanParry(ref FighterState victim, AttackSpec attack)
        {
            if (attack.Has(AttackFlags.Unparryable) || attack.Has(AttackFlags.Grab))
            {
                return false;
            }

            return FighterController.IsParryCapable(victim.Action) && victim.LocalFrame <= victim.ParryWindowEndLocal;
        }

        private bool CanBlock(int victimIndex, ref FighterState victim, AttackSpec attack)
        {
            bool guarding = victim.Action == FighterAction.GuardStand || victim.Action == FighterAction.GuardCrouch ||
                            victim.Action == FighterAction.Blockstun ||
                            (victim.Action == FighterAction.ParryRecovery && _ctx.State.Inputs[victimIndex].IsHeld(_ctx.Frame, InputButtons.Guard));
            if (!guarding)
            {
                return false;
            }

            switch (attack.Height)
            {
                case AttackHeight.Unblockable:
                    return false;
                case AttackHeight.Low:
                    return victim.CrouchGuard;
                case AttackHeight.Overhead:
                    return !victim.CrouchGuard;
                default:
                    return true;
            }
        }

        private bool TryGetArmor(int victimIndex, ref FighterState victim, AttackSpec attack, out int damagePermille)
        {
            damagePermille = 1000;
            if (attack.Has(AttackFlags.ArmorBreak))
            {
                return false;
            }

            var move = _ctx.MoveOf(victimIndex);
            if (move == null)
            {
                return false;
            }

            foreach (var armor in move.Armor)
            {
                if (armor.Window.Contains(victim.ActionFrame) && victim.ArmorHitsTaken < armor.Hits)
                {
                    damagePermille = armor.DamagePermille;
                    return true;
                }
            }

            if (victim.InRage && move.HasTag(MoveTags.Heavy) && move.IsAttack && victim.ActionFrame <= move.LastActiveFrame &&
                victim.ArmorHitsTaken < _ctx.Tuning.RageHeavyArmorHits)
            {
                damagePermille = 1000;
                return true;
            }

            return false;
        }

        private void ResolveCounterStance(ref Contact contact, MoveDefinition stanceMove, int instance)
        {
            var state = _ctx.State;
            ref FighterState defender = ref state.Fighters[contact.Victim];
            int hitstop = _ctx.Tuning.CounterStanceHitstop;
            if (contact.ProjectileSlot >= 0)
            {
                _ctx.Projectiles.Destroy(contact.ProjectileSlot);
            }
            else
            {
                ref FighterState attacker = ref state.Fighters[contact.Attacker];
                attacker.Contact |= MoveContact.Parried;
                attacker.HitstopRemaining = hitstop;
            }

            _ctx.Emit(CombatEventType.CounterStance, contact.Victim, contact.Attacker, instance, stanceMove.Index, 0, contact.Point, CombatEventFlags.None);
            _ctx.Controller.StartMove(contact.Victim, ref defender, stanceMove.Counter.CounterMoveIndex, autoFace: true);
            defender.HitstopRemaining = hitstop;
        }

        private void ResolveParry(ref Contact contact, AttackSpec attack, int attackerFacing, int instance)
        {
            var state = _ctx.State;
            ref FighterState defender = ref state.Fighters[contact.Victim];
            int hitstop = attack.Hitstop > _ctx.Tuning.ParryHitstop ? attack.Hitstop : _ctx.Tuning.ParryHitstop;

            defender.Action = FighterAction.ParryRecovery;
            defender.ActionFrame = 0;
            defender.MoveIndex = -1;
            defender.StunRemaining = 0;
            defender.Velocity = FixedVector2.Zero;
            defender.HitstopRemaining = hitstop;
            defender.ParryWindowEndLocal = NoFrame;
            defender.LastGuardPressLocal = NoFrame;
            defender.Facing = -attackerFacing;
            _ctx.AddMeter(ref defender.Shadow, _ctx.Tuning.ShadowGainPerfectParry, contact.Victim);
            _ctx.AddMeter(ref defender.Ultimate, _ctx.Tuning.UltimateGainOnBlock * 2, contact.Victim);

            var flags = CombatEventFlags.None;
            if (contact.ProjectileSlot >= 0)
            {
                flags |= CombatEventFlags.Projectile;
                _ctx.Projectiles.Destroy(contact.ProjectileSlot);
            }
            else
            {
                ref FighterState attacker = ref state.Fighters[contact.Attacker];
                attacker.HitstopRemaining = hitstop;
                attacker.Contact |= MoveContact.Parried;
                long posture = (long)attack.PostureDamage * _ctx.Tuning.ParryPostureDamagePermille / 1000 * _ctx.Stats[contact.Victim].PostureDamagePermille / 1000;
                bool broke = AddPosture(contact.Attacker, (int)posture, canBreak: true);
                if (!broke && attack.Has(AttackFlags.StaggerOnParry))
                {
                    Stagger(contact.Attacker, StaggerKind.Parried, _ctx.Tuning.ParryStaggerFrames);
                }
            }

            _ctx.Emit(CombatEventType.Parry, contact.Victim, contact.Attacker, instance, 0, 0, contact.Point, flags);
        }

        private void ResolveBlock(ref Contact contact, AttackSpec attack, int attackerFacing, int instance)
        {
            var state = _ctx.State;
            ref FighterState defender = ref state.Fighters[contact.Victim];
            bool projectile = contact.ProjectileSlot >= 0;
            var body = _ctx.Blueprints[contact.Victim].Body;

            defender.Action = FighterAction.Blockstun;
            defender.ActionFrame = 0;
            defender.MoveIndex = -1;
            defender.StunRemaining = attack.Blockstun;
            defender.Velocity = new FixedVector2(attack.BlockPushback.MulPermille(body.KnockbackPermille) * attackerFacing, Fixed.Zero);
            defender.Facing = -attackerFacing;
            defender.LastAttackerIndex = projectile ? -1 : contact.Attacker;

            int chip = ScaleDamage(attack.ChipDamage, contact.Attacker, contact.Victim, counter: false, punish: false);
            if (!attack.Has(AttackFlags.ChipKills) && chip >= defender.Health)
            {
                chip = defender.Health - 1;
            }

            if (chip > 0)
            {
                defender.Health -= chip;
            }

            int hitstop = attack.Hitstop * _ctx.Tuning.BlockHitstopPermille / 1000;
            defender.HitstopRemaining = hitstop;
            _ctx.AddMeter(ref defender.Ultimate, _ctx.Tuning.UltimateGainOnBlock, contact.Victim);
            if (projectile)
            {
                _ctx.Projectiles.ConsumeHit(contact.ProjectileSlot);
            }
            else
            {
                ref FighterState attacker = ref state.Fighters[contact.Attacker];
                attacker.HitstopRemaining = hitstop;
                attacker.Contact |= MoveContact.Blocked;
                _ctx.AddMeter(ref attacker.Ultimate, _ctx.Tuning.UltimateGainOnBlock, contact.Attacker);
            }

            var flags = projectile ? CombatEventFlags.Projectile : CombatEventFlags.None;
            _ctx.Emit(CombatEventType.Block, contact.Attacker, contact.Victim, instance, chip > 0 ? chip : 0, 0, contact.Point, flags);

            int posture = attack.Has(AttackFlags.GuardCrush)
                ? defender.MaxPosture
                : (int)((long)attack.PostureDamage * _ctx.Stats[contact.Attacker].PostureDamagePermille / 1000);
            AddPosture(contact.Victim, posture, canBreak: true);

            if (defender.Health <= 0)
            {
                defender.Health = 0;
                ApplyReaction(contact.Attacker, attackerFacing, contact.Victim, attack, counter: false, hitstun: attack.Hitstun);
                KnockOut(contact.Attacker, contact.Victim, contact.Point, finisher: false);
            }
        }

        private void ResolveArmor(ref Contact contact, AttackSpec attack, int instance, int damagePermille)
        {
            var state = _ctx.State;
            ref FighterState defender = ref state.Fighters[contact.Victim];
            int damage = ScaleDamage(attack.Damage, contact.Attacker, contact.Victim, counter: false, punish: false) * damagePermille / 1000;
            if (attack.Damage > 0 && damage < 1)
            {
                damage = 1;
            }

            defender.Health -= damage;
            defender.ArmorHitsTaken++;
            defender.HitstopRemaining = _ctx.Tuning.ArmorHitstop;
            bool projectile = contact.ProjectileSlot >= 0;
            if (projectile)
            {
                _ctx.Projectiles.ConsumeHit(contact.ProjectileSlot);
            }
            else
            {
                ref FighterState attacker = ref state.Fighters[contact.Attacker];
                attacker.HitstopRemaining = _ctx.Tuning.ArmorHitstop;
                attacker.Contact |= MoveContact.Armored;
            }

            _ctx.Emit(CombatEventType.ArmorHit, contact.Attacker, contact.Victim, instance, damage, 0, contact.Point,
                projectile ? CombatEventFlags.Projectile : CombatEventFlags.None);

            if (defender.Health <= 0)
            {
                defender.Health = 0;
                ApplyReaction(contact.Attacker, state.Fighters[contact.Attacker].Facing, contact.Victim, attack, counter: false, hitstun: attack.Hitstun);
                KnockOut(contact.Attacker, contact.Victim, contact.Point, finisher: attack.Has(AttackFlags.Finisher));
                return;
            }

            int posture = (int)((long)attack.PostureDamage * _ctx.Tuning.PostureOnHitPermille / 1000);
            AddPosture(contact.Victim, posture, canBreak: true);
        }

        private void ResolveHit(ref Contact contact, AttackSpec attack, int attackerFacing, int instance, int sourceMove)
        {
            var state = _ctx.State;
            int victimIndex = contact.Victim;
            ref FighterState victim = ref state.Fighters[victimIndex];
            bool projectile = contact.ProjectileSlot >= 0;

            bool counter = false;
            bool punish = false;
            var victimMove = _ctx.MoveOf(victimIndex);
            if (victimMove != null)
            {
                if (victimMove.IsAttack && victim.ActionFrame <= victimMove.LastActiveFrame)
                {
                    counter = true;
                }
                else
                {
                    punish = true;
                }
            }
            else if (victim.Action == FighterAction.Landing)
            {
                punish = true;
            }

            bool juggle = victim.Action == FighterAction.Launched;
            bool inCombo = juggle || victim.Action == FighterAction.Hitstun || victim.Action == FighterAction.AirHitstun ||
                           victim.Action == FighterAction.Knockdown;
            if (!inCombo)
            {
                StartCombo(ref victim, contact.Attacker, attack.ProrationPermille);
            }

            if (victim.Action == FighterAction.Knockdown)
            {
                victim.OffTheGroundUsed = true;
            }

            victim.ComboHits++;
            int damage = ScaleDamage(attack.Damage, contact.Attacker, victimIndex, counter, punish);
            damage = (int)((long)damage * ComboScalePermille(ref victim) / 1000);
            if (attack.Damage > 0 && damage < 1)
            {
                damage = 1;
            }

            victim.Health -= damage;
            victim.ComboDamage += damage;
            bool lethal = victim.Health <= 0;
            if (lethal)
            {
                victim.Health = 0;
            }

            AddPosture(victimIndex, (int)((long)attack.PostureDamage * _ctx.Tuning.PostureOnHitPermille / 1000), canBreak: false);

            int hitstun = attack.Hitstun;
            if (counter)
            {
                hitstun += _ctx.Tuning.CounterHitHitstunBonus;
            }

            if (punish)
            {
                hitstun += _ctx.Tuning.PunishHitstunBonus;
            }

            if (victim.ComboHits > _ctx.Tuning.HitstunDecayStartHit)
            {
                hitstun -= (victim.ComboHits - _ctx.Tuning.HitstunDecayStartHit) / _ctx.Tuning.HitstunDecayEveryHits;
            }

            if (hitstun < _ctx.Tuning.MinHitstun)
            {
                hitstun = _ctx.Tuning.MinHitstun;
            }

            bool launched = ApplyReaction(projectile ? -1 : contact.Attacker, attackerFacing, victimIndex, attack, counter, hitstun, lethal);

            int hitstop = lethal ? _ctx.Tuning.KnockoutHitstop : attack.Hitstop + (counter ? _ctx.Tuning.CounterHitHitstopBonus : 0);
            victim.HitstopRemaining = hitstop;
            victim.LastAttackerIndex = projectile ? -1 : contact.Attacker;

            ref FighterState attacker = ref state.Fighters[contact.Attacker];
            if (projectile)
            {
                _ctx.Projectiles.ConsumeHit(contact.ProjectileSlot);
            }
            else
            {
                if (attacker.HitstopRemaining < hitstop)
                {
                    attacker.HitstopRemaining = hitstop;
                }

                if (attacker.MoveInstance == instance)
                {
                    attacker.Contact |= MoveContact.Hit;
                }
            }

            _ctx.AddMeter(ref attacker.Ultimate, (int)((long)damage * _ctx.Tuning.UltimateGainDealtPermille / 1000), contact.Attacker);
            _ctx.AddMeter(ref attacker.Shadow, _ctx.Tuning.ShadowGainPerHit, contact.Attacker);
            if (!attacker.InRage)
            {
                _ctx.AddMeter(ref attacker.Rage, (int)((long)damage * _ctx.Tuning.RageGainDealtPermille / 1000), contact.Attacker);
            }

            if (!victim.InRage)
            {
                _ctx.AddMeter(ref victim.Rage, (int)((long)damage * _ctx.Tuning.RageGainTakenPermille / 1000), victimIndex);
            }

            _ctx.AddMeter(ref victim.Ultimate, (int)((long)damage * _ctx.Tuning.UltimateGainTakenPermille / 1000), victimIndex);

            if (attacker.InShadow && !lethal)
            {
                ScheduleEcho(contact.Attacker, victimIndex, damage, attack.PostureDamage, contact.Point);
            }

            var flags = BuildHitFlags(attack, counter, punish, juggle, lethal, projectile, attacker.InRage, sourceMove, contact.Attacker);
            _ctx.Emit(CombatEventType.Hit, contact.Attacker, victimIndex, instance, damage, sourceMove, contact.Point, flags);
            if (launched)
            {
                _ctx.Emit(CombatEventType.Launch, contact.Attacker, victimIndex, instance, 0, 0, contact.Point, flags);
            }

            if (lethal)
            {
                bool finisher = attack.Has(AttackFlags.Finisher) || IsCinematicMove(contact.Attacker, sourceMove);
                KnockOut(contact.Attacker, victimIndex, contact.Point, finisher);
                return;
            }

            if (!projectile && attack.OnHitMoveIndex >= 0 && attacker.Action == FighterAction.Move && attacker.MoveInstance == instance)
            {
                StartPairedAction(contact.Attacker, victimIndex, attack.OnHitMoveIndex);
            }
        }

        private CombatEventFlags BuildHitFlags(AttackSpec attack, bool counter, bool punish, bool juggle, bool lethal, bool projectile, bool rage, int sourceMove, int attackerIndex)
        {
            var flags = CombatEventFlags.None;
            if (counter)
            {
                flags |= CombatEventFlags.Counter;
            }

            if (punish)
            {
                flags |= CombatEventFlags.Punish;
            }

            if (juggle)
            {
                flags |= CombatEventFlags.Juggle;
            }

            if (lethal)
            {
                flags |= CombatEventFlags.Lethal;
            }

            if (projectile)
            {
                flags |= CombatEventFlags.Projectile;
            }

            if (rage)
            {
                flags |= CombatEventFlags.Rage;
            }

            if (attack.Height == AttackHeight.Low)
            {
                flags |= CombatEventFlags.Low;
            }
            else if (attack.Height == AttackHeight.Overhead)
            {
                flags |= CombatEventFlags.Overhead;
            }

            bool heavy = attack.Has(AttackFlags.Launch | AttackFlags.Knockdown | AttackFlags.HardKnockdown | AttackFlags.WallBounce | AttackFlags.GroundBounce);
            if (!heavy && sourceMove >= 0)
            {
                heavy = _ctx.Blueprints[attackerIndex].Moves[sourceMove].HasTag(MoveTags.Heavy | MoveTags.Ultimate | MoveTags.Charged);
            }

            if (heavy)
            {
                flags |= CombatEventFlags.Heavy;
            }

            return flags;
        }

        /// <summary>
        /// Puts a victim into the reaction an attack causes (hitstun, air reset, launch, knockdown) with velocity
        /// facing away from the attacker. Returns true when the victim was launched from the ground.
        /// </summary>
        public bool ApplyReaction(int attackerIndex, int attackerFacing, int victimIndex, AttackSpec attack, bool counter, int hitstun, bool lethal = false)
        {
            ref FighterState victim = ref _ctx.State.Fighters[victimIndex];
            if (victim.PairedPartner >= 0 && victim.Action == FighterAction.Move)
            {
                // The victim was holding someone (throw, cinematic): let them go before reacting.
                _ctx.Controller.ReleasePartner(victimIndex, ref victim, _ctx.Blueprints[victimIndex].Moves[victim.MoveIndex]);
            }

            var body = _ctx.Blueprints[victimIndex].Body;
            bool launch = attack.Has(AttackFlags.Launch) || (counter && attack.Has(AttackFlags.LaunchOnCounter));
            bool knockdown = attack.Has(AttackFlags.Knockdown | AttackFlags.HardKnockdown);
            bool spike = attack.Has(AttackFlags.Spike);
            if (spike)
            {
                // Spikes only knock down airborne victims; grounded victims take normal hitstun.
                knockdown = !victim.Grounded;
                launch = launch && !victim.Grounded;
            }
            bool wasJuggled = victim.Action == FighterAction.Launched;
            bool launchedFromGround = false;
            FixedVector2 velocity;

            victim.MoveIndex = -1;
            victim.Stagger = StaggerKind.None;
            victim.ActionFrame = 0;
            victim.PairedPartner = -1;
            victim.PairedTechRemaining = 0;

            if (!victim.Grounded)
            {
                if (wasJuggled || launch || knockdown || lethal)
                {
                    victim.Action = FighterAction.Launched;
                    velocity = launch && !IsZero(attack.LaunchVelocity) ? attack.LaunchVelocity : attack.AirKnockback;
                    if (wasJuggled)
                    {
                        victim.JugglePoints += attack.JuggleCost;
                    }
                }
                else
                {
                    victim.Action = FighterAction.AirHitstun;
                    victim.StunRemaining = hitstun;
                    velocity = attack.AirKnockback;
                }
            }
            else if (launch || knockdown || lethal)
            {
                victim.Action = FighterAction.Launched;
                velocity = attack.LaunchVelocity;
                if (IsZero(velocity))
                {
                    velocity = lethal && !launch && !knockdown ? _ctx.Tuning.KnockoutLaunchVelocity : _ctx.Tuning.TripVelocity;
                }

                victim.Grounded = false;
                launchedFromGround = launch;
            }
            else
            {
                victim.Action = FighterAction.Hitstun;
                victim.StunRemaining = hitstun;
                velocity = new FixedVector2(attack.Knockback, Fixed.Zero);
            }

            if (lethal && velocity.Y.Raw <= 0)
            {
                velocity = _ctx.Tuning.KnockoutLaunchVelocity;
                victim.Action = FighterAction.Launched;
            }

            if (victim.Action == FighterAction.Launched && victim.Grounded)
            {
                victim.Grounded = false;
            }

            victim.Velocity = new FixedVector2(
                velocity.X.MulPermille(body.KnockbackPermille) * attackerFacing,
                velocity.Y.MulPermille(body.KnockbackPermille));

            if (victim.Action == FighterAction.Launched)
            {
                victim.HardKnockdown = attack.Has(AttackFlags.HardKnockdown);
                if (attack.Has(AttackFlags.WallBounce) && !victim.WallBounceUsed)
                {
                    victim.PendingWallBounce = true;
                }

                if (attack.Has(AttackFlags.GroundBounce) && !victim.GroundBounceUsed && (!spike || !victim.Grounded || victim.Velocity.Y.Raw < 0))
                {
                    victim.PendingGroundBounce = true;
                }
            }

            if (victim.Action == FighterAction.Hitstun || victim.Action == FighterAction.Launched)
            {
                victim.Facing = -attackerFacing;
            }

            if (lethal)
            {
                victim.IsKnockedOut = true;
            }

            victim.LastAttackerIndex = attackerIndex;
            return launchedFromGround;
        }

        /// <summary>Scripted damage of a paired action (throw, execution, ultimate cinematic).</summary>
        public void ApplyPairedHit(int attackerIndex, int victimIndex, PairedHit hit, MoveDefinition move)
        {
            ref FighterState victim = ref _ctx.State.Fighters[victimIndex];
            if (victim.IsKnockedOut)
            {
                return;
            }

            victim.ComboHits++;
            int damage = ScaleDamage(hit.Damage, attackerIndex, victimIndex, counter: false, punish: false);
            if (!hit.Unscaled)
            {
                damage = (int)((long)damage * ComboScalePermille(ref victim) / 1000);
            }

            if (hit.Damage > 0 && damage < 1)
            {
                damage = 1;
            }

            victim.Health -= damage;
            victim.ComboDamage += damage;
            AddPosture(victimIndex, hit.PostureDamage, canBreak: false);
            if (!victim.InRage)
            {
                _ctx.AddMeter(ref victim.Rage, (int)((long)damage * _ctx.Tuning.RageGainTakenPermille / 1000), victimIndex);
            }

            ref FighterState attacker = ref _ctx.State.Fighters[attackerIndex];
            bool lethal = victim.Health <= 0;
            var flags = lethal ? CombatEventFlags.Lethal : CombatEventFlags.None;
            _ctx.Emit(CombatEventType.PairedHit, attackerIndex, victimIndex, attacker.MoveInstance, damage, move.Index, victim.Position, flags);
            if (lethal)
            {
                victim.Health = 0;
                victim.IsKnockedOut = true;
                KnockOut(attackerIndex, victimIndex, victim.Position, finisher: true);
            }
        }

        private void StartPairedAction(int attackerIndex, int victimIndex, int moveIndex)
        {
            var state = _ctx.State;
            ref FighterState attacker = ref state.Fighters[attackerIndex];
            ref FighterState victim = ref state.Fighters[victimIndex];
            int hitstop = attacker.HitstopRemaining;
            _ctx.Controller.StartMove(attackerIndex, ref attacker, moveIndex, autoFace: false);
            attacker.HitstopRemaining = hitstop;
            var move = _ctx.Blueprints[attackerIndex].Moves[moveIndex];
            if (move.Paired == null || victim.IsKnockedOut)
            {
                return;
            }

            if (victim.ComboHits == 0)
            {
                StartCombo(ref victim, attackerIndex, 1000);
            }

            attacker.PairedPartner = victimIndex;
            victim.Action = FighterAction.PairedVictim;
            victim.ActionFrame = 0;
            victim.MoveIndex = -1;
            victim.StunRemaining = 0;
            victim.Stagger = StaggerKind.None;
            victim.PairedPartner = attackerIndex;
            victim.PairedTechRemaining = move.Paired.TechWindow;
            victim.PairedOffset = move.Paired.VictimOffset;
            victim.Velocity = FixedVector2.Zero;

            CombatEventType type = move.HasTag(MoveTags.Execution)
                ? CombatEventType.ExecutionStart
                : (move.HasTag(MoveTags.Ultimate | MoveTags.Cinematic) || move.Paired.Cinematic ? CombatEventType.UltimateCinematic : CombatEventType.GrabConnect);
            _ctx.Emit(type, attackerIndex, victimIndex, attacker.MoveInstance, moveIndex, 0, victim.Position, CombatEventFlags.None);
        }

        // ================================================================== helpers

        private void StartCombo(ref FighterState victim, int attackerIndex, int starterPermille)
        {
            victim.ComboHits = 0;
            victim.ComboDamage = 0;
            victim.JugglePoints = 0;
            victim.ComboStarterPermille = starterPermille;
            victim.ComboAttacker = attackerIndex;
            victim.WallBounceUsed = false;
            victim.GroundBounceUsed = false;
            victim.OffTheGroundUsed = false;
            victim.PendingWallBounce = false;
            victim.PendingGroundBounce = false;
        }

        private int ComboScalePermille(ref FighterState victim)
        {
            var tuning = _ctx.Tuning;
            int hits = victim.ComboHits;
            if (hits <= 1)
            {
                // The combo starter always deals full damage; its proration applies to the hits after it.
                return 1000;
            }

            int perHit = hits <= 2 ? 1000 : 1000 - tuning.ComboScalingStepPermille * (hits - 2);
            if (perHit < tuning.MinComboScalingPermille)
            {
                perHit = tuning.MinComboScalingPermille;
            }

            int total = perHit * victim.ComboStarterPermille / 1000;
            return total < tuning.MinComboScalingPermille ? tuning.MinComboScalingPermille : total;
        }

        private int ScaleDamage(int baseDamage, int attackerIndex, int victimIndex, bool counter, bool punish)
        {
            if (baseDamage <= 0)
            {
                return 0;
            }

            long damage = baseDamage;
            damage = damage * _ctx.Stats[attackerIndex].AttackPermille / 1000;
            damage = damage * _ctx.Stats[victimIndex].DamageTakenPermille / 1000;
            if (counter)
            {
                damage = damage * _ctx.Tuning.CounterHitDamagePermille / 1000;
            }

            if (punish)
            {
                damage = damage * _ctx.Tuning.PunishDamagePermille / 1000;
            }

            if (_ctx.State.Fighters[attackerIndex].InRage)
            {
                damage = damage * _ctx.Tuning.RageDamagePermille / 1000;
            }

            return damage < 1 ? 1 : (int)damage;
        }

        /// <summary>Adds posture. Returns true when it broke the guard.</summary>
        private bool AddPosture(int index, int amount, bool canBreak)
        {
            if (amount <= 0)
            {
                return false;
            }

            ref FighterState f = ref _ctx.State.Fighters[index];
            f.PostureRegenDelay = _ctx.Tuning.PostureRegenDelayFrames;
            f.Posture += amount;
            if (f.Posture < f.MaxPosture)
            {
                return false;
            }

            if (!canBreak)
            {
                f.Posture = f.MaxPosture - 1;
                return false;
            }

            f.Posture = f.MaxPosture;
            Stagger(index, StaggerKind.GuardBreak, _ctx.Tuning.GuardBreakStaggerFrames);
            _ctx.Emit(CombatEventType.GuardBreak, -1, index, f.Position);
            return true;
        }

        private void Stagger(int index, StaggerKind kind, int frames)
        {
            ref FighterState f = ref _ctx.State.Fighters[index];
            if (f.PairedPartner >= 0 && f.Action == FighterAction.Move)
            {
                _ctx.Controller.ReleasePartner(index, ref f, _ctx.Blueprints[index].Moves[f.MoveIndex]);
            }

            f.Action = FighterAction.Staggered;
            f.Stagger = kind;
            f.ActionFrame = 0;
            f.MoveIndex = -1;
            f.StunRemaining = frames;
            f.Velocity = f.Velocity.WithX(Fixed.Zero);
        }

        private void KnockOut(int attackerIndex, int victimIndex, FixedVector2 point, bool finisher)
        {
            ref FighterState victim = ref _ctx.State.Fighters[victimIndex];
            victim.IsKnockedOut = true;
            victim.Health = 0;
            var flags = CombatEventFlags.Lethal | (finisher ? CombatEventFlags.Finisher : CombatEventFlags.None);
            _ctx.Emit(CombatEventType.KnockOut, attackerIndex, victimIndex, 0, 0, 0, point, flags);
        }

        private bool IsCinematicMove(int attackerIndex, int moveIndex)
        {
            if (moveIndex < 0)
            {
                return false;
            }

            return _ctx.Blueprints[attackerIndex].Moves[moveIndex].HasTag(MoveTags.Ultimate | MoveTags.Execution | MoveTags.Cinematic);
        }

        private static bool IsZero(FixedVector2 v) => v.X.Raw == 0 && v.Y.Raw == 0;

        // ================================================================== shadow echoes

        private void ScheduleEcho(int attackerIndex, int victimIndex, int damage, int postureDamage, FixedVector2 point)
        {
            var echoes = _ctx.State.Echoes;
            for (int i = 0; i < echoes.Length; i++)
            {
                if (echoes[i].Active)
                {
                    continue;
                }

                int echoDamage = (int)((long)damage * _ctx.Tuning.ShadowEchoDamagePermille / 1000);
                echoes[i] = new EchoHitState
                {
                    Active = true,
                    TriggerFrame = _ctx.Frame + _ctx.Tuning.ShadowEchoDelayFrames,
                    Attacker = attackerIndex,
                    Victim = victimIndex,
                    Damage = echoDamage < 1 ? 1 : echoDamage,
                    PostureDamage = postureDamage * _ctx.Tuning.ShadowEchoDamagePermille / 1000,
                    Position = point,
                };
                return;
            }
        }

        private void ProcessEchoes()
        {
            var state = _ctx.State;
            for (int i = 0; i < state.Echoes.Length; i++)
            {
                ref EchoHitState echo = ref state.Echoes[i];
                if (!echo.Active || echo.TriggerFrame > state.Frame)
                {
                    continue;
                }

                echo.Active = false;
                ref FighterState victim = ref state.Fighters[echo.Victim];
                if (!victim.Active || victim.IsKnockedOut || victim.Action == FighterAction.WakeUp ||
                    (victim.Action == FighterAction.PairedVictim && victim.PairedPartner != echo.Attacker))
                {
                    continue;
                }

                victim.Health -= echo.Damage;
                if (victim.ComboHits > 0)
                {
                    victim.ComboDamage += echo.Damage;
                }

                AddPosture(echo.Victim, echo.PostureDamage, canBreak: false);
                if (victim.Action == FighterAction.Hitstun || victim.Action == FighterAction.AirHitstun)
                {
                    victim.StunRemaining += _ctx.Tuning.ShadowEchoHitstunBonus;
                }

                if (victim.HitstopRemaining < _ctx.Tuning.ShadowEchoHitstop)
                {
                    victim.HitstopRemaining = _ctx.Tuning.ShadowEchoHitstop;
                }

                bool lethal = victim.Health <= 0;
                _ctx.Emit(CombatEventType.ShadowEcho, echo.Attacker, echo.Victim, 0, echo.Damage, 0, echo.Position,
                    lethal ? CombatEventFlags.Lethal : CombatEventFlags.None);
                if (lethal)
                {
                    victim.Health = 0;
                    if (victim.Action != FighterAction.PairedVictim)
                    {
                        var echoAttack = EchoKnockoutAttack;
                        ApplyReaction(echo.Attacker, state.Fighters[echo.Attacker].Facing, echo.Victim, echoAttack, counter: false, hitstun: 0, lethal: true);
                    }

                    KnockOut(echo.Attacker, echo.Victim, echo.Position, finisher: false);
                }
            }
        }

        private static readonly AttackSpec EchoKnockoutAttack = new AttackSpec { Key = "echo" };

        private struct Contact
        {
            public int Attacker;
            public int ProjectileSlot;
            public int Victim;
            public int AttackIndex;
            public int Group;
            public FixedVector2 Point;
            public bool Grab;
            public bool Consumed;

            // Captured at detection so a trade still resolves after the attacker was hit earlier this frame.
            public AttackSpec Attack;
            public int AttackerFacing;
            public int Instance;
            public int SourceMove;
        }
    }
}

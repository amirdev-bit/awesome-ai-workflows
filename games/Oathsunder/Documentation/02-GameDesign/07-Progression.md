# OATHSUNDER — Progression, Itemization & Crafting

> **Document:** GDD 02-07 · **Owner:** Lead Systems Design · **Status:** Phase 2 design baseline
> **Scope:** every PvE power system and the cosmetic collection. Nothing in this document applies
> in ranked PvP, which uses normalized stats (§15).

---

## 1. Principles

1. **Power is PvE-only.** Levels, attributes, skills, talents, gear, runes, enchantments, sets and mastery bonuses make the campaign and endgame easier. They never enter Ranked, Casual PvP or Clan Wars.
2. **Skill beats stats.** A level-appropriate player with good fundamentals can clear any story content on Oathwarden with Rare gear. Gear exists to open up builds and higher difficulties, not to gate the story.
3. **Nothing that gives power is sold.** No XP, material, loot, upgrade or respec is purchasable (10-Economy-and-LiveOps §7).
4. **Canon invariants.** No item, rune, enchantment, skill, talent, set or mastery may change these canon combat constants (`00-Canon.md` §7.2):

| Invariant | Value |
|---|---|
| Simulation rate | 60 ticks/s |
| Input buffer | 8 frames; hitstop does not age it |
| Perfect Parry window | ≤ 6 frames; 2 frames when mashing |
| Hitstop classes | Light 8, Heavy 12, Ultimate 16 |
| Ember Rage | 8 s, +20% damage, 1-hit armor on heavy attacks |
| Umbral Shadow | 6 s, delayed shadow echo on every hit |
| Execution condition | Guard-broken, or ≤ 15% health and staggered |
| Bounces | One wall bounce and one ground bounce per combo |

Effects may add triggered bonuses *around* these rules, for example "when Ember Rage activates, heal 8%", but never
change the rule itself.

---

## 2. Character level (1–60)

### 2.1 XP curve

**XP to go from level *L* to *L*+1:** `XP(L) = round(220 × L^1.6)`

**Story mission XP:** `round(0.42 × XP(R))`, where *R* is the mission's recommended level.
**Side mission / hidden duel XP:** `round(0.30 × XP(R))`.
**Recommended level of region *r* (1–12), mission *m* (1–10):** `R = max(1, 5 × (r − 1) + ceil(m / 2))`.

With 10 story missions plus 3 side missions per region, a player enters each region at its canon
floor level (`00-Canon.md` §4) and reaches level 60 at the Solemn Throne finale.

| Level | XP to next level | Cumulative XP to reach this level | Story mission XP at this recommended level | Side mission XP | Typical play time to reach |
|---|---|---|---|---|---|
| 1 | 220 | 0 | 92 | 66 | 0 h |
| 5 | 2,889 | 4,185 | 1,213 | 867 | 1.5 h |
| 10 | 8,758 | 29,421 | 3,678 | 2,627 | 5 h |
| 20 | 26,550 | 191,133 | 11,151 | 7,965 | 15 h |
| 30 | 50,794 | 560,918 | 21,333 | 15,238 | 24 h |
| 40 | 80,486 | 1,198,266 | 33,804 | 24,146 | 33 h |
| 50 | 115,020 | 2,154,726 | 48,308 | 34,506 | 42 h |
| 60 | — (cap) | 3,476,741 | 64,672 | 46,194 | 52 h |

Other XP sources: encounters in open areas (5% of mission XP each), Survival waves, Boss Rush, Arena
contracts, Daily Challenges (10% of the current level's XP each), and the Undermourn Descent
(15–60% of the current level's XP per run). **PvP grants no character XP**, only account level, Pass XP and
mastery XP, so ranked cannot be used as an XP farm and PvE power stays a PvE reward.

### 2.2 After level 60: Renown

XP earned past the cap fills **Renown** levels. Each Renown level costs 150,000 XP and pays 2,000
Marks plus one **Reliquary Cache** (a cosmetic drawn from the earnable pool, with duplicate protection). Renown
grants no power.

### 2.3 Per-level rewards

| Reward | Amount |
|---|---|
| Attribute points | +3 per level (levels 2–60: 177) |
| Skill points (Sundered Path) | +1 per level (59) |
| Talent points (Oathmarks) | +1 at every even level (30) |
| Base HP | `1,000 + 60 × (L − 1)`, which is 4,540 at level 60 |
| Base posture | `1,000 + 20 × (L − 1)`, which is 2,180 at level 60 (engine scale: `fighter.rhen.json` `maxPosture` 1,000) |

Additional points: +1 attribute point and +1 skill point per Oathlord defeated (12 each), and +1 talent
point per legendary boss defeated for the first time (up to 12).

---

## 3. Attributes

Every attribute starts at **10**. Allocatable points total 189 (177 from levels + 12 from Oathlords),
and the per-attribute cap is **99**. **Respec is free and unlimited** at any camp shrine (attributes, skills and talents).

### 3.1 Diminishing returns

An attribute's **effective points** *E(A)* give its bonuses:

| Attribute value *A* | Each point above the previous band counts as | *E(A)* at band end |
|---|---|---|
| 1–30 | 1.0 | 30 |
| 31–50 | 0.6 | 42 |
| 51–70 | 0.3 | 48 |
| 71–99 | 0.1 | 50.9 |

Reference: *E*(10) = 10 · *E*(40) = 36 · *E*(60) = 45 · *E*(80) = 49 · *E*(99) = 50.9.

### 3.2 Per effective point

| Attribute | Theme | Per effective point |
|---|---|---|
| **Might** | Strength of the strike | +1.5% Heavy, Charged and Special physical damage · +1.0% posture damage dealt |
| **Finesse** | Precision and speed | +1.5% Light and air physical damage · +0.4% critical chance (PvE) · +1% Bleed buildup |
| **Vitality** | Endurance | +40 max HP · +0.2% physical damage reduction (multiplies with armor) |
| **Resolve** | Discipline of the guard | +2% max posture · +1.5% posture recovery rate · −0.5% chip damage taken |
| **Ember** | Life-force | +1.5% Fire and Radiant damage · +1% Rage meter gain · +0.5% healing received |
| **Umbra** | Shadow-force | +1.5% Shadow damage · +1% Shadow meter gain · +1% shadow echo damage |

**Hybrid elements** use the average effective points of two attributes: **Frost** = avg(Resolve,
Umbra), **Storm** = avg(Might, Ember), **Venom** = avg(Finesse, Umbra), each at +1.5% per averaged
effective point.

**Critical hits (PvE only):** base 5%, +0.4% per effective Finesse point, capped at 40%. Crit
damage ×1.5 (up to ×2.0 through talents and runes). Crits change damage only, never hitstun or
frame data.

### 3.3 Example level-60 builds

| Build | Might | Finesse | Vitality | Resolve | Ember | Umbra | Plays like |
|---|---|---|---|---|---|---|---|
| Oathwarden (defensive) | 30 | 20 | 50 | 60 | 44 | 45 | Parry and posture tank; War Hammer, Katana |
| Sundered (dual-meter) | 20 | 30 | 40 | 30 | 65 | 64 | Rage + Shadow uptime, Fire/Shadow infusions |
| Duelist (precision) | 45 | 65 | 40 | 30 | 35 | 34 | Crits and Light chains; Dual Blades, Daggers |

Each build spends exactly 189 allocatable points on top of the 60 starting points (249 total).

---

## 4. Damage and defense (PvE)

```
FinalDamage = MoveBase × (Power / 100)
            × (1 + AttributeBonus + ΣDamageBonuses)
            × CritMult × RageMult(1.20 when Ember Rage is active)
            × CounterMult (CH 1.10 / PC 1.15) × ComboScale
            × (1 − ArmorDR) × (1 − ElementResist)
```

| Term | Definition |
|---|---|
| `MoveBase` | The move's damage value in move data (the same number PvP uses) |
| `Power` | Weapon Power: `RarityBase × (1 + 0.06 × (iL − 1)) × (1 + 0.04 × UpgradeLevel)` |
| `RarityBase` | Common 100 · Uncommon 105 · Rare 112 · Epic 120 · Legendary 128 · Oathbound 135 |
| `ArmorDR` | `Armor / (Armor + 25 × AttackerLevel + 250)` |
| Armor per piece | `20 × WeightFactor × (1 + 0.06 × (iL − 1)) × RarityMult × (1 + 0.04 × UpgradeLevel)`; WeightFactor Silk 0.6 / Lamellar 1.0 / Plate 1.5; RarityMult = RarityBase / 100 |
| `ComboScale` | 01-Combat §12.2 |

Example: a Legendary item-level-60 +10 katana has Power 128 × 4.54 × 1.40 = **813.6**. A full Lamellar
Legendary iL60 +10 set gives about 813 armor, or **31.7%** DR against a level-60 attacker (Silk about 22%, Plate about 41%).

**Healing.** In story missions, HP is fully restored between encounters. Inside an encounter, healing
comes only from effects (drain, Execution talents, set bonuses). There is no consumable heal button.
**Restoratives** (sold by healer merchants in some region states, 02-World-and-Narrative §4) are
out-of-combat consumables: they can be used at checkpoints, between Survival waves and in Descent rest
rooms, never during an encounter.

---

## 5. Skill tree — The Sundered Path

The Sundered Path shapes **how Rhen uses Ember, Umbra and his guard**. Its nodes add triggered effects and
techniques around the canon meters. **Skill points:** 71 at level 60 with all Oathlords defeated. **Full
tree cost:** 96, so a complete build takes about 74% of the tree.

**Branches:** Ember Path (Rage and Fire offense) · Umbral Path (Shadow, evasion, echoes) · Warden's Path
(guard, parry, posture, Executions).

**Technique upgrades.** Some story and Trial missions award free, named Technique upgrades that sit
outside the tree and cost no points (03-Missions). They follow the canon invariants (§1) and are PvE only:

| Technique | Source | Effect |
|---|---|---|
| *Kindled* (Ember Rage upgrade) | `mission.emberfall.09` | Activating Ember Rage releases a 2 m kindling burst: knockback and 50% weapon damage (Fire) |
| *Reed-Still* (Perfect Parry upgrade) | `mission.weepingreeds.08` | Each Perfect Parry against a multi-hit string restores 3% posture |
| *Chime-Read* (Perfect Parry upgrade) | `mission.silkwind.02` | A soft chime plays 12 frames before any off-screen attack connects, so it can be parried by sound |
**Tier gates (points spent in that branch):** T1 0 · T2 5 · T3 12 · T4 20 · T5 (capstone) 25.
All effects are PvE only.

### 5.1 Ember Path (32 points)

| # | Node | Tier | Cost | Effect |
|---|---|---|---|---|
| E1 | **Kindled Blood** | T1 | 1 × 3 ranks | +5 / 10 / 15% Rage meter gain from dealing damage |
| E2 | **Hearthstrike** | T1 | 2 | Heavy attacks add 15 Burn buildup |
| E3 | **Emberskin** | T1 | 1 × 2 ranks | −4 / 8% damage taken while the Rage meter is at 50% or more |
| E4 | **Ashen Momentum** | T2 | 2 | Activating Ember Rage restores 8% max HP |
| E5 | **Cinder Trail** | T2 | 2 | `universal.dash` leaves an ember trail for 3 s that deals 30% weapon damage per second as Fire |
| E6 | **Furnace Heart** | T2 | 1 × 3 ranks | +6 / 12 / 18% Fire damage |
| E7 | **Blazing Finish** | T3 | 3 | Executions during Ember Rage refund 25% Rage meter when the mode ends |
| E8 | **Stoked Guard** | T3 | 2 | Blocking builds +50% more Rage meter |
| E9 | **Wildfire** | T3 | 3 | Burning enemies that die spread 40 Burn buildup to enemies within 3 m |
| E10 | **Iron Ember** | T4 | 3 | During Ember Rage, Perfect Parries emit an ember burst (120% weapon damage, Fire) |
| E11 | **Smoldering Resolve** | T4 | 2 | When Ember Rage ends, gain 20% damage reduction for 3 s |
| E12 | **Heart of the Pyre** | T5 | 5 | When the Rage meter fills, your next Heavy within 3 s ignites (+50% damage, guaranteed Burn) without spending the meter |

### 5.2 Umbral Path (32 points)

| # | Node | Tier | Cost | Effect |
|---|---|---|---|---|
| U1 | **Shade-Step** | T1 | 1 × 3 ranks | +5 / 10 / 15% Shadow meter from Perfect Dodges |
| U2 | **Long Shadow** | T1 | 2 | Rolls travel 12% farther |
| U3 | **Sable's Whisper** | T1 | 1 | Sable calls out unblockable attacks 0.3 s earlier (an audio cue on top of the telegraph) |
| U4 | **Echo Edge** | T2 | 1 × 3 ranks | Shadow echoes deal +8 / 16 / 24% damage |
| U5 | **Night Veil** | T2 | 2 | After a Perfect Dodge, ranged enemies lose their target on you for 1 s |
| U6 | **Umbral Bite** | T2 | 1 × 3 ranks | +6 / 12 / 18% Shadow damage |
| U7 | **Deep Time** | T3 | 3 | PvE Shadow Time lasts +20 frames |
| U8 | **Twin Shadow** | T3 | 3 | During Umbral Shadow, Perfect Dodges spawn an echo strike on the attacker (60% weapon damage) |
| U9 | **Hollowing** | T3 | 2 | Shadow echoes add 15 Umbra buildup |
| U10 | **Shadowed Heart** | T4 | 2 | While Umbral Shadow is active, regenerate 1% max HP per second |
| U11 | **Unseen Blade** | T4 | 3 | Attacks against enemies slowed by Shadow Time are always Counter Hits |
| U12 | **Sable Unchained** | T5 | 5 | Once per encounter, lethal damage leaves you at 1 HP instead, and Sable fills your Shadow meter |

### 5.3 Warden's Path (32 points)

| # | Node | Tier | Cost | Effect |
|---|---|---|---|---|
| W1 | **Tempered Guard** | T1 | 1 × 3 ranks | −10 / 20 / 30% chip damage taken |
| W2 | **Steady Stance** | T1 | 1 × 3 ranks | +5 / 10 / 15% max posture |
| W3 | **Knight's Recall** | T1 | 2 | Posture recovers 25% faster while you are not guarding |
| W4 | **Riposte Doctrine** | T2 | 2 | Perfect Parries deal +25% posture damage to the attacker |
| W5 | **Throwbreaker** | T2 | 2 | A successful throw tech staggers the PvE thrower for 30 frames |
| W6 | **Oathwall** | T2 | 1 × 3 ranks | +3 / 6 / 9% physical damage reduction |
| W7 | **Headsman's Eye** | T3 | 2 | Execution range +20% |
| W8 | **Warden's Due** | T3 | 3 | Executions restore 10% max HP |
| W9 | **Unbroken Vow** | T3 | 2 | Once every 30 s, a Guard Break against you becomes a 20-frame stagger |
| W10 | **Iron Rebuke** | T4 | 3 | After 3 Perfect Parries within 6 s, your next attack gains the Guard Crush property |
| W11 | **Sentinel's Patience** | T4 | 2 | Guarding for 2 s without being hit gives your next attack +15% damage |
| W12 | **The Last Warden** | T5 | 5 | Guard-broken enemies take +30% damage from all sources; Executions against elites and bosses deal +50% damage |

---

## 6. Talent tree — Oathmarks

Oathmarks are **passive build talents**: percentage bonuses that make a build specialize. **Talent
points:** 42 at most (30 from levels + 12 from legendary bosses). **Full tree cost:** 84 (each rank
costs 1 point; capstones cost 3 for a single rank), so a full build covers half the tree. All effects are PvE only.

**Branches:** Blade (offense) · Bastion (survival) · Arcana (elements and constructs).
**Tier gates:** capstones need 15 points in their branch. Every other node is open.

### 6.1 Blade (29 points)

| # | Talent | Ranks | Per rank |
|---|---|---|---|
| B1 | **Keen Edge** | 3 | +2% Light damage |
| B2 | **Heavy Hand** | 3 | +2% Heavy damage |
| B3 | **Aerialist** | 3 | +3% damage against airborne targets |
| B4 | **Opportunist** | 2 | +5% Counter Hit damage |
| B5 | **Punisher's Creed** | 2 | +5% Punish Counter damage |
| B6 | **Breaker** | 3 | +3% posture damage |
| B7 | **Charged Conviction** | 2 | +6% Charged attack damage |
| B8 | **Special Discipline** | 3 | +3% Special skill damage |
| B9 | **Wall-Bane** | 1 | +12% damage on wall-bounce and ground-bounce hits |
| B10 | **Critical Study** | 3 | +1.5% critical chance |
| B11 | **Executioner's Weight** | 1 | Critical damage ×1.5 → ×1.75 |
| B12 | **Master of Forms** (capstone) | 1 (3 pts) | Each switch between Light-chain and Heavy-chain moves in one combo gives +2% damage (max 10%) until the combo ends |

### 6.2 Bastion (25 points)

| # | Talent | Ranks | Per rank |
|---|---|---|---|
| S1 | **Hardy** | 3 | +3% max HP |
| S2 | **Steady Breath** | 3 | +4% posture recovery rate |
| S3 | **Second Wind** | 1 | Once per encounter, dropping below 25% HP heals 15% over 3 s |
| S4 | **Rooted** | 2 | −10% guard pushback |
| S5 | **Quick Rise** | 1 | Tech rolls travel 15% farther |
| S6 | **Resilient** | 3 | −5% status buildup taken |
| S7 | **Mender** | 2 | +10% healing received |
| S8 | **Last Stand** | 1 | +15% damage while below 20% HP |
| S9 | **Elemental Warding** | 3 | +5% all elemental resistance |
| S10 | **Unshaken** | 1 | Immune to Chill slow while guarding |
| S11 | **Iron Will** | 2 | −10% Guard Break stagger duration against you |
| S12 | **Bulwark of Varanth** (capstone) | 1 (3 pts) | Perfect Parries restore 2% max HP |

### 6.3 Arcana (30 points)

| # | Talent | Ranks | Per rank |
|---|---|---|---|
| A1 | **Emberlore** | 3 | +4% Fire damage |
| A2 | **Umbralore** | 3 | +4% Shadow damage |
| A3 | **Rimelore** | 3 | +4% Frost damage |
| A4 | **Stormlore** | 3 | +4% Storm damage |
| A5 | **Venomlore** | 3 | +4% Venom damage |
| A6 | **Dawnlore** | 3 | +4% Radiant damage |
| A7 | **Catalyst** | 2 | +5% status buildup |
| A8 | **Lingering Hex** | 2 | +10% status duration |
| A9 | **Elemental Harmony** | 1 | Targets carrying two different statuses take +15% damage from both |
| A10 | **Oath-light Well** | 2 | +4% Ultimate meter gain |
| A11 | **Arcane Reservoir** | 2 | Arcane constructs last +20% longer |
| A12 | **Confluence** (capstone) | 1 (3 pts) | Applying a third distinct status to a target triggers a Sundering Burst: 250% weapon damage, split equally across the three elements |

---

## 7. Weapon mastery and armor mastery

### 7.1 Weapon mastery

Full rules are in 06-Weapons §4: 20 levels per class (Arcane shares one track across its sub-forms), branches and
properties, cosmetics, and **Mastery Edge up to +5% damage in PvE only**. It is **disabled in ranked**, where every player gets the Full Kit.

### 7.2 Armor mastery

Armor comes in three weights. Wearing **3 or more pieces** of one weight earns that weight's armor mastery
XP (1 per 1% of max HP blocked or taken, 50 per encounter). Each track has 10 levels. **Armor mastery
never exceeds +3% of any stat and is disabled in ranked.**

| Weight | Base trait (PvE) | Mastery 3 / 6 / 9 | Mastery 2, 5, 8, 10 (cosmetic) |
|---|---|---|---|
| **Silk** (light) | +5% walk speed, −40% armor vs. Lamellar | +1 / 2 / 3% Shadow meter gain | Dye sets, *Silkwalker* title, cloth trail, mastery emote |
| **Lamellar** (medium) | Baseline | +1 / 2 / 3% all elemental resistance | Dye sets, *Lamellar Sworn* title, lacquer sheen, emote |
| **Plate** (heavy) | −5% walk speed, +50% armor vs. Lamellar, +10% max posture | +1 / 2 / 3% max posture | Dye sets, *Ironbound* title, forge-glow trim, emote |

Armor mastery XP to the next level: `200 × level²` (19,000 total to level 10).
Walk speed changes only movement velocity. They never change animation timing or frame data.

---

## 8. Magic and elemental damage

Six elements, matching the canon energies (Ember: fire and light; Umbra: shadow and void). **Void is
the lore term for concentrated Umbra and uses the Shadow element.**

### 8.1 Buildup rules

- Every elemental hit adds **buildup** for its element to the target (0–100). At 100 the status **procs**.
- Buildup decays by 12 per second after 2 s without new buildup.
- After a proc, the target is **immune** to that element's buildup for 8 s (regular), 15 s (elites) or 25 s (bosses).
- **Bosses need 200 buildup** to proc, and some statuses have boss-specific effects.
- **Elemental resistance** ranges from −50% (weakness) to +75% and reduces both damage and buildup.

### 8.2 Elements and statuses

| Element | Energy | Status | Effect (regular / elite) | Boss effect | Natural enemies |
|---|---|---|---|---|---|
| **Fire** | Ember | **Burn** | 1.5% max HP per second for 6 s; +10% posture damage taken | 0.4% max HP/s for 6 s | Strong vs. Rootbound (−25% resist); weak vs. Ironroot constructs (+50%) |
| **Shadow** | Umbra | **Hollowed** | 8 s: target healing −50%, meter gain −30%; shadow echoes deal +25% to it | Meter gain −30% for 8 s, no healing penalty | Unsworn resist Shadow +50% |
| **Frost** | Umbra-aligned | **Chill → Frozen** | Every 25 buildup: −5% animation speed (max −15%). At 100: **Frozen** for 1.5 s (elites 0.8 s) | Immune to Frozen. **Brittle** instead: +25% posture damage taken for 6 s | Weak: Weeping Reeds, Verdant Rot. Resistant: Rimewood (+75%) |
| **Storm** | Ember-aligned | **Shock** | 5 s: every hit the target takes arcs 8% of its damage to 2 enemies within 4 m. Proc stuns for 0.5 s (elites 0.25 s) | Posture recovery halted for 4 s | Weak: Sunken Archive (wet), Ironroot constructs. Resistant: Cloudspire (+50%) |
| **Venom** | Umbra-aligned | **Poison** | 1% max HP per second for 12 s; healing −30%; a re-proc adds a 2nd stack (max 2) | 0.3% max HP/s for 12 s | Resistant: Verdant Rot (+75%) |
| **Radiant** | Ember | **Seared** | 6 s: +12% damage taken from all sources. Unsworn: +20% instead, and they cannot use Umbra abilities (echoes, shadow-steps) | +12% damage taken for 6 s | Strong vs. all Unsworn (−25%); resisted by the Solemn Throne's Throne-Sworn (+50%) |
| Physical (weapon) | — | **Bleed** | Proc deals 8% max HP instantly and +15 posture | 3% max HP | Unsworn and constructs are immune (no blood) |

**PvP:** elemental statuses are off. Elemental VFX on weapons is cosmetic, and Daggers' *Open Wounds*
uses PvP Cuts instead (06-Weapons §14).

---

## 9. Enchantments (Mireth's Archive)

Enchanting opens when Mireth Vale joins (`mission.silkwind.03`), and from then on she enchants gear at
any camp. A weapon holds **1 Infusion + 1 Inscription**, and each armor piece holds **1 Ward**.
Enchantments can be replaced at will; removing one is free and refunds nothing. Mireth's **rune
crafting tiers** (02-World-and-Narrative §10) gate the enchantment tiers: Tier I when she joins, Tier II
after `quest.mireth.02`, Tier III after `quest.mireth.04`.

### 9.1 Weapon Infusions (element)

An Infusion converts part of physical damage into its element and adds buildup per hit: Light 8, Heavy 14,
Special 12, Charged 22, shadow echo 4.

| Infusion | Element | Tier I / II / III conversion | Buildup multiplier I / II / III |
|---|---|---|---|
| Emberbrand | Fire | 25 / 30 / 35% | ×1.0 / 1.2 / 1.4 |
| Umbral Script | Shadow | 25 / 30 / 35% | ×1.0 / 1.2 / 1.4 |
| Rimebite | Frost | 25 / 30 / 35% | ×1.0 / 1.2 / 1.4 |
| Stormglyph | Storm | 25 / 30 / 35% | ×1.0 / 1.2 / 1.4 |
| Nightshade Oil | Venom | 25 / 30 / 35% | ×1.0 / 1.2 / 1.4 |
| Dawnseal | Radiant | 25 / 30 / 35% | ×1.0 / 1.2 / 1.4 |

### 9.2 Weapon Inscriptions (property)

| Inscription | Tier I / II / III |
|---|---|
| **Keen** | +3 / 4 / 5% critical chance |
| **Balanced** | +5 / 6.5 / 8% posture damage |
| **Hungering** | Drain 1 / 1.5 / 2% of damage dealt as HP |
| **Swift** | +6 / 8 / 10% Rage and Shadow meter gain |
| **Headsman's** | +12 / 16 / 20% Execution damage |
| **Echoing** | +9 / 12 / 15% shadow echo damage |

### 9.3 Armor Wards

Emberward, Umbraward, Frostward, Stormward, Venomward, Radiantward: **+8 / 10 / 12% resistance** to that
element per piece at Tier I / II / III. Total resistance from Wards is capped at 60% per element.

### 9.4 Costs

| Tier | Marks | Materials |
|---|---|---|
| I | 2,000 | 5 × any region rare material |
| II | 6,000 | 3 × Oathlord material (any region) + 10 Umbral Dust |
| III | 15,000 | 1 × Oath-light Ingot + 30 Umbral Dust |

---

## 10. Runes

### 10.1 Slots

| Item | Common | Uncommon | Rare | Epic | Legendary | Oathbound |
|---|---|---|---|---|---|---|
| Weapon (● Circle: offense) | 0 | 1 ● | 1 ● | 2 ● | 2 ● | 2 ● + 1 ★ Star |
| Armor piece (■ Square: defense) | 0 | 0 | 1 ■ | 1 ■ | 1 ■ | 2 ■ |
| Charm (▲ Triangle: utility) | 0 | 0 | 1 ▲ | 1 ▲ | 2 ▲ | 2 ▲ |

Runes can be collected from the start of the game. **Socketing, fusing and crafting open when Mireth joins**
(`mission.silkwind.03`), and runes found earlier wait in the inventory. Slot shapes are strict: Circle
runes go only in Circle slots, and so on. **Unsocketing is free and never destroys the rune.** Fusing 3
identical runes of tier *n* makes one of tier *n*+1 at Mireth (I → II: 1,000 Marks, needs rune crafting tier
II; II → III: 5,000 Marks, needs tier III). Sources: elites (20% drop), bosses (100%), Descent shrines, raid
chests, and crafting at Mireth from Umbral Dust (Tier I: 40 Dust).

**Twilight socket.** While Rhen's Oath balance is in the **Dusk** band (02-World-and-Narrative §6.2), one
extra socket on the weapon accepts a rune of any shape. It goes dark, keeping the rune, when the balance
leaves Dusk.

**Named runes.** Besides the 30 core runes below, story missions (03-Missions, e.g. *Litany Break*,
*Held Breath I*, *Pack-Breaker*) and every boss (05-Bosses, e.g. *Rune of the Last Rite*) award named
unique runes. Each named rune takes the shape of its effect category (offense ●, defense ■, utility ▲).
Named runes with a numeral (I) can be fused like core runes; the others have a single tier.

### 10.2 Rune list (30)

| # | Rune | Shape | Tier I / II / III |
|---|---|---|---|
| 1 | Rune of Kindling | ● | +8 / 12 / 16% Fire damage |
| 2 | Rune of the Hollow | ● | +8 / 12 / 16% Shadow damage |
| 3 | Rune of Rime | ● | +8 / 12 / 16% Frost damage |
| 4 | Rune of the Tempest | ● | +8 / 12 / 16% Storm damage |
| 5 | Rune of Nightshade | ● | +8 / 12 / 16% Venom damage |
| 6 | Rune of Dawn | ● | +8 / 12 / 16% Radiant damage |
| 7 | Whetstone Rune | ● | +4 / 6 / 8% Light damage |
| 8 | Anvil Rune | ● | +4 / 6 / 8% Heavy damage |
| 9 | Rune of the Ambush | ● | +8 / 12 / 16% Counter Hit damage |
| 10 | Headsman's Rune | ● | +10 / 15 / 20% Execution damage |
| 11 | Rune of the Open Vein | ● | +10 / 15 / 20% Bleed buildup |
| 12 | Rune of the Bulwark | ■ | +5 / 7.5 / 10% max posture |
| 13 | Stoneskin Rune | ■ | +3 / 4 / 5% damage reduction |
| 14 | Rune of Mending | ■ | +8 / 12 / 16% healing received |
| 15 | Rune of Warding | ■ | +6 / 9 / 12% all elemental resistance |
| 16 | Rune of the Rooted | ■ | −10 / 15 / 20% knockback taken |
| 17 | Rune of the Unbowed | ■ | −6 / 9 / 12% Guard Break stagger duration |
| 18 | Rune of Second Breath | ■ | Once per encounter below 30% HP: regenerate 6 / 9 / 12% HP over 5 s |
| 19 | Rune of Clear Mind | ■ | −10 / 15 / 20% status buildup taken |
| 20 | Rune of the Hearth | ▲ | +6 / 9 / 12% Rage meter gain |
| 21 | Rune of the Gale | ▲ | +6 / 9 / 12% Shadow meter gain |
| 22 | Rune of Oath-light | ▲ | +5 / 7.5 / 10% Ultimate meter gain |
| 23 | Rune of the Scavenger | ▲ | +8 / 12 / 16% salvage materials |
| 24 | Fleetfoot Rune | ▲ | +3 / 4.5 / 6% walk speed |
| 25 | Rune of Lingering | ▲ | +10 / 15 / 20% status duration |
| 26 | Rune of the Pilgrim | ▲ | +5 / 7.5 / 10% character XP |
| 27 | Rune of the Balanced Scale | ★ | +0.5% damage per Oathstone sundered and +0.5% damage reduction per Oathstone restored (max 6% each) |
| 28 | Rune of the Tithe | ★ | Every 10 enemies defeated grant +5% Ultimate meter |
| 29 | Rune of Sable | ★ | 8% chance for any hit outside Umbral Shadow to spawn a shadow echo (standard echo: 30% damage, 10 frames later) |
| 30 | Rune of Aurem's Seal | ★ | +10% Fire and +10% Radiant damage; Ultimates apply Seared |

★ Star runes have one tier and drop only from Oathlord re-fights at Oathsundered difficulty or higher, raid final
chests, and Descent Vow 6+ clears.

---

## 11. Crafting and the Blacksmith (Tessen)

Upgrading and crafting open when Tessen joins at the Cold Forge (`mission.ironroot.02`). Before that,
gear improves through drops only. From then on, Tessen's forge travels with the camp. **Every upgrade always
succeeds.** There are no failure rolls, downgrades or item destruction.

| Forge tier | Unlocked by | Services |
|---|---|---|
| I | Tessen joins (`mission.ironroot.02`) | Upgrades up to +6, Rare crafting, salvage |
| II | `quest.tessen.01` *Cold Iron* | Upgrades up to +9, Epic crafting, Reforge, Re-temper |
| III | `quest.tessen.02` *The Abbess's Door* | Upgrades to +10, and the *Penitent Quench* path: Re-temper cost stops doubling on the same item (flat 1,500 Marks + 5 Umbral Dust) |

**Region state** (02-World-and-Narrative §4) can move Tessen's Marks prices by up to ±10%. For example,
restoring Ironroot's Oathstone lowers them and sundering it raises them. Material costs never change.

### 11.1 Materials by region

Each item carries an **origin region** (where it dropped or was crafted), and that region's materials upgrade it.
Common materials match the material rewards named in 03-Missions (e.g. *Salt-Bronze*, *Caldera Iron*).

| # | Region | Common material | Rare material | Oathlord material |
|---|---|---|---|---|
| 1 | Emberfall Monastery | Ash-Iron | Censer Resin | Tolling Ember |
| 2 | The Weeping Reeds | Salt-Bronze | Drowned Pearl | Bell-Tide Brass |
| 3 | Ironroot Forge | Caldera Iron | Magma Core | Anvil Heart |
| 4 | Silkwind Groves | Windsilk | Petal Lacquer | Twinbloom Seed |
| 5 | The Glass Ossuary | Sunglass | Sunbone | Seraph Feather |
| 6 | Rimewood Steppe | Rime-Steel | Aurora Shard | Greymane Fang |
| 7 | Lanternhold | Lantern Paper | Canal Gold | Masque Silk |
| 8 | The Verdant Rot | Glowcap Resin | Mycelial Thread | Bridal Spore |
| 9 | Cloudspire Aqueducts | Stormglass | Skywater Crystal | Still Wind Pearl |
| 10 | The Sunken Archive | Pressure-Pearl | Drowned Vellum | Memory Quill |
| 11 | Bloodmoon Citadel | Bloodsteel | Warbanner Silk | Duty-Seal Iron |
| 12 | The Solemn Throne | Sun-Gold | Sunmetal | Solemn Sun Fragment |

**Universal materials:** **Umbral Dust** (from salvage; used for runes and enchantments), **Oath-light
Ingot** (Oathlord re-fights, raids, Descent Vow clears, and weekly Boss Rush first clear; never sold),
**Ember Salts** (Survival and Endless; converts 10 common materials of one region into 10 of another at Tessen, so
players are never locked out of a region's materials).

Sources: regular enemies drop commons (15%), elites drop rares (40%), story Oathlord kills drop 3 of their material
(re-fights drop 1–2), and every region has 6 harvest nodes that refresh daily.

### 11.2 Upgrade tiers (+0 → +10)

**Marks cost (weapon):** `30 × n² × (1 + iL / 10)`, where *n* is the target level. **Armor** costs 60% of the weapon cost (rounded).
Each upgrade adds +4% Power or armor (so +10 = ×1.40).

| Target | Materials (weapon; armor needs 60%, rounded up) | Marks (weapon, iL 5) | Marks (weapon, iL 30) | Marks (weapon, iL 60) | Marks (armor, iL 60) |
|---|---|---|---|---|---|
| +1 | 4 common | 45 | 120 | 210 | 126 |
| +2 | 6 common | 180 | 480 | 840 | 504 |
| +3 | 8 common | 405 | 1,080 | 1,890 | 1,134 |
| +4 | 4 rare | 720 | 1,920 | 3,360 | 2,016 |
| +5 | 6 rare | 1,125 | 3,000 | 5,250 | 3,150 |
| +6 | 8 rare | 1,620 | 4,320 | 7,560 | 4,536 |
| +7 | 4 rare + 1 Oathlord material | 2,205 | 5,880 | 10,290 | 6,174 |
| +8 | 6 rare + 2 Oathlord material | 2,880 | 7,680 | 13,440 | 8,064 |
| +9 | 8 rare + 3 Oathlord material + 1 Oath-light Ingot | 3,645 | 9,720 | 17,010 | 10,206 |
| +10 | 3 Oathlord material + 3 Oath-light Ingot | 4,500 | 12,000 | 21,000 | 12,600 |
| **Total +0 → +10** | 18 common · 36 rare · 9 Oathlord · 4 Ingot | **17,325** | **46,200** | **80,850** | **48,510** |

### 11.3 Crafting, reforging and salvage

| Service | Rules |
|---|---|
| **Craft** | Tessen crafts **Rare** items of any unlocked class or armor weight at the current region's item level (2,000 Marks + 10 common + 4 rare), or **Epic** items (8,000 Marks + 8 rare + 2 Oathlord material). Legendary and Oathbound items cannot be crafted |
| **Reforge** | Raises an item's item level to the player's level and keeps its upgrades, runes and enchantments. Cost: `150 × ΔiL × (RarityBase / 100)` Marks. A favorite Legendary from level 20 stays viable at level 60 |
| **Re-temper** | Rerolls one affix on an Epic or better item: 1,500 Marks + 5 Umbral Dust, then doubling each time on the same item (cap 24,000) |
| **Salvage** | Returns 50% of the materials spent on upgrades, plus Umbral Dust: Common 1 · Uncommon 3 · Rare 8 · Epic 20 · Legendary 60 · Oathbound 150 |

---

## 12. Loot

### 12.1 Rarity tiers

| Tier | Color | Hex | RarityBase | Affixes | Rune slots (weapon) | Special |
|---|---|---|---|---|---|---|
| **Common** | Ash grey | `#9AA0A6` | 100 | 0 | 0 | — |
| **Uncommon** | Reed green | `#4CAF50` | 105 | 1 | 1 | — |
| **Rare** | Tide blue | `#3D7BD9` | 112 | 2 | 1 | — |
| **Epic** | Umbral violet | `#8E44C9` | 120 | 3 | 2 | — |
| **Legendary** | Ember orange | `#F28C28` | 128 | 3 + 1 unique effect | 2 | Signature weapons and set pieces |
| **Oathbound** | Oath gold on white-gold halo | `#E8C15A` | 135 | 3 + unique effect + **Oath trait** | 2 + ★ | Only at Oathsundered difficulty or higher, in raids, and at Descent Vow 4+ |

Affix pool (values scale with rarity; the table shows the Rare value): +4% Light damage, +4% Heavy damage, +6% HP,
+6% max posture, +6% element damage (per element), +8% status buildup, +6% meter gain (Rage/Shadow/Ultimate),
+2% critical chance, +10% critical damage, +8% Execution damage, +5% damage reduction against Unsworn,
+5% damage against a named faction.

### 12.2 Drop rules

| Source | Drop chance | Common | Uncommon | Rare | Epic | Legendary | Oathbound |
|---|---|---|---|---|---|---|---|
| Regular enemy | 5% | 70% | 25% | 5% | — | — | — |
| Elite | 100% (1 item) | — | 50% | 38% | 11% | 1% | — |
| Story mission chest | 100% (1 item) | — | — | 60% | 35% | 5% | — |
| Oathlord, first clear | Guaranteed signature Legendary + 1 Epic | — | — | — | 100% | 100% | — |
| Oathlord re-fight (Oathwarden) | 100% (2 items) | — | — | 30% | 55% | 15% | — |
| Oathlord re-fight (Oathsundered+) | 100% (2 items) | — | — | — | 70% | 27% | 3% |
| Legendary boss (repeat) | 100% (2 items) | — | — | — | 65% | 32% | 3% (Oathsundered+) |
| Raid final chest | 100% (3 items) | — | — | — | 40% | 50% | 10% |
| Descent final guardian | 100% (2 items) | — | — | — | 40% | 50% | 10% (Vow 4+) |

- **Smart loot:** 65% of drops match the equipped weapon class or armor weight. The other 35% roll across all unlocked classes and weights.
- **Item level** = the recommended level of the content, or the player's level, whichever is higher, capped at 60.
- **Duplicate protection:** a Legendary or Oathbound item cannot drop again until every other item in that source's pool has dropped once.

### 12.3 Pity (bad-luck protection)

| Counter | Rule |
|---|---|
| **Legendary pity** | Every Legendary-eligible chest without a Legendary adds +1.5 percentage points to the next chest's Legendary chance. The **40th** eligible chest is guaranteed. The counter resets on any Legendary |
| **Oathbound pity** | The **30th** Oathbound-eligible chest without one is guaranteed. Resets on any Oathbound |
| **Set completion** | Once 4 pieces of a set are owned, the 5th has double drop weight from its source |
| Transparency | Current pity counters show in the Reliquary (collection) screen. They cannot be bought, skipped or boosted |

### 12.4 Legendary signature weapons

Every boss's canon signature weapon drops as a Legendary on the first defeat (PvE effect listed; in
PvP the weapon's look is a cosmetic skin for its class).

| Weapon | Class | Source | Unique effect (PvE) |
|---|---|---|---|
| *Censer of Last Rites* | Chain Sword | `boss.kessh` | Extended-chain hits add 25 Burn buildup; the chain deals +10% to Burning enemies |
| *Tidebell* | Naginata | `boss.ilvane` | At 3 Tide, spinning hits ring a bell that staggers regular enemies within 3 m (once per 6 s) |
| *Worldanvil* | War Hammer | `boss.gorran` | Guard Crushes release a 4 m shockwave (150% weapon damage) |
| *Thorn & Bloom* | Dual Blades | `boss.petals` | Petal Count procs also burst for 120% weapon damage |
| *Noonpiercer* | Spear | `boss.maal` | Spearpoint tip hits add 30 Radiant buildup |
| *Fangs of the Long Night* | Gauntlets | `boss.tharuk` | Command grabs add 50 Frost buildup; grabs deal +25% to Frozen enemies |
| *Silken Lash* | Whip Blade | `boss.veloran` | The 3rd consecutive Tip Snap on one target deals +60% |
| *Bridal Reaper* | Scythe | `boss.myrrhen` | Harvest drain doubles (8%) against Poisoned enemies; blade hits add 15 Venom |
| *Unmoving Branch* | Staff | `boss.oru` | Eye of Calm catches restore 20% posture and give +100 Shadow meter (10%) |
| *Codex Umbrae* | Arcane (Grimoire) | `boss.quill` | Sealing Circle roots last 50% longer; glyphs add 40 Shadow buildup |
| *Vowcleaver* | Nodachi | `boss.hask` | Above 50% HP: armored heavies deal +15%. Below 50%: Longvow Armor absorbs 2 hits outside Rage |
| *First Oathblade* | Katana | `boss.aurem` | Draw-Cuts add 50 Radiant buildup and restore 3% HP |
| *Ninefold Fang* | Daggers | `boss.tamsin` | Every 9th dagger hit in a combo adds a free Backstab strike (80% weapon damage) |
| *Umbral Oathblade* | Katana | `boss.sableascendant` (defeated, not yielded to) | Shadow echoes deal +30% and add 10 Shadow buildup |
| *Crucible Fists* | Gauntlets | `boss.ferrousmaw` (raid) | Burning enemies caught by Fang Lock explode for 200% weapon damage (Fire) |
| *Mourning Fans* | Arcane (Warfan) | `boss.corvaine` | Paper Cranes add 25 Shadow buildup; Hollowed enemies take +20% crane damage |
| *Last Harvest* | Scythe | `boss.ossric` | Scythe Executions restore 15% HP and reset Tithe Mark |
| *Anchor Hymn* | Chain Sword | `boss.drownedchoir` (raid) | Chain Snare also drags enemies within 2 m of the target |
| *Penitent Maul* | War Hammer | `boss.velka` | Iron Resolve absorbs up to 3 strikes |
| *Antler Lance* | Spear | `boss.eirmund` | Pole Vault leaves a 3 s frost field (20 Frost buildup per second) |
| *Lotus Rod* | Staff | `boss.senajari` | Air combos of 10+ hits restore 5% HP on landing |
| *Spinecoil* | Whip Blade | `boss.isketh` | Silk Pull adds 40 Bleed buildup |
| *Hundred Edges* | Dual Blades | `boss.hundredhanded` (raid) | Parting Wind releases 6 spectral blades (40% weapon damage each) |
| *Oathsunder* | Nodachi | `boss.firstshadow` | While either Ember Rage or Umbral Shadow is active, the other meter fills +20% faster |
| *Lamp of the Unreborn* | Arcane (Soul Lantern) | First full clear of the Undermourn Descent (09-GameModes §6) | Wisps add 20 Shadow buildup and last 25% longer |

### 12.5 Oath traits (Oathbound items)

Each Oathbound item carries one **vow**: a strong bonus that holds while the vow is kept and turns
off for 5 s when it is broken. The HUD shows a small seal that cracks when a vow breaks.

| Oath trait | While kept | Broken by |
|---|---|---|
| **Vow of the Open Hand** | +12% damage | Guarding (Perfect Parries do not break it) |
| **Vow of Ember** | +15% Fire damage and +10% Rage gain | Activating Umbral Shadow |
| **Vow of Shadow** | +15% Shadow damage and +10% Shadow gain | Activating Ember Rage |
| **Vow of the Duelist** | +15% damage against the most recently hit enemy | Hitting a different enemy |
| **Vow of Mercy** | Executions heal 12% HP | Hitting a staggered enemy with anything but `Execute` |
| **Vow of Haste** | +10% damage, +5% walk speed | Standing still for more than 1 s |
| **Vow of the Tithe** | +1% damage per enemy defeated this encounter (max 15%) | Taking a hit above 20% of max HP |
| **Vow of Silence** | +20% critical damage | Using a Special skill |

---

## 13. Legendary sets

Each set has 5 pieces (Helm, Cuirass, Bracers, Greaves, Mantle) of one armor weight, with a **2-piece**
and a **4-piece** bonus. Region sets drop from that region's elites and Oathlord re-fights at
Oathwarden difficulty or higher, and from Boss Rush. Raid and Descent sets drop only there.

| # | Set | Weight | Source | 2-piece | 4-piece |
|---|---|---|---|---|---|
| 1 | **Vestments of the Ashen Choir** | Silk | Emberfall | +15% Rage meter gain | Activating Ember Rage releases a 3 m Ember nova: 250% weapon damage, 60 Burn buildup |
| 2 | **Reedwalker's Oilcloth** | Lamellar | Weeping Reeds | Rolls travel 15% farther | Perfect Dodges leave a Reed Mirror decoy that draws enemy attacks for 2 s, then bursts for 150% weapon damage |
| 3 | **Anvilborn Plate** | Plate | Ironroot | +10% posture damage on Heavies | During Ember Rage, strikes absorbed by armor reflect 30% of their damage as Fire |
| 4 | **Petalbound Silks** | Silk | Silkwind | +8% Light damage | Every 12th consecutive hit in a combo scatters petals: 150% Light damage + 30 Bleed |
| 5 | **Sunscorched Raiment** | Lamellar | Glass Ossuary | +15% Radiant damage | Perfect Parries instantly Sear regular enemies (+60 Radiant buildup against elites and bosses) |
| 6 | **Rimeborn Furs** | Plate | Rimewood | +15% Frost damage, +20% Frost resistance | Executing a Frozen enemy shatters it: 300% weapon damage (Frost) to enemies within 3 m |
| 7 | **Lantern-Night Regalia** | Silk | Lanternhold | Executions restore 8% HP | Executions grant *Encore*: +25% damage for 4 s |
| 8 | **Mycelial Shroud** | Lamellar | Verdant Rot | +20% Venom buildup | −12% damage taken from Poisoned enemies; drain 3% of damage dealt to them |
| 9 | **Stormcrest Vestments** | Silk | Cloudspire | +15% Storm damage | Air combos of 8+ hits call down lightning: 200% weapon damage (Storm), 50 Shock buildup |
| 10 | **Inkbound Robes** | Silk | Sunken Archive | +12% projectile and construct damage | Constructs last 30% longer; construct limit +1 per type |
| 11 | **Bloodmoon Warplate** | Plate | Bloodmoon Citadel | +10% damage against guard-broken enemies | Guard Breaks you cause stagger every enemy within 4 m for 1 s |
| 12 | **Throne-Sworn Regalia** | Plate | Solemn Throne | +15% Radiant and Fire damage | Landing an Ultimate restores 20% HP and Sears the target |
| 13 | **Enginewright's Regalia** | Plate | Raid: *The Engine Wakes* (`raid.ferrousmaw`) | +12% Charged attack damage | Releasing a fully charged attack vents steam: 1-hit armor for 2 s |
| 14 | **Chorister's Drowned Vestments** | Lamellar | Raid: *The Choir Beneath* (`raid.drownedchoir`) | +15% Shadow meter gain | During Umbral Shadow, each echo is followed by a second, fainter echo (20% damage) |
| 15 | **Gatewarden's Hundredfold** | Plate | Raid: *The Gate Below* (`raid.hundredhanded`) | +10% damage reduction against Unsworn | Every 5th blocked hit returns a spectral blade (100% weapon damage) |
| 16 | **Raiment of the Unsworn** | Silk | Undermourn Descent (Vow 4+) | +10% shadow echo damage | Echoes add 20 Shadow buildup; Hollowed enemies take +25% echo damage |

Set bonuses follow the canon invariants (§1): they add effects around Ember Rage and Umbral Shadow
and never change their duration or bonus.

**Boss legendary armor.** Each boss's legendary armor piece in 05-Bosses (e.g. *Vestments of the
Unreturned* from Abbot Kessh) is a standalone Legendary item outside these sets. The one exception is the
three raid bosses: their armor pieces (*Enginewright's Harness*, *Chorister's Drowned Surplice*,
*Gatewarden's Hundredfold Mail*) are the **Cuirass** of sets 13–15 and keep their own effect on top of
the set bonuses.

---

## 14. Cosmetic collection — The Reliquary

Neve keeps the **Reliquary**, the in-game collection book. It lists every cosmetic, its source, and
whether the player owns it.

| Category | Launch count | Earned in play | Store | Notes |
|---|---|---|---|---|
| **Outfits** (Rhen skins) | 48 | 32 (region, legendary boss, raid, Descent, ranked, mastery) | 16 | Full-body appearance; set looks can be transmogged once owned |
| **Weapon skins** | 90 (6 per moveset × 15) | 60 (mastery 19, bosses, events) | 30 | The silhouette and reach must match the base weapon within 5% (readability) |
| **Trails** | 45 | 35 (mastery 7/14/20, armor mastery) | 10 | Trails never use telegraph hues (crimson, amber, azure, violet, oath gold) at full saturation |
| **Emotes** | 30 | 20 | 10 | Usable in camp, PvP pre-round and after finishers |
| **Execution variants** | 45 (3 per moveset) | 30 (mastery 17, raids, Descent) | 15 | Same length and camera as the base Execution ±0.3 s (PvP pacing) |
| **Finisher variants** | 12 | 8 | 4 | Round-ending slow-motion flourish (VFX, color grade, sound stinger) |
| **Sable appearances** | 12 | 8 | 4 | Always violet-dominant, so echoes stay readable |
| **Titles** | 120 | 120 | 0 | Titles are never sold |
| **Clan banners** | 60 | 45 | 15 | Clan Wars territory banners |
| **HUD frames** | 20 | 14 | 6 | Health bar and portrait frames |

**Rules:** at least **65%** of all cosmetics are earnable through play, and every store cosmetic has
an earnable alternative in the same category. No cosmetic may change hitboxes, hurtboxes, animation timing, sound
cues for telegraphs, or the five telegraph colors. Store cosmetics come directly from the catalog
(no random packs); see 10-Economy §4.

---

## 15. No-pay-to-win guarantees and ranked normalization

### 15.1 Guarantees (public, printed on the store page and in the EULA summary)

1. **The store sells cosmetics only.** No gear, materials, XP, mastery, runes, enchantments, respecs, pity progress, currencies convertible to any of these, or time-savers.
2. **Premium currency (Lumens) cannot be converted** into Marks, materials or any earned currency, directly or indirectly (no "bundles" that include them).
3. **No paid randomness.** No loot box, gacha or random pack can be bought with real money or premium currency.
4. **Every gameplay unlock is earned by playing:** weapon classes (campaign; all classes are free in PvP and Training), moves (mastery or Veteran Start), modes and difficulty tiers.
5. **PvE leaderboards** (Time Trials, Endless, Descent) record only runs with Tempering off, and they list the gear power used. They are split into "Normalized" and "Open" boards.

### 15.2 Ranked stat normalization

In **Ranked, Casual PvP and Clan Wars** every fighter uses the **Normalized Profile**:

| Property | Normalized value |
|---|---|
| HP | 1,000 |
| Posture | 1,000 |
| Damage | Move-data base values (`MoveBase`) with no multipliers |
| Attributes, skills, talents, gear stats, runes, enchantments, set bonuses | Ignored |
| Weapon mastery | Disabled: **Full Kit** (all branches and properties), Mastery Edge 0% |
| Armor mastery | Disabled |
| Critical hits, elemental statuses | Off (Daggers use PvP Cuts) |
| Meter rules | 01-Combat §9.1 PvP values |
| Cosmetics | Displayed (outfits, weapon skins, trails, execution and finisher variants) |

Casual lobbies may enable **Kit Rules** (player PvE stats on). These matches are unranked, give no
Glory and cannot be used in Clan Wars.

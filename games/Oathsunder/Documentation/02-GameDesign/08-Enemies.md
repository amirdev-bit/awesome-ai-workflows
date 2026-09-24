# OATHSUNDER — Enemies, AI Personality & Adaptive Difficulty

> **Document:** GDD 02-08 · **Owner:** Lead Combat Design · **Status:** Phase 2 design baseline
> **Scope:** regular enemy archetypes (55), elite variants and affixes, the AI personality model,
> Adaptive Tempering (dynamic difficulty), player-pattern analysis, fairness caps and arena hazards.
> The Oathlords and legendary bosses (`00-Canon.md` §4–5) use the same six-trait model and Habit
> Ledger. Their phase scripts, trait values and learned answers are in 05-Bosses §0.2–0.3.

---

## 1. Principles and IDs

1. **Same rules as Rhen.** Enemies run on the same 60 Hz deterministic simulation, use canon weapon classes, and obey guard, parry, posture, juggle and Execution rules (01-Combat). They cannot do anything the player cannot see and answer.
2. **Every archetype teaches one thing.** Each region's roster introduces or tests a specific skill (the "Teaches" column), so the campaign doubles as a curriculum.
3. **Readable before hard.** An enemy is made harder by combining known tells in new ways, never by hiding tells.
4. **Deterministic AI.** All AI randomness uses the simulation's seeded RNG, so replays, Time Trials and resume-from-suspend reproduce exactly.

**IDs.** Archetypes use `enemy.<name>` (lowercase, no separators inside the name), e.g., `enemy.ashacolyte`.
Enemies use their weapon class's move data, often a subset, with enemy tuning. An archetype may add
at most **one** signature move with the ID `enemy.<name>.<move>` (e.g., `enemy.bellwarden.bellgrab`). Elite
variants share their archetype's ID and add the suffix `.elite` for data overrides (e.g., `enemy.ashacolyte.elite`).

---

## 2. Roles

| Role | Job in an encounter | Token behavior |
|---|---|---|
| **Grunt** | Honest pressure; teaches fundamentals | Normal priority |
| **Skirmisher** | Hit-and-run, flanking, spacing | Takes a token only when the player is in its range band |
| **Bruiser** | Armor, heavy damage, posture pressure | High priority |
| **Duelist** | 1v1 fundamentals: parries, mixups, stances | High priority; fights best alone |
| **Grappler** | Throws and command grabs | Normal priority; grabs only while holding a token |
| **Juggernaut** | Very high HP and armor; demands posture play and Executions | High priority; slow cadence |
| **Controller** | Pulls, traps, area denial, hazard use | Uses the **ranged token** |
| **Ranged / Zoner** | Projectiles, long reach | Uses the ranged token |
| **Support / Summoner / Commander** | Buffs, heals, summons; a priority target | Rarely attacks; never holds a melee token while supporting |
| **Assassin** | Ambush, cross-ups, feints | Must wait for the **back-attack token** |
| **Swarm** | Low HP, many bodies, simple patterns | Low priority; counts as half a token |

---

## 3. AI personality model

Every enemy (and every boss phase) has a six-parameter profile, each 0–100. The AI is a
**utility selector**: each candidate action gets a score, and the AI picks from the top 3 by weighted
seeded random. The parameters shape those scores through the formulas below.

### 3.1 Parameters and behavior mapping

| Parameter | Meaning | Behavior mapping (targets) |
|---|---|---|
| **Aggression** (Ag) | Desire to initiate | Attack attempts per second while holding a token: `0.25 + 1.25 × Ag/100`. Preferred distance: `bestPokeRange × (1.3 − 0.6 × Ag/100)`. Chance to keep pressing after a blocked string: `0.2 + 0.6 × Ag/100` |
| **Patience** (Pa) | Willingness to wait and react | Guard-hold time before acting: `0.2 + 1.8 × Pa/100` s. Whiff-punish attempt chance: `0.2 + 0.75 × Pa/100`. Chance to wait out a string instead of mashing: `Pa/100`. Perfect Parry attempt chance against a readable attack: `0.1 + 0.5 × Pa/100` (capped by difficulty, §8) |
| **Adaptivity** (Ad) | How fast it learns the player's habits | Habit Ledger learning rate: `α = 0.02 + 0.18 × Ad/100` per observation. At 0 the enemy ignores the Ledger (§7) |
| **Cunning** (Cu) | Deception and mixups | Mixup chance after conditioning (high/low/throw): `0.1 + 0.6 × Cu/100`. Feints (cancel into Guard, delayed strings): `0.4 × Cu/100`. Baits (whiffing at range to draw a punish): `0.3 × Cu/100`. Throw weight against a guarding player: `1 + Cu/100` |
| **Courage** (Co) | Risk tolerance under pressure | Disengages when its HP fraction drops below `0.5 − 0.4 × Co/100`. Chance to attack while at frame disadvantage: `0.05 + 0.45 × Co/100`. At Co < 30, a wounded enemy may retreat to allies or call reinforcements (once per encounter) |
| **Showmanship** (Sh) | Style over efficiency | Chance to taunt after knocking Rhen down: `0.5 × Sh/100` (a taunt is a deliberate 40-frame punish window). Weight for flashy moves (launchers, Charged attacks, Ultimates): `×(1 + Sh/100)`. Chance to choose an Execution over a combo finish: `Sh/100`. Barks and entrance flourishes scale with Sh |

### 3.2 Reaction model

- The AI reacts to **observable events** only: an animation's startup has begun, a hitbox is active, a position or meter has changed, a telegraph is shown. It never sees buffered or pending inputs.
- **Reaction delay** = max(difficulty reaction floor, archetype base reaction) + 0–6 frames of seeded jitter.

| Difficulty | Reaction floor (regular / elite / boss) |
|---|---|
| Pilgrim | 24 / 22 / 20 frames |
| Oathwarden | 18 / 16 / 15 frames |
| Oathsundered | 14 / 13 / 12 frames |
| Undermourn | 11 / 11 / 10 frames |

### 3.3 Group behavior

- **Attack tokens:** only token holders may be in startup or active frames against Rhen. Token count: Pilgrim 1, Oathwarden 2, Oathsundered 2 (3 in set pieces), Undermourn 3. A holder releases its token when its action recovers.
- **Ranged token:** 1 ranged or Controller attack in flight at a time (2 on Undermourn).
- **Back-attack token:** 1 enemy at a time may attack from behind Rhen, and only after a 30-frame off-screen/edge warning (01-Combat §13.2).
- Enemies without tokens circle at their preferred distance, reposition, guard, taunt (Sh) or buff (Support). They never stand idle in the player's face.

---

## 4. Regular archetypes (55)

The roster uses the enemy names in the mission rosters of 03-Missions and the Husk/Remnant taxonomy of
02-World-and-Narrative (§3.3): **Husks** are feral Unsworn and appear in every region, while **Remnants**
remember who they were. Columns: **AI** = Ag / Pa / Ad / Cu / Co / Sh.

### 4.1 Emberfall Monastery (levels 1–5): the Choir, Rhen's old company, the first Husks

| ID | Name | Class | Role | Signature behavior | Teaches | Elite variant | AI |
|---|---|---|---|---|---|---|---|
| `enemy.oathwardensentinel` | Oathwarden Sentinel | Katana | Duelist | Rhen's former company: honest Katana strings with one telegraphed low; guards between strings | Light/Heavy chains, standing and crouching guard (Prologue) | **Oathwarden Sergeant** (Stillwater stance) | 45/55/20/30/60/20 |
| `enemy.ashennovice` | Ashen Novice | Staff | Grunt | Three-strike staff string that always ends in a slow, telegraphed Temple Strike overhead | Standing guard against overheads; the first clean Perfect Parry | **Ashen Brother** (Eye of Calm counter) | 35/60/10/15/50/20 |
| `enemy.censerbearer` | Censer-Bearer | Chain Sword | Controller | Swings an ember censer at 3.5 m, leaving 2 s burning ash patches; grabs a bound or guarding Rhen (techable throw) | Throw tech; closing distance with dash; throwing a guarding enemy | **Censer Deacon** (Chain Snare) | 40/55/15/30/35/30 |
| `enemy.bellwarden` | Bell Warden | War Hammer | Bruiser (always spawns as an elite) | Armored overhead swings. If Rhen guards for more than 2 s, it uses **Bell Grab** (`enemy.bellwarden.bellgrab`, techable) | Armor, throws, not turtling | Base form is elite. On Oathsundered+ it adds Iron Resolve | 55/40/15/35/70/25 |
| `enemy.undermournhusk` | Undermourn Husk | Gauntlets (claws) | Skirmisher (Husk) | Screams before every lunge; low claw sweeps; packs of 2–3 on the Drowned Stair | Perfect Dodge into Shadow Time; crouch guard against lows | **Deep Husk** (Echoing affix) | 75/15/5/10/90/10 |
| `enemy.ashhusk` | Ash Husk | Gauntlets (claws) | Swarm (Husk) | Bursts out of ash piles in waves; one attacker at a time, the rest circle | Group spacing; Ember Rage against crowds | **Cinder-Crowned Husk** (Ironclad affix) | 70/15/5/10/85/10 |
| `enemy.cinderpenitent` | Cinder Penitent | War Hammer | Bruiser | Its hits add Burn. Below 25% HP it ignites into a crimson unblockable burst after 60 frames | Dodging unblockables; finishing with an Execution before the burst | **Cinder Flagellant** (burst radius 3 m → 4.5 m) | 60/25/10/15/95/35 |

*Mini-boss:* `enemy.staireel`, **the Stair-Eel** (Whip Blade body-lash, Juggernaut), appears only in
`mission.emberfall.03`. It teaches the posture bar, Guard Break and the first Execution (00-Overview §7).

### 4.2 The Weeping Reeds (levels 5–10): Ilvane's household, reavers, Drowned Husks

| ID | Name | Class | Role | Signature behavior | Teaches | Elite variant | AI |
|---|---|---|---|---|---|---|---|
| `enemy.ferryhook` | Ferry-Hook | Naginata (boathook) | Controller | Hooks Rhen and drags him toward canal edges; slow low sweeps | Crouch guard; hazard awareness | **Ferry-Hook Bosun** (pull range +50%) | 50/30/5/20/80/10 |
| `enemy.drownedhusk` | Drowned Husk | Gauntlets (claws) | Grunt (Husk) | Climbs out of the water in groups; grab-and-drag-under (a techable throw) | Throw tech under group pressure | **Bloated Husk** (Vengeful affix) | 60/20/5/15/85/10 |
| `enemy.saltreaver` | Salt Reaver | Spear (harpoon) | Skirmisher | Pokes at max range from the fog edge, then fades back | Whiff punishing; dash, then guard, to approach | **Reaver Captain** (Pole Vault cross-overs) | 45/70/20/40/40/15 |
| `enemy.boglurker` | Bog Lurker | Daggers | Assassin | Ambushes from under the water (ripple and edge-arrow telegraph); cross-up stabs with Bleed | Off-screen warnings; guarding cross-ups | **Bog Matriarch** (Shadowstep feints) | 70/25/20/55/45/20 |
| `enemy.quietwatersister` | Quiet Water Sister | Naginata | Duelist | Spinning strings that mix the Undertow low with the Breaking Wave overhead; builds Tide | High/low mixups; interrupting momentum | **Quiet Water Matron** (Whirlpool Guard) | 55/45/35/60/55/45 |

### 4.3 Ironroot Forge (levels 10–15): the forge-city's guards and its clamped workers

| ID | Name | Class | Role | Signature behavior | Teaches | Elite variant | AI |
|---|---|---|---|---|---|---|---|
| `enemy.slagguard` | Slagguard | War Hammer | Bruiser | Armored swings; charges Stoking into a Forgequake Guard Crush (gold telegraph) | Reading Guard Crush; parrying armored attacks | **Slagguard Foreman** (Ironclad affix) | 55/45/15/20/75/30 |
| `enemy.rivethusk` | Rivet Husk | Gauntlets (riveted claws) | Swarm (Husk) | Riveted plates give 1-hit armor on its first attack of each string. The plates fall off after a Guard Break | Multi-hit strings and Guard Breaks strip armor | **Boilerplate Husk** (armor on every attack) | 75/15/5/10/95/15 |
| `enemy.tallyclerk` | Tally-Clerk | Daggers | Support / Assassin | Throws knives from range and marks Rhen with a tally; at 5 tallies it calls a Slagguard, then retreats | Priority targets; handling knife zoning | **Chief Clerk** (Mirage Feint) | 40/60/30/60/25/30 |
| `enemy.forgethrall` | Forge Thrall | Gauntlets | Grappler | Shadow-clamped workers: jab strings into Fang Lock command grabs (violet). An Execution unclamps a Thrall instead of killing it, and it flees | Command grabs (jump or strike them); Execution as mercy | **Thrall Overseer** (Skyseize anti-air grab) | 70/30/30/55/65/25 |

### 4.4 Silkwind Groves (levels 15–20): Thornsworn, loom wardens, Red Tallies raiders

| ID | Name | Class | Role | Signature behavior | Teaches | Elite variant | AI |
|---|---|---|---|---|---|---|---|
| `enemy.thornsworn` | Thornsworn | Dual Blades | Duelist | The Petals' bodyguard: 6-hit strings with a delayed 4th/5th hit; Thornwhirl cross-up | Parrying strings; the anti-mash rule | **Thornsworn Captain** (Petal Shroud) | 65/40/40/55/50/60 |
| `enemy.windhusk` | Wind Husk | Gauntlets (claws) | Skirmisher (Husk) | Rides the wind gusts and always attacks from the gust's direction | Reading arena wind; turning to face cross-ups | **Gale-Torn Husk** (Swift affix) | 70/20/10/25/70/20 |
| `enemy.loomwarden` | Loom Warden | Whip Blade | Controller | Pulls Rhen into ambushes with silk threads; Silk Pull from 5 m | Escaping pulls (guard or jump); anti-air | **Master Weaver** (two threads) | 45/50/20/40/40/30 |
| `enemy.petaldancer` | Petal Dancer | Arcane (Warfan) | Ranged | Returning Fan and Paper Cranes from range; Turning Screen deflects projectiles | Approaching zoners; Perfect Dodge through projectiles | **Crane Dancer** (5 cranes) | 40/55/30/45/35/50 |
| `enemy.redtalliesraider` | Red Tallies Raider | Katana | Duelist (mirror) | Ludo Skarre's mercenaries. After the player repeats the same attack twice, it enters Stillwater; Crescent Rush punishes | Counter stances; lows and throws beat stances | **Red Tallies Sergeant** (Draw-Cut after parries) | 50/60/55/50/60/40 |

### 4.5 The Glass Ossuary (levels 20–25): the Glassblind, bone scavengers, Mirage Husks

| ID | Name | Class | Role | Signature behavior | Teaches | Elite variant | AI |
|---|---|---|---|---|---|---|---|
| `enemy.glassblindzealot` | Glassblind Zealot | Spear | Zoner | Blind soldiers who track by sound. Dashes and rolls are "loud" and draw a tip poke, while walking is quiet. Planted Vigil against jump-ins | Spacing and movement discipline; the risk of jumping | **Glassblind Veteran** (Lunging Line) | 40/70/35/35/60/20 |
| `enemy.bonecrawler` | Bone-Crawler | Scythe | Controller | Hooks Rhen and drags him onto its Tithe Marks | Reading pulls; not standing on traps | **Bone Reaper** (2 Tithe Marks) | 50/50/30/50/45/35 |
| `enemy.miragehusk` | Mirage Husk | Daggers (claws) | Assassin (Husk) | Rises from its own reflection. A mirage copy repeats each of its attacks 20 frames later | Tracking the real enemy; blocking delayed repeats | **Mirage Twin** (two copies) | 60/35/15/60/50/30 |
| `enemy.sunlancepilgrim` | Sunlance Pilgrim | Staff | Support | Heals allies 5% HP per second while it is not being attacked; launches Rhen if engaged | Target priority | **Sunlance Hierophant** (grants allies Oath-Shielded) | 30/60/20/30/30/40 |

### 4.6 Rimewood Steppe (levels 25–30): Greymane clans, hounds, aurora shamans

| ID | Name | Class | Role | Signature behavior | Teaches | Elite variant | AI |
|---|---|---|---|---|---|---|---|
| `enemy.greymaneraider` | Greymane Raider | Gauntlets | Grappler | Clinch throws after blocked jabs; Skyseize catches jump-ins | Throw-tech timing; disciplined jumping | **Greymane Warchief** (Commander affix) | 70/35/40/50/70/45 |
| `enemy.frosthound` | Frost Hound | Daggers (fang rig) | Skirmisher (pack) | Packs of 3 that flank; only one bites at a time, and they rotate | Group positioning; using walls | **Frost Hound Alpha** (howl: pack +10 Aggression) | 80/20/25/45/60/20 |
| `enemy.aurorashaman` | Aurora Shaman | Arcane (Soul Lantern) | Summoner | Summons frost wisps; Soul Tether keeps Rhen close | Construct management; priority | **Aurora Oracle** (wisps add Frost buildup) | 30/60/35/45/25/50 |
| `enemy.rimehusk` | Rime Husk | Nodachi | Juggernaut (Husk) | Frost-armored charges; its blocked hits add Frost buildup | Status buildup through guard; dodging instead of blocking | **Rime Revenant** (Frozen on proc lasts 1.5 s) | 50/45/15/20/90/30 |

### 4.7 Lanternhold (levels 30–35): Masquers, the Duke's Silks, Guild toughs, Paper Wraiths

| ID | Name | Class | Role | Signature behavior | Teaches | Elite variant | AI |
|---|---|---|---|---|---|---|---|
| `enemy.masquer` | Masquer | Whip Blade | Ranged / Duelist | Full-arena Lash Line; Cocoon when approached | Beating the longest range; dead zones | **Masque Virtuoso** (Gossamer Storm) | 45/60/45/55/45/80 |
| `enemy.dukessilk` | Duke's Silk | Chain Sword | Bruiser | The Duke's guard: Chain Snare pulls Rhen into allies' attacks; guards often | Posture pressure; forcing Guard Breaks | **Silk Captain** (Bulwark affix) | 50/55/30/35/65/35 |
| `enemy.guildtough` | Guild Tough | Daggers | Assassin | Shadowstep feints and knife zoning | Not over-reacting; reading feints | **Guild Shade** (Mirage Feint) | 60/45/50/75/40/40 |
| `enemy.paperwraith` | Paper Wraith | Arcane (Warfan) | Ranged (Husk) | Floats; crane swarms; vanishes after 3 hits and reappears elsewhere | Tracking targets; anti-air | **Lantern Wraith** (Stormbound affix) | 45/40/20/50/30/45 |

### 4.8 The Verdant Rot (levels 35–40): the Rootbound, spore-kin, the hive

| ID | Name | Class | Role | Signature behavior | Teaches | Elite variant | AI |
|---|---|---|---|---|---|---|---|
| `enemy.glowcaphusk` | Glowcap Husk | War Hammer (fungal club) | Juggernaut (Husk) | Regenerates 2% HP per second. Unless finished by an Execution, it regrows once at 20% HP | Posture and Executions as a kill condition | **Glowcap Colossus** (regrows twice) | 50/40/10/15/100/25 |
| `enemy.sporewalker` | Spore-Walker | Arcane (Soul Lantern) | Support | Venom spore clouds that linger 5 s; revives one fallen Rootbound per encounter | Area denial; priority | **Spore Matron** (two revives) | 30/60/25/40/35/35 |
| `enemy.rootboundacolyte` | Rootbound Acolyte | Scythe | Duelist | Blade hits drain HP and add Venom | Status management; not trading hits | **Rootbound Hierarch** (Venomous affix) | 60/40/30/40/70/40 |
| `enemy.myconidbride` | Myconid Bride | Whip Blade (hyphae lash) | Controller | Pulls Rhen into spore clouds; hits root Rhen for 20 frames | Escaping pulls under status pressure | **Bride of the Heartwood** (two lashes) | 45/50/30/45/50/60 |
| `enemy.marrowdrone` | Marrow Drone | Daggers | Swarm (hive) | Flying trio with Venom stings; has to be juggled or anti-aired | Air combos; anti-air | **Marrow Stinger** (Bleed stings) | 75/15/10/20/60/15 |

### 4.9 Cloudspire Aqueducts (levels 40–45): wardens, storm monks, Still Wind adepts

| ID | Name | Class | Role | Signature behavior | Teaches | Elite variant | AI |
|---|---|---|---|---|---|---|---|
| `enemy.aqueductwarden` | Aqueduct Warden | Spear | Controller | Planted Vigil and Lunging Line push Rhen toward bridge edges | Positioning near hazards; back rolls | **Aqueduct Keeper** (Ironclad affix) | 50/55/30/35/70/30 |
| `enemy.galehusk` | Gale Husk | Gauntlets (claws) | Skirmisher (aerial Husk) | Glides overhead on storm winds; diving overheads | Anti-air; standing guard against overheads | **Storm-Torn Husk** (Stormbound affix) | 60/30/15/30/60/30 |
| `enemy.stormbellmonk` | Storm-Bell Monk | Gauntlets | Duelist (rushdown) | Storm-charged frame traps with Shock buildup; Rising Iron reversal | Frame traps; when to use a reversal | **Storm-Bell Master** (Swift affix) | 80/30/45/50/60/50 |
| `enemy.stillwindadept` | Still Wind Adept | Staff | Duelist (juggler) | Launches Rhen and air-combos him (enemy Stun Ceiling: 180 frames / 6 hits); Eye of Calm counters | Avoiding launchers; air recovery; counter stances | **Still Wind Elder** (Cyclone Pole) | 55/50/40/45/55/55 |

### 4.10 The Sunken Archive (levels 45–50): divers, Page-Wights, memory-echoes

| ID | Name | Class | Role | Signature behavior | Teaches | Elite variant | AI |
|---|---|---|---|---|---|---|---|
| `enemy.salvagediver` | Salvage Diver | Chain Sword (grapnel) | Bruiser | A diving suit gives 1-hit armor; Chain Snare pulls Rhen into Page-Wight traps | Handling coordinated enemies | **Diver Foreman** (Commander affix) | 55/50/35/45/65/30 |
| `enemy.inkhusk` | Ink Husk | Gauntlets (claws) | Swarm (Husk) | Splits into two small Ink Husks when killed by anything other than an Execution | Executions against splitting enemies | **Blot Husk** (splits into three) | 70/15/5/15/80/15 |
| `enemy.pagewight` | Page-Wight | Arcane (Grimoire) | Ranged (trapper) | Sealing Circles and Ember Verse layered together | Reading traps and projectiles; Perfect Dodge through bolts | **Page-Wight Magister** (Black Folio Lance) | 35/65/40/60/30/35 |
| `enemy.archivesentinel` | Archive Sentinel | Spear | Bruiser (guardian) | Body-blocks for Page-Wights; Planted Vigil | Priority; crossing up guardians | **Archive Warden** (Bulwark affix) | 40/70/30/30/80/15 |
| `enemy.memoryechoarchivist` | Memory-echo Archivist | Arcane (Soul Lantern) | Controller (echo) | "Reads" Rhen: for 10 s it copies the player's most-used move from the Habit Ledger (a floating page shows which) | Varying habits | **Memory-echo Librarian** (copies the top 2 moves) | 45/50/80/60/40/40 |

### 4.11 Bloodmoon Citadel (levels 50–55): the war-capital's soldiers and siege dead

| ID | Name | Class | Role | Signature behavior | Teaches | Elite variant | AI |
|---|---|---|---|---|---|---|---|
| `enemy.siegehusk` | Siege Husk | War Hammer (ram-claws) | Juggernaut (Husk) | Charges in waves at the gates; reassembles once at 30% HP unless finished by an Execution | Why Executions matter | **Siege Behemoth** (reassembles twice) | 55/30/5/10/100/20 |
| `enemy.redmoonknight` | Red Moon Knight | Spear | Grunt (formation) | Fights in lines of 3; the rear rank pokes over the front | Breaking formations with throws and cross-ups | **Red Moon Siege Captain** (Commander affix; calls ballista volleys onto crimson floor zones) | 50/55/25/30/75/20 |
| `enemy.crimsonvowguard` | Crimson Vowguard | Nodachi | Bruiser (berserker) | Hask's blood-sworn. Activates Ember Rage at 50% HP (canon: 8 s, +20%, and its nodachi heavies get 2-hit armor) | Respecting Rage armor; parrying through armor | **Vowguard Champion** (Ember-Wrought + Swift) | 85/20/20/25/95/60 |
| `enemy.oathwardenveteran` | Oathwarden Veteran | Katana | Duelist | Rhen's old teachers: they salute before drawing, parry often, and use Stillwater and Draw-Cuts | Mixing up an opponent who parries | **Oathwarden Master-at-Arms** (Mirrorblade affix) | 55/65/65/60/75/35 |

### 4.12 The Solemn Throne (levels 55–60): the Sunward Guard, the Chancery, the Gate's dead

| ID | Name | Class | Role | Signature behavior | Teaches | Elite variant | AI |
|---|---|---|---|---|---|---|---|
| `enemy.sunwardguard` | Sunward Guard | Spear | Duelist (late-game elite tier) | Disciplined spear work; tip hits Sear Rhen (+12% damage taken); Planted Vigil | Spacing under status pressure | **Sunward Captain** (Oath-Shielded affix) | 55/60/50/50/80/40 |
| `enemy.gildedhusk` | Gilded Husk | Dual Blades (gilded claws) | Assassin (Husk) | Courtiers tithed early. Activates Umbral Shadow when its meter fills (canon echoes) | Blocking and parrying echoes | **Gilded Revenant** (Tithed + Echoing) | 70/35/45/55/60/45 |
| `enemy.tithereader` | Tithe-Reader | Arcane (Soul Lantern) | Support / Controller (late-game elite tier) | Soul Tether and wisps; refills allies' posture; reads from the Ledger to Sear Rhen | Priority; status on the player | **Senior Tithe-Reader** (Commander affix) | 40/60/45/55/45/45 |
| `enemy.undermournremnant` | Undermourn Remnant | Naginata | Skirmisher (Remnant) | Long spinning charges across the Undercroft. At 25% HP it stops and speaks, and an Execution then releases its shadow instead of destroying it | Dodging charges; punishing long recovery | **Remnant Captain** (two charges in a row) | 65/40/35/40/80/50 |

---

## 5. Elite variants

### 5.1 Elite rules

| Property | Regular | Elite |
|---|---|---|
| HP | ×1.0 | ×2.5 |
| Posture | ×1.0 | ×1.6 |
| Damage | ×1.0 | ×1.15 |
| Extra tool | — | The one listed in its archetype row |
| Affixes | — | Oathwarden 1 · Oathsundered 2 · Undermourn 3 (Pilgrim 0) |
| Presentation | — | Nameplate with the elite name, an oath-gold outline, affix icons, an entrance bark |
| Tokens | Normal | Priority for a token, but never above the difficulty's token cap |
| Execution (PvE) | Kills | 40% of max HP; lethal at ≤ 15% HP and staggered |
| Loot | 5% drop | Guaranteed drop (07-Progression §12.2) |

### 5.2 Elite affixes (14)

| Affix | Effect | Counterplay | Icon |
|---|---|---|---|
| **Ember-Wrought** | Activates Ember Rage at 50% HP (canon rules) | Space out for 8 s; parry the armored heavies | Ember flame |
| **Tithed** | Activates Umbral Shadow once at 60% HP (canon echoes) | Block and parry the echoes; stay out of range | Violet eye |
| **Ironclad** | All heavies gain 1-hit armor; +50% posture | Throws and multi-hit strings | Anvil |
| **Swift** | +15% walk speed; dodges ×1.5 as often (follows dodge fatigue rules) | Delayed attacks; throw the dodge recovery | Feather |
| **Vengeful** | On death, a crimson unblockable burst detonates after 60 frames (3 m) | Dodge or leave the radius | Cracked skull |
| **Warded (element)** | Immune to one element; +25% resistance to the others | Switch Infusion or fight physical | Element sigil |
| **Commander** | Allies within 6 m get +10% damage and +10 Aggression | Kill it first | Banner |
| **Mirrorblade** | Automatically parries the player's 3rd consecutive use of the same move ID | Vary your moves | Twin mirrors |
| **Oath-Shielded** | Radiant shield equal to 20% HP absorbs damage; a Guard Crush or Perfect Parry breaks it instantly | Parry; charge a Guard Crush | Gold ring |
| **Venomous** | Its hits add 15 Venom buildup, even when blocked | Clear Mind rune, Venomward, Perfect Parry | Drop |
| **Stormbound** | Every 8 s, calls 3 lightning strikes on crimson circles (40-frame telegraph) near Rhen | Keep moving | Bolt |
| **Echoing** (Unsworn only) | All its hits echo like Umbral Shadow at 30% damage | Parry the echoes (+150 Shadow each, the normal parry gain) | Double silhouette |
| **Relentless** | Attacks 20% more often; −30 Patience | Parry and punish its recovery | Drumbeat |
| **Bulwark** | Guards 60% of the time when not attacking; posture recovers ×2 | Throws, Guard Crush, command grabs | Tower shield |

**Affix rules:** Vengeful never combines with Swift. Oath-Shielded never combines with Ironclad.
Undermourn difficulty draws affixes from the full pool; lower difficulties exclude Echoing and Mirrorblade
before region 7.

---

## 6. Adaptive Tempering (dynamic difficulty)

Adaptive Tempering (`system.tempering`) tunes enemy **behavior** to player performance within fixed
bounds. It is the only dynamic difficulty system in OATHSUNDER, and it follows the "never cheats"
rules in 00-Overview §6.2.

### 6.1 Inputs: Performance Index

Computed at the end of every encounter (0–1):

```
PI = 0.40 × (1 − damageTakenFraction)
   + 0.25 × defenseQuality          // (successful blocks + Perfect Parries + Perfect Dodges) / enemy attacks aimed at Rhen
   + 0.20 × clearSpeedScore         // 1.0 at or under par time, 0 at 3× par
   + 0.15 × (1 − min(retries, 3) / 3)
```

A rolling average over the last 5 encounters drives the Tempering level *T* (−3 … +3).
Rolling PI > 0.75 → *T* +1. Rolling PI < 0.40 → *T* −1. *T* changes by at most one step per
encounter and **only between encounters, never mid-fight** (boss phase transitions included).

### 6.2 Outputs (bounded)

| *T* | State | Aggression | Cunning | Reaction floor | Attack tokens |
|---|---|---|---|---|---|
| −3 | Easing | −15 | −15 | +4 frames | −1 (min 1) |
| −2 | Easing | −10 | −10 | +2 frames | 0 |
| −1 | Easing | −5 | −5 | +1 frame | 0 |
| 0 | Neutral | 0 | 0 | 0 | 0 |
| +1 | Pressing | +5 | +5 | −1 frame | 0 |
| +2 | Pressing | +10 | +10 | −2 frames | 0 |
| +3 | Pressing | +15 | +15 | −2 frames (never below the tier floor − 2) | +1 (max 3) |

**Relief rule:** after 3 failed attempts at the same encounter, *T* drops to at most −2 (−10 Aggression,
+2 reaction frames) and cannot rise above 0 until the encounter is cleared. The game offers, but never
forces, a lower difficulty tier.

**Never changed:** HP, damage, posture, armor, affixes, loot, telegraph timing, Stun Ceiling, and
elemental values.

**Where it runs:** story, side content, Arena contracts and Survival, when enabled (default on for Pilgrim/Oathwarden,
off for Oathsundered/Undermourn). **Never** in PvP, Time Trials, Endless, Boss Rush leaderboards,
Descent leaderboard runs or Raids (raids use fixed tuning so groups share one experience).

---

## 7. Player-pattern analysis: the Habit Ledger

The **Habit Ledger** records the player's repeated choices in `habit.*` counters. It is one shared
system: bosses use it as specified in 05-Bosses §0.3, and regular enemies and elites use it as specified
here. Enemies with Adaptivity > 0 read the Ledger and shift their utility scores toward
counter-strategies, **always with a tell**.

### 7.1 Tracked habits

The IDs and detection rules marked ◆ are shared with 05-Bosses §0.3. The others are added here for all
enemies, and bosses may use them too.

| Habit ID | Detected when | Enemy adaptation | Tell (regular enemies) |
|---|---|---|---|
| `habit.rollspam` ◆ | 3 or more dodges within 2 s, or dodge used for more than 50% of defensive actions over 20 s | Delayed hits timed to dodge recovery; throws on recovery; long-reach follow-ups at the roll's exit | Sable: "They've noticed you roll." The enemy waits with its weapon low |
| `habit.parrymash` ◆ | `Guard` pressed 3 or more times within 20 frames (canon: mashing shrinks the parry window to 2 frames) | Delayed string hits and hesitation feints | The enemy freezes mid-string with a white glint |
| `habit.parryfish` | More than 40% of the last 20 `Guard` presses had no incoming attack within 20 frames | Throws and delayed strings | The enemy freezes mid-string with a white glint |
| `habit.jumpin` ◆ | 3 or more jump-ins started from mid range or further within 30 s | Anti-air readiness (`.dps`, Planted Vigil, Skyseize) | The enemy looks up and raises its weapon |
| `habit.wakeup.attack` ◆ | The player attacks on wake-up in 2 of the last 3 knockdowns | Guarding or parrying on the player's wake-up | The enemy stands in a guard pose over the fallen player |
| `habit.wakeup.roll` ◆ | The player techs the same way in 2 of the last 3 knockdowns | Meaty positioning on that side | The enemy steps toward the predicted spot before Rhen lands |
| `habit.turtle` ◆ | Guard held for more than 60% of a 10 s window | Throws, Guard Crush charges, command grabs | Open-hand gesture (violet telegraphs as usual) |
| `habit.backdash` ◆ | 3 or more backsteps within 10 s | Long-reach punishes and gap-closers | The enemy leans forward, weight on the front foot |
| `habit.dash.approach` ◆ | 3 or more forward dash approaches within 15 s | Pokes at maximum range | The enemy plants its feet and extends its weapon |
| `habit.rollthrough` | 3 or more `universal.rollforward` through attacks within 30 s | Turn-around attacks; retreating strikes | The enemy's feet pivot early |
| `habit.combo.repeat` ◆ | The same 3-hit starter used 4 or more times within 60 s | Pre-emptive guard or parry after the first hit (+15 points parry chance, cap 40%) | Ready pose with a white glint when the starter begins |
| `habit.guard.low` | Crouch blocks are 70% or more of all blocks | More overheads | Weapon raised high (azure glyph as usual) |
| `habit.guard.high` | Standing blocks are 80% or more of all blocks | More lows | Weapon dipped low (amber glyph as usual) |
| `habit.techreliable` | The player techs 70% or more of throws | Fewer throws, more strike/throw frame traps | A shoulder dip before the frame trap |
| `habit.mashblock` | The player attacks on the first possible frame after blockstun in 50% or more of cases | Frame traps; Counter Hit baits | A delayed last hit with an audible inhale |
| `habit.shadow.raw` ◆ | Umbral Shadow activated from neutral as soon as it fills | Spacing out of echo range | Enemies step back together |
| `habit.rage.raw` ◆ | Ember Rage activated from neutral as soon as it fills | Defensive stance; backing off for the duration | Enemies raise guards together |
| `habit.maxrange` | 60% or more of attacks are at over 85% of the move's range | Lunging whiff punishes gain priority | The enemy crouches, ready to spring |
| `habit.stack` ◆ *(raids)* | 3 or more players in the same plane within 4 m for 5 s | Plane-wide attacks (05-Bosses) | Per raid |

### 7.2 Learning and forgetting

- Each observation updates the habit weight: `w ← w + α × (observed − w)`, with α from Adaptivity (§3.1). A habit is **learned** once its weight crosses 0.5.
- **Decay:** a learned weight decays if the player stops the habit for 30 s. This matches the boss rule in 05-Bosses §0.3.
- **Active answers:** an enemy can have **at most two learned answers active** at once (bosses: two per phase).
- **Scope:** for regular enemies and elites, the Ledger resets at the end of each encounter, and a retry starts fresh. Bosses follow 05-Bosses §0.3: learned weights keep 50% on retry ("mercy decay"), and only Aurem and Tamsin carry learning between fights. Raid wipes clear learned habits (05-Bosses §0.6).
- **Warm-up (regular enemies):** no adaptation during the first 20 s of an encounter.
- **First use:** the first time an enemy uses a learned answer, it plays its tell. Bosses play their leitmotif sting instead (05-Bosses §0.3).

### 7.3 Privacy

The Ledger stays on the device. Only anonymized, aggregated habit counts go to telemetry (for
balance). **Reflection Contracts** (Arena, 09-GameModes §7) use another player's habit profile only if that player
opts in, and never with any identifying data.

---

## 8. Fairness caps (all difficulties, all modes)

| Cap | Value |
|---|---|
| Input reading | **None.** The AI never sees buffered, pending or held inputs, only simulation state that has already become visible |
| Reaction floor | Per difficulty (§3.2). Tempering can lower it by at most 2 frames and never below the tier floor − 2 |
| Perfect Parry rate | At most 15% (Oathwarden), 25% (Oathsundered) or 35% (Undermourn) of the player's parryable attacks per encounter, bosses +10 points. A parry must be preceded by an observable reaction delay |
| Counter-strategy frequency | At most two learned answers active (§7.2). A single habit's counter is used on at most **35%** of eligible opportunities, and all adaptation together on at most **50%** of the enemy's choices |
| Tell requirement | Every Ledger-driven choice plays its tell at least **12 frames** before the committing action |
| Enemy combos on Rhen | Stun Ceiling of 180 frames or 6 hits (01-Combat §12.2) |
| Simultaneous attackers | Token caps (§3.3). No unwarned off-screen attacks |
| Chip | Enemy chip cannot KO Rhen on Pilgrim or Oathwarden |
| Accessibility | Players using Guard Assist or game-speed assist are never the subject of adaptation to guard-height habits (`habit.guard.low`, `habit.guard.high`) |
| Stats | Tempering and the Ledger never change HP, damage, posture, armor or loot |

---

## 9. Arena hazards (PvE only)

Hazards are unblockable, so every hazard's danger area gets a **crimson floor border** before it becomes
dangerous. The table lists each hazard's additional tell. **No hazards exist in ranked arenas.** Falling from an edge in PvE deals 15% max HP and returns Rhen to the nearest safe
ground with 60 frames of invulnerability. It never kills instantly.

| Region | Hazard | Effect | Additional tell |
|---|---|---|---|
| Emberfall | Ash patches (from Censer-Bearers) | 2% HP/s + Burn buildup 10/s | Ember smoke rising from the patch |
| Weeping Reeds | Canal edges, fog banks | Edge fall; fog hides enemies beyond 6 m (edge arrows still show) | Wet-stone border |
| Ironroot | Molten vents | Erupt every 6 s: 12% HP, launch | Crimson glow 40 frames before eruption |
| Silkwind | Wind gusts | Push fighters 2 m (both sides) | Petal streams show direction 60 frames early |
| Glass Ossuary | Glass-shard floors, sun mirrors | Standing on shards for more than 1 s adds Bleed buildup 10/s; mirrors reflect projectiles back along their path | Shards glint; mirrors flash once when they reflect |
| Rimewood | Ice floors | +30% slide on knockback; Frost buildup 5/s | Frost sheen border |
| Lanternhold | Canal edges, falling lanterns | Edge fall; lanterns hit for 8% and Burn | Lantern shadow grows 40 frames before impact |
| Verdant Rot | Spore clouds | Venom buildup 15/s | Green haze border |
| Cloudspire | Bridge edges, lightning | Edge fall; lightning strike 15% + Shock | Crimson circle 40 frames before the strike |
| Sunken Archive | Collapsing shelves | 10% HP, knockdown | Dust fall 50 frames before the collapse |
| Bloodmoon Citadel | Ballista zones | 18% HP, unblockable | Crimson strip 50 frames before the volley |
| Solemn Throne | Sun-sigils | Radiant pillars: 12% HP + Sear | Gold circle 40 frames before |

# OATHSUNDER — Game Modes

> **Document:** GDD 02-09 · **Owner:** Lead Systems Design · **Status:** Phase 2 design baseline
> **Cross-refs:** combat rules in 01-Combat, class unlocks in 06-Weapons §3, rewards and currencies in
> 07-Progression and 10-Economy-and-LiveOps, and enemy AI and Tempering in 08-Enemies.

---

## 1. Mode summary

| # | Mode | ID | Unlock | Unit of play (mobile) | Full session | Online / offline | Stats |
|---|---|---|---|---|---|---|---|
| 2 | Story Campaign | `mode.story` | Start (mobile: Emberfall free, regions 2–12 with the campaign purchase) | Scene, 3–5 min | Region arc, 4–5 h | Offline (cloud sync) | PvE |
| 3 | Survival | `mode.survival` | Oskar joins (`mission.ironroot.06`) | 5-wave block, 4–5 min | 30 waves, 25–30 min | Offline (leaderboard online) | PvE |
| 4 | Boss Rush | `mode.bossrush` | 3 Oathlords defeated | One boss, 5–8 min | Full gauntlet, 60–90 min | Offline (leaderboard online) | PvE |
| 5 | Endless | `mode.endless` | Survival wave 30 cleared | 5 waves, 4–5 min | Open-ended | Offline (leaderboard online) | PvE, Normalized or Open |
| 6 | Undermourn Descent | `mode.descent` | Silkwind Groves complete | Floor, 3–5 min | Run, 45–60 min | Offline (seeded weekly online) | Descent baseline |
| 7 | Arena | `mode.arena` | Oskar joins (`mission.ironroot.06`) | One duel, 1–3 min | Ladder, 8–12 min | Offline (Reflections online) | PvE |
| 8 | Training | `mode.training` | Adekan joins (`mission.emberfall.05`) | Lesson, 2–3 min | Open lab | Offline | Normalized, Full Kit |
| 9 | Ranked PvP | `mode.ranked` | Account level 10 + Fundamentals course | Match, 2–3 min | 5–30 min | Online | Normalized, Full Kit |
| 10 | Casual PvP | `mode.casual` | `mission.emberfall.05` | Match, 2–3 min | Open | Online (Local Versus offline on PC/Deck) | Normalized (Kit Rules optional) |
| 11 | Clan Wars | `mode.clanwars` | Account level 20 + clan membership | Banner Duel, 2–3 min | Siege Finale, 20–30 min | Online | Normalized, Full Kit |
| 12 | Raids | `mode.raid` | Per raid (§12) | — | 20–35 min | Online | PvE (item-level cap) |
| 13 | Daily Challenges | `mode.daily` | Prologue complete (`mission.emberfall.03`) | Challenge, 2–4 min | 10 min | Offline (sync online) | Any |
| 14 | Weekly Events | `mode.weekly` | Neve joins (`mission.weepingreeds.02`) or account level 8 | Contract, 5–10 min | 20 min | Online (for event data) | Per event |
| 15 | Time Trials | `mode.timetrial` | Abbot Kessh defeated | Trial, 1–10 min | Open | Offline (leaderboard online) | Normalized or Open |

**Account level** (1–100) is separate from character level. It rises from every mode, including PvP,
and gates only social and competitive features.

---

## 2. Story Campaign (`mode.story`)

**Structure**
- 12 regions in canon order, **10 story missions each (120)**, plus **60 side missions** (at least 5 per region), **37 companion questline steps**, and the hidden duels of the legendary bosses (03-Missions §14). Mission types (Duel, Gauntlet, Boss, Survival Wave, Stealth-Duel, Escort-Duel, Trial, Chase, Cinematic) are defined in 03-Missions §0.1. The Prologue is `mission.emberfall.01`–`03`.
- For session design, a mission is split into 1–4 **Scenes** (3–5 min each, checkpointed). Mission 10 of each region is the **Oathlord duel**: three phases, an intro cinematic, an exclusive theme and a custom arena, followed by the Oath Choice.
- Recurring rival **Tamsin** (`boss.tamsin`) fights Rhen in regions 2–6 (encounters I–V, where she withdraws at a set HP), and the full boss duel is on the Lanternhold rooftops (`mission.lanternhold.08`).
- Between missions: the region camp (Tessen, Mireth, Adekan, Neve and Oskar as each is met).

**Oathstone choices and endings**
After each Oathlord, the player chooses to **Restore** or **Sunder** that region's Oathstone. The
choice is final for the playthrough and is shown on the world map.

| Ending | Condition (canon) | Mode-level rule |
|---|---|---|
| `ending.solemndawn` The Solemn Dawn | Restore ≥ 9 Oathstones | The route is fixed after `mission.solemnthrone.10`. Every route passes through `finale.gate`, the mirror duel with Sable (02-World-and-Narrative §8) |
| `ending.unboundnight` The Unbound Night | Sunder ≥ 9 Oathstones | Same as above |
| `ending.rewovenoath` The Rewoven Oath (true) | Balanced choices + Liss's questline + all 12 Oath Fragments → defeat The First Shadow | "Balanced" means neither side reaches 9 (restored count between 4 and 8). Oath Fragments (`fragment.<virtue>`) are hidden one per region and are never missable |
| `ending.sableascendant` Sable Ascendant (secret) | Yield to Sable in the mirror duel | During `finale.gate`, Sable offers his hand (the Offer), and taking it is the yield. This is checked first and overrides every other route |

**Mission Vows (optional objectives)**
Every story mission offers 3 **Vows**, such as "Finish without being guard-broken", "Land 3 Executions"
or "Clear under par time". Each Vow pays +20% mission Marks. 20 of a region's 30 Vows unlock that region's
outfit (07-Progression §14).

**Difficulty:** Pilgrim / Oathwarden / Oathsundered / Undermourn (00-Overview §6). It can be changed at any checkpoint.

**New Game+ — "The Second Oath"** (02-World-and-Narrative §8.4): unlocks after any ending. It resets choices and
story flags and keeps weapon mastery, cosmetics and the Echo Codex. On the progression side it also keeps
character level, gear, Reliquary and the Tithe Ledger. Enemies scale to the player's level (minimum 60), and Undermourn
difficulty is available from the start. Each NG+ cycle (up to NG+3) adds +1 elite affix and +2 percentage points of
Oathbound chance.

**Rewards:** character XP, Marks, loot, weapon class unlocks, signature Legendary weapons, region
cosmetics, story. **Session:** Scene 3–5 min, mission about 12 min, Oathlord 8–12 min. The main path takes about
40 h and reaches level 60 at about 52 h with side content. **Offline:** yes (cloud save syncs when online).

---

## 3. Survival (`mode.survival`) — Salt Crow Contracts

**Rules**
- A **contract** is 30 waves of enemies drawn from unlocked regions. HP carries between waves, and 15% HP is restored after each wave.
- Every 5th wave, choose 1 of 3 **Free Blade Boons** for the rest of the contract (e.g., *Sellsword's Edge* +10% damage, *Hot Blood* +20% Rage gain, *Headsman's Pay* Executions heal 10%).
- Waves 10 and 20 are **champion waves** (an elite with +1 affix). Wave 30 is an **Oathlord echo** (a random defeated Oathlord at 70% HP, phases 2–3 only).
- **Cash-out:** after waves 10 and 20 the player may bank rewards and leave, or continue. Dying keeps banked rewards and 50% of unbanked rewards.
- **Leaderboard runs** have Tempering off and are split into Normalized and Open boards (07-Progression §15.1).

**Unlock:** Oskar Dray joins (`mission.ironroot.06`), and the Salt Crows' contract board appears in camp. Rimewood maps join the rotation according to region state (02-World-and-Narrative §4).
**Rewards:** Marks (200 + 10 × wave × region tier), Ember Salts, materials, a 5% rune chance per wave,
first-clear cosmetics (wave 10 banner *Salt Crow Standard*, wave 20 trail *Mercenary Ember*, wave 30 outfit
*Free Blade Colors*), weekly leaderboard titles.
**Session:** 4–5 min per 5-wave block; a full contract takes 25–30 min. Suspending is allowed between waves.
**Offline:** yes. Leaderboard submission needs a connection (runs queue while offline).

---

## 4. Boss Rush (`mode.bossrush`) — The Gauntlet of Oaths

**Formats**

| Format | Content | HP between bosses | Length |
|---|---|---|---|
| **Region Trial** | 3 consecutive defeated Oathlords | +30% | 15–25 min |
| **Oathlord Gauntlet** | Every defeated Oathlord in canon order | +30% | 60–90 min |
| **Legend Gauntlet** | Every defeated legendary boss (raid bosses in solo-scaled form) | +30% | 60–80 min |

**Rules:** Tempering is off. Choose any difficulty. Optional **modifiers** each add a score multiplier: *Phase Three*
(bosses start in phase 3, ×1.5), *No Parry* (×1.4), *Bare Steel* (no Ultimate, ×1.3), *Oath of One*
(1 HP, ×3.0). Suspending is allowed between bosses.
**Unlock:** 3 Oathlords defeated (end of Ironroot Forge).
**Rewards:** region set pieces (07-Progression §13), **1 Oath-light Ingot** on each format's weekly
first clear, Boss Rush titles, time leaderboards (Normalized / Open).
**Offline:** yes.

---

## 5. Endless (`mode.endless`) — The Long Night

**Rules**
- Infinite waves, **one life**, 5% HP restored between waves.
- Every 5 waves a **Night Mark** stacks: +10 enemy Aggression, +1 elite affix, a hazard layer, +1 enemy per wave, repeating in that order.
- **Score** = waves cleared × style multiplier (Executions ×1.02 each, Perfect Parries ×1.005 each, max ×3.0).
- Two boards: **Normalized** (Normalized Profile, 07-Progression §15.2, with the PvE meter rules) and **Open** (own build, gear Power shown). Tempering is always off.
- Runs are validated by deterministic replay re-simulation on the server before posting.

**Unlock:** clear Survival wave 30 once.
**Rewards:** Marks per wave (tapering after wave 50), titles at waves 25/50/75/100, weekly board cosmetics
(top 1%: animated banner; top 10%: banner; everyone who clears wave 25: emblem).
**Session:** 4–5 min per 5 waves; open-ended. Suspending is allowed between waves.
**Offline:** yes. Leaderboards need a connection.

---

## 6. Undermourn Descent (`mode.descent`) — roguelike dungeon

Rhen goes down into the Undermourn itself, where every run is different and death sends him back up
the Drowned Stair.

### 6.1 Structure

| Stratum | Floors | Theme | Guardian (floor 6/12/18/24) |
|---|---|---|---|
| I · **The Drowned Stair** | 1–6 | Sinking steps, black water | Echo of a defeated Oathlord (random) |
| II · **The Tithe Halls** | 7–12 | Endless ledgers of names, ash-light | Echo of a defeated Oathlord |
| III · **The Mire of Names** | 13–18 | Shadows half-drowned in mud | Echo of a defeated Oathlord |
| IV · **The Sunless Deep** | 19–24 | Silent dark, only Oath-light | Echo of a defeated Oathlord |
| **The Last Step** | 25 | The foot of the Undermourn Gate | **Echo of the Hundred-Handed Warden** (`boss.hundredhanded`, solo-scaled and one phase shorter than the raid) |

Each floor offers **2–3 doors**, and each door shows its room type:

| Room | Content |
|---|---|
| Skirmish | 1–2 encounters. Reward: Tithe Coins |
| Elite | 1 elite + escorts. Reward: rare relic chance 40% |
| Shrine | Choose 1 of 3 relics |
| Anvil | Upgrade one relic, or +10% weapon Power for the run |
| Lost Stall | Neve's lost lantern-stall: buy relics or healing with Tithe Coins |
| Sable's Bargain | Take a curse for a rare or Oath relic |
| Still Pool | Heal 30% |
| Echo | A mini-boss echo of a legendary boss you have defeated |
| Mystery | A random event (story vignette, gamble, hidden duel) |

### 6.2 Run rules

- Choose any **campaign-unlocked** class or Arcane sub-form. Moves follow the player's mastery unlocks (or Veteran Start).
- **Descent baseline stats:** 2,000 HP, 1,500 posture, Power 300, no gear, no attributes, no skills, no talents. Power comes only from relics, Anvils and the Tithe Ledger (§6.6). PvE combat rules apply (01-Combat §14).
- **Enemy scaling** is fixed against the baseline, independent of the player's level: Stratum I ×1.0, II ×1.2, III ×1.4, IV ×1.6, Floor 25 ×1.8 (HP and damage), and Vows add on top (§6.5).
- Death ends the run. Tithe Coins are lost, and **Shades** (meta currency) are kept.
- **Shades earned:** 10 + 2 × floor per floor cleared, 50 per guardian, 300 for Floor 25, multiplied by curses and Vows.

### 6.3 Relics (30)

| Relic | Rarity | Effect (run only) |
|---|---|---|
| Ferryman's Coin | Common | +15% Tithe Coins |
| Cracked Bell | Common | Perfect Parries release a 2 m shockwave (60% weapon damage) |
| Drowned Rosary | Common | +10% max HP; heal 5% on each floor clear |
| Ember Wick | Common | Start every encounter with 30% Rage meter |
| Shade Thread | Common | Start every encounter with 30% Shadow meter |
| Grave Salt | Common | +20% damage against elites |
| Whetstone of Ash | Common | +12% Light damage |
| Anvil Splinter | Common | +12% Heavy damage |
| Moth Wing | Common | Rolls travel 20% farther |
| Tallow Lantern | Common | Reveals the room types two floors ahead |
| Censer of Embers | Rare | Activating Ember Rage procs Burn on every enemy within 5 m |
| Hollow Mirror | Rare | Perfect Dodges leave a decoy that absorbs 1 hit |
| Chain of Penance | Rare | Executions restore 15% HP |
| Spider-silk Cord | Rare | Pull-in moves stagger for 20 frames longer |
| Glass Heart | Rare | +30% damage dealt, +20% damage taken |
| Oath-Splinter | Rare | +15% Ultimate meter gain; Ultimates heal 10% |
| Twin Petal Charm | Rare | Every 10th hit in a combo adds a bonus hit (100% weapon damage) |
| Frozen Tear | Rare | All hits add 8 Frost buildup |
| Storm Knot | Rare | Every 5th hit arcs 50% of its damage (Storm) to a second enemy |
| Venom Vial | Rare | Heavy attacks add 20 Venom buildup |
| Sunbone Idol | Rare | +25% Radiant damage; Unsworn take +10% damage |
| Warbanner Scrap | Rare | +10% damage per elite killed on this floor (max 30%) |
| Quill of Memory | Rare | Once per stratum, lethal damage rewinds the simulation 3 s instead |
| Heart of the Tithe | Oath | Each enemy killed gives +0.5% damage for the run (max 25%) |
| Sable's Promise | Oath | Shadow echoes deal 60% of the triggering hit instead of 30% |
| Aurem's Signet | Oath | Ultimate meter starts full at each stratum |
| Unreborn Candle | Oath | Once per run, lethal damage revives you at 40% HP |
| Twelvefold Seal | Oath | +3% damage for each curse you carry |
| First Oathblade Shard | Oath | Every class gains the Katana Draw-Cut trait (+50% posture damage after a Perfect Parry) |
| Gatekey Fragment | Oath | Floor 25 opens a hidden chamber: the Warden echo fights at full strength for double rewards |

### 6.4 Curses (13)

Curses come from Sable's Bargain (and Vow 7). Each raises the Shade multiplier.

| Curse | Effect | Shades |
|---|---|---|
| Curse of the Leaden Guard | Max posture −30% | +15% |
| Curse of the Clouded Eye | Low and overhead glyphs are hidden (animation and audio tells only) | +15% |
| Curse of Hunger | No healing from any source | +25% |
| Curse of the Crowd | +1 enemy per encounter | +15% |
| Curse of the Tithe-Taker | Lose 10% of Tithe Coins on each floor | +10% |
| Curse of Ash Lungs | Rage meter gain −50% | +10% |
| Curse of the Severed Shadow | Shadow meter gain −50% | +10% |
| Curse of Iron Skin | All enemies gain Ironclad | +20% |
| Curse of the Swift Dead | All enemies gain Swift | +15% |
| Curse of the Final Breath | All elites gain Vengeful | +15% |
| Curse of the Short Night | Shadow Time lasts half as long | +15% |
| Curse of the Glass Blade | Max HP halved | +30% |
| Curse of Silence | Sable's voice cues are off; Perfect Parries give no Ultimate meter | +10% |

### 6.5 Vows of Descent (difficulty after the first clear)

Vows are cumulative. Each Vow adds +10% Shades.

| Vow | Adds |
|---|---|
| 1 | Enemies +10% HP |
| 2 | Elites +1 affix |
| 3 | Guardians start in their Phase 2 |
| 4 | Still Pools heal 20% instead of 30% · **unlocks Oathbound drops and the Raiment of the Unsworn set** |
| 5 | +1 enemy per Skirmish |
| 6 | Enemies +15 Aggression · **Star rune drops** |
| 7 | One curse is forced at the start of each stratum |
| 8 | Shrines offer 2 relics instead of 3 |
| 9 | Reaction floors drop to Undermourn values |
| 10 | All elites gain Vengeful |
| 11 | Guardians activate Ember Rage at 50% HP |
| 12 | **The Twelvefold Vow:** all of the above, and no Still Pools |

### 6.6 Meta progression — the Tithe Ledger

Shades buy permanent unlocks. **Meta power cap:** permanent stat unlocks stop at +10% max HP and 2 rerolls
per run. Every other unlock adds choice or variety, not raw power.

| Unlock | Shades | Effect |
|---|---|---|
| Lantern of Choices | 200 | Shrines offer 4 relics |
| Second Door | 300 | +1 door choice on floors with 2 doors |
| Ferryman's Discount | 250 | Lost Stall prices −15% |
| Deeper Pockets | 400 | Start with 50 Tithe Coins |
| Kindled Start | 350 | Choose a starting relic from 3 commons |
| Oath Start | 1,200 | Choose a starting relic from 2 rares (requires Kindled Start) |
| Vital Tithe I / II / III | 300 / 600 / 900 | +3 / 6 / 10% max HP (meta cap) |
| Reroll I / II | 500 / 1,000 | 1 / 2 relic rerolls per run (meta cap) |
| Still Pool Blessing | 400 | Still Pools heal +10 percentage points |
| Relic Codex I–IV | 500 each | The relic pool starts at 14 relics, and each Codex adds 4 (all 30 at IV) |
| Guardian's Omen | 600 | Guardian doors show which Oathlord echo waits behind them |
| Sable's Bargain+ | 800 | Sable offers a choice of 2 curses |
| Cosmetic: *Drowned Pilgrim* outfit | 2,000 | Outfit |
| Cosmetic: *Tithe-Ash* trail | 1,500 | Trail |
| Cosmetic: *Ledger of Names* banner | 1,000 | Clan / profile banner |

### 6.7 Rewards to the main game

- Marks and materials on every run (scaled to the deepest floor).
- **First full clear:** unlocks **Arcane — Soul Lantern** if it is not unlocked yet (06-Weapons §3), awards the Legendary *Lamp of the Unreborn* (07-Progression §12.4) and the title *Returned from Below*.
- **Vow 3+ weekly first clear:** 1 Oath-light Ingot.
- **Vow 4+:** Raiment of the Unsworn set pieces and Oathbound chance (07-Progression §12.2).
- **Vow 6 / Vow 12 clears:** outfits *Tithe-Eater* / *Twelvefold Pilgrim*.
- **Seeded Descent** (weekly): the same seed for everyone, with a separate leaderboard and Tempering off.

**Unlock:** Silkwind Groves complete. **Session:** a floor takes 3–5 min, a full run 45–60 min, and suspending is allowed between rooms.
**Offline:** yes. Seeded Descent needs a connection to fetch the seed and submit.

---

## 7. Arena (`mode.arena`) — Free Blade Contracts

Oskar Dray runs arena contracts for the Free Blades.

| Contract type | Rules | Reward |
|---|---|---|
| **Ladders** | 5 AI duels in a row under a rule set (e.g., *Katana Only*, *Executions Only Kill*, *1 vs 2*, *Half Health*, *No Guard*) | Marks, materials, ladder cosmetics |
| **Champion Ladder** (weekly) | 7 duels against named elites with rotating affixes | Oath-light Ingot (first weekly clear), title |
| **Reflection Contracts** | Duel an AI **Reflection** built from an opted-in player's Habit Ledger profile and loadout (normalized stats) | Marks, Reflection titles; the source player gets a notification and Marks when their Reflection wins |
| **Class Proving** | 3 duels using a class below Mastery 5 | Double mastery XP |

**Unlock:** Oskar Dray joins (`mission.ironroot.06`). Story-side *Arena* side missions (ranked pit and canal fights, 03-Missions §14.1) feed the same Arena rank. **Session:** one duel takes 1–3 min, a ladder 8–12 min.
**Offline:** yes, except Reflection Contracts (20 are cached for offline play).

---

## 8. Training (`mode.training`) — Brother Adekan's Dojo

**All 13 classes and all three Arcane sub-forms, with the Full Kit, are available regardless of campaign progress.**

| Feature | Detail |
|---|---|
| **Frame data display** | For every move: startup, active, recovery, on block, on hit, on Counter Hit, JP cost, posture value, cancel list. A live **frame meter** strip for both fighters: startup green, active red, recovery blue, invulnerable gold, armor orange, hitstop grey. An advantage readout after every exchange |
| **Hitbox view** | Hitboxes red, hurtboxes blue, throwboxes violet, pushboxes white, invulnerable hurtboxes gold, armored hurtboxes orange outline. Toggle per fighter |
| **Recording / playback dummy** | 5 slots × 10 s. Playback: single, sequential or random (with weights). Triggers: on wake-up, on block, on hit, on Perfect Parry, on throw tech. Dummy guard: none / all / after first hit / random / stand / crouch. Dummy Perfect Parry: none / every hit / random %. Wake-up tech: none / Recovery Roll (`universal.techroll`) / random. Throw tech %. Counter Hit mode. Rage/Shadow active toggles |
| **Input display** | Numpad notation with button icons, a raw input log with frame counts, motion recognition (shows "236 read" or "missed 3") and the active control scheme |
| **Combo tools** | Damage, scaling %, JP meter, Stun Ceiling bar, bounce-used flags, posture values, meter gain per hit |
| **Simulation tools** | Save/load state (deterministic snapshot), speed 25 / 50 / 100%, frame advance (1 frame per press), infinite meters, position presets (midscreen, left/right corner, custom) |
| **Courses** | **Fundamentals** (12 lessons: movement, guard, lows/overheads, Perfect Parry and the anti-mash rule, Perfect Dodge, throws and tech, launchers, bounces, posture, meters, Executions, punishing). **Class Trials** (10 per moveset, 150 total). **Punish Trainer** and **Parry Trainer** (random delays) |
| **Matchup lab** | Any class or sub-form for the dummy, with recording |

**Unlock:** Brother Adekan joins (`mission.emberfall.05`). The lab tools (frame data, hitboxes, recording, input display) and Fundamentals open at **Dojo tier I**. **Dojo tier II** (`quest.adekan.01`) adds Class Trials and the Punish and Parry Trainers. **Dojo tier III** (`quest.adekan.05`) adds Master Trials (5 hard trials per moveset) and the *Hymn of Returning* cosmetic. Dojo tier II also unlocks automatically at account level 10, so Ranked preparation never waits on later story steps.
**Rewards:** mastery XP for first clears of Class Trials (100 each); title *Adekan's Student* and emote
*Dojo Bow* for completing Fundamentals, which is also a Ranked requirement. **Offline:** yes.

---

## 9. Ranked PvP (`mode.ranked`)

### 9.1 Match rules

- 1v1, **first to 2 rounds**, 99 s rounds (engine `rules.ranked`), 01-Combat PvP rules, **Normalized Profile and Full Kit** (07-Progression §15.2).
- Classic and Simplified are both legal, with no damage penalty (Simplified Specials +2 frames).
- Blind class and sub-form pick. At **Oathlord tier and above**, matches are **first-to-2 games**: the loser of a game may change class, and the winner stays locked.
- **10 ranked arenas**, all walled, hazard-free and visually low-noise (one per region theme, plus the Undermourn Gate and the Solemn Throne dais).
- Rollback netcode (01-Combat §14). Matchmaking prefers < 80 ms ping for 15 s, then < 120 ms.

### 9.2 Rating: Glicko-2

| Parameter | Value |
|---|---|
| Initial rating / RD / volatility | 1500 / 350 / 0.06 |
| System constant τ | 0.5 |
| Update cadence | After every match (each match is treated as its own rating period) |
| RD floor | 45 |
| Inactivity | RD grows each idle week using Glicko-2's pre-period step, reaching 350 again after about 1 year |
| Displayed value | **Oath Rating** = rating rounded to an integer. Hidden during placement |
| Matchmaking window | ±100 rating, widening by 50 every 10 s up to ±400. The same opponent at most 2 times in a row unless both choose Rematch |

### 9.3 Tiers

| Tier | Divisions | Oath Rating |
|---|---|---|
| **Kindled** | III / II / I | < 1050 / 1050–1099 / 1100–1149 |
| **Ironbound** | III / II / I | 1150–1199 / 1200–1249 / 1250–1299 |
| **Steelsworn** | III / II / I | 1300–1349 / 1350–1399 / 1400–1449 |
| **Emberbrand** | III / II / I | 1450–1499 / 1500–1549 / 1550–1599 |
| **Umbralsworn** | III / II / I | 1600–1649 / 1650–1699 / 1700–1749 |
| **Oathkeeper** | III / II / I | 1750–1799 / 1800–1849 / 1850–1899 |
| **Oathlord** | — | 1900+ |
| **The Twelvefold** | — | The top 12 Oathlords per server region, recalculated daily |

- **Placement:** 10 matches. Placement cannot place a player above Oathkeeper I.
- **Demotion shield:** after crossing below a tier boundary, the player keeps the tier for 3 matches while within 25 rating of it.
- **Decay (Oathkeeper and above):** after 14 days without a ranked match, −15 rating per day, down to a floor of 1750. Each match played banks 1 day of protection (max 14). Decay never drops a player's tier below Oathkeeper III.
- **Season:** 10 weeks, aligned with the Season Pass. **Soft reset:** `r' = 1500 + 0.5 × (r − 1500)`, `RD' = max(RD, 150)`, then 3 placement matches.

### 9.4 Rewards, integrity and access

- **Per match:** Glory (win 30, loss 10, +10 for the first win of the day), Pass XP, account XP, mastery XP (06-Weapons §4.2).
- **Season end (highest tier reached):** tier banner and title. Steelsworn+: seasonal weapon-skin color. Umbralsworn+: animated profile frame. Oathlord+: seasonal finisher variant. **The Twelvefold:** a unique title plus the player's name engraved in the season's Reliquary plaque.
- **Integrity:** server-side deterministic re-simulation of reported or anomalous matches; desync detection every 60 frames. **Leavers:** the match counts as a loss, plus an escalating queue lockout (5 min → 15 min → 60 min → 24 h, resets after 20 clean matches). Disconnect grace: 20 s to reconnect.
- **Access:** free for every player (no purchase needed); account level 10 + Fundamentals completed.
- **Session:** match 2–3 min. **Online only.**

---

## 10. Casual PvP (`mode.casual`)

| Sub-mode | Rules |
|---|---|
| **Quick Match** | Unranked, hidden casual MMR, Normalized Profile, ranked arenas |
| **Lobbies** | Up to 8 players, winner stays, spectating, chat (text filter on; voice off for under-18 accounts) |
| **Friend Match** | Direct invite, Rematch |
| **Custom Rules** | Rounds 1–5, timer 60 / 99 / unlimited, any arena including hazard arenas, meter start values, **Kit Rules** (PvE stats on; unranked, no Glory) |
| **Local Versus** | Two local controllers on PC / Steam Deck / macOS, offline |

**Unlock:** `mission.emberfall.05`. **Rewards:** Glory (half of ranked), Pass XP, account XP, mastery XP.
No rating, no character XP. **Session:** match 2–3 min. **Online** (Local Versus offline).

---

## 11. Clan Wars (`mode.clanwars`) — The Banner War

**Clans** are Free Blade companies: 5–30 members. Clans unlock when Oskar joins (`mission.ironroot.06`, clan contracts open) or at account level 15, whichever comes first. Founding a clan costs 5,000 Marks.

### 11.1 Weekly cycle (UTC)

| Day | Phase | Rules |
|---|---|---|
| Monday | **Muster** | Clans are grouped into War Groups of 8 by Clan Rating. The map of Varanth's 12 regions is dealt out: each clan starts holding 1 region, and 4 regions are neutral |
| Tuesday–Saturday | **Battle days** | Each member gets **3 War Banners** per day (banked up to 6). A Banner is spent on a **Banner Duel** against a rival clan's region: live against an online defender if one is available within 20 s, otherwise against that clan's **Garrison** (opted-in members' Reflections, or a Free Blade Sentinel AI at the clan's rating). A win removes 10 control points (a region has 100) |
| Sunday | **Siege Finale** | The top two clans of each War Group fight live **5v5 relays** for the Solemn Throne tile: winner stays, carrying 50% of remaining HP into the next round. Best of 3 relays |

- All Clan War matches use the **Normalized Profile and Full Kit**.
- **Scoring:** regions held at Saturday 23:59 plus the Siege Finale result set the weekly placement and Clan Rating change (Glicko-2 at clan level).

### 11.2 Rewards

- **Banner Seals** (clan currency): 10 per Banner Duel fought, 20 per win, and weekly placement payouts (1st: 1,000 to every member who played at least 3 duels that week, scaling down to 200 for 8th). Seals buy clan cosmetics (banners, clan trail tints, camp decorations).
- Weekly titles for the War Group winner and the Siege champion. Seasonal clan ladder rewards: a banner frame and a clan emblem border.
- Rewards for participation, not only for wins: every member who spends at least 3 Banners in the week earns the base payout.

**Unlock:** account level 20 + clan membership. **Session:** Banner Duel 2–3 min, about 10 min per day; Siege Finale 20–30 min.
**Online only.**

---

## 12. Raids (`mode.raid`)

The three canon raid bosses are fought as 3–4 player co-op raids. **Boss sheets, phase scripts and raid
mechanics are specified in 05-Bosses (§0.6, §15, §18, §23), and discovery points in 03-Missions §14.**
This section sets the mode rules around them.

### 12.1 Mode rules

| Rule | Value |
|---|---|
| Party | 3–4 players, matchmade or premade. Boss HP ×1.0 (3 players) / ×1.3 (4), posture ×1.0 / ×1.2 (05-Bosses §0.6) |
| Stats | PvE builds, capped at the raid's **item-level cap**: *The Choir Beneath* iL 30, *The Engine Wakes* iL 40, *The Gate Below* iL 60. Items above the cap are scaled down to it. Tempering is off |
| Planes | Front and Rear gameplay planes. `Dodge`+`8` / `Dodge`+`2` change plane (01-Combat §5.1) |
| Roles | Soft roles defined per raid (e.g., Anchor / Breaker / Valve-runner for the Ferrous Maw). Any class can fill any role |
| Downed allies | Revive by holding `Execute` for 3 s beside the downed ally. Each raid has its own bleed-out and revive cost, plus a pool of **Oath-light Revives** per phase that refills at each phase transition |
| Guard Break | One shared posture bar. When it breaks, every player in range joins a **Chain Execution** |
| Wipes | The attempt restarts at the last phase reached, with learned habits cleared |
| Difficulties | Normal (Oathwarden rules) · **Oathsworn** (Oathsundered rules, Oathbound drops) · **Sundered** (Undermourn rules) |
| Lockout | **Reward lockout, not play lockout.** The first clear each week per difficulty opens the final chest, and later clears give materials and Marks only |
| Netcode | Rollback with host validation. A reconnect within 60 s restores the player's slot |

### 12.2 The three raids

| Raid | Boss | Opens | Arena | Core co-op mechanic (05-Bosses) | Signature drops |
|---|---|---|---|---|---|
| ***The Choir Beneath*** (`raid.drownedchoir`) | **The Drowned Choir** (`boss.drownedchoir`), Three Voices, One Throat | Discovered in `mission.weepingreeds.09`; opens at **level 25** | The Drowned Chapel | **Harmony marks:** each Voice fixes on one player. In Phase II the three Voices have separate posture bars linked by a 5 s Dissonance window. Breath lasts 10 s underwater, and air pockets refill it | *Anchor Hymn* (Chain Sword), **Chorister's Drowned Vestments** set |
| ***The Engine Wakes*** (`raid.ferrousmaw`) | **The Ferrous Maw** (`boss.ferrousmaw`), Engine of Ironroot | Discovered in `mission.ironroot.10`; opens at **level 35** | The Engine Cradle | **Heat threat:** the Maw targets the highest-threat player, and venting a valve dumps threat. Chain Overload is the burst window for a Chain Execution on the Core | *Crucible Fists* (Gauntlets), **Enginewright's Regalia** set |
| ***The Gate Below*** (`raid.hundredhanded`) | **The Hundred-Handed Warden** (`boss.hundredhanded`), Keeper of the Gate | After **any ending** is reached; level 60 | The Gate Below | **Per-cluster aggro:** a Perfect Parry against a cluster takes its aggro for 8 s. Severing every cluster in a phase staggers the Warden. Phase III has a 3-minute Gate timer | *Hundred Edges* (Dual Blades), **Gatewarden's Hundredfold** set |

**Rewards:** raid sets and signature weapons (07-Progression §12–13), each boss's legendary rune and
cosmetic (05-Bosses), Oath-light Ingots, and raid titles. **Session:** 20–35 min. **Online only.**

---

## 13. Daily Challenges (`mode.daily`)

- **3 challenges per day** (reset at 04:00 local). Unclaimed challenges **bank for up to 3 days (9 total)**, so missing a day costs nothing.
- 1 free reroll per day. Challenges are always about playing, never about spending: e.g., "Land 5 Perfect Parries", "Win an Arena duel with the Staff", "Clear 3 Descent floors", "Execute 2 elites".
- **Rewards per challenge:** Marks (`500 + 20 × level`), 1,000 Pass XP, and 10% of the current level's XP. **All 3:** +2,000 Pass XP.
- **Unlock:** Prologue complete (`mission.emberfall.03`). **Session:** 2–4 min each. **Offline:** playable; claims sync online.

---

## 14. Weekly Events (`mode.weekly`)

One event runs each week (Monday 04:00 local to the next Monday), rotating through these types:

| Event | Host | Rules | Rewards |
|---|---|---|---|
| **Lantern Festival Contracts** | Neve | 5 themed contracts with festival modifiers (lantern hazards, double Executions) | Festival Tokens → event cosmetics store |
| **Mirror Week** | Sable | PvE fights start with full Shadow meter; echo damage +50%; Sable taunts | Tokens, Sable appearances |
| **Oathlord Echoes** | Mireth | A remixed Oathlord fight (e.g., Abbot Kessh in a Rimewood blizzard) with an exclusive modifier | Tokens, echo-themed weapon skins |
| **Class Spotlight** | Tessen | Double mastery XP for the featured class; a featured Arena ladder; a community goal (e.g., 10 million Executions) | Tokens, featured-class trail |
| **Free Blade Tournament** | Oskar | Weekend Casual PvP brackets (normalized, 16/32/64 players, Swiss rounds then top 8) | Glory, tournament banners |

- **Festival Tokens** are earned only by playing. When an event ends, **leftover tokens convert to Marks** at 1 token = 50 Marks. They never expire into nothing.
- Every event cosmetic returns within 12 months (10-Economy §4).
- **Unlock:** Neve joins (`mission.weepingreeds.02`) or account level 8. **Session:** 5–20 min. **Online** (event data); contracts can then be played offline.

---

## 15. Time Trials (`mode.timetrial`)

- Every story **Scene**, every **Oathlord and legendary boss** fight, and weekly **seeded Descent floors** have par times.
- **Medals:** Bronze (par × 1.5), Silver (par × 1.2), Gold (par), **Oath** (the developer's best time).
- **Boards:** **Normalized** (level-60 Normalized Profile, any class) and **Open** (own build, gear Power shown). Tempering is always off. Every record carries a replay that is validated by deterministic server re-simulation before posting, and the top 100 replays are downloadable.
- **Rewards:** first Gold per trial gives 1,000 Marks. Titles at 25 / 100 / 250 Gold medals. At 50 Oath medals, the weapon skin set *Trialmaster's Edge* (one per moveset).
- **Unlock:** Abbot Kessh defeated. A trial appears for each Scene or boss once it is completed. **Session:** 1–10 min per trial.
**Offline:** yes. Leaderboards need a connection.

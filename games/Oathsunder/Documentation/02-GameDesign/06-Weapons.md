# OATHSUNDER — Weapons & Movesets

> **Document:** GDD 02-06 · **Owner:** Lead Combat Design · **Status:** Phase 2 design baseline
> **Canon:** the 13 weapon classes, class IDs and style names are from `00-Canon.md` §6. The Katana
> move list is fixed (engineering-locked). Every other move ID in this document is new and becomes
> stable once shipped. Frame data lives in move JSON; this document sets heights, properties,
> routes and intent. Notation: 01-Combat §1.1.

---

## 1. Conventions

### 1.1 Move ID slots (all classes)

Every class uses the same slot suffixes, so players and data can carry knowledge between classes.

| Suffix | Slot | Input |
|---|---|---|
| `.l1` … `.l6` | Light chain nodes | `L` repeatedly |
| `.l2h` | Launcher branch | `L L H` |
| `.l3h` | Mastery 13 branch (most classes) | `L L L H` |
| `.l3d` | Low branch (Dual Blades only) | `L L L 2L` |
| `.h1` … `.h3` | Heavy chain nodes | `H` repeatedly |
| `.h2l` | Mastery 13 branch (War Hammer only) | `H H L` |
| `.hcharge` / `.hcharged` / `.hchargedfull` | Charged attack: hold stance / early release / full-charge release | `[H]` / `]H[` |
| `.fl` | Forward Light | `6L` |
| `.bh` | Back Heavy (every class's standing overhead) | `4H` |
| `.dl` | Down Light (low) | `2L` |
| `.dh` | Down Heavy (low knockdown) | `2H` |
| `.jl`, `.jl2`, `.jl3` | Air Light chain | `j.L` repeatedly |
| `.jh` | Air Heavy (spike) | `j.H` |
| `.s` | Neutral Special (stance / summon / grab) | `5S` (both schemes) |
| `.qcfs` | Forward Special | Classic `236S` / Simplified `6S` |
| `.dps` | Rising Special (anti-air) | Classic `623S` / Simplified `8S` |
| `.qcbs` | Back Special | Classic `214S` / Simplified `4S` |
| `.jqcfs` | Air forward Special (some classes) | Classic `j.236S` / Simplified `j.6S` |
| `.ultimate` | Ultimate | `Ultimate` (meter full) |
| `.execution` | Execution | `Execute` (conditions met) |

Special follow-ups use the parent slot plus a word (e.g., `katana.s` → `katana.sreprisal`).
**Engine data patterns** (from `moveset.katana.json`): a full-charge release is authored as its own move
`<weapon-short>.hchargedfull` (Katana: *Moonsplitter — Full Moon*), and paired or cinematic continuations
use a `.hit` suffix (`katana.ultimate.hit`, `katana.execution.hit`, `universal.throwforward.hit`, `universal.throwback.hit`). Where a
class's `.hcharged` row below lists a full-charge (level 3) property, that release is authored as
`.hchargedfull` in data.
Simplified-only Special triggers (`6S`, `8S`, `4S`, `j.6S`) have **+2 startup frames**, while `5S` has none
(01-Combat §3.2). Every other property is identical.
Cinematic lengths for Ultimates and Executions are design targets pending animation. Current Katana data
authors *Thousand Oaths* at 150 frames and *Oathbreaker's Mercy* at 30 + 120 frames.

### 1.2 Arcane sub-form IDs

Arcane Weapons (`weapon.arcane`) are one class with three sub-forms. Each sub-form is a distinct moveset
with its own weapon-short for move IDs, and all three share one mastery track (`mastery.arcane`).

| Sub-form | Sub-form ID | Move ID prefix | Example |
|---|---|---|---|
| Grimoire | `weapon.arcane.grimoire` | `grimoire.` | `grimoire.qcfs` Ember Verse |
| Warfan | `weapon.arcane.warfan` | `warfan.` | `warfan.qcfs` Returning Fan |
| Soul Lantern | `weapon.arcane.soullantern` | `soullantern.` | `soullantern.s` Call the Lost |

### 1.3 Rating scale

Range, Speed, Damage and Posture are rated **1–5** relative to the other classes (5 = best in class).
**Startup tier** is the fastest normal's startup band: **S** ≤ 5 f · **A** 6–7 f · **B** 8–9 f · **C** 10–12 f.
Difficulty is rated ★ (easiest) to ★★★★★.

---

## 2. Class overview

| Class | Class ID | Style | Range | Speed | Damage | Posture | Startup | Difficulty | Class trait |
|---|---|---|---|---|---|---|---|---|---|
| Katana | `weapon.katana` | Oathblade Style | 3 | 4 | 3 | 3 | A | ★★ | Draw-Cut |
| Nodachi | `weapon.nodachi` | Longvow Style | 5 | 1 | 5 | 4 | C | ★★★ | Longvow Armor |
| Dual Blades | `weapon.dualblades` | Twin Petal Style | 2 | 5 | 2 | 1 | A | ★★★ | Petal Count |
| Spear | `weapon.spear` | Pierce Line Style | 4 | 3 | 3 | 3 | B | ★★ | Spearpoint |
| Naginata | `weapon.naginata` | Tidewheel Style | 4 | 2 | 3 | 3 | B | ★★★ | Tide Momentum |
| Staff | `weapon.staff` | Still Wind Style | 3 | 4 | 2 | 3 | A | ★★★★ | Featherweight Juggle |
| Scythe | `weapon.scythe` | Harvest Style | 4 | 2 | 4 | 2 | B | ★★★★ | Harvest |
| War Hammer | `weapon.warhammer` | Anvil Style | 3 | 1 | 5 | 5 | C | ★★ | Anvil Weight |
| Gauntlets | `weapon.gauntlets` | Iron Fang Style | 1 | 5 | 3 | 3 | **S** | ★★ | Clinch |
| Daggers | `weapon.daggers` | Ninefold Style | 3 | 4 | 2 | 2 | A | ★★★★★ | Open Wounds |
| Chain Sword | `weapon.chainsword` | Censer Chain Style | 4 | 3 | 3 | 3 | B | ★★★ | Extension |
| Whip Blade | `weapon.whipblade` | Silken Lash Style | 5 | 3 | 2 | 2 | B | ★★★★ | Tip Snap |
| Arcane — Grimoire | `weapon.arcane` | Umbral Arts | 5 | 2 | 3 | 2 | B | ★★★★ | Conjuration Limit |
| Arcane — Warfan | `weapon.arcane` | Umbral Arts | 3 | 4 | 2 | 2 | A | ★★★ | Conjuration Limit |
| Arcane — Soul Lantern | `weapon.arcane` | Umbral Arts | 3 | 2 | 3 | 3 | B | ★★★★★ | Conjuration Limit |

Gauntlets are the only S-tier startup class, which matches their canon identity ("fastest startup").

---

## 3. Acquiring weapon classes

In the campaign, Rhen claims most classes by defeating a fighter who wields one. The Oathlord who
carries a class's signature weapon always unlocks it if it isn't unlocked yet, and that weapon is
also awarded as a Legendary item (07-Progression §12.4, 05-Bosses).

| Class | PvE unlock (campaign) | Typical level |
|---|---|---|
| Katana | Start of game (*Warden's Plain Edge*, `mission.emberfall.01`) | 1 |
| Chain Sword | Clear the Censer Chain Style trial after the duel with Precentor Vashti Coil (`mission.emberfall.06`), **or** defeat Abbot Kessh (`boss.kessh`, `mission.emberfall.10`) | 3–5 |
| Daggers | Survive the first duel with Tamsin (`boss.tamsin`, `mission.weepingreeds.06`) | 8 |
| Naginata | Defeat Mother Ilvane (`boss.ilvane`) | 10 |
| War Hammer | Defeat Gorran Vox (`boss.gorran`) | 15 |
| Dual Blades | Defeat Ysolde & Yrrah (`boss.petals`) | 20 |
| Spear | Defeat Seraph Maal (`boss.maal`) | 25 |
| Gauntlets | Defeat Tharuk Greymane (`boss.tharuk`) | 30 |
| Whip Blade | Defeat Duke Veloran Sae (`boss.veloran`) | 35 |
| Arcane — Soul Lantern | Defeat Hrolm the Skyreader (`mission.rimewood.07`), **or** complete the first full clear of the Undermourn Descent | 28 |
| Arcane — Warfan | Defeat Lady Corvaine (`boss.corvaine`, hidden duel `side.lanternhold.05`). If she is skipped, Warfan unlocks with the Grimoire | 35+ |
| Scythe | Defeat Queen Myrrhen (`boss.myrrhen`) | 40 |
| Staff | Defeat Master Oru (`boss.oru`) | 45 |
| Arcane — Grimoire | Defeat Archivist Quill (`boss.quill`) | 50 |
| Nodachi | Defeat General Hask Varrow (`boss.hask`) | 55 |

**PvP and Training:** all 13 classes and all three Arcane sub-forms are available from the start in
Training, Casual PvP, Ranked PvP and Clan Wars, with the Full Kit (§4.4). Campaign progress never
gates competitive play.

**Sable Ascendant:** canon says `boss.sableascendant` "shifts to any mastered class." A class counts as
**mastered** when its mastery reaches level 10 (the Ultimate unlock).

---

## 4. Mastery system

### 4.1 Shared track (every class; Arcane shares one track across its sub-forms)

| Level | Unlock (every class) | Mastery XP to next level |
|---|---|---|
| 1 | **Base kit:** Light chain (incl. `.l3d` for Dual Blades), Heavy chain, `.fl`, `.bh`, `.dl`, `.dh`, air chain, `.qcfs`, `.execution` | 300 |
| 2 | Launcher branch `.l2h` | 354 |
| 3 | `.qcbs` (and `.jqcfs` where the class has one) | 418 |
| 4 | **Mastery Edge I:** +1% damage (PvE only) | 493 |
| 5 | Charged attack `.hcharge` → `.hcharged` / `.hchargedfull` | 582 |
| 6 | `.dps` | 686 |
| 7 | Trail I (class trail, cosmetic) | 810 |
| 8 | Mastery Edge II: +2% total (PvE only) | 956 |
| 9 | `.s` and its follow-ups | 1,128 |
| 10 | `.ultimate`. The class now counts as **mastered** | 1,331 |
| 11 | Property I (class-specific) | 1,570 |
| 12 | Mastery Edge III: +3% total (PvE only) | 1,853 |
| 13 | Branch `.l3h` (War Hammer `.h2l`; Katana gets a chain route instead) | 2,186 |
| 14 | Trail II + class mastery emote (cosmetic) | 2,580 |
| 15 | Property II (class-specific) | 3,044 |
| 16 | Mastery Edge IV: +4% total (PvE only) | 3,592 |
| 17 | Execution variant (cosmetic finisher variant) | 4,239 |
| 18 | Property III (class-specific) | 5,002 |
| 19 | Master's weapon skin (cosmetic) | 5,902 |
| 20 | **Mastery Edge V: +5% total (PvE cap)**, Mastery Crest title, Oath-light trail | — |

Formula: Mastery XP to go from level *n* to *n*+1 = `round(300 × 1.18^(n−1))`. Reaching level 10 takes 5,727 total
(about 2.3 h of play with the class), and level 20 takes 37,026 total (about 15 h).

### 4.2 Mastery XP sources

| Source | Mastery XP |
|---|---|
| Hit landed with the class (PvE) | 1 |
| Counter Hit / Punish Counter | 3 |
| Perfect Parry / Perfect Dodge while the class is equipped | 5 |
| Execution / Ultimate landed | 25 |
| Encounter cleared | 40 |
| Story mission completed | 150 + 5 × recommended level |
| Oathlord or legendary boss defeated | 600 |
| Training combo trial, first clear (10 per class) | 100 each |
| PvP match (Casual / Ranked / Clan Wars) | 120 win / 80 loss |

The Training dummy grants no mastery XP outside combo trials (anti-idle-farming).
**Tessen's Tutelage (catch-up):** a class earns double mastery XP while its level is more than 5 below
the player's highest class mastery.

### 4.3 Guarantees

- Mastery adds **at most +5% damage, in PvE only**, and nothing else numeric.
- Properties and branches change *how a class plays*, not how hard it hits.
- **Veteran Start** (Settings → Gameplay) unlocks every move and property of every owned class immediately. Cosmetic rewards and Mastery Edge are still earned by playing.

### 4.4 Ranked and PvP: mastery is disabled

In Ranked, Casual PvP and Clan Wars, mastery is **disabled**. Every player fights with the **Full
Kit**: all branches and properties of every class are unlocked, and Mastery Edge is 0%. Cosmetic
mastery rewards (trails, skins, crests, execution variants) are displayed.

---

## 5. Katana — Oathblade Style (`weapon.katana`)

**Fantasy.** The weapon Rhen swore on and the weapon he died with. Oathblade fighters wait
behind a calm guard, answer an opponent's commitment with a draw-cut, and end the exchange before
the opponent realizes it has begun.

| Range | Speed | Damage | Posture | Startup | Difficulty |
|---|---|---|---|---|---|
| 3 | 4 | 3 | 3 | A | ★★ |

**Strengths:** the most complete toolset (a reversal, a projectile, a counter stance, a guard crush),
strong Perfect Parry rewards, easy launcher confirms.
**Weaknesses:** no single best-in-class tool, modest range, and a counter stance that loses to throws,
command grabs and patience (40 frames if nothing is caught).

**Class trait — Draw-Cut (proposal).** For 20 frames after a successful Perfect Parry, `katana.h1` Iron
Draw and `katana.hcharged` / `katana.hchargedfull` Moonsplitter come out as draw-cuts and deal **+50%
posture damage**.

### 5.1 Move list (fixed)

| ID | Name | Input (Classic / Simplified) | Height | Properties | Unlock |
|---|---|---|---|---|---|
| `katana.l1` | Rising Petal | `L` | Mid | Chain starter; fastest Katana normal | M1 |
| `katana.l2` | Falling Petal | `L` (after l1) | Mid | — | M1 |
| `katana.l3` | Crescent Cut | `L` (after l2) | Mid | Special-cancelable | M1 |
| `katana.l4` | Oathseal | `L` (after l3) | Mid | Light finisher, **wall bounce**, knockback | M1 |
| `katana.l2h` | Heaven's Draw | `L L H` | Mid | **Launcher**, jump-cancel on hit | M2 |
| `katana.h1` | Iron Draw | `H` | Mid | **Launches on Counter Hit** (engine `LaunchOnCounter`); Draw-Cut eligible | M1 |
| `katana.h2` | Twin Moon | `H` (after h1) | Mid | Two hits | M1 |
| `katana.h3` | Sundering Arc | `H` (after h2) | **Overhead** | Heavy finisher, **ground bounce** | M1 |
| `katana.hcharge` | Moonsplitter Stance | `[H]` | — | Hold stance: full charge after 30 frames, auto-release at 60 (engine); Dash, Backstep, Rolls or Dodge cancel it (feint) | M5 |
| `katana.hcharged` | Moonsplitter | `]H[` before full charge | Mid | Knockdown; Draw-Cut eligible | M5 |
| `katana.hchargedfull` | Moonsplitter — Full Moon | `]H[` at full charge | Mid | **Guard Crush**, wall bounce, staggers the attacker if parried (`StaggerOnParry`). Engine data ID for canon's "full charge is a guard crush" | M5 |
| `katana.fl` | Stepping Thrust | `6L` | Mid | Advancing thrust; Special-cancelable | M1 |
| `katana.bh` | Mountain Descent | `4H` | **Overhead** | Slow, readable overhead | M1 |
| `katana.dl` | Grass Cutter | `2L` | **Low** | Special-cancelable | M1 |
| `katana.dh` | Reaping Sweep | `2H` | **Low** | Soft knockdown | M1 |
| `katana.jl` | Swallow Cut | `j.L` | Overhead (air) | — | M1 |
| `katana.jl2` | Swallow Return | `j.L` (after jl) | Overhead (air) | — | M1 |
| `katana.jh` | Falling Heaven | `j.H` | Overhead (air) | **Air spike, ground bounce** | M1 |
| `katana.s` | Stillwater | `5S` / `5S` | — | Counter stance, catch window f4–24: catches **any strike (all heights) and projectiles**; loses to throws and command grabs; 40 frames total if nothing is caught | M9 |
| `katana.sreprisal` | Stillwater Reprisal | Automatic on a Stillwater catch | Mid | Counter-cut, invulnerable f1–10, **launcher**; reaches about 2 m (target) | M9 |
| `katana.qcfs` | Crescent Rush | `236S` / `6S` | Mid | Dashing slash, 1.65 m (engine); **knockdown** combo ender | M1 |
| `katana.dps` | Ascending Dragon | `623S` / `8S` | Mid | **Invincible** reversal (strikes, f1–8), anti-air, **launcher**; heavily punishable on block | M6 |
| `katana.qcbs` | Severing Wind | `214S` / `4S` | Mid (projectile) | Wind-slash projectile | M3 |
| `katana.ultimate` | Thousand Oaths | `Ultimate` | — | Invulnerable f1–14, chip can KO; on hit → `katana.ultimate.hit` cinematic | M10 |
| `katana.execution` | Oathbreaker's Mercy | `Execute` | — | Execution grab (invulnerable f1–6) → `katana.execution.hit` paired Execution | M1 |

### 5.2 Combo trees

```
LIGHT TREE
L ──► L ──► L ──► L        katana.l1 Rising Petal → l2 Falling Petal → l3 Crescent Cut → l4 Oathseal (wb)
      │     │
      │     └──► 4H        M13 route: katana.l3 → katana.bh Mountain Descent (overhead ender)
      └──► H               katana.l2h Heaven's Draw (launcher, jc)

HEAVY TREE
H ──► H ──► H              katana.h1 Iron Draw → h2 Twin Moon → h3 Sundering Arc (gb)

CHARGED
[H] ── hold ──► ]H[        katana.hcharge Moonsplitter Stance → katana.hcharged Moonsplitter (early release)
                                                          → katana.hchargedfull Moonsplitter — Full Moon (full charge: guard crush)

AIR
j.L ──► j.L ──► j.H        katana.jl Swallow Cut → jl2 Swallow Return → jh Falling Heaven (spike, gb)

STANCE
5S ── struck (any height) or hit by a projectile on f4–24 ──► katana.sreprisal Stillwater Reprisal (launcher)
```

### 5.3 Ultimate — Thousand Oaths (`katana.ultimate`)

The invulnerable opening draw-cut connects and the world drains to ash-white. Rhen sheathes his
blade. Around the target, a thousand faint oath-sigils light up in the air in Ember gold and
Umbra violet, each one an oath someone once swore. Sable steps out of Rhen's shadow, and the two
cut through the sigils in alternating single-frame flashes: Ember for Rhen, Umbra for Sable. The
last shot shows both sheathing in unison with a single *click*. The sigils shatter, color floods
back, and the target falls. **5.5 s (PvP cut 4.5 s).**

### 5.4 Execution — Oathbreaker's Mercy (`katana.execution`)

Rhen catches the staggered opponent's wrist, turns their blade aside, kneels them and draws across
in one motion. At the moment of the cut, Sable pulls the victim's shadow free of the tithe, and it rises
as ember-motes, unclaimed. **3.0 s.** On the mobile default setting, the cut is shown as an ash burst.

### 5.5 Signature routes

| Level | Route | Notes |
|---|---|---|
| Beginner | `L L L L (wb)` | Full Light chain. Oathseal wall-bounces in the corner and knocks back midscreen |
| Beginner | `2L > 236S` (`2L > 6S`) | Low confirm into the Crescent Rush knockdown |
| Intermediate | `L L H jc j.L j.L j.H (gb), 6L > 214S` | Heaven's Draw launch, air chain, Falling Heaven spike, Stepping Thrust into point-blank Severing Wind |
| Intermediate | Perfect Parry `, H H H (gb)` | Parry recovery is actionable at once. Draw-Cut Iron Draw (+50% posture) into the Heavy chain |
| Expert | `2L, L L L L (wb), SHADOW, H H H (gb), 623S > ULT` | Corner, full Shadow and Ultimate meters. Every Heavy is echoed by Sable, and Ascending Dragon relaunches into Thousand Oaths |
| Expert | `CH H (launch), jc j.L j.L j.H (gb), 623S` | Iron Draw launches on Counter Hit. Whiff-punish or frame-trap confirm into a full air route |

### 5.6 Matchup notes

- **vs Nodachi:** Get inside 1.5 m and stay there. Their armored heavies still get caught by Stillwater, because the stance counters the strike, not the hitstun. Watch for throws, which beat the stance.
- **vs Daggers:** Don't chase Shadowstep. Hold Stillwater at the reappear point, because it catches Backstab (`daggers.sstrike`).
- **vs Whip Blade:** Severing Wind trades evenly with Lash Line. Approach with dash then `Guard`, because their dead zone under 0.8 m is your whole Light chain.
- **vs Gauntlets:** You win at 1.5–2 m with `6L`. Never sit in Stillwater at point-blank range, because Fang Lock (command grab) beats it.

### 5.7 Mastery track (class-specific unlocks)

| Level | Unlock |
|---|---|
| 7 | Trail I: **Ashen Petal** |
| 11 | Property I: `katana.l4` Oathseal becomes Special-cancelable on hit |
| 13 | Route: `L L L 4H`. `katana.l3` Crescent Cut chains into `katana.bh` Mountain Descent, an overhead ender for Light-chain mixups. No new move ID |
| 14 | Trail II: **Moonlit Oath** · Emote: *Sheathing Bow* |
| 15 | Property II: `katana.qcbs` Severing Wind can be held (`[S]`) to delay release by up to 20 frames |
| 17 | Execution variant: *Oathbreaker's Mercy — Ashfall* |
| 18 | Property III: `katana.l2h` Heaven's Draw accepts a delayed input (`L L dl. H`, up to 16 frames) for frame traps |
| 19 | Skin: **Master's Oathblade — Unsheathed Dawn** |
| 20 | Crest: *Oathblade Master* · Trail: **Oath-light Katana** |

---

## 6. Nodachi — Longvow Style (`weapon.nodachi`)

**Fantasy.** A blade as tall as its bearer, carried like a promise that can never be set down.
Longvow fighters decide where the fight happens, which is at the tip of their sword, and make every
step toward them cost blood.

| Range | Speed | Damage | Posture | Startup | Difficulty |
|---|---|---|---|---|---|
| 5 | 1 | 5 | 4 | C | ★★★ |

**Strengths:** longest bladed normals, armored heavies, the highest single-hit damage, and charge attacks that own the screen.
**Weaknesses:** the slowest startup, weakness inside 1 m, a short air combo, and large whiff recovery.

**Class trait — Longvow Armor.** The Heavy chain, `4H`, `2H` and charge levels 2–3 of `nodachi.hcharge`
have **1-hit armor at all times**. During Ember Rage this becomes 2-hit armor (01-Combat §9.2).

### 6.1 Move list

| ID | Name | Input (Classic / Simplified) | Height | Properties | Unlock |
|---|---|---|---|---|---|
| `nodachi.l1` | Longvow Cut | `L` | Mid | Long horizontal cut, 2.6 m | M1 |
| `nodachi.l2` | Returning Vow | `L` (after l1) | Mid | Reverse cut, pushes back | M1 |
| `nodachi.l3` | Stone-Step Cut | `L` (after l2) | Mid | Advances 1 m | M1 |
| `nodachi.l4` | Horizon Sweep | `L` (after l3) | Mid | Light finisher, **wall bounce** | M1 |
| `nodachi.l2h` | Skyward Vow | `L L H` | Mid | **Launcher**, jc, 1-hit armor | M2 |
| `nodachi.l3h` | Kneeling Vow | `L L L H` | Mid | Drops to one knee (ducks highs) and rises with a cleave; 1-hit armor; soft knockdown | M13 |
| `nodachi.h1` | Vowbreaker | `H` | Mid | 1-hit armor | M1 |
| `nodachi.h2` | Longstride Cleave | `H` (after h1) | Mid | Advances 1.5 m, 1-hit armor | M1 |
| `nodachi.h3` | Burden of the Oath | `H` (after h2) | Overhead | **Ground bounce**, 1-hit armor | M1 |
| `nodachi.hcharge` | Longvow Stance | `[H]` | — | 3 levels; walks forward slowly; armor from level 2 | M5 |
| `nodachi.hcharged` | Horizon Cleaver | `]H[` | Mid | 4.5 m reach; level 3 = **Guard Crush** | M5 |
| `nodachi.fl` | Reaching Point | `6L` | Mid | Longest poke in the class (3.8 m); Special-cancelable | M1 |
| `nodachi.bh` | Temple Bell | `4H` | **Overhead** | 1-hit armor, Crumple on CH | M1 |
| `nodachi.dl` | Root Cutter | `2L` | **Low** | Long low poke (2.8 m) | M1 |
| `nodachi.dh` | Stonefield Sweep | `2H` | **Low** | Knockdown, 1-hit armor | M1 |
| `nodachi.jl` | Falling Vow | `j.L` | Overhead (air) | — | M1 |
| `nodachi.jh` | Meteor Oath | `j.H` | Overhead (air) | **Air spike, ground bounce** | M1 |
| `nodachi.s` | Unbending | `5S` / `5S` | — | 40-frame armor stance: absorbs one strike, then triggers Retort | M9 |
| `nodachi.sretort` | Unbending Retort | Automatic after an absorb | Mid | Crumple on hit | M9 |
| `nodachi.qcfs` | Vowstride | `236S` / `6S` | Mid | Dash-slash; hold `S` up to 40 frames for 3 levels; level 3 **wall bounce** | M1 |
| `nodachi.dps` | Rising Oath | `623S` / `8S` | Mid | Anti-air, **launcher**, 1-hit armor, **no invulnerability** | M6 |
| `nodachi.qcbs` | Oath Withdrawn | `214S` / `4S` | **Low** | Backsteps 1.5 m, then a long low cut; whiff-punish tool | M3 |
| `nodachi.ultimate` | Red Horizon | `Ultimate` | — | Cinematic | M10 |
| `nodachi.execution` | Last Promise | `Execute` | — | Paired Execution | M1 |

### 6.2 Combo trees

```
LIGHT TREE
L ──► L ──► L ──► L       nodachi.l1 Longvow Cut → l2 Returning Vow → l3 Stone-Step Cut → l4 Horizon Sweep (wb)
      │     └──► H        nodachi.l3h Kneeling Vow (M13)
      └──► H              nodachi.l2h Skyward Vow (launcher)
HEAVY TREE
H ──► H ──► H             nodachi.h1 Vowbreaker → h2 Longstride Cleave → h3 Burden of the Oath (gb)   [all 1-hit armor]
CHARGED   [H] ──► ]H[     nodachi.hcharge Longvow Stance → nodachi.hcharged Horizon Cleaver (L3 guard crush)
AIR       j.L ──► j.H     nodachi.jl Falling Vow → nodachi.jh Meteor Oath (spike, gb)
STANCE    5S ── absorb ──► nodachi.sretort Unbending Retort
```

### 6.3 Ultimate — Red Horizon (`nodachi.ultimate`)

Rhen plants the nodachi's tip in the ground. Ember light runs down the blade and draws a red line
across the floor to the horizon, and heat haze lifts the target off their feet. Rhen swings once.
The sound drops out. The horizon line itself splits, and sky and earth separate along the cut. A beat
later the shockwave returns and the target falls. Sable stands behind Rhen holding the empty
scabbard like a standard. **5.0 s.**

### 6.4 Execution — Last Promise (`nodachi.execution`)

Rhen pins the opponent with the flat of the blade across both shoulders, says one word
("Kept."), and draws the nodachi's full length through in a single pull. **3.0 s.**

### 6.5 Signature routes

| Level | Route | Notes |
|---|---|---|
| Beginner | `L L L L (wb)` | Horizon Sweep wall-bounces from 3 m out |
| Beginner | `6L` at max range, then `236S` on whiff | Poke-and-punish spacing loop |
| Intermediate | `CH 4H (crumple), H H H (gb), 236S` | Temple Bell crumple into the armored Heavy chain |
| Intermediate | `L L H jc j.L j.H (gb), 623S` | Short air combo, Rising Oath relaunch |
| Expert | Armor through a poke with `H`, `H H (gb), 236S[L3] (wb), RAGE, [H] ]H[ L2, 623S > ULT` | Vowbreaker absorbs the poke. Vowstride L3 wall bounce, Rage activation, then Horizon Cleaver and Rising Oath into Red Horizon |
| Expert | `[H]` held to level 3 in the corner → Guard Crush → `EXE` | Read-based Guard Crush pressure into Last Promise |

### 6.6 Matchup notes

- **vs Gauntlets:** Your hardest matchup. Wall them out with `6L` and `2L`. Armor beats their Lights but never their command grabs (`gauntlets.s`, `gauntlets.qcbs`).
- **vs Whip Blade:** They outrange you by about 1 m. Close the gap with Vowstride level 2 (armor beats a single snap).
- **vs Katana:** Inside 2 m, Stillwater catches your heavies regardless of armor. At tip range its Reprisal (about 2 m reach) whiffs, so stay at the tip, walk in and throw a stance you have read, or let it run out (40 frames).
- **vs Daggers:** Unbending punishes Backstab attempts. Don't swing at Shadowstep feints, because your whiff recovery is their best damage.

### 6.7 Mastery track

| Level | Unlock |
|---|---|
| 7 | Trail I: **Red Thread** |
| 11 | Property I: `nodachi.hcharge` can cancel into `nodachi.qcfs` Vowstride, keeping the current charge level |
| 13 | Branch: `nodachi.l3h` Kneeling Vow |
| 14 | Trail II: **Longvow Ember** · Emote: *Planted Vow* |
| 15 | Property II: `nodachi.qcbs` Oath Withdrawn can Special-cancel into `nodachi.qcfs` Vowstride |
| 17 | Execution variant: *Last Promise — Red Horizon Ash* |
| 18 | Property III: `nodachi.dps` Rising Oath becomes jump-cancelable on hit |
| 19 | Skin: **Master's Longvow — Unbroken Horizon** |
| 20 | Crest: *Longvow Master* · Trail: **Oath-light Nodachi** |

---

## 7. Dual Blades — Twin Petal Style (`weapon.dualblades`)

**Fantasy.** Two blades, one breath. Twin Petal fighters never stop moving and never stop cutting.
They don't break an opponent's guard; they wear it away one petal at a time.

| Range | Speed | Damage | Posture | Startup | Difficulty |
|---|---|---|---|---|---|
| 2 | 5 | 2 | 1 | A | ★★★ |

**Strengths:** the highest hit rate in the game, the longest chains (6-hit Light chain), fast meter
gain, cross-ups, and the best Umbral Shadow synergy (more hits mean more echoes).
**Weaknesses:** the lowest posture damage (poor at forcing Guard Breaks), low damage per hit, short range,
and long strings that are easy to parry.

**Class trait — Petal Count (proposal).** Every 10th hit in a single combo (echoes included) grants **+30
Shadow meter (3%)** (PvE +50). A petal counter next to the combo counter makes the rhythm visible.

### 7.1 Move list

| ID | Name | Input (Classic / Simplified) | Height | Properties | Unlock |
|---|---|---|---|---|---|
| `dualblades.l1` | Petal Flick | `L` | Mid | — | M1 |
| `dualblades.l2` | Petal Turn | `L` (after l1) | Mid | — | M1 |
| `dualblades.l3` | Blossom Spiral | `L` (after l2) | Mid | 2 hits | M1 |
| `dualblades.l4` | Scatter Bloom | `L` (after l3) | Mid | 3 hits | M1 |
| `dualblades.l5` | Petal Gale | `L` (after l4) | Mid | 5 hits, slight pull-in | M1 |
| `dualblades.l6` | Parting Wind | `L` (after l5) | Mid | Light finisher, **wall bounce** | M1 |
| `dualblades.l3d` | Undergrowth | `L L L 2L` | **Low** | Low branch, 2 hits, soft knockdown | M1 |
| `dualblades.l2h` | Rising Pair | `L L H` | Mid | **Launcher**, jc | M2 |
| `dualblades.l3h` | Thornwhirl | `L L L H` | Overhead | Leaping spin over the opponent: **cross-up**, side switch | M13 |
| `dualblades.h1` | Twin Fang | `H` | Mid | — | M1 |
| `dualblades.h2` | Scissor Cut | `H` (after h1) | Mid | 2 hits | M1 |
| `dualblades.h3` | Blooming Cross | `H` (after h2) | Mid | **Ground bounce** | M1 |
| `dualblades.hcharge` | Gale Coil | `[H]` | — | Spins up in place, 3 levels | M5 |
| `dualblades.hcharged` | Tempest of Petals | `]H[` | Mid | Spinning advance: L1 4 hits, L2 8 hits, L3 12 hits + cross-up. Each blocked hit deals 3 posture. No Guard Crush | M5 |
| `dualblades.fl` | Darting Pair | `6L` | Mid | Advances 2.5 m | M1 |
| `dualblades.bh` | Falling Leaf | `4H` | **Overhead** | Flip | M1 |
| `dualblades.dl` | Root Nip | `2L` | **Low** | Fastest low in the class; Special-cancelable | M1 |
| `dualblades.dh` | Thorn Sweep | `2H` | **Low** | Knockdown | M1 |
| `dualblades.jl` | Air Petal | `j.L` | Overhead (air) | — | M1 |
| `dualblades.jl2` | Air Petal Return | `j.L` (after jl) | Overhead (air) | — | M1 |
| `dualblades.jl3` | Petal Storm | `j.L` (after jl2) | Overhead (air) | 4 hits | M1 |
| `dualblades.jh` | Twin Descent | `j.H` | Overhead (air) | **Air spike, ground bounce** | M1 |
| `dualblades.s` | Petal Shroud | `5S` / `5S` | — | Low-profile evasive spin, projectile-invulnerable f4–20 | M9 |
| `dualblades.sstrike` | Shroud Strike | `L` during Petal Shroud | Mid | Exits the spin with a slash; cross-up if the spin passed the opponent | M9 |
| `dualblades.qcfs` | Twin Rush | `236S` / `6S` | Mid | Dash-through, 4 hits; crosses up on hit and block | M1 |
| `dualblades.dps` | Whirling Ascent | `623S` / `8S` | Mid | Rising spin, 5 hits, **launcher**, invulnerable to air attacks f1–10 | M6 |
| `dualblades.qcbs` | Thorn Scatter | `214S` / `4S` | Mid (projectile) | Both blades thrown as a boomerang (hits going and returning); normals are disabled for 40 frames until the blades return | M3 |
| `dualblades.ultimate` | Garden of Blades | `Ultimate` | — | Cinematic | M10 |
| `dualblades.execution` | Two Petals Fall | `Execute` | — | Paired Execution | M1 |

### 7.2 Combo trees

```
LIGHT TREE
L ─► L ─► L ─► L ─► L ─► L     l1 Petal Flick → l2 Petal Turn → l3 Blossom Spiral → l4 Scatter Bloom → l5 Petal Gale → l6 Parting Wind (wb)
     │    ├─► 2L                dualblades.l3d Undergrowth (low branch)
     │    └─► H                 dualblades.l3h Thornwhirl (M13, cross-up)
     └─► H                      dualblades.l2h Rising Pair (launcher)
HEAVY TREE   H ─► H ─► H        h1 Twin Fang → h2 Scissor Cut → h3 Blooming Cross (gb)
CHARGED      [H] ─► ]H[         Gale Coil → Tempest of Petals
AIR          j.L ─► j.L ─► j.L ─► j.H     Air Petal → Air Petal Return → Petal Storm → Twin Descent (spike, gb)
STANCE       5S ~ L             Petal Shroud → Shroud Strike
```

### 7.3 Ultimate — Garden of Blades (`dualblades.ultimate`)

Rhen spins through the target and the world fills with falling silk petals. He and Sable dance a
spiral around the target, and every petal that touches it becomes a cut, sixty in all. Then both
blades cross at the target's throat, the petals freeze in mid-air, and they scatter on the
final breath. **5.5 s.**

### 7.4 Execution — Two Petals Fall (`dualblades.execution`)

Rhen hooks one blade behind the opponent's knee and the other over their shoulder, pulls in opposite
directions and spins behind them. Two petals drift down as the opponent kneels. **2.5 s.**

### 7.5 Signature routes

| Level | Route | Notes |
|---|---|---|
| Beginner | `L L L L L L (wb)` | 13 hits and a Petal Count proc |
| Beginner | `L L L 2L` | Mid-mid-mid-low string that ends in a knockdown |
| Intermediate | `2L, L L H jc j.L j.L j.L j.H (gb), 236S` | Air Petal chain, Twin Descent spike, then Twin Rush to cross up and reposition |
| Intermediate | `5S ~ L` through a projectile | Petal Shroud under the projectile, Shroud Strike on arrival |
| Expert | `SHADOW, L L L L L L (wb), H H H (gb), 623S jc j.L j.L j.L` | The echo storm: 40+ hits and several Petal Count procs |
| Expert | `L L L H (cross-up), L L L 2L` | Thornwhirl side switch into a low (M13) |

### 7.6 Matchup notes

- **vs War Hammer:** Favorable. Their 1-hit armor absorbs only your first hit, and your multi-hit strings strip it. Do not trade with Iron Resolve (2-hit absorb).
- **vs Katana:** Stillwater catches any strike, so don't auto-pilot your strings. Stop short and throw, or delay `l4`/`l5` (M18) past the catch window.
- **vs Spear:** Get past the tip with Twin Rush, then stay inside the shaft's reach.
- **vs Staff:** A juggle mirror. Whoever wins the first air exchange usually wins the round, and Whirling Ascent beats their jump-ins.

### 7.7 Mastery track

| Level | Unlock |
|---|---|
| 7 | Trail I: **Falling Silk** |
| 11 | Property I: `dualblades.l6` Parting Wind becomes Special-cancelable on hit |
| 13 | Branch: `dualblades.l3h` Thornwhirl |
| 14 | Trail II: **Twin Bloom** · Emote: *Petal Toss* |
| 15 | Property II: `dualblades.qcbs` Thorn Scatter can be recalled early (press `S` again) |
| 17 | Execution variant: *Two Petals Fall — Silkwind* |
| 18 | Property III: `dualblades.l4` and `dualblades.l5` accept delayed input (up to 14 frames) for frame traps against parries |
| 19 | Skin: **Master's Twin Petals — Thorn-Kissed** |
| 20 | Crest: *Twin Petal Master* · Trail: **Oath-light Twin Blades** |

---

## 8. Spear — Pierce Line Style (`weapon.spear`)

**Fantasy.** The line is the law. A Pierce Line fighter draws an invisible boundary at the
spear's tip and punishes anyone who crosses it, then vaults over the wall they built and strikes from
above.

| Range | Speed | Damage | Posture | Startup | Difficulty |
|---|---|---|---|---|---|
| 4 | 3 | 3 | 3 | B | ★★ |

**Strengths:** the best mid-range pokes, sweet-spot damage, pole-vault mobility for corner escapes,
and a strong anti-air.
**Weaknesses:** weak at point-blank range (inside the tip), linear thrusts that lose to `Dodge`, and modest combo
damage without tip hits.

**Class trait — Spearpoint.** Hits with the last 0.5 m of the spear (a tip glint shows the sweet
spot) deal **+15% damage and +4 frames of hitstun**.

### 8.1 Move list

| ID | Name | Input (Classic / Simplified) | Height | Properties | Unlock |
|---|---|---|---|---|---|
| `spear.l1` | Straight Line | `L` | Mid | Poke, 2.4 m | M1 |
| `spear.l2` | Double Line | `L` (after l1) | Mid | 2 thrusts | M1 |
| `spear.l3` | Shaft Sweep | `L` (after l2) | **Low** | Low in the chain | M1 |
| `spear.l4` | Sunline Thrust | `L` (after l3) | Mid | Light finisher, **wall bounce** | M1 |
| `spear.l2h` | Hoisting Line | `L L H` | Mid | **Launcher**, jc | M2 |
| `spear.l3h` | Lineburst | `L L L H` | Mid | 5-thrust flurry that carries toward the wall; every thrust can tip | M13 |
| `spear.h1` | Rooted Thrust | `H` | Mid | 3.2 m thrust | M1 |
| `spear.h2` | Turning Butt | `H` (after h1) | Mid | Close-range butt strike (covers point-blank) | M1 |
| `spear.h3` | Pinning Line | `H` (after h2) | Overhead | **Ground bounce** (pins, then releases) | M1 |
| `spear.hcharge` | Drawn Line | `[H]` | — | 3 levels | M5 |
| `spear.hcharged` | Meridian Lance | `]H[` | Mid | Destroys projectiles; level 3 = **Guard Crush** | M5 |
| `spear.fl` | Reaching Line | `6L` | Mid | 3.6 m poke | M1 |
| `spear.bh` | Crown Strike | `4H` | **Overhead** | — | M1 |
| `spear.dl` | Ankle Line | `2L` | **Low** | Long low poke; Special-cancelable | M1 |
| `spear.dh` | Sweeping Shaft | `2H` | **Low** | Knockdown | M1 |
| `spear.jl` | Sky Line | `j.L` | Overhead (air) | — | M1 |
| `spear.jl2` | Sky Line Return | `j.L` (after jl) | Overhead (air) | — | M1 |
| `spear.jh` | Descending Line | `j.H` | Overhead (air) | **Air spike, ground bounce** | M1 |
| `spear.s` | Planted Vigil | `5S` / `5S` | — | Plants the spear. A 30-frame counter stance against airborne attacks and high/mid strikes | M9 |
| `spear.svigil` | Vigil Sweep | Automatic on a Vigil catch | Mid | **Launcher** | M9 |
| `spear.qcfs` | Lunging Line | `236S` / `6S` | Mid | 4 m dash-thrust; tip sweet spot | M1 |
| `spear.dps` | Rising Line | `623S` / `8S` | Mid | Anti-air thrust, **launcher**, upper-body invulnerable f1–8 | M6 |
| `spear.qcbs` | Pole Vault | `214S` / `4S` | — | Vaults over the opponent to the other side (airborne f6–30, side switch) | M3 |
| `spear.vaultdrop` | Vaulting Drop | `L` or `H` during Pole Vault | Overhead | Diving strike, **ground bounce** | M3 |
| `spear.ultimate` | Thousand-League Line | `Ultimate` | — | Cinematic | M10 |
| `spear.execution` | Last Line | `Execute` | — | Paired Execution | M1 |

### 8.2 Combo trees

```
LIGHT TREE
L ─► L ─► L ─► L        l1 Straight Line → l2 Double Line → l3 Shaft Sweep (LOW) → l4 Sunline Thrust (wb)
     │    └─► H         spear.l3h Lineburst (M13)
     └─► H              spear.l2h Hoisting Line (launcher)
HEAVY TREE  H ─► H ─► H       h1 Rooted Thrust → h2 Turning Butt → h3 Pinning Line (gb)
CHARGED     [H] ─► ]H[        Drawn Line → Meridian Lance (L3 guard crush, destroys projectiles)
AIR         j.L ─► j.L ─► j.H Sky Line → Sky Line Return → Descending Line (spike, gb)
SPECIAL     214S ~ L/H        Pole Vault → Vaulting Drop (overhead, gb)
STANCE      5S ─ catch ─► spear.svigil Vigil Sweep (launcher)
```

### 8.3 Ultimate — Thousand-League Line (`spear.ultimate`)

Rhen vaults straight up, and the camera follows him through the clouds until the moon fills the
frame. He hurls the spear, which becomes a streak of Oath-light, pierces the target and pins it
to a sun-sigil burning in the ground. Sable drops out of the sky onto the spear's butt and drives
it deeper. Rhen lands and wrenches the spear free as the sigil fades. **5.0 s.**

### 8.4 Execution — Last Line (`spear.execution`)

Rhen spins the spear, sweeps the opponent to their knees, sets the butt against the floor with the
point beneath their chin, and draws a single line upward. **2.5 s.**

### 8.5 Signature routes

| Level | Route | Notes |
|---|---|---|
| Beginner | `6L` at tip range, repeat | The Pierce Line fence. Learn the tip glint |
| Beginner | `L L L L (wb)` | Shaft Sweep in the middle of the chain teaches the low |
| Intermediate | `2L > 236S` | Ankle Line into Lunging Line with the tip bonus |
| Intermediate | `L L H jc j.L j.L j.H (gb), 623S` | Launcher route with a Rising Line relaunch |
| Expert | `214S ~ L (gb), L L L L (wb), SHADOW, 6L > 236S, 623S > ULT` | Corner escape and reversal of position: Vaulting Drop crosses over, the full chain wall-bounces, and echoes follow |
| Expert | `5S` catches a jump-in, `, H H H (floor slam)` | Vigil Sweep launch into the Heavy chain. The bounce is already spent, so Pinning Line floor-slams |

### 8.6 Matchup notes

- **vs Whip Blade:** You are outranged. Close in with Pole Vault and use Planted Vigil against their jumping Air Flick.
- **vs Gauntlets:** Keep them at the tip with `6L` and `2L`, and punish jumps with Rising Line. Turning Butt (`h2`) is your answer at point-blank range.
- **vs Nodachi:** Their armor ignores single pokes. Tip their whiff recovery and throw them when they sit in Unbending.
- **vs Daggers:** Shadowstep lands inside your tip range. `h2` Turning Butt and Pole Vault escape.

### 8.7 Mastery track

| Level | Unlock |
|---|---|
| 7 | Trail I: **Sunline** |
| 11 | Property I: a tip hit with `spear.qcfs` Lunging Line causes a **wall bounce** |
| 13 | Branch: `spear.l3h` Lineburst |
| 14 | Trail II: **Glass Meridian** · Emote: *Spear Twirl* |
| 15 | Property II: holding `4` during `spear.qcbs` Pole Vault lands on the original side (vault feint) |
| 17 | Execution variant: *Last Line — Glass Sun* |
| 18 | Property III: `spear.fl` Reaching Line becomes Special-cancelable on a tip hit |
| 19 | Skin: **Master's Pierce Line — Bone-White Shaft** |
| 20 | Crest: *Pierce Line Master* · Trail: **Oath-light Spear** |

---

## 9. Naginata — Tidewheel Style (`weapon.naginata`)

**Fantasy.** Water never stops, it only changes direction. Tidewheel fighters turn every swing into
the next, building a spinning momentum that sweeps legs, climbs over guards and drowns the
opponent in arcs.

| Range | Speed | Damage | Posture | Startup | Difficulty |
|---|---|---|---|---|---|
| 4 | 2 | 3 | 3 | B | ★★★ |

**Strengths:** the best low game, 360° arcs that hit cross-ups, and momentum that rewards sustained offense.
**Weaknesses:** a slow start, momentum that is lost when interrupted, and a spin rhythm that invites parries.

**Class trait — Tide Momentum.** Each **spinning** move (marked *spin*) that connects or is blocked
adds 1 **Tide** (max 3). Each stack gives spinning moves +5% damage and +0.3 m range. At 3 Tide,
`naginata.hcharged` Riptide Crash is a Guard Crush at any charge level (and consumes the stacks). Tide
decays by 1 every 90 frames without a spinning move. Being hit removes all Tide.

### 9.1 Move list

| ID | Name | Input (Classic / Simplified) | Height | Properties | Unlock |
|---|---|---|---|---|---|
| `naginata.l1` | Shallow Arc | `L` | Mid | — | M1 |
| `naginata.l2` | Returning Arc | `L` (after l1) | Mid | *spin* | M1 |
| `naginata.l3` | Undertow | `L` (after l2) | **Low** | *spin* | M1 |
| `naginata.l4` | Full Tide | `L` (after l3) | Mid | *spin*, Light finisher, **wall bounce** | M1 |
| `naginata.l2h` | Rising Swell | `L L H` | Mid | **Launcher**, jc | M2 |
| `naginata.l3h` | Surging Crest | `L L L H` | **Overhead** | After the low Undertow: a high/low mixup inside the string | M13 |
| `naginata.h1` | Wheel Cut | `H` | Mid | *spin* | M1 |
| `naginata.h2` | Second Wheel | `H` (after h1) | Mid | *spin*, 2 hits | M1 |
| `naginata.h3` | Maelstrom Wheel | `H` (after h2) | Mid | *spin*, **ground bounce** | M1 |
| `naginata.hcharge` | Gathering Tide | `[H]` | — | The blade spins overhead and hits airborne opponents; 3 levels | M5 |
| `naginata.hcharged` | Riptide Crash | `]H[` | Mid | Level 3 or 3 Tide = **Guard Crush**; +10% damage per Tide consumed | M5 |
| `naginata.fl` | Current Thrust | `6L` | Mid | 3 m thrust | M1 |
| `naginata.bh` | Breaking Wave | `4H` | **Overhead** | — | M1 |
| `naginata.dl` | Reed Cutter | `2L` | **Low** | Special-cancelable | M1 |
| `naginata.dh` | Tidewheel Sweep | `2H` | **Low** | *spin*, knockdown, 360° (hits behind) | M1 |
| `naginata.jl` | Spray Cut | `j.L` | Overhead (air) | — | M1 |
| `naginata.jl2` | Spray Return | `j.L` (after jl) | Overhead (air) | *spin* | M1 |
| `naginata.jh` | Crashing Wave | `j.H` | Overhead (air) | **Air spike, ground bounce** | M1 |
| `naginata.s` | Whirlpool Guard | `5S` / `5S` | — | 40-frame spinning stance: destroys projectiles, counters high/mid strikes | M9 |
| `naginata.sreturn` | Whirlpool Return | Automatic on a counter | **Low** | Knockdown, *spin* | M9 |
| `naginata.qcfs` | Flood Advance | `236S` / `6S` | Mid → **Low** | *spin*, 4 hits, the last hit is low; +1 Tide | M1 |
| `naginata.dps` | Waterspout | `623S` / `8S` | Mid | *spin*, **launcher**, invulnerable to air attacks f1–9 | M6 |
| `naginata.qcbs` | Ebb | `214S` / `4S` | **Low** | *spin*, retreats 2 m with a trailing sweep; +1 Tide | M3 |
| `naginata.ultimate` | Rivers Return | `Ultimate` | — | Cinematic | M10 |
| `naginata.execution` | Undertow Rite | `Execute` | — | Paired Execution | M1 |

### 9.2 Combo trees

```
LIGHT TREE
L ─► L ─► L ─► L        l1 Shallow Arc → l2 Returning Arc → l3 Undertow (LOW) → l4 Full Tide (wb)
     │    └─► H         naginata.l3h Surging Crest (OVERHEAD, M13)
     └─► H              naginata.l2h Rising Swell (launcher)
HEAVY TREE  H ─► H ─► H       Wheel Cut → Second Wheel → Maelstrom Wheel (gb)   [all spin: +Tide]
CHARGED     [H] ─► ]H[        Gathering Tide → Riptide Crash (L3 or 3 Tide: guard crush)
AIR         j.L ─► j.L ─► j.H Spray Cut → Spray Return → Crashing Wave (spike, gb)
STANCE      5S ─ counter ─► naginata.sreturn Whirlpool Return (low knockdown)
```

### 9.3 Ultimate — Rivers Return (`naginata.ultimate`)

Spectral water rises through the arena floor. Rhen whirls the naginata into a tidewheel that pulls
the water into a spiral and drags the target under. The camera follows them below the surface into
silence, where the shadows of the drowned drift past. Then the wheel reverses, the flood erupts
upward and throws the target into the sky, and Rhen's final arc cuts through the falling rain.
**5.5 s.**

### 9.4 Execution — Undertow Rite (`naginata.execution`)

Rhen hooks the blade behind the opponent's ankle and sweeps them onto their back. He turns once
around them in the rhythm of the tide and brings the blade down. **2.5 s.**

### 9.5 Signature routes

| Level | Route | Notes |
|---|---|---|
| Beginner | `L L L L (wb)` | Mid, mid, low, mid. Teaches the in-string low |
| Beginner | `2L, 2H` | Low poke into Tidewheel Sweep knockdown |
| Intermediate | `L L H jc j.L j.L j.H (gb), 236S` | Launcher route that ends by building Tide |
| Intermediate | `214S, 236S` | Ebb then Flood Advance: +2 Tide, crossing half the arena |
| Expert | `236S, L L L L (wb), H H H (gb), 623S > ULT` at 3 Tide | All spins at +15% damage and +0.9 m range |
| Expert | 3 Tide, `[H] ]H[` (instant Guard Crush) → `EXE` | Tide cash-out into Undertow Rite |

### 9.6 Matchup notes

- **vs Katana:** Stillwater catches every height, so condition it: stop a spin short and throw, or bait the stance and punish its 40-frame whiff with Flood Advance.
- **vs Dual Blades:** Tidewheel Sweep and your spins hit their cross-ups from both sides.
- **vs Gauntlets:** They live inside your spin radius. Ebb out and restart the wheel at range.
- **vs Grimoire:** Whirlpool Guard erases their Ember Verse, and Flood Advance goes under Sealing Circle glyph placement.

### 9.7 Mastery track

| Level | Unlock |
|---|---|
| 7 | Trail I: **Saltmarsh Mist** |
| 11 | Property I: Tide decays after 150 frames instead of 90 |
| 13 | Branch: `naginata.l3h` Surging Crest |
| 14 | Trail II: **Tideglass** · Emote: *Wheel Salute* |
| 15 | Property II: `naginata.qcbs` Ebb can Special-cancel into `naginata.qcfs` Flood Advance |
| 17 | Execution variant: *Undertow Rite — Fog Tide* |
| 18 | Property III: `naginata.s` Whirlpool Guard **reflects** projectiles instead of destroying them |
| 19 | Skin: **Master's Tidewheel — Reedbound Glaive** |
| 20 | Crest: *Tidewheel Master* · Trail: **Oath-light Naginata** |

---

## 10. Staff — Still Wind Style (`weapon.staff`)

**Fantasy.** Stillness is not the absence of motion. It is motion placed perfectly. Still Wind
fighters lift opponents off the ground and keep them there, spinning the staff like a storm around a
calm eye.

| Range | Speed | Damage | Posture | Startup | Difficulty |
|---|---|---|---|---|---|
| 3 | 4 | 2 | 3 | A | ★★★★ |

**Strengths:** the best air combos, a strong counter stance, pole-spins that erase projectiles, and a great anti-air.
**Weaknesses:** low damage per hit, a dependence on launches (weak grounded confirms), and mediocre lows.

**Class trait — Featherweight Juggle (proposal).** Combos started by a Staff hit get a **10 JP** juggle
budget instead of the engine's 8 (01-Combat §7.4), which makes the Staff's air routes the longest in the game.

### 10.1 Move list

| ID | Name | Input (Classic / Simplified) | Height | Properties | Unlock |
|---|---|---|---|---|---|
| `staff.l1` | Wind Tap | `L` | Mid | — | M1 |
| `staff.l2` | Wind Turn | `L` (after l1) | Mid | — | M1 |
| `staff.l3` | Spinning Reed | `L` (after l2) | Mid | 3 hits, destroys projectiles | M1 |
| `staff.l4` | Still Point | `L` (after l3) | Mid | Light finisher, **wall bounce** | M1 |
| `staff.l2h` | Lifting Breeze | `L L H` | Mid | **Launcher**, jc | M2 |
| `staff.l3h` | Pinwheel Rise | `L L L H` | Mid | Vacuum spin that pulls 1 m, then a high **launcher**, jc | M13 |
| `staff.h1` | Heavy Branch | `H` | Mid | — | M1 |
| `staff.h2` | Twin Branch | `H` (after h1) | Mid | 2 hits | M1 |
| `staff.h3` | Falling Bough | `H` (after h2) | Overhead | **Ground bounce** | M1 |
| `staff.hcharge` | Gathering Gale | `[H]` | — | 3 levels | M5 |
| `staff.hcharged` | Cyclone Pole | `]H[` | Mid | 2 m vacuum pull, then launch; level 3 = **Guard Crush** | M5 |
| `staff.fl` | Reaching Branch | `6L` | Mid | 2.8 m poke; Special-cancelable | M1 |
| `staff.bh` | Temple Strike | `4H` | **Overhead** | — | M1 |
| `staff.dl` | Shin Tap | `2L` | **Low** | — | M1 |
| `staff.dh` | Root Sweep | `2H` | **Low** | Knockdown | M1 |
| `staff.jl` | Air Branch | `j.L` | Overhead (air) | — | M1 |
| `staff.jl2` | Air Branch Return | `j.L` (after jl) | Overhead (air) | — | M1 |
| `staff.jl3` | Pinwheel | `j.L` (after jl2) | Overhead (air) | 3 hits | M1 |
| `staff.jh` | Cloud Splitter | `j.H` | Overhead (air) | **Air spike, ground bounce** | M1 |
| `staff.s` | Eye of Calm | `5S` / `5S` | — | 30-frame counter stance: catches high, mid and overhead strikes | M9 |
| `staff.sreversal` | Calm Reversal | Automatic on a catch | Mid | **Launcher**, jc | M9 |
| `staff.qcfs` | Wheeling Branch | `236S` / `6S` | Mid | Pole-spin advance, 4 hits; destroys projectiles | M1 |
| `staff.dps` | Skyward Reed | `623S` / `8S` | Mid | Vaults up on the staff; **launcher**; invulnerable to air attacks f1–10 | M6 |
| `staff.qcbs` | Gust Palm | `214S` / `4S` | Mid (projectile) | 2.5 m wind push; knockback; destroys projectiles | M3 |
| `staff.jqcfs` | Falling Gyre | `j.236S` / `j.6S` | Overhead (air) | Spinning dive; **ground bounce** on airborne targets, soft knockdown on grounded ones | M3 |
| `staff.ultimate` | Heart of the Gale | `Ultimate` | — | Cinematic | M10 |
| `staff.execution` | Quiet Branch | `Execute` | — | Paired Execution | M1 |

### 10.2 Combo trees

```
LIGHT TREE
L ─► L ─► L ─► L        l1 Wind Tap → l2 Wind Turn → l3 Spinning Reed → l4 Still Point (wb)
     │    └─► H         staff.l3h Pinwheel Rise (M13, vacuum launcher)
     └─► H              staff.l2h Lifting Breeze (launcher)
HEAVY TREE  H ─► H ─► H       Heavy Branch → Twin Branch → Falling Bough (gb)
CHARGED     [H] ─► ]H[        Gathering Gale → Cyclone Pole (L3 guard crush)
AIR         j.L ─► j.L ─► j.L ─► j.H     Air Branch → Air Branch Return → Pinwheel → Cloud Splitter (spike, gb)
            j.L ─► j.L ─► j.L > j.236S   … Pinwheel → Falling Gyre (M11 cancel)
STANCE      5S ─ catch ─► staff.sreversal Calm Reversal (launcher)
```

### 10.3 Ultimate — Heart of the Gale (`staff.ultimate`)

Rhen plants the staff and spins around it until a tornado forms and lifts the target into the
funnel. The camera rises with them through cloud and lightning, and Rhen and Sable ride the wind,
striking from opposite sides in a twelve-beat rhythm. Then the eye of the storm opens into sudden
stillness under clear stars. Rhen delivers one vertical strike that drives the target back down to
the earth. **6.0 s.**

### 10.4 Execution — Quiet Branch (`staff.execution`)

Rhen sweeps the staff under the opponent's arms and locks them in place. He turns them to face
the sky and ends it with a single strike to the sternum. The wind stops. **2.5 s.**

### 10.5 Signature routes

| Level | Route | Notes |
|---|---|---|
| Beginner | `L L L L (wb)` | Still Point wall bounce |
| Beginner | `L L H jc j.L j.L j.H (gb)` | The core launcher route every Staff player learns first |
| Intermediate | `L L H jc j.L j.L j.L > j.236S (gb), 623S` | Pinwheel into Falling Gyre (M11), Skyward Reed relaunch |
| Intermediate | `5S` catch → `jc j.L j.L j.L j.H (gb)` | Calm Reversal into a full air chain |
| Expert | `2L, L L H jc j.L j.L j.L > j.236S (gb), 623S jc j.L j.H` | Uses the full 10 JP Featherweight budget. The second air chain is stale, and Cloud Splitter floor-slams because the bounce is spent |
| Expert | `L L L H jc j.L j.L j.L j.H (gb), 236S > ULT` | Pinwheel Rise (M13) into a full chain and Heart of the Gale |

### 10.6 Matchup notes

- **vs Grimoire:** Wheeling Branch and Gust Palm erase their projectiles. Approach behind a spin.
- **vs Nodachi:** Eye of Calm catches their armored heavies, because counters trigger on the strike and ignore armor.
- **vs Gauntlets:** Eye of Calm loses to Fang Lock. Use `2L` and Gust Palm to keep them out instead.
- **vs Whip Blade:** Skyward Reed goes over Lash Line, and Falling Gyre punishes their whiff recovery from the air.

### 10.7 Mastery track

| Level | Unlock |
|---|---|
| 7 | Trail I: **Cloud Wisp** |
| 11 | Property I: `staff.jl3` Pinwheel can Special-cancel into `staff.jqcfs` Falling Gyre |
| 13 | Branch: `staff.l3h` Pinwheel Rise |
| 14 | Trail II: **Aqueduct Rain** · Emote: *Resting Spin* |
| 15 | Property II: `staff.qcbs` Gust Palm **reflects** projectiles |
| 17 | Execution variant: *Quiet Branch — Stormlight* |
| 18 | Property III: holding `2` during `staff.s` Eye of Calm switches it to a low-catching stance |
| 19 | Skin: **Master's Still Wind — Cloudspire Branch** |
| 20 | Crest: *Still Wind Master* · Trail: **Oath-light Staff** |

---

## 11. Scythe — Harvest Style (`weapon.scythe`)

**Fantasy.** The harvest comes for everyone. A Harvest fighter reaches behind the opponent's guard
with a hooked blade, drags them into range, and takes a little of their life with every cut.

| Range | Speed | Damage | Posture | Startup | Difficulty |
|---|---|---|---|---|---|
| 4 | 2 | 4 | 2 | B | ★★★★ |

**Strengths:** pull-ins that deny retreat, cross-ups by hooking from behind, drain sustain (PvE), and traps.
**Weaknesses:** slow recovery, weakness at point-blank range, low posture damage, and a dependence on reads.

**Class trait — Harvest.** Hits with the blade (not the shaft; hitboxes are marked in data) drain
life. **PvE:** heal 4% of the damage dealt. **PvP:** no healing; each blade hit instead grants **+10 Shadow
meter (1%)** on top of the engine's per-hit gain.

### 11.1 Move list

| ID | Name | Input (Classic / Simplified) | Height | Properties | Unlock |
|---|---|---|---|---|---|
| `scythe.l1` | Reap | `L` | Mid | — | M1 |
| `scythe.l2` | Glean | `L` (after l1) | Mid | — | M1 |
| `scythe.l3` | Winnow | `L` (after l2) | Mid | 2 hits | M1 |
| `scythe.l4` | Harvest Moon | `L` (after l3) | Mid | Light finisher, **wall bounce** | M1 |
| `scythe.l2h` | Uprooting | `L L H` | Mid | **Launcher**, jc | M2 |
| `scythe.l3h` | Crossing the Field | `L L L H` | Mid | Hooks behind and vaults over: **cross-up**, side switch | M13 |
| `scythe.h1` | Hook | `H` | Mid | **Pull-in** 1 m | M1 |
| `scythe.h2` | Pull of the Field | `H` (after h1) | Mid | **Pull-in** 1.5 m | M1 |
| `scythe.h3` | Threshing | `H` (after h2) | Overhead | **Ground bounce** | M1 |
| `scythe.hcharge` | Grave Wind | `[H]` | — | 3 levels | M5 |
| `scythe.hcharged` | Reaper's Wheel | `]H[` | Mid | Spinning pull; level 3 = **Guard Crush** | M5 |
| `scythe.fl` | Long Hook | `6L` | Mid | 3.2 m; **pull-in** on hit | M1 |
| `scythe.bh` | Tolling Arc | `4H` | **Overhead** | The blade arcs behind the opponent (hits cross-up side) | M1 |
| `scythe.dl` | Stubble Cut | `2L` | **Low** | Special-cancelable | M1 |
| `scythe.dh` | Field Reaper | `2H` | **Low** | Knockdown + pull-in | M1 |
| `scythe.jl` | Crow's Cut | `j.L` | Overhead (air) | — | M1 |
| `scythe.jl2` | Crow's Return | `j.L` (after jl) | Overhead (air) | — | M1 |
| `scythe.jh` | Carrion Fall | `j.H` | Overhead (air) | **Air spike, ground bounce** | M1 |
| `scythe.s` | Soul Hook | `5S` / `5S` | Mid | 3.5 m hook; pulls the opponent adjacent on hit, 30-frame stagger | M9 |
| `scythe.sreap` | Soul Reap | `H` during the Soul Hook pull | Mid | Slash timed with the arrival | M9 |
| `scythe.qcfs` | Crossing Harvest | `236S` / `6S` | Mid | Dash-through slash: **cross-up** on hit and block | M1 |
| `scythe.dps` | Grave Rise | `623S` / `8S` | Mid | Rising spin, **launcher**, tall hitbox; **no invulnerability** | M6 |
| `scythe.qcbs` | Tithe Mark | `214S` / `4S` | Mid (trap) | Places a sigil 2.5 m ahead (max 1). Detonates on contact or after 300 frames and launches; blade-drain applies | M3 |
| `scythe.ultimate` | Reaping of the Unreborn | `Ultimate` | — | Cinematic | M10 |
| `scythe.execution` | Gleaner's Due | `Execute` | — | Paired Execution | M1 |

### 11.2 Combo trees

```
LIGHT TREE
L ─► L ─► L ─► L        l1 Reap → l2 Glean → l3 Winnow → l4 Harvest Moon (wb)
     │    └─► H         scythe.l3h Crossing the Field (M13, cross-up)
     └─► H              scythe.l2h Uprooting (launcher)
HEAVY TREE  H ─► H ─► H       Hook (pull) → Pull of the Field (pull) → Threshing (gb)
CHARGED     [H] ─► ]H[        Grave Wind → Reaper's Wheel (L3 guard crush)
AIR         j.L ─► j.L ─► j.H Crow's Cut → Crow's Return → Carrion Fall (spike, gb)
SPECIAL     5S ~ H            Soul Hook → Soul Reap
```

### 11.3 Ultimate — Reaping of the Unreborn (`scythe.ultimate`)

A field of pale wheat grows out of the shadows under the target. Rhen walks through it with the
scythe held low. Each stalk he cuts is a strand of the target's shadow, and Sable gathers them in
his arms. At the edge of the field Rhen turns and pulls the whole field toward him with one hooked
stroke. The target's shadow tears loose and is cut free: released, not tithed. **6.0 s.**

### 11.4 Execution — Gleaner's Due (`scythe.execution`)

Rhen hooks the scythe around the opponent from behind, pulls them back into a kneel, and draws the
blade in a slow arc. Their shadow flickers and is gathered by Sable. **3.0 s.** On the mobile
default setting, the cut happens off-frame.

### 11.5 Signature routes

| Level | Route | Notes |
|---|---|---|
| Beginner | `6L (pull), L L L L (wb)` | Long Hook drags them into the chain |
| Beginner | `H H H (gb)` | Two pulls and a Threshing ground bounce |
| Intermediate | `214S`; the opponent steps on it → `jc j.L j.L j.H (gb), 236S` | The trap launches, then a cross-up to reset pressure |
| Intermediate | `CH 4H, L L H jc j.L j.L j.H (gb)` | The cross-side overhead converts into a launch |
| Expert | `5S ~ H, L L H jc j.L j.L j.H (gb), [H] ]H[ L2, 623S > ULT` | Soul Hook from 3.5 m to Ultimate. In PvE the blade hits heal about 9% HP |
| Expert | `L L L H (cross-up), 2H (pull), 214S` | Crossing the Field (M13) into Field Reaper, then an oki trap on the wake-up point |

### 11.6 Matchup notes

- **vs Whip Blade:** A long-range mirror. Soul Hook beats their retreat, and Tithe Mark punishes their backdash patterns.
- **vs Gauntlets:** They are inside your blade. Crossing Harvest escapes, and `2L` into a pull resets the range you want.
- **vs Dual Blades:** Their hit volume beats your startup. Pre-place Tithe Mark and fight around it.
- **vs Spear:** Hook drags them off their tip distance, and Tolling Arc cross-side beats Planted Vigil.

### 11.7 Mastery track

| Level | Unlock |
|---|---|
| 7 | Trail I: **Pale Wheat** |
| 11 | Property I: `scythe.qcbs` Tithe Mark allows 2 active sigils |
| 13 | Branch: `scythe.l3h` Crossing the Field |
| 14 | Trail II: **Crowfeather** · Emote: *Scythe Lean* |
| 15 | Property II: `scythe.s` Soul Hook works on airborne opponents (pulls them down: **ground bounce**) |
| 17 | Execution variant: *Gleaner's Due — Glowcap* |
| 18 | Property III: `scythe.dps` Grave Rise gains 1-hit armor |
| 19 | Skin: **Master's Harvest — Rootbound Crescent** |
| 20 | Crest: *Harvest Master* · Trail: **Oath-light Scythe** |

---

## 12. War Hammer — Anvil Style (`weapon.warhammer`)

**Fantasy.** The anvil does not dodge. Anvil fighters walk through blows, crush guards to
splinters, and pound opponents into the floor until the floor gives way.

| Range | Speed | Damage | Posture | Startup | Difficulty |
|---|---|---|---|---|---|
| 3 | 1 | 5 | 5 | C | ★★ |

**Strengths:** the highest posture damage, Guard Crushes, armor on every Heavy, ground bounces, and an easy learning curve.
**Weaknesses:** the slowest attacks, poor mobility, very punishable whiffs, and weakness against zoners.

**Class trait — Anvil Weight.** Every Heavy-button attack has **1-hit armor** (2-hit in Ember Rage),
and blocked Heavy-button attacks deal **+25% posture damage**.

### 12.1 Move list

| ID | Name | Input (Classic / Simplified) | Height | Properties | Unlock |
|---|---|---|---|---|---|
| `warhammer.l1` | Haft Jab | `L` | Mid | — | M1 |
| `warhammer.l2` | Haft Hook | `L` (after l1) | Mid | — | M1 |
| `warhammer.l3` | Forge Swing | `L` (after l2) | Mid | Light finisher, **wall bounce** | M1 |
| `warhammer.l2h` | Bellows Lift | `L L H` | Mid | **Launcher**, jc, 1-hit armor | M2 |
| `warhammer.h1` | Anvil Blow | `H` | Mid | Armor | M1 |
| `warhammer.h2` | Second Blow | `H` (after h1) | Mid | Armor | M1 |
| `warhammer.h3` | Tempering Strike | `H` (after h2) | Overhead | **Ground bounce**, armor | M1 |
| `warhammer.h2l` | Haft Recoil | `H H L` | Mid | Fast haft jab out of the Heavy chain; +2 on block (pressure reset) | M13 |
| `warhammer.hcharge` | Stoking | `[H]` | — | Armor from level 1; 3 levels | M5 |
| `warhammer.hcharged` | Forgequake | `]H[` | Mid | Level 3 = **Guard Crush** plus a 3 m shockwave that hits **OTG** | M5 |
| `warhammer.fl` | Haft Thrust | `6L` | Mid | Special-cancelable | M1 |
| `warhammer.bh` | Hammerfall | `4H` | **Overhead** | Armor, Crumple on CH | M1 |
| `warhammer.dl` | Toe Crusher | `2L` | **Low** | Special-cancelable | M1 |
| `warhammer.dh` | Quake Sweep | `2H` | **Low** | Knockdown, 2 m shockwave, armor | M1 |
| `warhammer.jl` | Air Haft | `j.L` | Overhead (air) | — | M1 |
| `warhammer.jh` | Meteor Hammer | `j.H` | Overhead (air) | **Air spike, ground bounce** | M1 |
| `warhammer.s` | Iron Resolve | `5S` / `5S` | — | 60-frame stance that absorbs up to 2 strikes | M9 |
| `warhammer.srelease` | Resolve Breaker | Release `S`, or automatic after the 2nd absorb | Mid | +20% damage per absorbed hit; after 2 absorbs it is a **Guard Crush** | M9 |
| `warhammer.qcfs` | Rolling Anvil | `236S` / `6S` | Mid | Armored charge (1-hit), **wall bounce** | M1 |
| `warhammer.dps` | Geyser Blow | `623S` / `8S` | Mid | Upward blow, **launcher**, 1-hit armor, **no invulnerability** | M6 |
| `warhammer.qcbs` | Shockwave | `214S` / `4S` | **Low** (projectile) | Ground-travelling shockwave | M3 |
| `warhammer.ultimate` | The Ninth Forge | `Ultimate` | — | Cinematic | M10 |
| `warhammer.execution` | Final Temper | `Execute` | — | Paired Execution | M1 |

### 12.2 Combo trees

```
LIGHT TREE
L ─► L ─► L             l1 Haft Jab → l2 Haft Hook → l3 Forge Swing (wb)
     └─► H              warhammer.l2h Bellows Lift (launcher)
HEAVY TREE
H ─► H ─► H             h1 Anvil Blow → h2 Second Blow → h3 Tempering Strike (gb)   [all armored]
     └─► L              warhammer.h2l Haft Recoil (M13, +2 on block)
CHARGED   [H] ─► ]H[    Stoking → Forgequake (L3 guard crush + OTG shockwave)
AIR       j.L ─► j.H    Air Haft → Meteor Hammer (spike, gb)
STANCE    5S ─ absorb ×2 / release ─► warhammer.srelease Resolve Breaker
```

### 12.3 Ultimate — The Ninth Forge (`warhammer.ultimate`)

The hammer connects, and the arena dissolves into a spectral forge: nine anvils in a ring of
molten light. Rhen strikes the target down onto each anvil in turn, and each blow rings a different
note. Sable works the bellows, and the flames turn violet. The ninth blow quenches the target in a burst
of steam and Ember. **6.0 s.**

### 12.4 Execution — Final Temper (`warhammer.execution`)

Rhen hooks the hammer's beak behind the opponent's knee, drops them, and brings the hammer down in
a two-handed overhead. The floor cracks outward in a ring. **2.5 s.** On the mobile default
setting, the impact is framed from behind Rhen.

### 12.5 Signature routes

| Level | Route | Notes |
|---|---|---|
| Beginner | `L L L (wb)` | Three hits into a wall bounce |
| Beginner | `H H H (gb)` | Armored the whole way through |
| Intermediate | `L L H jc j.L j.H (gb), 236S (wb)` | Bellows Lift, Meteor Hammer, Rolling Anvil wall bounce |
| Intermediate | `H H L, H H L` (block pressure) | Haft Recoil (M13) resets pressure at +2 |
| Expert | `CH 4H (crumple), L L H jc j.L j.H (gb), 236S (wb), RAGE, [H] ]H[ L2, 623S > ULT` | Both bounces, Rage activation, 2-hit armor for the finish |
| Expert | `5S` absorbs 2 → Resolve Breaker (Guard Crush) → `EXE` | Punish a mashing opponent straight into Final Temper |

### 12.6 Matchup notes

- **vs Dual Blades:** Their multi-hit strings strip 1-hit armor. Wait for the string to end, or use Iron Resolve's 2 absorbs.
- **vs Whip Blade:** Your worst matchup. Rolling Anvil armor goes through single snaps, and Shockwave hits their low-profile retreat.
- **vs Katana:** Stillwater counters your heavies. Bait it with Iron Resolve (they can't counter a stance) and throw them.
- **vs Gauntlets:** Armor beats their jabs, but Fang Lock beats armor. Keep them at haft range with `6L`.

### 12.7 Mastery track

| Level | Unlock |
|---|---|
| 7 | Trail I: **Molten Spark** |
| 11 | Property I: `warhammer.qcbs` Shockwave can be held (`[S]`) to travel 50% farther |
| 13 | Branch: `warhammer.h2l` Haft Recoil |
| 14 | Trail II: **Slag Ember** · Emote: *Anvil Rest* |
| 15 | Property II: `warhammer.l3` Forge Swing gains 1-hit armor |
| 17 | Execution variant: *Final Temper — Quench* |
| 18 | Property III: `warhammer.hcharge` Stoking can walk forward while charging |
| 19 | Skin: **Master's Anvil — Ironroot Maul** |
| 20 | Crest: *Anvil Master* · Trail: **Oath-light Hammer** |

---

## 13. Gauntlets — Iron Fang Style (`weapon.gauntlets`)

**Fantasy.** No blade, no distance, no mercy. Iron Fang fighters live inside the opponent's reach,
win every exchange by a frame, and turn every hesitation into a grab.

| Range | Speed | Damage | Posture | Startup | Difficulty |
|---|---|---|---|---|---|
| 1 | 5 | 3 | 3 | **S** | ★★ |

**Strengths:** the fastest startup in the game, frame traps, two command grabs, the fastest walk speed, and an invincible reversal.
**Weaknesses:** the shortest range, weakness against zoning, a need to approach, and command grabs with long whiff recovery.

**Class trait — Clinch.** Universal throw range is **+15%**. A universal throw started within 12 frames
after the end of a blocked Gauntlets Light (after the 6-frame throw protection) deals **+20% damage**.

### 13.1 Move list

| ID | Name | Input (Classic / Simplified) | Height | Properties | Unlock |
|---|---|---|---|---|---|
| `gauntlets.l1` | Fang Jab | `L` | Mid | Fastest normal in the game | M1 |
| `gauntlets.l2` | Fang Cross | `L` (after l1) | Mid | — | M1 |
| `gauntlets.l3` | Iron Hook | `L` (after l2) | Mid | — | M1 |
| `gauntlets.l4` | Fang Rush | `L` (after l3) | Mid | 4 hits | M1 |
| `gauntlets.l5` | Breaking Palm | `L` (after l4) | Mid | Light finisher, **wall bounce** | M1 |
| `gauntlets.l2h` | Rising Fang | `L L H` | Mid | Uppercut **launcher**, jc | M2 |
| `gauntlets.l3h` | Liver Hook | `L L L H` | Mid | **Crumple** on Counter Hit or Punish Counter | M13 |
| `gauntlets.h1` | Iron Body Blow | `H` | Mid | — | M1 |
| `gauntlets.h2` | Iron Elbow | `H` (after h1) | Mid | — | M1 |
| `gauntlets.h3` | Hammerfist | `H` (after h2) | Overhead | **Ground bounce** | M1 |
| `gauntlets.hcharge` | Coiled Fang | `[H]` | — | 3 levels | M5 |
| `gauntlets.hcharged` | Ironheart Strike | `]H[` | Mid | Level 3 = **Guard Crush** | M5 |
| `gauntlets.fl` | Stepping Fang | `6L` | Mid | Advancing jab; Special-cancelable | M1 |
| `gauntlets.bh` | Axe Kick | `4H` | **Overhead** | — | M1 |
| `gauntlets.dl` | Low Jab | `2L` | **Low** | Fastest low in the game; chains into itself once | M1 |
| `gauntlets.dh` | Leg Sweep | `2H` | **Low** | Knockdown | M1 |
| `gauntlets.jl` | Air Fang | `j.L` | Overhead (air) | — | M1 |
| `gauntlets.jl2` | Air Knee | `j.L` (after jl) | Overhead (air) | — | M1 |
| `gauntlets.jh` | Diving Fist | `j.H` | Overhead (air) | **Air spike, ground bounce** | M1 |
| `gauntlets.s` | Fang Lock | `5S` / `5S` | Grab | **Command grab**: unblockable, cannot be teched, violet telegraph, 45-frame whiff recovery | M9 |
| `gauntlets.qcfs` | Iron Charge | `236S` / `6S` | Mid | Shoulder rush, 1-hit armor, **wall bounce** on CH | M1 |
| `gauntlets.dps` | Rising Iron | `623S` / `8S` | Mid | **Invincible** uppercut (f1–5), **launcher**, very punishable on block | M6 |
| `gauntlets.qcbs` | Skyseize | `214S` / `4S` | Grab | Anti-air **command grab** (airborne targets only) | M3 |
| `gauntlets.ultimate` | Night of Iron Fangs | `Ultimate` | — | Cinematic | M10 |
| `gauntlets.execution` | Heartstop | `Execute` | — | Paired Execution | M1 |

### 13.2 Combo trees

```
LIGHT TREE
L ─► L ─► L ─► L ─► L     l1 Fang Jab → l2 Fang Cross → l3 Iron Hook → l4 Fang Rush → l5 Breaking Palm (wb)
     │    └─► H           gauntlets.l3h Liver Hook (M13, CH/PC crumple)
     └─► H                gauntlets.l2h Rising Fang (launcher)
HEAVY TREE  H ─► H ─► H         Iron Body Blow → Iron Elbow → Hammerfist (gb)
CHARGED     [H] ─► ]H[          Coiled Fang → Ironheart Strike (L3 guard crush)
AIR         j.L ─► j.L ─► j.H   Air Fang → Air Knee → Diving Fist (spike, gb)
GRABS       5S Fang Lock (ground) · 214S Skyseize (air)
```

### 13.3 Ultimate — Night of Iron Fangs (`gauntlets.ultimate`)

The first punch lands and the world snaps to night. Ember lights Rhen's left fist and Umbra his
right. He and Sable, his mirror image, trade places with every blow: forty strikes in four
seconds. Each impact flashes a single frame of the target's memories. Then Rhen's Ember fist and Sable's
Umbra fist land together, and dawn floods back. **5.0 s.**

### 13.4 Execution — Heartstop (`gauntlets.execution`)

Rhen grabs the opponent's collar, drives a knee into them, spins them into a rear hold, and
delivers a single palm strike to the back. Their shadow flickers out. **2.5 s.** No gore.

### 13.5 Signature routes

| Level | Route | Notes |
|---|---|---|
| Beginner | `L L L L L (wb)` | Nine hits into a wall bounce |
| Beginner | `2L, 2L > 236S` | Low Jab twice into Iron Charge |
| Intermediate | `L L H jc j.L j.L j.H (gb), 623S` | Rising Fang route with a Rising Iron relaunch |
| Intermediate | `L` (blocked) → `GRAB` (Clinch +20%) **or** `L` (frame trap) | The core Iron Fang mixup |
| Expert | `CH 6L, L L L H (crumple), EXE` | Liver Hook (M13) crumple at ≤ 15% HP is a lethal Heartstop |
| Expert | `L L L L L (wb), SHADOW, H H H (gb), 623S > ULT` | Corner route. Echoes on every Fang Rush hit |

### 13.6 Matchup notes

- **vs Whip Blade:** The hardest matchup. Dash, then `Guard`, and use Iron Charge armor through single snaps. Once you are inside 0.8 m, their normals can't hit you.
- **vs Nodachi:** Their armor beats your Lights, but grabs ignore armor. Fang Lock and Clinch throws are your main offense.
- **vs Katana:** Fang Lock beats Stillwater, and Rising Iron beats Crescent Rush on reaction.
- **vs Daggers:** Speed mirror. Skyseize catches their airborne Knife Rain setups.

### 13.7 Mastery track

| Level | Unlock |
|---|---|
| 7 | Trail I: **Iron Spark** |
| 11 | Property I: `gauntlets.l4` Fang Rush becomes +2 on block |
| 13 | Branch: `gauntlets.l3h` Liver Hook |
| 14 | Trail II: **Aurora Knuckle** · Emote: *Knuckle Crack* |
| 15 | Property II: pressing `Guard` during `gauntlets.qcfs` Iron Charge startup cancels it (charge feint, 10-frame recovery) |
| 17 | Execution variant: *Heartstop — Rimebreath* |
| 18 | Property III: `gauntlets.dps` Rising Iron becomes jump-cancelable on hit |
| 19 | Skin: **Master's Iron Fang — Steppe-Forged** |
| 20 | Crest: *Iron Fang Master* · Trail: **Oath-light Gauntlets** |

---

## 14. Daggers — Ninefold Style (`weapon.daggers`)

**Fantasy.** Nine ways to die, and you only see one. Ninefold fighters vanish, reappear where they
shouldn't be, and leave wounds that keep bleeding after the fight has moved on.

| Range | Speed | Damage | Posture | Startup | Difficulty |
|---|---|---|---|---|---|
| 3 | 4 | 2 | 2 | A | ★★★★★ |

**Strengths:** teleport mixups, damage over time, throwing knives, and the fastest cross-ups.
**Weaknesses:** low damage per hit, teleports that are punishable when predicted, and low posture damage.

**Class trait — Open Wounds.** Dagger blade hits build **Bleed** (PvE status; 07-Progression §6).
In **PvP** every 5th dagger hit in a combo (echoes included) adds a **Cut**: 10 damage per second for
3 s (maximum 3 Cuts, 90 damage total). Cuts are deterministic and pause during hitstop and cinematics.

### 14.1 Move list

| ID | Name | Input (Classic / Simplified) | Height | Properties | Unlock |
|---|---|---|---|---|---|
| `daggers.l1` | Needle | `L` | Mid | — | M1 |
| `daggers.l2` | Second Needle | `L` (after l1) | Mid | — | M1 |
| `daggers.l3` | Stitching Cuts | `L` (after l2) | Mid | 3 hits | M1 |
| `daggers.l4` | Parting Stitch | `L` (after l3) | Mid | Light finisher, **wall bounce** | M1 |
| `daggers.l2h` | Upward Stitch | `L L H` | Mid | **Launcher**, jc | M2 |
| `daggers.l3h` | Vanishing Stitch | `L L L H` | Mid | Vanishes and strikes from behind: **cross-up**, −2 on block | M13 |
| `daggers.h1` | Hilt Punch | `H` | Mid | — | M1 |
| `daggers.h2` | Reverse Stab | `H` (after h1) | Mid | — | M1 |
| `daggers.h3` | Pinning Stab | `H` (after h2) | Overhead | **Ground bounce** | M1 |
| `daggers.hcharge` | Drawn Needles | `[H]` | — | 3 levels | M5 |
| `daggers.hcharged` | Needle Rain | `]H[` | Mid (projectile) | Fan of knives: L1 3 knives, L2 5, L3 9 tracking knives that pierce. No Guard Crush | M5 |
| `daggers.fl` | Lunge Needle | `6L` | Mid | Special-cancelable | M1 |
| `daggers.bh` | Dropping Knife | `4H` | **Overhead** | Front flip | M1 |
| `daggers.dl` | Heel Cut | `2L` | **Low** | Strong Bleed buildup | M1 |
| `daggers.dh` | Hamstring | `2H` | **Low** | Knockdown | M1 |
| `daggers.jl` | Air Needle | `j.L` | Overhead (air) | — | M1 |
| `daggers.jl2` | Air Stitch | `j.L` (after jl) | Overhead (air) | — | M1 |
| `daggers.jh` | Falling Needle | `j.H` | Overhead (air) | **Air spike, ground bounce** | M1 |
| `daggers.s` | Shadowstep | `5S` / `5S` | — | Vanish (f6–24 invulnerable); the held direction picks where to reappear: `6` behind the opponent, `4` 3 m back, `5` in place (feint). `Guard` while vanished cancels (12 frames recovery) | M9 |
| `daggers.sstrike` | Backstab | `L` or `H` while reappearing | Mid | Crumple on Counter Hit | M9 |
| `daggers.qcfs` | Throwing Knives | `236S` / `6S` | Mid (projectile) | 3 knives | M1 |
| `daggers.jqcfs` | Knife Rain | `j.236S` / `j.6S` | Overhead (projectile) | 3 knives angled down | M3 |
| `daggers.dps` | Rising Needle | `623S` / `8S` | Mid | Flip kick, **launcher**, invulnerable to air attacks f1–8 | M6 |
| `daggers.qcbs` | Mirage Feint | `214S` / `4S` | — | Steps back 1.5 m and leaves a decoy that absorbs 1 hit and bursts into smoke (PvP: 20-frame stagger; PvE: blinds enemies for 1.5 s) | M3 |
| `daggers.ultimate` | A Stitch in the Dark | `Ultimate` | — | Cinematic | M10 |
| `daggers.execution` | Quiet Needle | `Execute` | — | Paired Execution | M1 |

### 14.2 Combo trees

```
LIGHT TREE
L ─► L ─► L ─► L        l1 Needle → l2 Second Needle → l3 Stitching Cuts → l4 Parting Stitch (wb)
     │    └─► H         daggers.l3h Vanishing Stitch (M13, cross-up)
     └─► H              daggers.l2h Upward Stitch (launcher)
HEAVY TREE  H ─► H ─► H       Hilt Punch → Reverse Stab → Pinning Stab (gb)
CHARGED     [H] ─► ]H[        Drawn Needles → Needle Rain
AIR         j.L ─► j.L ─► j.H Air Needle → Air Stitch → Falling Needle (spike, gb)
SPECIAL     5S [4/5/6] ~ L/H  Shadowstep → Backstab   ·   5S ~ Guard = feint
```

### 14.3 Ultimate — A Stitch in the Dark (`daggers.ultimate`)

The knife connects and every light in the arena goes out. Nine points of Oath-light open in the
dark, and each one is Rhen or Sable arriving from a different angle to put in a single stitch.
The ninth is Rhen alone, face to face, sliding the final needle home as the lights return.
**5.0 s.**

### 14.4 Execution — Quiet Needle (`daggers.execution`)

Rhen steps behind the staggered opponent in a blink of shadow, covers their eyes with one hand, and
places a single needle at the base of the skull. **2.0 s.**

### 14.5 Signature routes

| Level | Route | Notes |
|---|---|---|
| Beginner | `L L L L (wb)` | Six hits, one Cut in PvP |
| Beginner | `236S` at range, `j.236S` on approach | Knife zoning and Knife Rain cover |
| Intermediate | `L L H jc j.L j.L j.H (gb), 236S` | Launcher route ending with knives on the bounce |
| Intermediate | `5S [6] ~ L` / `5S [5] ~ Guard, GRAB` | Cross-up Backstab or feint into a throw: the core Ninefold 50/50 |
| Expert | `5S [6] ~ L (CH crumple), L L L L (wb), SHADOW, H H H (gb), 623S > ULT` | Echoes count toward Cuts (3 Cuts by the end) |
| Expert | `2L, L L L H (cross-up), L L H jc j.L j.L j.H` | Vanishing Stitch (M13) side switch into the launcher (l1/l2 stale) |

### 14.6 Matchup notes

- **vs Katana:** Stillwater catches Backstab. Feint Shadowstep with `Guard` and throw the stance.
- **vs Nodachi:** Your multi-hits strip their 1-hit armor, but Unbending punishes a Backstab. Mix Shadowstep `4` (retreat) with knives.
- **vs Gauntlets:** Speed mirror. Shadowstep escapes corner pressure, but never reappear into Fang Lock range on a read.
- **vs Whip Blade:** Throwing Knives trade with Lash Line. Shadowstep `6` lands inside their dead zone.

### 14.7 Mastery track

| Level | Unlock |
|---|---|
| 7 | Trail I: **Needle Glint** |
| 11 | Property I: `daggers.s` Shadowstep gains an `8` option (reappear above the opponent; Backstab becomes **Overhead**) |
| 13 | Branch: `daggers.l3h` Vanishing Stitch |
| 14 | Trail II: **Lantern Smoke** · Emote: *Knife Juggle* |
| 15 | Property II: holding `S` for `daggers.qcfs` Throwing Knives throws a 5-knife fan |
| 17 | Execution variant: *Quiet Needle — Canal Mist* |
| 18 | Property III: `daggers.qcbs` Mirage Feint's decoy can be detonated manually (press `S` again) |
| 19 | Skin: **Master's Ninefold — Rooftop Nightblades** |
| 20 | Crest: *Ninefold Master* · Trail: **Oath-light Daggers** |

---

## 15. Chain Sword — Censer Chain Style (`weapon.chainsword`)

**Fantasy.** A sword that remembers it was once a chain. Censer Chain fighters fight at two
distances at once: a blade in the hand, and a whip of segments that reaches across the room trailing
incense and embers.

| Range | Speed | Damage | Posture | Startup | Difficulty |
|---|---|---|---|---|---|
| 4 | 3 | 3 | 3 | B | ★★★ |

**Strengths:** two effective ranges, pulls, a huge anti-air arc, and Burn synergy in PvE.
**Weaknesses:** extended attacks recover slowly, the mode switch is readable, and the Heavies are weak at point-blank range.

**Class trait — Extension.** Light-button attacks are **sword mode** (0.9–1.6 m). Heavy-button attacks
and Specials **extend the chain** (2.5–4.5 m). A hit at full extension leaves the chain extended for
20 frames, and the next Light gains +1 m of range.

### 15.1 Move list

| ID | Name | Input (Classic / Simplified) | Height | Properties | Unlock |
|---|---|---|---|---|---|
| `chainsword.l1` | Link Cut | `L` | Mid | Sword mode | M1 |
| `chainsword.l2` | Double Link | `L` (after l1) | Mid | — | M1 |
| `chainsword.l3` | Censer Swing | `L` (after l2) | Mid | 2 hits | M1 |
| `chainsword.l4` | Chain Lash | `L` (after l3) | Mid | Extends; Light finisher, **wall bounce** | M1 |
| `chainsword.l2h` | Censer Rise | `L L H` | Mid | **Launcher**, jc | M2 |
| `chainsword.l3h` | Ember Coil | `L L L H` | Mid | The chain wraps and **pulls in**; 24-frame stagger on hit | M13 |
| `chainsword.h1` | Extending Lash | `H` | Mid | 4 m lash | M1 |
| `chainsword.h2` | Returning Coil | `H` (after h1) | Mid | Hits on the return; **pull-in** | M1 |
| `chainsword.h3` | Rite of Chains | `H` (after h2) | Overhead | **Ground bounce** | M1 |
| `chainsword.hcharge` | Swinging Censer | `[H]` | — | The chain whirls overhead and hits airborne opponents; 3 levels | M5 |
| `chainsword.hcharged` | Ashfall Rite | `]H[` | Mid | Level 3 = **Guard Crush**; strong Burn buildup (PvE) | M5 |
| `chainsword.fl` | Segment Thrust | `6L` | Mid | 3.5 m poke; Special-cancelable | M1 |
| `chainsword.bh` | Descending Chain | `4H` | **Overhead** | 3 m | M1 |
| `chainsword.dl` | Ankle Coil | `2L` | **Low** | Special-cancelable | M1 |
| `chainsword.dh` | Floor Lash | `2H` | **Low** | Knockdown, 3.5 m | M1 |
| `chainsword.jl` | Air Link | `j.L` | Overhead (air) | — | M1 |
| `chainsword.jl2` | Air Link Return | `j.L` (after jl) | Overhead (air) | — | M1 |
| `chainsword.jh` | Chain Anchor | `j.H` | Overhead (air) | **Air spike**: anchors and drags the target down, **ground bounce** | M1 |
| `chainsword.s` | Chain Snare | `5S` / `5S` | Mid | 3.5 m hook; **pulls** the opponent adjacent on hit | M9 |
| `chainsword.sreel` | Reeling Cut | `L` during the Chain Snare pull | Mid | Slash timed with the arrival | M9 |
| `chainsword.qcfs` | Serpent Lash | `236S` / `6S` | Mid | Full extension forward, 4.5 m | M1 |
| `chainsword.dps` | Censer Arc | `623S` / `8S` | Mid | Circular overhead arc, anti-air, **launcher** on airborne targets; **no invulnerability** | M6 |
| `chainsword.qcbs` | Coil Retreat | `214S` / `4S` | Mid | Backflip 2 m with a trailing lash | M3 |
| `chainsword.ultimate` | Litany of Links | `Ultimate` | — | Cinematic | M10 |
| `chainsword.execution` | Bound Penance | `Execute` | — | Paired Execution | M1 |

### 15.2 Combo trees

```
LIGHT TREE (sword mode)
L ─► L ─► L ─► L        l1 Link Cut → l2 Double Link → l3 Censer Swing → l4 Chain Lash (extends, wb)
     │    └─► H         chainsword.l3h Ember Coil (M13, pull)
     └─► H              chainsword.l2h Censer Rise (launcher)
HEAVY TREE (chain mode)
H ─► H ─► H             Extending Lash → Returning Coil (pull) → Rite of Chains (gb)
CHARGED   [H] ─► ]H[    Swinging Censer → Ashfall Rite (L3 guard crush)
AIR       j.L ─► j.L ─► j.H     Air Link → Air Link Return → Chain Anchor (spike, gb)
SPECIAL   5S ~ L        Chain Snare → Reeling Cut
```

### 15.3 Ultimate — Litany of Links (`chainsword.ultimate`)

The chain sword unspools without end. Its links wrap the target in a spiral of glowing ember-script
while Rhen recites a single line of the tithe rite backwards, and each link ignites in turn. Sable
hauls on the far end. The prayer circle tightens and snaps apart in a burst of incense smoke and
sparks. **5.5 s.**

### 15.4 Execution — Bound Penance (`chainsword.execution`)

Rhen wraps the chain around the opponent's arms, forces them to kneel as if in prayer, and draws
the chain tight through the blade segments. **3.0 s.**

### 15.5 Signature routes

| Level | Route | Notes |
|---|---|---|
| Beginner | `L L L L (wb)` | Sword mode to chain finisher |
| Beginner | `6L`, `236S` | Mid-range poke and full-extension lash |
| Intermediate | `2L > 236S` | Low into Serpent Lash |
| Intermediate | `L L H jc j.L j.L j.H (gb), 623S` | Chain Anchor drag-down into Censer Arc relaunch |
| Expert | `5S ~ L, L L L L (wb), RAGE, H H H (gb), [H] ]H[ L2, 623S > ULT` | Snare from 3.5 m to Litany of Links |
| Expert | `H (max range, CH), L` (extended: +1 m) `L L L (wb)` | Extension window confirms a max-range lash into the Light chain |

### 15.6 Matchup notes

- **vs Whip Blade:** They outrange your chain by about 0.5 m. Win the fight inside 2.5 m with sword-mode Lights.
- **vs Katana:** Stillwater catches any lash, but its Reprisal reaches only about 2 m. A caught lash at full extension leaves the Katana whiffing, so fight at 3 m+ and throw them when they stance up close.
- **vs Gauntlets:** Keep them at chain range. Coil Retreat resets the distance.
- **vs Spear:** Chain Snare pulls them off their tip distance, and Censer Arc punishes Pole Vault.

### 15.7 Mastery track

| Level | Unlock |
|---|---|
| 7 | Trail I: **Incense Trail** |
| 11 | Property I: the Extension window lasts 30 frames instead of 20 |
| 13 | Branch: `chainsword.l3h` Ember Coil |
| 14 | Trail II: **Censer Smoke** · Emote: *Chain Twirl* |
| 15 | Property II: `chainsword.qcbs` Coil Retreat can Special-cancel into `chainsword.qcfs` Serpent Lash |
| 17 | Execution variant: *Bound Penance — Tolling* |
| 18 | Property III: `chainsword.jh` Chain Anchor on a grounded opponent pulls Rhen down to them (air approach) |
| 19 | Skin: **Master's Censer Chain — Ashen Rosary** |
| 20 | Crest: *Censer Chain Master* · Trail: **Oath-light Chain Sword** |

---

## 16. Whip Blade — Silken Lash Style (`weapon.whipblade`)

**Fantasy.** The longest reach in Varanth belongs to a ribbon of steel. Silken Lash fighters paint
the arena with cracks and snaps, pulling opponents into the tip or pushing them away before they ever
reach the dancer.

| Range | Speed | Damage | Posture | Startup | Difficulty |
|---|---|---|---|---|---|
| 5 | 3 | 2 | 2 | B | ★★★★ |

**Strengths:** the longest reach (a full-arena `236S`), the best zoning, pulls, and a barrier against projectiles.
**Weaknesses:** the weakest up close, low damage, and long whiff recovery on max-range snaps.

**Class trait — Tip Snap.** Hits with the last 0.4 m of the whip deal **+25% damage and +6 frames
of hitstun**. **Dead zone:** Whip Blade normals other than `whipblade.l1` and `whipblade.dl` cannot hit
targets closer than 0.8 m, because the ribbon passes over them.

### 16.1 Move list

| ID | Name | Input (Classic / Simplified) | Height | Properties | Unlock |
|---|---|---|---|---|---|
| `whipblade.l1` | Flick | `L` | Mid | Close range; hits inside the dead zone | M1 |
| `whipblade.l2` | Snap | `L` (after l1) | Mid | 3 m | M1 |
| `whipblade.l3` | Crack | `L` (after l2) | Mid | 4 m | M1 |
| `whipblade.l4` | Unfurling Crack | `L` (after l3) | Mid | Light finisher, **wall bounce** | M1 |
| `whipblade.l2h` | Lifting Lash | `L L H` | Mid | **Launcher**, jc | M2 |
| `whipblade.l3h` | Silk Snare | `L L L H` | Mid | Coils and **pulls** the opponent to 1 m; 20-frame stagger | M13 |
| `whipblade.h1` | Long Snap | `H` | Mid | 4.5 m | M1 |
| `whipblade.h2` | Figure Eight | `H` (after h1) | Mid | 2 hits | M1 |
| `whipblade.h3` | Ribbon Fall | `H` (after h2) | Overhead | **Ground bounce** | M1 |
| `whipblade.hcharge` | Spooling | `[H]` | — | 3 levels | M5 |
| `whipblade.hcharged` | Gossamer Storm | `]H[` | Mid | Sweeps the full arena length (6 m); level 3 = **Guard Crush** | M5 |
| `whipblade.fl` | Reaching Snap | `6L` | Mid | 4.8 m; Special-cancelable | M1 |
| `whipblade.bh` | Overhead Crack | `4H` | **Overhead** | 3.5 m | M1 |
| `whipblade.dl` | Ankle Snap | `2L` | **Low** | Hits inside the dead zone | M1 |
| `whipblade.dh` | Floor Ribbon | `2H` | **Low** | Knockdown, 4 m | M1 |
| `whipblade.jl` | Air Flick | `j.L` | Overhead (air) | — | M1 |
| `whipblade.jl2` | Air Snap | `j.L` (after jl) | Overhead (air) | — | M1 |
| `whipblade.jh` | Descending Ribbon | `j.H` | Overhead (air) | **Air spike, ground bounce** | M1 |
| `whipblade.s` | Silk Pull | `5S` / `5S` | Mid | 5 m pull; on hit, drags the opponent to 1 m with a 24-frame stagger | M9 |
| `whipblade.qcfs` | Lash Line | `236S` / `6S` | Mid | Full-arena snap (6 m); tip bonus | M1 |
| `whipblade.dps` | Ribbon Ascent | `623S` / `8S` | Mid | Vertical whirl, anti-air, **launcher**; **no invulnerability** | M6 |
| `whipblade.qcbs` | Cocoon | `214S` / `4S` | Mid | 40-frame whirling barrier: destroys projectiles and hits close opponents | M3 |
| `whipblade.ultimate` | Weaver's Cradle | `Ultimate` | — | Cinematic | M10 |
| `whipblade.execution` | Unspooling | `Execute` | — | Paired Execution | M1 |

### 16.2 Combo trees

```
LIGHT TREE
L ─► L ─► L ─► L        l1 Flick → l2 Snap → l3 Crack → l4 Unfurling Crack (wb)
     │    └─► H         whipblade.l3h Silk Snare (M13, pull)
     └─► H              whipblade.l2h Lifting Lash (launcher)
HEAVY TREE  H ─► H ─► H       Long Snap → Figure Eight → Ribbon Fall (gb)
CHARGED     [H] ─► ]H[        Spooling → Gossamer Storm (L3 guard crush, full arena)
AIR         j.L ─► j.L ─► j.H Air Flick → Air Snap → Descending Ribbon (spike, gb)
```

### 16.3 Ultimate — Weaver's Cradle (`whipblade.ultimate`)

The whip blade leaves Rhen's hand and keeps moving on its own, weaving a cocoon of lantern-lit silk
around the target as paper lanterns rise into the night. Rhen and Sable take opposite ends and spin
the cocoon. With one final pull the silk unravels in a spiral of cuts, and the lanterns burst
overhead like fireworks. **5.5 s.**

### 16.4 Execution — Unspooling (`whipblade.execution`)

Rhen coils the ribbon around the opponent's body, walks one slow circle around them, and pulls.
The ribbon unspools and the opponent spins down to the floor. **2.5 s.** No gore.

### 16.5 Signature routes

| Level | Route | Notes |
|---|---|---|
| Beginner | `236S` from full screen | Lash Line zoning. Learn the tip |
| Beginner | `L L L L (wb)` | The chain walks outward from 0 to 4 m |
| Intermediate | `5S, L L H jc j.L j.L j.H (gb)` | Silk Pull from 5 m into the launcher |
| Intermediate | `6L (tip, CH) > 236S` | Two tip hits across the whole arena |
| Expert | `6L (tip, CH) > 236S, 5S, L L L L (wb), SHADOW, H H H (gb), 623S > ULT` | Full-arena starter to Weaver's Cradle |
| Expert | `214S` (Cocoon) through a projectile → `5S` | Erase the zoning and pull the zoner in |

### 16.6 Matchup notes

- **vs Gauntlets:** Keep them out with `6L`, `2H` and Lash Line. When they get inside, use Cocoon or `l1`/`2L`, never Heavies.
- **vs Katana:** Severing Wind trades with Lash Line. Stillwater's Reprisal only reaches about 2 m, so snap from 3 m+ and use Cocoon when they close in.
- **vs Spear:** You outrange them by about 1 m. Keep them at 4.5 m and Ribbon Ascent their Pole Vault.
- **vs Daggers:** Shadowstep reappears inside your dead zone. Cover it with Cocoon and `l1`.

### 16.7 Mastery track

| Level | Unlock |
|---|---|
| 7 | Trail I: **Lantern Ribbon** |
| 11 | Property I: the Tip Snap zone widens from 0.4 m to 0.55 m |
| 13 | Branch: `whipblade.l3h` Silk Snare |
| 14 | Trail II: **Festival Silk** · Emote: *Ribbon Dance* |
| 15 | Property II: pressing `S` during `whipblade.qcbs` Cocoon throws the barrier forward as a 3 m projectile |
| 17 | Execution variant: *Unspooling — Canal Lanterns* |
| 18 | Property III: `whipblade.s` Silk Pull works on airborne opponents (pulls them down: **ground bounce**) |
| 19 | Skin: **Master's Silken Lash — Paper Moon** |
| 20 | Crest: *Silken Lash Master* · Trail: **Oath-light Whip Blade** |

---

## 17. Arcane Weapons — Umbral Arts (`weapon.arcane`)

**Class fantasy.** Umbral Arts use the stuff of the Undermourn: written, folded or carried in a
lamp. Each sub-form is a full moveset with its own identity, and all three share one mastery track
(`mastery.arcane`). Every mastery unlock applies to all three sub-forms at once.

**Loadout rules.** The sub-form is chosen in the loadout (PvE) or at character select (PvP) and cannot
change during a match or encounter. Between PvP games in a set, the loser may switch sub-form like
any class switch.

**Class trait — Conjuration Limit.** Each sub-form can keep only a limited number of **constructs** (glyphs, fans,
cranes, wisps, tethers) on screen. When the limit is reached, the oldest one is replaced. Every construct
vanishes when the caster is **guard-broken or thrown**, and any strike destroys one unless noted.

| Sub-form | Construct limit |
|---|---|
| Grimoire | 2 glyphs (`grimoire.s`, `grimoire.l3h`) |
| Warfan | 1 thrown fan (`warfan.qcfs`) + 3 cranes (`warfan.qcbs`) |
| Soul Lantern | 2 wisps (`soullantern.s`, `soullantern.l3h`) + 1 tether (`soullantern.qcbs`) |

### 17.1 Grimoire (`weapon.arcane.grimoire`)

**Fantasy.** Knowledge as a weapon. The Grimoire's pages hold ember-verses and shadow-script, and its
bearer fights from across the room, writing traps into the floor and bolts into the air.

| Range | Speed | Damage | Posture | Startup | Difficulty |
|---|---|---|---|---|---|
| 5 | 2 | 3 | 2 | B | ★★★★ |

**Strengths:** the best projectile game, traps, projectile absorption, and strong anti-air pillars.
**Weaknesses:** the weakest melee, slow normals, constructs that vanish on a guard break or throw, and weakness against rushdown.

| ID | Name | Input (Classic / Simplified) | Height | Properties | Unlock |
|---|---|---|---|---|---|
| `grimoire.l1` | Page Cut | `L` | Mid | Book-edge strike | M1 |
| `grimoire.l2` | Page Turn | `L` (after l1) | Mid | — | M1 |
| `grimoire.l3` | Spine Strike | `L` (after l2) | Mid | — | M1 |
| `grimoire.l4` | Closing Verse | `L` (after l3) | Mid | Script burst, Light finisher, **wall bounce** | M1 |
| `grimoire.l2h` | Rising Script | `L L H` | Mid | **Launcher**, jc | M2 |
| `grimoire.l3h` | Marginal Glyph | `L L L H` | Mid (trap) | Plants a glyph at the opponent's feet that detonates after 40 frames and **launches** | M13 |
| `grimoire.h1` | Umbral Bolt | `H` | Mid (projectile) | 1 bolt | M1 |
| `grimoire.h2` | Twin Bolt | `H` (after h1) | Mid (projectile) | 2 bolts | M1 |
| `grimoire.h3` | Collapse Sigil | `H` (after h2) | Mid | Close-range burst, **ground bounce** | M1 |
| `grimoire.hcharge` | Inscribing | `[H]` | — | 3 levels | M5 |
| `grimoire.hcharged` | Black Folio Lance | `]H[` | Mid (beam) | 6 m beam; level 3 = **Guard Crush** | M5 |
| `grimoire.fl` | Marginalia | `6L` | Mid | Page-blade poke, 2 m | M1 |
| `grimoire.bh` | Falling Glyph | `4H` | **Overhead** | A glyph drops from above at 2.5 m | M1 |
| `grimoire.dl` | Floor Script | `2L` | **Low** | Script crawls 2.5 m along the floor | M1 |
| `grimoire.dh` | Rune Trip | `2H` | **Low** | Knockdown, 3 m | M1 |
| `grimoire.jl` | Air Page | `j.L` | Overhead (air) | — | M1 |
| `grimoire.jh` | Descending Glyph | `j.H` | Overhead (air projectile) | **Air spike, ground bounce** | M1 |
| `grimoire.s` | Sealing Circle | `5S` / `5S` | Mid (trap) | Trap glyph 2 m ahead; on contact it roots the target (PvE 40 frames, PvP 30 frames) | M9 |
| `grimoire.qcfs` | Ember Verse | `236S` / `6S` | Mid (projectile) | Fireball | M1 |
| `grimoire.jqcfs` | Falling Verse | `j.236S` / `j.6S` | Overhead (projectile) | Ember Verse angled downward | M3 |
| `grimoire.dps` | Pillar Script | `623S` / `8S` | Mid | Shadow pillar erupts 3 m ahead; anti-air **launcher** | M6 |
| `grimoire.qcbs` | Erasure | `214S` / `4S` | — | A turning page absorbs projectiles for 30 frames; after an absorb, the next Ember Verse is empowered (double size, 2 hits) | M3 |
| `grimoire.ultimate` | The Last Chapter | `Ultimate` | — | Cinematic | M10 |
| `grimoire.execution` | Final Entry | `Execute` | — | Paired Execution | M1 |

```
LIGHT  L ─► L ─► L ─► L   Page Cut → Page Turn → Spine Strike → Closing Verse (wb)
            │    └─► H    grimoire.l3h Marginal Glyph (M13)
            └─► H         grimoire.l2h Rising Script (launcher)
HEAVY  H ─► H ─► H        Umbral Bolt → Twin Bolt → Collapse Sigil (gb)
CHARGED [H] ─► ]H[        Inscribing → Black Folio Lance
AIR    j.L ─► j.H         Air Page → Descending Glyph (spike, gb)
```

**Ultimate — The Last Chapter (`grimoire.ultimate`).** The grimoire flies open and its pages storm
out into a spiral library around the target. Each page shows a moment of the target's life, and Rhen
reads them aloud while Sable tears them out one by one. The final page is blank. Rhen writes a single
word on it in Ember and slams the book shut, and the target is pressed between the pages in a flash of
Oath-light. **6.0 s.**

**Execution — Final Entry (`grimoire.execution`).** Rhen opens the grimoire in front of the
staggered opponent. Script crawls up their arms, and when Rhen claps the book shut they collapse as
their name is written. **2.5 s.**

| Level | Route | Notes |
|---|---|---|
| Beginner | `236S`, `236S` | Ember Verse zoning |
| Beginner | `L L L L (wb)` | Closing Verse wall bounce |
| Intermediate | `5S` then `236S` | Trap plus fireball: jumping over the fireball lands on the glyph |
| Intermediate | `L L H jc j.L j.H (gb), 623S` | Rising Script into Pillar Script relaunch |
| Expert | (Sealing Circle root) `L L L L (wb), SHADOW, H H H (gb), 236S, 623S > ULT` | Echoed bolts and pillars into The Last Chapter |
| Expert | `214S` absorb → empowered `236S` → `L L L H` | Erasure counter-zoning into a Marginal Glyph (M13) mine |

### 17.2 Warfan (`weapon.arcane.warfan`)

**Fantasy.** Silk, steel ribs and a breath of wind. The Warfan dancer cuts with folded edges,
throws blades that come home, and fills the air with paper cranes that bite.

| Range | Speed | Damage | Posture | Startup | Difficulty |
|---|---|---|---|---|---|
| 3 | 4 | 2 | 2 | A | ★★★ |

**Strengths:** fast mid-range normals, a returning projectile that hits twice, homing cranes for pressure, and an evasive stance.
**Weaknesses:** low damage, weaker normals while the fan is thrown, and fragility against armor.

| ID | Name | Input (Classic / Simplified) | Height | Properties | Unlock |
|---|---|---|---|---|---|
| `warfan.l1` | Fan Flick | `L` | Mid | — | M1 |
| `warfan.l2` | Fan Return | `L` (after l1) | Mid | — | M1 |
| `warfan.l3` | Opening Fan | `L` (after l2) | Mid | 2 hits | M1 |
| `warfan.l4` | Gale Fold | `L` (after l3) | Mid | Light finisher, **wall bounce** | M1 |
| `warfan.l2h` | Updraft | `L L H` | Mid | **Launcher**, jc | M2 |
| `warfan.l3h` | Twin Fold Toss | `L L L H` | Mid (construct) | Both fans are tossed 1.5 m and spin in place for 30 frames (multi-hit) | M13 |
| `warfan.h1` | Iron Rib | `H` | Mid | — | M1 |
| `warfan.h2` | Double Rib | `H` (after h1) | Mid | 2 hits | M1 |
| `warfan.h3` | Closing Gust | `H` (after h2) | Mid | Gust slam, **ground bounce** | M1 |
| `warfan.hcharge` | Wind Gathering | `[H]` | — | 3 levels | M5 |
| `warfan.hcharged` | Typhoon Fan | `]H[` | Mid | Gust pushes 3 m; level 3 = **Guard Crush** | M5 |
| `warfan.fl` | Leaping Fold | `6L` | Mid | Advancing hop | M1 |
| `warfan.bh` | Falling Fan | `4H` | **Overhead** | — | M1 |
| `warfan.dl` | Low Flick | `2L` | **Low** | Special-cancelable | M1 |
| `warfan.dh` | Sweeping Fold | `2H` | **Low** | Knockdown | M1 |
| `warfan.jl` | Air Fold | `j.L` | Overhead (air) | — | M1 |
| `warfan.jl2` | Air Fold Return | `j.L` (after jl) | Overhead (air) | — | M1 |
| `warfan.jh` | Downdraft | `j.H` | Overhead (air) | **Air spike, ground bounce** | M1 |
| `warfan.s` | Turning Screen | `5S` / `5S` | — | Evasive twirl: deflects projectiles and is projectile-invulnerable f4–24; counters high/mid strikes | M9 |
| `warfan.sriposte` | Screen Riposte | Automatic on a counter | Mid | **Crumple** | M9 |
| `warfan.qcfs` | Returning Fan | `236S` / `6S` | Mid (projectile) | Thrown fan hits going out and coming back. While it is out, Warfan normals use one fan (−10% damage) | M1 |
| `warfan.dps` | Rising Gale | `623S` / `8S` | Mid | Updraft, **launcher**, invulnerable to air attacks f1–8 | M6 |
| `warfan.qcbs` | Paper Cranes | `214S` / `4S` | Mid (constructs) | Releases 3 crane blades that hover, then home in one at a time | M3 |
| `warfan.ultimate` | Crane Requiem | `Ultimate` | — | Cinematic | M10 |
| `warfan.execution` | Folded Farewell | `Execute` | — | Paired Execution | M1 |

```
LIGHT  L ─► L ─► L ─► L   Fan Flick → Fan Return → Opening Fan → Gale Fold (wb)
            │    └─► H    warfan.l3h Twin Fold Toss (M13)
            └─► H         warfan.l2h Updraft (launcher)
HEAVY  H ─► H ─► H        Iron Rib → Double Rib → Closing Gust (gb)
CHARGED [H] ─► ]H[        Wind Gathering → Typhoon Fan
AIR    j.L ─► j.L ─► j.H  Air Fold → Air Fold Return → Downdraft (spike, gb)
STANCE 5S ─ counter ─► warfan.sriposte Screen Riposte
```

**Ultimate — Crane Requiem (`warfan.ultimate`).** Rhen snaps both fans open and hundreds of paper
cranes burst out, each one inscribed with the name of a tithed soul. They circle the target while
Sable conducts them like a choir. On Rhen's signal they dive in waves, and the last crane lands on
the target's shoulder and ignites. **5.5 s.**

**Execution — Folded Farewell (`warfan.execution`).** Rhen folds a fan shut against the opponent's
throat, spins behind them while opening it in one motion, and lets the silk fall over their face
like a shroud. **2.5 s.**

| Level | Route | Notes |
|---|---|---|
| Beginner | `L L L L (wb)` | Gale Fold wall bounce |
| Beginner | `236S` then walk in behind the returning fan | Returning Fan covers the approach |
| Intermediate | `214S, 236S, 66, L L H jc j.L j.L j.H (gb)` | Cranes and fan pressure into the launcher |
| Intermediate | `5S` counter → `L L L L (wb)` | Screen Riposte crumple into the chain |
| Expert | `CH 6L, L L H jc j.L j.L j.H (gb), 236S, 623S > ULT` | Returning Fan's return hit extends the juggle |
| Expert | `L L L H` (fans spinning) `, 214S, 4H/2L` | Twin Fold Toss (M13) holds them in place for a crane-covered high/low |

### 17.3 Soul Lantern (`weapon.arcane.soullantern`)

**Fantasy.** A lantern on a chain, and inside it the lost. The Soul Lantern bearer calls tithed
souls to fight beside them, swings the lantern like a flail, and tethers enemies so that every
wound is shared.

| Range | Speed | Damage | Posture | Startup | Difficulty |
|---|---|---|---|---|---|
| 3 | 2 | 3 | 3 | B | ★★★★★ |

**Strengths:** summons create pressure from two directions, the tether controls space, flail arcs are wide, and the class combines well with Umbral Shadow.
**Weaknesses:** slow, construct management is demanding, there is no invincible reversal, and wisps vanish if Rhen is thrown or guard-broken.

| ID | Name | Input (Classic / Simplified) | Height | Properties | Unlock |
|---|---|---|---|---|---|
| `soullantern.l1` | Lantern Swing | `L` | Mid | — | M1 |
| `soullantern.l2` | Wick Swing | `L` (after l1) | Mid | — | M1 |
| `soullantern.l3` | Tether Arc | `L` (after l2) | Mid | 2 hits | M1 |
| `soullantern.l4` | Flare | `L` (after l3) | Mid | Soulfire burst, Light finisher, **wall bounce** | M1 |
| `soullantern.l2h` | Rising Flame | `L L H` | Mid | **Launcher**, jc | M2 |
| `soullantern.l3h` | Wick Flare | `L L L H` | Mid | The lantern spits out a wisp mid-string (counts toward the wisp limit) | M13 |
| `soullantern.h1` | Soul Bash | `H` | Mid | — | M1 |
| `soullantern.h2` | Soul Toss | `H` (after h1) | Mid | Lantern thrown on its chain, 3 m | M1 |
| `soullantern.h3` | Lantern Drop | `H` (after h2) | Overhead | **Ground bounce** | M1 |
| `soullantern.hcharge` | Kindling Souls | `[H]` | — | 3 levels | M5 |
| `soullantern.hcharged` | Pyre Bloom | `]H[` | Mid | 2.5 m area around Rhen; level 3 = **Guard Crush** | M5 |
| `soullantern.fl` | Wick Lunge | `6L` | Mid | Special-cancelable | M1 |
| `soullantern.bh` | Hanging Lantern | `4H` | **Overhead** | — | M1 |
| `soullantern.dl` | Cinder Skim | `2L` | **Low** | — | M1 |
| `soullantern.dh` | Ground Flame | `2H` | **Low** | Knockdown | M1 |
| `soullantern.jl` | Air Swing | `j.L` | Overhead (air) | — | M1 |
| `soullantern.jh` | Falling Lantern | `j.H` | Overhead (air) | **Air spike, ground bounce** | M1 |
| `soullantern.s` | Call the Lost | `5S` / `5S` | Mid (construct) | Summons a wisp (max 2) that floats behind Rhen and dashes at the opponent 45 frames later; wisps last 360 frames | M9 |
| `soullantern.qcfs` | Wisp Volley | `236S` / `6S` | Mid (projectile) | Soulfire projectile | M1 |
| `soullantern.dps` | Beacon | `623S` / `8S` | Mid | Pillar of light from the lantern, anti-air, **launcher**; **no invulnerability** | M6 |
| `soullantern.qcbs` | Soul Tether | `214S` / `4S` | Mid | 3.5 m tether. On hit, it links both fighters for 240 frames, and moving beyond 3.5 m yanks the opponent back (20-frame stagger). PvE: 10% of damage dealt to the tethered target is drained | M3 |
| `soullantern.ultimate` | Procession of the Unreborn | `Ultimate` | — | Cinematic | M10 |
| `soullantern.execution` | Snuffing the Wick | `Execute` | — | Paired Execution | M1 |

```
LIGHT  L ─► L ─► L ─► L   Lantern Swing → Wick Swing → Tether Arc → Flare (wb)
            │    └─► H    soullantern.l3h Wick Flare (M13, wisp)
            └─► H         soullantern.l2h Rising Flame (launcher)
HEAVY  H ─► H ─► H        Soul Bash → Soul Toss → Lantern Drop (gb)
CHARGED [H] ─► ]H[        Kindling Souls → Pyre Bloom
AIR    j.L ─► j.H         Air Swing → Falling Lantern (spike, gb)
```

**Ultimate — Procession of the Unreborn (`soullantern.ultimate`).** The lantern opens and a
procession of tithed souls walks out, robed figures each carrying a small lantern of their own.
They pass through the target one by one, and each takes a little of its warmth. Sable walks at the
end of the line. The last soul turns, and it is the target's own shadow, which bows and walks away
with the others. **6.0 s.**

**Execution — Snuffing the Wick (`soullantern.execution`).** Rhen swings the lantern up under the
opponent's chin, catches it, and closes its shutter. The opponent's shadow is drawn into the flame,
which gutters and goes out. **2.5 s.**

| Level | Route | Notes |
|---|---|---|
| Beginner | `L L L L (wb)` | Flare wall bounce |
| Beginner | `5S`, then `236S` | Wisp plus projectile from two angles |
| Intermediate | `5S, 5S, 214S, L L H jc j.L j.H (gb)` | Two wisps and a tether, then the launcher. Wisps strike during the juggle |
| Intermediate | `6L > 214S` | Poke into tether: they cannot escape to range |
| Expert | `5S, 5S, SHADOW, L L L L (wb), H H H (gb), 623S > ULT` | The wisps strike during the wall bounce, and every hit echoes |
| Expert | `L L L H` (wisp), `2L` / `4H` | Wick Flare (M13) wisp covers a high/low mixup from behind |

### 17.4 Arcane matchup notes

- **Grimoire vs rushdown (Gauntlets, Daggers):** Pre-place Sealing Circle before they arrive, and never rely on Erasure up close.
- **Warfan vs armor (Nodachi, War Hammer):** Armor walks through single fan hits. Use cranes to strip the armor, then punish.
- **Soul Lantern vs Whip Blade:** Their zoning destroys wisps (1 strike each). Tether them first, then summon.
- **All sub-forms vs Naginata/Staff:** Whirlpool Guard, Wheeling Branch and Gust Palm erase projectiles. Win with melee and constructs instead.

### 17.5 Arcane mastery track (shared; each unlock applies to all three sub-forms)

| Level | Grimoire | Warfan | Soul Lantern |
|---|---|---|---|
| 7 | Trail I: **Inkwash** | Trail I: **Crane Silk** | Trail I: **Wisplight** |
| 11 | `grimoire.qcfs` Ember Verse can be held (`[S]`) to delay release by up to 20 frames | `warfan.qcfs` Returning Fan can be recalled early (`S` again) | Wisps last 480 frames |
| 13 | Branch `grimoire.l3h` Marginal Glyph | Branch `warfan.l3h` Twin Fold Toss | Branch `soullantern.l3h` Wick Flare |
| 14 | Trail II: **Drowned Ink** · Emote: *Arcane Flourish* | Trail II: **Paper Storm** · Emote: *Arcane Flourish* | Trail II: **Procession Glow** · Emote: *Arcane Flourish* |
| 15 | `grimoire.qcbs` Erasure **reflects** a projectile absorbed in its first 6 frames | `warfan.qcbs` Paper Cranes can all be launched at once (`S` again) | `soullantern.qcbs` Soul Tether can be detonated (`S` again) for an instant pull |
| 17 | Execution variant: *Final Entry — Sunken Script* | Execution variant: *Folded Farewell — Nightsilk* | Execution variant: *Snuffing the Wick — Undermourn Blue* |
| 18 | Holding `4`/`6` during `grimoire.dps` Pillar Script places it at 2 m / 4 m | `warfan.s` Turning Screen can be performed in the air | `soullantern.dps` Beacon gains 1-hit armor |
| 19 | Skin: **Master's Codex — Drowned Folio** | Skin: **Master's Warfan — Nightsilk Ribs** | Skin: **Master's Lantern — Procession Lamp** |
| 20 | Crest: *Umbral Arts Master* · Trail: **Oath-light Arcana** (all sub-forms) | ← | ← |

---

## 18. Cross-class balance guardrails

| Guardrail | Target |
|---|---|
| Ranked win rate per class (all tiers, Full Kit) | 47–53% |
| Ranked win rate per class in Oathlord tier and above | 45–55% |
| Minimum pick rate per class (and per Arcane sub-form) | 3% |
| Maximum pick rate per class | 18% |
| Optimal meterless combo damage (PvP) | 20–28% of normalized HP |
| Full-meter combo with Ultimate (PvP) | 40–48%, hard cap 50% |
| Invincible reversals (`.dps` with invulnerability: Katana, Gauntlets) | Must be ≤ −20 on block and give a Punish Counter on whiff |
| Command grabs (Gauntlets `.s`, `.qcbs`) | ≥ 10 frames startup, ≥ 40 frames whiff recovery, violet telegraph |
| Projectiles | Every class has at least one tool that beats projectiles (a parry always works; a class tool is additional) |
| Matchups | No matchup worse than 40/60 by win rate at Oathlord tier over a season; anything outside that range gets a data-only balance patch within 2 weeks |

Balance changes are **data-only patches** to move JSON. They never change a move ID, and they are listed in the
in-game patch notes with the old and new frame values.

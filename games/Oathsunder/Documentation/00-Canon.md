# OATHSUNDER — Project Canon

> **Status:** Authoritative. Every document, data file, asset name and line of code in this
> project must agree with this file. If something here is wrong, change it *here first*, then
> propagate. Anything not listed here is not canon yet.

---

## 1. Identity

| Key | Value |
|---|---|
| Title | **OATHSUNDER** |
| Tagline | *Every oath has a shadow.* |
| Genre | 2.5D cinematic action fighting RPG |
| Platforms | Android, iOS, Windows, macOS, Steam Deck |
| Engine | Unity 6 LTS, URP |
| Code namespace root | `Oathsunder` |
| Studio codename | `OS` (used only in build/CI identifiers, never in code namespaces) |
| Ratings target | PEGI 16 / ESRB M (stylized violence, no gore dismemberment on mobile default) |
| Business model | Premium campaign + optional cosmetic store. **No pay-to-win.** Ranked PvP uses normalized stats. |

---

## 2. World

**Varanth** is a continent ruled for five hundred years by **the Solemn Throne**. The first emperor,
**Aurem**, sealed **the Undermourn** (the drowned realm where shadows go at death) by swearing
**the Twelvefold Oath**. The Oath is anchored in twelve **Oathstones**, each held by an
**Oathlord** who rules one region.

The Oath's hidden price: at death every person's shadow is **tithed** to the seal. Tithed souls are
never reborn. The Oath is now failing and severed shadows — **the Unsworn** — are clawing their way
up into the waking world.

### 2.1 Energies (gameplay + lore)

| Term | Meaning | Gameplay |
|---|---|---|
| **Ember** | Life-force; warmth of the living | Rage meter / **Ember Rage** mode, fire & light elements |
| **Umbra** | Shadow-force; the stuff of the Undermourn | Shadow meter / **Umbral Shadow** mode, shadow & void elements |
| **Oath-light** | Power stored in Oathstones | Ultimate meter, legendary rewards |

### 2.2 Factions

| Faction | Description |
|---|---|
| **The Solemn Throne** | The empire. Bureaucratic, devout, terrified. |
| **Oathwardens** | Knight order that guards the Oathstones. Rhen's former order. |
| **The Unsworn** | Severed shadows. Most are feral; some remember who they were. |
| **The Ashen Choir** | Emberfall monks who perform the tithe rites. |
| **Lantern Guild** | Merchants and smugglers of Lanternhold; neutral, greedy, useful. |
| **The Rootbound** | Fungal cult of the Verdant Rot; worship Queen Myrrhen. |
| **Free Blades** | Mercenary companies; source of clans and arena contracts. |

---

## 3. Protagonist and companions

| ID | Name | Role |
|---|---|---|
| `char.rhen` | **Rhen** | Protagonist. Former Oathwarden, executed for refusing to tithe a living child's shadow. Returned from the Undermourn "Oathsundered": bound by no oath, able to wield Ember and Umbra together. Default weapon: Katana. |
| `char.sable` | **Sable** | Rhen's severed shadow. Sardonic voice in his head, physical partner in Shadow mode. Can become an antagonist (Sable Ascendant). |
| `char.liss` | **Liss** | The child whose shadow came loose early. Key to the true ending. |
| `char.mireth` | **Mireth Vale** | Disgraced Oathwarden archivist. Runes, enchantments, lore. |
| `char.tessen` | **Tessen** | Old forge-master. Blacksmith, crafting, weapon mastery. |
| `char.adekan` | **Brother Adekan** | Emberfall monk who defects. Trainer; hosts Training mode and the Dojo. |
| `char.oskar` | **Oskar Dray** | Free Blade captain. Arena, Survival, Clan contracts. |
| `char.neve` | **Neve** | Lantern Guild trader. Cosmetics, collectibles, weekly events. |

---

## 4. Regions (12) and main bosses — the Oathlords

Region order is campaign order. Each region has **10 story missions** (120 total) plus side content.

| # | Region ID | Region | Biome / mood | Level | Oathlord (boss ID) | Title | Signature weapon (class) | Oathstone |
|---|---|---|---|---|---|---|---|---|
| 1 | `region.emberfall` | **Emberfall Monastery** | Mountain monastery, ash-snow at dusk | 1–5 | **Abbot Kessh** (`boss.kessh`) | the Ash Censer | *Censer of Last Rites* (Chain Sword) | Devotion |
| 2 | `region.weepingreeds` | **The Weeping Reeds** | Drowned saltmarsh villages, rain, fog | 5–10 | **Mother Ilvane** (`boss.ilvane`) | the Drowned Bell | *Tidebell* (Naginata) | Mercy |
| 3 | `region.ironroot` | **Ironroot Forge** | Volcanic forge-city, molten rivers | 10–15 | **Gorran Vox** (`boss.gorran`) | the Anvil King | *Worldanvil* (War Hammer) | Labor |
| 4 | `region.silkwind` | **Silkwind Groves** | Bamboo and silk forest, wind, petals | 15–20 | **Ysolde & Yrrah** (`boss.petals`) | the Paired Petals | *Thorn & Bloom* (Dual Blades) | Kinship |
| 5 | `region.glassossuary` | **The Glass Ossuary** | Desert of fused glass and giant bones | 20–25 | **Seraph Maal** (`boss.maal`) | the Sunscorched | *Noonpiercer* (Spear) | Faith |
| 6 | `region.rimewood` | **Rimewood Steppe** | Frozen steppe, aurora, blizzards | 25–30 | **Tharuk Greymane** (`boss.tharuk`) | the Winter Wolf | *Fangs of the Long Night* (Gauntlets) | Strength |
| 7 | `region.lanternhold` | **Lanternhold** | Canal city, paper lanterns, eternal festival night | 30–35 | **Duke Veloran Sae** (`boss.veloran`) | the Masquerade | *Silken Lash* (Whip Blade) | Loyalty |
| 8 | `region.verdantrot` | **The Verdant Rot** | Bioluminescent fungal jungle | 35–40 | **Queen Myrrhen** (`boss.myrrhen`) | the Mycelial Bride | *Bridal Reaper* (Scythe) | Life |
| 9 | `region.cloudspire` | **Cloudspire Aqueducts** | Floating temple bridges, storms | 40–45 | **Master Oru** (`boss.oru`) | the Still Wind | *Unmoving Branch* (Staff) | Discipline |
| 10 | `region.sunkenarchive` | **The Sunken Archive** | Underwater library inside an air-dome | 45–50 | **Archivist Quill** (`boss.quill`) | the Thousand Pages | *Codex Umbrae* (Arcane — Grimoire) | Memory |
| 11 | `region.bloodmoon` | **Bloodmoon Citadel** | War-torn capital fortress under a red moon | 50–55 | **General Hask Varrow** (`boss.hask`) | the Crimson Vow | *Vowcleaver* (Nodachi) | Duty |
| 12 | `region.solemnthrone` | **The Solemn Throne** | Imperial palace over the Undermourn Gate | 55–60 | **Emperor Aurem** (`boss.aurem`) | the Solemn Sun | *First Oathblade* (Katana) | Sovereignty |

## 5. Legendary bosses — the Unsworn and beyond (12)

| # | Boss ID | Name | Title | Signature weapon (class) | Where / how |
|---|---|---|---|---|---|
| 13 | `boss.tamsin` | **Tamsin** | of the Nine Knives | *Ninefold Fang* (Daggers) | Recurring assassin rival, regions 2–7; final duel on Lanternhold rooftops |
| 14 | `boss.sableascendant` | **Sable Ascendant** | the Other You | *Umbral Oathblade* (Katana, shifts to any mastered class) | Mirror duel; secret ending path |
| 15 | `boss.ferrousmaw` | **The Ferrous Maw** | Engine of Ironroot | *Crucible Fists* (Gauntlets) | **Raid** boss |
| 16 | `boss.corvaine` | **Lady Corvaine** | the Ninth Widow | *Mourning Fans* (Arcane — Warfan) | Lanternhold hidden duel |
| 17 | `boss.ossric` | **Ossric** | the Grave-Warden | *Last Harvest* (Scythe) | Glass Ossuary hidden tomb |
| 18 | `boss.drownedchoir` | **The Drowned Choir** | Three Voices, One Throat | *Anchor Hymn* (Chain Sword) | **Raid** boss |
| 19 | `boss.velka` | **Iron Abbess Velka** | the Unquenched | *Penitent Maul* (War Hammer) | Ironroot hidden foundry |
| 20 | `boss.eirmund` | **Eirmund** | the Pale Stag | *Antler Lance* (Spear) | Rimewood hidden grove |
| 21 | `boss.senajari` | **Sen Ajari** | the Lotus Hermit | *Lotus Rod* (Staff) | Cloudspire summit; Oru's master |
| 22 | `boss.isketh` | **Isketh** | the Marrow Queen | *Spinecoil* (Whip Blade) | Verdant Rot hidden hive |
| 23 | `boss.hundredhanded` | **The Hundred-Handed Warden** | Keeper of the Gate | *Hundred Edges* (Dual Blades) | **Raid** boss, Undermourn |
| 24 | `boss.firstshadow` | **The First Shadow** | Umbra Prime | *Oathsunder* (Nodachi) | True final boss (Rewoven Oath ending) |

Every boss has: lore entry, signature weapon, intro cinematic, **three combat phases**, unique AI
personality profile, exclusive theme, custom arena, legendary rewards, exclusive execution.

---

## 6. Weapon classes (13)

| Class ID | Class | In-world style name | Identity (one line) |
|---|---|---|---|
| `weapon.katana` | Katana | **Oathblade Style** | Balanced, fast, parry-centric; draw-cut counters |
| `weapon.nodachi` | Nodachi | **Longvow Style** | Huge reach, slow, charge attacks, armor |
| `weapon.dualblades` | Dual Blades | **Twin Petal Style** | Highest hit rate, long strings, weak posture damage |
| `weapon.spear` | Spear | **Pierce Line Style** | Spacing and pokes, pole-vault mobility |
| `weapon.naginata` | Naginata | **Tidewheel Style** | Sweeping arcs, low control, spinning momentum |
| `weapon.staff` | Staff | **Still Wind Style** | Juggle and air combos, pole-spins, counters |
| `weapon.scythe` | Scythe | **Harvest Style** | Pull-in hooks, cross-ups, drain |
| `weapon.warhammer` | War Hammer | **Anvil Style** | Guard-crushing, ground bounce, hyper armor |
| `weapon.gauntlets` | Gauntlets | **Iron Fang Style** | Close-range rushdown, grabs, fastest startup |
| `weapon.daggers` | Daggers | **Ninefold Style** | Teleport feints, bleed, throwing knives |
| `weapon.chainsword` | Chain Sword | **Censer Chain Style** | Extending segmented blade, mid-to-long range whips |
| `weapon.whipblade` | Whip Blade | **Silken Lash Style** | Longest reach, zoning, snaps and pulls |
| `weapon.arcane` | Arcane Weapons | **Umbral Arts** | Projectiles, traps, summons; sub-forms Grimoire, Warfan, Soul Lantern |

Every class ships with: Light combo tree, Heavy combo tree, Air combo, Charged attack, Special skill,
Ultimate ability, Mastery progression.

---

## 7. Combat canon (summary — the code is authoritative for frame-level numbers)

### 7.1 Buttons (logical)

`Light`, `Heavy`, `Special`, `Guard`, `Dodge`, `Jump`, `Grab`, `Execute`, `Ultimate`, `Rage`, `Shadow`.
Directions use **numpad notation** relative to facing (`6` = forward, `4` = back, `2` = down …).

### 7.2 Mechanics

| Mechanic | Canon rule |
|---|---|
| Simulation rate | 60 ticks/s, fixed-point deterministic; rendering is unlocked (up to 240 FPS) and interpolated |
| Input buffer | Presses stay valid for **8** of the fighter's own frames; hitstop does not age the buffer |
| Guard | Dedicated `Guard` button. Standing guard blocks high/mid/overhead; crouch guard (`Guard`+`2`) blocks high/mid/low |
| Perfect Parry | Pressing `Guard` ≤ **6** frames before contact. Mashing shortens the window to **2** frames |
| Perfect Dodge | Dodge whose *perfect window* overlaps an attack → **Shadow Time** (attacker slowed) + Shadow meter |
| Posture | Blocking and being parried fill posture. Full posture → **Guard Break** (stagger) → Execution opportunity |
| Hitstop | Both fighters freeze on contact (light 8, heavy 12, ultimate 16 frames typical) |
| Juggles | Juggle points cap air combos; gravity scales with combo length; wall and ground bounce once per combo |
| Ember Rage | Rage meter full → `Rage`: 8 s, +20% damage, heavy attacks gain 1-hit armor |
| Umbral Shadow | Shadow meter full → `Shadow`: 6 s, every hit spawns a delayed **shadow echo** (Sable strikes again) |
| Ultimate | Ultimate meter full → weapon's Ultimate, a cinematic paired attack |
| Execution | Target guard-broken, or ≤ 15% health and staggered → `Execute` in range → paired execution |
| Finisher | Round-ending Execution or Ultimate triggers the slow-motion cinematic finisher |

---

## 8. Endings (4)

| Ending ID | Name | Condition |
|---|---|---|
| `ending.solemndawn` | **The Solemn Dawn** | Restore ≥ 9 Oathstones. Rhen takes the Throne; the cycle continues. |
| `ending.unboundnight` | **The Unbound Night** | Sunder ≥ 9 Oathstones. All oaths break; the dead walk free. |
| `ending.rewovenoath` | **The Rewoven Oath** (true) | Balanced choices + Liss's questline + all 12 Oath Fragments → defeat The First Shadow |
| `ending.sableascendant` | **Sable Ascendant** (secret) | Yield to Sable in the mirror duel |

---

## 9. Naming conventions (mandatory)

### 9.1 Code

| Item | Convention | Example |
|---|---|---|
| Namespaces | `Oathsunder.<Layer>.<Feature>` | `Oathsunder.Combat.Simulation` |
| Types, methods, properties | PascalCase | `HitResolver.ResolveContact` |
| Private fields | `_camelCase` | `_eventBuffer` |
| Constants / static readonly | PascalCase | `MaxFighters` |
| Interfaces | `I` prefix | `ICombatEventListener` |
| Assembly definitions | `Oathsunder.<Layer>[.<Feature>]` | `Oathsunder.Combat` |
| Test assemblies | `Oathsunder.Tests.<Mode>` | `Oathsunder.Tests.EditMode` |

### 9.2 Content IDs (data)

Lowercase, dot-separated, stable forever once shipped (save games and replays reference them).

`weapon.katana`, `katana.l1`, `universal.dash`, `boss.kessh`, `region.emberfall`, `char.rhen`, `fighter.rhen`.

### 9.3 Asset prefixes

| Prefix | Asset type | Example |
|---|---|---|
| `SK_` | Skeletal mesh | `SK_Rhen_Base` |
| `SM_` | Static mesh | `SM_Emberfall_Bell_01` |
| `A_` | Animation clip | `A_Katana_L1` |
| `AC_` / `AO_` | Animator controller / override | `AC_Humanoid_Combat` |
| `M_` / `MI_` | Material / material variant | `MI_Rhen_Armor_Ash` |
| `T_` | Texture (+ `_BC`, `_N`, `_MRAO`, `_E` suffix) | `T_Rhen_Armor_N` |
| `SG_` | Shader Graph | `SG_Character_Lit` |
| `VFX_` | Visual effect / particle prefab | `VFX_Hit_Katana_Heavy` |
| `SFX_` | Sound effect | `SFX_Katana_Swing_Light_01` |
| `MUS_` | Music stem | `MUS_Boss_Kessh_P2_Drums` |
| `VO_` | Voice line | `VO_Sable_Taunt_03` |
| `UI_` | UI sprite / document | `UI_HUD_HealthBar` |
| `PF_` | Prefab | `PF_Fighter_Rhen` |
| `SC_` | Scene | `SC_Arena_Emberfall_Belltower` |
| `DA_` | Data asset (ScriptableObject) | `DA_Fighter_Rhen` |

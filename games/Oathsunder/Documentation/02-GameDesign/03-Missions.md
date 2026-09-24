# OATHSUNDER — Missions

> **Phase 2 · Game Design Document · Missions**
> Source of truth: [`../00-Canon.md`](../00-Canon.md). The narrative context (factions, regions,
> endings, questlines) is in [`02-World-and-Narrative.md`](02-World-and-Narrative.md), the cast in
> [`04-Characters.md`](04-Characters.md), and boss detail in [`05-Bosses.md`](05-Bosses.md).

---

## 0. Conventions

| Rule | Detail |
|---|---|
| Count | **120 story missions**: 10 per region, in canon region order. Finale sequences (`finale.*`) and raids (`raid.*`) are listed separately and are not part of the 120. |
| Story mission ID | `mission.<regionshort>.<nn>`, where `<regionshort>` is the part of the canon region ID after `region.` (e.g. `region.weepingreeds` gives `mission.weepingreeds.06`). |
| Side content ID | `side.<regionshort>.<nn>` for side missions, `quest.<companion>.<nn>` for companion questline steps (defined in `02-World-and-Narrative.md` §10), and `raid.<bossshort>` for raids. |
| Mission 10 | Always the region's Oathlord boss, followed by the **Oath Choice** (Restore / Sunder) for that region's Oathstone. |
| Tamsin | **[Tamsin I–V]** marks the recurring encounters in regions 2–6, where she withdraws at a set HP. **[Tamsin VI]** is the full three-phase boss duel on the Lanternhold rooftops (`mission.lanternhold.08`, `boss.tamsin`). |
| Unlock markers | **Hidden duel unlocked** and **Raid discovered** mark where canon's legendary bosses become available. |
| Prologue | `mission.emberfall.01`–`03` form the Prologue: Rhen's refusal, execution and return. |

### 0.1 Mission types

| Type | Definition |
|---|---|
| **Duel** | A one-on-one fight against a named opponent, usually best of one with a mid-fight `clash` exchange. |
| **Gauntlet** | A continuous run through a sequence of arenas with no full heal between fights. Ends at a gate, a duel or a set piece. |
| **Boss** | A three-phase named boss with a custom arena, intro cinematic and exclusive execution. |
| **Survival Wave** | Hold a position through timed waves. Arena hazards escalate with each wave. |
| **Stealth-Duel** | Get through an area unseen. Silent takedowns are paired executions on unaware enemies, and being spotted turns into duels with reinforcements. It ends in a duel. |
| **Escort-Duel** | Keep an NPC alive while fighting. The NPC has a guard meter, and the player can intercept attacks aimed at them. |
| **Trial** | A combat challenge with a rule constraint (only parries, no dodge, no killing, sound-only). Teaches or tests one skill. |
| **Chase** | A scrolling pursuit or escape with moving platforms. Fights happen on the move and end at a catch point. |
| **Cinematic** | A story set piece with light interactivity (camera, dialogue, `challenge` or `mercy` nodes). |

### 0.2 Type distribution (story missions)

| Type | Count |
|---|---|
| Boss | 13 (12 Oathlords + Tamsin VI) |
| Duel | 27 |
| Gauntlet | 18 |
| Stealth-Duel | 15 |
| Escort-Duel | 13 |
| Survival Wave | 12 |
| Trial | 12 |
| Chase | 9 |
| Cinematic | 1 (plus cinematic beats inside other missions) |
| **Total** | **120** |

---

## 1. Emberfall Monastery — `region.emberfall` · Lv 1–5 · Oathlord: Abbot Kessh (`boss.kessh`)

| # | ID | Name | Type | Synopsis | Enemies / Boss | Reward highlight |
|---|---|---|---|---|---|---|
| 1 | `mission.emberfall.01` | **The Oathwarden's Refusal** | Duel | *Prologue.* Rhen escorts Liss up the Censer Steps. At the Belltower, Kessh reads the Edict of the Living Tithe, and Rhen turns his blade on his own company. Idris Hale disarms him in a scripted loss. | Oathwarden Sentinels ×4, Warden-Captain Idris Hale (scripted) | Oathblade Style basics. Katana *Warden's Plain Edge* |
| 2 | `mission.emberfall.02` | **Seven Steps to the Block** | Gauntlet | *Prologue.* Bound at the wrists, Rhen fights down seven terraces with kicks, `Grab` and `Guard`. Horvath reads Hask's writ, Kessh swings the censer, and the blade falls. Rhen's shadow tears free of the smoke. | Ashen Novices, Censer-Bearers | Grab and throw-tech tutorial. Title *The Executed* |
| 3 | `mission.emberfall.03` | **The Drowned Stair** | Trial | *Prologue.* Rhen wakes in the Undermourn beside Sable. Old Mother Silt guides them up the Drowned Stair while Rhen learns to dodge at the last instant and to fight with his shadow, and in the stair pool he lands his first Execution on the Stair-Eel. | Undermourn Husks, the Stair-Eel (mini-boss) | **Sable joins.** Umbral Shadow, Perfect Dodge and Execution unlocked |
| 4 | `mission.emberfall.04` | **Return to Ash** | Survival Wave | Rhen climbs out of the Ossuary Well into the catacombs forty days after his death. He holds the well-mouth against Ash Husks until the dawn bell. | Ash Husks (5 waves), Cinder Penitents | Ember Rage unlocked. `quest.sable.01` opens |
| 5 | `mission.emberfall.05` | **The Defector** | Escort-Duel | Brother Adekan is hiding in the Lower Cloister. Rhen escorts him out through Censer-Bearer patrols and learns that Adekan sent Liss downriver to the House of Quiet Water. | Censer-Bearers, Ashen Novices | **Adekan joins.** Training mode and the Dojo open |
| 6 | `mission.emberfall.06` | **Precentor's Hymn** | Duel | Precentor Vashti Coil fights Rhen in the Choir Loft while her choir sings the Kneel. Every verse speeds up her chain. She is beaten and flees. | Precentor Vashti Coil (Chain Sword) | Rune *Litany Break*. Censer Chain Style trial |
| 7 | `mission.emberfall.07` | **Bell of the Unreturned** | Stealth-Duel | Rhen slips into the Belltower, where tithed shadows are rung into the Oathstone, and silences the Censer-Bearers one by one before the bell can toll for six novices. | Censer-Bearers, Ashen Novices, Bell Warden (elite) | `fragment.devotion` becomes reachable. `quest.adekan.01` opens |
| 8 | `mission.emberfall.08` | **The Headsman's Due** | Duel | At the block where Rhen died, Deacon Horvath Pyle waits with the same headsman's blade and the same prayer. | Deacon Horvath Pyle (Nodachi) | Cosmetic *Headsman's Cowl*. Rune *Second Life* |
| 9 | `mission.emberfall.09` | **Ascent of Cinders** | Gauntlet | Kessh sets fire to the monastery's ash-stores. Rhen climbs the burning Censer Steps. Idris Hale blocks the top step, then stands aside: "One breath. That's all you get from me." | Cinder Penitents, Oathwarden Sentinels, Censer-Bearers | Ember Rage upgrade *Kindled* |
| 10 | `mission.emberfall.10` | **The Ash Censer** | Boss | Under the ash-snow at the Belltower, Abbot Kessh performs the Last Rite on Rhen a second time. This time Rhen does not die. | **Abbot Kessh** (`boss.kessh`) | Legendary *Censer of Last Rites*. **Oath Choice: Devotion** |

**Side missions: Emberfall**

| ID | Name | Category | Hook |
|---|---|---|---|
| `quest.sable.01` | Names in the Dark | Companion | After `.04`. Sable remembers the face of Rhen's mother, which Rhen lost in the Undermourn. |
| `quest.adekan.01` | The Novices | Companion | After `.07`. Six novices are to be tithed for the Quincentennial. |
| `quest.adekan.05` | The Unsung Rite | Companion | After `mission.sunkenarchive.10`. Adekan comes home to sing a funeral that takes nothing. |
| `side.emberfall.01` | The Candler's Debt | Guild | Old Pell's candles are made with tithe-ash. Who sells it to him? |
| `side.emberfall.02` | Ash Gardens | Echo | Six Shadow Echoes are buried where the tithe-ash was scattered. Sister Idra writes each name on the garden wall once it is heard. |
| `side.emberfall.03` | The Penitent Stair | Trial | Climb the pilgrim stair using only `Guard`, `Grab` and Perfect Parry. |
| `side.emberfall.04` | Warden's Shrine | Contract | Ash Husks hold the Oathwarden barracks. Clear them and recover the sigils of the Wardens who died there. |
| `side.emberfall.05` | Idra's Lamp | Quest | Sister Idra asks Rhen to carry a novice's last words to his mother in the pilgrim camp. |

---

## 2. The Weeping Reeds — `region.weepingreeds` · Lv 5–10 · Oathlord: Mother Ilvane (`boss.ilvane`)

| # | ID | Name | Type | Synopsis | Enemies / Boss | Reward highlight |
|---|---|---|---|---|---|---|
| 1 | `mission.weepingreeds.01` | **Downriver** | Chase | Rhen rides the Ashwater after a Choir tithe-barge sent to collect Liss, fighting across barges that crash through the reed channels. | Ferry-Hooks, Drowned Husks | Material *Salt-Bronze*. Reeds map |
| 2 | `mission.weepingreeds.02` | **Lantern in the Fog** | Escort-Duel | A smuggler's skiff runs aground in the fog. Rhen defends it from Salt Reavers, and its pilot, Neve, offers him a ride and a price. | Salt Reavers, Bog Lurkers | **Neve joins.** Store and collections open. `quest.neve.01` opens |
| 3 | `mission.weepingreeds.03` | **The Village That Sank** | Survival Wave | In Hollowmere, the tide rises over the chapel roof. Rhen holds the roof as Drowned Husks climb out of the flooded pews. | Drowned Husks, Bog Lurkers | Rune *Held Breath I* |
| 4 | `mission.weepingreeds.04` | **Quiet Water** | Stealth-Duel | Rhen gets into the House of Quiet Water and finds Sister Oona Weir drowning a loose-shadowed boy "gently". He stops her. | Quiet Water Sisters, Sister Oona Weir (Naginata) | *House Key of Quiet Water* |
| 5 | `mission.weepingreeds.05` | **The Child Who Casts Two** | Escort-Duel | Rhen carries Liss out of the drowning pools while Moth, her loose shadow, runs ahead through the stilt-house. The Sisters close in. | Quiet Water Sisters, Ferry-Hooks | **Liss joins.** `quest.liss.01` opens |
| 6 | `mission.weepingreeds.06` | **Nine Knives in the Rain** | Duel **[Tamsin I]** | On the Weeping Pier, a woman with nine knives comes for Liss. Tamsin pins Sable to the planks with a knife, and Sable feels it. Then she withdraws. | **Tamsin** (encounter I; withdraws at 50% HP) | **Daggers unlocked** (Ninefold Style). `knife.i`. `quest.sable.02` opens. `fragment.mercy` becomes reachable |
| 7 | `mission.weepingreeds.07` | **Ferryman's Toll** | Duel | Ferryman Collum Brack poles the funeral barge that carries Ilvane's drowned. His toll is one child. | Ferryman Collum Brack (Naginata) | Rune *Undertow I*. `quest.adekan.02` opens |
| 8 | `mission.weepingreeds.08` | **Gudrun's Stilt-House** | Trial | Gudrun, an eel-wife, hides escaped children under her floor. Rhen defends the house with parries only while the Sisters search it. | Quiet Water Sisters, Ferry-Hooks | Perfect Parry upgrade *Reed-Still* |
| 9 | `mission.weepingreeds.09` | **Where the Bell Tolls Under** | Gauntlet | Below the Bell Causeway lies the Drowned Chapel, where three voices sing without breathing. Rhen fights down through the flooded nave and up onto the causeway. | Drowned Husks, Quiet Water Sisters | **Raid discovered:** `raid.drownedchoir` (opens at Lv 25) |
| 10 | `mission.weepingreeds.10` | **The Drowned Bell** | Boss | On the Bell Causeway in the rising tide, Mother Ilvane rings the Tidebell and asks for Liss. She says it is for mercy. | **Mother Ilvane** (`boss.ilvane`) | Legendary *Tidebell*. **Oath Choice: Mercy** |

**Side missions: Weeping Reeds**

| ID | Name | Category | Hook |
|---|---|---|---|
| `quest.liss.01` | Two Shadows at Dusk | Companion | After `.05`. Moth wanders off at dusk. Follow it to the first Hanne echo. |
| `quest.neve.01` | Salt and Lanterns | Companion | After `.02`. Neve's contraband turns out to be children. |
| `quest.sable.02` | A Knife That Cuts Shadows | Companion | After `.06`. Sable wants to know whether he can die. |
| `quest.adekan.02` | Drowned Brothers | Companion | After `.07`. The Choir monks sent for Liss never came home. |
| `side.weepingreeds.01` | Eelwife's Children | Escort | Gudrun has three more loose-shadowed children to get past the House patrols. |
| `side.weepingreeds.02` | Salt Reaver Bounty | Contract | The Salt Crows are paying for the Reaver captain who sinks refugee skiffs. |
| `side.weepingreeds.03` | The Hollowmere Bells | Echo | Ring Hollowmere's five sunken bells in order to hear the village's last evening. |
| `side.weepingreeds.04` | Warden in the Fog | Rival | A scout from Idris Hale's company is tracking Rhen through the fog. Duel him before he reports. |
| `raid.drownedchoir` | The Choir Beneath | Raid | 3–4 players. Three drowned sisters sing through one throat in the chapel under the Reeds (`boss.drownedchoir`). |

---

## 3. Ironroot Forge — `region.ironroot` · Lv 10–15 · Oathlord: Gorran Vox (`boss.gorran`)

| # | ID | Name | Type | Synopsis | Enemies / Boss | Reward highlight |
|---|---|---|---|---|---|---|
| 1 | `mission.ironroot.01` | **Slag Road** | Gauntlet | Rhen enters the forge-city along the slag-cart lines, fighting Slagguard on moving ore carts over the molten river. | Slagguard, Rivet Husks | Material *Caldera Iron* |
| 2 | `mission.ironroot.02` | **The Old Man at the Cold Forge** | Trial | In an abandoned forge, an old smith named Tessen won't speak to anyone who can't win using heavy attacks only. Rhen proves he can. | Tessen (sparring), Rivet Husks | **Tessen joins.** Forge and crafting open. `quest.tessen.01` opens. `fragment.labor` becomes reachable |
| 3 | `mission.ironroot.03` | **Shadow Shift** | Stealth-Duel | On the Shadow Shift line, workers' shadows are clamped to the engines while the workers are still awake. Rhen frees a line and silences the Tally-Clerks. | Tally-Clerks (Daggers), Slagguard | Rune *Unpaid Hour I* |
| 4 | `mission.ironroot.04` | **The Cinder Strike** | Escort-Duel | Rhen escorts Juno Tallow and her strikers to Bellows Square through Gorran's strike-breakers. | Slagguard, Forge Thralls | Juno's favor. Ironroot merchants open |
| 5 | `mission.ironroot.05` | **Quench** | Survival Wave | Gorran floods the lower district with slag to break the strike. Rhen holds the Quenching Vats, and behind them finds a sealed chapel door stamped with a penitent's hammer. | Forge Thralls, Rivet Husks | **Furnace Key 1/3** (hidden foundry). `quest.tessen.02` opens |
| 6 | `mission.ironroot.06` | **Blood in the Pits** | Duel | In the Slag Pits, Captain Oskar Dray bets his company's last coin on Rhen against the Red Tallies' captain, Ludo Skarre. | Captain Ludo Skarre (War Hammer) | **Oskar joins.** Arena, Survival and Clan contracts open. `quest.oskar.01` opens |
| 7 | `mission.ironroot.07` | **Nine Knives in the Smoke** | Chase **[Tamsin II]** | Tamsin steals the forge-seal key Rhen needs for the Crucible. He chases her along the crane-lines above the molten river until she turns to fight. | **Tamsin** (encounter II; withdraws at 40% HP), Slagguard | `knife.ii`. Forge-seal key |
| 8 | `mission.ironroot.08` | **Tallyhand** | Duel | Forewoman Brisa Kell counts every shadow in Ironroot on a brass abacus the size of a wall, and she fights with a chain of its beads. | Forewoman Brisa Kell (Chain Sword) | Rune *Counted Strike* |
| 9 | `mission.ironroot.09` | **The Riveter** | Duel | Overseer Dunmarrow, a giant in slag-armor with a riveting hammer, holds the Crane Yard between Rhen and the Crucible. | Overseer Dunmarrow (War Hammer), crane hazards | Armor *Riveted Pauldrons* |
| 10 | `mission.ironroot.10` | **The Anvil King** | Boss | Gorran Vox waits in the Great Crucible with his workers' shadows clamped to the walls around him, and offers Rhen a job. | **Gorran Vox** (`boss.gorran`) | Legendary *Worldanvil*. **Oath Choice: Labor**. **Raid discovered:** `raid.ferrousmaw` (opens at Lv 35) |

**Side missions: Ironroot**

| ID | Name | Category | Hook |
|---|---|---|---|
| `quest.tessen.01` | Cold Iron | Companion | After `.02`. Relight the Cold Forge. |
| `quest.tessen.02` | The Abbess's Door | Companion | After `.05` with 3 Furnace Keys. Tessen was Velka's apprentice. |
| `quest.oskar.01` | Pit Debts | Companion | After `.06`. Oskar owes the Pits more than coin. |
| `side.ironroot.01` | Scab Wages | Contract | Juno's strikers need protection from Red Tallies strike-breakers. |
| `side.ironroot.02` | Clamped | Echo | Six clamped shadows are still talking on the Shadow Shift line. |
| `side.ironroot.03` | Slag Pit Ladder | Arena | Ten ranked pit fights, each with a crowd-thrown weapon modifier. |
| `side.ironroot.04` | **The Abbess Unquenched** | Hidden Duel | Three Furnace Keys open the penitent's door. Behind it, Iron Abbess Velka is still at her forge (`boss.velka`). |
| `side.ironroot.05` | Furnace Keys | Hunt | The two remaining Furnace Keys are in the Ledger Hall safe and on the Crane Yard's highest gantry. |
| `raid.ferrousmaw` | The Engine Wakes | Raid | 3–4 players. The engine under the Crucible, built from a thousand clamped shadows, tears itself loose (`boss.ferrousmaw`). |

---

## 4. Silkwind Groves — `region.silkwind` · Lv 15–20 · Oathlord: Ysolde & Yrrah (`boss.petals`)

| # | ID | Name | Type | Synopsis | Enemies / Boss | Reward highlight |
|---|---|---|---|---|---|---|
| 1 | `mission.silkwind.01` | **Petals on the Wind** | Gauntlet | Rhen passes through the Windward Bamboo at dawn, where the Thornsworn patrols time their strikes to the gusts. | Thornsworn (Dual Blades), Wind Husks | Material *Windsilk* |
| 2 | `mission.silkwind.02` | **The Listener** | Trial | Wind-Listener Amaru tunes the grove's chimes to warn of attacks. Rhen must parry by sound alone while the screen dims. | Thornsworn (training) | Perfect Parry upgrade *Chime-Read* |
| 3 | `mission.silkwind.03` | **The Loom Library** | Stealth-Duel | Loom Wardens are hunting a disgraced archivist through the library stacks. Rhen reaches Mireth Vale first. | Loom Wardens (Whip Blades), Thornsworn | **Mireth joins.** Runes and enchanting open. `quest.mireth.01` opens. `fragment.kinship` becomes reachable |
| 4 | `mission.silkwind.04` | **Red Thread** | Escort-Duel | Cassia's twin brother Cato has been taken to carry their dead mother's shadow in the Binding Rite. Rhen escorts Cassia through the Thornsworn checkpoints. | Thornsworn, Petal Dancers | `quest.liss.02` opens |
| 5 | `mission.silkwind.05` | **Dye Vats** | Survival Wave | Wind Husks and Red Tallies raiders hit the dye-works at the same time. Rhen holds the vats as colored smoke fills the valley. | Wind Husks, Red Tallies raiders | Cosmetic *Vat-Dyed Sash*. `quest.oskar.02` opens |
| 6 | `mission.silkwind.06` | **Nine Knives in the Bamboo** | Stealth-Duel **[Tamsin III]** | A night hunt in which each tracks the other by the sound of cut stalks. Tamsin withdraws and leaves a question carved into a stalk: *Why do you keep them alive?* | **Tamsin** (encounter III; withdraws at 35% HP) | `knife.iii` |
| 7 | `mission.silkwind.07` | **The Thornsworn** | Duel | Captain Lorcan Vey, the Petals' bodyguard, duels Rhen on a bridge of fallen bamboo over the gorge. | Captain Lorcan Vey (Dual Blades) | Rune *Thorn Guard* |
| 8 | `mission.silkwind.08` | **Loom-Mother** | Duel | Loom-Mother Saffi Orle fights among the Great Loom's tripwire threads, weaving the arena smaller as she goes. | Loom-Mother Saffi Orle (Whip Blade) | Armor *Loom-Mother's Mantle* |
| 9 | `mission.silkwind.09` | **The Binding Rite** | Gauntlet | Rhen fights through the rite-house and cuts Cato's thread before the last knot. On the pavilion above, Ysolde watches, and her sister's shadow moves a heartbeat behind her. | Thornsworn, Petal Dancers, Loom Wardens | `flag.cato.freed` |
| 10 | `mission.silkwind.10` | **The Paired Petals** | Boss | Ysolde and her dead twin Yrrah dance as one on the mirrored pond of the Twin Pavilion. | **Ysolde & Yrrah** (`boss.petals`) | Legendary *Thorn & Bloom*. **Oath Choice: Kinship** |

**Side missions: Silkwind**

| ID | Name | Category | Hook |
|---|---|---|---|
| `quest.mireth.01` | Petal-Script | Companion | After `.03`. Mireth needs a threadbook the Loom Wardens won't lend. |
| `quest.liss.02` | The Red Ribbon | Companion | After `.04`. A tether Liss can untie. |
| `quest.oskar.02` | Red Tallies | Companion | After `.05`. Ludo Skarre is back, and he's working for the Throne. |
| `side.silkwind.01` | Cut Cords | Quest | After Cato, four more families beg Rhen to cut their Binding threads before the fade takes both of the bound. |
| `side.silkwind.02` | Chime Trials | Trial | Amaru's six chime tunings. Each one is a parry-by-sound trial. |
| `side.silkwind.03` | Threads of the Dead | Echo | Six Shadow Echoes woven into funeral silks. |
| `side.silkwind.04` | The Hundred-Cut Challenger | Rival | A wandering Free Blade has cut ninety-nine bamboo stalks with one stroke each and wants Rhen to be the hundredth. |
| `side.silkwind.05` | Dye Caravan | Guild | Neve's dye caravan must reach the Reeds before the petal-storm. |

---

## 5. The Glass Ossuary — `region.glassossuary` · Lv 20–25 · Oathlord: Seraph Maal (`boss.maal`)

| # | ID | Name | Type | Synopsis | Enemies / Boss | Reward highlight |
|---|---|---|---|---|---|---|
| 1 | `mission.glassossuary.01` | **Where the Sand Burned** | Gauntlet | Rhen crosses the Ribcage Pass at midday. Standing in open sun drains health, and the Glassblind patrols fight by sound. | Glassblind Zealots (Spears), Bone-Crawlers | Material *Sunglass* |
| 2 | `mission.glassossuary.02` | **The Water-Seller** | Escort-Duel | Obed's water caravan is the only way across the Glass Sea, and the Bone-Crawlers know it too. | Bone-Crawlers, Mirage Husks | Water flask upgrade (+1 heal charge) |
| 3 | `mission.glassossuary.03` | **Cartographer of Bones** | Trial | Yusra Dahl maps a safe path across glass-hot tiles. Rhen must defeat a Glassblind squad without ever stepping off her route. | Glassblind Zealots | Rune *Surveyor's Step*. `quest.mireth.02` opens |
| 4 | `mission.glassossuary.04` | **The Glass Tongue** | Duel | Lector Ammun Sayre preaches from the Sermon Stair and fights between verses, his spear keeping time with the sermon. | Lector Ammun Sayre (Spear) | Rune *Zealot's Reach* |
| 5 | `mission.glassossuary.05` | **Mirage Host** | Survival Wave | At noon the Glass Sea shimmers and Mirage Husks rise from their own reflections. Rhen holds out until a Sleeper's rib shades the camp. | Mirage Husks, Bone-Crawlers | `quest.liss.03` opens |
| 6 | `mission.glassossuary.06` | **Pilgrims' Road** | Stealth-Duel | Wearing a sun-veil, Rhen joins a pilgrim column to the Noon Cathedral and quietly removes the Glassblind wardens who cull the stragglers. | Glassblind Zealots, Sunlance Pilgrims | `fragment.faith` becomes reachable |
| 7 | `mission.glassossuary.07` | **Nine Knives in the Glare** | Trial **[Tamsin IV]** | A glass-storm strands Rhen and Tamsin in the same ruin. They fight back to back against the Glassblind until dawn, then face each other. She withdraws: "Call it even. Once." | Glassblind Zealots, **Tamsin** (ally, then encounter IV; withdraws at 30% HP) | `knife.iv`. Cosmetic *Storm-Scoured Cloak* |
| 8 | `mission.glassossuary.08` | **The Hidden Tomb** | Gauntlet | Beneath the Cathedral, the Skull of the Giant holds a sealed tomb. Its door is carved with the Rebirth Song. | Bone-Crawlers, Mirage Husks | **Hidden duel unlocked:** `side.glassossuary.05` (`boss.ossric`) |
| 9 | `mission.glassossuary.09` | **The Glassblind** | Duel | Ghafir Sethe, captain of the Glassblind, blinded himself on the Cathedral steps. He fights in total darkness and hears every step Rhen takes. | Ghafir Sethe (Spear) | Rune *Listening Guard* |
| 10 | `mission.glassossuary.10` | **The Sunscorched** | Boss | At the noon zenith in the Noon Cathedral, Seraph Maal preaches the Solemn Sun to the man who has come to put it out. | **Seraph Maal** (`boss.maal`) | Legendary *Noonpiercer*. **Oath Choice: Faith** |

**Side missions: Glass Ossuary**

| ID | Name | Category | Hook |
|---|---|---|---|
| `quest.liss.03` | The Song Beneath the Sand | Companion | After `.05`. Liss hears singing under the bones. |
| `quest.mireth.02` | Bones That Testify | Companion | After `.03`. The Sleepers' bones carry inscriptions older than the Oath. |
| `side.glassossuary.01` | Water Rights | Quest | Obed charges a coin per swallow. Rhen can break his monopoly or pay it. |
| `side.glassossuary.02` | Maps of the Sleepers | Quest | Find five bone markers for Yusra's great survey. |
| `side.glassossuary.03` | Mirage Duel | Trial | Fight a mirage of yourself that uses your current loadout. |
| `side.glassossuary.04` | Last Words in Glass | Echo | Six pilgrims' echoes fused into the glass. |
| `side.glassossuary.05` | **The Grave-Warden** | Hidden Duel | The tomb opens for anyone who can hum the Rebirth Song. Ossric has been waiting since before the Oath (`boss.ossric`). |

---

## 6. Rimewood Steppe — `region.rimewood` · Lv 25–30 · Oathlord: Tharuk Greymane (`boss.tharuk`)

| # | ID | Name | Type | Synopsis | Enemies / Boss | Reward highlight |
|---|---|---|---|---|---|---|
| 1 | `mission.rimewood.01` | **The Long Night Road** | Chase | A sled chase across the Frozen River, with Greymane raiders and their frost hounds closing in as the ice breaks up. | Greymane Raiders (Gauntlets), Frost Hounds | Material *Rime-Steel* |
| 2 | `mission.rimewood.02` | **Old Teacher** | Trial | Ser Mathilde Crowe, the retired Warden who taught Rhen to parry, tests him in the snow. He may use only Oathblade Style counters. | Ser Mathilde Crowe (Katana) | Katana technique *Crowe's Answer* |
| 3 | `mission.rimewood.03` | **Weighing of the Weak** | Stealth-Duel | At a Culling Moot, the old and the lame are weighed against stones. Rhen frees them before the aurora is sung. | Greymane Raiders, Aurora Shamans | `quest.tessen.03` opens |
| 4 | `mission.rimewood.04` | **The Pale Stag** | Chase | A white stag that leaves no footprints leads Rhen through a whiteout, with Rime Husks following the warmth of his body. | Rime Husks, Frost Hounds | **Hidden duel unlocked:** `side.rimewood.05` (`boss.eirmund`) |
| 5 | `mission.rimewood.05` | **Blizzard Wall** | Survival Wave | Rhen defends Yeva Sarn's camp of the clanless through a blizzard, with more Rime Husks arriving on every gust. | Rime Husks, Frost Hounds | `quest.sable.03` opens |
| 6 | `mission.rimewood.06` | **Nine Knives in the Snow** | Duel **[Tamsin V]** | Tamsin is fading, having sold half her shadow. In the whiteout she tells Rhen who hired her: Duke Veloran Sae, acting for the Throne. | **Tamsin** (encounter V; withdraws at 25% HP) | `knife.v`. Lanternhold revealed as the next destination |
| 7 | `mission.rimewood.07` | **The Skyreader** | Duel | Hrolm the Skyreader reads the aurora to choose who will be culled. He fights at the Aurora Stones and draws power from the sky. | Hrolm the Skyreader (Arcane — Soul Lantern) | **Arcane — Soul Lantern unlocked.** `quest.oskar.03` opens. `fragment.strength` becomes reachable |
| 8 | `mission.rimewood.08` | **Ashfang** | Duel | Kirra Ashfang, Tharuk's adopted daughter, has never lost a duel, and she is about to learn what her father's strength costs. **Mercy choice.** | Kirra Ashfang (Gauntlets) | Armor *Ashfang Wraps*. Sets `flag.kirra.spared` |
| 9 | `mission.rimewood.09` | **The Wolf's Moot** | Gauntlet | Rhen fights through the Greymane war camp to the Howling Cairn as the clans gather for the year's last Moot. | Greymane Raiders, Frost Hounds, Aurora Shamans | Rune *Pack-Breaker* |
| 10 | `mission.rimewood.10` | **The Winter Wolf** | Boss | In fanged gauntlets, in the heart of a blizzard on the Howling Cairn, Tharuk Greymane fights to prove that strength is the only mercy. | **Tharuk Greymane** (`boss.tharuk`) | Legendary *Fangs of the Long Night*. **Oath Choice: Strength** |

**Side missions: Rimewood**

| ID | Name | Category | Hook |
|---|---|---|---|
| `quest.sable.03` | The Mirror Pool | Companion | After `.05`. The culled dead call Sable "brother". |
| `quest.tessen.03` | Star-Iron | Companion | After `.03`. A star fell on the steppe, and the Greymanes want it too. |
| `quest.oskar.03` | Tobin's Echo | Companion | After `.07`. Oskar's little brother was culled here. |
| `side.rimewood.01` | Culled | Quest | Some culled elders are still alive in the snow. Bring them to Yeva's camp. |
| `side.rimewood.02` | Mathilde's Last Lesson | Trial | Crowe's final lesson: beat her without taking a hit. |
| `side.rimewood.03` | Hound-Pack Bounty | Contract | A frost-hound pack went wild after its riders were culled. |
| `side.rimewood.04` | Aurora Echoes | Echo | Six echoes caught in the aurora, audible only at the Aurora Stones at midnight. |
| `side.rimewood.05` | **The Pale Stag** | Hidden Duel | In the Grove of First Snow, the stag takes off its skull. Eirmund still hunts the Unsworn (`boss.eirmund`). |

---

## 7. Lanternhold — `region.lanternhold` · Lv 30–35 · Oathlord: Duke Veloran Sae (`boss.veloran`)

| # | ID | Name | Type | Synopsis | Enemies / Boss | Reward highlight |
|---|---|---|---|---|---|---|
| 1 | `mission.lanternhold.01` | **The Festival That Never Ends** | Stealth-Duel | Rhen reaches Lanternhold by canal on festival night 105,120, unmasked in a city where that is a crime, and takes a mask from a Masquer who won't be needing it. | Masquers (Whip Blades), Duke's Silks | `collectible.mask.01` |
| 2 | `mission.lanternhold.02` | **Every Lantern Has a Price** | Escort-Duel | Neve comes home. Rhen escorts her to the Guild Exchange past Hollis Crane's toughs, who want her ledger. | Guild Toughs (Daggers), Masquers | `quest.neve.02` opens. `fragment.loyalty` becomes reachable |
| 3 | `mission.lanternhold.03` | **Paper Wraiths** | Survival Wave | Unsworn trapped in the festival lanterns burst free over the Canal Market. Rhen holds the Lamplighters' Bridge with old Dov. | Paper Wraiths | Rune *Lamplight*. `quest.adekan.04` opens |
| 4 | `mission.lanternhold.04` | **The Mask-Maker** | Trial | Madame Quince's masks narrow the wearer's sight. Rhen fights in her workshop with a reduced HUD and a masked field of view. | Masquers | Cosmetic mask *Quince's Crescent*. `quest.mireth.03` opens |
| 5 | `mission.lanternhold.05` | **Canal Chase** | Chase | Hollis Crane's gondola carries the shade-notes for half the city. Rhen chases it gondola to gondola down the Grand Canal. | Guild Toughs, Paper Wraiths | Hollis's shade-ledger (partial). `quest.neve.03` opens |
| 6 | `mission.lanternhold.06` | **The Smiling Mask** | Duel | Masque-Captain Ormond Lisle stages the duel as the finale at the Opera of Lanterns, and the audience applauds every hit. | Masque-Captain Ormond Lisle (Whip Blade) | Rune *Encore*. `quest.sable.04` opens |
| 7 | `mission.lanternhold.07` | **The Pale Mask** | Stealth-Duel | Vivienne Sae, the Duke's sister and spymaster, runs the city's informers from a house of whispers. Rhen gets in and confronts her. | Duke's Silks, Vivienne Sae (Daggers) | Intel: the Chancellor's seal on Tamsin's contract |
| 8 | `mission.lanternhold.08` | **The Ninth Knife** | Boss **[Tamsin VI, final]** | On the Tiles at the height of the fireworks, Tamsin fights her last contract with all nine knives. **Mercy choice.** | **Tamsin** (`boss.tamsin`) | Legendary *Ninefold Fang*. `knife.ix`. If spared, `side.lanternhold.05` opens. `quest.neve.04` opens |
| 9 | `mission.lanternhold.09` | **Masquerade** | Gauntlet | At the Duke's grand masquerade in the Palace of Ten Thousand Lanterns, Rhen fights through dancers who never stop dancing. | Masquers, Duke's Silks | Mask *Duke's Invitation* |
| 10 | `mission.lanternhold.10` | **The Masquerade** | Boss | Duke Veloran Sae waltzes Rhen across the ballroom. Under the last mask there is no one at all. | **Duke Veloran Sae** (`boss.veloran`) | Legendary *Silken Lash*. **Oath Choice: Loyalty** |

**Side missions: Lanternhold**

| ID | Name | Category | Hook |
|---|---|---|---|
| `quest.neve.02` | A Ledger of Shadows | Companion | After `.02`. The shadow market, where Tamsin's shade-note is held. |
| `quest.neve.03` | Hollis Crane | Companion | After `.05`. A reckoning with the man who buys shadows. |
| `quest.neve.04` | The Nine Black Lanterns | Companion | After `.08`. Knives VI–VIII, and the road to the Mourning House. |
| `quest.neve.05` | The Last Lantern | Companion | After `mission.cloudspire.10`. The Guild seat, or the ledgers burning. |
| `quest.sable.04` | Who Wears Whom | Companion | After `.06`. Sable wears Rhen's face for an hour. |
| `quest.mireth.03` | The Censured Hand | Companion | After `.04`. Mireth's banned treatise is being sold as a curiosity. |
| `quest.adekan.04` | Choir in Exile | Companion | After `.03`. Emberfall's exiles have reached the canals. |
| `side.lanternhold.01` | Masks for the Maskless | Quest | Madame Quince makes masks for citizens whose collateral was seized. Deliver them before the Silks' night-count. |
| `side.lanternhold.02` | Opera Understudy | Trial | Fight on the Opera stage in time with the orchestra. Hits on the beat deal double posture damage. |
| `side.lanternhold.03` | Paper Prayers | Echo | Six Shadow Echoes folded into festival lanterns. |
| `side.lanternhold.04` | Canal Ladder | Arena | Free Blade duels on moving gondolas. |
| `side.lanternhold.05` | **The Ninth Widow** | Hidden Duel | The Mourning House opens through Tamsin (if spared) or through all nine knives (if not). Lady Corvaine still mourns (`boss.corvaine`). |

---

## 8. The Verdant Rot — `region.verdantrot` · Lv 35–40 · Oathlord: Queen Myrrhen (`boss.myrrhen`)

| # | ID | Name | Type | Synopsis | Enemies / Boss | Reward highlight |
|---|---|---|---|---|---|---|
| 1 | `mission.verdantrot.01` | **Into the Glow** | Gauntlet | Across the Glowcap Verge, every hit lights the mushrooms and draws more Glowcap Husks. | Glowcap Husks, Spore-Walkers | Material *Glowcap Resin* |
| 2 | `mission.verdantrot.02` | **The Lost Botanist** | Escort-Duel | Evander Crale, a Throne naturalist who is half-rooted and still walking, needs to reach his camp before the roots finish their work. | Rootbound Acolytes (Scythes), Spore-Walkers | Evander's field notes (spore resistance +20%) |
| 3 | `mission.verdantrot.03` | **Spore Season** | Survival Wave | A spore bloom fills the jungle. Visibility drops and Ember drains until it settles. | Spore-Walkers, Glowcap Husks | Rune *Clean Breath* |
| 4 | `mission.verdantrot.04` | **Wedding Procession** | Stealth-Duel | A Rootbound procession is taking a living bride to the Rot. Rhen joins it in lichen lace and cuts her free before the vows. | Rootbound Acolytes, Myconid Brides | `fragment.life` becomes reachable |
| 5 | `mission.verdantrot.05` | **Sporemother** | Duel | Sporemother Vell, high priestess of the Rootbound, fights inside a cloud of her own spores. | Sporemother Vell (Scythe) | Armor *Sporemother's Veil* |
| 6 | `mission.verdantrot.06` | **The Hive Beneath** | Gauntlet | The ground gives way into the Marrow Hollows, whose honeycomb walls are bone. Something enormous is breathing. | Marrow Drones, Glowcap Husks | **Hidden duel unlocked:** `side.verdantrot.05` (`boss.isketh`) |
| 7 | `mission.verdantrot.07` | **Moss and Marrow** | Escort-Duel | Liss has made a friend, Tuck, a moss-child. Rhen escorts both of them across the Rot while Moth draws the Unsworn to the children. | Glowcap Husks, Rootbound Acolytes | `quest.liss.04` opens |
| 8 | `mission.verdantrot.08` | **The Groom** | Duel | Anselk the Groom, Myrrhen's husband for eighty years and held upright by the mycelium, will not let anyone reach his bride. | Anselk the Groom (Spear) | Rune *Unwed* |
| 9 | `mission.verdantrot.09` | **Bridal March** | Chase | The roots of the Bridal Grove pull the whole jungle toward the Heartwood. Rhen races up the rising roots to the Queen's altar. | Rootbound Acolytes, Spore-Walkers | Heartwood key |
| 10 | `mission.verdantrot.10` | **The Mycelial Bride** | Boss | Queen Myrrhen, kept alive for ninety-eight years, asks Rhen to be her second groom. | **Queen Myrrhen** (`boss.myrrhen`) | Legendary *Bridal Reaper*. **Oath Choice: Life** |

**Side missions: Verdant Rot**

| ID | Name | Category | Hook |
|---|---|---|---|
| `quest.liss.04` | The Child Who Would Not Root | Companion | After `.07`. Tuck wants to root his grandmother. |
| `side.verdantrot.01` | Evander's Specimens | Quest | Evander needs spore samples from three fungal "brides" before he finishes rooting. |
| `side.verdantrot.02` | The Unwed | Quest | Three more brides are walking to the Rot, and one of them wants to go. |
| `side.verdantrot.03` | Spore Echoes | Echo | Six echoes held in the glow of the mycelium. |
| `side.verdantrot.04` | Glowcap Gauntlet | Trial | Air-combo challenges on the bounce caps. |
| `side.verdantrot.05` | **The Marrow Queen** | Hidden Duel | The Rootbound walled their first queen into the Marrow Hollows. She is still hungry (`boss.isketh`). |

---

## 9. Cloudspire Aqueducts — `region.cloudspire` · Lv 40–45 · Oathlord: Master Oru (`boss.oru`)

| # | ID | Name | Type | Synopsis | Enemies / Boss | Reward highlight |
|---|---|---|---|---|---|---|
| 1 | `mission.cloudspire.01` | **The Floating Stair** | Gauntlet | Rhen climbs the drifting aqueduct bridges in a storm while Aqueduct Wardens hold every span. | Aqueduct Wardens (Spears), Gale Husks | Material *Stormglass* |
| 2 | `mission.cloudspire.02` | **Engineer's Plea** | Escort-Duel | Halloran Petch keeps the bridges afloat on failing Oath-light regulators. Rhen escorts him to three of them under attack. | Gale Husks, Storm-Bell Monks | Bridge fast-travel |
| 3 | `mission.cloudspire.03` | **Kite-Runner** | Chase | Suri, a kite-runner, carries a message from the summit. Rhen chases the Gale Husks hunting her across the spans. | Gale Husks | Suri's message (first hint of Sen Ajari) |
| 4 | `mission.cloudspire.04` | **The Ninth Bridge** | Duel | Sister Wren keeps the Ninth Bridge and has never let anyone cross it in anger. | Sister Wren (Staff) | `fragment.discipline` becomes reachable |
| 5 | `mission.cloudspire.05` | **Storm Bells** | Survival Wave | Lightning strikes the Storm Bells while waves of Storm-Bell Monks hold the tower that keeps the storm out of the Temple. | Storm-Bell Monks, Gale Husks | Rune *Grounded* |
| 6 | `mission.cloudspire.06` | **The Hundred Stances** | Trial | Master Oru's entrance trial: Rhen faces a hundred students and may never use Dodge. | Still Wind Adepts (Staffs) | Dojo drill *Hundred Stances*. `quest.adekan.03` opens |
| 7 | `mission.cloudspire.07` | **First Student** | Duel | Imre Tal, Oru's finest student, has given the stone every feeling he ever had, and he fights like it. **Mercy choice.** | Imre Tal (Staff) | Armor *Weighted Robe*. Sets `flag.imre.spared` |
| 8 | `mission.cloudspire.08` | **Aqueduct Falls** | Stealth-Duel | Rhen swims the aqueduct channels into the Still Wind cloister and silences the Adepts guarding the Temple gate. | Still Wind Adepts, Aqueduct Wardens | Temple gate key |
| 9 | `mission.cloudspire.09` | **Eye of the Storm** | Gauntlet | Rhen fights through the storm wall into the calm eye, where the Temple of Stillness sits in sunlight. | Storm-Bell Monks, Gale Husks, Still Wind Adepts | Rune *Eye of Calm* |
| 10 | `mission.cloudspire.10` | **The Still Wind** | Boss | Master Oru has not moved his feet in forty years. Rhen has to make him. | **Master Oru** (`boss.oru`) | Legendary *Unmoving Branch*. **Oath Choice: Discipline**. **Hidden duel unlocked:** `side.cloudspire.05` (`boss.senajari`) |

**Side missions: Cloudspire**

| ID | Name | Category | Hook |
|---|---|---|---|
| `quest.adekan.03` | Stillness | Companion | After `.06`. Adekan must win without striking first. |
| `side.cloudspire.01` | Kite Letters | Quest | Suri still has letters addressed to bridges that fell years ago. |
| `side.cloudspire.02` | Regulator Repair | Contract | Halloran's last three regulators are failing under Gale Husk swarms. |
| `side.cloudspire.03` | The Unmoving Trial | Trial | Oru's precepts become challenge rules: no Jump, no Dodge, only counters. |
| `side.cloudspire.04` | Wind Echoes | Echo | Six echoes of students whose feelings were tithed. They speak in flat, careful voices. |
| `side.cloudspire.05` | **The Lotus at the Summit** | Hidden Duel | Oru's dying words send Rhen to the peak, where his master has sat for sixty years (`boss.senajari`). |

---

## 10. The Sunken Archive — `region.sunkenarchive` · Lv 45–50 · Oathlord: Archivist Quill (`boss.quill`)

| # | ID | Name | Type | Synopsis | Enemies / Boss | Reward highlight |
|---|---|---|---|---|---|---|
| 1 | `mission.sunkenarchive.01` | **The Dome Beneath the Sea** | Gauntlet | Rhen descends in a diving bell and fights through the Airlock while Salvage Divers loot the outer halls. | Salvage Divers, Ink Husks | Material *Pressure-Pearl* |
| 2 | `mission.sunkenarchive.02` | **Salvage Rights** | Escort-Duel | Diver Maren Ostrow patches breaches in the dome while Ink Husks pour through the ones she hasn't reached. | Ink Husks, Salvage Divers | Rune *Breathwork* |
| 3 | `mission.sunkenarchive.03` | **The Drowned Stacks** | Stealth-Duel | Page-Wights patrol the flooded shelves. Each was a reader who never finished their book. | Page-Wights (Arcane — Grimoire), Archive Sentinels | Lore scroll cache (3 scrolls) |
| 4 | `mission.sunkenarchive.04` | **Ink Tide** | Survival Wave | The Ink Reservoir ruptures, and every drop that touches the floor stands up. | Ink Husks | Rune *Blotted* |
| 5 | `mission.sunkenarchive.05` | **Curator** | Duel | Curator Salk guards the Drowned Stacks with a shelving pole he has used for forty years and never once for reading. | Curator Salk (Staff) | Archive key |
| 6 | `mission.sunkenarchive.06` | **The Redacted Page** | Trial | In Quill's memory palace, each round replays the day of Mireth's disgrace, and the combat rules change every time the memory is rewritten. | Memory-echo Archivists | `quest.mireth.04` opens |
| 7 | `mission.sunkenarchive.07` | **Lotte's Run** | Chase | Scrivener Lotte Brenn flees with the Ledger of the Tithed as the corridors collapse behind her. Rhen gets her out. | Archive Sentinels, Page-Wights | `quest.liss.05` opens |
| 8 | `mission.sunkenarchive.08` | **The Index** | Duel | The Index, a scholar with a thousand catalog tags sewn into his skin, remembers every fight Rhen has ever had and quotes them back to him. | The Index (Nodachi) | Rune *Cross-Reference* |
| 9 | `mission.sunkenarchive.09` | **The Hidden Clause** | Cinematic | Quill shows Rhen the thirteenth clause of the Oath, written in Aurem's hand between the lines. The keystone of the seal is the Emperor's own shadow. | None | `fragment.memory`. `quest.sable.05` opens |
| 10 | `mission.sunkenarchive.10` | **The Thousand Pages** | Boss | Archivist Quill fights to keep the page that would free Varanth, because remembering is the only thing he has left. | **Archivist Quill** (`boss.quill`) | Legendary *Codex Umbrae*. **Codex Lens**. **Oath Choice: Memory**. `quest.adekan.05` opens |

**Side missions: Sunken Archive**

| ID | Name | Category | Hook |
|---|---|---|---|
| `quest.sable.05` | The Shadow Under the Throne | Companion | After `.09`. Sable has read what happens to a severed shadow under a throne. |
| `quest.liss.05` | The Unwritten Name | Companion | After `.07`. Hanne's name is not in the Ledger. |
| `quest.mireth.04` | The Redacted Page | Companion | After `.06`. Mireth faces her old master's memory. |
| `side.sunkenarchive.01` | Breach Patrol | Contract | Maren pays by the breath for every crack sealed. |
| `side.sunkenarchive.02` | Overdue | Quest | Page-Wights still carry books borrowed two hundred years ago. Get them back. |
| `side.sunkenarchive.03` | Index of the Tithed | Echo | Six echoes of names read aloud. One of them is Hanne. |
| `side.sunkenarchive.04` | Memory Palace | Trial | One-phase echoes of every Oathlord defeated so far, back to back. |
| `side.sunkenarchive.05` | Lotte's Copy | Quest | Lotte wants the Ledger copied for the surface before anyone can redact it again. |

---

## 11. Bloodmoon Citadel — `region.bloodmoon` · Lv 50–55 · Oathlord: General Hask Varrow (`boss.hask`)

| # | ID | Name | Type | Synopsis | Enemies / Boss | Reward highlight |
|---|---|---|---|---|---|---|
| 1 | `mission.bloodmoon.01` | **Siege at the Red Gate** | Survival Wave | Rhen arrives in the middle of an Unsworn assault and holds the Red Gate beside soldiers whose orders are to arrest him. | Siege Husks, Red Moon Knights | Material *Bloodsteel* |
| 2 | `mission.bloodmoon.02` | **Old Oaths** | Duel | Warden-Captain Idris Hale, Rhen's sworn brother, finds him on the wall. The fight ends with no one dead and nothing settled. | Warden-Captain Idris Hale (Katana) | `quest.tessen.04` opens |
| 3 | `mission.bloodmoon.03` | **Refugee Road** | Escort-Duel | Quartermaster Benedek Solt moves refugees through the Lower Ring before the next assault. | Siege Husks, Crimson Vowguard | `quest.oskar.04` opens |
| 4 | `mission.bloodmoon.04` | **The Recruit** | Trial | Young Warden Jory Vance wants to desert. Rhen fights off his pursuers without killing: Guard Break is allowed, Execution is not. | Crimson Vowguard, Oathwarden Veterans | Rune *Mercy Stroke* |
| 5 | `mission.bloodmoon.05` | **Walls of Brandt** | Gauntlet | Siege-Marshal Ulla Brandt holds the battlements with trebuchets that don't care whose side anyone is on. Rhen fights up to her artillery platform. | Crimson Vowguard, Siege-Marshal Ulla Brandt (War Hammer) | `fragment.duty` becomes reachable (artillery breach) |
| 6 | `mission.bloodmoon.06` | **Red Moon Rising** | Stealth-Duel | Under the full red moon, Rhen gets into Warden Keep to find Hask's orders for Liss. | Oathwarden Veterans, Crimson Vowguard | Hask's writ. `quest.mireth.05` opens |
| 7 | `mission.bloodmoon.07` | **The Burning Ward** | Chase | Hask's vanguard is burning the Ward to deny it to the Unsworn, with refugees still inside. Rhen races them block by block. | Crimson Vowguard, Siege Husks | Armor *Ember-Scorched Cloak* |
| 8 | `mission.bloodmoon.08` | **Brother** | Duel | Idris Hale again, on the Oathwarden Bridge, and this time it is settled. **Mercy choice.** | Warden-Captain Idris Hale (Katana) | Sets `flag.idris.spared`. `quest.sable.06` opens |
| 9 | `mission.bloodmoon.09` | **The Vow Hall** | Gauntlet | The Oathwarden veterans who taught Rhen hold the approach to the Vow Hall. Each one salutes before he draws. | Oathwarden Veterans, Crimson Vowguard | `quest.oskar.05` opens |
| 10 | `mission.bloodmoon.10` | **The Crimson Vow** | Boss | Under the red moon, General Hask Varrow waits with the blade he used to sign Rhen's death. | **General Hask Varrow** (`boss.hask`) | Legendary *Vowcleaver*. **Oath Choice: Duty** |

**Side missions: Bloodmoon Citadel**

| ID | Name | Category | Hook |
|---|---|---|---|
| `quest.sable.06` | Terms | Companion | After `.08`. Sable asks for a promise. |
| `quest.tessen.04` | The Blade I Made for Him | Companion | After `.02`. Tessen forged Vowcleaver. |
| `quest.oskar.04` | A Contract with the Citadel | Companion | After `.03`. Hask offers the Salt Crows the Lower Ring. |
| `quest.oskar.05` | Colors | Companion | After `.09`. Oskar chooses his company's colors. |
| `quest.mireth.05` | Publish | Companion | After `.06`. Nail the page to the door, or hand it to Liss. |
| `side.bloodmoon.01` | Bread for the Lower Ring | Quest | Cook Tamber's ovens are still lit, but the flour carts stopped coming. |
| `side.bloodmoon.02` | Deserters | Quest | Jory's friends are hiding in the cisterns, and the Vowguard has dogs. |
| `side.bloodmoon.03` | Siege Echoes | Echo | Six echoes from the walls, including a Warden who held a rope on the day Rhen died. |
| `side.bloodmoon.04` | Wall Duty | Survival Wave | A repeatable night-watch challenge with escalating siege waves. |
| `side.bloodmoon.05` | Oathwarden Trials | Trial | The Warden Keep's old knighting trials, using Oathblade Style only. |
| `side.bloodmoon.06` | Sigils Home | Quest | Bring every recovered Oathwarden Sigil to the Warden Keep shrine. |

---

## 12. The Solemn Throne — `region.solemnthrone` · Lv 55–60 · Oathlord: Emperor Aurem (`boss.aurem`)

| # | ID | Name | Type | Synopsis | Enemies / Boss | Reward highlight |
|---|---|---|---|---|---|---|
| 1 | `mission.solemnthrone.01` | **The Sunward Stair** | Gauntlet | Ten thousand steps up to the palace against the Sunward Guard. Freed Oathwardens (if Duty was sundered) and the Salt Crows (if `flag.oskar.colors = rhen`) fight beside Rhen as AI allies. | Sunward Guard (Spears), Gilded Husks | Material *Sun-Gold* |
| 2 | `mission.solemnthrone.02` | **The Chancellor's Invitation** | Stealth-Duel | Chancellor Orsolya Venn invites Rhen to parley in the Hall of Petitions. It is a trap, and Rhen has to get himself out. | Sunward Guard, Tithe-Readers (Arcane — Soul Lantern) | Petition seals |
| 3 | `mission.solemnthrone.03` | **Gilded Husks** | Survival Wave | The courtiers whose shadows were tithed early all wake at once in their gold paint. | Gilded Husks | Rune *Gilt Edge* |
| 4 | `mission.solemnthrone.04` | **Ledger of the Tithed** | Duel | High Tithe-Reader Casimir Dole guards the Tithe Chancery's master ledger with a soul lantern that can read every shadow in the room. | High Tithe-Reader Casimir Dole (Arcane — Soul Lantern) | The master ledger (epilogue variable) |
| 5 | `mission.solemnthrone.05` | **Sunward Guard** | Duel | Ser Blaise Ondrey, captain of the Sunward Guard, has never drawn his weapon inside the palace. Tonight he does. | Ser Blaise Ondrey (Spear) | `quest.tessen.05` opens |
| 6 | `mission.solemnthrone.06` | **Liss at the Gate** | Escort-Duel | Liss's shadow is being pulled toward the Gate. Rhen escorts her through the Undercroft as the shadow tide rises through the grates. | Undermourn Remnants, Gilded Husks | `quest.liss.06` opens |
| 7 | `mission.solemnthrone.07` | **The Chancellor** | Duel | Orsolya Venn wrote the Edict of the Living Tithe. She defends it, and herself, with the Edict's own book. | Chancellor Orsolya Venn (Arcane — Grimoire) | Rune *Edict Unwritten* |
| 8 | `mission.solemnthrone.08` | **What the Sun Hides** | Stealth-Duel | Rhen slips through the Sun Hall's antechamber while Aurem speaks to him through the glass floor. Halfway across, Sable tears loose for three terrible seconds and has to be fought back into place. | Sunward Guard, Sable (scripted, one exchange) | Lore: *The Sun's Confession* |
| 9 | `mission.solemnthrone.09` | **Twelve Echoes** | Trial | The Sovereignty stone raises everyone Rhen has faced. Eleven Oathlords return, gold if their stone was restored and ink if it was sundered, each for a single phase. The twelfth echo is Rhen himself, as he was on the day he refused. | Echoes of the eleven Oathlords, Echo of Oathwarden Rhen | Rune *Twelvefold Memory* |
| 10 | `mission.solemnthrone.10` | **The Solemn Sun** | Boss | Emperor Aurem, the Sun that does not set, fights with every oath Rhen chose to keep. | **Emperor Aurem** (`boss.aurem`) | Legendary *First Oathblade*. `fragment.sovereignty`. **Oath Choice: Sovereignty**, then the finale |

**Side missions: Solemn Throne**

| ID | Name | Category | Hook |
|---|---|---|---|
| `quest.liss.06` | Lend, and Return | Companion | After `.06`. Liss asks Rhen to teach her how to make an oath. |
| `quest.tessen.05` | Duskforged | Companion | After `.05`. One last blade. |
| `side.solemnthrone.01` | Petitioners | Quest | Unread petitions, some centuries old. Deliver the ones whose writers are still alive. |
| `side.solemnthrone.02` | Gilded Echoes | Echo | Six echoes of courtiers who tithed their shadows for fashion. |
| `side.solemnthrone.03` | The Chancery Files | Stealth | Steal the Edict orders before the Chancery burns them. |
| `side.solemnthrone.04` | Sunward Gauntlet | Trial | The Sunward Guard's promotion trial, run in reverse. |
| `side.solemnthrone.05` | Old Knives, Old Oaths | Rival | Before the Gate, Tamsin (if spared) or Idris (if spared) asks for one last spar. If neither lived, Ser Mathilde Crowe comes. |
| `raid.hundredhanded` | The Gate Below | Raid | Post-campaign, 3–4 players. The Keeper of the Gate still holds a hundred blades (`boss.hundredhanded`). |

---

## 13. Finale sequences (outside the 120)

Rules for these sequences are in `02-World-and-Narrative.md` §8.

| ID | Name | Type | Route | Synopsis | Boss / enemies | Result |
|---|---|---|---|---|---|---|
| `finale.gate` | **The Other You** | Boss | All routes | At the threshold of the Gate, Sable steps out of Rhen and will not step back in until they have settled whose body this is. | **Sable Ascendant** (`boss.sableascendant`) | Win: the route's finale. Yield (take Sable's hand during the Offer): `finale.sableascendant` |
| `finale.solemndawn` | **The Last Watch** | Survival Wave | Dawn (Restore ≥ 9) | Rhen holds the Gate Undercroft for twelve bells while the restored stones re-anchor. Each restored region's Keeper appears once as an assist. | Undermourn Remnants, Gilded Husks, Red Moon Knights | `ending.solemndawn` |
| `finale.unboundnight` | **The Night Unbound** | Chase | Night (Sunder ≥ 9) | Rhen cuts the keystone socket. The Gate bursts, and Rhen and Sable race up the collapsing Sunward Stair ahead of the rising Undermourn. | Freed Husks, collapsing architecture | `ending.unboundnight` |
| `finale.rewovenoath` | **Umbra Prime** | Boss | Balanced (4–8 Restores) + `quest.liss` + 12 fragments | They descend through the Gate. The Hundred-Handed Warden kneels to Liss. At the keystone, Rhen and Sable face what Aurem cut away. | **The First Shadow** (`boss.firstshadow`) | `ending.rewovenoath` |
| `finale.sableascendant` | **Wearing Him** | Cinematic | Yield | A short playable walk as Sable, in Rhen's body, up to the Throne. The court kneels, and the shadow on the floor speaks in Rhen's voice. | None | `ending.sableascendant` |

---

## 14. Raids and hidden duels at a glance

| Boss | ID | Mission | Region | Unlocked by | Players |
|---|---|---|---|---|---|
| The Drowned Choir | `boss.drownedchoir` | `raid.drownedchoir` | Weeping Reeds | Discovered in `mission.weepingreeds.09`, opens at Lv 25 | 3–4 |
| The Ferrous Maw | `boss.ferrousmaw` | `raid.ferrousmaw` | Ironroot Forge | Discovered in `mission.ironroot.10`, opens at Lv 35 | 3–4 |
| The Hundred-Handed Warden | `boss.hundredhanded` | `raid.hundredhanded` | The Undermourn (from the Solemn Throne) | Any ending reached | 3–4 |
| Iron Abbess Velka | `boss.velka` | `side.ironroot.04` | Ironroot Forge | 3 Furnace Keys (`mission.ironroot.05`, `side.ironroot.05`) | 1 |
| Ossric | `boss.ossric` | `side.glassossuary.05` | Glass Ossuary | `mission.glassossuary.08` | 1 |
| Eirmund | `boss.eirmund` | `side.rimewood.05` | Rimewood Steppe | `mission.rimewood.04` | 1 |
| Lady Corvaine | `boss.corvaine` | `side.lanternhold.05` | Lanternhold | `flag.tamsin.spared`, or all nine knives | 1 |
| Isketh | `boss.isketh` | `side.verdantrot.05` | Verdant Rot | `mission.verdantrot.06` | 1 |
| Sen Ajari | `boss.senajari` | `side.cloudspire.05` | Cloudspire Aqueducts | `mission.cloudspire.10` | 1 |

### 14.1 Side-mission categories

| Category | What it is | Rewards |
|---|---|---|
| Companion | A step of a companion questline (`quest.*`) | Approval, companion service tiers, questline rewards |
| Quest | A local story with a named NPC | Runes, materials, NPC fate in the epilogue |
| Contract | Free Blade work posted by Oskar or local captains | Currency, clan points, gear |
| Echo | A region's six Shadow Echoes as one guided hunt | Echo Codex entries, series cosmetics |
| Trial | A rules-constrained challenge | Technique upgrades, mastery XP |
| Arena | Ranked pit or canal fights | Arena rank, cosmetics |
| Rival | A duel against a recurring or wandering rival | Unique cosmetics, techniques |
| Guild | Lantern Guild errands for Neve | Store stock, weekly-event tokens |
| Hunt | A collectible hunt that unlocks something | Keys, unlocks |
| Hidden Duel | A legendary boss outside the main path | Legendary rewards (see `05-Bosses.md`) |
| Raid | A 3–4 player co-op legendary boss | Legendary rewards, raid cosmetics |
| Stealth | A side infiltration | Intel, lore |
| Escort | A side escort | Approval, NPC fate |

**Side-content totals:** 37 companion steps (`quest.*`), 60 side missions (`side.*`), 3 raids. Every region has at least five `side.*` missions plus its companion steps.

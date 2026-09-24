# OATHSUNDER — Bosses

> **Phase 2 · Game Design Document · Bosses**
> Source of truth: [`../00-Canon.md`](../00-Canon.md). Canon requires every boss to have a lore entry,
> signature weapon, intro cinematic, **three combat phases**, a unique AI personality profile, an
> exclusive theme, a custom arena, legendary rewards and an exclusive execution. This document
> specifies all of them for the 24 canon bosses, in canon order. Frame-level numbers belong to
> combat code. Where this document gives frames or seconds, they are design targets for tuning.
> Story context: [`02-World-and-Narrative.md`](02-World-and-Narrative.md). Missions:
> [`03-Missions.md`](03-Missions.md). Cast and voices: [`04-Characters.md`](04-Characters.md).

---

## 0. How to read a boss sheet

### 0.1 Phases

- Every boss has **three phases**, with the HP thresholds listed on its sheet.
- A phase transition is a **90-frame interstitial**: both fighters are invulnerable, the arena changes,
  and a `clash` line plays. The boss's posture resets to 50%, and the player keeps all meters.
- Every phase names **one primary skill test**, which is the canon mechanic the phase is built to
  teach or check: Perfect Parry, Perfect Dodge / Shadow Time, crouch guard, spacing, air combos,
  anti-air, throw teching, posture pressure, meter management.
- Canon rules always apply. Standing guard blocks overheads, crouch guard blocks lows, and parry
  mashing shrinks the Perfect Parry window. Bosses are built around those rules and never against
  them.

### 0.2 AI personality traits (0–100)

These are the same six parameters as the enemy AI model (08-Enemies §3.1). Boss values are listed on each sheet.

| Trait | Low end | High end | What it drives |
|---|---|---|---|
| **Aggression** | Waits for the player to act | Starts exchanges constantly | Attack initiation rate, pressure after blockstun |
| **Patience** | Commits at once | Holds guard, baits, waits for whiffs | Baiting, whiff-punish priority, guard-hold duration |
| **Adaptivity** | Barely reads the Habit Ledger | Learns fast and switches answers quickly | Weight gain on learned counters (§0.3) |
| **Cunning** | Honest, readable strings | Feints, delays, mixups, fake-outs | Feint frequency, delay variance, mixup depth |
| **Courage** | Retreats when hurt | Trades, holds ground, armors through | Retreat threshold, armor usage, low-HP behavior |
| **Showmanship** | All business | Taunts, flourishes, signature moves | Flourish frequency, which creates deliberate punish windows |

### 0.3 The Habit Ledger (learned punishes)

The combat layer records player habits in `habit.*` counters. A boss "learns" a habit when that
habit's counter crosses a threshold scaled by the boss's **Adaptivity**. From then on, the boss
weights its authored counter for that habit more heavily.

| Habit ID | Detected when | Typical learned answer |
|---|---|---|
| `habit.rollspam` | 3 or more dodges within 2 s, or dodge used for more than 50% of defensive actions over 20 s | Delayed hits timed to dodge recovery, tracking follow-ups, traps at the roll's exit |
| `habit.parrymash` | `Guard` pressed 3 or more times within 20 frames (canon: mashing shortens the parry window to 2 frames) | Delayed string hits and hesitation feints |
| `habit.jumpin` | 3 or more jump-ins started from mid range or further within 30 s | Anti-air moves and air grabs |
| `habit.wakeup.attack` | The player attacks on wakeup in 2 of the last 3 knockdowns | Guarding or parrying on the player's wakeup |
| `habit.wakeup.roll` | The player rolls the same way on wakeup in 2 of the last 3 knockdowns | Covering that roll direction |
| `habit.turtle` | Guard held for more than 60% of a 10 s window | Grabs, guard-crushers, unblockables |
| `habit.backdash` | 3 or more backdashes within 10 s | Long-reach punishes and gap-closers |
| `habit.dash.approach` | 3 or more forward dash approaches within 15 s | Pokes at maximum range |
| `habit.combo.repeat` | The same 3-hit starter used 4 or more times within 60 s | Pre-emptive guard or parry after the first hit |
| `habit.shadow.raw` | Umbral Shadow activated from neutral as soon as it fills | Spacing out of echo range, or draining the meter first |
| `habit.rage.raw` | Ember Rage activated from neutral as soon as it fills | Defensive stance and backing off for the duration |
| `habit.stack` *(raids)* | 3 or more players in the same plane within 4 m for 5 s | Plane-wide attacks |

The Ledger is one shared system. 08-Enemies §7 adds more habits for regular enemies, and bosses may
use those too.

**Fairness rules.** (1) The first time a boss uses a learned answer, it plays a unique audio sting,
a fragment of its leitmotif. (2) A learned weight decays if the player stops the habit for 30 s.
(3) A boss can have **at most two learned answers active per phase**. (4) On retry, learned weights
keep 50% of their value ("mercy decay"). Only Aurem and Tamsin carry learning between fights, and
their sheets explain how. (Sable Ascendant and The First Shadow read a campaign-long habit profile,
kept on the device, instead of carrying a fight's weights.)

### 0.4 Soundtrack conventions

- **The Oath Row.** Twelve pitches, one for each line of the Twelvefold Oath (I Devotion to XII
  Sovereignty). Each Oathlord's leitmotif is built on the pitch of their line. Aurem's theme states
  the whole row. The First Shadow's theme is the row inverted.
- **Rhen's theme** comes from the Oathwarden Hymn (see Hask). **Sable's theme** is Rhen's in
  retrograde.
- **The Rebirth Song** is the pre-Oath lullaby. It links Liss, Ossric, the Drowned Choir and the true
  ending.
- Stems follow canon naming: `MUS_Boss_<Name>_P<n>_<Stem>` (e.g. `MUS_Boss_Kessh_P2_Drums`). Phase
  changes crossfade on the next bar line.

### 0.5 Execution conventions

- **Trigger.** Both directions use the canon rule: the target is guard-broken, or at 15% health or
  less and staggered, and the executor presses `Execute` in range (bosses trigger theirs through AI).
  A boss execution on a player at or below 15% health is the defeat cinematic. On a guard-broken
  player above 15%, it deals heavy damage and cuts away before the lethal beat.
- **The player's round-ending execution** triggers the canon slow-motion cinematic finisher.
- **Rating.** PEGI 16 / ESRB M. **No dismemberment, and no decapitation shown**, on any platform.
  Wounds read as ember sparks (Ember) or ink (Umbra). The camera cuts to silhouette or light at the
  lethal frame. Every execution below is written to this standard, so the mobile default needs no
  alternate version.
- **Assets.** `A_Exec_<Boss>_OnRhen` and `A_Exec_Rhen_On<Boss>`. They run 3.5–6 s and can be skipped
  after the first viewing.

### 0.6 Raid conventions (3–4 players)

| Rule | Detail |
|---|---|
| Planes | Raid arenas have two gameplay planes, **Front** and **Rear**. Change plane with `Dodge` + `8` (to Rear) or `Dodge` + `2` (to Front), which has the same invulnerability as a dodge. The boss spans both planes. |
| Scaling | Boss HP is ×1.0 with 3 players and ×1.3 with 4. Posture scales ×1.0 and ×1.2. Stats are capped at the raid's item-level cap. |
| Aggro | Each raid defines its own threat source, and the target is always shown with a sigil above the targeted player. **Perfect Parry always generates threat.** |
| Stagger sharing | The boss has one **shared posture bar**. Coordinated windows (defined per raid) add burst posture damage. When it breaks, every player in range joins a **Chain Execution** (co-op paired execution). |
| Revives | Downed players have a raid-specific **bleed-out**. A standing ally revives them by holding `Execute` for 3 s. The team has **Oath-light Revives** (shown as lanterns) per phase, and they refill at each phase transition. When all players are down, the attempt ends. |
| Friendly fire | Off. Knockback between players is off. |
| Wipe recovery | The raid restarts at the last phase reached, with learned habits cleared. |

---

## 1. Abbot Kessh — the Ash Censer

| | |
|---|---|
| **ID** | `boss.kessh` |
| **Title** | the Ash Censer, Oathlord of Devotion |
| **Region / mission** | `region.emberfall`, `mission.emberfall.10`, recommended Lv 5 |
| **Arena** | **The Belltower of Last Rites** (`SC_Boss_Kessh_Belltower`, a boss variant of `SC_Arena_Emberfall_Belltower`) |

**Arena.** An octagonal terrace at the top of the Belltower, open to the dusk, with ash-snow falling.
The great bronze bell (`SM_Emberfall_Bell_01`) hangs center-rear over the Oathstone of Devotion,
which is set into the floor. **Hazards:**

- **The bell.** Any fighter can strike it with a heavy attack. The shockwave staggers anyone not
  guarding, and Kessh rings it himself in Phase II.
- **Eight censer racks** along the balustrade. They break and spill embers that burn the floor for
  6 s.
- **Balustrade sections.** Each gives one wall bounce and then gives way. In Phase III the whole
  balustrade collapses and the fight drops to the ring-walk below.
- **Ash drifts.** A heavy knockdown into one blinds the fallen fighter for 20 frames.

**Lore.** Kessh was a charcoal-burner's son from the Ashspine slopes. He came to the Choir at seven,
carrying his mother's ash in a clay cup, and has never left the mountain since. He has performed
30,112 Last Rites and can recite every name. He does not believe the tithe is a necessary evil. He
believes it is **love**: the dead kneel, and he keeps them burning, forever, together, in the
Oathstone. The ash of every soul he has tithed is sealed in the tiny censers of his chain so that
none of them will ever be alone. When the Edict of the Living Tithe arrived, he wept all night, and
then he obeyed it. He presided over Rhen's execution, and he takes Rhen's return as a failure of his
own devotion: a rite he did not finish. The first line of the Oath reads *What I kneel to, I keep
burning*. Kessh has spent fifty years burning what kneels to him.

**Signature weapon: *Censer of Last Rites*** (Chain Sword, Censer Chain Style). A segmented blade
whose links are thumb-sized bronze censers, each trailing ash. At full extension it reaches 6 m, and
its tip is a larger censer holding the "Kneel ember". Kessh fights from the center and barely moves,
letting the chain do the walking. His **Litany** is a three-hit whip string that alternates high,
mid and low heights. His ash clouds hang in the air and hide his wind-ups. **The Last Rite** is a
ranged lasso grab that pulls the Shadow meter out of its target.

**Intro cinematic**

1. Ash-snow falls on the empty terrace. The bell hums the Kneel note on its own.
2. Kessh kneels before the Oathstone with his back to the camera, swinging a censer slowly. Under
   the music, a whisper layer recites names.
3. "You left before the rite was finished, child." He stands, and the censer chain uncoils link by
   link down the steps toward Rhen.
4. Sable whispers: "He's going to try to finish it." Rhen draws. (`challenge` node.)
5. Kessh strikes the bell, and for one frame the ash-snow stops in the air. Title card: **ABBOT
   KESSH, THE ASH CENSER.**

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. The Litany** | 100–65% | **Litany**: a 3-hit chain string, high, mid, low, whose second hit can be delayed. **Censer Sweep**: a long horizontal whip that can be jumped. **Ash Veil**: a 3 m ash cloud that lasts 6 s, and attacks thrown from inside it have shorter visual tells. **The Last Rite**: a 5 m lasso grab, techable on the flash, that drains 50% of the Shadow meter on hit. | Static. Censer racks intact. Dusk light. | **Perfect Parry across mixed heights.** Standing guard for the high and mid, crouch guard for the low. Parrying all three Litany hits breaks 35% of his posture. |
| **II. The Bell of Last Rites** | 65–30% | **Toll**: he strikes the bell every 12 s, sending out a shockwave ring that must be guarded, jumped or perfect-dodged (a perfect dodge gives Shadow Time). **Rising Censer**: an anti-air upswing. **Chain Hymn**: an overhead 360° spin that denies space around him. **Kindle**: he whips a censer rack to set the floor burning. | He kicks over two racks, leaving two permanent ember patches that narrow the floor. The bell rings on a cycle. | **Spacing and anti-air discipline.** Stay just outside the chain tip and punish his whiffs. Jump-ins are met with Rising Censer. |
| **III. Last Rite** | 30–0% | He breathes in the ash of his own censers and glows from inside. **Ash Litany**: the Litany with random 0, 10 or 20-frame delays. **Pyre Ring**: the chain becomes a closing ring of fire. **Final Rite**: a command grab that drains 100% of the Shadow meter and deals heavy damage, techable. The ash-snow thickens into a blizzard that cuts visibility. | The first Pyre Ring breaks the balustrade, and the fight drops to the lower ring-walk. It is narrower, with walls at both ends and the cracked bell overhead. | **Perfect Dodge against delayed swings** (he waits out roll-spam), turning Shadow Time into punishes. **Throw teching** Final Rite. |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 45 | 65 | 35 | 30 | 85 | 70 |

- **Learns to punish:** `habit.parrymash` (delays Litany hit 2 by 20 frames), `habit.jumpin` (weights
  Rising Censer), `habit.rollspam` (drops Ash Veil where the roll ends).
- **Tells:** he never retreats. After landing The Last Rite he stops to pray for 40 frames, which is a
  deliberate punish window driven by his Showmanship.

**Soundtrack: "Litany of Ash"**

- *Instrumentation:* a male choir holding a single low drone (the Kneel), tuned temple bells, bowed
  psaltery, frame drums, and pipe-organ pedal in Phase III.
- *Tempo:* P1 72 BPM (chant and sparse bells). P2 96 BPM (frame drums enter, and the bell toll is
  tuned to the key). P3 128 BPM (organ pedal, and the choir splits into shrieking overtones).
- *Leitmotif:* the **Kneel figure**, a descending four-note bell phrase built on Oath Row pitch I.
- *Stems:* `MUS_Boss_Kessh_P1_Choir`, `MUS_Boss_Kessh_P2_Drums`, `MUS_Boss_Kessh_P3_Organ`.

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Censer of Last Rites*** (Chain Sword) | Unlocks Censer Chain Style if the player doesn't have it yet |
| Armor | **Vestments of the Unreturned** (cowl and chest) | Keeps 10% of the Shadow meter when hit |
| Rune | **Rune of the Last Rite** | Grabs drain 15% of the target's Shadow or Rage meter |
| Cosmetic | **Ash-Snow Mantle** | Ash drifts off Rhen's shoulders |

**Executions**

- **Kessh executes Rhen.** The chain wraps Rhen's chest three times and pulls him to his knees. Kessh
  swings the tip-censer slowly over his head, and the smoke draws Rhen's shadow up out of him like a
  thread into the censer. Kessh closes the lid. "Finished." Rhen slumps, grey.
- **Rhen executes Kessh.** Rhen catches the chain in mid-whip, wraps it around his forearm and hauls
  Kessh to his knees in front of the bell. He strikes the bell with Kessh's own tip-censer. The tone
  cracks every censer on the chain, and thirty thousand ash-shadows pour out and wrap around the
  abbot, who sees them for the first time: "…So many." Rhen draws and cuts through the smoke. Kessh
  kneels and comes apart into ash-snow.

---

## 2. Mother Ilvane — the Drowned Bell

| | |
|---|---|
| **ID** | `boss.ilvane` |
| **Title** | the Drowned Bell, Oathlord of Mercy |
| **Region / mission** | `region.weepingreeds`, `mission.weepingreeds.10`, recommended Lv 10 |
| **Arena** | **The Bell Causeway Chapel** (`SC_Boss_Ilvane_BellCauseway`) |

**Arena.** The end of a quarter-mile rotting causeway. There is a roofless chapel, and the Tidebell
hangs from a gallows-frame above the Oathstone of Mercy. Rain falls, reeds fill the far layer, and
bells hang on poles along the planks. **Hazards:**

- **The tide rises during the fight.** Dry in Phase I. Ankle-deep in Phase II, which adds 6 frames of
  dodge recovery. Waist-deep in the center in Phase III.
- **Planks** break under a ground bounce and leave holes.
- **Pole-bells**, when struck, send out a slowing ripple.
- **Funeral lanterns** drift across the fight and burst into steam that blocks vision.

**Lore.** Ilvane was the Reeds' midwife for thirty years and delivered half the marsh. In 471 AS the
Mourning Flood took the chapel while her three daughters were singing the evening hymn inside. She
dove for them for three days. When the Choir came to tithe the bodies, she hid them under the water,
and found that the censers could not find a shadow beneath the Reeds. From that she built a
doctrine. In the Houses of Quiet Water, loose-shadowed children are "gently" drowned before the
Choir can take them, so that their shadows join her daughters' choir under the chapel instead of the
stone. She believes she is saving them. Every dusk she rings the Tidebell to hear her girls answer.
Mercy's line, *What I pull from the water, I do not hold under*, has become its own opposite.

**Signature weapon: *Tidebell*** (Naginata, Tidewheel Style). A long curved blade with a bronze bell
hung beneath its collar. Every ring sends a ripple across the water. Ilvane fights in wide sweeping
arcs and low sweeps that force crouch guard. Each full spin adds a **Tide stack**. At three stacks,
her next spin becomes **Undertow Spin**, an unblockable low that has to be jumped or perfect-dodged.

**Intro cinematic**

1. Rain on black water. A paper boat drifts past with a child's shoe in it.
2. Ilvane stands waist-deep beside the causeway, singing a lullaby and rocking the Tidebell like a
   cradle.
3. She sees Liss behind Rhen. "Oh, sweetling. You've come home to the water."
4. Rhen steps in front of the child. Ilvane's face hardens, gently. "You'd give her to the stone,
   Oathwarden? I'd give her to her sisters."
5. She rings the Tidebell once, and three voices answer from under the causeway. **MOTHER ILVANE, THE
   DROWNED BELL.**

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. Lullaby** | 100–70% | **Low Tide**: two low sweeps. **Cradle Arc**: a wide high-mid arc with 5 m reach. **Rocking Step**: a backstep into a lunging thrust. **Toll**: a bell ripple that slows by 20% for 2 s. | Dry causeway, rain. | **Crouch guard and reading high versus low.** The bell sways left before a low and right before a high, a paired audio-visual tell. |
| **II. The Rising Tide** | 70–40% | **Tidewheel**: a spinning combo that builds Tide stacks, and at 3 stacks becomes **Undertow Spin**. **Pole-Bell Chain**: she rings the causeway bells in sequence to send traveling ripples. **Hook Pull**: the naginata hook drags the player to her. | The water is ankle-deep (+6 frames dodge recovery), and planks break into holes. | **Interrupting momentum.** Punish between spins before the stacks reach 3, and perfect-dodge Undertow Spin. |
| **III. Under** | 40–0% | **Undertow**: she dives and comes up under the player as a command grab, techable on the ripple flash. **Drowning Embrace**: a grab that holds Rhen underwater if not teched. **Chorus**: three hand-shaped Husks rise and cling, shaken off with `Dodge`. **Last Lullaby**: a full-circle sweep below 15% HP. | The causeway collapses into waist-deep water. Only two dry platforms remain, the chapel steps and the bell-frame. | **Throw teching** (Undertow, Drowning Embrace) and **Perfect Dodge** against ripple-telegraphed surfacing. |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 40 | 80 | 50 | 55 | 60 | 35 |

- **Learns to punish:** `habit.turtle` (a crouching turtle is answered with the overhead **Bell
  Drop**), `habit.rollspam` (Toll timed to roll recovery), `habit.wakeup.roll` (Low Tide covers the
  roll direction).
- **Tells:** "a mother's hesitation". Whenever Rhen stands between her and the chapel steps where
  Liss watches, she hesitates for 30 frames.

**Soundtrack: "Mercy Under Water"**

- *Instrumentation:* solo female voice singing a lullaby in 6/8, hurdy-gurdy drone, handbells, a cello
  section, and underwater-filtered girls' choir (her daughters) in Phase III.
- *Tempo:* P1 60 BPM (rocking 6/8). P2 84 BPM (hurdy-gurdy, handbells marking the tolls). P3 110 BPM
  (cello ostinato as the choir surfaces).
- *Leitmotif:* a **three-note cradle figure** on Oath Row pitch II. The Drowned Choir raid sets the
  same three notes in harmony.

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Tidebell*** (Naginata) | Unlocks Tidewheel Style |
| Armor | **Shroud of Quiet Water** | Resistance to slows and water effects |
| Rune | **Rune of the Tolling Bell** | A Perfect Dodge sends out a small slowing ripple |
| Cosmetic | **Reedwife's Lantern** | A hand-lantern idle and emote |

**Executions**

- **Ilvane executes Rhen.** She hooks Rhen with the naginata and lays him back into the water, almost
  tenderly. She rests the Tidebell on his chest and rings it. Bubbles rise, and his shadow sinks away
  beneath him while she hums.
- **Rhen executes Ilvane.** Rhen cuts the Tidebell's cord and it drops into the water. From below,
  three voices sing her lullaby back to her. Ilvane lets go of the naginata, turns toward the sound
  and whispers "My girls…". Rhen's last cut severs her Oath-tether across the surface of the water,
  and she walks down into the deep toward the voices as the ripples close over her.

---

## 3. Gorran Vox — the Anvil King

| | |
|---|---|
| **ID** | `boss.gorran` |
| **Title** | the Anvil King, Oathlord of Labor |
| **Region / mission** | `region.ironroot`, `mission.ironroot.10`, recommended Lv 15 |
| **Arena** | **The Great Crucible** (`SC_Boss_Gorran_Crucible`) |

**Arena.** A circular casting floor inside the caldera. The walls are lined with shadow clamps, and
the clamped shadows writhe in them. The Oathstone of Labor is set into a giant anvil at the rear.
Behind the far wall, the sleeping face of the Ferrous Maw is visible, and its eyes light up in Phase
III. **Hazards:**

- **Pour-channels.** Every 20 s one section of the floor floods with molten metal, after a 2 s klaxon warning.
- **Hanging chains.** `Jump` + `Grab` swings on one to cross a pour.
- **Slag walls** break to open escape routes.
- **Shadow clamps.** In Phase III, a player hit on a clamp frees the shadow inside, which strikes
  Gorran once.

**Lore.** Gorran was born under a clamp: his mother's shadow was on the line the day he came into
the world. He was apprenticed at eight and was Ironroot's fastest striker by eighteen. At
twenty-two he led the Cinder Rising and burned the guild ledgers in Bellows Square, and the workers
crowned him. A year later the Throne's tithe demand had not changed by a single shadow, so Gorran
made the arithmetic work the only way it could. He doubled the Shadow Shifts, and paid wages for
them. He believes he freed Ironroot and fed it at the same time, and he isn't wrong about the wages.
Labor's line reads *What my hands make, my hands owe nothing*. Gorran made every hand owe
everything.

**Signature weapon: *Worldanvil*** (War Hammer, Anvil Style). A two-handed hammer whose head is a
small anvil wrapped in the chains of the old guild ledgers. Every strike leaves a glowing tally mark
on whatever it hits. Gorran's combos follow Ironroot's **shift rhythm**: long, short, short. The
third beat is always a guard-crusher, and his heavy attacks carry hyper armor.

**Intro cinematic**

1. The workers' shadows writhe in their clamps. The only sound is the hammer rhythm.
2. Gorran is forging at the anvil with his back to Rhen, and doesn't turn: "Shift's not over."
3. He quenches a blade in a burst of steam, turns and looks Rhen over. "Oathwarden. Dead one. You've
   got good hands. I could use them."
4. Rhen answers (`challenge` node). Gorran shrugs and hefts the Worldanvil. "Everyone works. Even the
   dead."
5. He strikes the anvil and the pour-gates open. **GORRAN VOX, THE ANVIL KING.**

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. First Shift** | 100–70% | **Long-Short-Short**: a 3-beat string whose third beat crushes guard. **Tally Stamp**: an overhead with hyper armor. **Bellows Rise**: an anti-air uppercut. **Ledger Chain**: the chained head whips out 4 m. | Pour-channels on a 20 s cycle. | **Perfect Parry on rhythm.** Parrying the third beat breaks his armor and 30% of his posture. Space out of the guard-crusher. |
| **II. Overtime** | 70–35% | **Ground Bounce Slam**: a floor slam that bounces the player into his juggle. **Rivet Throw**: a command grab that throws the player into the wall clamps, techable. **Pour Call**: opens two pour-channels at once. **Slag Wall**: raises walls that split the arena. | Two pour-channels active at once. The chains become the main way out. | **Throw teching** (Rivet Throw) and **air combos**: every whiffed slam leaves him open to an air route from the chains. |
| **III. Strike** | 35–0% | He chains himself to the engine. He loses hyper armor but moves 25% faster, with wild strings and **Last Wage** (he throws the hammer and fights barehanded for 4 s). **Maw's Arm**: the Ferrous Maw's arm sweeps in from the background. | The Crucible tilts 5°, so the pour flows to one side. Clamps can be broken to free shadows. | **Perfect Dodge** (Maw's Arm and fast strings) and **anti-air** against his leaping slams. |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 70 | 30 | 45 | 25 | 90 | 60 |

- **Learns to punish:** `habit.turtle` (more guard-crushers), `habit.backdash` (Ledger Chain after a
  backdash), `habit.jumpin` (weights Bellows Rise).
- **Stance overrides:** *Taunt* adds Aggression +20 (not the default +15). *Salute* makes him open with
  a telegraphed Tally Stamp.

**Soundtrack: "The Anvil's Hymn"**

- *Instrumentation:* anvils tuned to pitches, industrial percussion (sheet metal and chains), low
  brass, and a male work-chant.
- *Tempo:* P1 90 BPM. P2 110 BPM. P3 140 BPM, where the Ferrous Maw's engine groan enters in key.
- *Leitmotif:* the long-short-short anvil rhythm under a rising three-note brass call on Oath Row
  pitch III.

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Worldanvil*** (War Hammer) | Unlocks Anvil Style |
| Armor | **Foreman's Scaled Apron** | Charged attacks gain +1 hit of armor |
| Rune | **Rune of the Long Shift** | The third hit of any string deals +10% posture damage |
| Cosmetic | **Molten Footprints** | A glowing footprint trail |

**Executions**

- **Gorran executes Rhen.** He lays Rhen across the Worldanvil like work on the bench and stamps a
  tally mark into his breastplate with his fist. A clamp on the wall snaps shut on Rhen's shadow and
  drags it into the line.
- **Rhen executes Gorran.** Rhen loops a hanging chain around Gorran's hammer arm and kicks the engine
  lever. The chain hauls Gorran back against the clamp wall, and the hands of the freed shadows close
  on him. Rhen cuts the chain in a shower of sparks. Gorran sinks to his knees in cooling metal that
  sets around him. "…Shift's over."

---

## 4. Ysolde & Yrrah — the Paired Petals

| | |
|---|---|
| **ID** | `boss.petals` |
| **Title** | the Paired Petals, Oathlords of Kinship |
| **Region / mission** | `region.silkwind`, `mission.silkwind.10`, recommended Lv 20 |
| **Arena** | **The Twin Pavilion** (`SC_Boss_Petals_TwinPavilion`) |

**Arena.** A lacquered pavilion over a still black pond, with the two Oathstones of Kinship bound
together by red silk beneath it. The pond is a **reflection layer**, a mirrored surface where shadow
bodies move. Petals fall throughout. **Hazards:**

- **Reflection layer.** In Phase II, the sister in the reflection can only be hit by striking the water
  where her reflection is (down-attacks) or with Umbral Shadow echoes, which land in both layers.
- **Lacquered screens** can be broken.
- **Drifting lanterns** explode when hit.
- **Red threads** in Phase III work as tripwires that knock down anyone who dashes through them.

**Lore.** The twins were born into the Silkwind loom-house that holds Kinship and, as custom
demands, were tied wrist to wrist with red thread at birth. Ysolde was quick and fierce, and Yrrah
was slow and kind. When Yrrah died of marsh fever at nineteen (488 AS), Ysolde, heir to Kinship,
refused to cut the thread. She wove her sister's shadow to her own with oath-silk from the stone. For
twelve years they have ruled together, and each year Yrrah fades a little further and takes a little
of Ysolde with her. The **Binding Rite** is Ysolde's law: no family in Silkwind will ever have to
lose anyone again. Kinship's line is *Whom I call blood, I do not bind*, and it has become the law
that binds every sibling in the groves.

**Signature weapon: *Thorn & Bloom*** (Dual Blades, Twin Petal Style). **Thorn** is Ysolde's hooked
black blade, and **Bloom** is Yrrah's pale, leaf-shaped one. They fight in long, fast strings with a
high hit rate but little posture damage. One sister's string is echoed by the other from the
opposite side, a beat late.

**Intro cinematic**

1. Petals fall onto still black water. Two lanterns drift together and touch.
2. Ysolde dances alone on the pavilion. Her reflection in the pond moves a heartbeat behind her, and
   it isn't her reflection.
3. Yrrah rises from the water behind her as a shadow and rests her chin on Ysolde's shoulder.
4. Ysolde: "You cut Cato's thread. Do you know what it is to be the one left?" Yrrah, a beat later,
   softly: "…Let go."
5. Ysolde draws Thorn and Yrrah draws Bloom. **YSOLDE & YRRAH, THE PAIRED PETALS.**
   - *Plead* stance: Rhen says "She's asking you to let go." Ysolde hesitates, and Yrrah skips her first
     attack.

**Combat phases** (one shared bar, the **Kinship bar**)

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. Thorn** | 100–65% | Ysolde fights in the flesh with **Petal String** (7 hits, high and mid), **Thorn Hook** (pull-in) and **Pirouette** (a spinning cross-up). Yrrah repeats each string from the opposite side 30 frames later. | The pavilion floor with drifting lanterns. | **Cross-up blocking and spacing.** Keep both sisters on one side. Perfect-parry Petal String's final hit (Bloom's echo must be blocked, not parried). |
| **II. Bloom** | 65–30% | They swap: Yrrah is solid and Ysolde is shadow. Yrrah has **Bloom Arc** (a slow drift), **Petal Storm** (wind that pushes toward the edges) and **Thread Snare**. Ysolde attacks from the reflection. | The lacquer screens shatter, the petal-storm blows, and the reflection layer is active. | **Perfect Dodge.** Yrrah's slow arcs have wide perfect-dodge windows. Use the Shadow Time and Umbral Shadow to hit Ysolde in the water. |
| **III. One Bloom** | 30–0% | They merge into one four-armed dancer. **Kinship String**: high/low mixups with four blades. **Red Thread**: a command grab that ties the player up, techable. **Last Dance**: a 12-hit super string with a hidden gap after hit 8. | The pavilion comes loose and slowly turns on the pond. Threads cross the arena. | **Perfect Parry against long strings** (find the gap) and **throw teching**. |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 75 | 40 | 60 | 65 | 55 | 85 |

- **Learns to punish:** `habit.parrymash` (delays inside strings), `habit.rollspam` (Yrrah waits at
  the roll's exit), `habit.jumpin` (one sister anti-airs while the other presses).
- **Tells:** 10% of Yrrah's attacks are deliberately pulled short (the "let go" tell). A `clash` line
  from Yrrah to Sable plays in Phase II: "You too? …Does he hold on too tight?"

**Soundtrack: "Two Threads"**

- *Instrumentation:* two solo violins in strict canon, a bar apart, with plucked zither, wind chimes
  and frame drum.
- *Tempo:* P1 100 BPM. P2 120 BPM (the violins swap the lead). P3 150 BPM in 7/8 (the canon collapses
  into unison).
- *Leitmotif:* a two-voice canon on Oath Row pitch IV, the same melody offset by one bar.

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Thorn & Bloom*** (Dual Blades) | Unlocks Twin Petal Style |
| Armor | **Loom-Sister Silks** | Faster recovery after blocking |
| Rune | **Rune of the Paired Petal** | Every 5th hit of a string repeats as a pale echo at 30% damage |
| Cosmetic | **Petal Wake** | A petal trail when dashing |

**Executions**

- **The Petals execute Rhen.** Thorn and Bloom cross at his throat from either side. The sisters spin
  him between them like thread on a spindle and stitch his shadow to the floor with red silk.
- **Rhen executes the Petals.** Rhen cuts the red thread between them. Yrrah's shadow floats up, free,
  and touches her sister's cheek. Rhen's second cut is quick. Ysolde falls beside the water, her hand
  closed on the loose end of the thread, as petals fall. What becomes of Yrrah is decided by the Oath
  Choice.

---

## 5. Seraph Maal — the Sunscorched

| | |
|---|---|
| **ID** | `boss.maal` |
| **Title** | the Sunscorched, Oathlord of Faith |
| **Region / mission** | `region.glassossuary`, `mission.glassossuary.10`, recommended Lv 25 |
| **Arena** | **The Noon Cathedral** (`SC_Boss_Maal_NoonCathedral`) |

**Arena.** The nave of a cathedral built inside a Sleeper's skull. Two beams of noon sun come
through the eye sockets, a ring of mirrors stands on pivots, and the floor is stained glass. On the
altar, the clear glass Oathstone of Faith holds its fuelless flame. **Hazards:**

- **Sunbeams** burn 5% health per second while you stand in them.
- **Mirrors** turn 45° when hit and redirect the beams. Aiming a beam at Maal staggers him.
- **Stained-glass panels** crack under ground bounces, and in Phase III they give way to the chamber
  below.

**Lore.** Maal was born in a pilgrim caravan and walked the Glass Sea before he could talk. At twenty
he climbed to the Cathedral and stared through the skull's eye into the noon sun for forty days,
praying to see the face of Aurem, the Sun made flesh. On the fortieth day he saw, he says,
*everything*, and went blind. The Throne made him Lord of Faith for the miracle. He sells pilgrims'
shadows to the stone as the **Noon Tithe**, promising each one it will rise to the Sun, and he is
kind to every single one of them. His Glassblind blind themselves in imitation of him. Faith's line
is *What I cannot see, I will not sell*. Maal has sold everything he cannot see.

**Signature weapon: *Noonpiercer*** (Spear, Pierce Line Style). A long spear with a mirrored blade
that catches the light, its shaft wrapped in pilgrims' prayer ribbons. Maal fights at the far edge
of his reach. He pokes and spaces, and pole-vaults over or away from the player. He is blind: in
Phase II he tracks **sound events** (dodges, dashes, jumps and landings), not sight.

**Intro cinematic**

1. Pilgrims kneel in the nave, and a shaft of noon light moves across them one at a time.
2. Maal preaches, eyes bound in gold cloth: "…and the Sun shall see what we cannot."
3. He stops in the middle of a sentence and tilts his head toward Rhen's footsteps. "You walk like a
   man with two shadows."
4. The pilgrims scatter. Maal smiles and lifts Noonpiercer, and its blade throws the beam into Rhen's
   eyes in a white screen flare.
5. **SERAPH MAAL, THE SUNSCORCHED.**

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. Morning Sermon** | 100–70% | **Pierce Line**: three pokes at stepped ranges. **Vault Strike**: a pole-vault cross-up. **Glare**: a blade flash that hides the next poke. **Sweep of the Faithful**: a low sweep. | Static beams. | **Spacing.** Whiff-punish his pokes at max range, and close the gap with a Perfect Dodge through Pierce Line. |
| **II. Blind Faith** | 70–40% | He tears off the bandage. **Sound tracking**: each dodge, dash or jump pings the player's position to him. A player who stands still or walks is lost to him, and he strikes their last known position. **Mirror Turn**: beams sweep across the floor. **Sun Lance**: he throws the spear along a beam, and it returns. | The beams sweep. | **Patience and Perfect Parry.** Stay quiet and parry his searching thrusts. Mashing dodge gives away your position. |
| **III. Zenith** | 40–0% | A third beam comes through the crown of the skull. **Zenith Rise**: he vaults to the ceiling and dives. **Noon Judgment**: three columns of light at the player's position, 1 s apart. **Faith's Embrace**: a grab that pins Rhen in a beam, techable. | The floor glare pulses. Stained glass gives way to a smaller lower chamber. | **Anti-air** (anti-airing Zenith Rise causes a big stagger) and **Perfect Dodge** timing on Noon Judgment. |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 50 | 70 | 55 | 40 | 95 | 75 |

- **Learns to punish:** `habit.dash.approach` (pokes at max range), `habit.jumpin` (Zenith Rise),
  `habit.backdash` (a vault that follows the backdash). In Phase II, `habit.rollspam` is punished by
  design, since every roll pings.
- **Tells:** he preaches between exchanges. A full verse (60 frames) is a safe opening, a result of
  his Showmanship.

**Soundtrack: "Noon Hymnal"**

- *Instrumentation:* fretless lute, reed flute, a high-register women's choir, a sun-gong and frame
  drum.
- *Tempo:* P1 76 BPM. P2 96 BPM (the choir drops out, and the player's footsteps are mixed forward so
  you can hear what he hears). P3 132 BPM (full choir, and the gong on every Noon Judgment).
- *Leitmotif:* a rising five-note **sunrise figure** on Oath Row pitch V. It never resolves until his
  execution.

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Noonpiercer*** (Spear) | Unlocks Pierce Line Style |
| Armor | **Mirrorplate of the Zenith** | Resistance to burning and light effects |
| Rune | **Rune of the Glare** | A Perfect Parry flashes the attacker and delays their next action by 10 frames |
| Cosmetic | **Scorched Halo** | A faint sun-ring overhead |

**Executions**

- **Maal executes Rhen.** He pins Rhen to the floor inside a sunbeam with Noonpiercer's haft, and the
  light burns Rhen's shadow down to nothing. Maal kneels beside him and prays.
- **Rhen executes Maal.** Rhen turns the last mirror and the beam hits Maal full in his blind eyes.
  For one moment he **sees**: an empty throne and a man with no shadow. He weeps. Rhen draws and
  cuts. Maal falls onto the glass, and it fuses around him into a kneeling statue.

---

## 6. Tharuk Greymane — the Winter Wolf

| | |
|---|---|
| **ID** | `boss.tharuk` |
| **Title** | the Winter Wolf, Oathlord of Strength |
| **Region / mission** | `region.rimewood`, `mission.rimewood.10`, recommended Lv 30 |
| **Arena** | **The Howling Cairn** (`SC_Boss_Tharuk_HowlingCairn`) |

**Arena.** A ring of stacked stones on a hilltop, with the Oathstone of Strength chained at its heart.
Hundreds of carved name-stones are heaped around the chains. There are wolf-skull totems and cracked
ice plates, with a blizzard blowing and the aurora overhead. **Hazards:**

- **Whiteout cycles.** Visibility drops to 4 m for 5 s, every 20 s in Phase I and every 12 s in Phase
  II.
- **Ice plates** crack after three heavy impacts and expose freezing water, which slows movement and
  drains Ember.
- **Three wolf-skull totems** fuel his war-howl speed buff. Breaking all three removes it.
- **Frost hounds** join in Phase II.

**Lore.** Tharuk was a clanless orphan who fought his way to chief of the Greymane by twenty and, by
thirty, had united the steppe against the Unsworn raids. The Throne offered him the Oathstone of
Strength and a bargain: keep the tithe paid, and the aurora would shelter the clans. The tithe was
heavier than the clans could carry, so he chose who would pay it. That was the **Culling Moot**. He
has walked every culled elder out into the snow himself, carved every name, and stacked the stones
around his Oathstone. He adopted Kirra, the daughter of a woman he culled. Strength's line is *What I
can break, I choose to carry*. He broke the ones he should have carried, and he knows it.

**Signature weapon: *Fangs of the Long Night*** (Gauntlets, Iron Fang Style). Black iron gauntlets
with wolf-fang knuckles, frost blooming on the iron. Tharuk is pure rushdown. He has the fastest
startup in the game, grabs out of everything, and never leaves your face.

**Intro cinematic**

1. Blizzard. Wolves howl in a ring around the cairn.
2. Tharuk sits on the Oathstone's chains, carving a name into a stone, one of hundreds.
3. "Every one of these, I carried out myself. Did you carry yours, Oathwarden?"
4. If `flag.kirra.spared`, Kirra's voice comes from off-screen, "Father…", and he doesn't turn. If
   not, he sets down a stone carved **KIRRA**.
5. He cracks his knuckles and frost blooms across them. **THARUK GREYMANE, THE WINTER WOLF.**

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. The Moot** | 100–70% | **Fang Rush**: a very fast 4-hit string. **Wolf's Grip**: a grab, techable. **Rime Hook**: an overhead. **Den Guard**: a parry stance that counter-grabs strikes. | The normal blizzard cycle. | **Throw teching and guard discipline** against the fastest startup in the game. Perfect-parry the last hit of Fang Rush. Don't strike into Den Guard. |
| **II. Whiteout** | 70–35% | During each whiteout he vanishes and lunges from the direction of the howl (**Pack Lunge**, an audio tell). Two **frost hounds** join. The totems give him a speed buff. | Whiteouts every 12 s. Hounds on the field. | **Perfect Dodge by sound** into Shadow Time. Break the totems. |
| **III. The Long Night** | 35–0% | The aurora turns black. **Permanent berserk**: +damage, and 1-hit armor on his heavies. **Long Night** chains Fang Rush into grabs. **Carry**: he lifts the player and slams them through the ice, techable. If `flag.kirra.spared`, at 20% HP Kirra's voice calls out and he freezes for 5 s (a free punish window and a `clash` line). | Ice breaks, opening pools of freezing water. | **Parry and counter under pressure**, and **spacing** that uses the water to stay out of grab range. |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 90 | 20 | 50 | 35 | 95 | 45 |

- **Learns to punish:** `habit.turtle` (grabs), `habit.backdash` (Pack Lunge), `habit.wakeup.attack`
  (guards on the player's wakeup and punishes), `habit.parrymash` (switches to throws).

**Soundtrack: "The Long Night"**

- *Instrumentation:* overtone chant, bowed bass-fiddle, frame drums, a wolf-horn and wind textures.
- *Tempo:* P1 120 BPM. P2 140 BPM (during whiteouts the drums drop out, leaving only the howl and a
  heartbeat pulse). P3 160 BPM (horns come in, and the chant becomes a roar).
- *Leitmotif:* a howling horn glissando on Oath Row pitch VI. Eirmund's hunting horn answers it in the
  same key.

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Fangs of the Long Night*** (Gauntlets) | Unlocks Iron Fang Style |
| Armor | **Greymane Pelt-Cloak** | Resistance to cold and slow effects |
| Rune | **Rune of the Pack** | Throws deal +15% damage and briefly slow with frost |
| Cosmetic | **Aurora Breath** | Visible breath tinted with aurora colors |

**Executions**

- **Tharuk executes Rhen.** He lifts Rhen by the throat, slams him through the ice, and holds him
  under the frozen surface until his shadow freezes and cracks like the ice above him.
- **Rhen executes Tharuk.** Rhen breaks his guard, and the old wolf drops to one knee in the snow.
  Rhen takes the one uncarved stone from the heap, the one Tharuk was saving for himself, and puts it
  in his hand. One cut. Snow settles over him, and the wolves stop howling.

---

## 7. Duke Veloran Sae — the Masquerade

| | |
|---|---|
| **ID** | `boss.veloran` |
| **Title** | the Masquerade, Oathlord of Loyalty |
| **Region / mission** | `region.lanternhold`, `mission.lanternhold.10`, recommended Lv 35 |
| **Arena** | **The Ballroom of Ten Thousand Lanterns** (`SC_Boss_Veloran_Ballroom`, a boss variant of `SC_Arena_Lanternhold_Ballroom`) |

**Arena.** A ballroom with mirrored walls and a gallery. Masked dancers waltz across the floor, and
chandeliers hang from chains overhead. The Oathstone of Loyalty sits in the throne-niche wearing a
porcelain mask, beside a lantern that has burned for 288 years. **Hazards:**

- **Chandeliers** drop when their chains are cut, dealing damage and leaving fire.
- **Masked dancers.** A knockback into one causes a stumble. In Phase II they crowd the floor and
  Veloran hides among them.
- **Mirrored walls** shatter into shard hazards.
- **Lowered lanterns** in Phase II create zones of darkness.

**Lore.** Veloran was the youngest son of House Sae, which swore Loyalty in 212 AS, and was never
meant to rule. In 488 AS he poisoned his uncle, Lord Castor Sae, and had Castor's widow, Lady
Corvaine, executed for it. That was the "Ninth Mourning". As Duke he enforces the **Masque Oath**:
every citizen wears a mask sworn to him, and each mask holds a sliver of its wearer's shadow as
collateral against disloyalty. He has worn so many masks for so long that nothing is left under the
last one except his shadow, wearing his face. He took Chancellor Venn's commission and hired Tamsin
to fill it. Loyalty's line is *Whom I serve, I may still refuse*. Veloran has made refusal
impossible.

**Signature weapon: *Silken Lash*** (Whip Blade, Silken Lash Style). A segmented blade bound in silk
that uncoils into a whip, embroidered with the names of everyone sworn to him. It has the longest
reach in the game. Veloran zones, snaps at range and pulls, and he dances between attacks. Every third
action is a flourish.

**Intro cinematic**

1. A waltz. A thousand masked dancers turn in perfect unison.
2. The music stops, and every dancer turns to look at Rhen at the same moment.
3. Veloran comes down the stairs, applauding slowly. "The dead man arrives at the ball. Delightful.
   You're underdressed."
4. He offers Rhen a mask. Rhen refuses it or cuts it in half (`challenge` node). "Pity. It would have
   suited you."
5. The Silken Lash uncoils like a ribbon and the dancers start again. **DUKE VELORAN SAE, THE
   MASQUERADE.**

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. The Waltz** | 100–70% | **Lash Snap**: a 7 m snap. **Figure Eight**: a zoning pattern. **Reel**: a pull-in. **Bow**: a flourish that is a feint half the time. | Dancers circle the edges, lanterns lit. | **Approach and spacing.** Perfect-dodge through his snaps to close in. Chasing at max range is exactly what he wants. |
| **II. Change of Masks** | 70–35% | The lanterns lower. Veloran vanishes into the dancers, and **three masked decoys** fight with his moveset. A hit shatters a decoy's mask, while the real Duke's mask has a hairline gold seam. **Masque Swap**: he trades places with a decoy. | Dancers fill the floor, with zones of darkness. | **Reading under pressure.** Find the real Duke, and don't spend an Ultimate on a decoy. |
| **III. Unmasked** | 35–0% | His mask breaks, and there is no face underneath, only his shadow wearing one. **Lash Pull**: a long-range command grab, techable. **Chandelier Drop**. **Gallery Dive**: he leaps to the gallery and dives. **Last Dance**: a whip vortex. | Chandeliers fall, mirrors shatter, and the dancers flee. | **Throw teching** (Lash Pull) and **anti-air** (Gallery Dive). |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 40 | 70 | 75 | 90 | 30 | 100 |

- **Learns to punish:** `habit.rollspam`, `habit.jumpin`, `habit.dash.approach`, `habit.parrymash`.
- **Tells:** his Courage is low, so below 50% health he retreats to the gallery for 3 s at a time. His
  Showmanship is 100, so every third action is a flourish with a 30-frame opening.

**Soundtrack: "Masquerade for Ten Thousand Lanterns"**

- *Instrumentation:* string quartet, harpsichord, glass harmonica, and brush snare.
- *Tempo:* P1 waltz in 3/4 at 120 BPM. P2 144 BPM (the quartet detunes, the waltz syncopates, and the
  dancers' feet become the percussion). P3 172 BPM in 6/8, a danse macabre with a shrieking glass
  harmonica.
- *Leitmotif:* a twirling waltz figure on Oath Row pitch VII that stops on an unresolved chord (the
  missing face).

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Silken Lash*** (Whip Blade) | Unlocks Silken Lash Style |
| Armor | **Ducal Masquerade Coat** | +10% damage on the first hit after a feint |
| Rune | **Rune of the Unmasked** | A Perfect Dodge through a long-range attack grants a free dash |
| Cosmetic | **Mask of a Hundred Smiles** | A face mask whose expression changes with the combat state |

**Executions**

- **Veloran executes Rhen.** He wraps Rhen in the lash, spins him into a waltz, and lifts away a mask
  from Rhen's face that shouldn't be there. Rhen's shadow peels off with it, and the Duke hangs it on
  the wall with a thousand others.
- **Rhen executes Veloran.** Rhen catches the lash and reels the Duke into the dance. As they turn,
  the lanterns go out one by one. At the last lantern, Rhen cuts. The Duke's clothes collapse empty,
  and the faceless mask rolls across the ballroom floor.

---

## 8. Queen Myrrhen — the Mycelial Bride

| | |
|---|---|
| **ID** | `boss.myrrhen` |
| **Title** | the Mycelial Bride, Oathlord of Life |
| **Region / mission** | `region.verdantrot`, `mission.verdantrot.10`, recommended Lv 40 |
| **Arena** | **The Heartwood Altar** (`SC_Boss_Myrrhen_Heartwood`) |

**Arena.** A wedding altar grown into the base of the Heartwood, under arches of root and hanging
lace. The green Oathstone of Life is set into the trunk and beats like a heart, and the rooted dead
sit in the pews. **Hazards:**

- **Spore pods** burst when hit, blocking vision and draining Ember.
- **Root walls** can be cut down to open new paths.
- **Sap pools** slow movement.
- **Rooted pews.** In Phase II the dead in them reach out and form grab zones.
- **The mycelial floor** in Phase III roots anyone who stands still for 2 s, holding them for 1 s.

**Lore.** The Greenfold starved in 402 AS. A Throne naturalist, Evander Crale, found that the fungal
Rot would answer a bargain, and he promised it his daughter in exchange for a harvest. Myrrhen was
nineteen. She walked into the jungle in her mother's wedding dress, and the Rot married her. The
harvest came. Myrrhen did not die, because the mycelium keeps her. For ninety-eight years she has
married the dying of the Greenfold to the Rot to keep their shadows from the tithe, and not one of
them has rested. She is kind and endlessly patient, and she cannot understand why anyone would want
to end. Life's line is *What lives, I let end*. Myrrhen lets nothing end.

**Signature weapon: *Bridal Reaper*** (Scythe, Harvest Style). A scythe with a blade of grown bone and
a living root for a handle, wrapped in lace. She fights with pull-in hooks and glides through the
player for cross-ups, and her hits drain Ember into her.

**Intro cinematic**

1. A wedding procession of the rooted dead walks slowly toward the altar, while a wooden organ
   creaks.
2. Myrrhen waits beneath a veil of glowing lace. She is lovely, and very young.
3. If Rhen has met Evander, his voice whispers "Myrrhen…". She doesn't hear it. She lifts her veil and
   smiles at Rhen: "You're dead, and still walking. Come. Let me keep you."
4. Roots offer Rhen a ring of bone. He refuses (`challenge` node).
5. She takes up the Bridal Reaper like a bouquet. **QUEEN MYRRHEN, THE MYCELIAL BRIDE.**

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. The Proposal** | 100–65% | **Reaping Hook**: a pull-in. **Bridal Glide**: a cross-up through the player. **Lace Veil**: a spore cloud. **Harvest**: a 3-hit string that drains 20% of the Ember meter. | Static. Glowing spores drift. | **Blocking cross-ups** and **spacing** to stay out of hook range. |
| **II. The Procession** | 65–30% | **Root Grab**: roots erupt from the floor with a visible tell. **Spore-Brides**: she splits into three, and the two false brides burst into spores when hit. **Kiss of the Rot**: a grab, techable. | The pews wake, root walls rise, and spore pods ripen. | **Perfect Dodge** (the brides' lunges) and **throw teching**. |
| **III. Wedding Night** | 30–0% | She joins with the Heartwood and floats, rooted. **Heartwood Reaping**: huge sweeps from above. **Unending**: she heals through her roots while grounded. Keeping her **airborne** cuts the roots. | The mycelial floor roots anyone standing still. | **Air combos** (juggle her to stop the healing) and **anti-air**. |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 55 | 60 | 60 | 70 | 50 | 80 |

- **Learns to punish:** `habit.turtle` (answered with Kiss of the Rot), `habit.wakeup.roll`,
  `habit.jumpin`.
- **Tells:** she never hurries. After landing a combo she pauses for 40 frames to "admire" the player.

**Soundtrack: "Bridal March of the Rot"**

- *Instrumentation:* a detuned wooden pipe organ, music box, bass clarinet, bone marimba and a
  whispering choir.
- *Tempo:* P1 88 BPM. P2 104 BPM (the music-box theme speeds up and slips). P3 126 BPM (full organ,
  and the choir of the rooted).
- *Leitmotif:* a wedding march in a minor key with one missing beat, on Oath Row pitch VIII.

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Bridal Reaper*** (Scythe) | Unlocks Harvest Style |
| Armor | **Veil of Glowcap Lace** | Resistance to spores and drain |
| Rune | **Rune of the Bride's Bouquet** | Each successful hook pull-in grants a stack of Ember |
| Cosmetic | **Spore Bouquet** | A victory pose with a glowing bouquet |

**Executions**

- **Myrrhen executes Rhen.** She hooks Rhen close and kisses his brow. Roots rise and wrap him, glowing
  flowers open along his armor, and he becomes a groom kneeling at the altar.
- **Rhen executes Myrrhen.** Rhen cuts her veil. Beneath it she is only a girl of nineteen. "…Is it
  over?" "Yes." He cuts the root-cord that binds her to the Heartwood. She smiles and comes apart,
  gently, into petals and spores that drift upward.

---

## 9. Master Oru — the Still Wind

| | |
|---|---|
| **ID** | `boss.oru` |
| **Title** | the Still Wind, Oathlord of Discipline |
| **Region / mission** | `region.cloudspire`, `mission.cloudspire.10`, recommended Lv 45 |
| **Arena** | **The Temple of Stillness** (`SC_Boss_Oru_TempleOfStillness`) |

**Arena.** A raked-gravel courtyard in sunlight, in the eye of the storm. The white Oathstone of
Discipline stands balanced on a single point, and walls of wind turn around the courtyard's edge.
**Hazards:**

- **Raked gravel.** Every dodge leaves a trail. Oru reads it, and so can the player.
- **Wind walls.** In Phase II the storm breaks in, and gusts push fighters.
- **Drifting bridge segments** beyond the courtyard open gaps in Phase II.
- **Lightning** in Phase III, telegraphed by a lit gravel circle.

**Lore.** Oru came to the Still Wind school at six and gave the Oathstone his first tantrum. At
twenty he gave it his grief for his mother. At thirty he gave it his love for a student. By fifty he
had nothing left to give, and became Master. For forty years he has not moved his feet in combat,
which is why he is called "the Unmoving". His own master, Sen Ajari, walked away sixty years ago,
saying that stillness had become a cage. Oru has never forgiven him. Discipline's line is *What I
hold still, I hold freely*. Oru holds his students still by tithing their feelings, and not one of
them is free.

**Signature weapon: *Unmoving Branch*** (Staff, Still Wind Style). A plain, perfectly straight
white-wood staff that he says was cut from the summit's lotus tree. In Phase I he fights only with
counters. After that, he fights with juggles, air combos and pole-spins.

**Intro cinematic**

1. The storm roars around a courtyard of perfect sunlight. Inside, it is silent.
2. Oru sits cross-legged before the balanced Oathstone, the gravel around him perfectly raked.
3. Rhen steps onto the gravel, and each step leaves a mark. Oru, eyes closed: "You walk loudly."
4. "Forty years I have not moved. You may try." He stands in one motion, and his feet don't leave
   their prints.
5. **MASTER ORU, THE STILL WIND.**

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. Unmoving** | 100–70% | His feet never move. **Still Answer**: a counter-stance that catches any strike and juggles the attacker. **Branch Reach**: a long poke. **Rooted Spin**: a 360° spin. **Returning Wind**: he parries projectiles back. **Grabs beat Still Answer**, and a cancelled attack baits it into a punishable recovery. | The eye of the storm, perfect calm. | **Patience, throws and feints.** Attacking without thinking gets you juggled. |
| **II. The First Step** | 70–35% | A cinematic beat: he takes his first step in forty years. Now he moves. **Rising Branch**: a launcher into **Sky Spin** (an air string). **Air Throw**. | The storm breaks in, gusts push, and bridge segments drift. | **Air recovery and anti-air.** Get out of his juggles, and meet his air approaches. |
| **III. Broken Stillness** | 35–0% | Every feeling he gave the stone comes back at once. **Tempest Strings**: fast pole-spin strings with irregular rhythm. **Fury Counter**: his counter-stance now attacks. **Lightning** strikes the courtyard. | A full storm, with telegraphed lightning. | **Perfect Parry** against irregular strings, and **Perfect Dodge** against lightning. |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 30 (P1 20, P2 45, P3 70) | 100 | 85 | 60 | 70 | 20 |

- **Learns to punish:** `habit.combo.repeat` (Still Answer on the first hit), `habit.jumpin` (Air
  Throw), `habit.wakeup.attack` (counter-stance on wakeup), `habit.parrymash` (feints).

**Soundtrack: "The Still Wind"**

- *Instrumentation:* end-blown bamboo flute, singing bowls, wind, low strings and a distant storm.
- *Tempo:* P1 50 BPM, almost silent, with the gravel as the loudest sound. P2 90 BPM (strings enter on
  the first step with a single bowl strike). P3 150 BPM (war drums and thunder sheets).
- *Leitmotif:* a single sustained flute note that bends, on Oath Row pitch IX. Sen Ajari's theme
  resolves it.

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Unmoving Branch*** (Staff) | Unlocks Still Wind Style |
| Armor | **Robes of the Windless Hour** | Resistance to wind and knockback |
| Rune | **Rune of Still Water** | Standing still for 1 s charges your next counter with +posture damage |
| Cosmetic | **Stillness Aura** | Rain stops falling around Rhen |

**Executions**

- **Oru executes Rhen.** Seven precise taps, and Rhen freezes mid-motion. Oru lifts his chin with the
  staff, then taps once more on the chest. Rhen's shadow slides off him like water off stone.
- **Rhen executes Oru.** Rhen sheathes his blade and stands still. Enraged, Oru strikes, and Rhen's
  draw-cut lands first. Oru steps back, his second step, and smiles for the first time in forty years.
  He kneels, and the storm stops.

---

## 10. Archivist Quill — the Thousand Pages

| | |
|---|---|
| **ID** | `boss.quill` |
| **Title** | the Thousand Pages, Oathlord of Memory |
| **Region / mission** | `region.sunkenarchive`, `mission.sunkenarchive.10`, recommended Lv 50 |
| **Arena** | **The Rotunda** (`SC_Boss_Quill_Rotunda`) |

**Arena.** The heart of the Archive, under the top of the air-dome, with the sea pressing on the
glass above. Bookshelves orbit the room, and the Oathstone of Memory, engraved with every tithed name
in microscopic script, is set in the floor. **Hazards:**

- **Orbiting shelves** are moving wall-bounce surfaces.
- **Dome cracks.** One opens at each phase change and sends down a water jet that knocks fighters
  back.
- **Reading lamps** set ink pools alight when knocked over.
- **Falling books** are thrown by his summons.

**Lore.** Quill came to the Archive at twelve as a copy-boy and has read every page in it. The
Oathstone of Memory takes the memories of its keeper in exchange for holding up the dome. By sixty,
Quill had forgotten his parents, his face and his name. **He knows everything and remembers
nothing.** He found the thirteenth clause at thirty and chose silence, believing that revealing it
would unmake the Oath and bring the Mourntide back. When his student Mireth found the Silence of
Births, he disgraced her to keep the secret. He wants someone to read it, and he cannot let anyone
take it. Memory's line is *What I remember, I return*. Quill returns nothing. On the Codex's last
page, in a hand he no longer recognizes, is his own name: **Edric Lowe**.

**Signature weapon: *Codex Umbrae*** (Arcane — Grimoire, Umbral Arts). A black book written in Umbra
ink whose pages cut, and which keeps writing on its own. Quill uses projectiles (page-blades), traps
(ink sigils) and summons (memory-echoes of fighters Rhen has already faced).

**Intro cinematic**

1. Pages drift down through the Rotunda like snow.
2. Quill writes furiously at a floating desk and doesn't look up. "Rhen. Oathwarden. Executed 500.
   Returned 500. Page 30,113 of the Ledger. Unfinished."
3. He looks up at last. "You read the clause. Of course you did. I left it open."
4. "I'm sorry. I cannot let it leave. I don't remember why. Only that I must not." He opens the Codex,
   and the pages lift.
5. **ARCHIVIST QUILL, THE THOUSAND PAGES.**

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. Citations** | 100–70% | **Page-Blades**: a fan of 5 projectiles that a Perfect Parry reflects. **Footnote**: an ink sigil at the player's feet that detonates after 1 s. **Cross-Reference**: a teleport to a shelf. **Marginalia**: a close-range book slam. | Shelves orbit slowly. | **Approach and Perfect Dodge** against volleys. **Parry-reflecting** pages. |
| **II. Index of the Defeated** | 70–35% | He summons **memory-echoes** of two Oathlords Rhen has defeated, chosen at random. Each uses one signature move from its own sheet and then dissolves, and its leitmotif plays on celesta as an audio tell. | Shelves topple, and dome crack 1 adds water jets. | **Adaptivity and spacing** against two fighting styles at once. |
| **III. Unwritten** | 35–0% | He writes Rhen's name in the Codex. **HUD erasure**: the health numbers, then the meters, then the minimap vanish, and each hit on Quill brings one back. **Redaction**: a black bar sweeps across the arena at head or ankle height, to be crouched or jumped. **Annotation**: he pre-parries the player's most-used string. | Dome crack 2 floods the lower ring ankle-deep. | **Fundamentals with less information**, and parrying projectiles. |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 35 | 85 | 95 | 85 | 25 | 50 |

- **Learns to punish:** `habit.combo.repeat` is his signature (**Annotation** records the player's top
  three strings and parries them pre-emptively). Also `habit.rollspam` (Footnote at the roll's exit)
  and `habit.jumpin` (an upward Page-Blade fan).
- **Tells:** he retreats to the shelves because his Courage is low. He talks throughout, quoting the
  player's fight history from the Habit Ledger: "Seven jump-ins. Predictable. Page eleven."

**Soundtrack: "Index of the Drowned"**

- *Instrumentation:* celesta, prepared piano, whispered voices reading names, string harmonics and
  water drops.
- *Tempo:* P1 70 BPM. P2 92 BPM (earlier Oathlords' leitmotifs quoted on celesta). P3 118 BPM in 5/4
  (a page-turning rhythm, and the piano dissolves).
- *Leitmotif:* a melody on Oath Row pitch X that repeats with one note changed each time, like a
  memory decaying.

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Codex Umbrae*** (Arcane — Grimoire) | Unlocks Umbral Arts (Grimoire form) |
| Armor | **Archivist's Ink-Stained Mantle** | Resistance to projectiles |
| Rune | **Rune of the Footnote** | A Perfect Parry leaves a delayed ink trap at the attacker's feet |
| Cosmetic | **Marginalia** | Glyphs that float around Rhen |
| Story | **Codex Lens** | Marks every unfound Oath Fragment on the map |

**Executions**

- **Quill executes Rhen.** He opens the Codex, and Rhen's name writes itself across the page. Quill
  strikes it through with his pen. Rhen fades into ink that runs down into the engraved names on the
  floor.
- **Rhen executes Quill.** Rhen tears out the Codex's last page and reads it aloud: "Edric Lowe."
  Quill hears his own name for the first time in thirty years. "…Oh. That was me." He weeps, and comes
  apart into pages that drift up to the dome.

---

## 11. General Hask Varrow — the Crimson Vow

| | |
|---|---|
| **ID** | `boss.hask` |
| **Title** | the Crimson Vow, Oathlord of Duty, Lord-Commander of the Oathwardens |
| **Region / mission** | `region.bloodmoon`, `mission.bloodmoon.10`, recommended Lv 55 |
| **Arena** | **The Vow Hall** (`SC_Boss_Hask_VowHall`, a boss variant of `SC_Arena_Bloodmoon_VowHall`) |

**Arena.** A long hall hung with the banners of every Oathwarden company, its roof broken open to the
red moon. The Oathstone of Duty sits on the altar, dark with five centuries of palm-blood. Vow-pillars
line the nave. **Hazards:**

- **Burning banners** fall across the floor on a cycle.
- **The moonbeam.** Standing in it builds Rage faster, for either fighter.
- **Vow-pillars** can be broken.
- **The hall collapses** from the far end during Phase III, shrinking the arena by 40%.

**Lore.** Hask was a Warden at sixteen, Lord-Commander at thirty, and General at thirty-five, in 468
AS, the year the moon turned red. He swore the **Crimson Vow** in this hall with his palm cut open:
*I will not fall while the Throne stands.* For three years he has held the walls against the Unsworn
without leaving them. He found Rhen at nine, holding his mother's censer-chain, and made him a
Warden. When the Edict came, he signed Rhen's death writ because duty demanded it, and he hasn't
slept since. Duty's line is *What I am ordered, I weigh*. Hask has never weighed anything, and in
this hall he finally does.

**Signature weapon: *Vowcleaver*** (Nodachi, Longvow Style). Forged by Tessen thirty years ago, with a
red-wrapped grip stained by Hask's vow-blood. Hask has enormous reach and fights with slow charged
attacks and armor. He **taught Rhen**, so he knows the Oathblade answers before Rhen uses them.

**Intro cinematic**

1. The Vow Hall under the red moon. The banners of dead companies burn slowly.
2. Hask kneels at the Oathstone with his palm cut and bleeding onto the stone. Vowcleaver is planted
   beside him.
3. He speaks without turning. "You were nine. You held the chain while they sang for your mother. I
   thought: that one will never refuse anything."
4. He stands and turns. "I was wrong. I've never been prouder, and I've never been more ashamed. Draw,
   boy."
5. **GENERAL HASK VARROW, THE CRIMSON VOW.**
   - *Plead* stance: Rhen says "Walk away with me." Hask: "…No." He skips his first attack.

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. Muster** | 100–70% | **Longvow Cleave**: a charged horizontal slash with armor. **Warden's Step**: a lunge thrust. **Crimson Arc**: an overhead. **Hilt Strike**: a fast close-range strike. **Counter-Draw**: he pre-empts Oathblade draw-cut counters. | Burning banners, and the moon rising. | **Perfect Parry against charged slashes.** Parrying a fully charged Cleave takes 40% of his posture. **Spacing** against his reach. |
| **II. The Bloodmoon** | 70–35% | The moon clears the roof and its beam empowers whoever stands in it. He plants the nodachi and speaks the Oathwarden oath, and **Warden-echoes** (two at a time, with katanas) take up the words. **Vow Throw**: a command grab. **Banner Fall**: he cuts down burning banners. | The moonbeam moves across the floor. | **Positioning against a crowd** and **throw teching**. |
| **III. Duty's End** | 35–0% | Wounded, he fights one-handed. His charged attacks have **unbreakable armor**. **Last Order**: a desperate 5-hit charged string. `clash` lines as he talks to Rhen. If `quest.tessen.04` is done: "Tessen always said this blade was too heavy for me." | The hall collapses from the far end, shrinking the arena by 40%. | **Perfect Dodge** against unbreakable charges, and **punishing** the recovery. |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 60 | 75 | 80 | 50 | 100 | 40 |

- **Learns to punish:** he starts **pre-seeded**. If the player's current weapon is a Katana, his
  counters to Oathblade draw-cut counters begin at full weight, because he taught them. Also
  `habit.rollspam`, `habit.parrymash` and `habit.wakeup.roll`.

**Soundtrack: "The Crimson Vow"**

- *Instrumentation:* military snare, brass chorale, a men's choir singing the **Oathwarden Hymn**, and
  low strings.
- *Tempo:* P1 100 BPM. P2 120 BPM (the full hymn over snares). P3 84 BPM, **slowing** into an elegy
  for solo trumpet and choir.
- *Leitmotif:* the Oathwarden Hymn on Oath Row pitch XI. It is the source of Rhen's own theme.

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Vowcleaver*** (Nodachi) | Unlocks Longvow Style |
| Armor | **Lord-Commander's Crimson Plate** | +1 hit of armor on charged attacks |
| Rune | **Rune of the Last Muster** | Below 25% health, charged attacks gain armor |
| Cosmetic | **Oathwarden's Tattered Banner** | A back cosmetic |

**Executions**

- **Hask executes Rhen.** He forces Rhen to kneel in exactly the pose from the Prologue's execution.
  "I'm sorry, boy." The cut. Cut to red moonlight on the floor.
- **Rhen executes Hask.** Hask drops to his knees. Rhen raises his blade, and Hask reaches up, puts
  his bloody hand over Rhen's on the grip, and guides it down himself. For a moment the red moon dims
  to rose.

---

## 12. Emperor Aurem — the Solemn Sun

| | |
|---|---|
| **ID** | `boss.aurem` |
| **Title** | the Solemn Sun, Oathlord of Sovereignty, First Emperor |
| **Region / mission** | `region.solemnthrone`, `mission.solemnthrone.10`, recommended Lv 60 |
| **Arena** | **The Sun Hall** (`SC_Boss_Aurem_SunHall`, a boss variant of `SC_Arena_SolemnThrone_SunHall`) |

**Arena.** The throne room. A golden sun-disc turns overhead, and the floor is glass laid over the
Undermourn Gate, with shadows churning below. Twelve pillars hold the Oathstone sockets. Each pillar
is **lit** if its stone was restored and **cracked** if it was sundered. The throne stands on a dais
over the Sovereignty keystone. **Hazards:**

- **Sun-disc beams** sweep the floor in Phase II.
- **The glass floor** cracks in Phase III, and shadow hands reach up through it, hurting both
  fighters.
- **The pillars** make the Oath Ledger physical: lit pillars power Aurem's Sworn Echoes, and cracked
  pillars release Rhen's Unsworn Aid.

**Lore.** Aurem was a sword-saint from the highlands who ended the Mourntide by swearing the
Twelvefold Oath alongside eleven champions. He also secretly wrote its thirteenth clause, and cut
away his own shadow to be the seal's keystone. He has not died since. A man without a shadow gives
death nothing to take. He watched his eleven companions die and be drawn into their own stones. He
has ruled for five hundred years and has not enjoyed a day of it. He believes, sincerely, that he
alone stands between Varanth and the return of the Mourntide, and he is partly right. In five
centuries he has never once looked down through the glass floor of his own throne room. Sovereignty's
line is *What I rule, I do not own*.

**Signature weapon: *First Oathblade*** (Katana, Oathblade Style). The original katana, the blade that
made the first cut. It is plain and sun-bright. Aurem fights in Oathblade Style **perfected**. Every
technique Rhen has learned, Aurem answers with its original form.

**Intro cinematic**

1. Silence in the Sun Hall. The sun-disc turns, and under the glass floor the shadows move like a sea.
2. Aurem sits on the throne with the blade across his knees, looking tired. "Five hundred years, and
   the first man to reach this hall is one I had killed."
3. He makes an offer. "Sit. Take it. I will kneel. I am so tired." (`challenge` node. The offer can
   only be refused.)
4. Rhen refuses. Aurem stands, and his steps cast no shadow. One by one the twelve pillars light or
   crack, showing the Oath Ledger.
5. He draws so fast that the sound arrives after the blade. **EMPEROR AUREM, THE SOLEMN SUN.**

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. Noon** | 100–70% | **Draw-Cut Counter**. **Sun Step**: an instant dash. **Twelvefold**: a 12-hit string. **Solemn Parry**: he pre-parries the player's top string from the Habit Ledger. **Crowning Cut**: an overhead. **His posture only takes damage from Perfect Parries and from punishes during Shadow Time.** | Eternal noon. | **Mastery of Perfect Parry.** Trading hits does not work. |
| **II. Sworn Echoes** | 70–35% | The sun-disc sweeps beams across the floor. **For every restored Oathstone, Aurem gains one Sworn Echo**: that stone's Oathlord's signature move, performed in gold. They are Kessh's Litany, Ilvane's Undertow Spin, Gorran's Tally Stamp, the Petals' Kinship String, Maal's Noon Judgment, Tharuk's Wolf's Grip, Veloran's Lash Pull, Myrrhen's Reaping Hook, Oru's Still Answer, Quill's Page-Blades and Hask's Longvow Cleave. If no stones were restored, he fights in pure Oathblade 15% faster instead. | Sweeping beams, and lit pillars glowing brighter. | **Adaptivity.** Recognize each echo, which quotes its leitmotif as an audio tell. |
| **III. The Sun Sets** | 35–0% | The glass cracks, the sun-disc goes dark, and the Gate's shadows reach up. Aurem, who has no shadow, is **afraid of the dark**. **Last Light**: blinding flashes. **Sovereign Grab**: a grab, techable. **Throne Fall**: an anti-air slash from the dais. **For every sundered Oathstone, Rhen gains one Unsworn Aid**: a freed shadow from that region that strikes Aurem once, triggered by the player's next Perfect Parry. | Cracked glass, reaching hands, darkness spreading from the edges. | **Everything**: Perfect Dodge, Perfect Parry, anti-air and throw teching. |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 55 | 90 | 100 | 80 | 60 (40 in P3) | 65 |

- **Learns to punish:** every habit in §0.3. **His Habit Ledger carries over between retries within a session**, with 50% mercy decay per retry. Solemn Parry is his signature learned answer.
- **Tells:** in Phase III his Courage drops, and he backs away from the spreading dark. Players can
  crowd him toward it.

**Soundtrack: "The Solemn Sun"**

- *Instrumentation:* full orchestra, mixed choir and pipe organ. **The Oath Row is stated whole for the
  first time.**
- *Tempo:* P1 80 BPM, a majestic brass chorale. P2 108 BPM, with each Sworn Echo quoting its Oathlord's
  leitmotif inside the texture. P3 140 BPM, falling to 60 BPM for the last stand, strings alone as
  the dark rises.
- *Leitmotif:* the complete twelve-note Oath Row.

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***First Oathblade*** (Katana) | A legendary Oathblade Style weapon |
| Armor | **Regalia of the Solemn Sun** | +5% Ultimate meter gain |
| Rune | **Rune of the Solemn Sun** | Each consecutive Perfect Parry adds 5% Ultimate meter, stacking up to 5 times |
| Cosmetic | **Sunless Crown** | A crown that casts no shadow |
| Story | `fragment.sovereignty` | Engraved on the blade's tang |

**Executions**

- **Aurem executes Rhen.** A draw-cut too fast to see, and Aurem sheathes before Rhen has fallen. The
  sun-disc flares, and Rhen's shadow is drawn up into the light like smoke.
- **Rhen executes Aurem.** Rhen forces Aurem to his knees on the cracked glass. Sable rises behind the
  Emperor and holds him still. Rhen cuts. In the dying light, for the first time in five hundred
  years, Aurem casts a shadow: thin and faint, reaching down through the glass toward the Gate, where
  something vast reaches back. "…There you are." He dies.

---

## 13. Tamsin — of the Nine Knives

| | |
|---|---|
| **ID** | `boss.tamsin` |
| **Title** | of the Nine Knives |
| **Region / mission** | Recurring rival in regions 2–7. Final duel: `region.lanternhold`, `mission.lanternhold.08`, recommended Lv 33 |
| **Arena** | **The Tiles at the Height of the Fireworks** (`SC_Boss_Tamsin_Rooftops`, a boss variant of `SC_Arena_Lanternhold_Rooftops`) |

**Arena.** Three stepped rooftops across the Lantern District, and the fight moves from one to the
next: Roof A in Phase I, the lower Roof B in Phase II, and the bell-tower roof in Phase III. There
are strings of paper lanterns, chimneys, fireworks and a warm drizzle. **Hazards:**

- **Breakable tiles.** A ground bounce breaks through to the next roof, which drives the phase
  transitions.
- **Lantern strings** can be cut to fall and burn.
- **Chimneys** block thrown knives.
- **Fireworks** light the whole scene for 1 s on a fixed schedule, showing where she has teleported.

**Lore.** Tamsin grew up with Neve in the Wick House, a canal orphanage. At eleven, Lady Corvaine
picked her from nine orphans to join the Nine Knives. The other eight died in training or on
contracts, and Tamsin carries their knives, each one named. To pay off the Wick House's debts she
sold half her shadow on Hollis Crane's shadow market. She is fading now, and her outline flickers.
She took the contract on Rhen and Liss from Duke Veloran, under the Chancellor's seal, because it
paid enough to buy her shadow back. She has never failed a contract. She has also never met anyone
who kept getting up.

**Signature weapon: *Ninefold Fang*** (Daggers, Ninefold Style). Nine daggers: two in hand, and the
rest orbiting on a bandolier of shadow. Because half her shadow is gone, she can step through
darkness. She fights with teleport feints, bleed strings and throwing knives.

**The recurring encounters**

| # | Mission | Type | Withdraws at | Moveset | What changes |
|---|---|---|---|---|---|
| I | `mission.weepingreeds.06` | Duel | 50% | Phase I kit only | She pins Sable with Knife I. The first Habit Ledger snapshot is taken. |
| II | `mission.ironroot.07` | Chase | 40% | Phase I kit plus Knife Fan on the move | Knife II. Snapshot. |
| III | `mission.silkwind.06` | Stealth-Duel | 35% | Adds Smoke Step | Knife III. Her carved question. Snapshot. |
| IV | `mission.glassossuary.07` | Trial (ally, then duel) | 30% | Phases I–II, partial | Knife IV. If Rhen keeps her above 50% HP while they fight together, the final duel starts with her Showmanship +10 and Aggression −5 ("respect"). |
| V | `mission.rimewood.06` | Duel | 25% | Phases I–II | Knife V. She names her employer. Snapshot. |
| VI | `mission.lanternhold.08` | Boss | — | Full kit | **Rival Memory** (see AI). Ends in a `mercy` node. |

**Intro cinematic** (final duel)

1. Festival fireworks over Lanternhold. Rhen climbs onto the Tiles.
2. Tamsin sits on the roof ridge counting her knives aloud: "…seven, eight, nine."
3. "Last contract. Paid in advance." She looks at her hand, which flickers. "Not much of me left to
   spend."
4. "Tell the girl I'm sorry. Or don't." She stands, and her knives lift into orbit.
5. A firework bursts. She's gone, and then she's behind him. **TAMSIN, OF THE NINE KNIVES.**
   - *Plead* stance: Rhen says "Neve's waiting for you." She hesitates for one beat, and her first
     attack is skipped.

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. Counting** | 100–65% | **Blink Feint**: a teleport that leaves two afterimages, and only the real one's knives glint. **Bleed String**: a dagger string that stacks bleed. **Knife Fan**: three thrown knives. **Low Slash**. | Roof A. | **Reading teleport feints.** Guard or parry the real attack. A Perfect Dodge through an afterimage strike gives Shadow Time. |
| **II. The Smoke** | 65–30% | **Pinning Throw**: her knives pin lantern strings and start fires. **Smoke Step**: smoke bombs and teleports. **Drop**: she falls on the player from above. **Knife Rain**. | The roof gives way, and the fight moves to Roof B among fires and smoke. | **Anti-air** (Drop) and **spacing** (Knife Rain). |
| **III. The Ninth Knife** | 30–0% | All nine knives orbit her. **Ninefold Volley**: nine knives in sequence, each of which can be perfect-parried to reflect it for posture damage. **Last Contract**: a command grab, techable. **Flicker**: her body blinks in and out, and 20% of her attacks come from the flicker. At 0 HP the **`mercy` node** plays. | The bell-tower roof, with fireworks at their height. | **Perfect Parry of the volley** and **throw teching**. |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 80 | 45 | 90 | 95 | 70 | 60 |

- **Rival Memory.** The Habit Ledger snapshots from encounters I–V carry into the final duel. She
  starts with her answers to the player's **two most frequent habits** already at full weight, and
  says so ("Still rolling left, Oathwarden?").
- **Learns to punish:** `habit.rollspam`, `habit.parrymash`, `habit.jumpin`, `habit.wakeup.roll`.

**Soundtrack: "Nine Knives"**

- *Instrumentation:* solo cello, hand percussion and box drum, plucked guitar, and the fireworks used
  as percussion.
- *Tempo:* P1 132 BPM. P2 150 BPM. P3 168 BPM. After the `mercy` node, the cue drops to solo cello at
  60 BPM.
- *Leitmotif:* a **nine-note run** that stops one note short of the octave (there is no tenth).
  Corvaine's lament is this figure inverted.

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Ninefold Fang*** (Daggers) | A legendary Ninefold Style weapon (the Daggers class itself unlocks at encounter I) |
| Armor | **Nightrunner's Leathers** | Faster teleport and dodge recovery |
| Rune | **Rune of the Ninth Knife** | Every 9th hit throws a free knife |
| Cosmetic | **Knife-Orbit** | An idle with nine orbiting knives |
| Collectible | `knife.ix` | Given to Rhen if she is spared, taken from her if not |

**Executions**

- **Tamsin executes Rhen.** Nine knives drop in a ring around Rhen. She vanishes and reappears behind
  him with a knife at his throat. "Nothing personal. It never is." Cut to a firework.
- **Rhen executes Tamsin** (kill at the `mercy` node). Rhen catches her ninth knife in mid-throw and
  presses it back into her hand, closing her fingers around it. She smiles, says "Keep count for
  me", and falls backward off the roof into the light of the fireworks.
- **Mercy** (spare). Rhen sheathes his blade. Tamsin sits down on the tiles, laughs, and hands him the
  ninth knife. "Mourning House, Lily Canal. She's waiting for someone." (`flag.tamsin.spared`.)

---

## 14. Sable Ascendant — the Other You

| | |
|---|---|
| **ID** | `boss.sableascendant` |
| **Title** | the Other You |
| **Region / mission** | The Undermourn Gate, `finale.gate` (every ending route), recommended Lv 60 |
| **Arena** | **The Mirror at the Gate** (`SC_Boss_SableAscendant_Mirror`) |

**Arena.** A perfect black-water mirror at the threshold of the Undermourn Gate, with the waking world
reflected upside down beneath it. As the fight goes on, the arena **cycles through memories** of
places Rhen has fought, taken from the three regions where this player died most often (from
telemetry). **Hazards:**

- **Ripples.** Every hit makes one, and a fighter standing in a ripple has 4 extra frames of dodge
  startup.
- **Memory set-pieces.** Each carries one hazard from its original arena: the Belltower bell, the
  Crucible pour, the Twin Pavilion reflection, and so on.
- **The inverted layer.** Sable can step into the reflection to reposition, and so can the player
  while in Umbral Shadow.

**Lore.** Sable has always been the part of Rhen that said the quiet thing out loud. At the Gate,
every route gives him a reason to fight:

- **Dawn route.** A throne demands a ruler with no shadow, so Sable knows he'll be cut away.
- **Night route.** With every oath broken, he sees no reason why he shouldn't be the one who walks in
  the light.
- **Balanced route.** Rewoven means rejoined, and Sable is afraid of disappearing back into Rhen.

He steps out of Rhen and won't step back in until they've settled who wears whom. `flag.sable.terms`
sets the tone of the whole fight.

**Signature weapon: *Umbral Oathblade*** (Katana, shifts to any mastered class). A katana of pure Umbra
that mirrors Rhen's equipped blade. In Phase II it becomes the player's **three most-used mastered
weapon classes** in turn, using their full movesets.

**Intro cinematic**

1. At the Gate, Rhen steps forward and his shadow stays behind.
2. Sable peels himself up off the black water, standing and wearing Rhen's face. For the first time,
   he speaks in Rhen's own voice.
3. His line depends on `flag.sable.terms`:
   - *promised*: "You promised. I'm just making sure."
   - *refused*: "You said no. So I'm asking differently."
   - *unanswered*: "You never answered. Let's settle it."
4. He draws the Umbral Oathblade, a perfect mirror of Rhen's.
5. **SABLE ASCENDANT, THE OTHER YOU.**

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. Mirror** | 100–65% | Sable uses the player's katana moveset and **the player's own most-used combos**, drawn from the whole campaign's Habit Ledger. He mirrors the player's stance. | Pure black mirror. | **Fundamentals.** To win, the player has to break their own habits. |
| **II. Every Weapon** | 65–30% | He shifts through the player's **top three mastered classes**, 20 s each (Katana variants fill in if fewer than three are mastered). Each shift is announced by that class's motif. | The arena cycles through memory set-pieces, each carrying its own hazard. | **Adaptivity.** Know every matchup. |
| **III. The Offer** | 30–0% | At 30% HP Sable stops, lowers his blade and holds out his hand: **The Offer**, a 12 s window. **Taking his hand (walk to him and press `Grab`) is the yield**, and leads to `ending.sableascendant`. Attacking, or letting the window close, triggers his final fury. **Umbral Storm**: every one of his hits spawns a delayed echo strike, mirroring canon Umbral Shadow. **Shadow Swap**: he trades places with the player. **Twin Draw**: two draw-cuts from opposite sides. | During the Offer, silence and a heartbeat. Afterward, the mirror cracks. | **Perfect Dodge and Shadow Time mastery** against echo strikes. |

**AI personality** (mirrored, computed per player)

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| Player's value ±10 | Player's value ±10 | 90 | Player's value ±10 | 80 | Player's value ±10 |

- Traits marked "player's value" come from the player's measured campaign profile, clamped to 20–95.
- **Sable approval modifiers.** At Sworn (≥ +60): Aggression −10 and Showmanship +15, because he plays
  with Rhen. At Severed (≤ −60), the **hostile variant**: Aggression +20 and Patience −20, and the Offer
  is a sneer, though it can still be taken.
- **Learns to punish:** the player's **top three habits** across the whole campaign, active from the
  first frame.

**Soundtrack: "The Other You"**

- *Instrumentation:* Rhen's theme in retrograde and inversion, on piano and reversed-reverb strings,
  over heartbeat percussion.
- *Tempo:* P1 96 BPM. P2 follows each weapon class's motif tempo. P3: the Offer is silence and a
  heartbeat at 60 BPM. If the Offer is refused, full orchestra at 180 BPM.
- *Leitmotif:* Rhen's motif played backward.

**Legendary rewards** (awarded only if Sable is defeated)

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Umbral Oathblade*** (Katana, shifts to any mastered class) | Takes the form of whichever mastered class is equipped |
| Armor | **Mirrorblack Raiment** | +10% Shadow meter gain |
| Rune | **Rune of the Reflection** | After a Perfect Dodge, your next hit spawns a shadow echo even outside Umbral Shadow |
| Cosmetic | **Twin Silhouette** | Rhen's shadow moves on its own in idle |

**Executions**

- **Sable executes Rhen.** Sable steps into Rhen the way you'd step into a coat. Rhen's body
  straightens and wears Sable's grin, and Rhen is left on the floor as the shadow. (If this is a
  normal defeat it is followed by a retry. Only the yield leads to the ending.)
- **Rhen executes Sable.** Rhen drops his blade and embraces Sable instead of cutting him. The shadow
  sinks back into Rhen's feet. Sable whispers: "…Fine. But I'm still doing the jokes."

---

## 15. The Ferrous Maw — Engine of Ironroot (RAID)

| | |
|---|---|
| **ID** | `boss.ferrousmaw` |
| **Title** | Engine of Ironroot |
| **Region / mission** | `region.ironroot`, `raid.ferrousmaw` (discovered in `mission.ironroot.10`, opens at Lv 35), **3–4 players** |
| **Arena** | **The Engine Cradle** (`SC_Raid_FerrousMaw_Cradle`) |

**Arena.** A vast pit beneath the Great Crucible, with two planes: the narrow upper **Front Catwalk** and the wide **Furnace Floor** at the rear. The Maw stands 20 m tall, its torso filling the
background, and its fists come into both planes. **Hazards:**

- **Pistons** crush on a cycle.
- **Four coolant valves.** Holding `Grab` on one for 2 s vents it, lowering the Maw's Heat.
- **Slag lanes** flood.
- **Falling rivets** mark impact circles before they land.

**Lore.** The Maw was built in 377 AS under the Iron Compact to drive Ironroot's deep forges. It runs
on a thousand clamped shadows, workers who died on shift and were fused into its boiler, and Gorran
built it larger. When Gorran falls, the Maw wakes. It does not want to fight. A thousand exhausted
shadows want the shift to end, and work is the only thing they remember how to do. If Labor was
**sundered**, it wakes angry, and its intro shows it tearing its own clamps. If Labor was
**restored**, it wakes confused, and its intro shows it trying to clock in. The difficulty is the
same either way.

**Signature weapon: *Crucible Fists*** (Gauntlets, Iron Fang Style). Two crucible-shaped fists that
pour molten metal. It is Iron Fang rushdown at the scale of a building: slams, grabs, and pours that cover whole planes.

**Intro cinematic**

1. The floor of the Great Crucible cracks, and molten light shows from below.
2. The raid team climbs down into the Cradle on chains while a thousand voices chant the shift song.
3. The Maw's eyes light, and its fists tear free of the rock.
4. It pours molten metal into its own chest furnace, and the chant rises.
5. **THE FERROUS MAW, ENGINE OF IRONROOT.**

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. Shift Start** | 100–70% | **Fist Slam** hits the plane of the player with the most threat and sends out a shockwave. **Crucible Grab** seizes one player, who can tech on the flash or be freed by allies dealing 5% posture damage to the fist within 4 s. **Pour** sends a molten line across one plane. **Piston Rhythm**. | Both planes are active. | **Spacing and Perfect Dodge** against slam shockwaves, and **co-op throw teching**. |
| **II. Overheat** | 70–35% | A **Heat gauge** rises, and at 100% it vents a blast across one plane. Players reduce it at the **coolant valves**, one venting while the others guard. The chest opens and exposes the **Shadow Core**. **Chain Overload** (stagger sharing): three heavy hits on the Core from **different players within 2 s** break 25% of its posture. | Slag floods the Furnace Floor lanes. | **Threat management and coordination.** |
| **III. Meltdown** | 35–0% | The Maw tears free of the Cradle and the arena **collapses into a single plane**. **Leap Slam** (anti-air read). **Thousand Hands**: workers' shadows reach out, and each one a player **executes** is freed and gives the team +10% damage for 5 s. **Final Core**. | One plane, with the walls glowing. | **Coordinated executions** and **anti-air**. |

**Raid mechanics**

| Mechanic | Rule |
|---|---|
| **Aggro: Heat threat** | Each player's threat = damage dealt, ×2 during Ember Rage, plus a spike for every Perfect Parry against a fist. The Maw targets the highest-threat player. Venting a valve dumps 50% of that player's threat. |
| **Stagger sharing** | A shared posture bar. Chain Overload is the burst window. At 0 posture there is a **Chain Execution** on the Core, and every player in range joins in. |
| **Revives** | A downed player is carried by conveyor toward the furnace over 20 s. An ally revives them by holding `Execute` for 3 s beside them, and **the reviver loses 25% of their current Ember meter**. **3 Oath-light Revives** per phase. A player who reaches the furnace is out until the phase ends. |
| **Soft roles** | The **Anchor** holds threat by parrying fists, the **Breaker** damages the Core, and the **Valve-runner** keeps the Heat down. |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 85 | 20 | 40 | 20 | 100 | 90 |

- **Learns to punish:** `habit.stack` (a Pour across the crowded plane), `habit.turtle` (Crucible Grab
  on guarding players), `habit.backdash`.

**Soundtrack: "Engine of Ironroot"**

- *Instrumentation:* industrial percussion built from sampled pistons, a massed chorus of workers
  singing Gorran's work-chant, and brass.
- *Tempo:* P1 120 BPM. P2 140 BPM, with the Heat gauge driving a rising drone. P3 160 BPM.
- *Leitmotif:* Gorran's long-short-short anvil rhythm doubled under a thousand-voice chant.

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Crucible Fists*** (Gauntlets) | A legendary Iron Fang Style weapon |
| Armor | **Enginewright's Harness** | Resistance to heat and burning |
| Rune | **Rune of the Thousand Hands** | Every 10th successful grab frees a worker's shadow that strikes once |
| Cosmetic | **Furnace Heart** | A glow in the chest |

**Executions**

- **The Maw executes a player.** It closes a crucible fist around the player (shown as a silhouette
  in the glow), lifts them to its chest furnace and drops them in. One new voice joins the chant.
- **The raid executes the Maw** (Chain Execution). Each player drives a piston lever home. The player
  who triggered the Execute climbs the fist and strikes the Shadow Core. A thousand shadows burst out
  in a column of light, and the Maw kneels, cools and goes silent. The chant ends on a held chord.

---

## 16. Lady Corvaine — the Ninth Widow

| | |
|---|---|
| **ID** | `boss.corvaine` |
| **Title** | the Ninth Widow |
| **Region / mission** | `region.lanternhold`, hidden duel `side.lanternhold.05`, recommended Lv 37 |
| **Arena** | **The Mourning House** (`SC_Boss_Corvaine_MourningHouse`) |

**Arena.** A narrow three-storey widow's house on the Lily Canal. There are black lanterns, paper
walls and mirrors draped in black, and nine portrait frames, the ninth of them empty. Rain drums on
the roof. **Hazards:**

- **Paper walls.** Her fans slice through them and open new paths.
- **Black lanterns.** In Phase III they go out one at a time.
- **Portraits** supply her Phase II summons.
- **Draped mirrors.** Pulling a drape shows her true position in the dark.

**Lore.** Corvaine married eight noblemen, each richer than the last, and each died of "grief" within
a year. The Lantern Guild paid her for every one. She was its finest assassin, and she trained nine
orphans as the Nine Knives. She loved her ninth husband, Lord Castor Sae. Veloran poisoned him and
had her executed for it in 488 AS. Her shadow came back as a Remnant and has never left the Mourning
House, waiting to see which of her Knives would come for her, or come for Veloran. She knows Tamsin
is the last of them.

**Signature weapon: *Mourning Fans*** (Arcane — Warfan, Umbral Arts). A pair of black funeral-paper
fans edged with blade-steel. She fights with projectiles (blade-paper arcs and black ribbons), traps
(folded paper cranes that explode) and summons (the widow-shadows of her portraits).

**Intro cinematic**

1. Rain. The black lanterns sway, and the canal door opens on its own.
2. Corvaine sits before the empty ninth frame, fanning herself.
3. She sees the ninth knife on Rhen's belt and goes still. "Which one of them sent you, darling? Or
   did one of them *not* send you?"
4. Her next line depends on the route:
   - *Tamsin spared*: "So she lived. Good girl."
   - *Tamsin killed*: "Ah. So she's gone. Then you'll do."
5. Her fans snap open like black paper flowers. **LADY CORVAINE, THE NINTH WIDOW.**

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. Eight Husbands** | 100–70% | **Fan Blades**: paired projectile arcs. **Crane Trap**: a paper-crane mine. **Veil Spin**: a close-range spin. **Black Ribbon**: a pull, techable. | Ground floor, lanterns lit. | **Spacing and parrying projectiles.** |
| **II. The Portraits** | 70–35% | She calls the shadows of her eight husbands out of their portraits as puppets, two at a time, fighting with katana, spear or hammer. She floats on the stairs above them. | The paper walls are cut open and the stairwell becomes part of the arena. | **Target priority and anti-air** against her attacks from above. |
| **III. The Ninth Mourning** | 35–0% | She weeps for Castor. **One black lantern goes out for every 10% of her HP.** In the dark, only the glint of her fans' steel edges and her sound cues give her away. **Grief Storm**: a whirlwind driven by her fans. **Mourning Embrace**: a grab. | Darkness spreads through the house. | **Perfect Dodge by sound and glint**, and **Perfect Parry**. |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 45 | 80 | 70 | 95 | 50 | 90 |

- **Learns to punish:** `habit.jumpin` (an air pull with Black Ribbon), `habit.dash.approach`,
  `habit.rollspam`.

**Soundtrack: "Nine Mournings"**

- *Instrumentation:* harpsichord, funeral bells, solo contralto, clarinet and rain.
- *Tempo:* P1 96 BPM in 3/4. P2 116 BPM. P3 138 BPM, with a bell tolling for each lantern that goes out.
- *Leitmotif:* a nine-note descending minor lament, Tamsin's figure turned upside down.

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Mourning Fans*** (Arcane — Warfan) | Unlocks Umbral Arts (Warfan form) |
| Armor | **Widow's Weeds of Lanternhold** | Traps last 20% longer |
| Rune | **Rune of the Ninth Mourning** | After taking a hit, a Perfect Parry within 2 s returns 50% of that damage as health |
| Cosmetic | **Black Lantern** | A hand-held prop |

**Executions**

- **Corvaine executes Rhen.** She folds her fans around his neck like a collar and kisses the air
  beside his cheek. A black veil drops over him, and his shadow joins her wall as a tenth portrait.
- **Rhen executes Corvaine.** Rhen offers her the ninth knife, hilt first. She takes it, closes her
  fans and sets the knife in the empty ninth frame. Rhen's last stroke cuts the black veil that has
  held her to the house. The lanterns relight one by one, and she fades into the portrait beside
  Castor's.

---

## 17. Ossric — the Grave-Warden

| | |
|---|---|
| **ID** | `boss.ossric` |
| **Title** | the Grave-Warden |
| **Region / mission** | `region.glassossuary`, hidden duel `side.glassossuary.05`, recommended Lv 27 |
| **Arena** | **The Tomb of the Giant's Heart** (`SC_Boss_Ossric_Tomb`) |

**Arena.** A chamber inside a Sleeper's ribcage, beneath the Noon Cathedral. Glass coffins are set
into the bone walls, sand pours through cracks, and a vertebra swings on a chain as a pendulum.
**Hazards:**

- **Sand-falls** push fighters and block sight.
- **The bone pendulum** crosses the middle of the arena every 8 s. In Phase III it can be cut down.
- **Glass coffins** break in Phase III and release old shadows.
- **The sand floor** floods ankle-deep in Phase II.

**Lore.** Before the Oath, the dead walked **the Old Road** down to the water, and the Grave-Wardens
guided them with lanterns, singing the Rebirth Song. Ossric was the last of them. When the Oath
sealed the Gate, the Old Road closed, and shadows went into stones instead of the water. With no one
left to guide, Ossric lay down in the heart of the greatest Sleeper and waited for someone to hum the
song and open the road again. He has waited five hundred years. Before he lets anyone walk the road,
he will find out whether they are worthy, by trying to guide them down it himself.

**Signature weapon: *Last Harvest*** (Scythe, Harvest Style). An old grave-warden's scythe with a road
lantern hanging from its handle. It was made to *gather* shadows, not to kill. Ossric fights with
pull-in hooks and cross-ups, and his drains take the Shadow meter.

**Intro cinematic**

1. The tomb door is carved with the Rebirth Song. Rhen hums it, as Liss taught him, and the door
   opens.
2. Inside, sand pours. A figure lies in a glass coffin with a lantern on its chest.
3. The lantern lights, and Ossric sits up. "The song. Someone remembered the song."
4. He rises, tall and thin in a bone-white shroud. "Then walk the road, child. I'll lead."
5. **OSSRIC, THE GRAVE-WARDEN.**

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. The Old Road** | 100–70% | **Gathering Hook**: a pull. **Lantern Swing**: a burning lantern strike. **Cross-Reap**: a cross-up. **Rite of the Road**: drains 30% of the Shadow meter. | Sand-falls and the pendulum. | **Blocking cross-ups and spacing.** |
| **II. Sand** | 70–35% | He **heals by draining the Shadow meter**. He disappears into sand-falls and steps out of others. | The sand floods ankle-deep and the sand-falls get heavier. | **Perfect Dodge**, and **denying his drain**: spend the Shadow meter before he can take it, or bait the drain while the meter is empty. |
| **III. Last Harvest** | 35–0% | The coffins break, and pre-Oath shadows rise. They are gentle, and fight only by holding on. He sings the Rebirth Song. **Ascending Reap**: a launcher. **Harvest Moon**: a leaping overhead. | The pendulum can be cut down to fall across the floor. | **Anti-air** (Harvest Moon) and **air combos** off the launcher. |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 50 | 90 | 45 | 55 | 85 | 30 |

- **Learns to punish:** `habit.shadow.raw` (drains before the player can activate), `habit.jumpin`,
  `habit.turtle`.

**Soundtrack: "The Grave-Warden"**

- *Instrumentation:* sand-rattles, bone flutes, a low male chant and bowed metal.
- *Tempo:* P1 64 BPM. P2 80 BPM. P3 100 BPM, when the **Rebirth Song** emerges in full on a solo voice.
- *Leitmotif:* the Rebirth Song, the pre-Oath lullaby Liss hums.

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Last Harvest*** (Scythe) | A legendary Harvest Style weapon |
| Armor | **Grave-Warden's Bone Shroud** | Resistance to meter drain |
| Rune | **Rune of the Lantern Road** | Executions restore 20% of the Shadow meter |
| Cosmetic | **Warden's Road-Lantern** | A lantern worn at the hip |

**Executions**

- **Ossric executes Rhen.** He hooks Rhen, lays him in a glass coffin and gently closes the lid. Sand
  pours over the glass until only the lantern light is left.
- **Rhen executes Ossric.** Rhen's cut severs the lantern from the scythe. Ossric catches it and nods:
  "The road is yours now." He walks into the sand-fall, which opens for a moment into a road of light,
  and then closes behind him.

---

## 18. The Drowned Choir — Three Voices, One Throat (RAID)

| | |
|---|---|
| **ID** | `boss.drownedchoir` |
| **Title** | Three Voices, One Throat |
| **Region / mission** | `region.weepingreeds`, `raid.drownedchoir` (discovered in `mission.weepingreeds.09`, opens at Lv 25), **3–4 players** |
| **Arena** | **The Drowned Chapel** (`SC_Raid_DrownedChoir_Chapel`) |

**Arena.** A chapel under the Bell Causeway, with two planes: the shallow-water **Nave** in front and the flooded **Choir Loft** behind it, linked by floating pews. The echo of the Tidebell rings through
the building, and the organ pipes still stand. **Hazards:**

- **Tide Hymns.** The water rises and falls with their song. At high tide, only the air pockets hold
  air, and every player has a **breath meter** of 10 s underwater.
- **Floating pews** are moving platforms between the planes.
- **Organ pipes** fire a sound blast when struck.
- **The anchor chain** drags across the floor as a hazard.

**Lore.** Hesper (15), Lark (13) and Orla (11) sang in the chapel choir of Hollowmere. On the night of
the Mourning Flood in 471 AS, the water rose during the evening hymn. They held hands and kept
singing. As they drowned, their shadows fused into one, *three voices, one throat*, and stayed under
the chapel, where their mother's bell calls them every dusk. Every child Ilvane drowned, she sent to
them. They don't want more sisters. They want to stop singing.

**Signature weapon: *Anchor Hymn*** (Chain Sword, Censer Chain Style). The chapel was once a moored
barge-church, and this is its anchor chain, ending in an anchor-blade. It is segmented and extends.
The three sisters share it, and in Phase II they split it between them.

**Intro cinematic**

1. The raid team dives through the causeway and surfaces in the nave's air pocket.
2. Singing. Pews float in the dark water. In the choir loft stand three girls' silhouettes holding
   hands, their heads bowed together into one shape.
3. Three voices speak as one: "Mother?"
4. They see strangers, and the hymn turns sharp.
5. The anchor chain rises out of the water. **THE DROWNED CHOIR, THREE VOICES, ONE THROAT.**

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. Evening Hymn** | 100–70% | **Anchor Sweep**: a sweep across a whole plane, blocked with crouch guard. **Drag**: the anchor hooks a player and pulls them into the loft. **Harmony**: each voice marks one player. **Tide Hymn**: the water rises, and players have 10 s to reach an air pocket. | Both planes, with the tide cycling. | **Crouch guard, spacing** and **positioning on air pockets**. |
| **II. Three Voices** | 70–35% | The Choir splits into three bodies. **Hesper** is high and ranged, with anti-air notes. **Lark** is mid-range with a chain whip. **Orla** is low and grabs (techable). **Dissonance**: all three must be staggered **within 5 s of each other**, or they re-harmonize and their posture resets. | Pews scatter, and the organ pipes become active. | **Stagger sharing** and **throw teching**. |
| **III. One Throat** | 35–0% | They merge again. The chapel floods except for **three air pockets**. **Final Hymn**: continuous damage to anyone outside a pocket, so players rotate between pockets while dealing damage. **At 10%, Ilvane's shadow appears**, as a Remnant if Mercy was sundered or as an echo if it was restored, and sings the lullaby. The Choir's attacks slow by 50%. | Full flood. | **Positioning and Perfect Dodge.** |

**Raid mechanics**

| Mechanic | Rule |
|---|---|
| **Aggro: Harmony marks** | Each voice fixes on one player, the highest-damage player at the moment it marks. In a 4-player raid, the unmarked fourth player becomes **the Rest** and is the target of every Anchor Sweep. Marks rotate every 30 s. |
| **Stagger sharing** | A shared **Chorus posture bar** in Phases I and III. Three separate posture bars in Phase II, linked by the 5 s Dissonance window. Breaking posture opens a Chain Execution. |
| **Revives** | A downed player sinks. An ally revives them by holding `Execute` over them for 3 s within 15 s, and **the reviver loses 30% of their breath**. **3 Oath-light Revives** per phase. A player who runs out of breath goes down. |
| **Breath** | 10 s underwater. Air pockets refill it immediately. |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 60 | 70 | 55 | 60 | 40 | 85 |

- **Learns to punish:** `habit.stack` (times a Tide Hymn to the crowded air pocket), `habit.turtle`
  (Drag), `habit.jumpin` (Hesper's anti-air notes).

**Soundtrack: "Anchor Hymn"**

- *Instrumentation:* a three-part a cappella girls' choir, pipe organ filtered as if underwater,
  handbells and hydrophone textures.
- *Tempo:* P1 66 BPM. P2 88 BPM, as the three voices separate into counterpoint. P3 108 BPM, until
  Ilvane's lullaby at 60 BPM takes over the final 10%.
- *Leitmotif:* Ilvane's three-note cradle figure in three-part harmony.

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Anchor Hymn*** (Chain Sword) | A legendary Censer Chain Style weapon |
| Armor | **Chorister's Drowned Surplice** | +50% breath and water resistance in all content |
| Rune | **Rune of Three Voices** | Every third hit of a string repeats as a 25% echo from a random angle |
| Cosmetic | **Choir of Echoes** | An emote in which three small shadows hum |

**Executions**

- **The Choir executes a player.** The anchor chain loops around the player and draws them down into
  the dark between the pews while three voices hum over them.
- **The raid executes the Choir** (Chain Execution). Every player takes hold of the anchor chain and
  hauls, pressing `Execute` in rhythm. The chapel rises out of the marsh, and daylight pours through
  its windows for the first time in twenty-nine years. The three shadows let go of each other's hands
  and rise, singing.

---

## 19. Iron Abbess Velka — the Unquenched

| | |
|---|---|
| **ID** | `boss.velka` |
| **Title** | the Unquenched |
| **Region / mission** | `region.ironroot`, hidden duel `side.ironroot.04` (needs 3 Furnace Keys), recommended Lv 17 |
| **Arena** | **The Hidden Foundry of the Penitent** (`SC_Boss_Velka_Foundry`) |

**Arena.** A chapel that is also a foundry. The pews are anvils and the stained glass is poured
metal. A pool of black quench-water fills the center, with bellows, casting molds and hanging
penitent chains around it. Behind the altar stands a censer mold the size of a house. **Hazards:**

- **The quench pool.** Knocking Velka into it, by heavy knockback or a `Grab` throw near the edge,
  strips her armor and heat for 8 s.
- **Bellows.** Striking one blasts heat across the floor, and in Phase III it also cools Velka.
- **Casting molds.** In Phase III she pours metal into them to trap the player.
- **Heated floor.** In Phase II, glowing floor segments burn.

**Lore.** Two hundred years ago, Velka was Abbess of the Ashen Choir's chantry in Ironroot, and she
forged the first tithe-censers. Every censer in Varanth descends from her molds. When she was dying,
she refused the Last Rite. She climbed into her own casting pool and let the metal take her. She did
not die. She is **unquenched**: metal and fire, still working. As a boy, Tessen was her apprentice for
three years, and ran when he saw what she was making, which was censers big enough to hold cities.
She is still making them.

**Signature weapon: *Penitent Maul*** (War Hammer, Anvil Style). A maul whose head is a glowing,
bell-shaped censer mold, with penitent chains wrapped around the haft. She crushes guards, bounces
fighters off the floor, and has hyper armor on nearly everything.

**Intro cinematic**

1. The penitent's door opens to three Furnace Keys, and heat pours out.
2. Rhythmic hammering. A figure of glowing metal in a nun's cowl is working at an anvil.
3. She doesn't look up. "Little Tessen sent you. He never finished his apprenticeship."
4. She lifts a newly cast censer from its mold. Behind her, the mold the size of a house glows. "Every
   shadow in Varanth deserves a vessel."
5. **IRON ABBESS VELKA, THE UNQUENCHED.**

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. Vespers** | 100–70% | **Penitent Blow**: an overhead with hyper armor. **Mold Slam**: a ground bounce. **Chain Whip**: 4 m of penitent chain. **Bellows Kick**: she kicks the bellows to send heat across the floor. | The foundry, with the quench pool full. | **Spacing and Perfect Parry.** A parry breaks her hyper armor. |
| **II. White Heat** | 70–35% | She glows white, and **her heated strikes are unblockable**. The floor heats in segments. Knocking her into the quench pool strips her heat for 8 s. | Heated floor segments, and steam from the pool. | **Perfect Dodge**, and **using the environment** (the pool). |
| **III. Unquenched** | 35–0% | The quench pool boils dry, and she is molten, slow but relentless. **Molten Embrace**: a grab, techable. **Pour**: she fills the molds around the player to trap them. She can only be **juggled while cooled** by a bellows blast. | No pool. Molds glow. | **Throw teching** and **air combos** during the cooled window. |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 65 | 55 | 40 | 30 | 100 | 55 |

- **Learns to punish:** `habit.turtle` (unblockable heated strikes), `habit.rollspam` (heats the floor
  where rolls end), `habit.jumpin`.
- **Tells:** she recites liturgy between exchanges, and each verse is a 45-frame opening.

**Soundtrack: "The Unquenched"**

- *Instrumentation:* pipe organ, anvils, a women's penitent choir and low brass.
- *Tempo:* P1 80 BPM. P2 100 BPM. P3 60 BPM, a heavy dirge.
- *Leitmotif:* Kessh's Kneel figure transposed into a major key and bent by one flattened note. It is
  the Choir's oldest hymn, as she first wrote it.

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Penitent Maul*** (War Hammer) | A legendary Anvil Style weapon |
| Armor | **Abbess's Unquenched Plate** | Resistance to burning. Heated attacks are shown clearly |
| Rune | **Rune of the Quench** | A Perfect Parry against a hyper-armored attack removes its armor for 1 s |
| Cosmetic | **Ember Halo** | A ring of embers |
| Service | Tessen's forge tier III | Through `quest.tessen.02` |

**Executions**

- **Velka executes Rhen.** She presses Rhen into a casting mold and pours. He becomes an iron penitent,
  kneeling in her chapel with the others.
- **Rhen executes Velka.** Rhen drags her by her own chains into the last of the quench water. Steam
  erupts. When it clears, she has finally cooled into black iron, kneeling. "…At last."

---

## 20. Eirmund — the Pale Stag

| | |
|---|---|
| **ID** | `boss.eirmund` |
| **Title** | the Pale Stag |
| **Region / mission** | `region.rimewood`, hidden duel `side.rimewood.05`, recommended Lv 32 |
| **Arena** | **The Grove of First Snow** (`SC_Boss_Eirmund_Grove`) |

**Arena.** White birches around a frozen pond, where curtains of aurora come all the way down to the
ground. The Rimewood dead hide here from the tithe, and their shadows watch from among the trees.
Night, deep snow. **Hazards:**

- **Birches** can be broken and fall across the arena, as bridges or as hazards.
- **The frozen pond** cracks into freezing water.
- **Aurora curtains.** Eirmund teleports through them in Phase II. A player who passes through one
  gets a small burst of Umbra.
- **Deep snow** at the edges of the grove slows movement.

**Lore.** Eirmund was the last prince of the Birchfolk, the people of the steppe before the Greymanes.
During the Mourntide he hunted the first Unsworn with an antler lance and a pack of white hounds, and
he died a few years after the Swearing and was tithed. When the seal began to crack in 497 AS, his
shadow came back up through it wearing the skull of a pale stag, and it remembered. He found the
Rimewood dead fleeing from the aurora and made the Grove of First Snow their sanctuary. He hunts any
Husk that comes near them, and any living person who might lead the tithe to them.

**Signature weapon: *Antler Lance*** (Spear, Pierce Line Style). A long lance of pale antler and
birch. He fights with long pokes and full-screen charges, and vaults on the lance like a leaping stag.

**Intro cinematic**

1. A pale stag stands in the grove where the aurora touches the ground.
2. The shadows of the dead huddle among the birches and watch.
3. The stag rears up, and it is a man in a stag's skull, lance in hand.
4. "You bring warmth into a place of the dead. That is how the stone finds them."
5. **EIRMUND, THE PALE STAG.**

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. The Hunt** | 100–70% | **Antler Thrust**: long pokes. **Stag Vault**: a vault over the player for a cross-up. **Charge**: a full-screen charge. **Birch Break**: he knocks a birch down across the arena. | The grove at night. | **Spacing and Perfect Dodge** (Charge). |
| **II. Aurora** | 70–35% | He **teleports through aurora curtains**. **Stag Run**: he becomes the stag and charges across the arena, vaulting at the end of each pass. **Frost Volley**: shards thrown from his antlers. | The aurora curtains come alive. | **Anti-air** against vaults, and **Perfect Parry** against charges. |
| **III. The Last Hunt** | 35–0% | **He hunts Sable.** His attacks drain the Shadow meter, and **Sever Hunt** pulls Sable out of Rhen for 10 s, fighting on his own while Eirmund goes after him. Activating **Umbral Shadow makes Sable invulnerable** and pulls him back. | The pond ice breaks. | **Umbral Shadow play**: when to hold the meter and when to spend it. |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 55 | 75 | 60 | 50 | 80 | 40 |

- **Learns to punish:** `habit.dash.approach`, `habit.jumpin`, `habit.shadow.raw` (closes in the moment
  Shadow is activated).

**Soundtrack: "The Pale Stag"**

- *Instrumentation:* solo hunting horn, harp, frost bells and overtone voice.
- *Tempo:* P1 92 BPM. P2 112 BPM. P3 130 BPM.
- *Leitmotif:* a four-note hunting-horn call that answers Tharuk's howl in the same key. It was one
  land before it was two.

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Antler Lance*** (Spear) | A legendary Pierce Line Style weapon |
| Armor | **Pale Hunter's Mantle** | Movement in snow and water is not slowed |
| Rune | **Rune of First Snow** | A Perfect Dodge leaves a frost patch that slows the attacker |
| Cosmetic | **Stag Skull Helm** | |

**Executions**

- **Eirmund executes Rhen.** He pins Rhen to a birch with the Antler Lance through his armor strap.
  Frost spreads from the lance, and Rhen's shadow freezes into the bark.
- **Rhen executes Eirmund.** Rhen's cut splits the stag skull, and underneath is a young man's face.
  Eirmund lies down in the snow. The aurora comes down and folds over him like a blanket, and the dead
  of the grove bow.

---

## 21. Sen Ajari — the Lotus Hermit

| | |
|---|---|
| **ID** | `boss.senajari` |
| **Title** | the Lotus Hermit |
| **Region / mission** | `region.cloudspire`, hidden duel `side.cloudspire.05` (after `mission.cloudspire.10`), recommended Lv 47 |
| **Arena** | **The Lotus Summit** (`SC_Boss_SenAjari_Summit`) |

**Arena.** A lotus-shaped stone platform above the clouds, in complete calm, with one small lotus
tree. **Hazards:**

- **Stone petals** open outward in Phase II, leaving edges and gaps.
- **Edges.** A fall costs 20% health and returns the fighter to the platform ("the clouds catch
  you").
- **Drifting petals** move just before his strikes, the breeze of the attack, which works as a visual
  tell.

**Lore.** Sen Ajari founded nothing and taught everyone. He became master of the Still Wind young, and
taught for twelve years before he realized the Oathstone of Discipline was tithing his students' feelings. He told the
Throne he would not swear it again, spent sixty days in meditation undoing his own oath, and walked
up the mountain. He has sat on the lotus stone for sixty years since. He is **oathless**. Rhen became
oathless by dying, and Sen Ajari by choice. He has been waiting for someone ready for the last
lesson, *stillness is not the absence of motion*. He has heard that Oru is dead.

**Signature weapon: *Lotus Rod*** (Staff, Still Wind Style). A rod of lotus-root wood, light and
flexible. He fights with redirections, counters and juggles, and with air combos that look like
dancing.

**Intro cinematic**

1. Silence above the clouds. An old man sits on a lotus of stone.
2. He opens one eye. "Oru took a step?" Rhen nods. Sen Ajari laughs until he cries.
3. "Good boy. Good boy." Then, seriously: "And you? Can you stand still while you move?"
4. He stands without using his hands, and the stone petals open around him.
5. **SEN AJARI, THE LOTUS HERMIT.**

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. Water** | 100–70% | **He doesn't attack. He only redirects.** **Redirect** turns any attack into a throw using the attacker's own momentum, and the heavier the attack, the harder the throw. **Soft Palm**: a push. **Yielding Step**. Grabs and feints work on him. If the player holds back for 5 s, he attacks, lightly. | The closed lotus. | **Restraint.** Grab, feint, and don't commit heavy attacks. |
| **II. Lotus Opening** | 70–35% | **Rising Lotus**: a launcher. **Petal Spin**: an air string. **Cloud Step**: an air dash. | The petals open, leaving edges and gaps. | **Air combos and air recovery.** |
| **III. Full Bloom** | 35–0% | Full speed. **He mirrors the player's mode**: aggressive during Ember Rage, evasive during Umbral Shadow, balanced otherwise. **The Breath**: strings that alternate an attack to parry with an attack to dodge (parry, dodge, parry, dodge). | The lotus turns slowly. | **Perfect Parry and Perfect Dodge in sequence.** |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 20 (rises to 60 in P3) | 100 | 100 | 70 | 60 | 10 |

- **Learns to punish:** every habit, but **he tells the player out loud** which habit he is punishing
  ("You roll like a frightened cat."). He is the teaching boss, and makes the Habit Ledger visible.

**Soundtrack: "Lotus at the Summit"**

- *Instrumentation:* singing bowls, zither, wind and solo alto flute.
- *Tempo:* P1 40 BPM. P2 72 BPM. P3 144 BPM (double-time).
- *Leitmotif:* Oru's single bent note, which here finally **resolves into a chord**.

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Lotus Rod*** (Staff) | A legendary Still Wind Style weapon |
| Armor | **Hermit's Cloudwoven Robe** | +10% air-combo damage |
| Rune | **Rune of the Open Lotus** | When an air combo ends, you get a free air dash |
| Cosmetic | **Floating Lotus** | A meditation idle |

**Executions**

- **Sen Ajari executes Rhen.** He taps Rhen's forehead with one finger. Rhen sits down to meditate and
  goes still, and his shadow gets up and walks away without him. "Rest."
- **Rhen executes Sen Ajari.** Sen Ajari lowers his rod and invites the last strike. Rhen's blade stops
  a hair from his throat. The old man laughs, bows, and comes apart into lotus petals that blow down
  the mountain. He chose his time.

---

## 22. Isketh — the Marrow Queen

| | |
|---|---|
| **ID** | `boss.isketh` |
| **Title** | the Marrow Queen |
| **Region / mission** | `region.verdantrot`, hidden duel `side.verdantrot.05`, recommended Lv 42 |
| **Arena** | **The Marrow Hollows** (`SC_Boss_Isketh_Hive`) |

**Arena.** A honeycomb of bone and fungus beneath the Heartwood, lit by glowworms, with glowing wax
cells and larvae sacs. **Hazards:**

- **Larvae sacs** burst when hit and release Marrow Drones.
- **Honeycomb walls** can be broken to open new paths. In Phase II, Isketh bursts out of them.
- **Dripping marrow** forms pools that slow.
- **Glowworm swarms.** In Phase III the hive goes dark except near them.

**Lore.** Before Myrrhen, the Rot had another bride. **Isketh** was a famine-queen of the Greenfold's
eastern villages. She married the Rot in 380 AS and taught it to feed on the living, taking their
marrow for the mycelium. When Myrrhen came in 402 AS, the Rootbound walled Isketh into the Marrow
Hollows themselves. She has been feeding on the rooted dead through the roots ever since, waiting.
She despises Myrrhen's gentleness: "She *keeps* them. I *use* them."

**Signature weapon: *Spinecoil*** (Whip Blade, Silken Lash Style). A whip made from a living spine,
vertebra by vertebra, ending in a blade-tail. She zones at long range with snaps and pulls, and she
climbs the walls and ceiling.

**Intro cinematic**

1. The hive breathes, and the wax cells glow.
2. Something with too many joints climbs across the ceiling, dripping from Spinecoil.
3. She drops down upside down, face to face with Rhen. "Warm. You're *warm*."
4. "Myrrhen sends me her leftovers. You're not a leftover."
5. **ISKETH, THE MARROW QUEEN.**

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. The Hive** | 100–70% | **Spine Snap**: a long whip. **Coil Pull**: a pull, techable or dodgeable. **Tail Sting**: poison. **Drone Call**: she bursts the larvae sacs. | Hive floor, glowworm light. | **Approaching through zoning** with Perfect Dodge. |
| **II. The Walls** | 70–35% | She climbs the walls and ceiling and **attacks from above**. She breaks through honeycomb walls to appear from new angles. | Walls break and new paths open. | **Anti-air.** Catch her drops. |
| **III. Marrow Feast** | 35–0% | **Marrow Drain**: every hit she lands **removes 3% of the player's maximum health** for the rest of the fight, unless it is **perfect-parried**. A Perfect Parry also restores the most recent lost chunk. | Darkness except near glowworm swarms. | **Perfect Parry.** |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 75 | 45 | 65 | 80 | 60 | 70 |

- **Learns to punish:** `habit.turtle` (Coil Pull), `habit.jumpin` (times her ceiling drops),
  `habit.parrymash` (feinted snaps).

**Soundtrack: "Marrow Queen"**

- *Instrumentation:* insect-like strings played with the wood of the bow, bone percussion, detuned harp
  and whispered chant.
- *Tempo:* P1 104 BPM. P2 126 BPM. P3 148 BPM.
- *Leitmotif:* a skittering sixteenth-note ostinato built on Myrrhen's wedding march, with the missing
  beat filled in by a click.

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Spinecoil*** (Whip Blade) | A legendary Silken Lash Style weapon |
| Armor | **Chitin of the Marrow Hollows** | Resistance to poison |
| Rune | **Rune of the Hollowed Bone** | A Perfect Parry restores 3% health |
| Cosmetic | **Hive Wings** | A back cosmetic |

**Executions**

- **Isketh executes Rhen.** She coils Spinecoil around Rhen's armored back, drags him into a wax cell
  and seals it. Glowworms crawl over the wax.
- **Rhen executes Isketh.** Rhen cuts Spinecoil in two, and she curls in on herself. He breaks the
  honeycomb above her, the glowworms pour down and wrap her in a cocoon of light, and the hive goes
  still.

---

## 23. The Hundred-Handed Warden — Keeper of the Gate (RAID)

| | |
|---|---|
| **ID** | `boss.hundredhanded` |
| **Title** | Keeper of the Gate |
| **Region / mission** | The Undermourn (reached from `region.solemnthrone`), `raid.hundredhanded` (opens after any ending), **3–4 players**, Lv 60 |
| **Arena** | **The Gate Below** (`SC_Raid_HundredHanded_Gate`) |

**Arena.** The Undermourn side of the Gate: a colossal door of black water standing upright, with
drowned stairs leading up to it. There are two planes, **the Stair** in front and **the Gate Ledge** behind. The Warden fills the background, a torso of fused armor with a hundred arms, each holding a
blade, and the arms reach into both planes. **Hazards:**

- **Arm sweeps** cross one plane at a time, and are telegraphed by the arm cluster raising.
- **The Tithe Pull.** The Gate's surface pulls downed players toward it.
- **Drowned stairs** make three levels of footing.
- **Black-water surges** in Phase III flood one plane at a time.

**Lore.** At the Swearing, a hundred Oathwardens swore to guard the Gate from inside, so that no one
could ever open it from below. Aurem fused their shadows into one Warden with a hundred hands. For
five hundred years it has held its post, and it recites its hundred names so it won't forget them. On
the Rewoven route it kneels to Liss, whose loose shadow is not something it was sworn to stop. After every ending it is still there, and it will not let the living take the road down.

**Signature weapon: *Hundred Edges*** (Dual Blades, Twin Petal Style). A hundred blades used in pairs.
It has the highest hit rate in the game, and at raid scale its long strings become storms of blades
that sweep across both planes.

**Intro cinematic**

1. The raid team comes down the drowned stair. A hundred names are being whispered.
2. The black-water Gate ripples, and arms come out of it, one at a time and then all at once.
3. The Warden's helm lifts. It has a hundred visor slits, each with a faint light behind it.
4. It salutes, raising a hundred blades in the Oathwarden salute.
5. **THE HUNDRED-HANDED WARDEN, KEEPER OF THE GATE.**

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. Hundred** | 100–70% | **Hand Count: 100**, in four clusters of 25 arms. Each cluster attacks its own lane. **Blade Storm**: a string across a whole plane. A Perfect Parry against a storm reflects posture damage into the cluster that threw it. **Breaking a cluster's posture severs it**, lowering the Hand Count by 25 and damaging the Warden's own posture. | Both planes. | **Perfect Parry against long strings.** |
| **II. Names** | 70–35% | The Warden recites names, and **each name summons a Warden-echo** (katana) on one plane with the name shown over its head. **An echo that isn't executed within 10 s rejoins the Warden and heals it 5%.** The Tithe Pull is active. | Echoes on both planes. | **Target priority and execution timing.** |
| **III. The Hundredth Hand** | 35–0% | The last hand holds a key and grips the Gate's lock. **The Gate begins to open**, a 3-minute enrage. **Black-water surges** flood one plane at a time. The raid must break **the Hand** (shared posture) while dodging the surges. | Surges and an opening Gate. | **Perfect Dodge and stagger coordination.** |

**Raid mechanics**

| Mechanic | Rule |
|---|---|
| **Aggro: per-cluster** | Each arm cluster targets the nearest player on its plane. A player who **perfect-parries a cluster's strike takes that cluster's aggro for 8 s**, which is how the raid tanks. |
| **Stagger sharing** | The Hand Count works as a shared posture bar. Severing every cluster in a phase staggers the Warden and opens a **Chain Execution** in which each player executes one arm. |
| **Revives** | **The Tithe Pull:** a downed player slides toward the Gate over 15 s. Holding `Execute` for 3 s revives them, but the nearest cluster targets whoever is reviving. **3 Oath-light Revives** in Phases I–II and **2** in Phase III. A player pulled into the Gate watches until the phase ends. |
| **Enrage** | Phase III's 3-minute Gate timer. If the Gate opens, the attempt ends. |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 70 | 65 | 70 | 45 | 100 | 60 |

- **Learns to punish:** `habit.stack` (sweeps across the crowded plane), `habit.rollspam` (delays in
  its cluster strings). It also always **punishes revivers**, by design.

**Soundtrack: "Keeper of the Gate"**

- *Instrumentation:* a massed choir reciting a hundred names, huge percussion, low brass and pipe organ.
- *Tempo:* P1 76 BPM. P2 96 BPM, with the recited names as the rhythmic spine. P3 120 BPM, with the
  Oathwarden Hymn slowed to half speed.
- *Leitmotif:* the Oathwarden Hymn at half speed. It is Hask's theme, and Rhen's, slowed to a hundred
  voices.

**Legendary rewards**

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Hundred Edges*** (Dual Blades) | A legendary Twin Petal Style weapon |
| Armor | **Gatewarden's Hundredfold Mail** | +10% posture resistance |
| Rune | **Rune of the Hundred Names** | Each hit past the 10th in a string adds +1% posture damage, up to +10% |
| Cosmetic | **Spectral Arms** | An aura of ghostly arms |

**Executions**

- **The Warden executes a player.** A dozen hands close around the player like a fist and draw them
  through the surface of the Gate. The recitation gains one name.
- **The raid executes the Warden** (Chain Execution). Each player takes hold of one of the last arms.
  Together they bow the Warden's helm down to the stair. It speaks all hundred names at once, then
  only one, its own first name, and goes silent. The arms lower like a salute.

---

## 24. The First Shadow — Umbra Prime

| | |
|---|---|
| **ID** | `boss.firstshadow` |
| **Title** | Umbra Prime |
| **Region / mission** | The Undermourn keystone, `finale.rewovenoath` (Balanced route with `quest.liss` complete and 12 fragments), Lv 60 |
| **Arena** | **The Keystone** (`SC_Boss_FirstShadow_Keystone`) |

**Arena.** The heart of the Undermourn: an upside-down cathedral hanging from the underside of the
Gate. The sky is drowned, and five centuries of tithed shadows turn through it like weather. The
floor is black water that reflects the waking world. **Liss stands at the edge of the arena holding
the twelve Oath Fragments like lanterns**, and as the fight goes on she lights them. **Hazards:**

- **Shadow weather.** Gusts of tithed shadows cross the arena and grab anyone in their path.
- **Fragment light.** In Phase III, each fragment Liss speaks creates a safe circle of light.
- **The memory-regions.** In Phase II the arena changes into one of the twelve regions every 20 s.

**Lore.** The First Shadow is Aurem's own shadow. He cut it away with the First Oathblade at the
Swearing and threw it into the Gate as its keystone. For five hundred years it has held every tithed
shadow, every name in the Ledger, and swollen into **Umbra Prime**. It was never evil. It is the
hunger and grief Aurem refused to carry: the part of him that would have died, been afraid, and
wept. It has held too much for too long, and all it wants is to be taken back. Its blade, Oathsunder,
is the cut itself, the act of severing given an edge.

**Signature weapon: *Oathsunder*** (Nodachi, Longvow Style). A nodachi of solid Umbra with a core of
sun-gold. It is the blade that made the first cut. The First Shadow fights with huge reach and slow
charged attacks behind armor, and it can **sever**: cut the tether between Rhen and Sable.

**Intro cinematic**

1. Rhen, Sable and Liss descend through the Gate. As they pass, the Hundred-Handed Warden kneels to
   Liss.
2. Below them, the tithed shadows circle like a storm around a vast kneeling figure made of five
   hundred years of shadow and crowned with light.
3. It raises its head. It has Aurem's face, enormous, weeping ink. "He cut me away so he could stand in
   the light."
4. Liss raises the first fragment, and it glows. "We came to take you home."
5. It draws Oathsunder, and the sound is a cut that happened five hundred years ago. **THE FIRST
   SHADOW, UMBRA PRIME.**

**Combat phases**

| Phase | HP | New moves and mechanics | Arena change | Skill tested |
|---|---|---|---|---|
| **I. The Keystone** | 100–70% | **Sunder Cleave**: a massive charged slash with armor. **Keystone Thrust**. **Grief Wave**: a shadow wave along the floor, to be jumped. **Oathsunder Draw**: the mirror of Aurem's draw-cut. Liss lights **fragments I–IV** at each posture break. | The Keystone cathedral, with shadow weather. | **Perfect Parry against charged slashes.** |
| **II. Twelve Lands** | 70–35% | Every 20 s **the arena becomes one of the twelve regions**, and the First Shadow fights with **that region's Oathlord's signature technique**: Kessh's Litany, Ilvane's Undertow Spin, Gorran's Long-Short-Short, the Petals' Kinship String, Maal's Noon Judgment, Tharuk's Fang Rush, Veloran's Lash Snap, Myrrhen's Reaping Hook, Oru's Still Answer, Quill's Page-Blades, Hask's Longvow Cleave and Aurem's Twelvefold. Liss lights **fragments V–VIII**. | Twelve region set-pieces in rotation. | **Adaptivity and fundamentals.** A boss rush inside one body. |
| **III. Umbra Prime** | 35–0% | **Sever**: it cuts Rhen and Sable apart, and they fight as two bodies. The player controls **Rhen (Ember only)** and **tags to Sable (Umbra only)** with `Shadow`. The First Shadow always goes after the one the player isn't controlling. Liss speaks **fragments IX–XII**, and each creates a **circle of light** where it cannot reach. **At 10%, Liss speaks the rewoven clause.** Rhen and Sable rejoin, and the joint **Rewoven Ultimate**, Ember and Umbra together, becomes the finisher. | Shadow weather at its worst, pushed back by circles of light. | **Tag timing, Shadow Time mastery, and Perfect Dodge.** |

**AI personality**

| Aggression | Patience | Adaptivity | Cunning | Courage | Showmanship |
|---|---|---|---|---|---|
| 75 | 60 | 90 | 70 | 100 | 90 |

- **Learns to punish:** the player's top habits from the whole campaign. In Phase II **each Oathlord
  form keeps the learned answers of the Oathlord it copies.**

**Soundtrack: "Umbra Prime"**

- *Instrumentation:* full orchestra, a choir of thousands (the tithed voices) and **Liss's solo voice**.
- *Tempo:* P1 72 BPM. P2 quotes each Oathlord's theme at that theme's own tempo, following the arena as
  it changes. P3 runs from 60 BPM up to 150 BPM. At the end, the **Rebirth Song** on a solo child's
  voice at 66 BPM, as the orchestra resolves the Oath Row into the **first major chord of the game**.
- *Leitmotif:* the Oath Row inverted (the Oath's shadow), set against the Rebirth Song.

**Legendary rewards** (granted after the credits)

| Type | Item | Effect / note |
|---|---|---|
| Weapon | ***Oathsunder*** (Nodachi) | A legendary Longvow Style weapon |
| Armor | **Rewoven Raiment** | Ember and Umbra meters gain +5% |
| Rune | **Rune of Lend and Return** | When Umbral Shadow ends, half of the shadow-echo damage it dealt comes back as health |
| Cosmetic | **Dawnshadow** | Rhen and Sable's twin silhouette at rest |

**Executions**

- **The First Shadow executes Rhen.** It cuts the space between Rhen and his shadow. Sable falls away
  into the storm, Rhen stands there empty, and the shadows fold over him.
- **Rhen executes the First Shadow.** This is not a killing. Rhen and Sable, together again, take
  Oathsunder from its hand and sheathe it in its chest. The vast shape shrinks until it is only the
  shadow of one man, Aurem's, kneeling in front of Liss. She says: "Lend, and return." It dissolves
  into light, and every tithed shadow in the Undermourn rises with it.

---

## Appendix A. Boss index

| # | Boss ID | Name | Mission | Phase thresholds | Arena scene | Rec. Lv | Raid |
|---|---|---|---|---|---|---|---|
| 1 | `boss.kessh` | Abbot Kessh | `mission.emberfall.10` | 65 / 30 | `SC_Boss_Kessh_Belltower` | 5 | |
| 2 | `boss.ilvane` | Mother Ilvane | `mission.weepingreeds.10` | 70 / 40 | `SC_Boss_Ilvane_BellCauseway` | 10 | |
| 3 | `boss.gorran` | Gorran Vox | `mission.ironroot.10` | 70 / 35 | `SC_Boss_Gorran_Crucible` | 15 | |
| 4 | `boss.petals` | Ysolde & Yrrah | `mission.silkwind.10` | 65 / 30 | `SC_Boss_Petals_TwinPavilion` | 20 | |
| 5 | `boss.maal` | Seraph Maal | `mission.glassossuary.10` | 70 / 40 | `SC_Boss_Maal_NoonCathedral` | 25 | |
| 6 | `boss.tharuk` | Tharuk Greymane | `mission.rimewood.10` | 70 / 35 | `SC_Boss_Tharuk_HowlingCairn` | 30 | |
| 7 | `boss.veloran` | Duke Veloran Sae | `mission.lanternhold.10` | 70 / 35 | `SC_Boss_Veloran_Ballroom` | 35 | |
| 8 | `boss.myrrhen` | Queen Myrrhen | `mission.verdantrot.10` | 65 / 30 | `SC_Boss_Myrrhen_Heartwood` | 40 | |
| 9 | `boss.oru` | Master Oru | `mission.cloudspire.10` | 70 / 35 | `SC_Boss_Oru_TempleOfStillness` | 45 | |
| 10 | `boss.quill` | Archivist Quill | `mission.sunkenarchive.10` | 70 / 35 | `SC_Boss_Quill_Rotunda` | 50 | |
| 11 | `boss.hask` | General Hask Varrow | `mission.bloodmoon.10` | 70 / 35 | `SC_Boss_Hask_VowHall` | 55 | |
| 12 | `boss.aurem` | Emperor Aurem | `mission.solemnthrone.10` | 70 / 35 | `SC_Boss_Aurem_SunHall` | 60 | |
| 13 | `boss.tamsin` | Tamsin | `mission.lanternhold.08` (plus 5 encounters) | 65 / 30 | `SC_Boss_Tamsin_Rooftops` | 33 | |
| 14 | `boss.sableascendant` | Sable Ascendant | `finale.gate` | 65 / 30 | `SC_Boss_SableAscendant_Mirror` | 60 | |
| 15 | `boss.ferrousmaw` | The Ferrous Maw | `raid.ferrousmaw` | 70 / 35 | `SC_Raid_FerrousMaw_Cradle` | 35 | **Yes** |
| 16 | `boss.corvaine` | Lady Corvaine | `side.lanternhold.05` | 70 / 35 | `SC_Boss_Corvaine_MourningHouse` | 37 | |
| 17 | `boss.ossric` | Ossric | `side.glassossuary.05` | 70 / 35 | `SC_Boss_Ossric_Tomb` | 27 | |
| 18 | `boss.drownedchoir` | The Drowned Choir | `raid.drownedchoir` | 70 / 35 | `SC_Raid_DrownedChoir_Chapel` | 25 | **Yes** |
| 19 | `boss.velka` | Iron Abbess Velka | `side.ironroot.04` | 70 / 35 | `SC_Boss_Velka_Foundry` | 17 | |
| 20 | `boss.eirmund` | Eirmund | `side.rimewood.05` | 70 / 35 | `SC_Boss_Eirmund_Grove` | 32 | |
| 21 | `boss.senajari` | Sen Ajari | `side.cloudspire.05` | 70 / 35 | `SC_Boss_SenAjari_Summit` | 47 | |
| 22 | `boss.isketh` | Isketh | `side.verdantrot.05` | 70 / 35 | `SC_Boss_Isketh_Hive` | 42 | |
| 23 | `boss.hundredhanded` | The Hundred-Handed Warden | `raid.hundredhanded` | 70 / 35 | `SC_Raid_HundredHanded_Gate` | 60 | **Yes** |
| 24 | `boss.firstshadow` | The First Shadow | `finale.rewovenoath` | 70 / 35 | `SC_Boss_FirstShadow_Keystone` | 60 | |

## Appendix B. Legendary signature weapons by class

Every canon weapon class has at least one legendary boss weapon.

| Class | Legendary weapons (boss) |
|---|---|
| `weapon.katana` | *First Oathblade* (Aurem), *Umbral Oathblade* (Sable Ascendant) |
| `weapon.nodachi` | *Vowcleaver* (Hask), *Oathsunder* (First Shadow) |
| `weapon.dualblades` | *Thorn & Bloom* (Petals), *Hundred Edges* (Hundred-Handed Warden) |
| `weapon.spear` | *Noonpiercer* (Maal), *Antler Lance* (Eirmund) |
| `weapon.naginata` | *Tidebell* (Ilvane) |
| `weapon.staff` | *Unmoving Branch* (Oru), *Lotus Rod* (Sen Ajari) |
| `weapon.scythe` | *Bridal Reaper* (Myrrhen), *Last Harvest* (Ossric) |
| `weapon.warhammer` | *Worldanvil* (Gorran), *Penitent Maul* (Velka) |
| `weapon.gauntlets` | *Fangs of the Long Night* (Tharuk), *Crucible Fists* (Ferrous Maw) |
| `weapon.daggers` | *Ninefold Fang* (Tamsin) |
| `weapon.chainsword` | *Censer of Last Rites* (Kessh), *Anchor Hymn* (Drowned Choir) |
| `weapon.whipblade` | *Silken Lash* (Veloran), *Spinecoil* (Isketh) |
| `weapon.arcane` | *Codex Umbrae*, Grimoire (Quill), *Mourning Fans*, Warfan (Corvaine) |

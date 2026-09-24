# OATHSUNDER — Combat Design

> **Document:** GDD 02-01 · **Owner:** Lead Combat Design · **Status:** Phase 2 design baseline
> **Authority:** `00-Canon.md` §7 is canon. The combat simulation code and the move data (JSON) are
> authoritative for frame-level numbers. Values marked **(engine)** mirror the shipped data
> (`tuning.combat.json`, `moveset.universal.json`, `moveset.katana.json`, `rules.*.json`,
> `fighter.rhen.json`). Values marked **(target)** are design targets the data must meet, and values
> marked **(proposal)** are design rules not yet in data. If tuning needs a different value, the
> change is logged here first.

---

## 1. Combat at a glance

- Two fighters (PvP) or Rhen against 1–6 enemies (PvE) on a 2.5D plane. Arenas are rendered in full depth, but combat is resolved on a single lane.
- **60 ticks/s fixed-point deterministic simulation.** Rendering is unlocked (up to 240 FPS) and interpolated. Replays, rollback netcode, Time Trial validation and resume-from-suspend all depend on this determinism.
- Eleven logical buttons (canon §7.1): `Light`, `Heavy`, `Special`, `Guard`, `Dodge`, `Jump`, `Grab`, `Execute`, `Ultimate`, `Rage`, `Shadow`.
- Directions use **numpad notation relative to facing**: `6` forward, `4` back, `2` down, `8` up, `5` neutral, with `1 3 7 9` as the diagonals.
- Moves are data. Every move has a stable ID `<weapon-short>.<move>` (e.g., `katana.l1`), and universal actions use `universal.*`.

### 1.1 Notation legend (used in every GDD chapter)

| Notation | Meaning |
|---|---|
| `5L`, `5H`, `5S` | Neutral Light / Heavy / Special |
| `2L`, `6L`, `4H`, `2H` | Direction + button (`2L` = crouching Light) |
| `L L L L` | Pressing Light repeatedly to advance the Light chain (`l1 → l2 → l3 → l4`) |
| `L L H` | Branch: two Lights, then Heavy (e.g., `katana.l2h`) |
| `j.L`, `j.H` | Jumping Light / Heavy |
| `[H]` / `]H[` | Hold Heavy / release Heavy |
| `236S` | Motion input (Classic): down, down-forward, forward + Special |
| `6S` | Simplified shortcut: forward + Special |
| `>` | Cancel into the next move |
| `,` | Link: the next move starts after the previous one recovers |
| `jc` | Jump-cancel (press `Jump` on hit) |
| `dl.` | Delay the next input |
| `~` | Follow-up input during a move (e.g., `5S ~ L`) |
| `(wb)` / `(gb)` | Wall bounce / ground bounce |
| `CH` / `PC` | Counter Hit / Punish Counter required |
| `RAGE`, `SHADOW`, `ULT`, `EXE`, `GRAB` | The `Rage`, `Shadow`, `Ultimate`, `Execute`, `Grab` buttons |

---

## 2. Logical buttons

| Button | Primary function | Secondary / contextual | Hold behavior |
|---|---|---|---|
| `Light` | Light chain (`l1…`), fast normals | `2L` low, `6L` forward normal, `j.L` air | — |
| `Heavy` | Heavy chain (`h1…`), branches from the Light chain | `4H` overhead, `2H` sweep, `j.H` spike | Hold = class Charged attack (`hcharge` → `hcharged`) |
| `Special` | Class Special skills | Direction or motion selects the skill | Some Specials have hold properties (e.g., charge levels) |
| `Guard` | Standing guard | `Guard`+`2` = crouch guard; fresh press = Perfect Parry attempt | Hold to keep guarding |
| `Dodge` | `universal.dodge` *Umbral Sway* (`5`, `2`, `7`, `8`, `9`) | `6`+`Dodge` dash, `4`+`Dodge` backstep, `3`+`Dodge` forward roll, `1`+`Dodge` back roll; `universal.techroll` on a soft knockdown | — |
| `Jump` | Jump (neutral) | `7`/`9` held = back/forward jump; jump-cancel on launcher hit | — |
| `Grab` | `universal.throwforward` *Hilt Toss* (any non-back direction) | `4`/`1`/`7`+`Grab` = `universal.throwback` *Reversing Wheel*; throw tech while being thrown | — |
| `Execute` | Paired Execution when the conditions are met | Does nothing unless the conditions are met (engine); the touch button appears only when an Execution is available | — |
| `Ultimate` | Weapon Ultimate when the Ultimate meter is full | — | — |
| `Rage` | `universal.rage` (Ember Rage) when the Rage meter is full | Usable grounded, airborne **or while stunned** (Rage Burst, §9.2) | — |
| `Shadow` | `universal.shadow` (Umbral Shadow) when the Shadow meter is full | Usable grounded or airborne, never while stunned; never while Ember Rage is active (§9) | — |

The dedicated `Jump` button means holding `8` does not jump by default. This keeps `8S` (Simplified) and
`623S` (Classic) from producing accidental jumps. Classic players can turn on **Up-to-Jump**
(off by default) in Controls.

---

## 3. Control schemes

Both schemes use the same move data, the same normals and the same Execution, Ultimate, Rage
and Shadow buttons. They differ only in how **Specials** are entered.

### 3.1 Classic

Special skills use motion inputs. Standard slot mapping for every class:

| Slot | Classic input | Move ID suffix | Katana example |
|---|---|---|---|
| Neutral Special | `5S` | `.s` | `katana.s` Stillwater |
| Quarter-circle forward | `236S` | `.qcfs` | `katana.qcfs` Crescent Rush |
| Dragon punch | `623S` | `.dps` | `katana.dps` Ascending Dragon |
| Quarter-circle back | `214S` | `.qcbs` | `katana.qcbs` Severing Wind |
| Air quarter-circle forward (some classes) | `j.236S` | `.jqcfs` | — (Katana has none) |

**Motion reading (target):**

| Rule | Pad / keyboard / stick | Touch stick |
|---|---|---|
| Max frames from first to last direction of a motion | 12 | 15 |
| `236` accepts | `2 3 6`, `2 6` | `2 3 6`, `2 6`, `1 3 6` |
| `623` accepts | `6 2 3`, `6 3 2 3`, `3 2 3` | same + `6 2 6` |
| `214` accepts | `2 1 4`, `2 4` | `2 1 4`, `2 4`, `3 1 4` |
| Priority when several motions match | `623` > `214` > `236` > `5S` | same |
| Buffer | Completed motion + `Special` stays valid for **8** fighter frames (canon); hitstop does not age the buffer | same |

### 3.2 Simplified

Special skills use **one direction + Special**. This scheme was designed for touch, and it is legal
everywhere, including ranked.

| Held direction at the `Special` press | Resolves to | Katana example |
|---|---|---|
| `6` | `.qcfs` slot (`6S`) | Crescent Rush |
| `8` | `.dps` slot (`8S`) | Ascending Dragon |
| `4` | `.qcbs` slot (`4S`) | Severing Wind |
| Any other direction (or none) | Neutral Special `.s` (`5S`, the same trigger in both schemes) | Stillwater |
| Airborne + `6` | `.jqcfs` slot (`j.6S`) | — |

**(engine)** Current trigger data matches the exact direction (`6`, `8`, `4`), and any other direction falls through to `.s`.
**(proposal)** Widen the Simplified trigger masks to `36`, `789` and `14` for touch comfort, so diagonals
resolve to the nearest slot.

**Simplified trade-off (engine: `simplifiedStartupPenalty` = 2):** Specials performed through a
**Simplified-only trigger** (`6S`, `8S`, `4S`, `j.6S`) have **+2 frames of startup**. `5S` uses the same
trigger in both schemes and has no penalty. Damage, posture, hitboxes, recovery and meter gain are
identical to Classic.
Ranked allows both schemes with **no damage penalty**. The scheme icon is shown on the versus screen
so it is always public.

### 3.3 Scheme parity rules

1. Normals, throws, universal actions, Executions, Ultimates, Rage and Shadow are identical in both schemes.
2. No Special exists in only one scheme.
3. Balance reviews track win rate by scheme each season. If Simplified win rate differs from Classic by more than 3 points in any tier with at least 10,000 games, the +2 frame value is re-examined here first.
4. Switching scheme takes one toggle from the pause or pre-match menu, including between ranked games.

### 3.4 Touch layout

Touch uses a **floating stick** on the left and an arc of action buttons on the right. Coordinates
give each control's center as a percentage of the **safe area** (x from the left edge, y from the
bottom edge). Sizes are diameters in density-independent pixels (dp). There is always at least
12 dp between buttons, and every hit target is at least 44 dp.

#### Phone (reference 20:9 landscape)

| Control | Center (x%, y%) | Size (dp) | Gesture |
|---|---|---|---|
| Movement stick (floating) | Spawns where the thumb lands inside x 0–40%, y 0–70%; rest position (15, 25) | Base 120, knob 48 | 8-way, 18% dead zone; double-tap-flick `6`/`4` = `universal.dash` / `universal.backstep` |
| `Light` | (87, 21) | 88 | Tap |
| `Heavy` | (95, 42) | 72 | Tap; hold = Charged attack |
| `Special` | (77, 11) | 72 | Tap = `5S`; **flick** up / toward / away = `8S` / `6S` / `4S` (Simplified, auto-mirrors with facing) |
| `Guard` | (76, 34) | 68 | Hold = guard; with stick `2` = crouch guard; a fresh tap = Perfect Parry attempt |
| `Dodge` | (66, 15) | 64 | Tap = Umbral Sway; swipe toward / away = dash / backstep; swipe down-toward / down-away = forward roll / back roll |
| `Jump` | (94, 64) | 60 | Tap; the stick's `7`/`9` sets jump direction |
| `Grab` | (85, 47) | 56 | Tap; with stick `4` = throw back |
| `Execute` | (66, 55), contextual | 96 | Appears only while an Execution is available (pulsing ember ring, 0.2 s fade-in) |
| `Ultimate` | (93, 87) | 60 | Lit and haptic pulse when the meter is full |
| `Rage` | (81, 87) | 52 | Next to the Rage meter |
| `Shadow` | (69, 87) | 52 | Next to the Shadow meter |
| Pause | (50, 95) | 40 | PvE pause; PvP options overlay |

#### Tablet (reference 4:3 landscape)

| Control | Center (x%, y%) | Size (dp) | Notes |
|---|---|---|---|
| Movement stick | Zone x 0–35%, y 0–60%; rest (12, 20) | Base 140, knob 56 | Same gestures as phone |
| `Light` | (89, 17) | 104 | |
| `Heavy` | (95, 34) | 88 | |
| `Special` | (79, 8) | 88 | Flick gestures |
| `Guard` | (82, 28) | 80 | |
| `Dodge` | (69, 14) | 76 | Same gestures as phone |
| `Jump` | (95, 52) | 72 | |
| `Grab` | (88, 42) | 68 | |
| `Execute` | (74, 44), contextual | 112 | |
| `Ultimate` | (94, 90) | 72 | |
| `Rage` | (86, 90) | 60 | |
| `Shadow` | (78, 90) | 60 | |

**Touch customization:** drag any control, scale 70–130%, opacity 20–100%, left-hand mirror, 3 saved
layouts per device, and a "Compact" preset for screens under 6". **Classic-on-touch** replaces Special
flicks with motion reading on the stick (15-frame leniency) and keeps tap-`Special` as `5S`.
**Touch haptics** confirm every successful button press with a 6 ms tick (can be turned off).

### 3.5 Gamepad default bindings

| Logical | Xbox layout | PlayStation layout | Notes |
|---|---|---|---|
| Movement | Left stick / D-pad | Left stick / D-pad | Both active; D-pad recommended for Classic |
| `Light` | X | Square | |
| `Heavy` | Y | Triangle | Hold for Charged attack |
| `Special` | B | Circle | |
| `Jump` | A | Cross | |
| `Guard` | RB | R1 | |
| `Dodge` | RT | R2 | DualSense: R2 stiffens during Dodge fatigue (§5.3) |
| `Grab` | LB | L1 | |
| `Execute` | LT | L2 | |
| `Ultimate` | Right stick ↑ or R3 click | Right stick ↑ or R3 | Right-stick directions are screen-absolute, not facing-relative |
| `Rage` | Right stick ← | Right stick ← | |
| `Shadow` | Right stick → | Right stick → | |
| Pause / Menu | Menu | Options | |
| Training: reset position | View | Touchpad | Training only |

**Arcade stick / leverless preset:** top row `Light`, `Heavy`, `Special`, `Ultimate`; bottom row
`Guard`, `Dodge`, `Grab`, `Execute`; `Jump` on `8` (Up-to-Jump on); `Rage` on L3/Function 1,
`Shadow` on R3/Function 2. **SOCD cleaning:** left+right = neutral; up+down = neutral.

### 3.6 Keyboard default bindings

| Logical | Default key | Alternate layout ("Arrows") |
|---|---|---|
| `4` / `6` (screen left/right) | A / D | ← / → |
| `8` / `2` | W / S | ↑ / ↓ |
| `Jump` | Space | Z |
| `Light` | J | A |
| `Heavy` | K | S |
| `Special` | L | D |
| `Guard` | I | Q |
| `Dodge` | O | W |
| `Grab` | U | E |
| `Execute` | ; | R |
| `Ultimate` | R | F |
| `Rage` | Q | C |
| `Shadow` | E | V |
| Pause | Esc | Esc |

Every binding on every device is fully remappable. A physical input can be bound to only one
logical button, so there are no macros, and remapping never creates an input that does two things at once.

---

## 4. Movement and spacing

| Action | Input | Notes |
|---|---|---|
| Walk | `4` / `6` | Walk speed is a class property (e.g., Gauntlets fastest, Nodachi slowest) |
| Crouch | `2` (`1`, `3`) | Lowers the hurtbox; some highs flagged *whiffs on crouch* miss entirely |
| Jump | `Jump` (+`7`/`8`/`9`) | 4-frame prejump (engine) in which the fighter cannot guard and can still be thrown. No air guard, no double jump; 1 air action per jump (engine) |
| Dash | `66` or `6`+`Dodge` | `universal.dash` *Ember Step* |
| Backstep | `44` or `4`+`Dodge` | `universal.backstep` *Fading Step* |
| Rolls | `3`+`Dodge` / `1`+`Dodge` | `universal.rollforward` *Crossing Roll* / `universal.rollback` *Retreating Roll* |
| Dodge | `Dodge` (neutral, or with `2`/`7`/`8`/`9`) | `universal.dodge` *Umbral Sway* |

**Facing and cross-ups.** Guard protects only the front. Facing auto-corrects while the fighter
is in neutral, walking, crouching or holding `Guard`. It does **not** auto-correct during hitstun,
blockstun, knockdown/rise, or the recovery of any action. The auto-turn takes **4 frames (target)**, and
the fighter's back is exposed during that time. A cross-up works by landing a hit from behind during one of those
locked states, or by switching sides faster than the auto-turn.

**Walls.** Every ranked arena is a walled box. PvE arenas may have hazards (canals, ember vents,
bridge edges). Hazards are unblockable, so they are always marked with a crimson floor border before
they become dangerous (08-Enemies §9).

---

## 5. Evasion: dash, backstep, rolls and dodge

### 5.1 Comparison (engine data, `moveset.universal.json`)

| | `universal.dash` *Ember Step* | `universal.backstep` *Fading Step* | `universal.rollforward` *Crossing Roll* | `universal.rollback` *Retreating Roll* | `universal.dodge` *Umbral Sway* |
|---|---|---|---|---|---|
| Input | `66` or `6`+`Dodge` | `44` or `4`+`Dodge` | `3`+`Dodge` | `1`+`Dodge` | `Dodge` (+`5`/`2`/`7`/`8`/`9`) |
| Total frames | 20 | 24 | 28 | 26 | 22 |
| Travel | Fast forward burst (f1–12), then slows | Quick hop back (f1–10) | Long roll that **passes through** the opponent (side switch) | Long roll back | Short sway (f1–6); stays in range |
| Invulnerability | None | Strikes f1–7 | Strikes and projectiles f3–16 | Strikes and projectiles f3–14 | Strikes and projectiles f2–11 |
| **Perfect window** | None | None | None | None | **f2–7 (6 frames)** |
| Cancels | Into normals, command normals, Specials, Ultimate or throw from f10 | — | — | — | After a **Perfect Dodge**, into normals, command normals, Specials or Ultimate on f8–16 |
| Best use | Close distance, run-up pressure, run-up throw | Make a close attack whiff | Escape the corner, go through a slow attack | Reset to mid-range | Beat a strike you have read, trigger Shadow Time, punish |
| Weakness | Loses to any well-timed poke | Loses to long reach and lows after f7 | Heavily punishable on a read | Loses to long-reach Specials | Loses to throws and delayed attacks |

`universal.techroll` *Recovery Roll* (24 frames, fully invulnerable f1–16) is the wake-up tech (§7.6).

**Raids only:** raid arenas have two gameplay planes, Front and Rear. `Dodge`+`8` moves to the Rear
plane and `Dodge`+`2` to the Front plane, with the same invulnerability as `universal.dodge`
(05-Bosses §0.6). Outside raids, `Dodge`+`8`/`2` is a normal Umbral Sway.

### 5.2 Perfect Dodge and Shadow Time

**For players:** Sway *through* an attack at the last moment. If the attack would have hit you
during the sway's perfect window, time bends: the attacker slows to a crawl while you move at
full speed, your Shadow meter jumps, and you can cancel straight into a punish.

**For designers:**

- A Perfect Dodge triggers when an active strike or projectile hitbox overlaps the dodger's hurtbox during `universal.dodge`'s `perfectEvade` window (f2–7, engine). **Only Umbral Sway has a perfect window.** Rolls, dash and backstep are plain evasion.
- It **cannot** trigger against throws, command grabs, Executions or Ultimate cinematics. It **can** trigger against unblockables, and dodging is the intended answer to them.
- **Shadow Time** strength is set per rule set (engine `rules.*.json`: `perfectDodgeTimeScale`, `perfectDodgeSlowFrames`):

| Rule set | Who is slowed | Time scale | Duration |
|---|---|---|---|
| `rules.story` / `rules.training` | The attacker (PvE: **(proposal)** plus every enemy within 6 m) | 35% speed | 90 frames |
| `rules.boss` | The boss | 35% speed | 75 frames |
| `rules.ranked` (all PvP) | The attacker only | 50% speed | 24 frames |

- A Perfect Dodge grants **+200 Shadow meter (20%)** (engine).
- Presentation: world desaturates toward violet, audio gets a low-pass filter, Sable's silhouette flickers behind Rhen, and the dodger keeps full color.
- **(proposal)** Shadow Time never stacks. A second Perfect Dodge during Shadow Time refreshes the duration but grants no additional meter.

### 5.3 Dodge fatigue (anti-spam) — (proposal)

The **third** Dodge-family action (sway or roll) started within 90 frames of the first gets **+8
recovery frames and no perfect window**. The fatigue resets after 90 frames without a Dodge-family
action. The HUD shows fatigue by dimming the Dodge button icon, and DualSense adds R2 resistance. Not yet
in data. It is proposed for the first balance pass if `habit.rollspam` rates exceed 30% in ranked
telemetry.

---

## 6. Defense

### 6.1 Guard

**Canon:** dedicated `Guard` button. **Standing guard** blocks high, mid and overhead. **Crouch
guard** (`Guard`+`2`) blocks high, mid and low.

| Attack height | Standing guard | Crouch guard | Telegraph |
|---|---|---|---|
| High | Blocks | Blocks (some highs whiff on crouchers) | None |
| Mid | Blocks | Blocks | None |
| Low | **Hits** | Blocks | Amber (§13) |
| Overhead | Blocks | **Hits** | Azure (§13) |
| Air attacks (default) | Blocks | **Hits** (air normals count as overhead unless flagged) | None |
| Throw / command grab | Cannot block | Cannot block | Violet (command grabs) |
| Unblockable | Cannot block | Cannot block | Crimson |

**Blocking results (targets):**

- Blockstun and pushback come from move data. Being in blockstun freezes auto-turn (§4).
- **Chip damage:** Lights and Heavies deal 0 chip. Specials deal 10% of their damage as chip, Charged attacks 15%, Ultimates 25%. **Chip can only KO from an Ultimate** (PvP). In PvE, chip can KO on Oathsundered and higher.
- Every blocked hit adds its full **posture** value to the defender's Posture bar (§8). Block hitstop is 75% of the attack's hitstop (engine).
- **Guard Assist** (Pilgrim difficulty only, PvE only): holding `Guard` automatically picks standing or crouching guard.

### 6.2 Perfect Parry

**For players:** Tap `Guard` just before an attack lands. A clean, well-timed tap parries: you
take nothing, you recover instantly, and the attacker's posture takes the damage. If you mash, the
window shrinks until it is almost impossible to hit.

**Canon:** pressing `Guard` **≤ 6 frames before contact** is a Perfect Parry. **Mashing shortens
the window to 2 frames.**

**Anti-mash rule (engine: `parryWindowFrames` 6, `parryWindowMashFrames` 2, `parryMashThresholdFrames` 20):**

1. Each fresh `Guard` press (a transition from not pressed to pressed) opens a parry window, provided the fighter is in a parry-capable state.
2. If the previous `Guard` press was **20 or fewer fighter frames** earlier, the press counts as mashing and gets a **2-frame** window. Otherwise it gets the full **6-frame** window.
3. A **successful Perfect Parry clears the press history**, so the next press is clean. A skilled player can parry every hit of a string. A masher cannot.
4. Holding `Guard` (blocking) is not a press. Releasing and re-pressing is a press.
5. Frames are counted in the defender's own simulation frames, so hitstop does not age the window. This matches the input buffer rule.

**What can be parried:** strikes and projectiles of any height, as long as the parry is pressed in
a stance that would block it (a crouching press for lows, a standing press for overheads). **Guard
Crush** attacks can be parried. Throws, command grabs, unblockables (`Unparryable` flag), Executions
and Ultimate cinematics cannot be parried.

**Parry results (engine):**

| Effect | Value |
|---|---|
| Damage / chip / posture to the defender | 0 |
| Defender recovery | 8-frame parry recovery, **actionable from frame 1**. The defender can punish immediately while the attacker is still in its move |
| Posture to the attacker | 150% of the parried hit's posture value |
| Attacker stagger | Moves flagged `StaggerOnParry` (e.g., `katana.hchargedfull`) stagger the attacker for **30 frames**. That counts as **staggered** for Execution |
| Meter (defender) | Shadow +150 (15%), Ultimate +30 (3%) |
| Hitstop | The attack's hitstop, minimum 10 frames, on both fighters, with a bell-chime SFX and a white-gold ring VFX |

A parried projectile is destroyed. Only specific class moves reflect projectiles (e.g., Grimoire
*Erasure*).

### 6.3 Throws and throw tech

| Property | Value (engine) |
|---|---|
| `universal.throwforward` *Hilt Toss* | `Grab` with any direction except back (`2 3 5 6 8 9`). Throws forward into a knockdown. 100 damage, 50 posture |
| `universal.throwback` *Reversing Wheel* | `Grab` + `1`/`4`/`7`. Throws behind with a side switch (corner escape). 110 damage, 50 posture |
| Frames | Grab box active f5–6, 30 frames total on whiff (very punishable) |
| Range | Grab box reaches 1.0 m in front of the fighter's origin |
| Throw protection | Fighters are throw-invulnerable for **4 frames** after leaving hitstun or blockstun. Airborne fighters cannot be thrown, and neither can a fighter in prejump |
| **Throw tech** | Press `Grab` within the **8-frame** tech window of the paired throw. Both fighters are pushed apart and recover in 14 frames |
| Beats | Guard, Perfect Parry attempts, counter stances such as `katana.s` Stillwater |
| Loses to | Jumping (prejump is throw-invulnerable), backstep (f1–7), any strike already active. A Perfect Dodge never triggers on a throw, and sway recovery is throwable |

**Command grabs** (class Specials such as `gauntlets.s` Fang Lock) are unblockable and **cannot be
teched**. To keep them fair, every command grab must have **≥ 10 frames startup**, **≥ 40 frames
whiff recovery**, a violet telegraph, and no invulnerability (target).

---

## 7. Offense

### 7.1 Chains, branches and cancels

- **Chains:** repeated presses advance a class's Light or Heavy chain (`l1 → l2 → …`, `h1 → h2 → h3`). Chains accept an input from the start of the previous move until 8 frames after its active frames end.
- **Branches:** a different button at a chain node selects a branch (e.g., `L L H` = `katana.l2h` Heaven's Draw). Branch nodes are listed per class in 06-Weapons.
- **Special cancel:** most normals can be canceled into a Special on hit or block (not on whiff). Chain finishers (e.g., `katana.l4`) can be Special-canceled only if the move data says so (mastery properties unlock several; see 06-Weapons).
- **Mode activation:** `Rage` and `Shadow` activate whenever the fighter can act, and out of a move's recovery where its cancel list allows. Ember Rage can also be activated while stunned (Rage Burst, §9.2).
- **Jump cancel:** launchers can be jump-canceled on hit (`jc`) to chase an air combo.

### 7.2 Counter hits and punishes

| Type | Trigger | Bonus (targets) | Feedback |
|---|---|---|---|
| **Counter Hit** (`CH`) | Hitting a fighter during the startup or active frames of their attack | **(engine)** +20% damage, +4 hitstun frames, +2 hitstop. Moves flagged `LaunchOnCounter` (e.g., `katana.h1` Iron Draw) or *CH-crumple* gain that property | Gold spark, "COUNTER" callout, metallic ring |
| **Punish Counter** (`PC`) | Hitting a fighter during the recovery of a whiffed or blocked action (including landing recovery) | **(engine)** +10% damage, +2 hitstun frames | Crimson spark, "PUNISH" callout, low thud |

Frame advantage on block is shown in Training for every move (09-GameModes §8). Design rule: every
move that is **−10 or worse on block** must be punishable by at least one Light of every class at the
distance where it is blocked, or it must push back far enough that it can't be.

### 7.3 Launchers

- Moves flagged **launcher** put a grounded opponent into a juggle state (e.g., `katana.l2h` Heaven's Draw, `katana.dps` Ascending Dragon).
- Launchers are jump-cancelable on hit. The standard route is launcher `jc` into the class's air chain (`j.L j.L j.H`).
- A launcher opens the juggle and spends Juggle Points like any other hit (§7.4).

### 7.4 Juggles

**Canon:** juggle points cap air combos; gravity scales with combo length; wall and ground bounce
once per combo.

| Rule | Value |
|---|---|
| Juggle budget | **(engine)** 8 Juggle Points (JP) per combo, tracked on the victim. Each juggle hit spends its move's JP cost (1 unless the move data sets `juggleCost`) |
| Budget exceeded | **(engine)** further hits whiff (they pass through the victim), who keeps falling and lands in a knockdown |
| Gravity scaling | **(engine)** +4% gravity per juggle hit, capped at ×1.8 |
| Hitstun decay | **(engine)** after the 6th hit, 1 frame of hitstun is removed per 2 further hits: hit 8 loses 1, hit 10 loses 2, … (minimum 6 frames) |
| Staleness | **(proposal)** the second use of the same move ID in one combo costs double JP and deals ×0.8 damage |
| Class exceptions | **(proposal)** Staff: combos started by a Staff hit have a 10 JP budget (06-Weapons §10). Other exceptions are listed per class in 06-Weapons |

### 7.5 Wall bounce and ground bounce

- **Wall bounce** (`wb`): a hit flagged *wall bounce* sends the victim into the wall; if it reaches the wall, they rebound toward the attacker in a juggle state. Once per combo.
- **Ground bounce** (`gb`): a hit flagged *ground bounce* slams an airborne or standing victim into the floor, and they rebound into a juggle. Once per combo.
- After its bounce has been used, a bounce-flagged hit becomes a regular knockdown: a **wall slump** (soft knockdown) or a **floor slam** (hard knockdown).
- Bounces do not refund Juggle Points. The bounce physics (rebound velocities) are engine tuning.

### 7.6 Knockdown, OTG and wake-up

| State | Rules (targets) |
|---|---|
| Soft knockdown | **(engine)** 32 frames down. Pressing `Dodge` within **10 frames** of hitting the ground techs into `universal.techroll` *Recovery Roll* (24 frames, fully invulnerable f1–16) |
| Hard knockdown | **(engine)** 55 frames down, no tech |
| Wake-up | **(engine)** While down, only `OffTheGround` attacks connect (once per combo). The 22-frame rise is fully invulnerable. The first actionable frame is vulnerable (meaty timing) |
| OTG | **(proposal)** Only moves flagged *OTG* hit a grounded fighter, once per combo |

### 7.7 Armor and invulnerability

- **Armor (N-hit):** absorbs the hitstun of N strikes. Damage and posture still apply. Armor never protects against throws, command grabs, unblockables, Executions or Ultimates.
- **Armor hitstop** is 6 frames (engine).
- **Invulnerability:** invulnerable frames are shown as gold in the Training hitbox view. Invincible reversals (e.g., `katana.dps` Ascending Dragon, strike-invulnerable f1–8) must be at least −20 on block and give a Punish Counter on whiff (target).

---

## 8. Posture and Guard Break

**For players:** The Posture bar sits under your health. Blocking fills it, being parried fills it
faster, and clean hits fill it a little. When it fills, your guard shatters and you are open to an
Execution. Stop taking posture damage and it recovers. A Perfect Parry costs you nothing.

**For designers:**

| Rule | Value (PvP normalized) |
|---|---|
| Posture maximum | **(engine)** 1,000 (`fighter.rhen.json`). PvE: scaled by level and Resolve (07-Progression §2–3) |
| Fills from | Blocking a hit: the move's full posture value (Katana data: Lights 40–140, Heavies 100–160, Charged 220, full charge 400). Being parried: 150% of the parried hit's posture value. **(engine)** Taking a hit: 50% of the move's posture value |
| Does **not** fill from | Perfect Parries you perform |
| Recovery | **(engine)** after 90 frames without posture damage, it regenerates 3 per frame (18% per second) |
| **Guard Crush** | Attacks flagged `GuardCrush` (e.g., `katana.hchargedfull` *Moonsplitter — Full Moon*) fill posture completely on block and cause an instant Guard Break. A parry beats them |
| **Guard Break** | Posture full → **(engine)** 80-frame stagger (executable). PvE durations vary by enemy class. Posture then resets to 0 |
| HUD | The posture bar flashes ember at 75% and cracks visibly at 90% |

Canon lists blocking and being parried as the sources of posture. The engine adds posture on hit
at 50%, which keeps pressure meaningful on opponents who don't block. That is a tuning addition
that doesn't contradict canon.

---

## 9. Meters and modes

Three meters, each tied to a canon energy. **(engine)** Every meter holds 1,000 points.

| Meter | Energy | HUD position | Mode / use |
|---|---|---|---|
| **Rage** | Ember | Left, under the Posture bar (ember orange) | `Rage` → **Ember Rage** |
| **Shadow** | Umbra | Right, under the Posture bar (umbral violet) | `Shadow` → **Umbral Shadow** |
| **Ultimate** | Oath-light | Center-bottom orb (oath gold) | `Ultimate` → weapon Ultimate |

### 9.1 Meter gain (engine, PvP normalized; PvE modifiers in 07-Progression)

With 1,000 HP and 1,000-point meters, "per point of damage" reads directly as a percentage.

| Event | Rage | Shadow | Ultimate |
|---|---|---|---|
| Damage taken | +0.7 per point (losing 10% HP → +7%) | — | +0.5 per point |
| Damage dealt | +0.15 per point | — | +1.0 per point (dealing 10% HP → +10%) |
| Each hit dealt | — | +12 (1.2%) | — |
| Block a hit (defender) | — | — | +15 (1.5%) |
| Perfect Parry (defender) | — | +150 (15%) | +30 (3%) |
| Perfect Dodge | — | +200 (20%) | — |

**Carry-over (engine rule flag `carryUltimateMeter`):** the Ultimate meter carries over between rounds.
Rage and Shadow start each round empty. Active modes end at round end.

### 9.2 Ember Rage (`universal.rage`)

**Canon:** Rage meter full → `Rage`: **8 s, +20% damage, heavy attacks gain 1-hit armor.**

- **(engine)** Activation costs the full meter (1,000) and takes 30 frames. The fighter is invulnerable to strikes, throws and projectiles on f1–20. It can be activated **grounded, airborne or while stunned**. It cannot be activated while Umbral Shadow is active.
- **Rage Burst (engine, `rageBurst` enabled in every rule set):** activation releases a 2.4 m **unblockable, unparryable** burst on f4–6 that deals no damage but knocks down. Used in hitstun or blockstun, it is OATHSUNDER's only combo breaker. It costs the whole Rage meter and gives up the chance to use Rage offensively.
- Duration: 480 frames (engine). **(proposal)** The timer pauses during Executions, Ultimate cinematics and finishers.
- +20% damage (engine), multiplying final damage.
- Heavy attacks gain 1-hit armor (engine `rageHeavyArmorHits` = 1). **(proposal)** For classes whose heavies already have 1-hit armor (Nodachi, War Hammer), the armor becomes 2-hit.
- Presentation: ember veins on Rhen's skin, a taiko layer in the music, weapon trail turns ember.

### 9.3 Umbral Shadow (`universal.shadow`)

**Canon:** Shadow meter full → `Shadow`: **6 s, every hit spawns a delayed shadow echo (Sable
strikes again).**

- **(engine)** Activation costs the full meter and takes 24 frames. The fighter is invulnerable to strikes, throws and projectiles on f1–16. It can be activated grounded or airborne, **not while stunned**, and not while Ember Rage is active.
- Duration: 360 frames (engine).
- **Shadow echo (engine):** every hit spawns Sable's echo, which strikes **10 frames later** for **30% of the triggering hit's damage**. If the victim is still in hitstun, the echo adds 3 frames of hitstun, and the echo's hitstop is 4 frames.
- **(proposal)** Echoes never launch, bounce or knock down. They don't spend Juggle Points or echo throws, Executions, Ultimates or other echoes. They follow blocked hits too, and can be blocked or parried separately.
- Presentation: Sable's violet silhouette with a reversed-audio swing.
- **Ember Rage and Umbral Shadow are mutually exclusive (engine).** Choosing which mode to spend, and when, is a core decision. Rhen holds both energies, but he channels one at a time.

### 9.4 Ultimate

**Canon:** Ultimate meter full → the weapon's Ultimate, a cinematic paired attack.

- Activation strike costs the full meter (1,000). Katana data: invulnerable to strikes, throws and projectiles on f1–14, and flagged `ChipKills`. Ultimates can be used from neutral or as a combo ender (Special-cancel rules apply).
- **On hit:** the paired cinematic plays. Katana data authors *Thousand Oaths* at 150 frames (2.5 s). Cinematic targets per class are in 06-Weapons, capped at 6 s in PvP. **(proposal)** Ultimates used as combo enders use a 50% damage-scaling floor.
- **On block:** meter is spent and chip applies (Ultimate chip can KO). Blocked Ultimates are at least −30 (target, punishable).
- **On whiff:** meter is spent.
- Ultimates are never echoed and cannot be parried or Perfect Dodged once the cinematic starts.

---

## 10. Executions and finishers

### 10.1 Execution

**Canon:** target guard-broken, or ≤ 15% health and staggered → `Execute` in range → paired
execution.

| Rule | Value (targets) |
|---|---|
| Range | **(engine)** 1.8 m |
| **Staggered** states | **(engine)** Guard Break stagger always qualifies; at ≤ 15% health any stagger (including parry stagger from `StaggerOnParry`) or hitstun qualifies. **(proposal)** *Crumple* states from *CH-crumple* moves |
| Invulnerability | Both fighters are invulnerable during the paired animation. Other enemies (PvE) freeze at 10% speed for its duration |
| No target | `Execute` without a valid target does nothing (engine): executions are a reward, never a gamble |
| Duration | 2–4 s (Katana data: 30-frame grab plus a 120-frame paired `katana.execution.hit`) |
| PvP damage | **(engine, Katana)** Guard Break Execution: 300 (30%: 120 + 180), unscaled; lethal when it drops HP to 0. **(proposal)** at ≤ 15% HP the Execution is always lethal |
| PvE outcome | Regular enemies: killed. Elites: 40% of max HP, lethal at ≤ 15%. Bosses: 10% of max HP on Guard Break; phase-ending Executions are scripted per boss |
| Meter | **(proposal)** the executor gains Ultimate +150 (15%) |

Every class has its own Execution (`<weapon-short>.execution`, e.g., `katana.execution`
Oathbreaker's Mercy), which runs 2–4 s against regular enemies, elites and PvP opponents. Every boss
also has an exclusive Execution performed on it (3.5–6 s), and bosses and elites can execute Rhen
under the same canon condition. On a guard-broken Rhen above 15% health it deals heavy damage and
cuts away. At ≤ 15% health and staggered it is the defeat cinematic (05-Bosses §0.5).

### 10.2 Finisher

**Canon:** a round-ending Execution or Ultimate triggers the slow-motion cinematic finisher.

- Slow motion at 25% for 60 frames at the lethal contact, then a finisher camera cut. The finisher lasts at most 3 s in PvP and cannot be skipped in ranked, so both players have the same pace. In PvE it can be skipped after 1 s.
- A round ending on any other hit plays a standard KO: 0.5 s at 50% speed, with no cinematic.
- Finisher variants (visual only) are cosmetics (07-Progression §12).

---

## 11. Hit feel targets

### 11.1 Hitstop and camera

**Canon:** both fighters freeze on contact: light 8, heavy 12, ultimate 16 frames typical.

| Event | Hitstop (frames) | Camera | VFX |
|---|---|---|---|
| Block | 75% of the attack's hitstop (engine) | None | Small white-blue spark |
| Light hit | **8** | None | Class spark, small |
| Heavy hit | **12** | Zoom punch 3% over 6 frames; shake trauma 0.15 | Class spark, large, debris |
| Counter Hit | +2 (engine) | Additional 2% zoom punch | Gold spark, "COUNTER" |
| Punish Counter | +0 | — | Crimson spark, "PUNISH" |
| Launcher | 12 | Tilt up 4°, vertical follow (12-frame lag) | Rising streak |
| Wall / ground bounce | 14 | Shake trauma 0.25 / 0.30 | Wall cracks / floor debris |
| Perfect Parry | The attack's hitstop, minimum 10 (engine), on both fighters | 5% micro-zoom for 10 frames | White-gold ring, sparks fanning away from the defender |
| Perfect Dodge | 0 | Shadow Time grade (desaturate to violet) | Afterimage trail, Sable flicker |
| Guard Break | 20 | Shake trauma 0.40 | Shield-shatter glyph |
| Throw connect | 10 | 3° camera roll following the throw | Cloth and dust |
| Ultimate connect | **16** | Cut to cinematic camera; letterbox 2.35:1 | Class Ultimate VFX |
| Execution start | 12 | Cut to paired camera; letterbox | Class Execution VFX |
| Finisher | Slow motion 25% for 60 frames | Finisher cut | Ember/Umbra burst on the silhouette |

**Camera rules:** side camera, 38° vertical FOV, distance 6–11 m, dynamic framing keeps both
fighters in the inner 70% of the frame. The camera never rotates during PvP play except in
Execution, Ultimate and finisher cinematics. Shake uses a trauma model (shake = trauma²,
decay 1.5/s, max roll 1.5°), and a player slider scales it from 0 to 100%.

### 11.2 Haptics

| Event | Mobile (Core Haptics / Android) | Gamepad rumble |
|---|---|---|
| Light hit | 10 ms transient, intensity 0.3 | High-frequency motor 0.2, 60 ms |
| Heavy hit | 25 ms, intensity 0.6, sharpness 0.4 | Low 0.5 + high 0.3, 120 ms |
| Block | 8 ms, 0.2 | High 0.15, 40 ms |
| Perfect Parry | Double tick 2 × 12 ms, 0.7, 40 ms apart | High 0.6 twice |
| Perfect Dodge | 60 ms soft swell, 0.25 | Low 0.2 swell, 200 ms |
| Guard Break (either side) | 120 ms, 0.8 | Low 0.9, 250 ms |
| Telegraph (accessibility option) | Low: 2 short pulses. Overhead: 1 long pulse. Unblockable: 3 rapid pulses. Grab: 1 soft + 1 sharp | Same patterns on the high-frequency motor |
| Ultimate / Execution | Authored per class | Authored per class |

### 11.3 Audio intent

- **Layered hits:** transient (material clash) + body (class-specific weight) + tail (arena reverb). Heavy hits add a sub-bass thump below 80 Hz.
- **Parry** is a bright bell chime tuned to the weapon's key; the Emberfall bell motif runs through the whole soundtrack.
- **Telegraph stingers** are the highest mix priority: unblockable = rising brass stab, low = low metallic scrape, overhead = high whistle, grab = cloth snap with a reversed breath.
- **Mix priority:** telegraphs > player hits > enemy hits > companion VO > music. Combat VO ducks 6 dB under any telegraph.
- **Adaptive music:** Ember Rage adds a taiko layer, Umbral Shadow reverses and filters the melody stem, Shadow Time applies a low-pass at 800 Hz, and boss phase changes switch stems (`MUS_Boss_<Name>_P<n>_<Stem>`).

---

## 12. Combo philosophy

### 12.1 Freedom

- **Routes, not recipes.** Every class has at least three viable routes from each starter type (Light, low, overhead, launcher, Counter Hit, Punish Counter, Perfect Parry, Shadow Time), so players can express style.
- **Meter as expression.** Rage or Shadow activation mid-combo and Ultimate enders turn the same starter into different outcomes, and a Rage Burst turns defense into a reset.
- **Readable damage.** Unscaled Light-chain enders do 8–12% of normalized HP. An optimal meterless combo does 20–28%, and a full-meter route with Ultimate does 40–48%. No route may exceed 50% in PvP.

### 12.2 Anti-infinite rules (all enforced by the simulation)

1. **Juggle budget (engine):** 8 JP per combo (§7.4).
2. **Gravity scaling (engine):** +4% per juggle hit, capped at ×1.8.
3. **Hitstun decay (engine):** hit 8 loses 1 frame, hit 10 loses 2, and so on (minimum 6 frames).
4. **Damage scaling (engine):** the combo starter deals 100%; hit *n* ≥ 2 deals the per-hit factor (100% for hit 2, then −10% per hit) × the starter's proration (e.g. Katana lights 80%), floor **30%**. Cinematic paired hits flagged `unscaled` ignore scaling. **(proposal)** Ultimates and Executions floor 50%. Shadow echoes use their parent's scaling.
5. **One wall bounce and one ground bounce per combo.**
6. **Staleness (proposal):** repeated move IDs cost double JP.
7. **OTG once per combo (proposal).**
8. **Stun Ceiling (proposal):** once a combo reaches the continuous hitstun limit, the victim automatically flips out:

| Context | Stun Ceiling |
|---|---|
| PvP | 300 frames (5 s) |
| PvE, player combos on enemies | 420 frames (7 s) |
| PvE, enemy combos on Rhen | **180 frames (3 s) or 6 hits**, whichever comes first |

**Design review gate:** any new route found in QA that loops back to its starting state with the
same or more meter and position is a bug. It is fixed in data (JP cost, scaling or cancel
window). The only universal escape is the Rage Burst (§9.2), which costs a full Rage meter.

---

## 13. Readability rules

### 13.1 Telegraph colors (default palette)

| Telegraph | Color | Hex | Glyph (always shown with the color) | Audio stinger | Timing |
|---|---|---|---|---|---|
| **Unblockable** | Crimson | `#E0243A` | Broken ring around the attacker's weapon hand | Rising brass stab | At least **12 frames** before the first active frame; in PvE, at least 20 frames |
| **Low** | Amber | `#F5A623` | Downward chevron at the attacker's feet + a ground streak | Low metallic scrape | PvE: at startup frame 1. PvP: weapon-trail tint on active frames only (learning aid) |
| **Overhead** | Azure | `#3FA7F5` | Downward-pointing arc above the attacker's head | High whistle | Same as Low |
| **Grab** (throws + command grabs) | Violet | `#9B59D0` | Open-hand sigil | Cloth snap + reversed breath | Command grabs: at startup frame 1 in all modes. Universal throws: no telegraph |
| **Guard Crush** | Oath gold | `#E8C15A` | Cracked shield | Deep gong swell | From full charge onward |

**Color-blind presets** (Deuteranopia, Protanopia, Tritanopia) and a **High Contrast** preset remap
the five hues. Glyphs and stingers never change, so color is never the only signal. An optional
**Telegraph Haptics** setting adds the patterns in §11.2.

### 13.2 Readability rules for designers

1. **Silhouette first.** Every attack must be readable from the attacker's silhouette alone at 25% screen height. VFX supports the silhouette and never replaces it.
2. **Startup glint.** In PvE, every enemy attack starts with a weapon glint: white for parryable, crimson for unblockable.
3. **Attack tokens.** In group fights, only as many enemies as the difficulty's token count may be in startup or active frames against Rhen at once (00-Overview §6.1). The others circle, feint or reposition.
4. **Off-screen warnings.** Any attack from outside the camera frame shows an edge arrow in its telegraph color at least 20 frames before it connects.
5. **VFX hierarchy.** Enemy VFX render at 70% alpha while Rhen is in hitstun, and Rhen always carries a thin rim light. Sable's echoes are violet and are never red, amber, azure or gold.
6. **No telegraph inflation.** A move may carry only one telegraph type. A low unblockable is shown as unblockable (it must be dodged anyway).
7. **Hazards** are unblockable. They use a crimson floor border that is visible before the hazard is dangerous (08-Enemies §9).

---

## 14. PvE vs PvP rule differences

| Rule | PvE (Story, Survival, Boss Rush, Endless, Descent, Raids, Arena, Time Trials) | PvP (Ranked, Casual, Clan Wars) |
|---|---|---|
| Stats | Gear, attributes, skills, talents, mastery (+5% max), set bonuses (engine `rpgStats: true`) | **Normalized** (engine `rpgStats: false`): 1,000 HP, 1,000 posture, move-data damage, no gear/attribute/talent/skill/mastery effects |
| Move availability | Mastery unlocks (or Veteran Start) | **Full Kit:** every branch and property unlocked for every class |
| Shadow Time | 35% speed for 90 frames (bosses: 75 frames); **(proposal)** also slows enemies within 6 m | 50% speed for 24 frames, attacker only |
| Executions | Kill regular enemies; elites 40% / lethal ≤ 15%; bosses scripted | 25% damage; lethal at ≤ 15% and staggered |
| Critical hits | Yes (Finesse; 07-Progression §3) | None |
| Elemental status effects | Full (07-Progression §6) | None; elemental VFX are cosmetic |
| Chip KO | From Oathsundered difficulty up | Only from Ultimates |
| Stun Ceiling | 420 frames (player) / 180 frames or 6 hits (enemies) | 300 frames |
| Adaptive Tempering | Optional (never on leaderboards) | Never |
| Guard Assist | Pilgrim only | Never |
| Game speed assist | 80% option on Pilgrim | Never |
| Round rules | Encounter-based, no timer (engine `rules.story` / `rules.boss`). HP restores fully between encounters in story missions | **(engine `rules.ranked`)** first to 2 rounds, 99 s timer, Ultimate meter carries over. On timeout, the higher HP % wins. **(engine)** An exact tie (or double KO) is a drawn round that awards no win; if no one has won after 5 rounds the match is a draw. **(proposal)** Ranked replaces a drawn final round with a 30 s Sudden Death (first clean hit) |
| Pause | Yes | No (Casual lobbies can agree to a 60 s pause) |
| Simplified +2 frames | Yes | Yes |
| Netcode | Offline / host-authoritative for co-op | Rollback netcode, 1–3 frames adaptive input delay, 8-frame max rollback, desync check every 60 frames |

---

## 15. Accessibility summary (combat)

| Feature | Detail |
|---|---|
| Control schemes | Classic and Simplified, both ranked-legal |
| Full remapping | Every logical button on every device; touch layout editor |
| Hold/toggle options | `Guard` hold or toggle (PvE only); Charged attack auto-release at full charge |
| Telegraph accessibility | Color-blind presets, glyphs, audio stingers, telegraph haptics |
| Visual comfort | Camera shake 0–100%, flash reduction (caps full-screen flashes at 3 per second), Shadow Time desaturation 0–100% |
| Pace | Pilgrim game speed 80%, Guard Assist, extended Perfect Dodge tutorial |
| Audio | Subtitles for all combat VO with speaker names; directional visual indicators for off-screen audio |
| Motor | Motion leniency slider (12–18 frames) in PvE; ranked uses the defaults in §3.1 |

---

## 16. Designer checklist for a new move

Every move entry in data needs these design fields. The JSON schema itself is owned by engineering.

1. **ID** (`<weapon-short>.<move>`, lowercase, stable forever once shipped) and display name.
2. **Inputs:** Classic and Simplified, plus chain/branch parent node.
3. **Height:** high, mid, low, overhead, throw or unblockable. **Telegraph** type.
4. **Damage, posture value, chip %, JP cost.**
5. **Frame targets:** startup, active, recovery, on-block, on-hit, on-CH.
6. **Properties:** launcher, wall bounce, ground bounce, OTG, crumple, knockdown (soft/hard), armor (N-hit), invulnerability frames, projectile, guard crush, cross-up, *whiffs on crouch*, pull-in, CH-crumple, CH-launch.
7. **Cancels:** chain, branch, Special, jump, Rage/Shadow.
8. **Meter gain** overrides (if any).
9. **Mastery unlock level** (06-Weapons) and PvP Full Kit status.
10. **Feel:** hitstop class, camera event, haptic pattern, SFX set (`SFX_<Class>_<Move>_*`), VFX (`VFX_Hit_<Class>_<Weight>`), animation clip (`A_<Class>_<Move>`).

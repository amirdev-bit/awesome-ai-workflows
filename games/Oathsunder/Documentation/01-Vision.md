# Phase 1 — Vision Document

> **OATHSUNDER** · *Every oath has a shadow.*
> Canon: [`00-Canon.md`](00-Canon.md) · Next: [Game Design Document](02-GameDesign/00-Overview.md)

---

## 1. High concept

A **2.5D cinematic action fighting RPG** for mobile and PC. You play **Rhen**, an Oathwarden executed for
refusing to tithe a child's shadow, who returns from the Undermourn *Oathsundered*: bound by no oath, able to
wield both **Ember** (life) and **Umbra** (shadow). Across twelve regions of the dying empire of **Varanth**,
Rhen duels the twelve **Oathlords** who hold the seals of the Twelvefold Oath. At each seal you decide whether
to **Restore** it or **Sunder** it, and that choice reshapes the region, your powers and the ending.

The combat is a real fighting game — frame data, cancels, parries, throws, juggles — made accessible through
a Simplified control scheme and wrapped in an RPG with deep build variety and **no pay-to-win**.

## 2. Experience pillars

| # | Pillar | What it means | What we refuse |
|---|---|---|---|
| 1 | **Every frame is honest** | 60 Hz deterministic simulation, zero added input latency, readable startup, exact hitstop. Losing is always the player's own read. | Random damage in PvP, hidden input delay, animation-driven hitboxes that disagree with what you see. |
| 2 | **Weight you can feel** | Hitstop, directional knockback, camera impulse, haptics and layered audio on every contact. A katana draw and a war-hammer slam must feel physically different. | Floaty "damage number" combat. |
| 3 | **Mastery over grind** | Defensive skill (parry, perfect dodge, throw tech, guard reading) beats stats. Ranked normalises stats completely. | Power sold for money; walls that only levels can break. |
| 4 | **A world that answers back** | Restore/Sunder choices visibly change arenas, enemies, music and endings. Bosses adapt to how *you* fight. | Static, scripted boss loops. |
| 5 | **Premium on every screen** | Cinematic 2.5D lighting, a minimalist 120 FPS UI, touch-first controls that don't feel like a compromise on PC. | Cluttered mobile-game HUDs, ads, energy timers. |

## 3. Player fantasy

*"I am a disgraced knight with a sword and my own rebellious shadow at my side. I read my opponent, deflect
their strongest blow at the last possible frame, and answer with a combo I invented myself."*

Three fantasies, one character:

- **The Duelist** — spacing, parries, punishes. Served by the combat core (Phase 5) and ranked PvP (Phase 14).
- **The Oathbreaker** — moral weight, world change, four endings. Served by narrative systems (Phase 2).
- **The Collector-Smith** — 13 weapon classes, runes, legendary sets, cosmetics. Served by RPG systems (Phase 10).

## 4. Audience and platforms

| Segment | Profile | What they need |
|---|---|---|
| Core mobile action players (primary) | 16–34, play daily in 3–5 minute sessions, grew up on mobile fighting RPGs | One-thumb-friendly Simplified controls, short missions, offline story |
| Fighting-game players (secondary) | Care about frame data, netcode, ranked integrity | Classic motion inputs, rollback netcode, training mode with hitboxes and frame data |
| Dark-fantasy RPG players (tertiary) | Story, builds, collectibles | Branching narrative, deep gear/rune systems, lore |

| Platform | Target | Notes |
|---|---|---|
| Android low-end | 60 FPS stable | Simplified shaders, 30 % render scale fallback, baked GI |
| Android mid / iOS | 90 FPS | Dynamic resolution, full VFX |
| Flagship mobile | 120 FPS | Full pipeline minus SSR |
| PC Ultra / Steam Deck | Up to 240 FPS | Full HDR pipeline, SSR, volumetrics |

The simulation always ticks at 60 Hz; rendering is decoupled and interpolated, so every platform plays the
same game frame for frame (a hard requirement for cross-play).

## 5. Unique selling points

1. **A real fighting-game core in an RPG.** Frame data, cancel windows, parries, throw techs and rollback
   netcode — with a Simplified scheme that costs only 2 frames of startup on specials, so touch players can
   compete in ranked.
2. **Rhen's shadow is a partner, not a skin.** *Umbral Shadow* mode makes Sable repeat every hit as a delayed
   echo; perfect dodges trigger *Shadow Time*; Sable is also a voiced companion, a mirror boss and an ending.
3. **Restore or Sunder.** Twelve binary choices with visible consequences, combining into four endings.
4. **Bosses that learn.** Boss AI builds a profile of your habits (roll spam, parry mashing, predictable
   wake-ups) and punishes them within the fight — always within published fairness caps.
5. **Honest monetisation.** Premium story, cosmetic-only store, stat-normalised ranked.

## 6. Art direction

**"Ink and Ember."** Dark, painterly 2.5D: characters and props are fully 3D, lit in real time on a
side-on combat plane with deep parallax layers behind and foreground silhouettes in front.

| Element | Direction |
|---|---|
| Palette | Desaturated charcoal and bone base; each region owns one saturated accent (Emberfall: ember orange; Weeping Reeds: drowned teal; Lanternhold: paper-lantern red; Verdant Rot: bioluminescent green). Ember effects are warm (2,000–3,000 K); Umbra effects are cold violet-black with inverted rim light. |
| Silhouettes | Readable at 2 cm tall on a phone: every weapon class has a unique silhouette; bosses are 1.3–3× player height. |
| Lighting | One key light per arena that motivates mood (dusk sun, lantern clusters, storm lightning), real-time shadows on fighters, baked GI on sets, volumetric fog layers for depth. |
| Motion | Fast anticipation, violent contact, long recovery holds — poses must read at 60 Hz with hitstop. |
| VFX | Weapon trails coloured by element; contact sparks scale with hit weight; parries flash white-gold; perfect dodges invert colour for 6 frames. |
| UI | Minimal brush-stroke frames, no permanent clutter: health/posture/meters sit at the top edge; everything else is contextual. |
| Rating | Stylised violence: sprays of ink-like blood (can be disabled), no dismemberment on mobile default. |

## 7. Audio direction (summary — Phase 13 has the full spec)

Adaptive, layered score built from Varanth's instruments: bowed metal, taiko-like drums, throat choir,
hurdy-gurdy drones. Each boss has a leitmotif that fractures across its three phases. Every weapon class has
its own swing, impact and parry sound families; hit sounds scale with damage and counter hits add a pitched
"ring" so skilled play *sounds* skilled.

## 8. Narrative hook

Five hundred years ago Emperor Aurem sealed the Undermourn with the Twelvefold Oath. The price was hidden: at
death every shadow is tithed to the seal and never reborn. Now the Oath is failing and the Unsworn are rising.
Rhen, freed from the Oath by his own execution, is the only person in Varanth who can decide whether it should
be saved.

## 9. Business model

- **Premium campaign** (mobile: free prologue region + one-time unlock; PC: full price).
- **Cosmetic store** (weapon skins, outfits, trails, finisher variants) — never stats.
- **Season pass** with a meaningful free track; premium track is cosmetic only.
- **No loot boxes purchased with money, no energy timers, no ads.**
- Ranked PvP always uses normalised stats (`rules.ranked` → `rpgStats: false`, enforced by an automated test).

## 10. Success metrics

| Metric | Target |
|---|---|
| Median input-to-first-active-frame latency (60 Hz sim, 120 Hz display) | ≤ 1 tick + 1 display frame |
| Crash-free sessions | ≥ 99.8 % |
| D1 / D7 / D30 retention (mobile) | 45 % / 20 % / 9 % |
| Story completion | ≥ 35 % of players who finish region 1 |
| Ranked desync rate | < 1 per 10,000 matches (checksummed) |
| Store rating | ≥ 4.6 |

## 11. Scope and production plan

The project is built in the 16 phases defined by the brief. Engineering systems are implemented and tested
in code; content that requires human craft (2,000+ animation clips, character models, VO, music) is specified
precisely so production teams can build to spec.

| Milestone | Phases | Exit criteria |
|---|---|---|
| **M0 — Foundation** | 1–5 | Vision, GDD, architecture, project structure, deterministic combat core with tests and benchmarks |
| M1 — Playable duel | 6–7 | Rhen vs mirror on device, touch + pad + keyboard, frame-locked animation |
| M2 — Vertical slice | 8–11 | Emberfall region: enemies, AI, Katana/Chain Sword, Abbot Kessh boss, RPG loop |
| M3 — Alpha | 12–14 | All UI/UX, audio systems, rollback PvP |
| M4 — Beta | 15 | Performance targets met on the device matrix, content complete |
| M5 — Release candidate | 16 | Cert-ready builds, live-ops tooling |

Estimated team at full production: 55–70 people (engineering 14, design 9, art/animation 24, audio 4,
narrative 3, QA 8, production/live-ops 6).

## 12. Risks and mitigations

| Risk | Impact | Mitigation |
|---|---|---|
| Touch controls feel worse than pad | Mobile retention | Simplified scheme designed first; buffer on local frames; ranked parity via startup cost, not damage |
| Rollback desyncs | Ranked integrity | Fixed-point, allocation-free simulation; mirror, fuzz and sync tests in CI from day one |
| Animation volume (2,000+ clips) | Schedule | Shared humanoid rig, weapon-class retargeting, data-driven frame windows independent of clip length |
| Low-end Android thermal throttling | Performance | 60 Hz sim costs < 0.1 ms; render budget scaled per device tier |
| Moral choice fatigue | Narrative | Only 12 major choices; small choices are companion-voiced reactions, not menus |

## 13. Phase 1 deliverables

| Deliverable | Location |
|---|---|
| Canon (names, IDs, terminology) | [`00-Canon.md`](00-Canon.md) |
| Vision | this document |
| Visual target references | §6 above; mood boards are produced by art in Phase 7 per region |
| Dependencies for Phase 2 | Canon + pillars + audience + monetisation rules |

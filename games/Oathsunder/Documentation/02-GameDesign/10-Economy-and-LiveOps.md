# OATHSUNDER — Economy & LiveOps

> **Document:** GDD 02-10 · **Owner:** Lead Systems Design · **Status:** Phase 2 design baseline
> **Canon:** "Premium campaign + optional cosmetic store. **No pay-to-win.** Ranked PvP uses
> normalized stats." (`00-Canon.md` §1). Every rule below follows from that line.

---

## 1. Business model

| Product | Platforms | Price (USD reference) | Contents |
|---|---|---|---|
| **OATHSUNDER** (standard) | Windows, macOS, Steam Deck | $29.99 | Full campaign, all modes, all 13 weapon classes |
| **OATHSUNDER — Oathsundered Edition** | Windows, macOS, Steam Deck | $39.99 | Standard + 3 outfits, 2 weapon skins, digital artbook, soundtrack (cosmetic only) |
| **OATHSUNDER** (mobile download) | Android, iOS | Free | Emberfall Monastery (region 1: the Prologue and missions 4–10), Training (Adekan's Dojo), Casual PvP, Ranked PvP, Daily Challenges, Time Trials for Emberfall |
| **Campaign unlock** (mobile, one-time) | Android, iOS | $19.99 | Regions 2–12, Survival, Arena, Boss Rush, Endless, Undermourn Descent, Raids, all Time Trials, Weekly Events. No other purchase is ever needed to play any content |
| **Cosmetic store** | All | Lumens (§2) or direct price | Cosmetics only |

- **Cross-progression:** one OATHSUNDER account carries saves, cosmetics, Lumens, and pass ownership everywhere. Campaign ownership is honored on every platform family where it was bought, as store policies allow.
- **No ads**, ever, on any platform.
- Regional pricing follows platform price tiers, and the in-store price always shows the local amount.

---

## 2. Currencies

| Currency | ID | Type | How it is obtained | What it buys | Wallet cap | Convertible? |
|---|---|---|---|---|---|---|
| **Marks** | `currency.marks` | Earned (soft) | Missions, drops, salvage, challenges, Vows, event token conversion | Upgrades, crafting, enchanting, rune fusion, reforging, re-tempering, dyes, clan founding, Neve's Bazaar | 99,999,999 | No |
| **Glory** | `currency.glory` | Earned (PvP) | Ranked, Casual, Free Blade Tournaments | PvP cosmetics in Oskar's Glory Hall | 50,000 | No |
| **Shades** | `currency.shades` | Earned (Descent) | Undermourn Descent | Tithe Ledger unlocks and Descent cosmetics | 99,999 | No |
| **Banner Seals** | `currency.bannerseals` | Earned (clan) | Clan Wars | Clan cosmetics and camp banners | 99,999 | No |
| **Festival Tokens** | `currency.festivaltokens` | Earned (event) | Weekly Events | That event's cosmetic store | Event-scoped | Auto-convert to Marks at event end (1 : 50) |
| **Lumens** | `currency.lumens` | **Premium** (bought) *and* earnable in small amounts | Real money; Season Pass free track (300 per season); 20 milestone achievements (50 each, 1,000 total, one-time) | **Cosmetics and the premium Season Pass only** | 50,000 | **Never** into any earned currency, material or item with stats |

Tithe Coins exist only inside a Descent run and never reach the wallet. Materials (07-Progression §11)
are items, not currencies, and **are never sold**.

---

## 3. Sources and sinks

### 3.1 Table

| Currency | Sources (rates) | Sinks (costs) |
|---|---|---|
| **Marks** | Story mission `100 + 25 × level` (+20% per Vow met) · side mission 70% of that · elite 30–300 by level · salvage (Common 20 → Oathbound 2,000) · Daily Challenge `500 + 20 × level` · Survival `200 + 10 × wave × region tier` · Time Trial first Gold 1,000 · Renown level 2,000 · event token conversion | Upgrades +0→+10 (weapon 17,325 at iL 5 → 80,850 at iL 60; armor 60%) · craft Rare 2,000 / Epic 8,000 · enchant 2,000 / 6,000 / 15,000 · rune fusion 1,000 / 5,000 · reforge `150 × ΔiL × rarity` · re-temper 1,500 doubling to 24,000 · dyes 2,000–10,000 · clan founding 5,000 · Neve's Bazaar earned-tier cosmetics 5,000–25,000 |
| **Glory** | Ranked win 30 / loss 10, first win of the day +10 · Casual half of that · Tournament 50–500 | Glory Hall: PvP banners 600, emotes 900, trails 1,500, seasonal weapon skins 3,000, outfits 6,000 |
| **Shades** | 10 + 2 × floor per floor · guardian 50 · Floor 25: 300 · ×(curse + Vow multipliers) | Tithe Ledger (about 14,000 total for every unlock) · Descent cosmetics 1,000–2,000 |
| **Banner Seals** | 10 per Banner Duel, 20 per win, weekly placement 200–1,000 | Clan banners 800, clan trail tints 1,500, camp decorations 400–2,500 |
| **Festival Tokens** | Event contracts (100–400 each) · event daily bonus 100 | Event store items 300–2,000; leftovers → Marks |
| **Lumens** | Purchase · pass free track 300/season · achievements 1,000 one-time | Outfits 1,500 · weapon skins 800 · trails 400 · emotes 300 · execution variants 1,000 · finisher variants 1,200 · Sable appearances 1,000 · HUD frames 300 · clan banners 400 · premium pass 1,000 |

### 3.2 Marks flow model (per hour of typical play)

| Level band | Faucet (Marks/h) | Main sinks at this band | Target sink / source ratio |
|---|---|---|---|
| 1–15 | about 3,000 | +1…+6 upgrades, first enchantments | 0.9 |
| 16–35 | about 7,000 | +7…+9 upgrades, rune fusion, crafting Epics | 0.95 |
| 36–60 | about 11,000 | +10 upgrades, reforging, Tier III enchantments | 1.0 |
| Endgame (60, Renown) | about 13,000 | Reforging, re-tempering, dyes, Bazaar, alternate builds | 0.85–1.0 |

**Inflation guard:** if the median wallet of level-60 players goes above 1,500,000 Marks for 4 consecutive
weeks, add Bazaar catalog items (cosmetic Marks sinks). Upgrade costs are never raised after launch.
**Story-driven prices:** region state and the Oath balance (02-World-and-Narrative §4, §6.2) may move
Marks prices at Tessen's forge and Neve's Bazaar by up to ±10%. **Lumens prices never change with story
choices.**

---

## 4. Cosmetic store rules (Neve's Lantern Stall)

1. **Cosmetics only.** Everything sold changes appearance and nothing else (07-Progression §14 readability rules apply to every item). In the story the stall opens when Neve joins (`mission.weepingreeds.02`). The main-menu Store tab opens at the same point or at account level 10, whichever comes first.
2. **Direct purchase.** Every item has a fixed, visible price. No loot boxes, gacha, mystery packs, "spin" wheels or random bundles, whether bought with money or Lumens.
3. **Price transparency.** Every Lumens price also shows the local-currency equivalent ("1,500 Lumens ≈ $14.99"). Items can also be bought directly in local currency without Lumens.
4. **No leftover traps.** Lumens packs are 500 / 1,000 / 2,000 / 5,000 at a flat 100 Lumens ≈ $1 (no bonus tiers), and every price is a multiple of 100. If a player is short, the store offers exactly the missing amount, rounded up to 100.
5. **The permanent catalog shows everything.** The "Featured" shelf only highlights. Event-exclusive cosmetics return to the catalog or Archive within **12 months**, and the return window is stated on the item.
6. **No countdown pressure.** Availability shows as a plain date. There are no animated timers, "only X left" messages or pop-up offers during gameplay.
7. **Bundles** show the sum of the individual prices and the discount, never contain currency, and are **completion-priced** (items already owned are subtracted).
8. **Try before you buy.** Every cosmetic can be previewed in Training on any class, including Execution and finisher variants.
9. **Undo.** A purchase can be refunded within 48 h if the item has not been used in a match or mission.
10. **Gifting** is available only between adult accounts that have been friends for 30+ days.
11. **Personalized pricing and spend-based targeting are forbidden.** Every player sees the same prices and offers.

---

## 5. Season Pass — The Pilgrim's Road

| Rule | Value |
|---|---|
| Length | 10 weeks (aligned with the ranked season) |
| Tiers | 50; 10,000 Pass XP per tier (500,000 total) |
| Premium price | 1,000 Lumens (≈ $9.99). No tier-skip purchases exist |
| Expected completion | About 2.5 hours of play per week, from any mix of modes |
| Catch-up | From week 7, Pass XP is ×1.5 for everyone |
| Expiry | **Passes never expire.** When a season ends, the pass moves to the **Archive**. Owned premium passes and every free track can still be progressed there, one active pass at a time |
| Content rules | Cosmetics, Lumens and titles only. The **premium track never contains Marks, materials, XP boosts or anything with stats**. The free track may contain Marks, because they are earned by playing |

**Pass XP sources:** story mission 3,000 · side mission 2,000 · PvP match 700 (win 900) · Descent floor
600 · Survival 5-wave block 800 · raid clear 4,000 · weekly event contract 2,500 · Daily Challenge 1,000 each
(+2,000 for all 3).

**Track contents (per season)**

| Track | Contents |
|---|---|
| **Free** (meaningful by design) | 1 full outfit (tier 25), the season's signature weapon skin for one moveset (tier 50), 2 trails, 2 emotes, 1 banner, 1 HUD frame, **300 Lumens**, 3 titles, 20,000 Marks spread across tiers. About 40% of all pass items |
| **Premium** | 3 outfits, 4 weapon skins, 1 Execution variant, 1 finisher variant, 1 Sable appearance, 3 trails, 3 emotes, 2 banners, **1,000 Lumens** (enough for the next pass) |

---

## 6. LiveOps

### 6.1 Season event calendar template (10 weeks)

| Week | Weekly Event (09-GameModes §14) | Ranked / Clan | Store & pass | Other |
|---|---|---|---|---|
| 1 | Class Spotlight (the season's featured class) | Season start, placements | Pass launch; season catalog drop | Balance data patch |
| 2 | Lantern Festival Contracts | — | Festival cosmetics | New Arena Champion Ladder |
| 3 | Oathlord Echoes | Clan season week 1 | — | New Descent Relic Codex entries |
| 4 | Free Blade Tournament | — | Tournament banners in the Glory Hall | Community goal check-in |
| 5 | Mirror Week | Mid-season ranked stats post | Mid-season catalog refresh | Developer livestream |
| 6 | Class Spotlight (second class) | — | — | Balance data patch (if needed) |
| 7 | Lantern Festival Contracts II | — | — | **Pass catch-up (×1.5) begins** |
| 8 | Oathlord Echoes (remix) | Clan season week 6 | — | Seeded Descent "Grand Vow" weekend |
| 9 | Free Blade Tournament Finals | Twelvefold race | — | — |
| 10 | Season Finale: Sable's Night (double Shadow meter in PvE) | Ranked final push; Clan Siege of the Season | Archive opens for the ending pass | Season recap cards; patch notes for the next season |

**Annual tentpoles:** *Night of Bells* (anniversary, Emberfall), *Lantern Festival* (mid-year,
Lanternhold), *The Long Night* (winter solstice, Rimewood), *Day of the Unreborn* (autumn,
remembrance of the tithed). Each lasts 2 weeks and brings free story vignettes and event cosmetics, and all
of its cosmetics return within 12 months.

### 6.2 Content and patch cadence

| Cadence | Content |
|---|---|
| Every 2 weeks (as needed) | Balance data patch: move JSON only, with move IDs unchanged and old and new frame data published |
| Every season (10 weeks) | Pass, events, Arena ladders, Descent relic entries, cosmetics, 1 Oathlord Echo remix |
| Twice a year | Major update: new Descent stratum modifiers, a raid difficulty tier, a new Arena format. **New bosses, regions or named characters require a canon update first** (`00-Canon.md` header rule) |
| Hotfix | Critical issues within 48 h. Ranked exploits: the affected content is disabled within 4 h |

---

## 7. Ethical monetization rules

| # | Rule |
|---|---|
| 1 | **No loot boxes with paid currency.** No paid randomness of any kind. The only random cosmetic reward (Renown's Reliquary Cache) is earned, never sold, has published odds and duplicate protection |
| 2 | **No pay-to-win.** The store sells cosmetics only. Lumens never convert into power or earned currencies (07-Progression §15) |
| 3 | **Price transparency.** Local-currency equivalents everywhere, no bonus-tier obfuscation, completion-priced bundles |
| 4 | **Minors' protections.** Age gate at account creation (platform age signals are respected). **Under 13:** no purchases, no chat, no gifting. **13–17:** purchases off by default until a parent or guardian sets a monthly limit through the platform or account family controls; no gifting; text chat filtered, voice off; no push notifications by default |
| 5 | **Spending limits for everyone.** Default adult monthly cap: $100. Adjustable from $0 to $500. Increases take effect after a **72-hour cooling-off period**, decreases immediately. Hard cap: $500 per month |
| 6 | **Spending dashboard.** Lifetime, 30-day and per-season spending shown in account settings, with a one-tap "set a limit" |
| 7 | **No dark patterns.** No confirm-shaming, no pre-checked boxes, no disguised ads, no fake scarcity, no purchase prompts during gameplay or right after a loss. Purchases take 2 taps and show the real-money price |
| 8 | **No FOMO on gameplay.** Every gameplay-affecting unlock is permanent and earnable. Event cosmetics return within 12 months, and event tokens convert to Marks |
| 9 | **No spend-based targeting.** No personalized prices or offers, and no "whale" segmentation in marketing |
| 10 | **Data respect.** No sale of personal data. Telemetry is used for balance, stability and fraud prevention only, and players can see and delete their data |
| 11 | **Compliance.** PEGI and ESRB "In-Game Purchases" labeling, the Belgian and Dutch loot box positions (not applicable, since there is no paid randomness), platform store policies, and consumer-protection refund rules |

---

## 8. KPIs to watch

### 8.1 Player experience and retention

| KPI | Target (mobile / PC) | Alert |
|---|---|---|
| FTUE completion (reaches the 29:00 beat, 00-Overview §7) | ≥ 80% / ≥ 90% | Any single beat losing > 5% of players |
| D1 / D7 / D30 retention | 45 / 20 / 10% · 60 / 32 / 18% | −3 points week over week |
| Sessions per day (DAU) | 2.5 / 1.3 | — |
| Average session length | 11 min / 65 min | Mobile > 25 min (session design is failing) |
| Median Oathlord attempts on Oathwarden | 2–4 | > 6 for any Oathlord |
| Tempering state distribution | about 60% Neutral | > 30% at *T* = −3 in any region |
| Difficulty distribution | Pilgrim ≤ 25%, Oathwarden ~55% | — |
| Simplified scheme share / Classic–Simplified win-rate gap | Tracked / < 3 points | Gap ≥ 3 points in any tier (01-Combat §3.3) |

### 8.2 Competitive health

| KPI | Target | Alert |
|---|---|---|
| Ranked queue time (median) | < 30 s at peak, < 90 s off-peak | > 120 s |
| Match quality (predicted win probability 45–55%) | ≥ 80% of matches | < 70% |
| Disconnect rate | < 2% | > 3% |
| Average rollback frames | < 2 | > 4 |
| Class win-rate spread / pick rates | 06-Weapons §18 | Any breach |
| Reported-match re-simulation desync rate | 0 | Any |

### 8.3 Business and economy health

| KPI | Target | Guardrail |
|---|---|---|
| Mobile campaign conversion (of D30 players) | 12% | — |
| Store payer rate (MAU) | 4–6% | Not a target to maximize at the expense of the rules in §7 |
| Premium pass attach rate (MAU) | 8% | — |
| Refund rate | < 3% | > 5% triggers a store review |
| Payers hitting the monthly spending cap | < 0.5% of payers | Above that, review offers and the defaults |
| Spend flagged from 13–17 accounts outside parental limits | 0 | Any occurrence is a P1 incident |
| Marks sink/source ratio at endgame | 0.85–1.0 | < 0.7 for 4 weeks → add sinks (§3.2) |
| Free-track pass completion | ≥ 50% of pass participants | < 35% → lower XP per tier for the next season |
| Cosmetic earnable share | ≥ 65% (07-Progression §14) | Any season below 65% is blocked before release |

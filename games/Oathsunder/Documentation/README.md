# OATHSUNDER — Documentation

Production documentation, one document per phase. Everything here must agree with the **[Canon](00-Canon.md)**.

| Phase | Document | Status |
|---|---|---|
| — | [00 · Canon](00-Canon.md) — names, IDs, regions, bosses, weapons, combat rules, naming conventions | ✅ |
| 1 | [01 · Vision](01-Vision.md) | ✅ |
| 2 | [02 · Game Design Document](02-GameDesign/00-Overview.md) — [overview](02-GameDesign/00-Overview.md), [combat](02-GameDesign/01-Combat.md), [world & narrative](02-GameDesign/02-World-and-Narrative.md), [missions](02-GameDesign/03-Missions.md), [characters](02-GameDesign/04-Characters.md), [bosses](02-GameDesign/05-Bosses.md), [weapons](02-GameDesign/06-Weapons.md), [progression](02-GameDesign/07-Progression.md), [enemies](02-GameDesign/08-Enemies.md), [game modes](02-GameDesign/09-GameModes.md), [economy & live-ops](02-GameDesign/10-Economy-and-LiveOps.md) | ✅ |
| 3 | [03 · Technical Architecture](03-TechnicalArchitecture.md) | ✅ |
| 4 | [04 · Project Structure](04-ProjectStructure.md) | ✅ |
| 5 | [05 · Core Combat Framework](05-CoreCombat.md) + [generated frame data, animation spec, cue list](05-CoreCombat/Generated-FrameData.md) | ✅ implemented, 144 tests |
| 6 | Player Controller — input devices, touch layout, remapping, camera | next |
| 7 | Animation System | planned |
| 8 | Enemy AI | planned |
| 9 | Weapons (12 remaining classes) | planned |
| 10 | RPG Systems | planned |
| 11 | Boss Framework | planned |
| 12 | UI / UX | planned |
| 13 | Audio Systems | planned |
| 14 | Multiplayer (rollback netcode) | planned |
| 15 | Optimization | planned |
| 16 | Release Candidate | planned |

Generated files (`05-CoreCombat/Generated-*.md`) are produced by `Tools/DotNet/Oathsunder.Tools.FrameData` from
the shipped JSON and must not be edited by hand.

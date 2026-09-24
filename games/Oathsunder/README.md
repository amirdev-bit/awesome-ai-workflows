# OATHSUNDER

*Every oath has a shadow.* An original 2.5D cinematic dark-fantasy action fighting RPG for mobile and PC,
built on Unity 6 LTS (URP).

- **Design and production docs:** [`Documentation/`](Documentation/README.md) — start with the
  [Canon](Documentation/00-Canon.md) and the [Vision](Documentation/01-Vision.md).
- **Combat core:** a 60 Hz deterministic, fixed-point, allocation-free fighting simulation, ready for rollback
  netcode — see [Phase 5](Documentation/05-CoreCombat.md).

## Quick start

| I want to… | Do this |
|---|---|
| Open the game | Unity Hub → *Add* → this folder (Unity 6 LTS, 6000.0.23f1 or a later 6000.0 LTS patch) |
| Run the combat tests without Unity | `cd Tools/DotNet && dotnet test Oathsunder.Simulation.Tests` (.NET 8 SDK) |
| Run the benchmarks | `cd Tools/DotNet && dotnet run -c Release --project Oathsunder.Tools.Benchmarks` |
| Regenerate frame data docs | `cd Tools/DotNet && dotnet run -c Release --project Oathsunder.Tools.FrameData -- ../../Documentation/05-CoreCombat/Generated-FrameData.md` |
| Edit a move | Change `Assets/_Project/Content/Combat/MoveSets/*.json`, then run the tests (or *Oathsunder → Combat → Validate Content* in Unity) |

## Status

Phases 1–5 are delivered: vision, game design document, technical architecture, project structure, and the
core combat framework (implemented and tested). Phases 6–16 follow the plan in the documentation index.

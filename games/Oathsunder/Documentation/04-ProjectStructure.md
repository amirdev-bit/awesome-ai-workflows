# Phase 4 — Project Structure

> Architecture: [`03-TechnicalArchitecture.md`](03-TechnicalArchitecture.md) · Canon naming: [`00-Canon.md`](00-Canon.md) §9

---

## 1. Repository layout

The Unity project lives in `games/Oathsunder/`. Everything the game owns sits under `Assets/_Project/` so that
third-party packages and Asset Store imports can never collide with it.

```
games/Oathsunder/
├── Assets/
│   └── _Project/
│       ├── Runtime/                     code shipped in the player
│       │   ├── Core/                    Oathsunder.Core        (engine-free)   ✅ Phase 5
│       │   ├── Combat/                  Oathsunder.Combat      (engine-free)   ✅ Phase 5
│       │   │   ├── Input/               buttons, directions, input history, motion commands
│       │   │   ├── Definitions/         move/fighter/stage/rules/tuning data model
│       │   │   ├── Content/             JSON parser, blueprint builder, content set, frame data
│       │   │   ├── Simulation/          world, state, controller, physics, hits, projectiles, rounds
│       │   │   └── Events/              combat events and buffer
│       │   ├── Gameplay/                Oathsunder.Gameplay    (Unity bridge)  ✅ Phases 5–6
│       │   │   ├── Combat/              runner, presenter, feedback, cues, debug drawer, configs
│       │   │   ├── Controls/            player input source, profile store, rebinder, touch overlay
│       │   │   └── Cameras/             combat camera rig
│       │   ├── Input/                   Oathsunder.Controls    (engine-free)   ✅ Phase 6
│       │   ├── Animation/               Oathsunder.Animation                    Phase 7
│       │   ├── AI/                      Oathsunder.AI          (engine-free)    Phase 8
│       │   ├── RPG/                     Oathsunder.RPG         (engine-free)    Phase 10
│       │   ├── Bosses/                  Oathsunder.Bosses                       Phase 11
│       │   ├── UI/                      Oathsunder.UI                           Phase 12
│       │   ├── Audio/                   Oathsunder.Audio                        Phase 13
│       │   ├── Networking/              Oathsunder.Netcode     (engine-free)    Phase 14
│       │   └── Systems/                 Oathsunder.Systems                      Phases 10–16
│       ├── Editor/                      Oathsunder.Editor                       ✅ Phase 5
│       │   └── Combat/                  content validator, import hook, Frame Data window
│       ├── Content/                     designer data (JSON)
│       │   └── Combat/                  ✅ Fighters/ MoveSets/ Stages/ Rules/ Tuning/
│       ├── Art/                         meshes, materials, textures (README: budgets)
│       ├── Animation/                   clips and controllers (README: clip rules)
│       ├── Audio/                       music stems, SFX, VO, mixers
│       ├── VFX/                         combat, modes, ultimates, environment
│       ├── Shaders/                     Shader Graph + HLSL
│       ├── UI/                          UXML, USS, sprites, fonts
│       ├── Scenes/                      Boot, Frontend, Arenas, Test
│       ├── Settings/                    URP tiers, Input actions, quality
│       ├── Prefabs/
│       └── Tests/
│           ├── EditMode/                Oathsunder.Tests.EditMode               ✅ 176 tests
│           ├── PlayMode/                Oathsunder.Tests.PlayMode               ✅ runner smoke tests
│           └── Performance/                                                     Phase 15
├── Packages/manifest.json               ✅ pinned Unity 6 packages
├── ProjectSettings/ProjectVersion.txt   ✅ 6000.0.23f1 (upgrade to latest Unity 6 LTS patch)
├── Tools/DotNet/                        ✅ .NET 8 tooling (outside Unity)
│   ├── Oathsunder.sln
│   ├── Directory.Build.props            LangVersion 9, warnings as errors
│   ├── Oathsunder.Simulation/           compiles Runtime/Core + Runtime/Combat
│   ├── Oathsunder.Simulation.Tests/     runs Tests/EditMode under NUnit
│   ├── Oathsunder.Tools.Benchmarks/     performance report
│   ├── Oathsunder.Tools.FrameData/      frame data, animation spec, cue list generator
│   ├── Oathsunder.UnityCompileCheck/    C# type-check of Unity-facing code against API stubs
│   └── Shared/                          content locator for tools
├── Documentation/                       ✅ phases 1–5 (this folder)
├── .gitattributes                       ✅ LF + Unity smart merge + Git LFS for binaries
└── .gitignore                           ✅ Unity + tooling outputs
```

✅ = present in the repository now. Every planned folder already contains a README describing its conventions,
budgets and owning phase.

## 2. Assemblies

| Assembly | asmdef | Platforms | `noEngineReferences` | References |
|---|---|---|---|---|
| `Oathsunder.Core` | `Runtime/Core/Oathsunder.Core.asmdef` | all | **true** | — |
| `Oathsunder.Combat` | `Runtime/Combat/Oathsunder.Combat.asmdef` | all | **true** | Core |
| `Oathsunder.Controls` | `Runtime/Input/Oathsunder.Controls.asmdef` | all | **true** | Core, Combat |
| `Oathsunder.Gameplay` | `Runtime/Gameplay/Oathsunder.Gameplay.asmdef` | all | false | Core, Combat, Controls, Unity.InputSystem |
| `Oathsunder.Editor` | `Editor/Oathsunder.Editor.asmdef` | Editor | false | Core, Combat, Gameplay |
| `Oathsunder.Tests.EditMode` | `Tests/EditMode/Oathsunder.Tests.EditMode.asmdef` | Editor | false | Core, Combat, TestRunner, NUnit |
| `Oathsunder.Tests.PlayMode` | `Tests/PlayMode/Oathsunder.Tests.PlayMode.asmdef` | all | false | Core, Combat, Gameplay, TestRunner, NUnit |

**Dependency rule:** an assembly may only reference assemblies above it in this table. Engine-free assemblies
must keep `noEngineReferences: true`; the .NET build in CI fails if one of them touches `UnityEngine`.

## 3. File list (Phases 1–5)

### Runtime — `Oathsunder.Core`

| File | Responsibility |
|---|---|
| `Mathematics/Fixed.cs` | Q47.16 fixed point: exact decimal parsing, odd-symmetric rounding, sqrt |
| `Mathematics/FixedVector2.cs` | 2D vector on the combat plane |
| `Mathematics/FixedAabb.cs` | Boxes with facing flip, overlap, intersection |
| `Random/Pcg32.cs` | Deterministic PCG32 RNG (value type = snapshot) |
| `Hashing/StateHasher.cs` | 64-bit incremental state checksum |
| `Serialization/JsonReader.cs`, `JsonNode.cs`, `JsonExceptions.cs` | Strict RFC 8259 JSON with line/column and JSON-path errors |

### Runtime — `Oathsunder.Combat`

| File | Responsibility |
|---|---|
| `Input/InputButtons.cs`, `NumpadDirection.cs`, `InputFrame.cs` | Logical buttons, numpad directions, 15-bit packed input |
| `Input/InputHistory.cs` | 128-frame ring buffer, local-frame buffering, consumption |
| `Input/MotionCommand.cs` | 236 / 214 / 623 / 66 / 44 recognition with leniency |
| `Definitions/*.cs` | Move, attack, hitbox, trigger, cancel, paired action, projectile, fighter, stage, rules, tuning, stats, blueprint |
| `Content/CombatContentParser.cs` | JSON → definitions (SI units → per-frame fixed) |
| `Content/FighterBlueprintBuilder.cs` | Merge, resolve, validate (collects every error) |
| `Content/CombatContentSet.cs` | Content registry fed by TextAssets or files |
| `Content/FrameDataCalculator.cs` | Frame data for training mode, docs and tests |
| `Simulation/CombatWorld.cs` | Public facade: `Step`, `SaveState`, `LoadState`, `Checksum` |
| `Simulation/CombatWorldState.cs`, `FighterState.cs`, `ProjectileState.cs` | Snapshotable state |
| `Simulation/FighterController.cs` | State machine, move selection, cancels, stun states, paired actions |
| `Simulation/PhysicsSystem.cs` | Gravity, landing, walls, separation, bodies, facing |
| `Simulation/HitSystem.cs` | Contact detection and resolution, combos, juggles, echoes |
| `Simulation/ProjectileSystem.cs`, `ResourceSystem.cs`, `RoundSystem.cs` | Projectiles, meters/modes, round flow |
| `Events/CombatEvent.cs`, `CombatEventBuffer.cs` | Output events |

### Runtime — `Oathsunder.Gameplay`

| File | Responsibility |
|---|---|
| `Combat/CombatSimulationRunner.cs` | Fixed 60 Hz tick from Unity, interpolation, listeners |
| `Combat/ICombatInputSource.cs` | Input contract + neutral and recorded sources |
| `Combat/FighterPresenter.cs` | Interpolated transform, frame-locked Animator pose |
| `Combat/CombatFeedbackDirector.cs`, `CombatCueLibrary.cs` | VFX, SFX, camera shake, haptics from events |
| `Combat/CombatContentLibrary.cs`, `CombatMatchConfig.cs` | ScriptableObject content and match composition |
| `Combat/CombatDebugDrawer.cs` | Hitbox / hurtbox / pushbox gizmos |
| `Combat/FixedConversions.cs` | Simulation → Unity conversions (one way) |

### Editor — `Oathsunder.Editor`

| File | Responsibility |
|---|---|
| `Combat/CombatContentValidator.cs` | Menu + batch-mode validation, library sync, import post-processor |
| `Combat/FrameDataWindow.cs` | Frame data table and timeline window |

### Content — `Assets/_Project/Content/Combat`

| File | Contents |
|---|---|
| `Fighters/fighter.rhen.json` | Rhen's body: movement, physics, stances, boxes |
| `MoveSets/moveset.universal.json` | Dash, backstep, rolls, dodge, throws, Ember Rage, Umbral Shadow, tech roll |
| `MoveSets/moveset.katana.json` | Full Katana (Oathblade Style) move set |
| `Stages/stage.emberfall.belltower.json`, `stage.training.dojo.json` | Arena bounds |
| `Rules/rules.story.json`, `rules.ranked.json`, `rules.training.json`, `rules.boss.json` | Mode rules |
| `Tuning/tuning.combat.json` | Global mechanics constants |

## 4. Content file conventions

- File name prefix decides the type: `fighter.`, `moveset.`, `stage.`, `rules.`, `tuning.`.
- The `id` inside a file is the stable content id (canon §9.2); it is referenced by saves and replays and must
  never change once shipped. Rename the display `name` freely.
- Units: metres, metres per second, metres per second squared; frames are 1-based and inclusive
  (`"frames": [6, 8]` = active on move frames 6, 7 and 8).
- Strict JSON (no comments, no trailing commas) so every tool can read it.

## 5. Opening and running

| Task | Command / action |
|---|---|
| Open in Unity | Unity Hub → Add → `games/Oathsunder` (Unity 6 LTS). Unity generates `.meta` files on first import; commit them. |
| Run tests in Unity | Window → General → Test Runner → EditMode / PlayMode |
| Run tests without Unity | `cd games/Oathsunder/Tools/DotNet && dotnet test Oathsunder.Simulation.Tests` |
| Benchmarks | `dotnet run -c Release --project Oathsunder.Tools.Benchmarks` |
| Regenerate frame data docs | `dotnet run -c Release --project Oathsunder.Tools.FrameData -- ../../Documentation/05-CoreCombat/Generated-FrameData.md` |
| Validate content (Unity) | Menu **Oathsunder → Combat → Validate Content** |
| Frame data window | Menu **Oathsunder → Combat → Frame Data** |

## 6. Source control

- `.gitattributes` forces LF on text assets, routes Unity YAML through Smart Merge
  (`git config merge.unityyamlmerge.driver "<UnityYAMLMerge path> merge -p %O %B %A %A"`), and stores binary
  art/audio in **Git LFS**.
- Branching: trunk-based; short-lived feature branches; every PR runs the simulation CI.
- `.meta` files are always committed with their assets.

## 7. Phase 4 deliverables and next-phase dependencies

| Deliverable | Status |
|---|---|
| Folder structure with conventions | ✅ created, with a README per planned area |
| Assembly definitions and dependency rules | ✅ |
| Package manifest (URP, Input System, Cinemachine, Burst, Collections, Addressables, Animation Rigging, Timeline, VFX Graph, Test Framework) | ✅ |
| Tooling solution | ✅ |
| Source-control configuration | ✅ |

**Next dependencies:** Phase 6 adds `Runtime/Input` (Input System actions in `Settings/Input`), the combat test
scene `Scenes/Test/SC_Test_Combat`, and `PF_Fighter_Rhen`.

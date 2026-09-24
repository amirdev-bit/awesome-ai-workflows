# Scenes

> Owner phase: Phase 6 onward. Conventions follow `Documentation/00-Canon.md` §9.

```
Scenes/
  Boot/          SC_Boot (composition root, services, first load)
  Frontend/      SC_Frontend
  Arenas/<RegionId>/  SC_Arena_<Region>_<Arena> (additively loaded; gameplay bounds come from stage.*.json)
  Test/          SC_Test_Combat (two fighters, debug drawer, frame-step controls)
```

# VFX

> Owner phase: Phase 7 (combat), 11 (bosses), 15 (tiering). Conventions follow `Documentation/00-Canon.md` §9.

```
VFX/
  Combat/Hits/          VFX_Hit_<Weight>_<Element>
  Combat/Trails/        VFX_Trail_<WeaponClass>_<Weight>
  Combat/Defense/       parry, perfect dodge, guard break, block sparks
  Modes/                Ember Rage, Umbral Shadow, Shadow Time
  Ultimates/<WeaponClass>/
  Environment/<RegionId>/  weather, destruction, ambience
```

Every combat VFX has a mobile-low variant (≤ 32 particles, no lights, no distortion) selected by quality tier.
Cue names from move data map to these prefabs through the `CombatCueLibrary` asset.

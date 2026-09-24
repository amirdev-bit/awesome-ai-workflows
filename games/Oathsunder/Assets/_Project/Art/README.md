# Art

> Owner phase: Phase 7 (characters), 9 (weapons), 11 (bosses), 15 (optimisation). Conventions follow `Documentation/00-Canon.md` §9.

Source-controlled, engine-ready art. DCC source files (.blend, .ma, .spp, .ztl) live in the art depot, not here.

```
Art/
  Characters/<CharacterId>/   Meshes/  Materials/  Textures/     e.g. Characters/Rhen/
  Weapons/<WeaponClass>/      Meshes/  Materials/  Textures/     e.g. Weapons/Katana/
  Bosses/<BossId>/            Meshes/  Materials/  Textures/
  Environments/<RegionId>/    Props/  Architecture/  Terrain/  Skies/  Materials/  Textures/
  Shared/                     Trim sheets, decals, detail maps, global materials
```

| Rule | Value |
|---|---|
| Scale | 1 unit = 1 m; characters 1.75–1.90 m; combat plane at z = 0 |
| Character budget (mobile / PC) | 18k / 60k tris LOD0, 3 LODs, 2 materials, 75 bones + 40 secondary |
| Boss budget (mobile / PC) | 35k / 120k tris LOD0 |
| Textures | `_BC` (sRGB), `_N` (normal), `_MRAO` (linear: metal, roughness, AO), `_E` emissive; 2048² characters, 1024² weapons (mobile ASTC 6×6, PC BC7) |
| Naming | `SK_`, `SM_`, `M_`, `MI_`, `T_` prefixes (canon §9.3) |

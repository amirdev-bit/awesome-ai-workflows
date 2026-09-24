# Shaders

> Owner phase: Phase 7 and 15. Conventions follow `Documentation/00-Canon.md` §9.

```
Shaders/
  ShaderGraph/   SG_Character_Lit, SG_Weapon_Trail, SG_Umbra_Dissolve, SG_Environment_Layered …
  HLSL/          custom functions and full HLSL passes (hair, cloth anisotropy, stylised rim)
  Includes/      shared .hlsl includes
```

Keywords are stripped per tier with `ShaderVariantCollection`s; every shader declares its mobile fallback path.

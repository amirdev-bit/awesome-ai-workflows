# Audio

> Owner phase: Phase 13. Conventions follow `Documentation/00-Canon.md` §9.

```
Audio/
  Music/<RegionId|BossId>/   stems per layer: MUS_<Owner>_<Section>_<Layer>
  SFX/Weapons/<WeaponClass>/ swings, impacts (flesh/armour/guard), parries, special skills
  SFX/Impacts/               generic impact layers used by synthesised cues (impact.hit.heavy …)
  SFX/Movement/              footsteps per surface, dash, roll, land, cloth
  SFX/UI/                    menu, rewards, notifications
  Ambience/<RegionId>/       beds and one-shots
  VO/<CharacterId>/          VO_<Character>_<Line>
  Mixers/                    AM_ master mixer and snapshots (combat intensity, cinematic, pause)
```

| Rule | Value |
|---|---|
| Format | 48 kHz source WAV; Vorbis (PC) / ADPCM for short SFX on mobile |
| Loudness | Mix to −16 LUFS integrated (mobile) / −23 LUFS (PC home theatre preset) |
| Variation | Every hit and swing has ≥ 4 variations; random-container playback |

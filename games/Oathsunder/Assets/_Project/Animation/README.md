# Animation

> Owner phase: Phase 7. Conventions follow `Documentation/00-Canon.md` §9.

Clips are authored **in place at 60 fps with exactly the frame count listed in the generated animation
specification** (`Documentation/05-CoreCombat/Generated-FrameData.md`). The simulation owns root motion and
timing; animation owns the pose.

```
Animation/
  Humanoid/Locomotion/    idle, walk, crouch, jump, land, turn
  Humanoid/Reactions/     hitstun (high/mid/low × light/heavy), blockstun, launch, knockdown, wake-up, stagger
  Humanoid/Defense/       guard, parry recovery, dodge, rolls, backstep, tech roll
  Weapons/<WeaponClass>/  every move of the class (A_<Class>_<Move>)
  Paired/                 throws, executions, ultimates (attacker + victim clips share a frame count)
  Bosses/<BossId>/
  Controllers/            AC_ controllers and AO_ overrides (Phase 7 replaces with the Playables graph)
```

| Rule | Value |
|---|---|
| Rig | Unity Humanoid with shared skeleton; weapon bone `weapon_r`, secondary bones for cloth/hair |
| Key poses | Contact pose on the move's first active frame; recovery pose must read clearly at 3 m |
| Paired clips | Victim clip authored relative to the attacker's root at the `victimOffset` in data |
| Naming | `A_<Owner>_<Move>`; victim clips `A_Victim_<Action>_<Owner>` |

# Runtime/Bosses — `Oathsunder.Bosses`

> Owner phase: Phase 11.

Boss phase controller (health thresholds, execution-driven transitions, arena changes), boss-specific AI extensions and cinematic hooks.

The assembly definition is added together with the first code in its phase, following the dependency rules in
`Documentation/03-TechnicalArchitecture.md` §2 (engine-free wherever the logic affects gameplay outcomes).

# Runtime/Input — `Oathsunder.Input`

> Owner phase: Phase 6.

Device mapping (touch, gamepad, keyboard/mouse) onto logical `InputButtons`, sub-tick press latching, full remapping, Simplified/Classic schemes. Implements `ICombatInputSource`.

The assembly definition is added together with the first code in its phase, following the dependency rules in
`Documentation/03-TechnicalArchitecture.md` §2 (engine-free wherever the logic affects gameplay outcomes).

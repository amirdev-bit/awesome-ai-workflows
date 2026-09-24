# Runtime/Input — `Oathsunder.Controls`

> Owner phase: 6. Engine-free (`noEngineReferences: true`); Unity device reading lives in
> `Runtime/Gameplay/Controls`.

| File | Responsibility |
|---|---|
| `InputLatch.cs` | Render-rate samples → one `InputFrame` per 60 Hz tick without losing taps |
| `DirectionFilters.cs` | Analog → 8-way quantiser with cardinal bias; SOCD cleaning for digital devices |
| `InputComposer.cs` | Merges keyboard, gamepad and touch into logical buttons + direction per the profile |
| `ControlProfile.cs` | Scheme, direction settings and remappable bindings; JSON persistence; conflict detection |
| `TouchLayout.cs` | Phone and tablet layouts; touch resolver (floating stick, slide-press buttons) |

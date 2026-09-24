using Oathsunder.Combat.Events;

namespace Oathsunder.Combat.Simulation
{
    /// <summary>Per-frame upkeep of meters, modes, posture regeneration, time dilation and throw protection.</summary>
    internal sealed class ResourceSystem
    {
        private readonly CombatContext _ctx;

        public ResourceSystem(CombatContext context)
        {
            _ctx = context;
        }

        public void Update()
        {
            var state = _ctx.State;
            var tuning = _ctx.Tuning;
            for (int i = 0; i < state.FighterCount; i++)
            {
                ref FighterState f = ref state.Fighters[i];
                if (!f.Active)
                {
                    continue;
                }

                if (f.PostureRegenDelay > 0)
                {
                    f.PostureRegenDelay--;
                }
                else if (f.Posture > 0 && f.Action != FighterAction.Staggered)
                {
                    f.Posture -= tuning.PostureRegenPerFrame;
                    if (f.Posture < 0)
                    {
                        f.Posture = 0;
                    }
                }

                if (f.RageFrames > 0 && --f.RageFrames == 0)
                {
                    _ctx.Emit(CombatEventType.RageEnd, i, -1, f.Position);
                }

                if (f.ShadowFrames > 0 && --f.ShadowFrames == 0)
                {
                    _ctx.Emit(CombatEventType.ShadowEnd, i, -1, f.Position);
                }

                if (f.TimeScaleFrames > 0 && --f.TimeScaleFrames == 0)
                {
                    f.TimeScalePermille = 1000;
                    f.TimeAccumulator = 0;
                }

                if (f.ThrowInvulnerableFrames > 0)
                {
                    f.ThrowInvulnerableFrames--;
                }

                f.Rage = Clamp(f.Rage, tuning.MeterMax);
                f.Shadow = Clamp(f.Shadow, tuning.MeterMax);
                f.Ultimate = Clamp(f.Ultimate, tuning.MeterMax);
            }
        }

        private static int Clamp(int value, int max) => value < 0 ? 0 : (value > max ? max : value);
    }
}

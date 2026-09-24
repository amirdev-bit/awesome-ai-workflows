using System;
using System.Diagnostics;
using System.Globalization;
using Oathsunder.Combat.Definitions;
using Oathsunder.Combat.Input;
using Oathsunder.Combat.Simulation;
using Oathsunder.Tests.Support;

namespace Oathsunder.Tools
{
    /// <summary>
    /// Measures simulation cost per frame (1v1 and 2v2), rollback re-simulation cost and checksum cost.
    /// Inputs are pre-generated so only the simulation is timed. Prints a Markdown report.
    /// </summary>
    internal static class Program
    {
        private const int WarmupFrames = 20_000;
        private const int MeasuredFrames = 200_000;

        private static int Main()
        {
            var content = ContentLocator.Load();
            Console.WriteLine($"| Scenario | Mean (µs) | p50 (µs) | p99 (µs) | Max (µs) | Alloc/frame (B) |");
            Console.WriteLine("|---|---:|---:|---:|---:|---:|");
            Report("1v1 simulation step", Measure(content, 2, rollbackDepth: 0, checksum: false));
            Report("2v2 simulation step", Measure(content, 4, rollbackDepth: 0, checksum: false));
            Report("1v1 step + checksum", Measure(content, 2, rollbackDepth: 0, checksum: true));
            Report("1v1 8-frame rollback (restore + 8 re-sim + save)", Measure(content, 2, rollbackDepth: 8, checksum: false));
            Console.WriteLine();
            Console.WriteLine($"Frames per scenario: {MeasuredFrames:N0} measured after {WarmupFrames:N0} warm-up; fuzzed inputs; match kept live.");
            Console.WriteLine();
            MeasureInputPipeline();
            Console.WriteLine();
            Console.WriteLine($"Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}, " +
                              $"{System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}, {Environment.ProcessorCount} logical cores");
            return 0;
        }

        private static CombatWorld Create(Oathsunder.Combat.Content.CombatContentSet content, int fighters)
        {
            var setup = new CombatSetup
            {
                Stage = content.Stage("stage.training.dojo"),
                Tuning = content.Tuning,
                Rules = content.Rules("rules.ranked"),
            };
            // Keep the match running for the whole measurement: a finished match makes Step a no-op.
            setup.Rules.RoundTimerFrames = 60 * 60;
            setup.Rules.RoundsToWin = 100_000;
            for (int i = 0; i < fighters; i++)
            {
                setup.Combatants.Add(new CombatantSetup(content.BuildBlueprint("fighter.rhen", "moveset.universal", "weapon.katana"), i % 2));
            }

            return new CombatWorld(setup);
        }

        private static Result Measure(Oathsunder.Combat.Content.CombatContentSet content, int fighters, int rollbackDepth, bool checksum)
        {
            var world = Create(content, fighters);
            var generators = new RandomInputGenerator[fighters];
            for (int i = 0; i < fighters; i++)
            {
                generators[i] = new RandomInputGenerator((ulong)(1000 + i));
            }

            int total = WarmupFrames + MeasuredFrames;
            var inputs = new InputFrame[total][];
            for (int f = 0; f < total; f++)
            {
                inputs[f] = new InputFrame[fighters];
                for (int i = 0; i < fighters; i++)
                {
                    inputs[f][i] = generators[i].Next(f % 2 == 0 ? 1 : -1);
                }
            }

            var snapshots = new CombatWorldState[rollbackDepth + 1];
            for (int i = 0; i < snapshots.Length; i++)
            {
                snapshots[i] = new CombatWorldState();
            }

            var samples = new double[MeasuredFrames];
            var stopwatch = new Stopwatch();
            double tickToMicro = 1_000_000.0 / Stopwatch.Frequency;
            long allocated = 0;
            for (int f = 0; f < total; f++)
            {
                bool measuring = f >= WarmupFrames;
                long allocBefore = measuring ? GC.GetAllocatedBytesForCurrentThread() : 0;
                stopwatch.Restart();
                if (rollbackDepth > 0 && f >= rollbackDepth)
                {
                    world.LoadState(snapshots[(f - rollbackDepth) % snapshots.Length]);
                    for (int r = f - rollbackDepth; r < f; r++)
                    {
                        world.Step(inputs[r]);
                    }
                }

                world.SaveState(snapshots[f % snapshots.Length]);
                world.Step(inputs[f]);
                if (checksum)
                {
                    world.Checksum();
                }

                stopwatch.Stop();
                if (world.State.Round.Phase == RoundPhase.MatchOver)
                {
                    throw new InvalidOperationException("The benchmark match ended; results would be meaningless.");
                }

                if (measuring)
                {
                    samples[f - WarmupFrames] = stopwatch.ElapsedTicks * tickToMicro;
                    allocated += GC.GetAllocatedBytesForCurrentThread() - allocBefore;
                }
            }

            Array.Sort(samples);
            double sum = 0;
            foreach (double s in samples)
            {
                sum += s;
            }

            return new Result(sum / samples.Length, samples[samples.Length / 2], samples[(int)(samples.Length * 0.99)], samples[samples.Length - 1], (double)allocated / MeasuredFrames);
        }

        /// <summary>Cost of the device → logical input path (composer + latch) per rendered frame at 240 FPS.</summary>
        private static void MeasureInputPipeline()
        {
            var profile = Oathsunder.Controls.ControlProfile.CreateDefault(ControlScheme.Classic);
            var composer = new Oathsunder.Controls.InputComposer(profile);
            var latch = new Oathsunder.Controls.InputLatch();
            var touch = new Oathsunder.Controls.TouchControlResolver(Oathsunder.Controls.TouchLayout.Phone());
            const int samples = 4_000_000;
            var random = new System.Random(7);
            var snapshots = new Oathsunder.Controls.DeviceSnapshot[1024];
            for (int i = 0; i < snapshots.Length; i++)
            {
                snapshots[i] = new Oathsunder.Controls.DeviceSnapshot
                {
                    Held = (InputButtons)random.Next(0, 2048),
                    StickX = (float)(random.NextDouble() * 2 - 1),
                    StickY = (float)(random.NextDouble() * 2 - 1),
                    Left = random.Next(8) == 0,
                    Right = random.Next(8) == 0,
                };
            }

            long before = GC.GetAllocatedBytesForCurrentThread();
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < samples; i++)
            {
                touch.Update(i & 3, (i & 7) == 0 ? Oathsunder.Controls.TouchPhaseKind.Began : Oathsunder.Controls.TouchPhaseKind.Moved, 300f + (i & 63), 300f, 2400f, 1080f);
                composer.Compose(snapshots[i & 1023], out var buttons, out int x, out int y);
                latch.Sample(buttons | touch.Held, x, y);
                if ((i & 3) == 3)
                {
                    latch.ConsumeTick();
                }
            }

            stopwatch.Stop();
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            double ns = stopwatch.Elapsed.TotalMilliseconds * 1_000_000.0 / samples;
            Console.WriteLine($"Input pipeline (touch resolve + compose + latch) per rendered frame: {ns.ToString("0.0", CultureInfo.InvariantCulture)} ns, allocation {allocated / (double)samples:0.00} B");
        }

        private static void Report(string name, Result r)
        {
            string F(double v) => v.ToString("0.00", CultureInfo.InvariantCulture);
            Console.WriteLine($"| {name} | {F(r.Mean)} | {F(r.P50)} | {F(r.P99)} | {F(r.Max)} | {F(r.AllocPerFrame)} |");
        }

        private readonly struct Result
        {
            public Result(double mean, double p50, double p99, double max, double allocPerFrame)
            {
                Mean = mean;
                P50 = p50;
                P99 = p99;
                Max = max;
                AllocPerFrame = allocPerFrame;
            }

            public double Mean { get; }
            public double P50 { get; }
            public double P99 { get; }
            public double Max { get; }
            public double AllocPerFrame { get; }
        }
    }
}

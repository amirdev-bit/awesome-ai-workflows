using System;
using System.Collections.Generic;

namespace Oathsunder.Combat.Input
{
    /// <summary>One step of a motion command: a set of accepted directions.</summary>
    public readonly struct MotionStep
    {
        /// <summary>Accepted facing-relative directions.</summary>
        public readonly DirectionMask Accept;

        /// <summary>Creates a step.</summary>
        public MotionStep(DirectionMask accept)
        {
            Accept = accept;
        }
    }

    /// <summary>
    /// A directional motion such as a quarter-circle (236) or a double tap (66), matched backwards through the
    /// input history with per-step and total leniency windows.
    /// </summary>
    public sealed class MotionCommand
    {
        /// <summary>Creates a motion command.</summary>
        /// <param name="id">Stable identifier used in data (e.g. "236").</param>
        /// <param name="steps">Steps in input order.</param>
        /// <param name="stepWindow">Max frames between consecutive steps.</param>
        /// <param name="totalWindow">Max frames from the first step to the final step.</param>
        /// <param name="finalLeniency">Max frames the final step may precede the button press.</param>
        public MotionCommand(string id, MotionStep[] steps, int stepWindow, int totalWindow, int finalLeniency)
        {
            if (steps == null || steps.Length == 0)
            {
                throw new ArgumentException("A motion needs at least one step.", nameof(steps));
            }

            Id = id;
            Steps = steps;
            StepWindow = stepWindow;
            TotalWindow = totalWindow;
            FinalLeniency = finalLeniency;
        }

        /// <summary>Stable identifier.</summary>
        public string Id { get; }

        /// <summary>Steps in input order.</summary>
        public MotionStep[] Steps { get; }

        /// <summary>Max frames between consecutive steps.</summary>
        public int StepWindow { get; }

        /// <summary>Max frames from first to final step.</summary>
        public int TotalWindow { get; }

        /// <summary>Max frames the final step may precede the button press.</summary>
        public int FinalLeniency { get; }

        /// <summary>
        /// True when the motion was completed with its final step on or shortly before <paramref name="pressFrame"/>.
        /// Directions are evaluated relative to <paramref name="facingSign"/>.
        /// </summary>
        public bool MatchesBefore(InputHistory history, int pressFrame, int facingSign, int earliestFrame)
        {
            int lastStep = Steps.Length - 1;
            int searchFrom = pressFrame;
            int searchTo = Math.Max(earliestFrame, pressFrame - FinalLeniency);
            int found = FindLatest(history, Steps[lastStep].Accept, searchFrom, searchTo, facingSign);
            if (found < 0)
            {
                return false;
            }

            // Between the final step and the press only neutral is allowed. This stops "6 then 236" from
            // resolving as 623 while still letting players release the stick just before pressing.
            for (int frame = found + 1; frame <= pressFrame; frame++)
            {
                if (history.Get(frame).Direction(facingSign) != NumpadDirection.Neutral)
                {
                    return false;
                }
            }

            return MatchEarlierSteps(history, lastStep, found, facingSign, earliestFrame);
        }

        /// <summary>
        /// True when the final step was <i>entered</i> on <paramref name="frame"/> (the direction was not in the
        /// final step's set on the previous frame) and every earlier step precedes it. Used by motion-only
        /// commands such as dashes, whose "press" is the final direction itself.
        /// </summary>
        public bool CompletesOn(InputHistory history, int frame, int facingSign, int earliestFrame)
        {
            int lastStep = Steps.Length - 1;
            DirectionMask accept = Steps[lastStep].Accept;
            if (!accept.Matches(history.Get(frame).Direction(facingSign)))
            {
                return false;
            }

            if (accept.Matches(history.Get(frame - 1).Direction(facingSign)) && history.Contains(frame - 1))
            {
                return false;
            }

            return MatchEarlierSteps(history, lastStep, frame, facingSign, earliestFrame);
        }

        private bool MatchEarlierSteps(InputHistory history, int lastStep, int lastFound, int facingSign, int earliestFrame)
        {
            int finalFrame = lastFound;
            int previous = lastFound;
            for (int step = lastStep - 1; step >= 0; step--)
            {
                int from = previous - 1;
                int to = Math.Max(earliestFrame, previous - StepWindow);
                int found = FindLatest(history, Steps[step].Accept, from, to, facingSign);
                if (found < 0)
                {
                    return false;
                }

                previous = found;
            }

            return finalFrame - previous <= TotalWindow;
        }

        private static int FindLatest(InputHistory history, DirectionMask accept, int from, int to, int facingSign)
        {
            for (int frame = from; frame >= to; frame--)
            {
                if (!history.Contains(frame))
                {
                    return -1;
                }

                if (accept.Matches(history.Get(frame).Direction(facingSign)))
                {
                    return frame;
                }
            }

            return -1;
        }
    }

    /// <summary>The standard motion vocabulary. Data files reference these by id.</summary>
    public static class StandardMotions
    {
        /// <summary>236 — quarter circle forward.</summary>
        public static readonly MotionCommand QuarterCircleForward = new MotionCommand(
            "236",
            new[] { new MotionStep(DirectionMask.Down), new MotionStep(DirectionMask.DownForward), new MotionStep(DirectionMask.Forward | DirectionMask.UpForward) },
            stepWindow: 10, totalWindow: 20, finalLeniency: 3);

        /// <summary>214 — quarter circle back.</summary>
        public static readonly MotionCommand QuarterCircleBack = new MotionCommand(
            "214",
            new[] { new MotionStep(DirectionMask.Down), new MotionStep(DirectionMask.DownBack), new MotionStep(DirectionMask.Back | DirectionMask.UpBack) },
            stepWindow: 10, totalWindow: 20, finalLeniency: 3);

        /// <summary>623 — "dragon" motion. Also accepts the 323 shortcut.</summary>
        public static readonly MotionCommand DragonForward = new MotionCommand(
            "623",
            new[] { new MotionStep(DirectionMask.Forward | DirectionMask.DownForward), new MotionStep(DirectionMask.Down | DirectionMask.DownBack), new MotionStep(DirectionMask.DownForward) },
            stepWindow: 10, totalWindow: 20, finalLeniency: 3);

        /// <summary>66 — double tap forward (the middle step must leave the forward column).</summary>
        public static readonly MotionCommand DoubleTapForward = new MotionCommand(
            "66",
            new[]
            {
                new MotionStep(DirectionMask.Forward),
                new MotionStep(DirectionMask.DownBack | DirectionMask.Down | DirectionMask.Back | DirectionMask.Neutral | DirectionMask.UpBack | DirectionMask.Up),
                new MotionStep(DirectionMask.Forward),
            },
            stepWindow: 8, totalWindow: 14, finalLeniency: 0);

        /// <summary>44 — double tap back (the middle step must leave the back column).</summary>
        public static readonly MotionCommand DoubleTapBack = new MotionCommand(
            "44",
            new[]
            {
                new MotionStep(DirectionMask.Back),
                new MotionStep(DirectionMask.Down | DirectionMask.DownForward | DirectionMask.Neutral | DirectionMask.Forward | DirectionMask.Up | DirectionMask.UpForward),
                new MotionStep(DirectionMask.Back),
            },
            stepWindow: 8, totalWindow: 14, finalLeniency: 0);

        private static readonly Dictionary<string, MotionCommand> ById = new Dictionary<string, MotionCommand>(StringComparer.Ordinal)
        {
            { QuarterCircleForward.Id, QuarterCircleForward },
            { QuarterCircleBack.Id, QuarterCircleBack },
            { DragonForward.Id, DragonForward },
            { DoubleTapForward.Id, DoubleTapForward },
            { DoubleTapBack.Id, DoubleTapBack },
        };

        /// <summary>Looks up a motion by id.</summary>
        public static bool TryGet(string id, out MotionCommand motion) => ById.TryGetValue(id, out motion);
    }
}

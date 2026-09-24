using Oathsunder.Core.Mathematics;

namespace Oathsunder.Combat.Definitions
{
    /// <summary>Hurtboxes and pushbox for one stance.</summary>
    public sealed class StanceBoxes
    {
        /// <summary>Facing-relative hurtboxes.</summary>
        public FixedAabb[] Hurtboxes = new FixedAabb[0];

        /// <summary>Facing-relative pushbox (body collision and stage bounds).</summary>
        public FixedAabb Pushbox;
    }

    /// <summary>
    /// Body definition of a fighter: movement, physics, stances and base resources. The weapon moveset is
    /// separate, so any character can wield any weapon class.
    /// </summary>
    public sealed class FighterDefinition
    {
        /// <summary>Stable id, e.g. "fighter.rhen".</summary>
        public string Id = "";

        /// <summary>Display name.</summary>
        public string Name = "";

        /// <summary>Maximum health.</summary>
        public int MaxHealth = 1000;

        /// <summary>Maximum posture.</summary>
        public int MaxPosture = 1000;

        /// <summary>Walk forward speed (m/frame).</summary>
        public Fixed WalkForwardSpeed;

        /// <summary>Walk backward speed (m/frame).</summary>
        public Fixed WalkBackwardSpeed;

        /// <summary>Frames of jump startup (grounded, throwable).</summary>
        public int PreJumpFrames = 4;

        /// <summary>Jump vertical launch velocity (m/frame).</summary>
        public Fixed JumpVelocityY;

        /// <summary>Forward jump horizontal velocity (m/frame).</summary>
        public Fixed JumpForwardVelocityX;

        /// <summary>Backward jump horizontal velocity (m/frame).</summary>
        public Fixed JumpBackwardVelocityX;

        /// <summary>Gravity (m/frame²).</summary>
        public Fixed Gravity;

        /// <summary>Terminal fall speed (m/frame, positive).</summary>
        public Fixed MaxFallSpeed;

        /// <summary>Ground friction applied to residual velocity (m/frame²).</summary>
        public Fixed GroundFriction;

        /// <summary>Landing recovery after a normal jump.</summary>
        public int LandingFrames = 3;

        /// <summary>Soft knockdown duration before wake-up.</summary>
        public int SoftKnockdownFrames = 30;

        /// <summary>Hard knockdown duration before wake-up.</summary>
        public int HardKnockdownFrames = 50;

        /// <summary>Wake-up duration (fully invulnerable).</summary>
        public int WakeUpFrames = 20;

        /// <summary>Knockback multiplier (permille; heavier fighters fly less).</summary>
        public int KnockbackPermille = 1000;

        /// <summary>Standing boxes.</summary>
        public StanceBoxes Standing = new StanceBoxes();

        /// <summary>Crouching boxes.</summary>
        public StanceBoxes Crouching = new StanceBoxes();

        /// <summary>Airborne boxes.</summary>
        public StanceBoxes Airborne = new StanceBoxes();

        /// <summary>Knocked-down boxes.</summary>
        public StanceBoxes Knockdown = new StanceBoxes();
    }
}

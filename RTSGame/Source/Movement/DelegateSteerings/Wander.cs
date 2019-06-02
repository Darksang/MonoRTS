using System;
using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Wander : Face {
        // Holds the radius of the Wander
        public float WanderRadius { get; set; }
        // Holds the forward offset of the Wander
        public float WanderOffset { get; set; }
        // Holds the maximum rate at which the Wander orientation can change
        public float WanderRate { get; set; }
        // Holds the current orientation of the Wander target
        public float WanderOrientation { get; set; }
        // Used to generate random orientation
        private Random Random;

        public Wander() : base() {
            WanderRadius = 2f;
            WanderOffset = 2f;
            WanderRate = 1f;
            WanderOrientation = 2f;

            Random = new Random();
        }

        public override Steering GetSteering(Unit Unit) {
            // Update wander orientation
            float RandomBinomial = (float)(Random.NextDouble() - Random.NextDouble());
            WanderOrientation += RandomBinomial * WanderRate;

            // Calculate the combined target orientation
            float TargetOrientation = WanderOrientation + Unit.Body.Rotation;
            float TX = (float)Math.Cos(MathHelper.ToRadians(TargetOrientation));
            float TY = (float)Math.Sin(MathHelper.ToRadians(TargetOrientation));
            Vector2 TargetOrientationVector = new Vector2(TX, TY);

            // Calculate the center of the wander circle
            float OX = (float)Math.Cos(MathHelper.ToRadians(Unit.Body.Rotation));
            float OY = (float)Math.Sin(MathHelper.ToRadians(Unit.Body.Rotation));
            Vector2 OrientationVector = new Vector2(OX, OY);

            Vector2 Target = Unit.Transform.Position + WanderOffset * OrientationVector;

            // Calculate the target location
            Target += WanderRadius * TargetOrientationVector;

            // Delegate to Face
            Steering Result = GetSteering(Unit, Target);

            // Set the linear acceleration to be at full acceleration in the direction of the orientation
            Result.Linear = Unit.Body.MaxAcceleration * OrientationVector;

            return Result;
        }
    }
}

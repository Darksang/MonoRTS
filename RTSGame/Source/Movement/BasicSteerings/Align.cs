using System;
using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Align : SteeringBehaviour {

        // Holds the target angle
        public float Angle { get; set; }
        // Holds the angle for slowing down
        public float SlowAngle { get; set; }
        // Holds the time to achieve target rotation
        public float TimeToTarget { get; set; }

        // Create an Align behaviour
        public Align() : base() {
            TimeToTarget = 0.25f; // Default value, can be changed using its property
        }

        public override Steering GetSteering(Unit Unit) {
            Steering Result = new Steering();

            // If there is no target there's no need to move
            if (Target == null)
                return Result;

            // Get naive direction to the target
            float Rotation = MathHelper.ToRadians(Target.Transform.Rotation) - MathHelper.ToRadians(Unit.Transform.Rotation);
            float RotationSize = Math.Abs(Rotation);

            // Check if we're there
            if (RotationSize < Angle)
                return Result;

            float TargetRotation;

            // If we are outside SlowRadius, use maximum rotation
            if (RotationSize > SlowAngle)
                TargetRotation = Unit.Body.MaxRotation;
            else
                // Otherwise calculate scaled rotation
                TargetRotation = Unit.Body.MaxRotation * RotationSize / SlowAngle;

            // The final target rotation combines speed and direction
            TargetRotation *= Rotation / RotationSize;

            // Acceleration tries to get to the target rotation
            Result.Angular = TargetRotation - Unit.Body.Rotation;
            Result.Angular /= TimeToTarget;

            // If it's too fast, clip it to MaxAngular
            if (Math.Abs(Result.Angular) > Unit.Body.MaxAngular) {
                Result.Angular /= Math.Abs(Result.Angular);
                Result.Angular *= Unit.Body.MaxAngular;
            }

            // Output the steering
            return Result;
        }

        public Steering GetSteering(Unit Unit, float Orientation) {
            Steering Result = new Steering();

            // Get naive direction to the target
            float Rotation = Orientation - MathHelper.ToRadians(Unit.Transform.Rotation);
            float RotationSize = Math.Abs(Rotation);

            // Check if we're there
            if (RotationSize < Angle)
                return Result;

            float TargetRotation;

            // If we are outside SlowRadius, use maximum rotation
            if (RotationSize > SlowAngle)
                TargetRotation = Unit.Body.MaxRotation;
            else
                // Otherwise calculate scaled rotation
                TargetRotation = Unit.Body.MaxRotation * RotationSize / SlowAngle;

            // The final target rotation combines speed and direction
            TargetRotation *= Rotation / RotationSize;

            // Acceleration tries to get to the target rotation
            Result.Angular = TargetRotation - Unit.Body.Rotation;
            Result.Angular /= TimeToTarget;

            // If it's too fast, clip it to MaxAngular
            if (Math.Abs(Result.Angular) > Unit.Body.MaxAngular) {
                Result.Angular /= Math.Abs(Result.Angular);
                Result.Angular *= Unit.Body.MaxAngular;
            }

            // Output the steering
            return Result;
        }

        public override void SetTarget(Unit Target) {
            this.Target = Target;

            Angle = MathHelper.ToRadians(Target.Body.ExteriorAngle);
            SlowAngle = MathHelper.ToRadians(Target.Body.InteriorAngle);
        }
    }
}

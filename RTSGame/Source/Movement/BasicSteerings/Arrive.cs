using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Arrive : SteeringBehaviour {

        // Holds the radius for arriving at the target
        public float Radius { get; set; }
        // Hols the radius for slowing down
        public float SlowRadius { get; set; }
        // Holds the time over which to achieve target speed
        public float TimeToTarget { get; set; }

        // Create an Arrive behaviour
        public Arrive() : base() {
            TimeToTarget = 0.1f; // Default value, can be changed using its property
        }

        public override Steering GetSteering(Unit Unit) {
            Steering Result = new Steering();

            // If there is no target there's no need to move
            if (Target == null)
                return Result;

            // Get the direction to the target
            Vector2 Direction = Target.Transform.Position - Unit.Transform.Position;
            float Distance = Direction.Length();

            // Check if we're within target Radius
            if (Distance < Radius)
                // We arrived, no need to move
                return Result;

            float TargetSpeed;

            // If we are outside the SlowRadius, then go MaxVelocity
            if (Distance > SlowRadius)
                TargetSpeed = Unit.Body.MaxVelocity;
            else
                // Otherwise, calculate scaled speed
                TargetSpeed = Unit.Body.MaxVelocity * Distance / SlowRadius;

            // Combine TargetSpeed and Direction
            Vector2 TargetVelocity = Direction;
            TargetVelocity.Normalize();
            TargetVelocity *= TargetSpeed;

            // Acceleration tries to get to the TargetVelocity
            Result.Linear = TargetVelocity - Unit.Body.Velocity;
            Result.Linear /= TimeToTarget;

            // If it's too fast, clip it to MaxAcceleration
            if (Result.Linear.Length() > Unit.Body.MaxAcceleration) {
                Result.Linear.Normalize();
                Result.Linear *= Unit.Body.MaxAcceleration;
            }

            // Output the steering
            return Result;
        }

        public override void SetTarget(Unit Target) {
            this.Target = Target;

            Radius = Target.Body.ExteriorRadius;
            SlowRadius = Target.Body.InteriorRadius;
        }
    }
}

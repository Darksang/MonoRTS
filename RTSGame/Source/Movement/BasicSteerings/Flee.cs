using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Flee : SteeringBehaviour {

        // Create a Flee behaviour
        public Flee() : base() { }

        // Seeks assigned Target
        public override Steering GetSteering(Unit Unit) {
            Steering Result = new Steering();

            // If there is no target there's no need to move
            if (Target == null)
                return Result;

            // Get the direction away from the target
            Result.Linear = Unit.Transform.Position - Target.Transform.Position;

            // Give full acceleration along this direction
            Result.Linear.Normalize();
            Result.Linear *= Unit.Body.MaxAcceleration;

            // Output the steering
            return Result;
        }

        // Flees from a specific position
        public Steering GetSteering(Unit Unit, Vector2 Position) {
            Steering Result = new Steering();

            // Get the direction away from the target
            Result.Linear = Unit.Transform.Position - Position;

            // Give full acceleration along this direction
            Result.Linear.Normalize();
            Result.Linear *= Unit.Body.MaxAcceleration;

            // Output the steering
            return Result;
        }

        public override void SetTarget(Unit Target) {
            this.Target = Target;
        }
    }
}
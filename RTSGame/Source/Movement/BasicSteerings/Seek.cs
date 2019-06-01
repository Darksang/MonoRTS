using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Seek : SteeringBehaviour {

        // Create a Seek behaviour
        public Seek() : base() { }

        // Seeks assigned Target
        public override Steering GetSteering(Unit Unit) {
            Steering Result = new Steering();

            // If there is no target there's no need to move
            if (Target == null)
                return Result;

            // Get the direction to the target
            Result.Linear = Target.Transform.Position - Unit.Transform.Position;

            // Give full acceleration along this direction
            Result.Linear.Normalize();
            Result.Linear *= Unit.Body.MaxAcceleration;

            // Output the steering
            return Result;
        }

        // Seeks specific position
        public Steering GetSteering(Unit Unit, Vector2 Position) {
            Steering Result = new Steering();

            // Get the direction to the target position
            Result.Linear = Position - Unit.Transform.Position;

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

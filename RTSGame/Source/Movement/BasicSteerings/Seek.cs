namespace RTSGame {

    public class Seek : SteeringBehaviour {

        // Create a Seek behaviour assigned to a Unit
        public Seek() : base() { }

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

        public override void SetTarget(Unit Target) {
            this.Target = Target;
        }
    }
}

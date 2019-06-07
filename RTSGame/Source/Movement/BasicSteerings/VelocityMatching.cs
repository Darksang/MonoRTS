namespace RTSGame {

    public class VelocityMatching : SteeringBehaviour {

        // Holds the time to target constant
        public float TimeToTarget { get; set; }

        // Create a VelocityMatching behaviour
        public VelocityMatching() : base() {
            TimeToTarget = 0.1f; // Default value, can be changed using its property
            Type = SteeringType.VelocityMatching;
        }

        public override Steering GetSteering(Unit Unit) {
            Steering Result = new Steering();

            // If there is no target there's no need to move
            if (Target == null)
                return Result;

            // Acceleration tries to get to the target velocity
            Result.Linear = Target.Body.Velocity - Unit.Body.Velocity;
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
        }
    }
}

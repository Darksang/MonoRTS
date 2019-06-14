using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Alignment : SteeringBehaviour {

        // Holds group of targets
        public List<Unit> Targets;
        // Holds the threshold to take action
        public float Threshold { get; set; }

        // Create n Alignment behaviour
        public Alignment() : base() {
            Targets = new List<Unit>();
            Threshold = 300f;
            Type = SteeringType.Alignment;
        }

        public override Steering GetSteering(Unit Unit) {
            Vector2 Heading = Vector2.Zero;
            int Count = 0;

            // Loop through each target
            foreach (Unit U in Targets) {
                if (U == Unit)
                    continue;
                // Check if the target is close
                Vector2 Direction = U.Transform.Position - Unit.Transform.Position;
                float Distance = Direction.Length();

                if (Distance > Threshold)
                    continue;

                Heading += U.Body.Velocity;
                Count++;
            }

            if (Count > 0) {
                Heading /= Count;
                Heading -= Unit.Body.Velocity;
            }

            Steering Result = new Steering();
            Result.Linear = Heading;

            return Result;
        }

        // TODO: Set group of targets?
        public override void SetTarget(Unit Target) { }
    }
}

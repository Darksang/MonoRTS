using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Separation : SteeringBehaviour {

        // Holds group of targets
        public List<Unit> Targets;
        // Holds the threshold to take action
        public float Threshold { get; set; }

        // Create a Separation behaviour
        public Separation() : base() {
            Targets = new List<Unit>();
            Threshold = 150f;
            Type = SteeringType.Separation;
        }

        public override Steering GetSteering(Unit Unit) {
            Steering Result = new Steering();
            
            // Loop through each target
            foreach (Unit U in Targets) {
                if (U == Unit)
                    continue;
                // Check if the target is close 
                Vector2 Direction = U.Transform.Position - Unit.Transform.Position;
                float Distance = Direction.Length();

                if (Distance < Threshold) {
                    // Calculate the strength of repulsion
                    float Strength = Unit.Body.MaxAcceleration * (Threshold - Distance) / Threshold;
                    // Add the acceleration
                    Direction.Normalize(); 
                    Result.Linear += Strength * Direction;
                }
            }

            // Output the result
            return Result;
        }

        // TODO: Set group of targets?
        public override void SetTarget(Unit Target) { }
    }
}

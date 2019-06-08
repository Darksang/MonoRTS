using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Cohesion : SteeringBehaviour {

        // Holds group of targets
        public List<Unit> Targets;
        // Holds the threshold to take action
        public float Threshold { get; set; }

        // Create a Cohesion behaviour
        public Cohesion() : base() {
            Targets = new List<Unit>();
            Threshold = 100f;
        }

        public override Steering GetSteering(Unit Unit) {
            Vector2 CenterOfMass = Vector2.Zero;
            int Count = 0;

            // Loop through each target
            foreach (Unit U in Targets) {
                // Check if the target is close
                Vector2 Direction = U.Transform.Position - Unit.Transform.Position;
                float Distance = Direction.Length();

                if (Distance < Threshold)
                    continue;

                CenterOfMass += U.Transform.Position;
                Count++;
            }

            if (Count > 0) {
                CenterOfMass /= Count;
            }

            Steering Result = new Steering();
            Result.Linear = CenterOfMass;

            return Result;
        }

        // TODO: Set group of targets?
        public override void SetTarget(Unit Target) { }
    }
}

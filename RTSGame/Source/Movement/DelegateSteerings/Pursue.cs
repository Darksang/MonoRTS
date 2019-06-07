﻿using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Pursue : Seek {

        // Holds the maximum prediction time
        public float MaxPrediction { get; set; }

        // Create a Pursue behaviour
        public Pursue() : base() {
            MaxPrediction = 4f; // Default value, can be changed using its property
            Type = SteeringType.Pursue;
        }

        public override Steering GetSteering(Unit Unit) {
            // If there is no target there's no need to move
            if (Target == null)
                return new Steering();

            // Get the distance to the target
            Vector2 Direction = Target.Transform.Position - Unit.Transform.Position;
            float Distance = Direction.Length();

            // Get current speed
            float Speed = Unit.Body.Velocity.Length();

            float Prediction;
            // Check if speed is too small to give a reasonable prediction time
            if (Speed <= Distance / MaxPrediction)
                Prediction = MaxPrediction;
            else
                // Otherwise, calculate the prediction time
                Prediction = Distance / Speed;

            // Predict target position in the future
            Vector2 FuturePosition = Target.Transform.Position + Target.Body.Velocity * Prediction;

            return GetSteering(Unit, FuturePosition);
        }
    }
}

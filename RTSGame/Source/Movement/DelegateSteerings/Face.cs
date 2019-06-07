using System;
using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Face : Align {

        // Create a Face behaviour
        public Face() : base() {
            Type = SteeringType.Face;
        }

        public override Steering GetSteering(Unit Unit) {
            // If there is no target there's no need to move
            if (Target == null)
                return new Steering();

            // Get the distance to the target
            Vector2 Direction = Target.Transform.Position - Unit.Transform.Position;

            // Check for a zero direction, and make no change if so
            if (Direction.Length() == 0)
                return base.GetSteering(Unit);

            // Calculate target orientation
            float Orientation = (float)Math.Atan2(Direction.Y, Direction.X);

            return GetSteering(Unit, Orientation);
        }

        public Steering GetSteering(Unit Unit, Vector2 Target) {
            // Get the distance to the target
            Vector2 Direction = Target - Unit.Transform.Position;

            // Check for a zero direction, and make no change if so
            if (Direction.Length() == 0)
                return base.GetSteering(Unit);

            // Calculate target orientation
            float Orientation = (float)Math.Atan2(Direction.Y, Direction.X);

            return GetSteering(Unit, Orientation);
        }
    }
}

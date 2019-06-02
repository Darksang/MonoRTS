using System;

namespace RTSGame {

    public class LookWhereYouGoing : Align {

        // Create a Face behaviour
        public LookWhereYouGoing() : base() { }

        public override Steering GetSteering(Unit Unit) {
            // Check for a zero direction, and make no change if so
            if (Unit.Body.Velocity.Length() == 0)
                return new Steering();

            // Otherwise set the target based on the velocity
            float Orientation = (float)Math.Atan2(Unit.Body.Velocity.Y, Unit.Body.Velocity.X);

            return GetSteering(Unit, Orientation);
        }
    }
}

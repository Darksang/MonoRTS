using Microsoft.Xna.Framework;

namespace RTSGame {

    public class MoveToPosition : Seek {
        // Holds the target position
        public Vector2 WorldPosition;

        public MoveToPosition() : base() {
            WorldPosition = new Vector2();
            Weight = 0;
            Type = SteeringType.MoveToPosition;
        }

        public override Steering GetSteering(Unit Unit) {
            return GetSteering(Unit, WorldPosition);
        }
    }
}

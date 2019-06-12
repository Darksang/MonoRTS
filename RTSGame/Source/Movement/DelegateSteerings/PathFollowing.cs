using Microsoft.Xna.Framework;

namespace RTSGame {

    public class PathFollowing : Seek {

        // Holds the Path to follow
        private Path Path;
        // Current target position in the Path
        private int NextPosition;
        // Radius to advance to the next position
        private float Radius;

        // Create a PathFollowing behaviour
        public PathFollowing() : base() {
            Path = null;
            Radius = 8f;
            Type = SteeringType.PathFollowing;
        }

        public override Steering GetSteering(Unit Unit) {
            // If the Path is empty, we don't move
            if (Path == null || NextPosition >= Path.Positions.Count)
                return new Steering();

            // Check if we arrived at the next position in the Path
            Vector2 Direction = Path.Positions[NextPosition] - Unit.Transform.Position;
            float Distance = Direction.Length();

            if (Distance < Radius) {
                NextPosition++;

                // If there are no more positions in the Path, stop moving
                if (NextPosition >= Path.Positions.Count)
                    return new Steering();
            }

            // Seek the current target position in the Path
            return GetSteering(Unit, Path.Positions[NextPosition]);
        }

        public void SetPath(Path Path) {
            this.Path = Path;
            NextPosition = 1;
        }
    }
}

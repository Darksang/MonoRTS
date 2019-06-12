using Microsoft.Xna.Framework;

using FarseerPhysics.Dynamics;

namespace RTSGame {

    public class ObstacleAvoidance : Seek {
        // We need the Physics world to be able to raycast
        public World World;

        // Holds the minimum distance to a wall to avoid collision
        public float AvoidDistance;
        // Holds the distance to look ahead for a collision
        public float LookAhead;

        // Creates a ObstacleAvoidance behaviour
        public ObstacleAvoidance() {
            AvoidDistance = 300f;
            LookAhead = 350f;
        }

        public override Steering GetSteering(Unit Unit) {
            // Calculate the collision ray vector
            Vector2 RayVector = Unit.Body.Velocity;
            RayVector.Normalize();
            RayVector *= LookAhead;

            // Find a collision
            bool Hit = false;
            Vector2 CollisionPosition = Vector2.Zero;
            Vector2 CollisionNormal = Vector2.Zero;

            World.RayCast((f, p, n, fr) => {
                Hit = true;

                CollisionPosition = p;
                CollisionNormal = n;

                return 0;
            }, ConvertUnits.ToSimUnits(Unit.Transform.Position), ConvertUnits.ToSimUnits(RayVector));

            // If there's no collision, do nothing
            if (!Hit)
                return new Steering();

            // Otherwise create a target
            Vector2 Target = ConvertUnits.ToDisplayUnits(CollisionPosition) + CollisionNormal * AvoidDistance;

            return GetSteering(Unit, Target);
        }
    }
}

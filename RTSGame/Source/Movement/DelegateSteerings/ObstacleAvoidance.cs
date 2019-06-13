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

        // Generated Ray, can be used to draw it on screen
        public Vector2 Ray;
        // Holds the position where a collision took place, can be used to draw it on screen
        public Vector2 CollisionPosition;

        // Creates a ObstacleAvoidance behaviour
        public ObstacleAvoidance() {
            AvoidDistance = 250f;
            LookAhead = 250f;
        }

        public override Steering GetSteering(Unit Unit) {
            // Calculate the collision ray vector
            Vector2 RayVector = Unit.Body.Velocity;
            RayVector.Normalize();
            RayVector *= LookAhead;
            RayVector += Unit.Transform.Position;
            Ray = RayVector;

            // Find a collision
            bool Hit = false;

            Vector2 CollisionNormal = Vector2.Zero;

            World.RayCast((f, p, n, fr) => {
                if (f.Body.UserData != null)
                    return -1;

                Hit = true;
                
                CollisionPosition = ConvertUnits.ToDisplayUnits(p);
                CollisionNormal = n;

                return 0;
            }, ConvertUnits.ToSimUnits(Unit.Transform.Position), ConvertUnits.ToSimUnits(RayVector));

            // If there's no collision, do nothing
            if (!Hit)
                return new Steering();

            // Otherwise create a target
            Vector2 Target = CollisionPosition + CollisionNormal * AvoidDistance;

            return GetSteering(Unit, Target);
        }
    }
}

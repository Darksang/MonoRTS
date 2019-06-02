using FarseerPhysics.Factories;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;

using PhysicsBody = FarseerPhysics.Dynamics.Body;

namespace RTSGame {

    public class Collider {
        // Physics body
        public PhysicsBody Body { get; private set; }
        // Defines the shape of the body
        public Fixture Fixture { get; private set; }

        public Collider() {

        }

        public void Initialize(World World, Unit Unit) {
            Body = BodyFactory.CreateBody(World);

            Body.Position = Unit.Transform.Position;
        }
    }
}

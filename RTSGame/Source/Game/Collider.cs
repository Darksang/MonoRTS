﻿using FarseerPhysics.Factories;
using FarseerPhysics.Dynamics;

using PhysicsBody = FarseerPhysics.Dynamics.Body;

namespace RTSGame {

    public class Collider {
        // Physics body
        public PhysicsBody Body { get; set; }
        // Defines the shape of the body
        public Fixture Fixture { get; set; }

        // Pointer to the physics world
        public World World { get; set; }

        public Collider() {
            Body = null;
            Fixture = null;
        }

        // Initializes a Body in the physics World, with a Rectangle Shape
        public void Initialize(World World, Unit Unit) {
            // Scale Width and Height to simulation units
            float Width = ConvertUnits.ToSimUnits(Unit.Sprite.SpriteTexture.Width * Unit.Transform.Scale.X);
            float Height = ConvertUnits.ToSimUnits(Unit.Sprite.SpriteTexture.Height * Unit.Transform.Scale.Y);
            // Create a Body with a Rectangle fixture
            Body = BodyFactory.CreateRectangle(World, Width, Height, 1f, Unit);
            // Position the Body in the World according to Transform in sim units
            Body.Position = ConvertUnits.ToSimUnits(Unit.Transform.Position);

            Fixture = Body.FixtureList[0];

            this.World = World;
        }
    }
}

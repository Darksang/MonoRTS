using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerPhysics.Dynamics;

namespace RTSGame {

    public class Unit {
        // Name of the Unit
        public string Name { get; set; }
        // Holds the Position, Scale and Rotation of the Unit
        public Transform Transform { get; set; }
        // Holds the texture used to draw the Unit
        public Sprite Sprite { get; set; }
        // Holds information used to move the Unit using Steerings
        public Body Body { get; set; }
        // Physics Body in the World
        public Collider Collider { get; set; }

        // Holds all movement behaviours of the Unit
        public Dictionary<SteeringType, SteeringBehaviour> Behaviours { get; private set; }

        // Whether this Unit is selected or not
        public bool Selected { get; set; }

        public Unit(Sprite Sprite, World World) {
            Name = "";
            Transform = new Transform();
            this.Sprite = Sprite;
            Body = new Body();

            Collider = new Collider();
            Collider.Initialize(World, this);

            Behaviours = new Dictionary<SteeringType, SteeringBehaviour>();

            Selected = false;
        }

        public Unit(string Name, Sprite Sprite, World World) {
            this.Name = Name;
            Transform = new Transform();
            this.Sprite = Sprite;
            Body = new Body();

            Collider = new Collider();
            Collider.Initialize(World, this);

            Behaviours = new Dictionary<SteeringType, SteeringBehaviour>();

            Selected = false;
        }

        public void Update(float DeltaTime) {
            // Check if the Unit is able to move
            if (Body.CanMove) {
                // Compute final steering
                Steering Final = GetBlendedSteering();

                // Apply Newton-Euler equations
                Body.Velocity += Final.Linear * DeltaTime;
                Body.Rotation += MathHelper.ToDegrees(Final.Angular) * DeltaTime;

                // Cap velocity and rotation to their max values
                if (Body.Velocity.X > Body.MaxVelocity)
                    Body.Velocity = new Vector2(Body.MaxVelocity, Body.Velocity.Y);

                if (Body.Velocity.X < -Body.MaxVelocity)
                    Body.Velocity = new Vector2(-Body.MaxVelocity, Body.Velocity.Y);

                if (Body.Velocity.Y > Body.MaxVelocity)
                    Body.Velocity = new Vector2(Body.Velocity.X, Body.MaxVelocity);

                if (Body.Velocity.Y < -Body.MaxVelocity)
                    Body.Velocity = new Vector2(Body.Velocity.X, -Body.MaxVelocity);

                if (Body.Rotation > Body.MaxRotation)
                    Body.Rotation = Body.MaxRotation;

                // Move the Unit based on computed Velocity and Rotation
                Transform.Position += Body.Velocity * DeltaTime;
                Transform.Rotation += Body.Rotation * DeltaTime;
            }

            // Update Physics Body position
            Collider.Body.Position = ConvertUnits.ToSimUnits(Transform.Position);
        }

        // Combine all SteeringBehaviours
        private Steering GetBlendedSteering() {
            // Create the Steering structure for accumulation
            Steering Result = new Steering();

            // Accumulate all accelerations
            foreach (SteeringBehaviour B in Behaviours.Values) {
                Steering S = B.GetSteering(this);

                Result.Linear += B.Weight * S.Linear;
                Result.Angular += S.Angular;
            }

            return Result;
        }

        // Adds a specific Steering if it's not already contained
        public void AddSteering(SteeringType Type) {
            if (!Behaviours.ContainsKey(Type)) {
                switch(Type) {
                    case SteeringType.Align:
                        Behaviours.Add(Type, new Align());
                        break;

                    case SteeringType.AntiAlign:
                        Behaviours.Add(Type, new AntiAlign());
                        break;

                    case SteeringType.Arrive:
                        Behaviours.Add(Type, new Arrive());
                        break;

                    case SteeringType.Flee:
                        Behaviours.Add(Type, new Flee());
                        break;

                    case SteeringType.Seek:
                        Behaviours.Add(Type, new Seek());
                        break;

                    case SteeringType.VelocityMatching:
                        Behaviours.Add(Type, new VelocityMatching());
                        break;

                    case SteeringType.Evade:
                        Behaviours.Add(Type, new Evade());
                        break;

                    case SteeringType.Face:
                        Behaviours.Add(Type, new Face());
                        break;

                    case SteeringType.LookWhereYouGoing:
                        Behaviours.Add(Type, new LookWhereYouGoing());
                        break;

                    case SteeringType.Pursue:
                        Behaviours.Add(Type, new Pursue());
                        break;

                    case SteeringType.Wander:
                        Behaviours.Add(Type, new Wander());
                        break;

                    default:
                        break;
                }
            }
        }

        // Removes a specific Steering
        public void RemoveSteering(SteeringType Type) {
            if (Behaviours.ContainsKey(Type))
                Behaviours.Remove(Type);
        }

        // Sets the target for a specific Steering
        public void SetSteeringTarget(SteeringType Type, Unit Target) {
            if (Behaviours.ContainsKey(Type))
                Behaviours[Type].SetTarget(Target);
        }

        public void Draw(SpriteBatch Batch) {
            Batch.Draw(Sprite.SpriteTexture, Transform.Position, null, Sprite.SpriteColor, MathHelper.ToRadians(Transform.Rotation), 
                new Vector2(Sprite.SpriteTexture.Width / 2f, Sprite.SpriteTexture.Height / 2f), Transform.Scale, SpriteEffects.None, Sprite.Layer);
        }

        public void DestroyUnit(World World) {
            // Removes Body from World
            World.RemoveBody(Collider.Body);
        }
    }
}

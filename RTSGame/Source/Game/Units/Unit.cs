﻿using System;
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
            // TODO: Find out how to make the character stop slowly when there are no Steerings working
            // Check if the Unit is able to move
            if (Body.CanMove) {
                // Compute final steering
                Steering Final = GetBlendedSteering();

                // TODO: Is Final.Angular != 0 bad?
                if (Final.Linear.Length() > 0 || Final.Angular != 0) {

                    // Update position and orientation
                    Transform.Position += Body.Velocity * DeltaTime;
                    Transform.Rotation += Body.RotationVelocity * DeltaTime;

                    // Update velocity and rotation
                    Body.Velocity += Final.Linear * DeltaTime;
                    Body.RotationVelocity += Final.Angular * DeltaTime;

                    // Clip speed if its too high
                    if (Body.Velocity.Length() > Body.MaxVelocity)
                        Body.ClipVelocity();

                    // Update Physics Body position
                    Collider.Body.Position = ConvertUnits.ToSimUnits(Transform.Position);
                } else {
                    Body.Velocity = new Vector2(0f, 0f);
                    Body.RotationVelocity = 0f;
                }
            }
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

        // Set Unit orientation based on its velocity
        public float GetNewOrientation() {
            // Make sure we have a velocity
            if (Body.Velocity.Length() > 0)
                // Calculate orientation using arc tangent of the velocity
                return (float)Math.Atan2(Body.Velocity.Y, Body.Velocity.X);

            return Transform.Rotation;
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

        public void RemoveSteering(SteeringType Type) {
            if (Behaviours.ContainsKey(Type))
                Behaviours.Remove(Type);
        }

        public void SetSteeringTarget(SteeringType Type, Unit Target) {
            if (Behaviours.ContainsKey(Type))
                Behaviours[Type].SetTarget(Target);
        }

        public void SetSteeringWeight(SteeringType Type, int Weight) {
            if (Behaviours.ContainsKey(Type))
                Behaviours[Type].Weight = Weight;
        }

        public void Draw(SpriteBatch Batch) {
            SpriteEffects Flip = SpriteEffects.None;

            if (Body.Velocity.X >= 0.01f)
                Flip = SpriteEffects.FlipHorizontally;
            else if (Body.Velocity.X <= -0.01f)
                Flip = SpriteEffects.None;

            Batch.Draw(Sprite.SpriteTexture, Transform.Position, null, Sprite.SpriteColor, Transform.Rotation, 
                new Vector2(Sprite.SpriteTexture.Width / 2f, Sprite.SpriteTexture.Height / 2f), Transform.Scale, Flip, Sprite.Layer);
        }

        public void DestroyUnit(World World) {
            // Removes Body from World
            World.RemoveBody(Collider.Body);
        }
    }
}
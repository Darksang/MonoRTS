﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

        // Holds all movement behaviours of the Unit
        public List<SteeringBehaviour> MovementBehaviours { get; private set; }

        // Whether this Unit is selected or not
        public bool Selected { get; set; }

        public Unit(Sprite Sprite) {
            Name = "";
            Transform = new Transform();
            this.Sprite = Sprite;
            Body = new Body();

            MovementBehaviours = new List<SteeringBehaviour>();

            Selected = false;
        }

        public Unit(string Name, Sprite Sprite) {
            this.Name = Name;
            Transform = new Transform();
            this.Sprite = Sprite;
            Body = new Body();

            MovementBehaviours = new List<SteeringBehaviour>();

            Selected = false;
        }

        public void Update(float DeltaTime) {
            // Check if the Unit is able to move
            if (Body.CanMove) {
                // Compute final steering
                Steering Final = GetBlendedSteering();

                // Move the Unit
                Transform.Position += Body.Velocity * DeltaTime;
                Transform.Rotation += Body.Rotation * DeltaTime;

                // Apply Newton-Euler equations
                Body.Velocity += Final.Linear * DeltaTime;
                Body.Rotation += Final.Angular * DeltaTime;
            }
        }

        // Combine all SteeringBehaviours
        private Steering GetBlendedSteering() {
            // Create the Steering structure for accumulation
            Steering Result = new Steering();

            // Accumulate all accelerations
            foreach (SteeringBehaviour B in MovementBehaviours) {
                Steering S = B.GetSteering(this);

                Result.Linear += B.Weight * S.Linear;
                Result.Angular += S.Angular;
            }

            // TODO: Crop the result? max(Result.Linear, MaxAcceleration) max(Result.Angular, MaxRotation)

            return Result;
        }

        // Sets the target for this Unit
        public void SetUnitTarget(Unit Unit) {
            // TODO: Set target individually for each Behaviour
            foreach (SteeringBehaviour B in MovementBehaviours)
                B.SetTarget(Unit);
        }

        // TODO: Change SteeringBehaviour for an Enum, then create adecuate Steering inside
        public void AddSteering(SteeringBehaviour Behaviour) {
            // TODO: Check if it already exists
            MovementBehaviours.Add(Behaviour);
        }

        public void Draw(SpriteBatch Batch) {
            Batch.Draw(Sprite.SpriteTexture, Transform.Position, null, Sprite.SpriteColor, Transform.Rotation, 
                new Vector2(Sprite.SpriteTexture.Width / 2f, Sprite.SpriteTexture.Height / 2f), Transform.Scale, SpriteEffects.None, Sprite.Layer);
        }
    }
}

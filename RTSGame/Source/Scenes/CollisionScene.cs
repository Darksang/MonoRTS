﻿using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using PhysicsBody = FarseerPhysics.Dynamics.Body;

using ImGuiNET;

namespace RTSGame {

    public class CollisionScene : Scene {
        // Raycasting
        private Vector2 StartRay;
        private Vector2 EndRay;

        public CollisionScene(MainGame MainGame) : base(MainGame) { }

        public override void Initialize() {
            // Reset physics world
            Vector2 Gravity = new Vector2(0f, 0f);
            World = new World(Gravity);

            // Reset containers
            Units = new List<Unit>();

            // Reset camera
            Game.Camera.Position = new Vector2(-Game.Graphics.PreferredBackBufferWidth / 2f, -Game.Graphics.PreferredBackBufferHeight / 2f);
            Game.Camera.Zoom = 1f;

            StartRay = new Vector2(-200f, -200f);

            // Test Obstacle
            PhysicsBody Obstacle = BodyFactory.CreateRectangle(World, ConvertUnits.ToSimUnits(200f), ConvertUnits.ToSimUnits(200f), 1f);
            Obstacle.Position = ConvertUnits.ToSimUnits(new Vector2(0f, 0f));
            Obstacle.FixtureList[0].IsSensor = true;

            // Test Unit
            Sprite S = new Sprite(Game.Sprites["Stump"]);
            Unit U = new Unit("Collision Avoid", S, World);
            U.DrawDebugVelocity = true;
            U.Body.MaxVelocity = 100f;
            U.Transform.Position = new Vector2(-500f, 0f);
            U.Collider.Body.Position = ConvertUnits.ToSimUnits(U.Transform.Position);

            U.AddSteering(SteeringType.Wander);
            U.AddSteering(SteeringType.ObstacleAvoidance);

            Units.Add(U);
        }

        public override void Update(GameTime GameTime) {
            float DeltaTime = (float)GameTime.ElapsedGameTime.TotalSeconds;

            if (!ImGui.GetIO().WantCaptureMouse) {
                if (Game.MouseState.LeftButton == ButtonState.Pressed && Game.PreviousMouseState.LeftButton == ButtonState.Released) {
                    Vector2 MouseWorldCoords = Game.Camera.ScreenToWorld(new Vector2(Game.MouseState.X, Game.MouseState.Y));
                    EndRay = MouseWorldCoords;
                }

                if (Game.MouseState.RightButton == ButtonState.Pressed && Game.PreviousMouseState.RightButton == ButtonState.Released) {
                    Vector2 MouseWorldCoords = Game.Camera.ScreenToWorld(new Vector2(Game.MouseState.X, Game.MouseState.Y));
                    StartRay = MouseWorldCoords;
                }
            }

            if (!ImGui.GetIO().WantCaptureKeyboard) {
                if (Game.KeyboardState.IsKeyDown(Keys.Space) && Game.PreviousKeyboardState.IsKeyUp(Keys.Space)) {
                    Console.WriteLine("RayCasting");

                    bool Hit = false;

                    World.RayCast((f, p, n, fr) => {
                        Hit = true;
                        return 0;
                    }, ConvertUnits.ToSimUnits(StartRay), ConvertUnits.ToSimUnits(EndRay));

                    if (Hit)
                        Console.WriteLine("RayCast Hit");
                    else
                        Console.WriteLine("No Raycast Hit");
                }
            }

            foreach (Unit U in Units)
                U.Update(DeltaTime);

            World.Step(1f / 60f);
        }

        public override void Render(GameTime GameTime) {
            var CameraMatrix = Game.Camera.GetViewMatrix();

            Game.SpriteBatch.Begin(transformMatrix: CameraMatrix, samplerState: SamplerState.PointClamp);

            // Draw Obstacle and RayCast test line
            // NOTE: In PhysicsWorld the rectangle position is at its center, when drawing is in top left | Draw obstacles
            RectangleF R = new RectangleF(-100f, -100f, 200f, 200f);
            Game.SpriteBatch.DrawRectangle(R, Color.Green);

            Game.SpriteBatch.DrawLine(StartRay, EndRay, Color.Coral);

            Game.SpriteBatch.DrawPoint(StartRay, Color.Blue, 3f);
            Game.SpriteBatch.DrawPoint(EndRay, Color.Red, 3f);

            // Draw Units
            foreach (Unit U in Units)
                U.Draw(Game.SpriteBatch);

            Game.SpriteBatch.End();
        }

        public override void Destroy() {
            World.Clear();
            Units.Clear();
        }
    }
}
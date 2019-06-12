using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision;

using ImGuiNET;

namespace RTSGame {

    public class GroupsScene : Scene {
        // Selected group of Units
        private List<Unit> SelectedUnits;

        // Unit selection
        private Vector2 SelectionStart;
        private bool Selecting;

        // Formation
        private DefensiveCirclePattern FormationPattern;
        private FormationManager FormationManager;

        public GroupsScene(MainGame MainGame) : base(MainGame) { }

        public override void Initialize() {
            // Reset physics world
            Vector2 Gravity = new Vector2(0f, 0f);
            World = new World(Gravity);

            // Reset containers
            Units = new List<Unit>();

            // Reset camera
            Game.Camera.Position = new Vector2(-Game.Graphics.PreferredBackBufferWidth / 2f, -Game.Graphics.PreferredBackBufferHeight / 2f);
            Game.Camera.Zoom = 1f;

            SelectedUnits = new List<Unit>();
            Selecting = false;

            FormationPattern = new DefensiveCirclePattern(50f);
            FormationManager = new FormationManager(FormationPattern);

            /* Unit 1
            Sprite S = new Sprite(Game.Sprites["Ghost"]);
            Unit U1 = new Unit("Unit 1", S, World);
            U1.Transform.Position = new Vector2(0f, 0f);
            U1.Collider.Body.Position = ConvertUnits.ToSimUnits(U1.Transform.Position);
            U1.DrawDebugVelocity = true;

            U1.AddSteering(SteeringType.Alignment);
            U1.AddSteering(SteeringType.Cohesion);
            U1.AddSteering(SteeringType.Separation);
            U1.AddSteering(SteeringType.MoveToPosition);
            U1.SetSteeringWeight(SteeringType.Separation, 4);
            //U1.AddSteering(SteeringType.Wander);

            Unit U2 = new Unit("Unit 2", S, World);
            U2.Transform.Position = new Vector2(120f, 120f);
            U2.Collider.Body.Position = ConvertUnits.ToSimUnits(U2.Transform.Position);
            U2.DrawDebugVelocity = true;

            U2.AddSteering(SteeringType.Alignment);
            U2.AddSteering(SteeringType.Cohesion);
            U2.AddSteering(SteeringType.Separation);
            U2.AddSteering(SteeringType.MoveToPosition);
            U2.SetSteeringWeight(SteeringType.Separation, 4);
            //U2.AddSteering(SteeringType.Wander);

            Unit U3 = new Unit("Unit 3", S, World);
            U3.Transform.Position = new Vector2(-120f, -50f);
            U3.Collider.Body.Position = ConvertUnits.ToSimUnits(U3.Transform.Position);
            U3.DrawDebugVelocity = true;

            U3.AddSteering(SteeringType.Alignment);
            U3.AddSteering(SteeringType.Cohesion);
            U3.AddSteering(SteeringType.Separation);
            U3.AddSteering(SteeringType.MoveToPosition);
            U3.SetSteeringWeight(SteeringType.Separation, 4);
            //U3.AddSteering(SteeringType.Wander);

            Units.Add(U1);
            Units.Add(U2);
            Units.Add(U3); */
        }

        public override void Update(GameTime GameTime) {
            float DeltaTime = (float)GameTime.ElapsedGameTime.TotalSeconds;

            // Process input
            AreaSelection();

            /*if (!ImGui.GetIO().WantCaptureKeyboard) {
                if (Game.KeyboardState.IsKeyDown(Keys.F) && Game.PreviousKeyboardState.IsKeyDown(Keys.F)) {
                    if (SelectedUnits.Count > 0)
                        foreach (Unit U in SelectedUnits)
                            FormationManager.AddUnit(U);
                }
            } */

            /* Move selected units with right click
            if (!ImGui.GetIO().WantCaptureMouse) {
                if (Game.MouseState.RightButton == ButtonState.Pressed && Game.PreviousMouseState.RightButton == ButtonState.Released) {
                    Vector2 WorldPosition = Game.Camera.ScreenToWorld(new Vector2(Game.MouseState.X, Game.MouseState.Y));
                    if (SelectedUnits.Count > 0) {
                        foreach (Unit U in SelectedUnits) {
                            U.MoveToPosition(WorldPosition);
                        }
                    }
                }
            } */

            /* TODO: How to make a group flock and stop flocking?
            if (Game.KeyboardState.IsKeyDown(Keys.F) && Game.PreviousKeyboardState.IsKeyUp(Keys.F)) {
                foreach (Unit U in SelectedUnits) {
                    U.SetGroupTarget(SelectedUnits);
                }
            } */

            // Update units
            foreach (Unit U in Units)
                U.Update(DeltaTime);

            // Update physics
            World.Step(1f / 60f);
        }

        public override void Render(GameTime GameTime) {
            var CameraMatrix = Game.Camera.GetViewMatrix();

            // Batch -> Normal sprites without effects
            Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: CameraMatrix);

            foreach (Unit U in Units)
                if (!U.Selected)
                    U.Draw(Game.SpriteBatch);

            Game.SpriteBatch.End();

            // Batch -> Sprites with Outline shader
            Vector4 OutlineColor = Color.Crimson.ToVector4();
            Game.Effects["Outline"].Parameters["OutlineColor"].SetValue(OutlineColor);

            Game.SpriteBatch.Begin(sortMode: SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp,
                transformMatrix: CameraMatrix, effect: Game.Effects["Outline"]);

            foreach (Unit U in Units)
                if (U.Selected) {
                    Vector2 TextureRes = new Vector2(U.Sprite.SpriteTexture.Width, U.Sprite.SpriteTexture.Height);
                    Game.Effects["Outline"].Parameters["TextureRes"].SetValue(TextureRes);
                    U.Draw(Game.SpriteBatch);
                }

            Game.SpriteBatch.End();

            // Draw selection rectangle
            if (Selecting) {
                Game.SpriteBatch.Begin();
                Size2 RectSize = new Size2(Math.Abs(Game.MouseState.X - SelectionStart.X), Math.Abs(Game.MouseState.Y - SelectionStart.Y));
                Point2 Origin = new Point2(Math.Min(SelectionStart.X, Game.MouseState.X), Math.Min(SelectionStart.Y, Game.MouseState.Y));
                Game.SpriteBatch.DrawRectangle(new RectangleF(Origin, RectSize), Color.Green);
                Game.SpriteBatch.End();
            }
        }

        public override void Destroy() {
            World.Clear();
            Units.Clear();
        }

        private void AreaSelection() {
            if (!ImGui.GetIO().WantCaptureMouse) {
                // Check if we have to start selecting wiht the mouse
                if (Game.MouseState.LeftButton == ButtonState.Pressed && !Selecting) {
                    // If we move the mouse without releasing the button we start selecting
                    if ((Game.MouseState.X != Game.PreviousMouseState.X) && (Game.MouseState.Y != Game.PreviousMouseState.Y)) {
                        Selecting = true;
                        SelectionStart = new Vector2(Game.MouseState.X, Game.MouseState.Y);
                    }
                }

                // Deselect everything with a single click
                if (Game.MouseState.LeftButton == ButtonState.Released && Game.PreviousMouseState.LeftButton == ButtonState.Pressed && !Selecting) {
                    if (SelectedUnits.Count != 0) {
                        foreach (Unit U in SelectedUnits)
                            U.Selected = false;

                        SelectedUnits.Clear();
                    }
                }
            }

            // Stop selecting with mouse when the left click is released
            if (Game.MouseState.LeftButton == ButtonState.Released && Game.PreviousMouseState.LeftButton == ButtonState.Pressed && Selecting) {
                // Compute area to test in world units, then convert it to simulation units
                Vector2 AABBMin = ConvertUnits.ToSimUnits(Game.Camera.ScreenToWorld(new Vector2(Math.Min(SelectionStart.X, Game.MouseState.X), Math.Min(SelectionStart.Y, Game.MouseState.Y))));
                Vector2 AABBMax = ConvertUnits.ToSimUnits(Game.Camera.ScreenToWorld(new Vector2(Math.Max(SelectionStart.X, Game.MouseState.X), Math.Max(SelectionStart.Y, Game.MouseState.Y))));
                AABB TestArea = new AABB(AABBMin, AABBMax);
                List<Fixture> Collisions = World.QueryAABB(ref TestArea);

                // No collision
                if (Collisions.Count == 0) {
                    // If there was a group of units selected, deselect it
                    if (SelectedUnits.Count != 0) {
                        foreach (Unit U in SelectedUnits)
                            U.Selected = false;

                        SelectedUnits.Clear();
                    }
                } else {
                    // If there was a group of units selected, deselect it
                    if (SelectedUnits.Count != 0)
                        foreach (Unit U in SelectedUnits)
                            U.Selected = false;

                    // Select new Units
                    SelectedUnits.Clear();
                    foreach (Fixture F in Collisions) {
                        Unit U = (Unit)F.Body.UserData;
                        U.Selected = true;
                        SelectedUnits.Add(U);
                    }
                }

                Selecting = false;
            }
        }
    }
}

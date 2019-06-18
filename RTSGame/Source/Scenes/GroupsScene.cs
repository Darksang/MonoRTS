using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision;

using ImGuiNET;

namespace RTSGame {

    public class GroupsScene : Scene {
        // Tilemap of the scene
        private TiledMap Map;
        // Tilemap renderer
        private TiledMapRenderer MapRenderer;

        // Pathfinding
        private Grid PathfindingGrid;
        private Pathfinding Pathfinder;

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
            Game.Camera.Position = new Vector2(-300f, -150f);
            Game.Camera.Zoom = 1.5f;

            // Load map and its renderer
            Map = Game.Maps["GroupsMap"];
            MapRenderer = new TiledMapRenderer(Game.GraphicsDevice);

            // Create the pathfinding grid based on the tilemap Height, Width and TileHeight
            PathfindingGrid = new Grid(Map);
            Pathfinder = new Pathfinding(PathfindingGrid);

            SelectedUnits = new List<Unit>();
            Selecting = false;

            FormationPattern = new DefensiveCirclePattern(50f);
            FormationManager = new FormationManager(FormationPattern);

            // Unit 1
            Sprite S = new Sprite(Game.Sprites["Cat"]);
            Unit U1 = new Unit("Unit 1", S, World, new Vector2(0.80f, 0.80f));
            U1.Transform.Position = new Vector2(150f, 150f);
            U1.Collider.Body.Position = ConvertUnits.ToSimUnits(U1.Transform.Position);
            U1.DrawDebugVelocity = true;

            U1.AddSteering(SteeringType.Alignment);
            U1.AddSteering(SteeringType.Cohesion);
            U1.AddSteering(SteeringType.Separation);
            //U1.AddSteering(SteeringType.PathFollowing);
            U1.SetSteeringWeight(SteeringType.Separation, 20);
            //U1.SetSteeringWeight(SteeringType.PathFollowing, 10);

            Unit U2 = new Unit("Unit 2", S, World, new Vector2(0.80f, 0.80f));
            U2.Transform.Position = new Vector2(220f, 220f);
            U2.Collider.Body.Position = ConvertUnits.ToSimUnits(U2.Transform.Position);
            U2.DrawDebugVelocity = true;

            U2.AddSteering(SteeringType.Alignment);
            U2.AddSteering(SteeringType.Cohesion);
            U2.AddSteering(SteeringType.Separation);
            //U2.AddSteering(SteeringType.PathFollowing);
            U2.SetSteeringWeight(SteeringType.Separation, 20);
            //U2.SetSteeringWeight(SteeringType.PathFollowing, 10);

            Unit U3 = new Unit("Unit 3", S, World, new Vector2(0.80f, 0.80f));
            U3.Transform.Position = new Vector2(280f, 170f);
            U3.Collider.Body.Position = ConvertUnits.ToSimUnits(U3.Transform.Position);
            U3.DrawDebugVelocity = true;

            U3.AddSteering(SteeringType.Alignment);
            U3.AddSteering(SteeringType.Cohesion);
            U3.AddSteering(SteeringType.Separation);
            //U3.AddSteering(SteeringType.PathFollowing);
            U3.SetSteeringWeight(SteeringType.Separation, 20);
            //U3.SetSteeringWeight(SteeringType.PathFollowing, 10);

            Units.Add(U1);
            Units.Add(U2);
            Units.Add(U3);
        }

        public override void Update(GameTime GameTime) {
            float DeltaTime = (float)GameTime.ElapsedGameTime.TotalSeconds;

            // Process input
            AreaSelection();

            if (!ImGui.GetIO().WantCaptureKeyboard) {
                // Flocking
                if (Game.KeyboardState.IsKeyDown(Keys.F) && Game.PreviousKeyboardState.IsKeyUp(Keys.F)) {
                    foreach (Unit U in SelectedUnits) {
                        List<Unit> Targets = new List<Unit>();

                        foreach (Unit Target in SelectedUnits)
                            if (U != Target)
                                Targets.Add(Target);

                        U.SetGroupTarget(Targets);
                    }
                }

                // Formation pattern
                if (Game.KeyboardState.IsKeyDown(Keys.Q) && Game.PreviousKeyboardState.IsKeyDown(Keys.Q)) {
                    if (SelectedUnits.Count > 0)
                        foreach (Unit U in SelectedUnits)
                            FormationManager.AddUnit(U);
                }
            }

            // Move selected units with right click
            if (!ImGui.GetIO().WantCaptureMouse) {
                if (Game.MouseState.RightButton == ButtonState.Pressed && Game.PreviousMouseState.RightButton == ButtonState.Released) {
                    Vector2 MouseWorldCoords = Game.Camera.ScreenToWorld(new Vector2(Game.MouseState.X, Game.MouseState.Y));
                    if (SelectedUnits.Count > 0) {
                        foreach (Unit U in SelectedUnits)
                            U.ClearPath();

                        // Calculate path
                        Path TargetPath = Pathfinder.FindPath(SelectedUnits[0].Transform.Position, MouseWorldCoords);
                        SelectedUnits[0].Move(TargetPath);
                    }
                }
            }

            MapRenderer.Update(Map, GameTime);

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
            
            // Draw Map
            MapRenderer.Draw(Map, CameraMatrix);

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
                Game.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
                Size2 RectSize = new Size2(Math.Abs(Game.MouseState.X - SelectionStart.X), Math.Abs(Game.MouseState.Y - SelectionStart.Y));
                Point2 Origin = new Point2(Math.Min(SelectionStart.X, Game.MouseState.X), Math.Min(SelectionStart.Y, Game.MouseState.Y));
                Game.SpriteBatch.DrawRectangle(new RectangleF(Origin, RectSize), Color.Green);
                Game.SpriteBatch.End();
            }

            DisplayEditor();
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

        #region ImGuiEditor

        private void DisplayEditor() {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0f, 200f), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(230f, 200f), ImGuiCond.Always);
            ImGuiWindowFlags Flags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse;

            Vector4 TextColor = Color.Lime.ToVector4();
            Vector4 TextColor2 = Color.Coral.ToVector4();

            ImGui.Begin("Groups", Flags);

            ImGui.Text("With a group of Units selected,");
            ImGui.Text("press"); ImGui.SameLine();
            ImGui.TextColored(new System.Numerics.Vector4(TextColor.X, TextColor.Y, TextColor.Z, TextColor.W), "F key");
            ImGui.SameLine(); ImGui.Text("to make them flock.");

            ImGui.Separator();

            ImGui.Text("With a group of Units selected,");
            ImGui.Text("press"); ImGui.SameLine();
            ImGui.TextColored(new System.Numerics.Vector4(TextColor.X, TextColor.Y, TextColor.Z, TextColor.W), "Q key");
            ImGui.SameLine(); ImGui.Text("to make a"); ImGui.SameLine();
            ImGui.TextColored(new System.Numerics.Vector4(TextColor2.X, TextColor2.Y, TextColor2.Z, TextColor2.W), "circle");
            ImGui.TextColored(new System.Numerics.Vector4(TextColor2.X, TextColor2.Y, TextColor2.Z, TextColor2.W), "pattern.");

            ImGui.End();
        }

        #endregion
    }
}

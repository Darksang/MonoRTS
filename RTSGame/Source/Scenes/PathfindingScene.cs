using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;

using FarseerPhysics.Dynamics;

using ImGuiNET;

namespace RTSGame {
    // TODO: Add not walkable tiles to see how it calculates the path
    public class PathfindingScene : Scene {
        // Tilemap of the scene
        private TiledMap Map;
        // Tilemap renderer
        private TiledMapRenderer MapRenderer;

        // Pathfinding
        private Grid PathfindingGrid;
        private Pathfinding Pathfinder;

        // Pathfinding test
        private Vector2 End;
        private Path Path;
        private Unit Unit;

        // Debug stuff
        private bool DrawDebugGrid;
        private Texture2D DebugGridTexture;

        public PathfindingScene(MainGame MainGame) : base(MainGame) { }

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
            Map = Game.Maps["PathfindingMap"];
            MapRenderer = new TiledMapRenderer(Game.GraphicsDevice);

            // Create the pathfinding grid based on the tilemap Height, Width and TileHeight
            PathfindingGrid = new Grid(Map);
            Pathfinder = new Pathfinding(PathfindingGrid);

            End = new Vector2(400f, 240f);
            Path = new Path();

            // Test Unit
            Sprite S = new Sprite(Game.Sprites["Ghost"]);
            Unit = new Unit("Pathfinder", S, World);
            Unit.Transform.Scale = new Vector2(0.5f, 0.5f);
            Unit.Body.MaxVelocity = 50f;

            Unit.AddSteering(SteeringType.PathFollowing);

            Path = Pathfinder.FindPath(Unit.Transform.Position, End);
            Unit.SetPath(Path);

            Units.Add(Unit);

            // Debug
            DrawDebugGrid = true;
            DebugGridTexture = new Texture2D(Game.GraphicsDevice, 1, 1);
            DebugGridTexture.SetData(new Color[] { Color.White });
        }

        public override void Update(GameTime GameTime) {
            float DeltaTime = (float)GameTime.ElapsedGameTime.TotalSeconds;

            // Switch debug draw with D
            if (!ImGui.GetIO().WantCaptureKeyboard) {
                if (Game.KeyboardState.IsKeyDown(Keys.D) && Game.PreviousKeyboardState.IsKeyUp(Keys.D))
                    DrawDebugGrid = !DrawDebugGrid;
            }

            // Calculate a path using mouse clicks
            if (!ImGui.GetIO().WantCaptureMouse) {
                // Change end position for pathfinding
                if (Game.MouseState.LeftButton == ButtonState.Pressed && Game.PreviousMouseState.LeftButton == ButtonState.Released) {
                    Vector2 MouseWorldCoords = Game.Camera.ScreenToWorld(new Vector2(Game.MouseState.X, Game.MouseState.Y));
                    End = MouseWorldCoords;

                    // Calculate path
                    Path = Pathfinder.FindPath(Unit.Transform.Position, End);
                    Unit.SetPath(Path);
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

            Game.SpriteBatch.Begin(transformMatrix: CameraMatrix, samplerState: SamplerState.PointClamp);

            // Draw Map
            MapRenderer.Draw(Map, CameraMatrix);

            if (DrawDebugGrid) {
                // Draw Debug Grid
                for (int x = 0; x <= Map.Width; x++) {
                    Rectangle Rect = new Rectangle(0 + x * Map.TileWidth, 0, 1, Map.WidthInPixels);
                    Game.SpriteBatch.Draw(DebugGridTexture, Rect, Color.Black);
                }

                for (int y = 0; y <= Map.Height; y++) {
                    Rectangle Rect = new Rectangle(0, 0 + y * Map.TileWidth, Map.HeightInPixels, 1);
                    Game.SpriteBatch.Draw(DebugGridTexture, Rect, Color.Black);
                }

                // Draw Nodes
                for (int x = 0; x < Map.Width; x++) {
                    for (int y = 0; y < Map.Height; y++) {
                        if (PathfindingGrid.Nodes[x, y].Walkable)
                            Game.SpriteBatch.DrawPoint(PathfindingGrid.Nodes[x, y].WorldPosition, Color.Blue, 3);
                        else
                            Game.SpriteBatch.DrawPoint(PathfindingGrid.Nodes[x, y].WorldPosition, Color.Red, 3);
                    }
                }

                // Draw calculated Path
                Vector2 Current = Vector2.Zero;
                foreach (Vector2 P in Path.Positions) {
                    if (Current != Vector2.Zero) {
                        Game.SpriteBatch.DrawLine(P, Current, Color.Coral, 2f);
                        Current = P;
                    }

                    Current = P;
                }
            }

            foreach (Unit U in Units)
                U.Draw(Game.SpriteBatch);

            Game.SpriteBatch.End();

            DisplayEditor();
        }

        public override void Destroy() {
            World.Clear();
            Units.Clear();
        }

        private void DisplayEditor() {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(Game.Graphics.PreferredBackBufferWidth - 330f, 20f), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(330f, 90f), ImGuiCond.Always);
            ImGuiWindowFlags Flags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse;

            ImGui.Begin("Pathfinding", Flags);

            Vector4 TextColor = Color.Red.ToVector4();

            ImGui.BulletText("Use"); ImGui.SameLine();
            ImGui.TextColored(new System.Numerics.Vector4(TextColor.X, TextColor.Y, TextColor.Z, TextColor.W), "Left Click"); ImGui.SameLine();
            ImGui.Text("to set the end point.");

            ImGui.Separator();

            float EndX = (float)System.Math.Floor(End.X / PathfindingGrid.TileSize);
            float EndY = (float)System.Math.Floor(End.Y / PathfindingGrid.TileSize);
            ImGui.BulletText("End Point: " + End);
            ImGui.BulletText("End Grid Position: [" + (int)EndX + ", " + (int)EndY + "]");

            ImGui.End();
        }
    }
}

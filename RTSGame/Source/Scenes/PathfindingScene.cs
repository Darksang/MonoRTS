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

    public class PathfindingScene : Scene {
        // Tilemap of the scene
        private TiledMap Map;
        // Tilemap renderer
        private TiledMapRenderer MapRenderer;

        // Pathfinding
        private Grid PathfindingGrid;
        private Pathfinding Pathfinder;

        // Pathfinding test
        private Vector2 Start;
        private Vector2 End;
        private Path Path;

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
            Game.Camera.Position = new Vector2(0f, 0f);
            Game.Camera.Zoom = 1f;

            // Load map and its renderer
            Map = Game.Maps["PathfindingMap"];
            MapRenderer = new TiledMapRenderer(Game.GraphicsDevice);

            // Create the pathfinding grid based on the tilemap Height, Width and TileHeight
            PathfindingGrid = new Grid(Map.Width, Map.Height, Map.TileHeight);
            Pathfinder = new Pathfinding(PathfindingGrid);

            Start = new Vector2(16f, 16f);
            End = new Vector2(48f, 48f);
            Path = new Path();

            Path = Pathfinder.FindPath(Start, End);

            // Debug
            DrawDebugGrid = true;
            DebugGridTexture = new Texture2D(Game.GraphicsDevice, 1, 1);
            DebugGridTexture.SetData(new Color[] { Color.White });
        }

        public override void Update(GameTime GameTime) {
            // Switch debug draw with D
            if (!ImGui.GetIO().WantCaptureKeyboard) {
                if (Game.KeyboardState.IsKeyDown(Keys.D) && Game.PreviousKeyboardState.IsKeyUp(Keys.D))
                    DrawDebugGrid = !DrawDebugGrid;
            }

            // Calculate a path clicking
            if (!ImGui.GetIO().WantCaptureMouse) {
                // Change end position for pathfinding
                if (Game.MouseState.LeftButton == ButtonState.Pressed && Game.PreviousMouseState.LeftButton == ButtonState.Released) {
                    Vector2 MouseWorldCoords = Game.Camera.ScreenToWorld(new Vector2(Game.MouseState.X, Game.MouseState.Y));
                    End = MouseWorldCoords;

                    // Calculate path
                    Path = Pathfinder.FindPath(Start, End);
                }

                // Change start position for pathfinding
                if (Game.MouseState.RightButton == ButtonState.Pressed && Game.PreviousMouseState.RightButton == ButtonState.Released) {
                    Vector2 MouseWorldCoords = Game.Camera.ScreenToWorld(new Vector2(Game.MouseState.X, Game.MouseState.Y));
                    Start = MouseWorldCoords;
                }
            }

            MapRenderer.Update(Map, GameTime);

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
                    Game.SpriteBatch.Draw(DebugGridTexture, Rect, Color.Red);
                }

                for (int y = 0; y <= Map.Height; y++) {
                    Rectangle Rect = new Rectangle(0, 0 + y * Map.TileWidth, Map.HeightInPixels, 1);
                    Game.SpriteBatch.Draw(DebugGridTexture, Rect, Color.Red);
                }

                // Draw Nodes
                for (int x = 0; x < Map.Width; x++) {
                    for (int y = 0; y < Map.Height; y++) {
                        if (PathfindingGrid.Nodes[x, y].Walkable)
                            Game.SpriteBatch.DrawPoint(PathfindingGrid.Nodes[x, y].WorldPosition, Color.Black, 3);
                        else
                            Game.SpriteBatch.DrawPoint(PathfindingGrid.Nodes[x, y].WorldPosition, Color.Red, 3);
                    }
                }

                // Draw calculated Path
                Vector2 Current = Vector2.Zero;
                foreach (Vector2 P in Path.Positions) {
                    if (Current != Vector2.Zero) {
                        Game.SpriteBatch.DrawLine(P, Current, Color.Green, 2f);
                        Current = P;
                    }

                    Current = P;
                }
            }

            // Draw Units
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
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0f, 200f), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(210f, 200f), ImGuiCond.Always);
            ImGuiWindowFlags Flags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse;

            ImGui.Begin("Pathfinding", Flags);

            ImGui.End();
        }
    }
}

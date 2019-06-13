using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using PhysicsBody = FarseerPhysics.Dynamics.Body;

using ImGuiNET;

namespace RTSGame {

    public class MainScene : Scene {
        // Tilemap of the scene
        private TiledMap Map;
        // Tilemap renderer
        private TiledMapRenderer MapRenderer;

        // Pathfinding
        private Grid PathfindingGrid;
        private Pathfinding Pathfinder;

        // Debug
        private bool DrawDebugGrid;
        private Texture2D DebugGridTexture;

        public MainScene(MainGame MainGame) : base(MainGame) { }

        public override void Initialize() {
            // Reset physics world
            Vector2 Gravity = new Vector2(0f, 0f);
            World = new World(Gravity);

            // Reset containers
            Units = new List<Unit>();

            // Reset camera
            Game.Camera.Position = new Vector2(-Game.Graphics.PreferredBackBufferWidth / 2f, -Game.Graphics.PreferredBackBufferHeight / 2f);
            Game.Camera.Zoom = 1f;

            // Load map and its renderer
            Map = Game.Maps["MainMap"];
            MapRenderer = new TiledMapRenderer(Game.GraphicsDevice);

            // Generate map obstacles
            if (Map.ObjectLayers.Count != 0) {
                foreach (TiledMapObject O in Map.ObjectLayers[0].Objects) {
                    // Create a static body in the physics world
                    PhysicsBody B = BodyFactory.CreateRectangle(World, ConvertUnits.ToSimUnits(O.Size.Width), ConvertUnits.ToSimUnits(O.Size.Height), 1f);
                    Vector2 Pos = new Vector2(O.Position.X + O.Size.Width / 2f, O.Position.Y + O.Size.Height / 2f);
                    B.Position = ConvertUnits.ToSimUnits(Pos);
                    B.FixtureList[0].IsSensor = true;
                }
            }

            // Create the pathfinding grid based on the tilemap Height, Width and TileHeight
            PathfindingGrid = new Grid(Map);
            Pathfinder = new Pathfinding(PathfindingGrid);

            // Debug
            DrawDebugGrid = false;
            DebugGridTexture = new Texture2D(Game.GraphicsDevice, 1, 1);
            DebugGridTexture.SetData(new Color[] { Color.White });
        }

        public override void Update(GameTime GameTime) {
            float DeltaTime = (float)GameTime.ElapsedGameTime.TotalSeconds;

            ClampCamera();

            // Switch debug draw with D
            if (!ImGui.GetIO().WantCaptureKeyboard) {
                if (Game.KeyboardState.IsKeyDown(Keys.D) && Game.PreviousKeyboardState.IsKeyUp(Keys.D))
                    DrawDebugGrid = !DrawDebugGrid;
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
            }

            foreach (Unit U in Units)
                U.Draw(Game.SpriteBatch);

            Game.SpriteBatch.End();
        }

        public override void Destroy() {
            World.Clear();
            Units.Clear();
        }

        // Clamps the Camera to the Map
        private void ClampCamera() {
            Game.Camera.Zoom = 1f;

            if (Game.Camera.Position.Y < 0f)
                Game.Camera.Position = new Vector2(Game.Camera.Position.X, 0f);

            if (Game.Camera.Position.Y > Map.HeightInPixels - Game.Graphics.PreferredBackBufferHeight)
                Game.Camera.Position = new Vector2(Game.Camera.Position.X, Map.HeightInPixels - Game.Graphics.PreferredBackBufferHeight);

            if (Game.Camera.Position.X < 0f)
                Game.Camera.Position = new Vector2(0f, Game.Camera.Position.Y);

            if (Game.Camera.Position.X > Map.WidthInPixels - Game.Graphics.PreferredBackBufferWidth)
                Game.Camera.Position = new Vector2(Map.WidthInPixels - Game.Graphics.PreferredBackBufferWidth, Game.Camera.Position.Y);
        }
    }
}

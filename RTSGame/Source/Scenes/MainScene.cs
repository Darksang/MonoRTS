using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;
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

        // Unit selection
        private Unit SelectedUnit;

        private bool Selecting;

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
                    if (O.Type == "Rectangle") {
                        // Create a static body in the physics world
                        PhysicsBody B = BodyFactory.CreateRectangle(World, ConvertUnits.ToSimUnits(O.Size.Width), ConvertUnits.ToSimUnits(O.Size.Height), 1f);
                        Vector2 Pos = new Vector2(O.Position.X + O.Size.Width / 2f, O.Position.Y + O.Size.Height / 2f);
                        B.Position = ConvertUnits.ToSimUnits(Pos);
                        B.FixtureList[0].IsSensor = true;
                    }
                }
            }

            // Create the pathfinding grid based on the tilemap Height, Width and TileHeight
            PathfindingGrid = new Grid(Map);
            Pathfinder = new Pathfinding(PathfindingGrid);

            SelectedUnit = null;
            Selecting = false;

            // Debug
            DrawDebugGrid = false;
            DebugGridTexture = new Texture2D(Game.GraphicsDevice, 1, 1);
            DebugGridTexture.SetData(new Color[] { Color.White });

            // Setup Game
            Sprite Stump = new Sprite(Game.Content.Load<Texture2D>("Sprites//Stump"));
            Sprite Ghost = new Sprite(Game.Content.Load<Texture2D>("Sprites//Ghost"));
            Sprite Cat = new Sprite(Game.Content.Load<Texture2D>("Sprites//Cat"));
            Sprite Healer = new Sprite(Game.Content.Load<Texture2D>("Sprites//Healer"));
            Sprite Assassin = new Sprite(Game.Content.Load<Texture2D>("Sprites//Assassin"));

            AssassinUnit U = new AssassinUnit("Unit 1", Assassin, World, new Vector2(0.55f, 0.55f));
            U.Transform.Position = new Vector2(800f, 800f);
            U.Collider.Body.Position = ConvertUnits.ToSimUnits(U.Transform.Position);
            U.AddSteering(SteeringType.PathFollowing);

            LeaderUnit L = new LeaderUnit("Leader", Cat, World, new Vector2(0.55f, 0.55f));
            L.Transform.Position = new Vector2(600f, 800f);
            L.Collider.Body.Position = ConvertUnits.ToSimUnits(L.Transform.Position);
            L.AddSteering(SteeringType.PathFollowing);

            // Generate Leaders
            if (Map.ObjectLayers.Count != 0) {
                foreach (TiledMapObject O in Map.ObjectLayers[0].Objects) {
                    if (O.Type == "Point") {
                        LeaderUnit Leader = new LeaderUnit(O.Name, Healer, World, new Vector2(0.55f, 0.55f));
                        Leader.Transform.Position = O.Position;
                        Leader.Collider.Body.Position = ConvertUnits.ToSimUnits(Leader.Transform.Position);
                        Leader.AddSteering(SteeringType.PathFollowing);
                        Units.Add(Leader);
                    }
                }
            }

            Units.Add(U);
            Units.Add(L);
        }

        public override void Update(GameTime GameTime) {
            float DeltaTime = (float)GameTime.ElapsedGameTime.TotalSeconds;

            // Process Input
            SingleSelection();

            // Switch debug draw with D
            if (!ImGui.GetIO().WantCaptureKeyboard) {
                if (Game.KeyboardState.IsKeyDown(Keys.D) && Game.PreviousKeyboardState.IsKeyUp(Keys.D))
                    DrawDebugGrid = !DrawDebugGrid;
            }

            if (!ImGui.GetIO().WantCaptureMouse) {
                // Order a selected Unit
                if (Game.MouseState.RightButton == ButtonState.Pressed && Game.PreviousMouseState.RightButton == ButtonState.Released && SelectedUnit != null) {
                    // Check if it has to attack or move
                    Vector2 MouseWorldCoords = Game.Camera.ScreenToWorld(new Vector2(Game.MouseState.X, Game.MouseState.Y));
                    Fixture Test = World.TestPoint(ConvertUnits.ToSimUnits(MouseWorldCoords));

                    // If we find another Unit, attack it
                    if (Test != null) {
                        Unit Collided = (Unit)Test.Body.UserData;
                        SelectedUnit.Attack(Collided);
                    } else {
                        // Calculate path
                        Path TargetPath = Pathfinder.FindPath(SelectedUnit.Transform.Position, MouseWorldCoords);
                        SelectedUnit.Move(TargetPath);
                    }
                }
            }

            ClampCamera();

            // Update Unit speed depending on terrain
            foreach (Unit U in Units) {
                // Get the tile the Unit is standing on
                Node N = PathfindingGrid.WorldToNode(U.Transform.Position);
                if (N != null) {
                    Map.TileLayers[0].TryGetTile(N.GridX, N.GridY, out TiledMapTile? Tile);
                    if (Tile != null) {
                        // Find out terrain type
                        TerrainType T = (TerrainType)Tile.Value.GlobalIdentifier;
                        if (Enum.IsDefined(typeof(TerrainType), Tile.Value.GlobalIdentifier))
                            U.ChangeVelocity(T);
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

            // Draw not selected Units
            foreach (Unit U in Units) {
                if (!U.Selected)
                    U.Draw(Game.SpriteBatch);

                Game.SpriteBatch.DrawString(Game.Fonts["Arial"], U.Stats.Health.ToString(),
                    new Vector2(U.Transform.Position.X - (U.Sprite.SpriteTexture.Width / 2f), U.Transform.Position.Y - U.Sprite.SpriteTexture.Height / 2f - 20f), Color.Black);
            }

            Game.SpriteBatch.End();

            // Batch -> Sprites with Outline shader
            Vector4 OutlineColor = Color.Crimson.ToVector4();
            Game.Effects["Outline"].Parameters["OutlineColor"].SetValue(OutlineColor);

            Game.SpriteBatch.Begin(sortMode: SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp,
                transformMatrix: CameraMatrix, effect: Game.Effects["Outline"]);

            // Draw selected Units
            foreach (Unit U in Units)
                if (U.Selected) {
                    Vector2 TextureRes = new Vector2(U.Sprite.SpriteTexture.Width, U.Sprite.SpriteTexture.Height);
                    Game.Effects["Outline"].Parameters["TextureRes"].SetValue(TextureRes);
                    U.Draw(Game.SpriteBatch);
                }

            Game.SpriteBatch.End();

            // TODO: Draw Minimap
            Game.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

            float MapWidth = 150f;
            float MapHeight = 150f;
            float ScreenWidth = Game.Graphics.PreferredBackBufferWidth;
            float ScreenHeight = Game.Graphics.PreferredBackBufferHeight;

            Vector2 MapPosition = new Vector2(ScreenWidth - 180f, ScreenHeight - 180f);

            Vector2 CameraWorldToMap = new Vector2(Game.Camera.Position.X * MapWidth / 2f / ScreenWidth, Game.Camera.Position.Y * MapHeight / 2f / ScreenHeight);

            Game.SpriteBatch.FillRectangle(new RectangleF(MapPosition, new Size2(MapWidth, MapHeight)), Color.Black);
            Game.SpriteBatch.DrawRectangle(new RectangleF(MapPosition, new Size2(MapWidth, MapHeight)), Color.White);

            Game.SpriteBatch.DrawRectangle(new RectangleF(CameraWorldToMap + MapPosition, new Size2(40f, 40f)), Color.Blue);

            Game.SpriteBatch.End();

            DisplayEditor();
        }

        public override void Destroy() {
            World.Clear();
            Units.Clear();
        }

        // Selects a single Unit
        private void SingleSelection() {
            if (!ImGui.GetIO().WantCaptureMouse) {
                if (Game.MouseState.LeftButton == ButtonState.Released && Game.PreviousMouseState.LeftButton == ButtonState.Pressed && !Selecting) {
                    Vector2 MouseWorldCoords = Game.Camera.ScreenToWorld(new Vector2(Game.MouseState.X, Game.MouseState.Y));
                    Fixture Test = World.TestPoint(ConvertUnits.ToSimUnits(MouseWorldCoords));

                    // If we retreive a fixture, select its unit
                    if (Test != null) {
                        if (SelectedUnit != null)
                            SelectedUnit.Selected = false;

                        Unit Collided = (Unit)Test.Body.UserData;
                        SelectedUnit = Collided;
                        Collided.Selected = true;
                    } else if (SelectedUnit != null) {
                        // If we don't retreive a fixture, and there was a selected unit, deselect it
                        SelectedUnit.Selected = false;
                        SelectedUnit = null;
                    }
                }
            }
        }

        // Clamps the Camera to the Map
        private void ClampCamera() {
            //Game.Camera.Zoom = 1f;

            if (Game.Camera.Position.Y < 0f)
                Game.Camera.Position = new Vector2(Game.Camera.Position.X, 0f);

            if (Game.Camera.Position.Y > Map.HeightInPixels - Game.Graphics.PreferredBackBufferHeight)
                Game.Camera.Position = new Vector2(Game.Camera.Position.X, Map.HeightInPixels - Game.Graphics.PreferredBackBufferHeight);

            if (Game.Camera.Position.X < 0f)
                Game.Camera.Position = new Vector2(0f, Game.Camera.Position.Y);

            if (Game.Camera.Position.X > Map.WidthInPixels - Game.Graphics.PreferredBackBufferWidth)
                Game.Camera.Position = new Vector2(Map.WidthInPixels - Game.Graphics.PreferredBackBufferWidth, Game.Camera.Position.Y);
        }

        #region ImGuiEditor

        private void DisplayEditor() {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0f, 200f), ImGuiCond.Once);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(210f, 200f), ImGuiCond.Always);
            ImGuiWindowFlags Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse;

            Vector4 TextColor = Color.Lime.ToVector4();

            ImGui.Begin("Selected Unit", Flags);

            if (SelectedUnit != null) {
                ImGui.TextColored(new System.Numerics.Vector4(TextColor.X, TextColor.Y, TextColor.Z, TextColor.W), SelectedUnit.Name);
                ImGui.Separator();
                if (ImGui.CollapsingHeader("Stats")) {
                    ImGui.Text("Health - " + SelectedUnit.Stats.Health);
                    ImGui.Text("Attack - " + SelectedUnit.Stats.Attack);
                    ImGui.Text("Defense - " + SelectedUnit.Stats.Defense);
                    ImGui.Text("Attack Range - " + SelectedUnit.Stats.AttackRange);
                    ImGui.Text("Attack Speed - " + SelectedUnit.Stats.AttackSpeed + "s");
                    ImGui.Text("Critical Chance - " + SelectedUnit.Stats.CriticalChance + "%%");
                }

                ImGui.Separator();
                if (ImGui.CollapsingHeader("Body")) {
                    ImGui.Text("Velocity - [" + SelectedUnit.Body.Velocity.X.ToString("0.00") + " " + SelectedUnit.Body.Velocity.Y.ToString("0.00") + "]");
                    ImGui.Text("Max Velocity - " + SelectedUnit.Body.MaxVelocity);
                    ImGui.Text("Interior Radius - " + SelectedUnit.Body.InteriorRadius);
                    ImGui.Text("Exterior Radius - " + SelectedUnit.Body.ExteriorRadius);
                }
            }

            ImGui.End();
        }

        #endregion
    }
}

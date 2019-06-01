using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

using ImGuiNET;

namespace RTSGame {
    public class MainGame : Game {

        public GraphicsDeviceManager Graphics { get; private set; }

        public SpriteBatch SpriteBatch { get; private set; }

        // ImGui
        private ImGuiRenderer ImGuiRenderer;

        // Camera
        public Camera2D Camera { get; private set; }
        public float CameraSensitivity { get; set; }

        // Input
        public KeyboardState PreviousKeyboardState { get; set; }
        public MouseState PreviousMouseState { get; set; }
        private int PreviousScrollWheelValue; // Used for camera zoom
        private Vector2 SelectionStart;
        private bool Selecting;

        // Shader used to outline sprites
        public Effect Outline;

        // List with all Units in the game
        public List<Unit> Units;

        // Test
        public Unit TestUnit;
        public Unit Target;

        public MainGame() {
            Graphics = new GraphicsDeviceManager(this);

            // Adjust window resolution
            Graphics.PreferredBackBufferWidth = 1024;
            Graphics.PreferredBackBufferHeight = 768;
            Graphics.PreferMultiSampling = true;
            // Disable VSync
            Graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
            Graphics.ApplyChanges();

            // Make mouse visible
            IsMouseVisible = true;

            Content.RootDirectory = "Content";
        }

        // Allows the game to perform any initialization before starting to run.
        // Calling base.Initialize will enumerate through any components and initialize them.
        protected override void Initialize() {
            // Initialize camera
            var ViewportAdapter = new BoxingViewportAdapter(Window, Graphics, Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight);
            Camera = new Camera2D(ViewportAdapter);
            Camera.MinimumZoom = 0.5f;
            CameraSensitivity = 400f;
            Camera.Position = new Vector2(-Graphics.PreferredBackBufferWidth / 2f, -Graphics.PreferredBackBufferHeight / 2f);

            PreviousScrollWheelValue = Mouse.GetState().ScrollWheelValue;
            SelectionStart = new Vector2();
            Selecting = false;

            // Initialize Unit lists
            Units = new List<Unit>();

            Sprite S = new Sprite(Content.Load<Texture2D>("Sprites//Laharl"));
            TestUnit = new Unit("Main Unit", S);
            Target = new Unit("Target Unit", S);
            Target.Transform.Position = new Vector2(500f, 500f);

            // Add arrive steering
            TestUnit.AddSteering(SteeringType.Arrive);

            // Add flee steering
            Target.AddSteering(SteeringType.Flee);

            Units.Add(TestUnit);
            Units.Add(Target);

            // Initialize ImGui renderer
            ImGuiRenderer = new ImGuiRenderer(this);
            ImGuiRenderer.RebuildFontAtlas();

            base.Initialize();
        }

        // Called once per game, load content here.
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            // Load Outline shader
            Outline = Content.Load<Effect>("Shaders//Outline");
        }

        // Called once per game, is the place to unload game-specific content (non ContentManager)
        protected override void UnloadContent() {
            
        }

        // Called once per frame to update game logic
        protected override void Update(GameTime GameTime) {
            float DeltaTime = (float)GameTime.ElapsedGameTime.TotalSeconds;

            var KeyState = Keyboard.GetState();
            var MouseState = Mouse.GetState();

            // Start new frame for ImGui
            ImGuiRenderer.BeforeLayout(GameTime);

            if (KeyState.IsKeyDown(Keys.Q))
                TestUnit.SetSteeringTarget(SteeringType.Arrive, Target);

            if (KeyState.IsKeyDown(Keys.W))
                Target.SetSteeringTarget(SteeringType.Flee, TestUnit);

            if (KeyState.IsKeyDown(Keys.R) && PreviousKeyboardState.IsKeyUp(Keys.R))
                TestUnit.Selected = !TestUnit.Selected;

            // Process selection input
            if (MouseState.LeftButton == ButtonState.Pressed && !Selecting)
                if ((MouseState.X != PreviousMouseState.X) && (MouseState.Y != PreviousMouseState.Y)) {
                    Selecting = true;
                    SelectionStart = new Vector2(MouseState.X, MouseState.Y);
                }

            if (MouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed && Selecting)
                Selecting = false;

            UpdateCamera(DeltaTime);

            // Update Units
            foreach (Unit U in Units)
                U.Update(DeltaTime);

            // Store input state for next frame
            PreviousScrollWheelValue = MouseState.ScrollWheelValue;
            PreviousKeyboardState = KeyState;
            PreviousMouseState = MouseState;

            base.Update(GameTime);
        }

        // Called once per frame to render
        protected override void Draw(GameTime GameTime) {
            GraphicsDevice.Clear(new Color(51, 76, 76));

            var CameraMatrix = Camera.GetViewMatrix();

            // Batch -> Normal sprites without effects
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: CameraMatrix);

            // Draw all non-selected units
            foreach (Unit U in Units)
                if (!U.Selected)
                    U.Draw(SpriteBatch);

            SpriteBatch.End();

            // Batch -> Sprites will have an outline of color OutlineColor
            Vector4 OutlineColor = Color.Crimson.ToVector4();
            Vector2 TextureRes = new Vector2(TestUnit.Sprite.SpriteTexture.Width, TestUnit.Sprite.SpriteTexture.Height);
            Outline.Parameters["OutlineColor"].SetValue(OutlineColor);
            Outline.Parameters["TextureRes"].SetValue(TextureRes);

            SpriteBatch.Begin(sortMode: SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp,
                transformMatrix: CameraMatrix, effect: Outline);

            // Draw all selected units with an Outline
            foreach (Unit U in Units)
                if (U.Selected)
                    U.Draw(SpriteBatch);

            SpriteBatch.End();

            // Batch -> Gizmos
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: CameraMatrix);
            DrawGizmos(SpriteBatch);
            SpriteBatch.End();

            // Draw Selection Rectangle
            if (Selecting) {
                SpriteBatch.Begin();
                Size2 RectSize = new Size2(Math.Abs(Mouse.GetState().X - SelectionStart.X), Math.Abs(Mouse.GetState().Y - SelectionStart.Y));
                Point2 Origin = new Point2(Math.Min(SelectionStart.X, Mouse.GetState().X), Math.Min(SelectionStart.Y, Mouse.GetState().Y));
                SpriteBatch.DrawRectangle(new RectangleF(Origin, RectSize), Color.Green);
                SpriteBatch.End();
            }

            // Display ImGui interface
            DisplayGUI(GameTime);

            ImGuiRenderer.AfterLayout();

            base.Draw(GameTime);
        }

        // Handles camera interaction
        private void UpdateCamera(float DeltaTime) {
            var MouseState = Mouse.GetState();
            var KeyState = Keyboard.GetState();

            // Camera movement using mouse
            if (IsActive) {
                if (MouseState.Y <= 0)
                    Camera.Move(new Vector2(0, -CameraSensitivity) * DeltaTime);

                if (MouseState.Y >= Graphics.PreferredBackBufferHeight - 10f)
                    Camera.Move(new Vector2(0f, CameraSensitivity) * DeltaTime);

                if (MouseState.X <= 0)
                    Camera.Move(new Vector2(-CameraSensitivity, 0f) * DeltaTime);

                if (MouseState.X >= Graphics.PreferredBackBufferWidth)
                    Camera.Move(new Vector2(CameraSensitivity, 0f) * DeltaTime);
            }

            // Camera movement using arrow keys
            if (KeyState.IsKeyDown(Keys.Up))
                Camera.Move(new Vector2(0, -CameraSensitivity) * DeltaTime);

            if (KeyState.IsKeyDown(Keys.Down))
                Camera.Move(new Vector2(0, CameraSensitivity) * DeltaTime);

            if (KeyState.IsKeyDown(Keys.Left))
                Camera.Move(new Vector2(-CameraSensitivity, 0) * DeltaTime);

            if (KeyState.IsKeyDown(Keys.Right))
                Camera.Move(new Vector2(CameraSensitivity, 0) * DeltaTime);

            // Camera Zoom
            if (MouseState.ScrollWheelValue < PreviousScrollWheelValue)
                Camera.ZoomOut(0.10f);

            if (MouseState.ScrollWheelValue > PreviousScrollWheelValue)
                Camera.ZoomIn(0.10f);
        }

        private void DrawGizmos(SpriteBatch Batch) {
            
        }

        #region IMGUI

        private bool MiscInfoWindow = true;
        private bool DemoWindow = false;
        private bool CameraWindow = false;
        private bool UnitWindow = true;

        private void DisplayGUI(GameTime GameTime) {
            // Main Menu
            {
                if (ImGui.BeginMainMenuBar()) {
                    if (ImGui.BeginMenu("General")) {
                        if (ImGui.MenuItem("Display Misc Info"))
                            MiscInfoWindow = true;
                        if (ImGui.MenuItem("Exit"))
                            Exit();
                        ImGui.EndMenu();
                    }

                    if (ImGui.BeginMenu("Game")) {
                        if (ImGui.MenuItem("Camera Window"))
                            CameraWindow = true;
                        if (ImGui.MenuItem("Unit Window"))
                            UnitWindow = true;
                        ImGui.EndMenu();
                    }

                    if (ImGui.BeginMenu("About")) {
                        if (ImGui.MenuItem("Display Demo Window"))
                            DemoWindow = true;
                        ImGui.EndMenu();
                    }

                    ImGui.EndMainMenuBar();
                }
            }

            if (MiscInfoWindow)
                ShowMiscInfoWindow(GameTime);

            if (CameraWindow)
                ShowCameraWindow();

            if (UnitWindow)
                ShowUnitWindow();

            if (DemoWindow)
                ImGui.ShowDemoWindow(ref DemoWindow);
        }

        private void ShowMiscInfoWindow(GameTime GameTime) {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0f, Graphics.PreferredBackBufferHeight - 120f), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(350f, 120f), ImGuiCond.Always);
            ImGuiWindowFlags Flags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse;

            if (!ImGui.Begin("Misc Info", ref MiscInfoWindow, Flags)) {
                ImGui.End();
                return;
            }

            ImGui.Text("Time Since Start: " + GameTime.TotalGameTime.ToString("hh':'mm':'ss"));
            ImGui.Text("Application Average " + (1000f / ImGui.GetIO().Framerate).ToString("0.000") + " ms/frame (" + ImGui.GetIO().Framerate.ToString("0.0") + " FPS)");
            ImGui.Text("Window Resolution: " + Graphics.PreferredBackBufferWidth + "x" + Graphics.PreferredBackBufferHeight);
            Vector2 MousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            ImGui.Text("Mouse Screen Position: " + MousePosition);

            ImGui.End();
        }

        private void ShowCameraWindow() {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0f, 100f), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(235f, 90f), ImGuiCond.Always);
            ImGuiWindowFlags Flags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse;

            if (!ImGui.Begin("Camera", ref CameraWindow, Flags)) {
                ImGui.End();
                return;
            }

            Vector2 Position = Camera.Position;
            ImGui.Text("Position - " + "[ " + Position.X.ToString("0.00") + " " + Position.Y.ToString("0.00") + " ]");
            ImGui.Text("Zoom - " + Camera.Zoom.ToString("0.000"));
            ImGui.Text("Is Selecting - " + Selecting);

            ImGui.End();
        }

        private void ShowUnitWindow() {
            if (!ImGui.Begin("Unit", ref UnitWindow)) {
                ImGui.End();
                return;
            }

            ImGui.BulletText("TODO");

            ImGui.End();

            /*ImGui.Separator();

            foreach (Unit U in Units) {
                ImGui.TextColored(new System.Numerics.Vector4(1f, 1f, 0f, 1f), U.Name);

                if (ImGui.CollapsingHeader("Transform")) {
                    ImGui.BulletText("Position - " + U.Transform.Position.ToString());
                    ImGui.BulletText("Scale - " + U.Transform.Scale.ToString());
                    ImGui.BulletText("Rotation - " + U.Transform.Rotation);
                }

                if (ImGui.CollapsingHeader("Body")) {
                    ImGui.BulletText("Can Move - " + U.Body.CanMove);
                    ImGui.BulletText("Velocity - " + U.Body.Velocity);
                    ImGui.BulletText("Rotation - " + U.Body.Rotation);
                    ImGui.BulletText("Max Velocity - " + U.Body.MaxVelocity);
                    ImGui.BulletText("Max Rotation - " + U.Body.MaxRotation);
                    ImGui.BulletText("Max Acceleration - " + U.Body.MaxAcceleration);
                    ImGui.BulletText("Max Angular - " + U.Body.MaxAngular);
                    ImGui.BulletText("Exterior Radius - " + U.Body.ExteriorRadius);
                    ImGui.BulletText("Interior Radius - " + U.Body.InteriorRadius);
                }

                if (ImGui.CollapsingHeader("Sprite")) {
                    ImGui.BulletText("Image - " + U.Sprite.SpriteTexture.Name);
                    ImGui.BulletText("Image Width - " + U.Sprite.SpriteTexture.Width);
                    ImGui.BulletText("Image Height - " + U.Sprite.SpriteTexture.Height);
                    ImGui.BulletText("Color - " + U.Sprite.SpriteColor.ToString());
                    ImGui.BulletText("Layer - " + U.Sprite.Layer);
                }

                ImGui.Separator();
            } */

            /*ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(2f, 2f));
            ImGui.Columns(2);
            ImGui.Separator();

            // Display Units data
            ImGui.PushID(TestUnit.Name);

            ImGui.AlignTextToFramePadding();
            bool NodeOpen = ImGui.TreeNode(TestUnit.Name);
            ImGui.NextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(new System.Numerics.Vector4(1f, 1f, 0f, 1f), "Unit");
            ImGui.NextColumn();

            if (NodeOpen) {
                // Display needed information

                // Transform
                ImGui.AlignTextToFramePadding();
                ImGui.TreeNodeEx("Transform");
                ImGui.TreePop();
            }

            ImGui.PopID();

            ImGui.Columns(1);
            ImGui.Separator();
            ImGui.PopStyleVar(); */
        }

        #endregion
    }
}

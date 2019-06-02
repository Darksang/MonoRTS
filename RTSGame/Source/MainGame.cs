using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision;

using ImGuiNET;

namespace RTSGame {
    public class MainGame : Game {

        public GraphicsDeviceManager Graphics { get; private set; }

        public SpriteBatch SpriteBatch { get; private set; }

        // Shader used to outline sprites
        private Effect Outline;

        // ImGui
        private ImGuiRenderer ImGuiRenderer;

        // Camera
        private BoxingViewportAdapter ViewportAdapter;
        public Camera2D Camera { get; private set; }
        public float CameraSensitivity { get; set; }

        // Input
        public KeyboardState PreviousKeyboardState { get; private set; }
        public MouseState PreviousMouseState { get; private set; }
        private int PreviousScrollWheelValue; // Used for camera zoom
        private Vector2 SelectionStart;
        private bool Selecting;

        // List with all Units in the game
        public List<Unit> Units { get; private set; }
        // Current selected single Unit
        public Unit SelectedUnit { get; private set; }
        // Current selected Units (when selecting dragging mouse)
        public List<Unit> SelectedUnits { get; private set; }

        // Physics World
        public World World { get; set; }

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
            // Initialize ImGui renderer
            ImGuiRenderer = new ImGuiRenderer(this);
            ImGuiRenderer.RebuildFontAtlas();

            // Initialize camera
            ViewportAdapter = new BoxingViewportAdapter(Window, Graphics, Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight);
            Camera = new Camera2D(ViewportAdapter);
            Camera.MinimumZoom = 0.5f;
            CameraSensitivity = 400f;
            Camera.Position = new Vector2(-Graphics.PreferredBackBufferWidth / 2f, -Graphics.PreferredBackBufferHeight / 2f);

            PreviousScrollWheelValue = Mouse.GetState().ScrollWheelValue;
            SelectionStart = new Vector2();
            Selecting = false;

            // Initialize Unit lists
            Units = new List<Unit>();
            SelectedUnit = null;
            SelectedUnits = new List<Unit>();

            // Initialize Physics World
            Vector2 Gravity = new Vector2(0f, 0f);
            World = new World(Gravity);

            Sprite S = new Sprite(Content.Load<Texture2D>("Sprites//Laharl"));
            TestUnit = new Unit("Main Unit", S, World);
            Target = new Unit("Target Unit", S, World);
            Target.Transform.Position = new Vector2(500f, 500f);

            //TestUnit.AddSteering(SteeringType.Wander);
            Target.AddSteering(SteeringType.Pursue);

            Units.Add(TestUnit);
            Units.Add(Target);

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

            if (!ImGui.GetIO().WantCaptureKeyboard) {
                if (KeyState.IsKeyDown(Keys.A))
                    Target.SetSteeringTarget(SteeringType.Pursue, TestUnit);

                // Pressing Escape deselects all units
                if (KeyState.IsKeyDown(Keys.Escape))
                    foreach (Unit U in Units)
                        if (U.Selected)
                            U.Selected = false;
            }

            // Process input for the game if ImGui doesn't want to capture input
            if (!ImGui.GetIO().WantCaptureMouse) {
                // Select single Unit with Left Click
                if (MouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed && !Selecting) {
                    Vector2 WorldMouseCoords = Camera.ScreenToWorld(new Vector2(MouseState.X, MouseState.Y));
                    Fixture Test = World.TestPoint(ConvertUnits.ToSimUnits(WorldMouseCoords));
                    if (Test != null) {
                        if (SelectedUnit != null)
                            SelectedUnit.Selected = false;
                        Unit Collided = (Unit)Test.Body.UserData;
                        SelectedUnit = Collided;
                        Collided.Selected = true;
                    } else if (SelectedUnit != null) {
                        SelectedUnit.Selected = false;
                        SelectedUnit = null;
                    }

                    if (SelectedUnits.Count != 0) {
                        foreach (Unit U in SelectedUnits)
                            U.Selected = false;

                        SelectedUnits.Clear();
                    }
                }

                // Process rectangle selection input
                if (MouseState.LeftButton == ButtonState.Pressed && !Selecting)
                    // If we move the mouse without releasing the button we start selecting
                    if ((MouseState.X != PreviousMouseState.X) && (MouseState.Y != PreviousMouseState.Y)) {
                        Selecting = true;
                        SelectionStart = new Vector2(MouseState.X, MouseState.Y);
                    }

                // Stop rectangle selection after releasing the button
                if (MouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed && Selecting) {
                    // Calculate area to test in world units, then convert it to simulation units
                    Vector2 AABBMin = ConvertUnits.ToSimUnits(Camera.ScreenToWorld(new Vector2(Math.Min(SelectionStart.X, MouseState.X), Math.Min(SelectionStart.Y, MouseState.Y))));
                    Vector2 AABBMax = ConvertUnits.ToSimUnits(Camera.ScreenToWorld(new Vector2(Math.Max(SelectionStart.X, MouseState.X), Math.Max(SelectionStart.Y, MouseState.Y))));
                    AABB TestArea = new AABB(AABBMin, AABBMax);
                    List<Fixture> Collisions = World.QueryAABB(ref TestArea);

                    // No collision
                    if (Collisions.Count == 0) {
                        // If there was a single unit selected, deselect it
                        if (SelectedUnit != null) {
                            SelectedUnit.Selected = false;
                            SelectedUnit = null;
                        }
                        // It there was a group of units selected, deselect them
                        if (SelectedUnits.Count != 0) {
                            foreach (Unit U in SelectedUnits)
                                U.Selected = false;

                            SelectedUnits.Clear();
                        }
                    } else {
                        // Select group of units
                        SelectedUnit = null;
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

            UpdateCamera(DeltaTime);

            // Update Units
            foreach (Unit U in Units)
                U.Update(DeltaTime);

            // Update Physics
            World.Step(1f / 60f);

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
            if (!ImGui.GetIO().WantCaptureKeyboard) {
                if (KeyState.IsKeyDown(Keys.Up))
                    Camera.Move(new Vector2(0, -CameraSensitivity) * DeltaTime);

                if (KeyState.IsKeyDown(Keys.Down))
                    Camera.Move(new Vector2(0, CameraSensitivity) * DeltaTime);

                if (KeyState.IsKeyDown(Keys.Left))
                    Camera.Move(new Vector2(-CameraSensitivity, 0) * DeltaTime);

                if (KeyState.IsKeyDown(Keys.Right))
                    Camera.Move(new Vector2(CameraSensitivity, 0) * DeltaTime);
            }

            // Process input for the game if ImGui doesn't want to capture input
            if (!ImGui.GetIO().WantCaptureMouse) {
                // Camera zoom
                if (MouseState.ScrollWheelValue < PreviousScrollWheelValue)
                    Camera.ZoomOut(0.10f);

                if (MouseState.ScrollWheelValue > PreviousScrollWheelValue)
                    Camera.ZoomIn(0.10f);
            }
        }

        private void AddUnit(string Name, Sprite Sprite) {
            Units.Add(new Unit(Name, Sprite, World));
        }

        // Removes Unit from the game
        private void RemoveUnit(Unit Unit) {
            if (SelectedUnit == Unit)
                SelectedUnit = null;

            Unit.DestroyUnit(World);
            Units.Remove(Unit);
        }

        private void DrawGizmos(SpriteBatch Batch) {
            
        }

        #region IMGUI

        private bool MiscInfoWindow = true;
        private bool DemoWindow = false;
        private bool CameraWindow = false;
        private bool UnitsWindow = true;
        private bool UnitWindow = true;
        private bool SelectedUnitsWindow = false;
        private bool SteeringsWindow = false;

        private string NewUnitName = "";

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
                        if (ImGui.MenuItem("Camera"))
                            CameraWindow = true;
                        if (ImGui.MenuItem("Units"))
                            UnitsWindow = true;
                        if (ImGui.MenuItem("Selected Unit"))
                            UnitWindow = true;
                        if (ImGui.MenuItem("Selected Units List"))
                            SelectedUnitsWindow = true;
                        if (ImGui.MenuItem("Steerings"))
                            SteeringsWindow = true;
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

            if (UnitsWindow)
                ShowUnitsWindow();

            if (UnitWindow)
                ShowUnitWindow();

            if (SelectedUnitsWindow)
                ShowSelectedUnitsWindow();

            if (SteeringsWindow)
                ShowSteeringsWindow();

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

            ImGui.End();
        }

        private void ShowUnitsWindow() {
            if (!ImGui.Begin("Units", ref UnitsWindow)) {
                ImGui.End();
                return;
            }

            ImGui.Separator();
            Vector4 TextColor = Color.Lime.ToVector4();
            ImGui.TextColored(new System.Numerics.Vector4(TextColor.X, TextColor.Y, TextColor.Z, TextColor.W), "Create New Unit");
            ImGui.PushItemWidth(100);
            ImGui.InputText("Unit Name", ref NewUnitName, 32);
            if (ImGui.Button("Create")) {
                // TODO: Create Unit
                Console.WriteLine(NewUnitName);
                NewUnitName = "";
            }

            ImGui.Separator();

            foreach (Unit U in Units)
                ImGui.BulletText(U.Name);

            ImGui.End();
        }

        private void ShowUnitWindow() {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(Graphics.PreferredBackBufferWidth - 280f, 20f), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(280f, 340f), ImGuiCond.Always);
            ImGuiWindowFlags Flags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse;
            if (!ImGui.Begin("Selected Unit", ref UnitWindow, Flags)) {
                ImGui.End();
                return;
            }

            Vector4 TextColor = Color.Lime.ToVector4();

            if (SelectedUnit != null) {
                ImGui.TextColored(new System.Numerics.Vector4(TextColor.X, TextColor.Y, TextColor.Z, TextColor.W), SelectedUnit.Name);
                ImGui.SameLine();
                if (ImGui.Button("Delete")) {
                    Console.WriteLine("Unit Deleted");
                    RemoveUnit(SelectedUnit);
                    SelectedUnit = null;
                    ImGui.End();
                    return;
                }

                ImGui.Separator();

                if (ImGui.CollapsingHeader("Transform", ImGuiTreeNodeFlags.DefaultOpen)) {
                    Vector2 Position = SelectedUnit.Transform.Position;
                    ImGui.BulletText("Position - " + "{" + Position.X.ToString("0.00") + " " + Position.Y.ToString("0.00") + "}");
                    ImGui.BulletText("Scale - " + SelectedUnit.Transform.Scale);
                    ImGui.BulletText("Rotation - " + SelectedUnit.Transform.Rotation);
                }

                ImGui.Separator();

                if (ImGui.CollapsingHeader("Steering Behaviours", ImGuiTreeNodeFlags.DefaultOpen)) {
                    foreach (SteeringType S in SelectedUnit.Behaviours.Keys) {
                        ImGui.BulletText(S.ToString());
                    }
                }

                ImGui.Separator();

                if (ImGui.CollapsingHeader("Body", ImGuiTreeNodeFlags.DefaultOpen)) {
                    Vector2 Velocity = SelectedUnit.Body.Velocity;
                    ImGui.BulletText("Velocity - " + "{" + Velocity.X.ToString("0.00") + " " + Velocity.Y.ToString("0.00") + "}");
                    ImGui.BulletText("Rotation - " + SelectedUnit.Body.Rotation);
                    ImGui.Separator();
                    ImGui.BulletText("Max Velocity - " + SelectedUnit.Body.MaxVelocity);
                    ImGui.BulletText("Max Rotation - " + SelectedUnit.Body.MaxRotation);
                    ImGui.Separator();
                    ImGui.BulletText("Max Acceleration - " + SelectedUnit.Body.MaxAcceleration);
                    ImGui.BulletText("Max Angular - " + SelectedUnit.Body.MaxAngular);
                    ImGui.Separator();
                    ImGui.BulletText("Exterior Radius - " + SelectedUnit.Body.ExteriorRadius);
                    ImGui.BulletText("Interior Radius - " + SelectedUnit.Body.InteriorRadius);
                    ImGui.Separator();
                    ImGui.BulletText("Exterior Angle - " + SelectedUnit.Body.ExteriorAngle);
                    ImGui.BulletText("Interior Angle - " + SelectedUnit.Body.InteriorAngle);
                }

                ImGui.Separator();

                if (ImGui.CollapsingHeader("Sprite")) {
                    ImGui.BulletText("Name - " + SelectedUnit.Sprite.SpriteTexture.Name);
                    ImGui.BulletText("Width - " + SelectedUnit.Sprite.SpriteTexture.Width);
                    ImGui.BulletText("Height - " + SelectedUnit.Sprite.SpriteTexture.Height);
                }
            } else {
                ImGui.TextColored(new System.Numerics.Vector4(TextColor.X, TextColor.Y, TextColor.Z, TextColor.W), "No Unit Selected");
                ImGui.Separator();
            }

            ImGui.End();
        }

        private void ShowSelectedUnitsWindow() {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(175f, 170f), ImGuiCond.Always);
            if (!ImGui.Begin("Selected Unit List", ref SelectedUnitsWindow, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse)) {
                ImGui.End();
                return;
            }

            ImGui.Separator();
            ImGui.Text("Is Selecting - " + Selecting);
            ImGui.Separator();
            foreach (Unit U in SelectedUnits)
                ImGui.BulletText(U.Name);

            ImGui.End();
        }

        private void ShowSteeringsWindow() {
            if (!ImGui.Begin("Steerings", ref SteeringsWindow)) {
                ImGui.End();
                return;
            }

            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered()) {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted("Here you can choose a Steering to add to the current selected Unit.");
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }

            // TODO: Select steering
            string Selected = SteeringType.Align.ToString();
            if (ImGui.BeginCombo("Steering Behaviour", Selected)) {
                ImGui.EndCombo();
            }

            ImGui.End();
        }

        #endregion
    }
}

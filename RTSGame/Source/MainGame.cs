using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using MonoGame.Extended.Tiled;

using ImGuiNET;

namespace RTSGame {

    public class MainGame : Game {

        public GraphicsDeviceManager Graphics { get; private set; }

        public SpriteBatch SpriteBatch { get; private set; }

        // Contains all Effects loaded in the game
        public Dictionary<string, Effect> Effects { get; private set; }
        // Contains all Texture2D loaded in the game
        public Dictionary<string, Texture2D> Sprites { get; private set; }
        // Contains all SpriteFonts loaded in the game
        public Dictionary<string, SpriteFont> Fonts { get; private set; }
        // Contains all Tilemaps loaded in the game
        public Dictionary<string, TiledMap> Maps { get; private set; }

        // ImGui
        private ImGuiRenderer ImGuiRenderer;

        // Camera
        private BoxingViewportAdapter ViewportAdapter;
        public Camera2D Camera { get; private set; }
        public float CameraSensitivity;

        // Input
        public KeyboardState KeyboardState { get; private set; }
        public KeyboardState PreviousKeyboardState { get; private set; }
        public MouseState MouseState { get; private set; }
        public MouseState PreviousMouseState { get; private set; }
        private int PreviousScrollWheelValue; // Used for camera zoom

        // Scenes
        private Dictionary<string, Scene> Scenes;
        private Scene ActiveScene;

        private bool ChangeScene;

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

            Effects = new Dictionary<string, Effect>();
            Sprites = new Dictionary<string, Texture2D>();
            Fonts = new Dictionary<string, SpriteFont>();
            Maps = new Dictionary<string, TiledMap>();

            Scenes = new Dictionary<string, Scene>();

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
            CameraSensitivity = 750f;
            Camera.Position = new Vector2(-Graphics.PreferredBackBufferWidth / 2f, -Graphics.PreferredBackBufferHeight / 2f);

            PreviousScrollWheelValue = Mouse.GetState().ScrollWheelValue;

            InitializeScenes();

            base.Initialize();
        }

        private void InitializeScenes() {
            Scenes.Add("Empty Scene", new EmptyScene(this));
            Scenes.Add("Main Scenario", new MainScene(this));
            Scenes.Add("Steering Test Scene", new SteeringTestScene(this));
            Scenes.Add("Collision Avoidance Scene", new CollisionScene(this));
            Scenes.Add("Pathfinding Scene", new PathfindingScene(this));
            Scenes.Add("Groups Scene", new GroupsScene(this));

            ActiveScene = Scenes["Empty Scene"];

            ChangeScene = false;

            SceneNames = new string[Scenes.Count];

            int i = 0;
            foreach (string S in Scenes.Keys) {
                SceneNames[i] = S;
                i++;
            }
        }

        // Called once per game, load content here.
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            // Load shaders
            Effects.Add("Outline", Content.Load<Effect>("Shaders//Outline"));

            // Load sprites
            Sprites.Add("Stump", Content.Load<Texture2D>("Sprites//Stump"));
            Sprites.Add("Ghost", Content.Load<Texture2D>("Sprites//Ghost"));
            Sprites.Add("Cat", Content.Load<Texture2D>("Sprites//Cat"));
            Sprites.Add("Healer", Content.Load<Texture2D>("Sprites//Healer"));
            Sprites.Add("Assassin", Content.Load<Texture2D>("Sprites//Assassin"));

            // Load fonts
            Fonts.Add("Arial", Content.Load<SpriteFont>("Fonts//Arial"));

            // Load tilemaps
            Maps.Add("PathfindingMap", Content.Load<TiledMap>("TileMaps//PathfindingMap"));
            Maps.Add("MainMap", Content.Load<TiledMap>("TileMaps//MainMap"));
        }

        // Called once per game, is the place to unload game-specific content (non ContentManager)
        protected override void UnloadContent() {
            
        }

        // Called once per frame to update game logic
        protected override void Update(GameTime GameTime) {
            float DeltaTime = (float)GameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();

            // Start new frame for ImGui
            ImGuiRenderer.BeforeLayout(GameTime);

            // Check if we have to change scene
            if (ChangeScene)
                LoadScene();

            UpdateCamera(DeltaTime);

            // Update scene
            ActiveScene.Update(GameTime);

            // Store input state for next frame
            PreviousKeyboardState = KeyboardState;
            PreviousMouseState = MouseState;
            PreviousScrollWheelValue = MouseState.ScrollWheelValue;

            base.Update(GameTime);
        }

        // Called once per frame to render
        protected override void Draw(GameTime GameTime) {
            float DeltaTime = (float)GameTime.ElapsedGameTime.TotalSeconds;

            GraphicsDevice.Clear(new Color(51, 76, 76));

            // Render scene
            ActiveScene.Render(GameTime);

            // Finish ImGui frame
            DisplayEditor(GameTime);
            ImGuiRenderer.AfterLayout();

            base.Draw(GameTime);
        }

        private void UpdateCamera(float DeltaTime) {
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
                if (KeyboardState.IsKeyDown(Keys.Up))
                    Camera.Move(new Vector2(0, -CameraSensitivity) * DeltaTime);

                if (KeyboardState.IsKeyDown(Keys.Down))
                    Camera.Move(new Vector2(0, CameraSensitivity) * DeltaTime);

                if (KeyboardState.IsKeyDown(Keys.Left))
                    Camera.Move(new Vector2(-CameraSensitivity, 0) * DeltaTime);

                if (KeyboardState.IsKeyDown(Keys.Right))
                    Camera.Move(new Vector2(CameraSensitivity, 0) * DeltaTime);
            }

            // Camera zoom
            if (!ImGui.GetIO().WantCaptureMouse) {
                if (MouseState.ScrollWheelValue < PreviousScrollWheelValue)
                    Camera.ZoomOut(0.10f);

                if (MouseState.ScrollWheelValue > PreviousScrollWheelValue)
                    Camera.ZoomIn(0.10f);
            }
        }

        private void LoadScene() {
            // Destroy old scene
            ActiveScene.Destroy();

            // Load new scene
            ActiveScene = Scenes[SceneNames[SelectedScene]];

            // Initialize new scene
            ActiveScene.Initialize();

            ChangeScene = false;
        }

        #region ImGuiEditor

        private bool MiscInfoWindow = true;
        private bool SceneWindow = true;
        private bool CameraWindow = false;
        private bool DemoWindow = false;

        private string[] SceneNames;
        private int SelectedScene = 0;

        private void DisplayEditor(GameTime Time) {
            // Main menu
            if (ImGui.BeginMainMenuBar()) {
                if (ImGui.BeginMenu("General")) {
                    if (ImGui.MenuItem("Display Misc Info"))
                        MiscInfoWindow = true;
                    if (ImGui.MenuItem("Exit"))
                        Exit();
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Scene")) {
                    if (ImGui.MenuItem("Change Scene"))
                        SceneWindow = true;
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Game")) {
                    if (ImGui.MenuItem("Camera"))
                        CameraWindow = true;
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Help")) {
                    if (ImGui.MenuItem("ImGui Demo"))
                        DemoWindow = true;
                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }

            // Windows
            if (MiscInfoWindow)
                ShowMiscInfoWindow(Time);

            if (SceneWindow)
                ShowSceneWindow();

            if (CameraWindow)
                ShowCameraWindow();

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
            if (!IsFixedTimeStep && !Graphics.SynchronizeWithVerticalRetrace)
                ImGui.Text("VSync: OFF");
            else
                ImGui.Text("VSync: ON");

            ImGui.End();
        }

        private void ShowSceneWindow() {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0f, 100f), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(220f, 95f), ImGuiCond.Always);
            ImGuiWindowFlags Flags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse;

            if (!ImGui.Begin("Scene Select", ref SceneWindow, Flags)) {
                ImGui.End();
                return;
            }

            ImGui.PushItemWidth(150f);
            ImGui.Combo("Scene", ref SelectedScene, SceneNames, SceneNames.Length);
            ImGui.PopItemWidth();

            ImGui.Separator();

            if (ImGui.Button("Change")) {
                ChangeScene = true;
            }

            ImGui.End();
        }

        private void ShowCameraWindow() {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0f, Graphics.PreferredBackBufferHeight - 210f), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(275f, 90f), ImGuiCond.Always);
            ImGuiWindowFlags Flags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse;

            if (!ImGui.Begin("Camera", ref CameraWindow, Flags)) {
                ImGui.End();
                return;
            }

            Vector2 Position = Camera.Position;
            ImGui.Text("Position - " + "[ " + Position.X.ToString("0.00") + " " + Position.Y.ToString("0.00") + " ]");
            ImGui.Text("Zoom - " + Camera.Zoom.ToString("0.000"));
            ImGui.SliderFloat("Sensitivity", ref CameraSensitivity, 100f, 800f);

            ImGui.End();
        }

        #endregion
    }
}

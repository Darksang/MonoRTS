using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerPhysics.Dynamics;

using ImGuiNET;

namespace RTSGame {

    public class SteeringTestScene : Scene {

        public SteeringTestScene(MainGame MainGame) : base(MainGame) { }

        public override void Initialize() {
            // Reset physics world
            Vector2 Gravity = new Vector2(0f, 0f);
            World = new World(Gravity);

            // Reset containers
            Units = new List<Unit>();

            // Reset camera
            Game.Camera.Position = new Vector2(-Game.Graphics.PreferredBackBufferWidth / 2f, -Game.Graphics.PreferredBackBufferHeight / 2f);
            Game.Camera.Zoom = 1f;

            SetupUnits();
        }

        public override void Update(GameTime GameTime) {
            float DeltaTime = (float)GameTime.ElapsedGameTime.TotalSeconds;

            // Update scene
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

            // Draw strings with AnisotropicClamp
            Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, transformMatrix: CameraMatrix);

            foreach (Unit U in Units)
                // Draw Unit name on top of it
                Game.SpriteBatch.DrawString(Game.Fonts["Consolas"], U.Name, 
                    new Vector2(U.Transform.Position.X - (U.Sprite.SpriteTexture.Width / 2f), U.Transform.Position.Y - U.Sprite.SpriteTexture.Height / 2f - 15f), Color.Black);

            Game.SpriteBatch.End();

            DisplayEditor();
        }

        public override void Destroy() {
            World.Clear();
            Units.Clear();
        }

        private void SetupUnits() {
            Sprite L = new Sprite(Game.Sprites["Stump"]);

            // Seek Steering
            Unit Seek = new Unit("Seek", L, World);
            Seek.Transform.Position = new Vector2(0f, 200f);
            Seek.Collider.Body.Position = ConvertUnits.ToSimUnits(new Vector2(0f, 200f));
            Seek.DrawDebugVelocity = true;

            Unit SeekTarget = new Unit("Seek Target", L, World);
            SeekTarget.Transform.Position = new Vector2(0f, -200f);
            SeekTarget.Collider.Body.Position = ConvertUnits.ToSimUnits(new Vector2(0f, -200f));

            Seek.AddSteering(SteeringType.Seek);

            Units.Add(Seek);
            Units.Add(SeekTarget);

            // Flee Steering
            Unit Flee = new Unit("Flee", L, World);
            Flee.Transform.Position = new Vector2(300f, 0f);
            Flee.Collider.Body.Position = ConvertUnits.ToSimUnits(new Vector2(300f, 0f));
            Flee.DrawDebugVelocity = true;

            Unit FleeTarget = new Unit("Flee Target", L, World);
            FleeTarget.Transform.Position = new Vector2(300f, -200f);
            FleeTarget.Collider.Body.Position = ConvertUnits.ToSimUnits(new Vector2(300f, -200f));

            Flee.AddSteering(SteeringType.Flee);

            Units.Add(Flee);
            Units.Add(FleeTarget);

            // Arrive Steering
            Unit Arrive = new Unit("Arrive", L, World);
            Arrive.Transform.Position = new Vector2(600f, 200f);
            Arrive.Collider.Body.Position = ConvertUnits.ToSimUnits(new Vector2(600f, 200f));
            Arrive.DrawDebugVelocity = true;

            Unit ArriveTarget = new Unit("Arrive Target", L, World);
            ArriveTarget.Transform.Position = new Vector2(600f, -200f);
            ArriveTarget.Collider.Body.Position = ConvertUnits.ToSimUnits(new Vector2(600f, -200f));
            ArriveTarget.DrawDebugRadiuses = true;
            ArriveTarget.Body.InteriorRadius = 150f;

            Arrive.AddSteering(SteeringType.Arrive);

            Units.Add(Arrive);
            Units.Add(ArriveTarget);

            // Align Steering
            Unit Align = new Unit("Align", L, World);
            Align.Transform.Position = new Vector2(900f, 200f);
            Align.Collider.Body.Position = ConvertUnits.ToSimUnits(new Vector2(900f, 200f));
            Align.Transform.Rotation = MathHelper.ToRadians(180f);

            Unit AlignTarget = new Unit("Align Target", L, World);
            AlignTarget.Transform.Position = new Vector2(900f, -200f);
            AlignTarget.Collider.Body.Position = ConvertUnits.ToSimUnits(new Vector2(900f, -200f));

            Align.AddSteering(SteeringType.Align);

            Units.Add(Align);
            Units.Add(AlignTarget);

            // AntiAlign Steering
            Unit AntiAlign = new Unit("AntiAlign", L, World);
            AntiAlign.Transform.Position = new Vector2(1200f, 200f);
            AntiAlign.Collider.Body.Position = ConvertUnits.ToSimUnits(new Vector2(1200f, 200f));

            Unit AntiAlignTarget = new Unit("AntiAlign Target", L, World);
            AntiAlignTarget.Transform.Position = new Vector2(1200f, -200f);
            AntiAlignTarget.Collider.Body.Position = ConvertUnits.ToSimUnits(new Vector2(1200f, -200f));

            AntiAlign.AddSteering(SteeringType.AntiAlign);

            Units.Add(AntiAlign);
            Units.Add(AntiAlignTarget);

            // Wander Steering
            Unit Wander = new Unit("Wander", L, World);
            Wander.Transform.Position = new Vector2(1500f, 0f);
            Wander.Collider.Body.Position = ConvertUnits.ToSimUnits(new Vector2(1500f, 0f));
            Wander.DrawDebugVelocity = true;

            Units.Add(Wander);

            // Pursue Steering
            Unit Pursue = new Unit("Pursue", L, World);
            Pursue.Transform.Position = new Vector2(1800f, 200f);
            Pursue.Collider.Body.Position = ConvertUnits.ToSimUnits(new Vector2(1800f, 200f));
            Pursue.Body.MaxVelocity = 200f;
            Pursue.DrawDebugVelocity = true;

            Unit PursueTarget = new Unit("Pursue Target", L, World);
            PursueTarget.Transform.Position = new Vector2(1800f, -200f);
            PursueTarget.Collider.Body.Position = ConvertUnits.ToSimUnits(new Vector2(1800f, -200f));
            PursueTarget.Body.MaxVelocity = 70f;

            Pursue.AddSteering(SteeringType.Pursue);

            Units.Add(Pursue);
            Units.Add(PursueTarget);

            // Evade Steering
            Unit Evade = new Unit("Evade", L, World);
            Evade.Transform.Position = new Vector2(2100f, 200f);
            Evade.Collider.Body.Position = ConvertUnits.ToSimUnits(new Vector2(2100f, 200f));
            Evade.Body.MaxVelocity = 200f;
            Evade.DrawDebugVelocity = true;

            Unit EvadeTarget = new Unit("Evade Target", L, World);
            EvadeTarget.Transform.Position = new Vector2(2100f, -200f);
            EvadeTarget.Collider.Body.Position = ConvertUnits.ToSimUnits(new Vector2(2100f, -200f));
            EvadeTarget.Body.MaxVelocity = 70f;

            Evade.AddSteering(SteeringType.Evade);

            Units.Add(Evade);
            Units.Add(EvadeTarget);

            // VelocityMatching Steering
            Unit VelocityMatching = new Unit("VelocityMatching", L, World);
            VelocityMatching.Transform.Position = new Vector2(2400f, 200f);
            VelocityMatching.Collider.Body.Position = ConvertUnits.ToSimUnits(new Vector2(2400f, 200f));
            VelocityMatching.Body.MaxVelocity = 200f;
            VelocityMatching.DrawDebugVelocity = true;

            Unit VelocityMatchingTarget = new Unit("VelocityMatching Target", L, World);
            VelocityMatchingTarget.Transform.Position = new Vector2(2400f, -200f);
            VelocityMatchingTarget.Collider.Body.Position = ConvertUnits.ToSimUnits(new Vector2(2400f, -200f));
            VelocityMatchingTarget.Body.MaxVelocity = 70f;
            VelocityMatchingTarget.DrawDebugVelocity = true;

            VelocityMatching.AddSteering(SteeringType.VelocityMatching);

            Units.Add(VelocityMatching);
            Units.Add(VelocityMatchingTarget);

            // Face Steering
            Unit Face = new Unit("Face", L, World);
            Face.Transform.Position = new Vector2(2700f, 200f);
            Face.Collider.Body.Position = ConvertUnits.ToSimUnits(new Vector2(2700f, 200f));

            Unit FaceTarget = new Unit("Face Target", L, World);
            FaceTarget.Transform.Position = new Vector2(2700f, -200f);
            FaceTarget.Collider.Body.Position = ConvertUnits.ToSimUnits(new Vector2(2700f, -200f));

            Face.AddSteering(SteeringType.Face);

            Units.Add(Face);
            Units.Add(FaceTarget);

            // LookWhereYouGoing Steering
            Unit Look = new Unit("Look", L, World);
            Look.Transform.Position = new Vector2(3000f, 200f);
            Look.Collider.Body.Position = ConvertUnits.ToSimUnits(new Vector2(3000f, 200f));
            Look.DrawDebugVelocity = true;

            Unit LookTarget = new Unit("Look Target", L, World);
            LookTarget.Transform.Position = new Vector2(3000f, -200f);
            LookTarget.Collider.Body.Position = ConvertUnits.ToSimUnits(new Vector2(3000f, -200f));
            LookTarget.DrawDebugVelocity = true;

            Look.AddSteering(SteeringType.Arrive);
            Look.AddSteering(SteeringType.LookWhereYouGoing);

            Units.Add(Look);
            Units.Add(LookTarget);
        }

        #region ImGuiEditor

        private void DisplayEditor() {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0f, 200f), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(210f, 200f), ImGuiCond.Always);
            ImGuiWindowFlags Flags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse;

            Vector4 TextColor = Color.Lime.ToVector4();

            ImGui.Begin("Steering Testing", Flags);

            // Seek
            ImGui.TextColored(new System.Numerics.Vector4(TextColor.X, TextColor.Y, TextColor.Z, TextColor.W), "Seek");
            ImGui.SameLine();
            if (ImGui.Button("Start##Seek")) {
                Unit X = null;
                Unit Y = null;
                foreach (Unit U in Units)
                    if (U.Name == "Seek") {
                        X = U;
                        break;
                    }

                foreach (Unit U in Units)
                    if (U.Name == "Seek Target") {
                        Y = U;
                        break;
                    }

                X.SetSteeringTarget(SteeringType.Seek, Y);
            }
            ImGui.Separator();

            // Flee
            ImGui.TextColored(new System.Numerics.Vector4(TextColor.X, TextColor.Y, TextColor.Z, TextColor.W), "Flee");
            ImGui.SameLine();
            if (ImGui.Button("Start##Flee")) {
                Unit X = null;
                Unit Y = null;
                foreach (Unit U in Units)
                    if (U.Name == "Flee") {
                        X = U;
                        break;
                    }

                foreach (Unit U in Units)
                    if (U.Name == "Flee Target") {
                        Y = U;
                        break;
                    }

                X.SetSteeringTarget(SteeringType.Flee, Y);
            }
            ImGui.Separator();

            // Arrive
            ImGui.TextColored(new System.Numerics.Vector4(TextColor.X, TextColor.Y, TextColor.Z, TextColor.W), "Arrive");
            ImGui.SameLine();
            if (ImGui.Button("Start##Arrive")) {
                Unit X = null;
                Unit Y = null;
                foreach (Unit U in Units)
                    if (U.Name == "Arrive") {
                        X = U;
                        break;
                    }

                foreach (Unit U in Units)
                    if (U.Name == "Arrive Target") {
                        Y = U;
                        break;
                    }

                X.SetSteeringTarget(SteeringType.Arrive, Y);
            }
            ImGui.Separator();

            // Align
            ImGui.TextColored(new System.Numerics.Vector4(TextColor.X, TextColor.Y, TextColor.Z, TextColor.W), "Align");
            ImGui.SameLine();
            if (ImGui.Button("Start##Align")) {
                Unit X = null;
                Unit Y = null;
                foreach (Unit U in Units)
                    if (U.Name == "Align") {
                        X = U;
                        break;
                    }

                foreach (Unit U in Units)
                    if (U.Name == "Align Target") {
                        Y = U;
                        break;
                    }

                X.SetSteeringTarget(SteeringType.Align, Y);
            }
            ImGui.Separator();

            // AntiAlign
            ImGui.TextColored(new System.Numerics.Vector4(TextColor.X, TextColor.Y, TextColor.Z, TextColor.W), "AntiAlign");
            ImGui.SameLine();
            if (ImGui.Button("Start##AntiAlign")) {
                Unit X = null;
                Unit Y = null;
                foreach (Unit U in Units)
                    if (U.Name == "AntiAlign") {
                        X = U;
                        break;
                    }

                foreach (Unit U in Units)
                    if (U.Name == "AntiAlign Target") {
                        Y = U;
                        break;
                    }

                X.SetSteeringTarget(SteeringType.AntiAlign, Y);
            }
            ImGui.Separator();

            // Wander
            ImGui.TextColored(new System.Numerics.Vector4(TextColor.X, TextColor.Y, TextColor.Z, TextColor.W), "Wander");
            ImGui.SameLine();
            if (ImGui.Button("Start##Wander")) {
                Unit X = null;
                foreach (Unit U in Units)
                    if (U.Name == "Wander") {
                        X = U;
                        break;
                    }

                X.AddSteering(SteeringType.Wander);
            }
            ImGui.Separator();

            // Pursue 
            ImGui.TextColored(new System.Numerics.Vector4(TextColor.X, TextColor.Y, TextColor.Z, TextColor.W), "Pursue");
            ImGui.SameLine();
            if (ImGui.Button("Start##Pursue")) {
                Unit X = null;
                Unit Y = null;
                foreach (Unit U in Units)
                    if (U.Name == "Pursue") {
                        X = U;
                        break;
                    }

                foreach (Unit U in Units)
                    if (U.Name == "Pursue Target") {
                        Y = U;
                        break;
                    }

                X.SetSteeringTarget(SteeringType.Pursue, Y);
                Y.AddSteering(SteeringType.Wander);
            }
            ImGui.Separator();

            // Evade 
            ImGui.TextColored(new System.Numerics.Vector4(TextColor.X, TextColor.Y, TextColor.Z, TextColor.W), "Evade");
            ImGui.SameLine();
            if (ImGui.Button("Start##Evade")) {
                Unit X = null;
                Unit Y = null;
                foreach (Unit U in Units)
                    if (U.Name == "Evade") {
                        X = U;
                        break;
                    }

                foreach (Unit U in Units)
                    if (U.Name == "Evade Target") {
                        Y = U;
                        break;
                    }

                X.SetSteeringTarget(SteeringType.Evade, Y);
                Y.AddSteering(SteeringType.Wander);
            }
            ImGui.Separator();

            // VelocityMatching
            ImGui.TextColored(new System.Numerics.Vector4(TextColor.X, TextColor.Y, TextColor.Z, TextColor.W), "VelocityMatching");
            ImGui.SameLine();
            if (ImGui.Button("Start##VelocityMatching")) {
                Unit X = null;
                Unit Y = null;
                foreach (Unit U in Units)
                    if (U.Name == "VelocityMatching") {
                        X = U;
                        break;
                    }

                foreach (Unit U in Units)
                    if (U.Name == "VelocityMatching Target") {
                        Y = U;
                        break;
                    }

                X.SetSteeringTarget(SteeringType.VelocityMatching, Y);
                Y.AddSteering(SteeringType.Wander);
            }
            ImGui.Separator();

            // Face
            ImGui.TextColored(new System.Numerics.Vector4(TextColor.X, TextColor.Y, TextColor.Z, TextColor.W), "Face");
            ImGui.SameLine();
            if (ImGui.Button("Start##Face")) {
                Unit X = null;
                Unit Y = null;
                foreach (Unit U in Units)
                    if (U.Name == "Face") {
                        X = U;
                        break;
                    }

                foreach (Unit U in Units)
                    if (U.Name == "Face Target") {
                        Y = U;
                        break;
                    }

                X.SetSteeringTarget(SteeringType.Face, Y);
            }
            ImGui.Separator();

            // LookWhereYouGoing
            ImGui.TextColored(new System.Numerics.Vector4(TextColor.X, TextColor.Y, TextColor.Z, TextColor.W), "LookWhereYouGoing");
            ImGui.SameLine();
            if (ImGui.Button("Start##Look")) {
                Unit X = null;
                Unit Y = null;
                foreach (Unit U in Units)
                    if (U.Name == "Look") {
                        X = U;
                        break;
                    }

                foreach (Unit U in Units)
                    if (U.Name == "Look Target") {
                        Y = U;
                        break;
                    }

                X.SetSteeringTarget(SteeringType.Arrive, Y);
            }
            ImGui.Separator();

            ImGui.End();
        }

        #endregion
    }
}

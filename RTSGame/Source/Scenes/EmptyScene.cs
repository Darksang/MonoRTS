using System.Collections.Generic;

using Microsoft.Xna.Framework;

using FarseerPhysics.Dynamics;

namespace RTSGame {

    public class EmptyScene : Scene {

        public EmptyScene(MainGame MainGame) : base(MainGame) {
            
        }

        public override void Initialize() {
            // Reset physics world
            Vector2 Gravity = new Vector2(0f, 0f);
            World = new World(Gravity);

            // Reset containers
            Units = new List<Unit>();

            // Reset camera
            Game.Camera.Position = new Vector2(-Game.Graphics.PreferredBackBufferWidth / 2f, -Game.Graphics.PreferredBackBufferHeight / 2f);
            Game.Camera.Zoom = 1f;
        }

        public override void Update(GameTime GameTime) {
            
        }

        public override void Render(GameTime GameTime) {
            
        }

        public override void Destroy() {
            World.Clear();
            Units.Clear();
        }
    }
}

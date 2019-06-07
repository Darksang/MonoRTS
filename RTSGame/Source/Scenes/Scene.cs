using System.Collections.Generic;

using Microsoft.Xna.Framework;

using FarseerPhysics.Dynamics;

namespace RTSGame {

    public abstract class Scene {
        
        public MainGame Game;

        // Physics world
        public World World;

        // List with all Units in the scene
        public List<Unit> Units;

        public Scene(MainGame Game) {
            this.Game = Game;

            // Initialize physics world
            Vector2 Gravity = new Vector2(0f, 0f);
            World = new World(Gravity);

            Units = new List<Unit>();
        }

        public abstract void Initialize();

        public abstract void Update(GameTime GameTime);

        public abstract void Render(GameTime GameTime);

        public abstract void Destroy();
    }
}

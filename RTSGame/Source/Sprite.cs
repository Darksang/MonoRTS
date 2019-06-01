using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTSGame {

    public class Sprite {
        // Texture associated with the Sprite
        public Texture2D SpriteTexture { get; set; }
        // Color mask for the Sprite
        public Color SpriteColor { get; set; }
        // Depth used for rendering
        public float Layer { get; set; }

        public Sprite(Texture2D Texture) {
            SpriteTexture = Texture;
            SpriteColor = Color.White;
            Layer = 0f;
        }
    }
}

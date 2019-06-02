using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Transform {
        // Position in space defined by a 2D Vector
        public Vector2 Position { get; set; }
        // Scale, (1, 1) it's the original size
        public Vector2 Scale { get; set; }
        // Rotation in degrees
        public float Rotation { get; set; }

        public Transform() {
            Position = new Vector2(0f, 0f);
            Scale = new Vector2(1f, 1f);
            Rotation = 0f;
        }
    }
}
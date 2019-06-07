using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Steering {
        // Accelerated Uniform Movement
        public Vector2 Linear;
        public float Angular;

        public Steering() {
            Linear = new Vector2(0f, 0f);
            Angular = 0f;
        }
    }
}

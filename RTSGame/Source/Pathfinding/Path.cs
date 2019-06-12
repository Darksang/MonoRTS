using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Path {
        // Holds all the positions of the Path
        public List<Vector2> Positions;

        public Path() {
            Positions = new List<Vector2>();
        }
    }
}

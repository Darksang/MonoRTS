using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Node {
        // Whether the Node is walkable or not
        public bool Walkable;
        // This Node's position in the game world
        public Vector2 WorldPosition;
        // Costs of the Node
        public float G;
        public float H;
        public float F { get { return G + H; } }

        // Position of the Node in the grid
        public int GridX;
        public int GridY;

        // Parent of the Node in the calculated Path
        public Node Parent;

        public Node(bool Walkable, Vector2 WorldPosition, int GridX, int GridY) {
            this.Walkable = Walkable;
            this.WorldPosition = WorldPosition;

            G = 1f;
            H = 0f;

            this.GridX = GridX;
            this.GridY = GridY;

            Parent = null;
        }

        public Node(bool Walkable, Vector2 WorldPosition, int GridX, int GridY, float Cost) {
            this.Walkable = Walkable;
            this.WorldPosition = WorldPosition;

            G = Cost;
            H = 0f;

            this.GridX = GridX;
            this.GridY = GridY;

            Parent = null;
        }
    }
}

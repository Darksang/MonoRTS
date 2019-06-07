using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Node {
        // Whether the Node is walkable or not
        public bool Walkable;
        // This Node's position in the game world
        public Vector2 WorldPosition;
        // Cost associated with the Node
        public float Cost;

        public Node(bool Walkable, Vector2 WorldPosition) {
            this.Walkable = Walkable;
            this.WorldPosition = WorldPosition;
            Cost = 1f;
        }

        public Node(bool Walkable, Vector2 WorldPosition, float Cost) {
            this.Walkable = Walkable;
            this.WorldPosition = WorldPosition;
            this.Cost = Cost;
        }
    }
}

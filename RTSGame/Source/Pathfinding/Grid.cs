using System;
using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Grid {
        // 2D Array containing all nodes for the grid
        public Node[,] Nodes;
        // Size of each tile of the grid
        public int TileSize;

        public Grid(int Width, int Height, int TileSize) {
            Nodes = new Node[Width, Height];
            this.TileSize = TileSize;

            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    Nodes[x, y] = new Node(true, new Vector2((x * TileSize / 2f) + (x + 1) * (TileSize / 2f), (y * TileSize / 2f) + (y + 1) * (TileSize / 2f)));
                }
            }
        }

        // Returns Grid coordinates associated to a world position
        public Vector2 WorldToNode(Vector2 WorldPosition) {
            Vector2 Result = new Vector2();

            Result.X = (float)Math.Floor(WorldPosition.X / TileSize);
            Result.Y = (float)Math.Floor(WorldPosition.Y / TileSize);

            return Result;
        }

        // Returns the Node in the specified position of the Grid
        public Node NodeInPosition(Vector2 NodePosition) {
            return Nodes[(int)NodePosition.X, (int)NodePosition.Y];
        }
    }
}

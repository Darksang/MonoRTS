using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Grid {
        // 2D Array containing all nodes for the grid
        public Node[,] Nodes;
        // Width and Height of the Grid
        public int GridWidth, GridHeight;
        // Size of each tile of the grid (We assume same Height/Width)
        public int TileSize;

        public Grid(int Width, int Height, int TileSize) {
            Nodes = new Node[Width, Height];

            GridWidth = Width;
            GridHeight = Height;

            this.TileSize = TileSize;

            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    // TODO: Check tile to see if it's walkable or not
                    Nodes[x, y] = new Node(true, new Vector2((x * TileSize / 2f) + (x + 1) * (TileSize / 2f), (y * TileSize / 2f) + (y + 1) * (TileSize / 2f)), x, y);
                }
            }
        }

        public List<Node> GetNeighbours(Node Node) {
            List<Node> Neighbours = new List<Node>();

            // Get the 8 neighbours of the Node
            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    // Skip the node itself
                    if (x == 0 && y == 0)
                        continue;

                    // Make sure we don't go out of bounds
                    int NeighbourX = Node.GridX + x;
                    int NeighbourY = Node.GridY + y;

                    if (NeighbourX >= 0 && NeighbourX < GridWidth && NeighbourY >= 0 && NeighbourY < GridHeight)
                        Neighbours.Add(Nodes[NeighbourX, NeighbourY]);
                }
            }

            return Neighbours;
        }

        // Returns the Node associated to a world position
        public Node WorldToNode(Vector2 WorldPosition) {
            Vector2 NodePosition = new Vector2();

            NodePosition.X = (float)Math.Floor(WorldPosition.X / TileSize);
            NodePosition.Y = (float)Math.Floor(WorldPosition.Y / TileSize);

            if (NodePosition.X >= 0 && NodePosition.X < GridWidth && NodePosition.Y >= 0 && NodePosition.Y < GridHeight)
                return Nodes[(int)NodePosition.X, (int)NodePosition.Y];
            else
                return null;
        }
    }
}

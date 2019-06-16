using System;
using System.Collections.Generic;

using MonoGame.Extended.Tiled;

using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Grid {
        // 2D Array containing all nodes for the grid
        public Node[,] Nodes;
        // Width and Height of the Grid
        public int GridWidth, GridHeight;
        // Size of each tile of the grid (We assume same Height/Width)
        public int TileSize;

        public Grid(TiledMap Map) {
            Nodes = new Node[Map.Width, Map.Height];

            GridWidth = Map.Width;
            GridHeight = Map.Height;

            TileSize = Map.TileHeight;

            for (int x = 0; x < GridWidth; x++) {
                for (int y = 0; y < GridHeight; y++) {
                    // Check all Layers for this tile
                    bool Walkable = false;

                    foreach (TiledMapTileLayer L in Map.TileLayers) {
                        L.TryGetTile(x, y, out TiledMapTile? Tile);
                        int ID = Tile.Value.GlobalIdentifier;

                        if (ID == 311)
                            Walkable = true; // Grass
                        else if (ID == 389)
                            Walkable = true; // Sand
                        else if (ID == 726)
                            Walkable = true; // Base Floor
                        else if (ID == 853 || ID == 854 || ID == 855 || ID == 885 || ID == 887 || ID == 917 || ID == 918 || ID == 919 || ID == 888 || ID == 889)
                            Walkable = false; // Base Walls
                        else if (ID == 526 || ID == 527 || ID == 528)
                            Walkable = true; // Bridge
                        else if (ID == 6 || ID == 7 || ID == 8 || ID == 38 || ID == 39 || ID == 40 || ID == 70 || ID == 71 || ID == 72 || ID == 102 || ID == 103 || ID == 104 || ID == 134 || ID == 135 || ID == 136)
                            Walkable = false; // Mountain
                        else if (ID == 458)
                            Walkable = false; // Cactus
                    }

                    Nodes[x, y] = new Node(Walkable, new Vector2((x * TileSize / 2f) + (x + 1) * (TileSize / 2f), (y * TileSize / 2f) + (y + 1) * (TileSize / 2f)), x, y);
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

        // Returns the Node associated to a world position. Returns null if it's outside the grid.
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

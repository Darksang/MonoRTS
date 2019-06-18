using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Pathfinding {

        // Grid used to calculate a path
        public Grid Grid;

        public Pathfinding(Grid Grid) {
            this.Grid = Grid;
        }

        private float ManhattanDistance(Vector2 P, Vector2 Q) {
            return Math.Abs(P.X - Q.X) + Math.Abs(P.Y - Q.Y);
        }

        private float ChebyshevDistance(Vector2 P, Vector2 Q) {
            return Math.Max(Math.Abs(P.X - Q.X), Math.Abs(P.Y - Q.Y));
        }

        private float EuclideanDistance(Vector2 P, Vector2 Q) {
            return (float)Math.Sqrt( (float)Math.Pow(P.X - Q.X, 2f) + (float)Math.Pow(P.Y - Q.Y, 2f));
        }

        public Path FindPath(Vector2 Start, Vector2 End) {
            Node StartNode = Grid.WorldToNode(Start);
            Node EndNode = Grid.WorldToNode(End);

            if (StartNode == null || EndNode == null)
                return new Path();

            // Initialize the Open and Closed sets
            List<Node> OpenNodes = new List<Node>();
            OpenNodes.Add(StartNode);
            HashSet<Node> ClosedNodes = new HashSet<Node>();

            while (OpenNodes.Count > 0) {
                // Find the lowest cost Node in the Open set
                Node CurrentNode = OpenNodes[0];
                for (int i = 1; i < OpenNodes.Count; i++) {
                    if (OpenNodes[i].F < CurrentNode.F)
                        CurrentNode = OpenNodes[i];
                    // If their F cost is equal, we select the one with the lowest H
                    else if (OpenNodes[i].F == CurrentNode.F && OpenNodes[i].H < CurrentNode.H)
                        CurrentNode = OpenNodes[i];
                }

                OpenNodes.Remove(CurrentNode);
                ClosedNodes.Add(CurrentNode);

                // Found the path, return the solution
                if (CurrentNode == EndNode)
                    return BuildPath(StartNode, EndNode);

               // Loop through each neighbour
               foreach (Node Neighbour in Grid.GetNeighbours(CurrentNode)) {
                    // Skip the Node if it isn't walkable or is processed already
                    if (!Neighbour.Walkable || ClosedNodes.Contains(Neighbour))
                        continue;

                    float CostToNeighbour = CurrentNode.G + ManhattanDistance(CurrentNode.WorldPosition, Neighbour.WorldPosition);

                    if (CostToNeighbour < Neighbour.G || !OpenNodes.Contains(Neighbour)) {
                        // TODO: Add G cost?
                        Neighbour.G = CostToNeighbour;
                        Neighbour.H = ManhattanDistance(Neighbour.WorldPosition, EndNode.WorldPosition);
                        Neighbour.Parent = CurrentNode;

                        // If this Node hasn't been processed yet, add it to the Open set
                        if (!OpenNodes.Contains(Neighbour))
                            OpenNodes.Add(Neighbour);
                    }
                }
            }

            return new Path();
        }

        private Path BuildPath(Node Start, Node End) {
            Path Path = new Path();
            Node CurrentNode = End;

            while (CurrentNode != Start) {
                Path.Positions.Add(CurrentNode.WorldPosition);
                CurrentNode = CurrentNode.Parent;
            }

            Path.Positions.Add(CurrentNode.WorldPosition);

            Path.Positions.Reverse();
            return Path;
        }
    }
}

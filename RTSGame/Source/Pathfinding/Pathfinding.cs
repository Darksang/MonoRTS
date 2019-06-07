using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Pathfinding {
        // Grid used to calculate a path
        public Grid Grid;

        public Pathfinding(Grid Grid) {
            this.Grid = Grid;
        }

        private float ManhattanDistance(Vector2 P, Vector2 Q) {
            return Math.Abs(P.X + Q.X) + Math.Abs(P.Y + Q.Y);
        }

        private float ChebyshevDistance(Vector2 P, Vector2 Q) {
            return Math.Max(Math.Abs(P.X - Q.X), Math.Abs(P.Y - Q.Y));
        }

        private float EuclideanDistance(Vector2 P, Vector2 Q) {
            return (float)Math.Sqrt( (float)Math.Pow(P.X - Q.X, 2f) + (float)Math.Pow(P.Y - Q.Y, 2f));
        }

        public void Path(Vector2 Start, Vector2 End) {

        }
    }
}

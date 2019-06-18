using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using MonoGame.Extended;

namespace RTSGame {

    public class GameManager {
        // Holds the position of the Red Team base
        public Vector2 RedTeamBase { get; set; }

        // Holds the position of the Blue Team base
        public Vector2 BlueTeamBase { get; set; }

        // Holds the area of the Red Team healing point
        public RectangleF RedHealingPoint { get; set; }

        // Holds the area of the Blue Team healing point
        public RectangleF BlueHealingPoint { get; set; }

        // Holds the Red Team leader
        public LeaderUnit RedLeader { get; set; }

        // Holds the Blue Team leader
        public LeaderUnit BlueLeader { get; set; }

        public GameManager() {
            RedTeamBase = BlueTeamBase = Vector2.Zero;

            RedHealingPoint = new Rectangle();
            BlueHealingPoint = new Rectangle();

            RedLeader = null;
            BlueLeader = null;
        }

        public bool IsGameOver() {
            if (RedLeader.Stats.Health <= 0)
                return true;
            else if (BlueLeader.Stats.Health <= 0)
                return true;

            return false;
        }

        public Team GetWinningTeam() {
            if (IsGameOver())
                if (RedLeader.Stats.Health <= 0)
                    return Team.Blue;
                else if (BlueLeader.Stats.Health <= 0)
                    return Team.Red;

            return Team.None;
        }
    }
}

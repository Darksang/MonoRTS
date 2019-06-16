using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using FarseerPhysics.Dynamics;

namespace RTSGame {

    public class LeaderUnit : Unit {

        public LeaderUnit(string Name, Sprite Sprite, World World) : base(Name, Sprite, World) {
            TerrainSpeed.Add(TerrainType.Grass, 70f);
            TerrainSpeed.Add(TerrainType.Sand, 50f);
            TerrainSpeed.Add(TerrainType.BaseFloor, 100f);

            Body.MaxVelocity = 70f;

            Stats.Health = 2000;
            Stats.Attack = 150;
            Stats.Defense = 150;

            Stats.AttackRange = 40f;
            Stats.AttackSpeed = 1.3f;
            Stats.CriticalChance = 20f;
        }

        public LeaderUnit(string Name, Sprite Sprite, World World, Vector2 Scale) : base(Name, Sprite, World, Scale) {
            TerrainSpeed.Add(TerrainType.Grass, 70f);
            TerrainSpeed.Add(TerrainType.Sand, 50f);
            TerrainSpeed.Add(TerrainType.BaseFloor, 100f);

            Body.MaxVelocity = 70f;

            Stats.Health = 2000;
            Stats.Attack = 150;
            Stats.Defense = 150;

            Stats.AttackRange = 40f;
            Stats.AttackSpeed = 1.3f;
            Stats.CriticalChance = 20f;
        }
    }
}

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using FarseerPhysics.Dynamics;

namespace RTSGame {

    public class RangedUnit : Unit {

        public RangedUnit(string Name, Sprite Sprite, World World) : base(Name, Sprite, World) {
            TerrainSpeed.Add(TerrainType.Grass, 100f);
            TerrainSpeed.Add(TerrainType.Sand, 50f);
            TerrainSpeed.Add(TerrainType.BaseFloor, 80f);

            Body.MaxVelocity = 100f;

            Stats.Health = 550;
            Stats.Attack = 350;
            Stats.Defense = 90;

            Stats.AttackRange = 200f;
            Stats.AttackSpeed = 1.5f;
            Stats.CriticalChance = 25f;
        }

        public RangedUnit(string Name, Sprite Sprite, World World, Vector2 Scale) : base(Name, Sprite, World, Scale) {
            TerrainSpeed.Add(TerrainType.Grass, 100f);
            TerrainSpeed.Add(TerrainType.Sand, 50f);
            TerrainSpeed.Add(TerrainType.BaseFloor, 80f);

            Body.MaxVelocity = 100f;

            Stats.Health = 550;
            Stats.Attack = 350;
            Stats.Defense = 90;

            Stats.AttackRange = 200f;
            Stats.AttackSpeed = 1.5f;
            Stats.CriticalChance = 25f;
        }
    }
}

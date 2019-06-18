using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using FarseerPhysics.Dynamics;

namespace RTSGame {

    public class AssassinUnit : Unit {

        public AssassinUnit(string Name, Sprite Sprite, World World) : base(Name, Sprite, World) {
            TerrainSpeed.Add(TerrainType.Grass, 110f);
            TerrainSpeed.Add(TerrainType.Sand, 130f);
            TerrainSpeed.Add(TerrainType.BaseFloor, 90f);

            Body.MaxVelocity = 110f;
            Body.MaxAcceleration = 1000f;

            Stats.Health = 400;
            Stats.Attack = 500;
            Stats.Defense = 100;

            Stats.AttackRange = 50f;
            Stats.AttackSpeed = 1f;
            Stats.CriticalChance = 40f;

            Stats.FieldOfView = 300f;
        }

        public AssassinUnit(string Name, Sprite Sprite, World World, Vector2 Scale) : base(Name, Sprite, World, Scale) {
            TerrainSpeed.Add(TerrainType.Grass, 110f);
            TerrainSpeed.Add(TerrainType.Sand, 130f);
            TerrainSpeed.Add(TerrainType.BaseFloor, 90f);

            Body.MaxVelocity = 110f;
            Body.MaxAcceleration = 1000f;

            Stats.Health = 400;
            Stats.Attack = 500;
            Stats.Defense = 100;

            Stats.AttackRange = 50f;
            Stats.AttackSpeed = 1f;
            Stats.CriticalChance = 40f;

            Stats.FieldOfView = 300f;
        }
    }
}

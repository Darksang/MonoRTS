using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using FarseerPhysics.Dynamics;

namespace RTSGame {

    public class MeleeUnit : Unit {

        public MeleeUnit(string Name, Sprite Sprite, World World) : base(Name, Sprite, World) {
            TerrainSpeed.Add(TerrainType.Grass, 90f);
            TerrainSpeed.Add(TerrainType.Sand, 70f);
            TerrainSpeed.Add(TerrainType.BaseFloor, 80f);

            Body.MaxVelocity = 90f;

            Stats.Health = 900;
            Stats.Attack = 200;
            Stats.Defense = 120;

            Stats.AttackRange = 40f;
            Stats.AttackSpeed = 1.1f;
            Stats.CriticalChance = 15f;

            Stats.FieldOfView = 250f;
        }

        public MeleeUnit(string Name, Sprite Sprite, World World, Vector2 Scale) : base(Name, Sprite, World, Scale) {
            TerrainSpeed.Add(TerrainType.Grass, 90f);
            TerrainSpeed.Add(TerrainType.Sand, 70f);
            TerrainSpeed.Add(TerrainType.BaseFloor, 80f);

            Body.MaxVelocity = 90f;

            Stats.Health = 900;
            Stats.Attack = 200;
            Stats.Defense = 120;

            Stats.AttackRange = 40f;
            Stats.AttackSpeed = 1.1f;
            Stats.CriticalChance = 15f;

            Stats.FieldOfView = 250f;
        }
    }
}

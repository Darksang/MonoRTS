using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using FarseerPhysics.Dynamics;

namespace RTSGame {

    public class RangedUnit : Unit {

        public RangedUnit(string Name, Sprite Sprite, World World) : base(Name, Sprite, World) {
            Body.MaxVelocity = 110f;

            Stats.Health = 550;
            Stats.Attack = 350;
            Stats.Defense = 90;

            Stats.AttackRange = 200f;
            Stats.AttackSpeed = 1.5f;
            Stats.CriticalChance = 25f;
        }

        public RangedUnit(string Name, Sprite Sprite, World World, Vector2 Scale) : base(Name, Sprite, World, Scale) {
            Body.MaxVelocity = 110f;

            Stats.Health = 550;
            Stats.Attack = 350;
            Stats.Defense = 90;

            Stats.AttackRange = 200f;
            Stats.AttackSpeed = 1.5f;
            Stats.CriticalChance = 25f;
        }
    }
}

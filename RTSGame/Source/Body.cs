using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Body {

        // Whether the body can move or not
        public bool CanMove { get; set; }

        // Current body velocity
        public Vector2 Velocity { get; set; }
        // Current rotation
        public float Rotation { get; set; }

        // Maximum Velocity
        public float MaxVelocity { get; set; }
        // Maximum Rotation
        public float MaxRotation { get; set; }

        // Maximum Acceleration
        public float MaxAcceleration { get; set; }
        // Maximum Angular
        public float MaxAngular { get; set; }

        // Interior radius used in Arrive, AcceleratedArrive....
        public float InteriorRadius { get; set; }
        // Exterior radius used in Arrive, AcceleratedArrive....
        public float ExteriorRadius { get; set; }

        // Interior angle used in Align, AntiAlign.....
        //public float InteriorAngle { get; set; }
        // Exterior angles used in Align, AntiAlign.....
        //public float ExteriorAngle { get; set; }

        public Body() {
            CanMove = true;

            Velocity = new Vector2(0f, 0f);
            Rotation = 0f;

            // TODO: Test values to determine which are better
            MaxVelocity = 200f;
            MaxRotation = 10f;

            MaxAcceleration = 0.5f;
            MaxAngular = 0.5f;

            InteriorRadius = 100f;
            ExteriorRadius = 150f;
        }
    }
}

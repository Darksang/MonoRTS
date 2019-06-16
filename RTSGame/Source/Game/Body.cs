using Microsoft.Xna.Framework;

namespace RTSGame {

    public class Body {

        // Whether the body can move or not
        public bool CanMove;

        // Current body velocity
        public Vector2 Velocity;
        // Current rotation velocity
        public float RotationVelocity;

        // Maximum Velocity
        public float MaxVelocity;
        // Maximum Rotation
        private float _maxRotationVelocity;
        public float MaxRotationVelocity { get { return _maxRotationVelocity; } set { _maxRotationVelocity = MathHelper.ToRadians(value); } }

        // Maximum Acceleration
        public float MaxAcceleration;
        // Maximum Angular
        private float _maxAngular;
        public float MaxAngular { get { return _maxAngular; } set { _maxAngular = MathHelper.ToRadians(value); } }

        // Interior radius used in Arrive...
        public float InteriorRadius;
        // Exterior radius used in Arrive...
        public float ExteriorRadius;

        // Interior angle used in Align, AntiAlign.....
        private float _interiorAngle;
        public float InteriorAngle { get { return _interiorAngle; } set { _interiorAngle = MathHelper.ToRadians(value); } }
        // Exterior angles used in Align, AntiAlign.....
        private float _exteriorAngle;
        public float ExteriorAngle { get { return _exteriorAngle; } set { _exteriorAngle = MathHelper.ToRadians(value); } }

        public Body() {
            CanMove = true;

            Velocity = new Vector2(0f, 0f);
            RotationVelocity = 0f;

            MaxVelocity = 70f;
            MaxRotationVelocity = 90f;

            MaxAcceleration = 500f;
            MaxAngular = 100f;

            InteriorRadius = 70f;
            ExteriorRadius = 250f;

            InteriorAngle = 5f;
            ExteriorAngle = 15f;
        }

        public void ClipVelocity() {
            Velocity.Normalize();
            Velocity *= MaxVelocity;
        }
    }
}

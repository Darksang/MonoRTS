using Microsoft.Xna.Framework;

namespace RTSGame {

    public static class ConvertUnits {
        // Display to Simulation -> 100 pixels is 1 meter
        private static float DisplayUnitsToSimUnitsRatio = 100f;
        // Inverse operation -> 1 meter is 100 pixels
        private static float SimUnitsToDisplayUnitsRatio = 1f / DisplayUnitsToSimUnitsRatio;

        public static void SetDisplayUnitToSimRatio(float Ratio) {
            DisplayUnitsToSimUnitsRatio = Ratio;
            SimUnitsToDisplayUnitsRatio = 1f / DisplayUnitsToSimUnitsRatio;
        }

        public static float ToDisplayUnits(float SimUnits) {
            return SimUnits * DisplayUnitsToSimUnitsRatio;
        }

        public static Vector2 ToDisplayUnits(Vector2 SimUnits) {
            return SimUnits * DisplayUnitsToSimUnitsRatio;
        }

        public static float ToSimUnits(float DisplayUnits) {
            return DisplayUnits * SimUnitsToDisplayUnitsRatio;
        }

        public static Vector2 ToSimUnits(Vector2 DisplayUnits) {
            return DisplayUnits * SimUnitsToDisplayUnitsRatio;
        }
    }
}

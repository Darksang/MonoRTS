using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace RTSGame {

    public class DefensiveCirclePattern : FormationPattern {
        // The radius of one character
        public float CharacterRadius { get; set; }
        // Holds the number of slots currently in the pattern
        public int NumberOfSlots { get; set; }

        public DefensiveCirclePattern(float CharacterRadius) {
            this.CharacterRadius = CharacterRadius;
        }

        public Transform GetDriftOffset(List<Slot> SlotAssignments) {
            // Compute center of mass
            Transform Center = new Transform();

            // Go through each assignment, and add its contribution to the center
            for (int i = 0; i < SlotAssignments.Count; i++) {
                Transform Location = GetSlotLocation(i);
                Center.Position += Location.Position;
                Center.Rotation += Location.Rotation;
            }

            // Divide through to get the drift offset
            Center.Position /= SlotAssignments.Count;
            Center.Rotation /= SlotAssignments.Count;

            return Center;
        }

        public Transform GetSlotLocation(int SlotNumber) {
            // We place the slots around a circle based on their slot number
            float AngleAroundCircle = (float)(SlotNumber / NumberOfSlots * Math.PI * 2f);

            // The radius depends on the radius of the character, and the number of characters in the circle
            float Radius = (float)(CharacterRadius / Math.Sin(Math.PI / NumberOfSlots));

            // Create a location
            Transform TargetLocation = new Transform();
            TargetLocation.Position = new Vector2((float)(Radius * Math.Cos(AngleAroundCircle)), (float)(Radius * Math.Sin(AngleAroundCircle)));
            TargetLocation.Rotation = AngleAroundCircle;

            return TargetLocation;
        }

        public bool SupportsSlots(int Slots) {
            // This pattern supports any number of slots
            return true;
        }
    }
}

using System.Collections.Generic;

namespace RTSGame {

    public interface FormationPattern {
        // Holds the number of slots currently in the pattern
        int NumberOfSlots { get; set; }

        // Calculates the drift offset when units are in given set of slots
        Transform GetDriftOffset(List<Slot> SlotAssignments);

        // Gets the location (Position and Orientation) of the given slot index
        Transform GetSlotLocation(int Slot);

        // Returns true if the pattern can support the given number of slots
        bool SupportsSlots(int Slots);
    }
}

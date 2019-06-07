using System;
using System.Collections.Generic;

namespace RTSGame {

    public class FormationManager {

        // List of slots assignments
        private List<Slot> Slots;
        // Holds a position and orientation that represents the drift offset
        private Transform DriftOffset;
        // Formation pattern
        private FormationPattern Pattern;

        public FormationManager(FormationPattern Pattern) {
            Slots = new List<Slot>();
            DriftOffset = new Transform();
            this.Pattern = Pattern;
        }

        public void UpdateSlotAssignments() {
            // Assign sequential slot numbers
            for (int i = 0; i < Slots.Count; i++)
                Slots[i].SlotNumber = i;

            // Update drift offset
            DriftOffset = Pattern.GetDriftOffset(Slots);
        }

        public void UpdateUnits() {
            // Go through each unit
            foreach (Slot S in Slots) {
                // Get location for this slot relative to the Anchor
                Transform RelativeLocation = S.Unit.Transform;

                Transform TargetLocation = new Transform();
            }
        }

        // Add a new Unit to the first available slot, returns false if there are no free slots
        public bool AddUnit(Unit Unit) {
            // Find out how many slots we have occupied
            int OccupiedSlots = Slots.Count;

            // Check if the pattern supports more slots
            if (Pattern.SupportsSlots(OccupiedSlots + 1)) {
                // Add a new slot
                Slot NewSlot = new Slot();

                NewSlot.Unit = Unit;

                Slots.Add(NewSlot);

                // Update the slots assignments
                UpdateSlotAssignments();

                return true;
            }

            return false;
        }

        // Removes a Unit from its slot
        public void RemoveUnit(Unit Unit) {
            // Find the character's slot
            Slot SlotToRemove = null;
            foreach (Slot S in Slots)
                if (S.Unit == Unit) {
                    SlotToRemove = S;
                    break;
                }

            // Make sure we have found a valid result
            if (Slots.Contains(SlotToRemove)) {
                // Remove the slot
                Slots.Remove(SlotToRemove);
                // Update the assignments
                UpdateSlotAssignments();
            }
        }
    }
}

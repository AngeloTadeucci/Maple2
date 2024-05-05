using Maple2.Model.Game;

namespace Maple2.Server.Game.Manager.Config;

public class HotBar {
    private const int MAX_SLOTS = 25;
    private const int ASSIGNABLE_SLOTS = 22;
    // Slot order is used if target slot is -1
    private static readonly int[] SLOT_ORDER = { 4, 5, 6, 7, 0, 1, 2, 3, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 };

    public QuickSlot[] Slots { get; } = new QuickSlot[MAX_SLOTS];

    public HotBar(QuickSlot[]? slots = null) {
        if (slots == null) {
            return;
        }

        int limit = Math.Min(MAX_SLOTS, slots.Length);
        for (int i = 0; i < limit; i++) {
            Slots[i] = slots[i];
        }
    }

    public void MoveQuickSlot(int targetIndex, in QuickSlot quickSlot, bool replace = true) {
        if (targetIndex is < 0 or >= ASSIGNABLE_SLOTS) {
            if (replace) {
                targetIndex = 0; // Replace slot 0 if no free slot is found
            }
            foreach (int slotIndex in SLOT_ORDER) {
                if (Slots[slotIndex] == default) {
                    targetIndex = slotIndex;
                    break;
                }
            }
        }

        // If no slots were found, return
        if (targetIndex is < 0 or >= ASSIGNABLE_SLOTS) {
            return;
        }

        int sourceSlotIndex = FindQuickSlotIndex(quickSlot.SkillId, quickSlot.ItemUid);
        if (sourceSlotIndex != -1) {
            // Swapping with an existing slot on the hotBar
            QuickSlot srcSlot = Slots[targetIndex];
            Slots[sourceSlotIndex] = new QuickSlot(
                srcSlot.SkillId,
                srcSlot.ItemId,
                srcSlot.ItemUid
            );
        }

        Slots[targetIndex] = quickSlot;
    }

    public int FindQuickSlotIndex(int skillId, long itemUid = 0) {
        for (int i = 0; i < MAX_SLOTS; i++) {
            QuickSlot currentSlot = Slots[i];
            if (currentSlot.SkillId == skillId && currentSlot.ItemUid == itemUid) {
                return i;
            }
        }

        return -1;
    }

    public bool RemoveQuickSlot(int skillId, long itemUid) {
        int targetIndex = FindQuickSlotIndex(skillId, itemUid);
        if (targetIndex is < 0 or >= MAX_SLOTS) {
            return false;
        }

        Slots[targetIndex] = default; // Clear
        return true;
    }
}

using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Items;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Serilog;

namespace Maple2.Server.Game.Manager;

public sealed class BeautyManager : IDisposable {
    private readonly GameSession session;
    private readonly ItemCollection items;
    private Item? previousHair;
    public BeautyManager(GameSession session) {
        this.session = session;
        items = new ItemCollection((short) (Constant.BaseStorageCount + session.Player.Value.Unlock.HairSlotExpand));
        using GameStorage.Request db = session.GameStorage.Context();
        foreach (Item item in db.GetSavedHairs(session.CharacterId).Where(item => items.Add(item).Count == 0)) {
            Log.Error("Failed to add saved hair:{Uid}", item.Uid);
        }
    }

    public void Dispose() {
        using GameStorage.Request db = session.GameStorage.Context();
        db.SaveItems(session.CharacterId, items.ToArray());
    }

    public void Load() {
        session.Send(BeautyPacket.StartList());
        session.Send(BeautyPacket.SaveSlots(session.Player.Value.Unlock.HairSlotExpand));
        session.Send(BeautyPacket.ListCount(items.Count));
        if (items.Count > 0) {
            session.Send(BeautyPacket.ListHair(items.OrderBy(hair => hair.CreationTime).ToList()));
            ;
        }
    }

    public void AddHair(long uid) {
        Item? hair = session.Item.GetOutfit(uid);
        if (hair == null || !hair.Type.IsHair) {
            return;
        }

        if (items.Contains(uid)) {
            return;
        }

        if (items.OpenSlots <= 0) {
            return;
        }

        Item? hairCopy = hair.Clone();
        hairCopy.Group = ItemGroup.SavedHair;
        using GameStorage.Request db = session.GameStorage.Context();
        hairCopy = db.CreateItem(session.CharacterId, hairCopy);
        if (hairCopy == null) {
            return;
        }
        items.Add(hairCopy);
        session.Send(BeautyPacket.SaveHair(hair, hairCopy));
        session.ConditionUpdate(ConditionType.beauty_style_add, codeLong: hairCopy.Id);
    }

    public bool RemoveHair(long uid) {
        if (!items.Remove(uid, out Item? hair)) {
            return false;
        }

        session.Item.Inventory.Discard(hair);
        session.Send(BeautyPacket.DeleteHair(uid));
        return true;
    }

    public void SavePreviousHair(Item? prevHair) => previousHair = prevHair;

    public void SelectPreviousHair() {
        if (previousHair == null) {
            return;
        }
        session.Item.Equips.EquipCosmetic(previousHair, EquipSlot.HR);
    }

    public void ClearPreviousHair() => previousHair = null;

    public bool EquipSavedCosmetic(long uid) {
        Item? cosmetic = items.Get(uid);
        if (cosmetic == null) {
            return false;
        }

        // invalid cosmetic
        if (cosmetic.Metadata.SlotNames.Length > 1) {
            return false;
        }

        // Clone() causes copy to have the same Uid as the original. However, CreateItem() will generate a new Uid.
        Item? copy = cosmetic.Clone();
        copy.Group = ItemGroup.Default;

        using GameStorage.Request db = session.GameStorage.Context();
        copy = db.CreateItem(session.CharacterId, copy);
        if (copy == null) {
            return false;
        }

        if (!session.Item.Equips.EquipCosmetic(copy, copy.Metadata.SlotNames[0])) {
            db.SaveItems(0, copy);
            return false;
        }
        session.Send(BeautyPacket.ApplySavedHair());
        session.ConditionUpdate(ConditionType.beauty_style_apply, codeLong: copy.Id);
        return true;
    }
}

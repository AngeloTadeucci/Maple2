using Maple2.Model;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class TutorialItemHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.RequestTutorialItem;

    public override void Handle(GameSession session, IByteReader packet) {
        if (session.Player.Value.Character.Level > 1) {
            return;
        }

        var jobTable = session.TableMetadata.JobTable;

        var jobCode = session.Player.Value.Character.Job.Code();

        if (!jobTable.Entries.TryGetValue(jobCode, out var jobMetadata)) {
            return;
        }

        if (session.Field == null || jobMetadata.Tutorial.StartField != session.Field.MapId) {
            return;
        }

        foreach (IGrouping<int, JobTable.Item> tutorialItemGroup in jobMetadata.Tutorial.StartItem.GroupBy(item => item.Id)) {
            int tutorialItemsInInventory = session.Item.Inventory.Find(tutorialItemGroup.Key).Count();

            tutorialItemsInInventory += session.Item.Equips.Gear.Count(x => x.Value.Id == tutorialItemGroup.Key);

            int tutorialItemsToSpawn = tutorialItemGroup.Select(item => item.Count).Sum();

            if (tutorialItemsInInventory >= tutorialItemsToSpawn) {
                continue;
            }

            Item? tutorialItem = session.Field?.ItemDrop.CreateItem(tutorialItemGroup.Key);
            if (tutorialItem == null) {
                continue;
            }

            while (tutorialItemsInInventory < tutorialItemsToSpawn) {
                session.Item.Inventory.Add(tutorialItem, true);
                tutorialItemsInInventory++;
            }
        }
    }
}

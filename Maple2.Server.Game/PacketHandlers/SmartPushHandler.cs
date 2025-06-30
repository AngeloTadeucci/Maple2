using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class SmartPushHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.SmartPush;

    public override void Handle(GameSession session, IByteReader packet) {
        int smartPushId = packet.ReadInt();
        if (!session.TableMetadata.SmartPushTable.Entries.TryGetValue(smartPushId, out SmartPushMetadata? metadata)) {
            return;
        }

        switch (metadata.Type) {
            case SmartPushType.autoInteraction:
                HandleAutoInteraction(session, packet, metadata);
                break;
            case SmartPushType.additionalEffect:
                HandleAdditionalEffect(session, packet, metadata);
                break;
            default:
                Logger.Error("Unhandled smart push type: {SmartPushType}", metadata.Type);
                break;
        }

    }

    private void HandleAutoInteraction(GameSession session, IByteReader packet, SmartPushMetadata metadata) {
        int amount = packet.ReadInt();
        int recipeId = packet.ReadInt();

        if (!session.TableMetadata.MasteryRecipeTable.Entries.TryGetValue(recipeId, out MasteryRecipeTable.Entry? recipeMetadata)) {
            return;
        }

        int successCount = 0;
        for (int i = 0; i < amount; i++) {
            if (metadata.RequiredItem.Tag != ItemTag.None && !session.Item.Inventory.Consume([metadata.RequiredItem])) {
                return;
            }

            float successRate = session.Mastery.GatherSuccessRate(recipeMetadata);
            if (successRate <= 0.0f) {
                break;
            }

            session.Mastery.BeforeGather(recipeMetadata);
            session.Mastery.Gather(recipeMetadata, session.Player.Position, session.Player.Rotation);
            successCount++;
        }

        session.Send(SmartPushPacket.ActivateGather(metadata.Id, successCount));
    }

    private void HandleAdditionalEffect(GameSession session, IByteReader packet, SmartPushMetadata metadata) {
        if (session.Field is null) return;
        int packageId = packet.ReadInt();
        if (metadata.RequiredItem.Tag != ItemTag.None && !session.Item.Inventory.Consume([metadata.RequiredItem])) {
            return;
        }

        if (metadata.Content is "AutoFishing" or "AutoPlayInstrument") {
            if (!AutoActionPackage(session, metadata, packageId)) {
                return;
            }
        } else {
            if (session.Currency.Meret < metadata.MeretCost) {
                return;
            }
            session.Currency.Meret -= metadata.MeretCost;
        }

        session.Buffs.AddBuff(session.Player, session.Player, (int) metadata.Value, 1, session.Field.FieldTick);
    }

    private bool AutoActionPackage(GameSession session, SmartPushMetadata smartPushMetadata, int packageId) {
        string content = smartPushMetadata.Content;
        switch (content) {
            case "AutoFishing":
                if (session.FindEvent(GameEventType.SaleAutoFishing).FirstOrDefault()?.Metadata.Data is SaleAutoFishing saleAutoFishing) {
                    content = saleAutoFishing.ContentType;
                }
                break;
            case "AutoPlayInstrument":
                if (session.FindEvent(GameEventType.SaleAutoPlayInstrument).FirstOrDefault()?.Metadata.Data is SaleAutoPlayInstrument saleAutoPlayInstrument) {
                    content = saleAutoPlayInstrument.ContentType;
                }
                break;
            default:
                Logger.Error("Unhandled auto action content: {Content}", content);
                return false;
        }


        if (!session.TableMetadata.AutoActionTable.Entries.TryGetValue(content, out IReadOnlyDictionary<int, AutoActionMetaData>? packages) ||
            !packages.TryGetValue(packageId, out AutoActionMetaData? packageMetadata)) {
            return false;
        }

        SmartPushCurrencyType currencyType;
        if (packageMetadata.MeretCost > 0) {
            currencyType = SmartPushCurrencyType.Meret;
        } else if (packageMetadata.MesoCost > 0) {
            currencyType = SmartPushCurrencyType.Meso;
        } else {
            currencyType = SmartPushCurrencyType.None;
        }

        // Check if player has enough currency
        switch (currencyType) {
            case SmartPushCurrencyType.Meret:
                if (session.Currency.Meret < packageMetadata.MeretCost) {
                    return false;
                }
                session.Currency.Meret -= packageMetadata.MeretCost;
                break;
            case SmartPushCurrencyType.Meso:
                if (session.Currency.Meso < packageMetadata.MesoCost) {
                    return false;
                }
                session.Currency.Meso -= packageMetadata.MesoCost;
                break;
            case SmartPushCurrencyType.None:
                break;
        }

        session.Send(SmartPushPacket.ActivateEffect(currencyType, (int) smartPushMetadata.Value));
        return true;
    }
}

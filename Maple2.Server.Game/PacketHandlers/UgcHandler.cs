using Grpc.Core;
using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Ugc;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.World.Service;
using WorldClient = Maple2.Server.World.Service.World.WorldClient;


namespace Maple2.Server.Game.PacketHandlers;

public class UgcHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.Ugc;

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required WebStorage WebStorage { private get; init; }
    public required ItemMetadataStorage ItemMetadata { private get; init; }
    public required TableMetadataStorage TableMetadata { private get; init; }
    public required WorldClient World { private get; init; }
    // ReSharper restore All
    #endregion

    private enum Command : byte {
        Upload = 1,
        Confirmation = 3,
        ProfilePicture = 11,
        LoadBanners = 18,
        ReserveBanner = 19,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Upload:
                HandleUpload(session, packet);
                return;
            case Command.Confirmation:
                HandleConfirmation(session, packet);
                return;
            case Command.ProfilePicture:
                HandleProfilePicture(session, packet);
                return;
            case Command.LoadBanners:
                HandleLoadBanners(session, packet);
                return;
            case Command.ReserveBanner:
                HandleReserveBanner(session, packet);
                return;
        }
    }

    private void HandleUpload(GameSession session, IByteReader packet) {
        packet.ReadLong();
        var info = packet.Read<UgcInfo>();
        packet.ReadLong();
        packet.ReadInt();
        packet.ReadShort();
        packet.ReadShort(); // -256

        switch (info.Type) {
            case UgcType.Item:
            case UgcType.Furniture:
            case UgcType.Mount:
                UploadItem(session, packet, info.Type);
                return;
            case UgcType.Banner:
                UploadBanner(session, packet);
                return;
            case UgcType.GuildEmblem:
                UploadGuildEmblem(session, packet);
                return;
            case UgcType.GuildBanner:
                UploadGuildBanner(session, packet);
                return;
            default:
                Logger.Information("Unimplemented Ugc Type: {type}", info.Type);
                return;
        }
    }

    private void UploadItem(GameSession session, IByteReader packet, UgcType ugcType) {
        long itemUid = packet.ReadLong();
        int itemId = packet.ReadInt();
        int amount = packet.ReadInt();
        string name = packet.ReadUnicodeString();
        packet.ReadByte();
        long cost = packet.ReadLong();
        bool useVoucher = packet.ReadBool();

        if (!TableMetadata.UgcDesignTable.Entries.TryGetValue(itemId, out UgcDesignTable.Entry? ugcMetadata)) {
            return;
        }

        if (session.Item.Inventory.FreeSlots(InventoryType.Outfit) <= 0) {
            return;
        }

        if (useVoucher) {
            Item? voucher = session.Item.Inventory.Filter(item => item.Metadata.Property.Tag == ItemTag.FreeDesignCoupon).FirstOrDefault();
            if (voucher != null) {
                if (!session.Item.Inventory.Consume(voucher.Uid, 1)) {
                    session.Send(NoticePacket.Notice(NoticePacket.Flags.Alert | NoticePacket.Flags.Message, StringCode.s_err_invalid_item));
                    return;
                }
            }
        } else {
            switch (ugcMetadata.CurrencyType) {
                case MeretMarketCurrencyType.Meso:
                    if (session.Currency.CanAddMeso(-ugcMetadata.CreatePrice) != -ugcMetadata.CreatePrice) {
                        session.Send(NoticePacket.MessageBox(StringCode.s_err_lack_meso));
                        return;
                    }
                    session.Currency.Meso -= ugcMetadata.CreatePrice;
                    break;
                case MeretMarketCurrencyType.Meret:
                    if (session.Currency.CanAddMeret(-ugcMetadata.CreatePrice) != -ugcMetadata.CreatePrice) {
                        session.Send(NoticePacket.MessageBox(StringCode.s_err_lack_merat));
                        return;
                    }
                    session.Currency.Meret -= ugcMetadata.CreatePrice;
                    break;
                case MeretMarketCurrencyType.RedMeret:
                    if (session.Currency.CanAddGameMeret(-ugcMetadata.CreatePrice) != -ugcMetadata.CreatePrice) {
                        session.Send(NoticePacket.MessageBox(StringCode.s_err_lack_merat_red));
                    }
                    session.Currency.GameMeret -= ugcMetadata.CreatePrice;
                    break;
                default:
                    Logger.Error("Unhandled currency type {UgcMetadataCurrencyType}", ugcMetadata.CurrencyType);
                    return;
            }
        }

        Item? item = session.Field.ItemDrop.CreateItem(itemId, ugcMetadata.ItemRarity);
        if (item == null) {
            Logger.Fatal("Failed to create UGC item {ItemId}", itemId);
            throw new InvalidOperationException($"Fatal: Creating UGC item: {itemId}");
        }

        using WebStorage.Request request = WebStorage.Context();
        UgcResource? resource = request.CreateUgc(ugcType, session.CharacterId);
        if (resource == null) {
            Logger.Fatal("Failed to create UGC resource for item {ItemUid}", item.Uid);
            throw new InvalidOperationException($"Fatal: Creating UGC resource: {item.Uid}");
        }

        item.Template = new UgcItemLook {
            Id = resource.Id,
            AccountId = session.AccountId,
            Author = session.PlayerName,
            CharacterId = session.CharacterId,
            CreationTime = DateTime.Now.ToEpochSeconds(),
            Name = name,
        };

        session.StagedUgcItem = item;
        session.Send(UgcPacket.Upload(resource));
    }

    private void UploadBanner(GameSession session, IByteReader packet) {
        long bannerId = packet.ReadLong();

        session.Field.Banners.TryGetValue(bannerId, out FieldUgcBanner? banner);
        if (banner is null) {
            Logger.Warning("Failed to find banner {BannerId}", bannerId);
            return;
        }

        BannerTable.Entry? bannerMetadata = TableMetadata.BannerTable.Entries.FirstOrDefault(x => x.Id == bannerId);
        if (bannerMetadata is null) {
            Logger.Warning("Failed to find banner metadata {BannerId}", bannerId);
            return;
        }

        List<BannerSlot> slots = [];

        byte hours = packet.ReadByte();
        for (int i = 0; i < hours; i++) {
            var reservation = packet.Read<UgcBannerReservation>();

            long price = bannerMetadata.Price[reservation.Hour];
            if (session.Currency.Meret < price) {
                session.Send(NoticePacket.MessageBox(StringCode.s_err_lack_merat));
                return;
            }

            BannerSlot? bannerSlot = banner.Slots.FirstOrDefault(slot => slot.Id == reservation.Uid);
            if (bannerSlot is null) {
                Logger.Warning("Failed to find banner slot {BannerId} {SlotId}", bannerId, reservation.Uid);
                return;
            }

            slots.Add(bannerSlot);
            session.Currency.Meret -= price;
        }

        using WebStorage.Request request = WebStorage.Context();
        UgcResource? resource = request.CreateUgc(UgcType.Banner, session.CharacterId);
        if (resource == null) {
            Logger.Fatal("Failed to create UGC resource for banner {BannerId}", bannerId);
            throw new InvalidOperationException($"Fatal: Creating UGC resource: {bannerId}");
        }

        UgcItemLook ugc = new() {
            Id = resource.Id,
            AccountId = session.AccountId,
            Author = session.PlayerName,
            CharacterId = session.CharacterId,
            CreationTime = DateTime.Now.ToEpochSeconds(),
            Name = $"AD Banner {bannerId}"
        };

        foreach (BannerSlot slot in slots) {
            slot.Template = ugc;

            BannerSlot? oldSlot = banner.Slots.First(x => x.Id == slot.Id);
            banner.Slots.Remove(oldSlot);
            banner.Slots.Add(slot);
        }

        session.Send(UgcPacket.Upload(resource));
    }

    private void UploadGuildBanner(GameSession session, IByteReader packet) {
        long guildId = packet.ReadLong();
        int bannerId = packet.ReadInt();

        if (session.Guild.Guild is null) {
            Logger.Warning("Failed to find guild for UGC {GuildId}", guildId);
            return;
        }

        if (session.Guild.Id != guildId) {
            Logger.Warning("Invalid guild for UGC {GuildId}", guildId);
            return;
        }

        using WebStorage.Request request = WebStorage.Context();
        UgcResource? resource = request.CreateUgc(UgcType.GuildBanner, session.CharacterId);
        if (resource == null) {
            Logger.Fatal("Failed to create UGC resource for banner {BannerId}", bannerId);
            throw new InvalidOperationException($"Fatal: Creating UGC resource: {bannerId}");
        }

        session.StagedGuildPoster = new GuildPoster {
            Id = bannerId,
            OwnerId = session.CharacterId,
            OwnerName = session.PlayerName,
            ResourceId = resource.Id,
        };

        session.Send(UgcPacket.Upload(resource));
    }

    private void UploadGuildEmblem(GameSession session, IByteReader packet) {
        if (session.Guild.Guild is null) {
            Logger.Error("Cannot upload guild emblem without a guild");
            return;
        }

        if (!session.Guild.HasPermission(session.CharacterId, GuildPermission.EditEmblem)) {
            session.Send(GuildPacket.Error(GuildError.s_guild_err_no_authority));
            return;
        }

        using WebStorage.Request request = WebStorage.Context();
        UgcResource? resource = request.CreateUgc(UgcType.GuildEmblem, session.CharacterId);
        if (resource == null) {
            Logger.Fatal("Failed to create UGC resource for guild id {GuildId}", session.Guild.Id);
            throw new InvalidOperationException($"Fatal: Creating UGC resource: {session.Guild.Id}");
        }

        session.Send(UgcPacket.Upload(resource));
    }

    private void HandleConfirmation(GameSession session, IByteReader packet) {
        var info = packet.Read<UgcInfo>();
        packet.ReadInt();
        long ugcUid = packet.ReadLong();
        string ugcGuid = packet.ReadUnicodeString();
        packet.ReadShort(); // -255

        if (info.AccountId != session.AccountId || info.CharacterId != session.CharacterId || ugcUid == 0) {
            Logger.Warning("Invalid UGC confirmation {AccountId} {CharacterId} {UgcUid}", info.AccountId, info.CharacterId, ugcUid);
            return;
        }

        using WebStorage.Request webRequest = WebStorage.Context();
        UgcResource? resource = webRequest.GetUgc(ugcUid);
        if (resource == null) {
            Logger.Warning("Failed to find UGC resource {UgcUid}", ugcUid);
            return;
        }

        switch (info.Type) {
            case UgcType.Item or UgcType.Furniture or UgcType.Mount:
                ConfirmItem(session, info, ugcUid, resource);
                break;
            case UgcType.Banner:
                ConfirmBanner(session, ugcUid, resource);
                break;
            case UgcType.GuildBanner:
                ConfirmGuildBanner(session, ugcUid, resource);
                break;
            case UgcType.GuildEmblem:
                ConfirmGuildEmblem(session, ugcUid, resource);
                break;
            default:
                Logger.Warning("Unhandled Confirmation for UGC Type {UgcType}", info.Type);
                break;
        }

        session.StagedUgcItem = null;
        session.StagedGuildPoster = null;

        void ConfirmGuildBanner(GameSession session, long ugcUid, UgcResource resource) {
            Guild? guild = session.Guild.Guild;
            if (guild is null) {
                Logger.Warning("Failed to find guild for UGC {UgcUid}", ugcUid);
                return;
            }

            if (session.StagedGuildPoster is null || session.StagedGuildPoster.ResourceId != ugcUid) {
                Logger.Warning("Failed to find guild poster for UGC {UgcUid}", ugcUid);
                return;
            }

            try {
                var request = new GuildRequest {
                    RequestorId = session.CharacterId,
                    UpdatePoster = new GuildRequest.Types.UpdatePoster {
                        GuildId = session.Guild.Id,
                        Id = session.StagedGuildPoster.Id,
                        Picture = resource.Path,
                        OwnerId = session.StagedGuildPoster.OwnerId,
                        OwnerName = session.StagedGuildPoster.OwnerName,
                        ResourceId = session.StagedGuildPoster.ResourceId,
                    },
                };

                GuildResponse response = World.Guild(request);
                var error = (GuildError) response.Error;
                if (error != GuildError.none) {
                    session.Send(GuildPacket.Error(error));
                    return;
                }
            } catch (RpcException) { /* ignored */ }
        }

        void ConfirmGuildEmblem(GameSession session, long ugcUid, UgcResource resource) {
            Guild? guild = session.Guild.Guild;
            if (guild is null) {
                Logger.Warning("Failed to find guild for UGC {UgcUid}", ugcUid);
                return;
            }

            try {
                var request = new GuildRequest {
                    RequestorId = session.CharacterId,
                    UpdateEmblem = new GuildRequest.Types.UpdateEmblem {
                        GuildId = session.Guild.Id,
                        Emblem = resource.Path,
                    },
                };

                GuildResponse response = World.Guild(request);
                var error = (GuildError) response.Error;
                if (error != GuildError.none) {
                    session.Send(GuildPacket.Error(error));
                    return;
                }
            } catch (RpcException) { /* ignored */ }
        }

        void ConfirmBanner(GameSession session, long ugcUid, UgcResource resource) {
            UgcBanner? banner = session.Field.Banners.Values.FirstOrDefault(x => x.Slots.Any(slot => slot.Template?.Id == ugcUid));
            if (banner is null) {
                Logger.Warning("Failed to find banner for UGC {UgcUid}", ugcUid);
                return;
            }

            using (GameStorage.Request db = session.GameStorage.Context()) {
                foreach (BannerSlot slot in banner.Slots.Where(slot => slot.Template?.Id == ugcUid)) {
                    slot.Template!.Url = resource.Path;
                    db.UpdateBannerSlot(slot);
                }
            }

            session.Send(UgcPacket.UpdateBanner(banner));
        }

        void ConfirmItem(GameSession session, UgcInfo info, long ugcUid, UgcResource resource) {
            Item? item = session.StagedUgcItem;
            if (item?.Template == null || !TableMetadata.UgcDesignTable.Entries.TryGetValue(item.Id, out UgcDesignTable.Entry? ugcMetadata)) {
                return;
            }

            item.Template.Url = resource.Path;

            using (GameStorage.Request gameRequest = session.GameStorage.Context()) {
                item = gameRequest.CreateItem(session.CharacterId, item);
                if (item == null) {
                    Logger.Fatal("Failed to create UGC Item {ugcUid}", ugcUid);
                    throw new InvalidOperationException($"Fatal: UGC Item creation: {ugcUid}");
                }
            }

            session.Item.Inventory.Add(item, notifyNew: true);
            session.Send(UgcPacket.UpdateItem(session.Player.ObjectId, item, ugcMetadata.CreatePrice, info.Type));
            session.Send(UgcPacket.UpdatePath(resource));
        }
    }

    private static void HandleProfilePicture(GameSession session, IByteReader packet) {
        string path = packet.ReadUnicodeString();
        session.Player.Value.Character.Picture = path;

        session.ConditionUpdate(ConditionType.change_profile);
        session.Field?.Broadcast(UgcPacket.ProfilePicture(session.Player));
    }

    private static void HandleLoadBanners(GameSession session, IByteReader packet) {
        session.Send(UgcPacket.LoadBanners(session.Field.Banners.Values.Select(x => (UgcBanner) x).ToList()));
    }

    private void HandleReserveBanner(GameSession session, IByteReader packet) {
        long bannerId = packet.ReadLong();
        int hours = packet.ReadInt();

        if (hours is < 0 or > 24) {
            return;
        }

        session.Field.Banners.TryGetValue(bannerId, out FieldUgcBanner? banner);
        if (banner is null) {
            Logger.Warning("Failed to find banner {BannerId}", bannerId);
            return;
        }

        List<BannerSlot> newSlots = [];

        using GameStorage.Request db = session.GameStorage.Context();
        for (int i = 0; i < hours; i++) {
            var reservation = packet.Read<UgcBannerReservation>();

            if (banner.Slots.Any(slot => slot.Date == reservation.Date && slot.Hour == reservation.Hour)) {
                Logger.Warning("Failed to reserve banner slot {BannerId} {Date} {Hour}", bannerId, reservation.Date, reservation.Hour);
                continue;
            }

            BannerSlot slot = new(banner.Id, reservation.Date, reservation.Hour);
            slot = db.AddBannerSlot(slot);
            newSlots.Add(slot);
            banner.Slots.Add(slot);
        }

        session.Send(UgcPacket.ReserveBannerSlots(bannerId, newSlots));
    }
}

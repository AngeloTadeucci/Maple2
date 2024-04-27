using System;
using System.Threading;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class BuddyEntry {
    public required long Id { get; init; }
    public required long OwnerId { get; init; }
    public required long BuddyId { get; init; }
    public required long LastModified { get; init; }

    public string Message = string.Empty;
    public BuddyType Type;

    public static implicit operator BuddyEntry(Buddy buddy) {
        return new BuddyEntry {
            Id = buddy.Id,
            OwnerId = buddy.OwnerId,
            BuddyId = buddy.Info.CharacterId,
            LastModified = buddy.LastModified,
            Message = buddy.Message,
            Type = buddy.Type,
        };
    }
}

public class Buddy : IByteSerializable, IDisposable {
    public readonly long Id;
    public readonly long OwnerId;
    public readonly long LastModified;
    public readonly PlayerInfo Info;

    public string Message;
    public BuddyType Type { get; private set; }

    public CancellationTokenSource? TokenSource;

    public Buddy(BuddyEntry entry, PlayerInfo info) {
        Id = entry.Id;
        OwnerId = entry.OwnerId;
        LastModified = entry.LastModified;
        Message = entry.Message;
        Info = info.Clone();

        SetType(entry.Type);
    }

    public void SetType(BuddyType type) {
        Type = type;

        // Remove any sensitive information from PlayerInfo
        if (Type == BuddyType.Blocked) {
            Info.Motto = string.Empty;
            Info.Picture = string.Empty;
            Info.Gender = Gender.Male;
            Info.GearScore = 0;
            Info.CurrentHp = 0;
            Info.TotalHp = 0;
            Info.MapId = 0;
            Info.Channel = 0;
            Info.HomeName = string.Empty;
            Info.PlotMapId = 0;
            Info.PlotNumber = 0;
            Info.ApartmentNumber = 0;
            Info.PlotExpiryTime = 0;
            Info.AchievementInfo = default;
        }
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(Id);
        writer.WriteLong(Info.CharacterId);
        writer.WriteLong(Info.AccountId);
        writer.WriteUnicodeString(Info.Name);
        writer.WriteUnicodeString(Type != BuddyType.Blocked ? Message : "");
        writer.WriteShort(Info.Channel); // Channel?
        writer.WriteInt(Info.MapId);
        writer.WriteInt((int) Info.Job.Code());
        writer.Write<Job>(Info.Job);
        writer.WriteShort(Info.Level);
        writer.WriteBool(Type.HasFlag(BuddyType.InRequest));
        writer.WriteBool(Type.HasFlag(BuddyType.OutRequest));
        writer.WriteBool(Type.HasFlag(BuddyType.Blocked));
        writer.WriteBool(Info.Online);
        writer.WriteBool(false);
        writer.WriteLong(LastModified);
        writer.WriteUnicodeString(Info.Picture);
        writer.WriteUnicodeString(Info.Motto);
        writer.WriteUnicodeString(Type == BuddyType.Blocked ? Message : "");
        writer.WriteInt(Info.PlotMapId);
        writer.WriteInt(Info.PlotNumber);
        writer.WriteInt(Info.ApartmentNumber);
        writer.WriteUnicodeString(Info.HomeName);
        writer.WriteLong(Info.PlotExpiryTime); // Home expiry time?
        writer.Write<AchievementInfo>(Info.AchievementInfo);
    }

    public void Dispose() {
        TokenSource?.Cancel();
        TokenSource?.Dispose();
        TokenSource = null;
    }
}

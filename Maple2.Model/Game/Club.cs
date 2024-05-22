using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class Club {
    public DateTime LastModified { get; init; }

    public long Id { get; init; }
    public required string Name;
    public required long LeaderId;
    public ClubMember Leader;
    public long CreationTime;

    public List<ClubMember> Members;

    [SetsRequiredMembers]
    public Club(long id, string name, long leaderId) {
        Id = id;
        Name = name;
        LeaderId = leaderId;

        Members = new List<ClubMember>();
        Leader = null!;
    }

    [SetsRequiredMembers]
    public Club(long id, string name, ClubMember leader) : this(id, name, leader.Info.CharacterId) {
        Leader = leader;
    }

    public void WriteClubData(IByteWriter writer, bool isCreate) {
        writer.WriteLong(Id);
        writer.WriteUnicodeString(Name);
        writer.WriteLong(Leader.Info.AccountId);
        writer.WriteLong(Leader.Info.CharacterId);
        writer.WriteUnicodeString(Leader.Info.Name);
        writer.WriteLong(CreationTime);
        writer.WriteByte((byte) (isCreate ? 1 : 2));
        writer.WriteInt();
        writer.WriteInt();
        writer.WriteLong();
    }
}

public class ClubMember : IByteSerializable, IDisposable {
    public required PlayerInfo Info;
    public long JoinTime;
    public long LastLoginTime;

    public CancellationTokenSource? TokenSource;


    public void Dispose() {
        TokenSource?.Cancel();
        TokenSource?.Dispose();
        TokenSource = null;
    }

    public void WriteTo(IByteWriter writer) {
        WriteInfo(writer, Info);

        writer.WriteLong(JoinTime);
        writer.WriteLong(LastLoginTime);
        writer.WriteBool(!Info.Online);
    }

    public static void WriteInfo(IByteWriter writer, PlayerInfo info) {
        writer.WriteLong(info.AccountId);
        writer.WriteLong(info.CharacterId);
        writer.WriteUnicodeString(info.Name);
        writer.Write(info.Gender);
        writer.WriteInt((int) info.Job.Code());
        writer.Write(info.Job);
        writer.WriteShort(info.Level);
        writer.WriteInt(info.GearScore);
        writer.WriteInt(info.MapId);
        writer.WriteShort(info.Channel);
        writer.WriteUnicodeString(info.Picture);
        writer.WriteInt(info.PlotMapId);
        writer.WriteInt(info.PlotNumber);
        writer.WriteInt(info.ApartmentNumber);
        writer.WriteLong(info.PlotExpiryTime);
        writer.Write(info.AchievementInfo);
    }
}


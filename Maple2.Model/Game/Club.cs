using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class Club : IByteSerializable {

    public long Id { get; init; }
    public required string Name;
    public required long LeaderId;
    public ClubMember Leader;
    public long CreationTime;
    public ClubState State = ClubState.Staged;
    public int BuffId;
    public long NameChangedTime;
    public long LastModified { get; init; }

    public ConcurrentDictionary<long, ClubMember> Members;

    [SetsRequiredMembers]
    public Club(long id, string name, long leaderId) {
        Id = id;
        Name = name;
        LeaderId = leaderId;

        Members = new ConcurrentDictionary<long, ClubMember>();
        Leader = null!;
    }

    [SetsRequiredMembers]
    public Club(long id, string name, ClubMember leader) : this(id, name, leader.Info.CharacterId) {
        Leader = leader;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(Id);
        writer.WriteUnicodeString(Name);
        writer.WriteLong(Leader.Info.AccountId);
        writer.WriteLong(Leader.Info.CharacterId);
        writer.WriteUnicodeString(Leader.Info.Name);
        writer.WriteLong(CreationTime);
        writer.Write<ClubState>(State);
        writer.WriteInt();
        writer.WriteInt();
        writer.WriteLong(NameChangedTime);
    }
}

public class ClubMember : IByteSerializable, IDisposable {
    public const byte TYPE = 2;

    public long ClubId { get; init; }
    public required PlayerInfo Info;
    public long AccountId => Info.AccountId;
    public long CharacterId => Info.CharacterId;
    public string Name => Info.Name;
    public long JoinTime;
    public long LoginTime;

    public CancellationTokenSource? TokenSource;


    public void Dispose() {
        TokenSource?.Cancel();
        TokenSource?.Dispose();
        TokenSource = null;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteByte(TYPE);
        writer.WriteLong(ClubId);

        WriteInfo(writer, this);
    }

    public static void WriteInfo(IByteWriter writer, ClubMember member) {
        PlayerInfo info = member.Info;
        writer.WriteLong(info.AccountId);
        writer.WriteLong(info.CharacterId);
        writer.WriteUnicodeString(info.Name);
        writer.Write<Gender>(info.Gender);
        writer.WriteInt((int) info.Job.Code());
        writer.Write<Job>(info.Job);
        writer.WriteShort(info.Level);
        writer.WriteInt(info.MapId);
        writer.WriteShort(info.Channel);
        writer.WriteUnicodeString(info.Picture);
        writer.WriteInt(info.PlotMapId);
        writer.WriteInt(info.PlotNumber);
        writer.WriteInt(info.ApartmentNumber);
        writer.WriteLong(info.PlotExpiryTime);
        writer.Write(info.AchievementInfo);
        writer.WriteLong(member.JoinTime);
        writer.WriteLong(member.LoginTime);
        writer.WriteBool(!info.Online);
    }
}


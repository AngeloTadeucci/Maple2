using System;
using System.Threading;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game.Party;

public class PartyMember : IByteSerializable, IDisposable {
    public long PartyId { get; init; }
    public required PlayerInfo Info;
    public long JoinTime;
    public long LoginTime;
    public long AccountId => Info.AccountId;
    public long CharacterId => Info.CharacterId;
    public string Name => Info.Name;
    public byte ReadyState = 0;

    public CancellationTokenSource? TokenSource;

    public void WriteTo(IByteWriter writer) {
        writer.WriteClass<PlayerInfo>(Info);
    }

    public void WriteDungeonEligibility(IByteWriter writer) {
        var Count = 0;
        writer.WriteInt(Count);
        for (var i = 0; i < Count; i++) {
            writer.WriteInt();
            writer.WriteByte();
        }
    }

    public void Dispose() {
        TokenSource?.Cancel();
        TokenSource?.Dispose();
        TokenSource = null;
    }
}

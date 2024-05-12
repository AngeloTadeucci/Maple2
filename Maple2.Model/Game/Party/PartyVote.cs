using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game.Party;

public class PartyVote : IByteSerializable {
    public readonly PartyVoteType Type;
    public readonly ICollection<long> PartyCharacterIds;
    public readonly ICollection<long> Approvals;
    public readonly ICollection<long> Disapprovals;

    public PartyVote(PartyVoteType type, ICollection<long> partyCharacterIds, long requestorId) {
        Type = type;
        PartyCharacterIds = partyCharacterIds;
        Approvals = new List<long>() { requestorId };
        Disapprovals = new List<long>();
    }

    public void WriteTo(IByteWriter writer) {
        writer.Write<PartyVoteType>(Type);
        writer.WriteInt(); // Counter
        writer.WriteLong(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        writer.WriteInt(PartyCharacterIds.Count);
        foreach (long characterId in PartyCharacterIds) {
            writer.WriteLong(characterId);
        }

        writer.WriteInt(Approvals.Count);
        foreach (long characterId in Approvals) {
            writer.WriteLong(characterId);
        }

        writer.WriteInt(Disapprovals.Count);
        foreach (long characterId in Disapprovals) {
            writer.WriteLong(characterId);
        }
    }
}

using System.Runtime.InteropServices;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game;

public class Marriage : IByteSerializable {
    public static Marriage Default => new() {
        Status = MaritalStatus.Single,
        Partner1 = new MarriagePartner(),
        Partner2 = new MarriagePartner(),
    };
    public long Id { get; init; }
    public MaritalStatus Status { get; set; }

    public required MarriagePartner Partner1 { get; set; }
    public required MarriagePartner Partner2 { get; set; }
    public long Exp { get; set; }
    public string Profile { get; set; } = string.Empty;
    public long CreationTime { get; set; }

    public IList<MarriageExp> ExpHistory { get; set; } = [];


    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(Id);
        writer.WriteByte((byte) Status);
        writer.WriteLong(CreationTime);
        writer.WriteLong(); // wedding time ?
        writer.WriteClass<MarriagePartner>(Partner1);
        writer.WriteClass<MarriagePartner>(Partner2);
        writer.WriteLong(Exp);
        writer.WriteUnicodeString(Partner1.Message);
        writer.WriteUnicodeString(Partner2.Message);
        writer.WriteUnicodeString();
        writer.WriteUnicodeString(Profile);
        writer.WriteInt(ExpHistory.Count);
        foreach (MarriageExp exp in ExpHistory) {
            writer.Write<MarriageExpType>(exp.Type);
            writer.WriteLong(exp.Amount);
            writer.WriteLong(exp.Time);
        }
    }
}

public class MarriageExp : IByteSerializable {
    public MarriageExpType Type { get; set; }
    public long Amount { get; set; }
    public long Time { get; set; }
    public void WriteTo(IByteWriter writer) {
        writer.Write<MarriageExpType>(Type);
        writer.WriteLong(Amount);
        writer.WriteLong(Time);
    }
}

public class MarriagePartner : IByteSerializable, IDisposable {
    public PlayerInfo? Info { get; set; }
    public long AccountId => Info?.AccountId ?? 0;
    public long CharacterId => Info?.CharacterId ?? 0;
    public string Name => Info?.Name ?? string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsOnline => Info?.Online ?? false;
    public CancellationTokenSource? TokenSource;
    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(CharacterId);
        writer.WriteLong(AccountId);
        writer.WriteUnicodeString(Name);
        writer.WriteBool(!IsOnline);
    }

    public void Dispose() {
        TokenSource?.Cancel();
        TokenSource?.Dispose();
        TokenSource = null;
    }
}

public class MarriageInfo : IByteSerializable {
    public MaritalStatus Status { get; set; }
    public long CreationTime { get; set; }
    public string Partner1Name { get; set; } = string.Empty;
    public string Partner2Name { get; set; } = string.Empty;
    public void WriteTo(IByteWriter writer) {
        writer.Write<MaritalStatus>(Status);
        writer.WriteLong(CreationTime);
        writer.WriteUnicodeString(Partner1Name);
        writer.WriteUnicodeString(Partner2Name);
    }
}

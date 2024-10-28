using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game;

public class PlotCube : HeldCube {
    public enum CubeType { Default, Construction, Liftable };

    public Vector3B Position { get; set; }
    public float Rotation { get; set; }
    public CubeType Type { get; set; }

    public int PlotId { get; set; }

    public HousingCategory HousingCategory { get; set; }

    public CubePortalSettings? CubePortalSettings { get; set; }
    public InteractCube? Interact { get; set; }

    public PlotCube(int itemId, long id = 0, UgcItemLook? template = null) {
        ItemId = itemId;
        ItemType = new ItemType(itemId);
        Id = id;
        Template = template;

        if (itemId is Constant.InteriorPortalCubeId) {
            CubePortalSettings = new CubePortalSettings();
        }
    }
}

public class InteractCube : IByteSerializable {
    public string Id { get; set; }
    public InteractCubeState State { get; set; }

    public readonly InteractCubeState DefaultState;

    public Nurturing? Nurturing { get; set; }

    public long InteractingCharacterId { get; set; }

    public InteractCube(Vector3B position, FunctionCubeMetadata metadata) {
        Id = $"4_{position.ConvertToInt()}";
        DefaultState = metadata.DefaultState;
        State = metadata.DefaultState;

        if (metadata.Nurturing is not null) {
            Nurturing = new Nurturing(metadata.Nurturing);
        }
    }

    public InteractCube(string id, InteractCubeState defaultState) {
        Id = id;
        DefaultState = defaultState;
        State = defaultState;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteUnicodeString(Id);
        writer.Write(State);
        if (Nurturing is not null) {
            writer.WriteClass<Nurturing>(Nurturing);
        }
        writer.WriteByte();
    }
}

public class Nurturing : IByteSerializable {
    public long Exp { get; set; }
    public DateTimeOffset LastFeedTime { get; set; }

    public short Stage { get; private set; }
    public short ClaimedGiftForStage { get; set; }
    public List<long> PlayedBy { get; set; }
    private DateTimeOffset CreationTime { get; set; }

    public readonly FunctionCubeMetadata.NurturingData NurturingMetadata;

    public Nurturing(FunctionCubeMetadata.NurturingData metadata) {
        CreationTime = DateTimeOffset.Now;
        NurturingMetadata = metadata;
        Stage = 1;
        ClaimedGiftForStage = 1;
        PlayedBy = [];
    }

    public Nurturing(long exp, short claimedGiftForStage, long[] playedBy, DateTimeOffset creationTime, DateTimeOffset lastFeedTime, FunctionCubeMetadata.NurturingData? metadata) {
        NurturingMetadata = metadata ?? throw new ArgumentException("FunctionCubeMetadata does not have a Nurturing metadata.");

        Exp = exp;
        Stage = 1;
        ClaimedGiftForStage = claimedGiftForStage;
        PlayedBy = playedBy.ToList();
        CreationTime = creationTime;
        LastFeedTime = lastFeedTime;

        FunctionCubeMetadata.NurturingData.Growth[] requiredGrowth = metadata.RequiredGrowth;
        if (exp >= requiredGrowth.Last().Exp) {
            Stage = requiredGrowth.Last().Stage;
            return;
        }

        for (short i = 0; i < requiredGrowth.Length; i++) {
            FunctionCubeMetadata.NurturingData.Growth growth = requiredGrowth[i];
            if (exp < growth.Exp) {
                Stage = (short) (i + 1);
                break;
            }
        }
    }

    public void Feed() {
        if (Exp >= NurturingMetadata.RequiredGrowth.Last().Exp) {
            return;
        }

        Exp += Constant.NurturingEatGrowth;
        if (Exp >= NurturingMetadata.RequiredGrowth.First(x => x.Stage == Stage).Exp) {
            Stage++;
        }
        LastFeedTime = DateTimeOffset.Now;
    }

    public bool Play(long accountId) {
        if (PlayedBy.Count >= Constant.NurturingPlayMaxCount) {
            return false;
        }

        if (PlayedBy.Contains(accountId)) {
            return false;
        }

        PlayedBy.Add(accountId);
        Feed();
        return true;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(CreationTime.ToUnixTimeSeconds());
        writer.WriteLong(Exp);
        writer.WriteShort(Stage);
        writer.WriteShort(ClaimedGiftForStage);
        writer.WriteShort(); // Unknown
        writer.WriteLong(LastFeedTime.ToUnixTimeSeconds());
    }
}

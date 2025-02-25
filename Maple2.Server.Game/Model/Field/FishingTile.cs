using System.Numerics;
using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Metadata.FieldEntity;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Server.Game.Model;

public class FishingTile : IByteSerializable {
    private readonly FieldFluidEntity metadata;
    private Vector3 Position => metadata.Position;
    public LiquidType LiquidType => metadata.LiquidType;
    private int FishId { get; set; }
    private int BoreTime { get; set; } // fishing time minus any rod or buff time reduction
    private int Unknown1 { get; set; }
    private short Unknown2 { get; set; }

    public FishingTile(FieldFluidEntity entity, int rodReduceTick) {
        metadata = entity;
        FishId = 10000001; // HAS to be this ID otherwise the fight fishing game will not work
        BoreTime = 15000 - rodReduceTick;
        Unknown1 = 25;
        Unknown2 = 1;
    }
    public void WriteTo(IByteWriter writer) {
        writer.Write<Vector3B>(Position);
        writer.WriteInt(FishId);
        writer.WriteInt(Unknown1);
        writer.WriteInt(BoreTime);
        writer.WriteShort(Unknown2);
    }
}

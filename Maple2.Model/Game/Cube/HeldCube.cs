using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game;

public class HeldCube : IByteSerializable, IByteDeserializable {
    public static readonly HeldCube Default = new();

    public long Id { get; set; }
    public int ItemId { get; protected set; }
    public UgcItemLook? Template { get; protected set; }

    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(ItemId);
        writer.WriteLong(Id);
        writer.WriteLong(); // expire timestamp for ugc item

        writer.WriteBool(Template != null);
        if (Template != null) {
            writer.WriteClass<UgcItemLook>(Template);
        }
    }

    public void ReadFrom(IByteReader reader) {
        ItemId = reader.ReadInt();
        Id = reader.ReadLong();
        reader.ReadLong();

        bool hasTemplate = reader.ReadBool();
        if (hasTemplate) {
            Template = reader.ReadClass<UgcItemLook>();
        }
    }
}

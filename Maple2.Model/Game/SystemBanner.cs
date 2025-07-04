using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class SystemBanner : IByteSerializable {
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty; // name must start with "homeproduct_" for Meret Market banners
    public SystemBannerType Type { get; init; }
    public SystemBannerFunction Function { get; init; }
    public string FunctionParameter { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty; // Meret Market banner resolution: 538x301
    public SystemBannerLanguage Language { get; init; }
    public long BeginTime { get; init; }
    public long EndTime { get; init; }

    public SystemBanner(int id) {
        Id = id;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(Id);
        writer.WriteUnicodeString(Name);
        writer.WriteUnicodeString(Type.ToString());
        writer.WriteUnicodeString(Function.ToString());
        writer.WriteUnicodeString(FunctionParameter);
        writer.WriteUnicodeString(Url);
        writer.Write<SystemBannerLanguage>(Language);
        writer.WriteLong(BeginTime);
        writer.WriteLong(EndTime);
    }
}

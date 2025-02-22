using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game;

public class InteractCube : IByteSerializable {
    public string Id { get; set; }
    public InteractCubeState State { get; set; }

    public readonly InteractCubeState DefaultState;

    public Nurturing? Nurturing { get; set; }
    public CubePortalSettings? PortalSettings { get; set; }
    public CubeNoticeSettings? NoticeSettings { get; set; }

    public long InteractingCharacterId { get; set; }

    public InteractCube(Vector3B position, FunctionCubeMetadata metadata) {
        Id = $"4_{position.ConvertToInt()}";
        DefaultState = metadata.DefaultState;
        State = metadata.DefaultState;

        if (metadata.Nurturing is not null) {
            Nurturing = new Nurturing(metadata.Nurturing);
        }

        if (metadata.ConfigurableCubeType is ConfigurableCubeType.UGCPortal) {
            PortalSettings = new CubePortalSettings(position);
        } else if (metadata.ConfigurableCubeType is ConfigurableCubeType.UGCNotice) {
            NoticeSettings = new CubeNoticeSettings(position);
        }
    }

    public InteractCube(string id, InteractCubeState defaultState, CubePortalSettings? portalSettings, CubeNoticeSettings? noticeSettings) {
        Id = id;
        DefaultState = defaultState;
        State = defaultState;
        PortalSettings = portalSettings;
        NoticeSettings = noticeSettings;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteUnicodeString(Id);
        writer.Write(State);
        if (Nurturing is not null) {
            writer.WriteClass<Nurturing>(Nurturing);
        }
    }
}

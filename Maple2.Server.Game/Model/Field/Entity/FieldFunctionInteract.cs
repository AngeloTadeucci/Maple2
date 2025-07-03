using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Packets;

namespace Maple2.Server.Game.Model;

public class FieldFunctionInteract : FieldEntity<FunctionCubeMetadata> {
    public readonly InteractCube InteractCube;
    public readonly long CubeId;

    private long NextUpdateTick { get; set; }

    public FieldFunctionInteract(FieldManager field, int objectId, InteractCube interactCube, long cubeId) : base(field, objectId, interactCube.Metadata) {
        InteractCube = interactCube;
        NextUpdateTick = Field.FieldTick + Value.AutoStateChangeTime;
        CubeId = cubeId;
    }

    public override void Update(long tickCount) {
        lock (InteractCube) {
            if (InteractCube.Metadata.ControlType is InteractCubeControlType.Notice) {
                return;
            }
            if (InteractCube.State is InteractCubeState.Available or InteractCubeState.None) {
                return;
            }

            if (tickCount < NextUpdateTick) {
                return;
            }

            InteractCube.State = InteractCubeState.Available;
            Field.Broadcast(FunctionCubePacket.UpdateFunctionCube(InteractCube));
        }
    }

    public bool Use() {
        lock (InteractCube) {
            if (InteractCube.State is not InteractCubeState.Available) {
                return false;
            }

            InteractCube.State = InteractCubeState.InUse;
            NextUpdateTick = Field.FieldTick + Value.AutoStateChangeTime;
            return true;
        }
    }
}

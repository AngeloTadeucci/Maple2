using System.Threading.Tasks;
using Grpc.Core;
using Maple2.Server.Core.Packets;
using Maple2.Server.Login.Session;

namespace Maple2.Server.Login.Service;

public partial class LoginService {
    public override Task<HeartbeatResponse> Heartbeat(HeartbeatRequest request, ServerCallContext context) {
        foreach (LoginSession loginSession in loginServer.GetSessions()) {
            loginSession.Send(RequestPacket.Heartbeat());
        }
        return Task.FromResult(new HeartbeatResponse());
    }
}

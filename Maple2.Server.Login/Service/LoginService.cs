using Grpc.Core;
using Serilog;

namespace Maple2.Server.Login.Service;

public partial class LoginService : Login.LoginBase {
    private readonly LoginServer loginServer;
    private readonly ILogger logger;

    public LoginService(LoginServer loginServer) {
        this.loginServer = loginServer;
        logger = Log.Logger.ForContext<LoginService>();
    }
}

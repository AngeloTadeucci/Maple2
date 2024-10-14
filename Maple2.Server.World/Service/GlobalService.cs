using Grpc.Core;
using Maple2.Database.Storage;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Serilog;
using ILogger = Serilog.ILogger;

// TODO: Move this to a Global server
// ReSharper disable once CheckNamespace
namespace Maple2.Server.Global.Service;

public partial class GlobalService : Global.GlobalBase {
    private readonly GameStorage gameStorage;

    private readonly ILogger logger = Log.Logger.ForContext<GlobalService>();

    public GlobalService(GameStorage gameStorage) {
        this.gameStorage = gameStorage;
    }

    public override Task<LoginResponse> Login(LoginRequest request, ServerCallContext context) {
#if !DEBUG // Allow empty username for testing
        if (string.IsNullOrWhiteSpace(request.Username)) {
            return Task.FromResult(new LoginResponse {
                Code = LoginResponse.Types.Code.ErrorId,
                Message = "Invalid Id",
            });
        }

        if (string.IsNullOrWhiteSpace(request.Password)) {
            return Task.FromResult(new LoginResponse {
                Code = LoginResponse.Types.Code.ErrorPassword,
                Message = "Invalid Password.",
            });
        }
#endif

        // Normalize username
        string username = request.Username.Trim().ToLower();
        string password = request.Password.Trim();
        var machineId = new Guid(request.MachineId);

        using GameStorage.Request db = gameStorage.Context();
        Account? account = db.GetAccount(username);
        if (account is null) {
            if (Constant.AutoRegister) {
                account = new Account {
                    Username = username,
                    MachineId = machineId,
                };

                db.BeginTransaction();
                account = db.CreateAccount(account, password);
                if (account == null) {
                    throw new RpcException(new Status(StatusCode.Internal, "Failed to create account."));
                }

                db.Commit();
                return Task.FromResult(new LoginResponse { AccountId = account.Id });
            }

            return Task.FromResult(new LoginResponse {
                Code = LoginResponse.Types.Code.ErrorId,
                Message = "Account not found",
            });
        }

        bool isPasswordValid = db.VerifyPassword(account.Id, password);
        if (!isPasswordValid) {
            return Task.FromResult(new LoginResponse {
                Code = LoginResponse.Types.Code.ErrorPassword,
                Message = "Incorrect Password.",
            });
        }

        if (account.MachineId != machineId) {
            logger.Warning("MachineId mismatch for account {AccountId}", account.Id);
            if (Constant.BlockLoginWithMismatchedMachineId) {
                return Task.FromResult(new LoginResponse {
                    Code = LoginResponse.Types.Code.BlockNexonSn,
                    Message = "MachineId mismatch",
                });
            }
        }

        if (account.MachineId == default) {
            db.UpdateMachineId(account.Id, machineId);
        }

        return Task.FromResult(new LoginResponse { AccountId = account.Id });
    }
}

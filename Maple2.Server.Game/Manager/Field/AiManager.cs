using Serilog;

namespace Maple2.Server.Game.Manager.Field;

public sealed class AiManager {
    private readonly ILogger logger = Log.Logger.ForContext<AiManager>();
    public FieldManager Field { get; init; }

    public AiManager(FieldManager field) {
        Field = field;
    }
}

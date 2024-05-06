using Maple2.PathEngine.Exception;
using Maple2.PathEngine.Interface;
using Maple2.PathEngine.Types;
using Serilog;

namespace Maple2.Server.Game.Util;

public class LogErrorHandler(ILogger logger) : IErrorHandler {

    public override ErrorResult handle(ErrorType type, string description, IDictionary<string, string> attributes) {
        if (type is ErrorType.Fatal or ErrorType.Assertion) {
            throw new PathEngineException(type, description, attributes);
        }

        Action<string> logAction = type switch {
            ErrorType.Warning => logger.Warning,
            ErrorType.NonFatal => logger.Information,
            _ => logger.Error,
        };
        logAction($"[{type}] {description}");
        foreach ((string key, string value) in attributes) {
            logAction($"- {key}={value}");
        }

        return ErrorResult.Continue;
    }
}

using Maple2.Database.Storage;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Session;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple2.Server.Game.Manager.Field;

public sealed class AiManager(FieldManager field) {
    private readonly ILogger logger = Log.Logger.ForContext<AiManager>();
    public FieldManager Field { get; init; } = field;

}

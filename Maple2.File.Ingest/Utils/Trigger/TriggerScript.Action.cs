using System.CodeDom.Compiler;
using System.Diagnostics;
using Maple2.Tools.Extensions;

namespace Maple2.File.Ingest.Utils.Trigger;

internal partial class TriggerScript {
    public class Action : IScriptBlock {
        public bool SingleLine => true;

        private readonly string? splitName;
        public IList<PyParameter> Args = [];
        public string? LineComment;

        private readonly TriggerDefinitionOverride? overrides;

        public Action(string? name, string? splitArgValue) {
            if (name != null) {
                overrides = TriggerDefinitionOverride.ActionOverride.GetValueOrDefault(name);
                if (splitArgValue != null) {
                    overrides = overrides?.FunctionLookup[splitArgValue];
                }
            }
            if (splitArgValue != null) {
                splitName = overrides?.Name;
            }
        }

        public virtual void WriteTo(IndentedTextWriter writer, bool isCommented) {
            if (overrides != null) {
                if (LineComment != null && (LineComment.Contains('\n') || LineComment.Length > LineCommentMax)) {
                    writer.WriteLineCommentString(LineComment, sameLine: false);

                    // Set null to avoid writing twice.
                    LineComment = null;
                }

                foreach ((string? name, _) in overrides.Types.Where(o => o.Value.Default == "<required>")) {
                    Debug.Assert(Args.Any(arg => arg.Name == name), $"Action {overrides.Name} is missing required arg: {name}");
                }

                IEnumerable<string> args = Args
                    .Where(arg => arg.Name != overrides.Compare.Field && arg.Name != overrides.FunctionSplitter)
                    .Where(arg => !arg.IsDefault(overrides.Types.GetValueOrDefault(arg.Name).Default))
                    .Select(arg => $"{arg.Name}={arg.FormatValue()}");
                writer.Write($"self.{overrides.Name}({string.Join(", ", args)})");
                if (LineComment != null) {
                    writer.WriteLineCommentString(LineComment);
                } else {
                    writer.WriteBlankLine();
                }
            } else if (LineComment != null) {
                writer.WriteLineCommentString(LineComment, false);
            }
        }

        public ISet<string> Imports() {
            return Args.Select(a => a.Import())
                .WhereNotNull()
                .ToHashSet();
        }
    }
}

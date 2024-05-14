using System.CodeDom.Compiler;

namespace Maple2.File.Ingest.Utils.Trigger;

internal partial class TriggerScript {
    public class Action : IScriptBlock {
        public bool SingleLine => true;

        private readonly string? splitName;
        public IList<PyParameter> Args = new List<PyParameter>();
        public string? LineComment;

        private readonly TriggerDefinitionOverride? overrides;

        public Action(string? name, string? splitName) {
            this.splitName = splitName;
            if (name != null) {
                overrides = TriggerDefinitionOverride.ActionOverride.GetValueOrDefault(name);
            }
        }

        public virtual void WriteTo(IndentedTextWriter writer, bool isCommented) {
            if (overrides != null) {
                string fullName = overrides.Name;
                if (splitName != null) {
                    fullName += $"_{splitName}";
                }

                if (LineComment != null && (LineComment.Contains('\n') || LineComment.Length > LineCommentMax)) {
                    writer.WriteLineCommentString(LineComment, sameLine: false);

                    // Set null to avoid writing twice.
                    LineComment = null;
                }

                IEnumerable<string> args = Args
                    .Where(arg => !string.IsNullOrWhiteSpace(arg.Value))
                    .Where(arg => arg.Name != overrides.Compare.Field && arg.Name != overrides.FunctionSplitter)
                    .Select(arg => $"{arg.Name}={arg.FormatValue()}");
                writer.Write($"self.{fullName}({string.Join(", ", args)})");
                if (LineComment != null) {
                    writer.WriteLineCommentString(LineComment);
                } else {
                    writer.WriteBlankLine();
                }
            } else if (LineComment != null) {
                writer.WriteLineCommentString(LineComment, false);
            }
        }
    }
}

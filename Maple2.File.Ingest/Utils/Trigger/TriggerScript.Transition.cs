using System.CodeDom.Compiler;
using System.Diagnostics;

namespace Maple2.File.Ingest.Utils.Trigger;

internal partial class TriggerScript {
    public class Transition(string? state, bool isValid, string? lineComment) {
        public void WriteTo(IndentedTextWriter writer, bool isCommented = false) {
            if (state == null) {
                writer.WriteLine("return None");
                return;
            }

            if (lineComment != null && (lineComment.Contains('\n') || lineComment.Length > LineCommentMax || !isValid)) {
                Debug.Assert(!lineComment.StartsWith('<'), lineComment);
                writer.WriteLineCommentString(lineComment, sameLine: false);

                // Set null to avoid writing twice.
                lineComment = null;
            }

            if (isValid || isCommented) {
                writer.Write($"return {state}(self.ctx)");
                if (lineComment != null) {
                    Debug.Assert(!lineComment.StartsWith('<'), lineComment);
                    writer.Write($" # {lineComment.Trim()}");
                }
                writer.WriteBlankLine();
            } else {
                writer.WriteLine($"return None # Missing State: {state}");
            }
        }
    }
}

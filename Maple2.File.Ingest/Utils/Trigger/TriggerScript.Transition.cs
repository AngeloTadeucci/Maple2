using System.CodeDom.Compiler;
using System.Diagnostics;

namespace Maple2.File.Ingest.Utils.Trigger;

internal partial class TriggerScript {
    public class Transition {
        public readonly string? State;
        public readonly bool IsValid;
        public string? LineComment;

        public Transition(string? state, bool isValid, string? lineComment) {
            State = state;
            IsValid = isValid;
            LineComment = lineComment;
        }

        public void WriteTo(IndentedTextWriter writer, bool isCommented = false) {
            if (State == null) {
                writer.WriteLine("return None");
                return;
            }

            if (LineComment != null && (LineComment.Contains('\n') || LineComment.Length > LineCommentMax || !IsValid)) {
                Debug.Assert(!LineComment.StartsWith('<'), LineComment);
                writer.WriteLineCommentString(LineComment, sameLine: false);

                // Set null to avoid writing twice.
                LineComment = null;
            }

            if (IsValid || isCommented) {
                writer.Write($"return {State}(self.ctx)");
                if (LineComment != null) {
                    Debug.Assert(!LineComment.StartsWith('<'), LineComment);
                    writer.Write($" # {LineComment.Trim()}");
                }
                writer.WriteBlankLine();
            } else {
                writer.WriteLine($"return None # Missing State: {State}");
            }
        }
    }
}

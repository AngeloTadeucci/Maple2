using System.CodeDom.Compiler;

namespace Maple2.File.Ingest.Utils;

public static class TextWriterExtensions {
    public static void WriteBlankLine(this IndentedTextWriter writer) {
        int indent = writer.Indent;
        writer.Indent = 0;
        writer.WriteLine();
        writer.Indent = indent;
    }

    public static void WriteLineCommentString(this IndentedTextWriter writer, string str, bool sameLine = true) {
        if (str.Contains('\n')) {
            writer.WriteComments(new[] { str.Trim() });
        } else {
            writer.WriteLine(sameLine ? $" # {str.Trim()}" : $"# {str.Trim()}");
        }
    }

    public static void WriteComments(this IndentedTextWriter writer, ICollection<string> comments) {
        if (comments.Count > 0) {
            if (comments.SelectMany(c => c.Split("\n")).Count() > 1) {
                writer.WriteLine("\"\"\"");
                foreach (string comment in comments) {
                    writer.WriteLine(comment.Replace("\t", "    ").Trim());
                }
                writer.WriteLine("\"\"\"");
            } else {
                writer.WriteLine($"# {comments.First().Trim()}");
            }
        }
    }
}

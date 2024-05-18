using System.CodeDom.Compiler;

namespace Maple2.File.Ingest.Utils.Trigger;

internal interface IScriptBlock {
    public bool SingleLine { get; }
    public bool IsCode => true;

    public void WriteTo(IndentedTextWriter writer, bool isCommented = false);

    public ISet<string> Imports();
}

internal class Comment : IScriptBlock {
    public bool SingleLine => !Value.Contains('\n');
    public bool IsCode => false;

    public readonly string Value;

    public Comment(string value) {
        Value = value.Trim();
    }

    public void WriteTo(IndentedTextWriter writer, bool isCommented = true) {
        if (Value.Contains('\n')) {
            writer.WriteLine("\"\"\"");
            // TODO: preserve tabbing instead of trimming all
            // str = str.Replace("\t", "    ");
            foreach (string line in Value.Split(["\n", "\r\n"], StringSplitOptions.TrimEntries)) {
                writer.WriteLine(line);
            }
            writer.WriteLine("\"\"\"");
        } else {
            writer.WriteLine($"# {Value}");
        }
    }

    public ISet<string> Imports() => new HashSet<string>();
}

internal class CommentWrapper : IScriptBlock {
    public bool SingleLine => Child.SingleLine;
    public bool IsCode => false;

    public readonly IScriptBlock Child;

    public CommentWrapper(IScriptBlock child) {
        Child = child;
    }

    public void WriteTo(IndentedTextWriter writer, bool isCommented = true) {
        if (Child.SingleLine) {
            writer.Write("# ");
            Child.WriteTo(writer, true);
        } else {
            writer.WriteLine("\"\"\""); // Start block comment
            Child.WriteTo(writer, true);
            writer.WriteLine("\"\"\""); // End block comment
        }
    }

    public ISet<string> Imports() => new HashSet<string>();
}

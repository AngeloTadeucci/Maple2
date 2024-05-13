using System.CodeDom.Compiler;

namespace Maple2.File.Ingest.Utils.Trigger;

internal partial class TriggerScript {
    private const int LineCommentMax = 25;

    public readonly IList<string> Imports;
    public readonly IList<IScriptBlock> States;
    public bool Shared { get; init; }

    public TriggerScript() {
        States = new List<IScriptBlock>();
        Imports = new List<string>();
    }

    public void WriteTo(IndentedTextWriter writer) {
        writer.WriteLine("import trigger_api");
        if (Imports.Count > 0) {
            writer.WriteBlankLine();
            foreach (string import in Imports) {
                writer.WriteLine($"#include {import.Replace('.', '/')}.py");
                writer.WriteLine($"from {import} import *");
            }
        }
        writer.WriteBlankLine();
        writer.WriteBlankLine();

        foreach (IScriptBlock state in States) {
            state.WriteTo(writer);
            // Extra line only if this is actual code
            if (state.IsCode) {
                writer.WriteBlankLine();
            }
            writer.WriteBlankLine();
        }

        // No initialization for dungeon_common
        if (!Shared) {
            if (States.First() is not State state) {
                if (States.First() is CommentWrapper wrapper) {
                    state = (State) wrapper.Child;
                } else {
                    throw new InvalidOperationException("No initial_state found");
                }
            }

            writer.WriteLine($"initial_state = {state.Name}");
        }
    }
}

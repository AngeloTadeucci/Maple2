using System.CodeDom.Compiler;

namespace Maple2.File.Ingest.Utils.Trigger;

internal partial class TriggerScript {
    private const int LineCommentMax = 25;

    public readonly IList<string> Imports = [];
    public readonly IList<IScriptBlock> States = [];
    public bool Shared { get; init; }

    public void WriteTo(IndentedTextWriter writer) {
        writer.WriteLine("import trigger_api");

        List<string> enumImports = [];
        foreach (string import in States.SelectMany(s => s.Imports()).Distinct()) {
            switch (import) {
                case "Vector3":
                    writer.WriteLine("from System.Numerics import Vector3");
                    break;
                case "Align" or "FieldGame" or "Weather" or "Locale":
                    enumImports.Add(import);
                    break;
                default:
                    throw new ArgumentException($"Unexpected import: {import}");
            }
        }
        if (enumImports.Count > 0) {
            writer.WriteLine($"from Maple2.Server.Game.Scripting.Trigger import {string.Join(", ", enumImports)}");
        }

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

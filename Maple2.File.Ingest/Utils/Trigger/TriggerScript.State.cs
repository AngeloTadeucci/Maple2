using System.CodeDom.Compiler;

namespace Maple2.File.Ingest.Utils.Trigger;

internal partial class TriggerScript {
    public class State : IScriptBlock {
        public bool SingleLine => false;

        public readonly string Name;
        public readonly List<string> Comments = new();
        public IList<IScriptBlock> OnEnter = new List<IScriptBlock>();
        public Transition? OnEnterTransition;
        public IList<IScriptBlock> Conditions = new List<IScriptBlock>();
        public IList<IScriptBlock> OnExit = new List<IScriptBlock>();

        public State(string name) {
            Name = name;
        }

        public void WriteTo(IndentedTextWriter writer, bool isCommented) {
            writer.WriteComments(Comments);
            writer.WriteLine($"class {Name}(trigger_api.Trigger):");
            bool hasBody = false;
            writer.Indent++;
            if (OnEnter.Count > 0 || OnEnterTransition != null) {
                bool hasOnEnterBody = false;
                writer.WriteLine("def on_enter(self) -> 'trigger_api.Trigger':");
                writer.Indent++;
                foreach (IScriptBlock action in OnEnter) {
                    action.WriteTo(writer, isCommented);
                    hasOnEnterBody |= action.IsCode;
                }
                OnEnterTransition?.WriteTo(writer, isCommented);
                hasOnEnterBody |= OnEnterTransition != null;
                if (!hasOnEnterBody) {
                    writer.WriteLine("pass");
                }
                writer.Indent--;
                if (Conditions.Count > 0 || OnExit.Count > 0) {
                    writer.WriteBlankLine();
                }
                hasBody = true;
            }
            if (Conditions.Count > 0) {
                writer.WriteLine("def on_tick(self) -> trigger_api.Trigger:");
                writer.Indent++;
                foreach (IScriptBlock condition in Conditions) {
                    condition.WriteTo(writer, isCommented);
                }
                writer.Indent--;
                if (OnExit.Count > 0) {
                    writer.WriteBlankLine();
                }
                hasBody = true;
            }
            if (OnExit.Count > 0) {
                bool hasOnExitBody = false;
                writer.WriteLine("def on_exit(self) -> None:");
                writer.Indent++;
                foreach (IScriptBlock action in OnExit) {
                    action.WriteTo(writer, isCommented);
                    hasOnExitBody |= action.IsCode;
                }
                if (!hasOnExitBody) {
                    writer.WriteLine("pass");
                }
                writer.Indent--;
                hasBody = true;
            }

            if (!hasBody) {
                writer.WriteLine("pass");
            }
            writer.Indent--;
        }
    }
}

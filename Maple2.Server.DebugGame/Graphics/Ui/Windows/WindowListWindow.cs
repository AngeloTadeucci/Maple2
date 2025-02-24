using ImGuiNET;
using System.Numerics;

namespace Maple2.Server.DebugGame.Graphics.Ui.Windows;

public class WindowListWindow : IUiWindow {
    public bool AllowMainWindow { get => true; }
    public bool AllowFieldWindow { get => false; }
    public bool Enabled { get; set; } = true;
    public string TypeName { get => "Windows"; }
    public DebugGraphicsContext? Context { get; set; }
    public ImGuiController? ImGuiController { get; set; }
    public DebugFieldWindow? FieldWindow { get; set; }

    public DebugFieldWindow? SelectedWindow;
    public FieldListWindow? FieldList { get; private set; }

    public void Initialize(DebugGraphicsContext context, ImGuiController controller, DebugFieldWindow? fieldWindow) {
        Context = context;
        ImGuiController = controller;
        FieldWindow = fieldWindow;

        FieldList = controller.GetUiWindow<FieldListWindow>();
    }

    public void Render() {
        ImGui.Begin(TypeName);

        if (FieldList is null) {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));

            ImGui.Text("Error: missing field list window");

            ImGui.PopStyleColor();

            ImGui.End();

            return;
        }

        ImGui.Text(string.Format("Average frame time: {0} ms; {1} FPS", Context!.DeltaAverage, 1000.0f / Context!.DeltaAverage));
        ImGui.Text(string.Format("Min frame time: {0} ms; {1} FPS", Context!.DeltaMin, 1000.0f / Context!.DeltaMin));
        ImGui.Text(string.Format("Max frame time: {0} ms; {1} FPS", Context!.DeltaMax, 1000.0f / Context!.DeltaMax));

        bool newWindowDisabled = false;

        DebugFieldWindow[] windows = Context!.FieldWindows.ToArray();

        foreach (DebugFieldWindow window in windows) {
            newWindowDisabled |= window.IsClosing;
        }

        if (newWindowDisabled) {
            ImGui.BeginDisabled();
        }

        if (ImGui.Button("New window") && !newWindowDisabled) {
            SelectedWindow = Context!.FieldWindowOpened();
        }

        if (newWindowDisabled) {
            ImGui.EndDisabled();
        }

        bool selectWindowDisabled = SelectedWindow is null;

        if (selectWindowDisabled) {
            ImGui.BeginDisabled();
        }

        ImGui.SameLine();

        bool closeWindow = ImGui.Button("Close window");

        if (closeWindow && !selectWindowDisabled) {
            SelectedWindow!.Close();
        }

        if (selectWindowDisabled) {
            ImGui.EndDisabled();
        }

        if (closeWindow) {
            SelectedWindow = null;
        }

        if (ImGui.BeginTable("Active windows", 4)) {
            ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Window");
            ImGui.TableSetColumnIndex(1);
            ImGui.Text("Map Id");
            ImGui.TableSetColumnIndex(2);
            ImGui.Text("Map Name");
            ImGui.TableSetColumnIndex(3);
            ImGui.Text("Instance Id");

            int index = 0;
            foreach (DebugFieldWindow window in windows) {
                ImGui.TableNextRow();

                bool selected = window == SelectedWindow;
                bool nextSelected = false;

                ImGui.TableSetColumnIndex(0);
                nextSelected |= ImGui.Selectable(string.Format("{0}##Active windows {1} 0", window.WindowName, index), selected);
                ImGui.TableSetColumnIndex(1);
                nextSelected |= ImGui.Selectable(string.Format("{0}##Active windows {1} 1", window.ActiveRenderer?.Field.MapId.ToString() ?? "", index), selected);
                ImGui.TableSetColumnIndex(2);
                nextSelected |= ImGui.Selectable(string.Format("{0}##Active windows {1} 2", window.ActiveRenderer?.Field.Metadata.Name ?? "", index), selected);
                ImGui.TableSetColumnIndex(3);
                nextSelected |= ImGui.Selectable(string.Format("{0}##Active windows {1} 3", window.ActiveRenderer?.Field.InstanceId.ToString() ?? "", index), selected);

                if (nextSelected) {
                    SelectedWindow = window;
                    FieldList!.SelectedRenderer = window.ActiveRenderer;
                }

                ++index;
            }

            ImGui.EndTable();
        }

        ImGui.End();
    }
}

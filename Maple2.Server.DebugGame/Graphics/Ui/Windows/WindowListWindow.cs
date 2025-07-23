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

        ImGui.Text($"Average frame time: {Context!.DeltaAverage} ms; {1000.0f / Context!.DeltaAverage} FPS");
        ImGui.Text($"Min frame time: {Context!.DeltaMin} ms; {1000.0f / Context!.DeltaMin} FPS");
        ImGui.Text($"Max frame time: {Context!.DeltaMax} ms; {1000.0f / Context!.DeltaMax} FPS");

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
                nextSelected |= ImGui.Selectable($"{window.WindowName}##Active windows {index} 0", selected);
                ImGui.TableSetColumnIndex(1);
                nextSelected |= ImGui.Selectable($"{window.ActiveRenderer?.Field.MapId.ToString() ?? ""}##Active windows {index} 1", selected);
                ImGui.TableSetColumnIndex(2);
                nextSelected |= ImGui.Selectable($"{window.ActiveRenderer?.Field.Metadata.Name ?? ""}##Active windows {index} 2", selected);
                ImGui.TableSetColumnIndex(3);
                nextSelected |= ImGui.Selectable($"{window.ActiveRenderer?.Field.RoomId.ToString() ?? ""}##Active windows {index} 3", selected);

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

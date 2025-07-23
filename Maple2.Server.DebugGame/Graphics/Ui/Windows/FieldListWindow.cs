using ImGuiNET;
using System.Numerics;

namespace Maple2.Server.DebugGame.Graphics.Ui.Windows;

public class FieldListWindow : IUiWindow {
    public bool AllowMainWindow => true;
    public bool AllowFieldWindow => false;
    public bool Enabled { get; set; } = true;
    public string TypeName => "Fields";
    public DebugGraphicsContext? Context { get; set; }
    public ImGuiController? ImGuiController { get; set; }
    public DebugFieldWindow? FieldWindow { get; set; }

    public DebugFieldRenderer? SelectedRenderer;
    public WindowListWindow? WindowList { get; private set; }

    public void Initialize(DebugGraphicsContext context, ImGuiController controller, DebugFieldWindow? fieldWindow) {
        Context = context;
        ImGuiController = controller;
        FieldWindow = fieldWindow;

        WindowList = controller.GetUiWindow<WindowListWindow>();
    }

    public void Render() {
        ImGui.Begin(TypeName);

        if (WindowList is null) {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));

            ImGui.Text("Error: missing window list window");

            ImGui.PopStyleColor();

            ImGui.End();

            return;
        }

        bool selectFieldDisabled = WindowList.SelectedWindow is null || SelectedRenderer is null || WindowList.SelectedWindow?.ActiveRenderer == SelectedRenderer;

        if (selectFieldDisabled) {
            ImGui.BeginDisabled();
        }

        if (ImGui.Button("Select field") && !selectFieldDisabled) {
            WindowList.SelectedWindow!.SetActiveRenderer(SelectedRenderer);

            SelectedRenderer = null;
        }

        if (selectFieldDisabled) {
            ImGui.EndDisabled();
        }

        ImGui.SameLine();

        selectFieldDisabled = WindowList.SelectedWindow?.ActiveRenderer is null;

        if (selectFieldDisabled) {
            ImGui.BeginDisabled();
        }

        if (ImGui.Button("Unselect Field")) {
            WindowList.SelectedWindow!.SetActiveRenderer(null);
        }

        if (selectFieldDisabled) {
            ImGui.EndDisabled();
        }

        if (ImGui.BeginTable("Active fields", 3)) {
            ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Map Id");
            ImGui.TableSetColumnIndex(1);
            ImGui.Text("Map Name");
            ImGui.TableSetColumnIndex(2);
            ImGui.Text("Instance Id");

            int index = 0;
            foreach (DebugFieldRenderer renderer in Context!.FieldRenderers) {
                ImGui.TableNextRow();

                bool selected = renderer == SelectedRenderer || WindowList.SelectedWindow?.ActiveRenderer == renderer;
                bool nextSelected = false;

                selectFieldDisabled = WindowList.SelectedWindow?.ActiveRenderer == renderer;

                if (selectFieldDisabled) {
                    ImGui.BeginDisabled();
                }

                ImGui.TableSetColumnIndex(0);
                nextSelected |= ImGui.Selectable($"{renderer.Field.MapId}##Active fields {index} 0", selected);
                ImGui.TableSetColumnIndex(1);
                nextSelected |= ImGui.Selectable($"{renderer.Field.Metadata.Name}##Active fields {index} 1", selected);
                ImGui.TableSetColumnIndex(2);
                nextSelected |= ImGui.Selectable($"{renderer.Field.RoomId}##Active fields {index} 2", selected);

                if (selectFieldDisabled) {
                    ImGui.EndDisabled();
                }

                if (nextSelected) {
                    SelectedRenderer = renderer;

                    // Auto-create field window if none exists for this field
                    AutoCreateFieldWindow(renderer);
                }

                ++index;
            }

            ImGui.EndTable();
        }

        ImGui.End();
    }

    private void AutoCreateFieldWindow(DebugFieldRenderer renderer) {
        if (Context == null) return;

        // Check if a field window already exists for this renderer
        foreach (DebugFieldWindow existingWindow in Context.FieldWindows) {
            if (existingWindow.ActiveRenderer == renderer) {
                // Window already exists for this field, don't create another
                return;
            }
        }

        // No window exists for this field, create one
        DebugFieldWindow newWindow = Context.FieldWindowOpened();
        newWindow.SetActiveRenderer(renderer);
    }
}


using ImGuiNET;
using Maple2.Server.DebugGame.Graphics.Ui.Windows;
using Maple2.Server.Game.Model;

namespace Maple2.Server.DebugGame.Graphics.Ui;

public class FieldPropertiesWindow : IUiWindow {
    public bool AllowMainWindow { get => false; }
    public bool AllowFieldWindow { get => false; }
    public bool Enabled { get; set; } = true;
    public string TypeName { get => "Field Properties"; }
    public DebugGraphicsContext? Context { get; set; }
    public ImGuiController? ImGuiController { get; set; }
    public DebugFieldWindow? FieldWindow { get; set; }

    public void Initialize(DebugGraphicsContext context, ImGuiController controller, DebugFieldWindow? fieldWindow) {
        Context = context;
        ImGuiController = controller;
        FieldWindow = fieldWindow;
    }

    public void Render() {
        ImGui.Begin(TypeName);

        if (ImGui.BeginTable("Map properties", 2)) {
            ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Property");
            ImGui.TableSetColumnIndex(1);
            ImGui.Text("Value");

            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Map Name");
            ImGui.TableSetColumnIndex(1);
            ImGui.Text(FieldWindow!.ActiveRenderer?.Field.Metadata.Name ?? "");

            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Map Id");
            ImGui.TableSetColumnIndex(1);
            ImGui.Text(FieldWindow!.ActiveRenderer?.Field.MapId.ToString() ?? "");

            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Instance Id");
            ImGui.TableSetColumnIndex(1);
            ImGui.Text(FieldWindow!.ActiveRenderer?.Field.InstanceId.ToString() ?? "");

            ImGui.EndTable();
        }

        if (ImGui.BeginTable("Players", 1)) {
            ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Player Name");

            if (FieldWindow!.ActiveRenderer is not null) {
                foreach ((int id, FieldPlayer player) in FieldWindow!.ActiveRenderer.Field.GetPlayers()) {
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(player.Value.Character.Name);
                }
            }

            ImGui.EndTable();
        }

        if (ImGui.BeginTable("Npcs", 3)) {
            ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Npc Name");
            ImGui.TableSetColumnIndex(1);
            ImGui.Text("Npc Id");
            ImGui.TableSetColumnIndex(2);
            ImGui.Text("Npc Level");
            //ImGui.TableSetColumnIndex(3);
            //ImGui.Text("Npc Type");

            if (FieldWindow!.ActiveRenderer is not null) {
                foreach (FieldNpc npc in FieldWindow!.ActiveRenderer.Field.EnumerateNpcs()) {
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(npc.Value.Metadata.Name);
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text(npc.Value.Id.ToString());
                    ImGui.TableSetColumnIndex(2);
                    ImGui.Text(npc.Value.Metadata.Basic.Level.ToString());
                }
            }

            ImGui.EndTable();
        }

        ImGui.End();
    }
}

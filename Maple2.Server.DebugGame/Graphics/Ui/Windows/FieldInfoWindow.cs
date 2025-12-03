using ImGuiNET;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.DebugGame.Graphics.Ui.Windows;

public class FieldInfoWindow : IUiWindow {
    public bool AllowMainWindow => false;
    public bool AllowFieldWindow => true;
    public bool Enabled { get; set; } = true;
    public string TypeName => "Field Information";
    public DebugGraphicsContext? Context { get; set; }
    public ImGuiController? ImGuiController { get; set; }
    public DebugFieldWindow? FieldWindow { get; set; }

    public void Initialize(DebugGraphicsContext context, ImGuiController controller, DebugFieldWindow? fieldWindow) {
        Context = context;
        ImGuiController = controller;
        FieldWindow = fieldWindow;
    }

    public void Render() {
        if (FieldWindow?.ActiveRenderer == null) return;
        DebugFieldRenderer? r = FieldWindow.ActiveRenderer;
        FieldManager field = r.Field;
        ImGui.Begin(TypeName);
        // Field Info Section

        ImGui.Text($"Map ID: {field.MapId}");
        ImGui.Text($"Room ID: {field.RoomId}");
        ImGui.Text($"Map Name: {field.Metadata.Name}");
        ImGui.Text($"Field Type: {field.FieldType}");
        ImGui.Text($"Field Instance Type: {field.FieldInstance.Type}");
        if (field.RoomTimer != null) ImGui.Text($"Room Timer Active: {field.RoomTimer.Duration}");
        if (field.DungeonId > 0) ImGui.Text($"Dungeon ID: {field.DungeonId}");

        if (field.AccelerationStructure != null) {
            ImGui.Separator();
            ImGui.Text("Field Entity Information:");
            ImGui.Text($"Grid Size: {field.AccelerationStructure.GridSize}");
            ImGui.Text($"Min Index: {field.AccelerationStructure.MinIndex}");
            ImGui.Text($"Max Index: {field.AccelerationStructure.MaxIndex}");
            ImGui.Separator();
            ImGui.Text("Entity Collection Sizes:");
            ImGui.Text($"Aligned: {field.AccelerationStructure.AlignedEntities.Length}");
            ImGui.Text($"Aligned Trimmed: {field.AccelerationStructure.AlignedTrimmedEntities.Length}");
            ImGui.Text($"Unaligned: {field.AccelerationStructure.UnalignedEntities.Length}");
            ImGui.Text($"Vibrate: {field.AccelerationStructure.VibrateEntities.Length}");
        }

        ImGui.Separator();
        ImGui.Text("Entity Counts:");
        ImGui.Indent();
        ImGui.Text($"Players: {field.Players.Count}");
        ImGui.Text($"NPCs: {field.Npcs.Count}");
        ImGui.Text($"Mobs: {field.Mobs.Count}");
        ImGui.Text($"Pets: {field.Pets.Count}");
        ImGui.Unindent();
        // Actor List Section (inside Field Info window)

        int totalActors = field.Players.Count + field.Npcs.Count + field.Mobs.Count;
        if (totalActors == 0) {
            ImGui.Text("No active actors");
        } else {
            bool clearSelectionDisabled = r.SelectedActor == null;
            if (clearSelectionDisabled) ImGui.BeginDisabled();
            if (ImGui.Button("Clear Selection") && !clearSelectionDisabled) {
                r.SelectedActor = null;
            }
            if (clearSelectionDisabled) ImGui.EndDisabled();
            if (ImGui.CollapsingHeader($"Players ({field.Players.Count})", ImGuiTreeNodeFlags.None)) {
                if (ImGui.BeginTable("Players Table", 4)) {
                    ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text("Name");
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text("Level");
                    ImGui.TableSetColumnIndex(2);
                    ImGui.Text("Object ID");
                    ImGui.TableSetColumnIndex(3);
                    ImGui.Text("Status");
                    int idx = 0;
                    foreach ((int objectId, FieldPlayer player) in field.Players) {
                        ImGui.TableNextRow();
                        bool selected = player == r.SelectedActor;
                        bool nextSel = false;
                        ImGui.TableSetColumnIndex(0);
                        nextSel |= ImGui.Selectable($"{player.Value.Character.Name}##P{idx}0", selected);
                        ImGui.TableSetColumnIndex(1);
                        nextSel |= ImGui.Selectable($"{player.Value.Character.Level}##P{idx}1", selected);
                        ImGui.TableSetColumnIndex(2);
                        nextSel |= ImGui.Selectable($"{objectId}##P{idx}2", selected);
                        ImGui.TableSetColumnIndex(3);
                        nextSel |= ImGui.Selectable($"{(player.IsDead ? "Dead" : "Alive")}##P{idx}3", selected);
                        if (nextSel) r.SelectedActor = player;
                        idx++;
                    }
                    ImGui.EndTable();
                }
            }
            int totalNpcs = field.Npcs.Count + field.Mobs.Count;
            if (ImGui.CollapsingHeader($"NPCs ({totalNpcs})", ImGuiTreeNodeFlags.None)) {
                if (ImGui.BeginTable("NPCs Table", 5)) {
                    ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text("Type");
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text("Name");
                    ImGui.TableSetColumnIndex(2);
                    ImGui.Text("Level");
                    ImGui.TableSetColumnIndex(3);
                    ImGui.Text("Object ID");
                    ImGui.TableSetColumnIndex(4);
                    ImGui.Text("Status");
                    int idx = 0;
                    foreach ((int objectId, FieldNpc npc) in field.Npcs) {
                        ImGui.TableNextRow();
                        bool selected = npc == r.SelectedActor;
                        bool nextSel = false;
                        ImGui.TableSetColumnIndex(0);
                        nextSel |= ImGui.Selectable($"NPC##N{idx}0", selected);
                        ImGui.TableSetColumnIndex(1);
                        nextSel |= ImGui.Selectable($"{npc.Value.Metadata.Name}##N{idx}1", selected);
                        ImGui.TableSetColumnIndex(2);
                        nextSel |= ImGui.Selectable($"{npc.Value.Metadata.Basic.Level}##N{idx}2", selected);
                        ImGui.TableSetColumnIndex(3);
                        nextSel |= ImGui.Selectable($"{objectId}##N{idx}3", selected);
                        ImGui.TableSetColumnIndex(4);
                        nextSel |= ImGui.Selectable($"{(npc.IsDead ? "Dead" : "Alive")}##N{idx}4", selected);
                        if (nextSel) r.SelectedActor = npc;
                        idx++;
                    }
                    foreach ((int objectId, FieldNpc mob) in field.Mobs) {
                        ImGui.TableNextRow();
                        bool selected = mob == r.SelectedActor;
                        bool nextSel = false;
                        ImGui.TableSetColumnIndex(0);
                        nextSel |= ImGui.Selectable($"Mob##M{idx}0", selected);
                        ImGui.TableSetColumnIndex(1);
                        nextSel |= ImGui.Selectable($"{mob.Value.Metadata.Name}##M{idx}1", selected);
                        ImGui.TableSetColumnIndex(2);
                        nextSel |= ImGui.Selectable($"{mob.Value.Metadata.Basic.Level}##M{idx}2", selected);
                        ImGui.TableSetColumnIndex(3);
                        nextSel |= ImGui.Selectable($"{objectId}##M{idx}3", selected);
                        ImGui.TableSetColumnIndex(4);
                        nextSel |= ImGui.Selectable($"{(mob.IsDead ? "Dead" : "Alive")}##M{idx}4", selected);
                        if (nextSel) r.SelectedActor = mob;
                        idx++;
                    }
                    ImGui.EndTable();
                }
            }
        }

        ImGuiController.ClampWindowToViewport();
        ImGui.End();
    }
}

using System.Numerics;
using Maple2.Server.Game.DebugGraphics;
using Maple2.Server.Game.Manager.Field;
using ImGuiNET;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Server.Game.Manager;
using Maple2.Server.Game.Model;

namespace Maple2.Server.DebugGame.Graphics;

public class DebugFieldRenderer : IFieldRenderer {
    public DebugGraphicsContext Context { get; init; }
    public FieldManager Field { get; init; }
    public bool IsActive {
        get {
            activeMutex.WaitOne();
            bool isActive = activeWindows.Count > 0;
            activeMutex.ReleaseMutex();
            return isActive;
        }
    }

    private readonly HashSet<DebugFieldWindow> activeWindows = [];
    private readonly Mutex activeMutex = new();
    private IActor? selectedActor = null;

    public DebugFieldRenderer(DebugGraphicsContext context, FieldManager field) {
        Context = context;
        Field = field;
    }

    public void Update() {
        if (!IsActive) {
            return;
        }

        if (!Context.HasFieldUpdated(Field)) {
            Context.FieldUpdated(Field);

            Field.Update();
        }
    }

    public void Render(double delta) {
        if (!IsActive) {
            return;
        }

        // Store field window position and size for positioning player details panel
        Vector2 fieldWindowPos = Vector2.Zero;
        Vector2 fieldWindowSize = Vector2.Zero;

        // Create a field information window
        if (ImGui.Begin("Field Information")) {
            // Get window position and size while the window is active
            fieldWindowPos = ImGui.GetWindowPos();
            fieldWindowSize = ImGui.GetWindowSize();

            RenderFieldBasicInfo();
            ImGui.Separator();
            RenderEntityCounts();
            ImGui.Separator();
            RenderActorList();
            ImGui.Separator();
            RenderFieldProperties();
        }
        ImGui.End();

        // Show actor details panel if an actor is selected
        if (selectedActor != null) {
            RenderActorDetailsPanel(fieldWindowPos, fieldWindowSize);
        }
    }

    private void RenderFieldBasicInfo() {
        ImGui.Text($"Map ID: {Field.MapId}");
        ImGui.Text($"Room ID: {Field.RoomId}");
        ImGui.Text($"Map Name: {Field.Metadata.Name}");
        ImGui.Text($"Field Type: {Field.FieldType}");

        if (Field.DungeonId > 0) {
            ImGui.Text($"Dungeon ID: {Field.DungeonId}");
        }
    }

    private void RenderEntityCounts() {
        ImGui.Text("Entity Counts:");
        ImGui.Indent();
        ImGui.Text($"Players: {Field.Players.Count}");
        ImGui.Text($"NPCs: {Field.Npcs.Count}");
        ImGui.Text($"Mobs: {Field.Mobs.Count}");
        ImGui.Text($"Pets: {Field.Pets.Count}");
        ImGui.Unindent();
    }

    private void RenderActorList() {
        ImGui.Text("Active Actors:");

        int totalActors = Field.Players.Count + Field.Npcs.Count + Field.Mobs.Count;
        if (totalActors == 0) {
            ImGui.Text("No active actors");
            return;
        }

        bool showActorDetailsDisabled = selectedActor == null;

        if (showActorDetailsDisabled) {
            ImGui.BeginDisabled();
        }

        if (ImGui.Button("Show Actor Details") && !showActorDetailsDisabled) {
            // Keep the selected actor to show details panel
        }

        if (showActorDetailsDisabled) {
            ImGui.EndDisabled();
        }

        ImGui.SameLine();

        bool clearSelectionDisabled = selectedActor == null;

        if (clearSelectionDisabled) {
            ImGui.BeginDisabled();
        }

        if (ImGui.Button("Clear Selection") && !clearSelectionDisabled) {
            selectedActor = null;
        }

        if (clearSelectionDisabled) {
            ImGui.EndDisabled();
        }

        if (ImGui.BeginTable("Active Actors", 5)) {
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

            int index = 0;

            // Render Players
            foreach ((int objectId, FieldPlayer player) in Field.Players) {
                ImGui.TableNextRow();

                bool selected = player == selectedActor;
                bool nextSelected = false;

                ImGui.TableSetColumnIndex(0);
                nextSelected |= ImGui.Selectable($"Player##Actor {index} 0", selected);
                ImGui.TableSetColumnIndex(1);
                nextSelected |= ImGui.Selectable($"{player.Value.Character.Name}##Actor {index} 1", selected);
                ImGui.TableSetColumnIndex(2);
                nextSelected |= ImGui.Selectable($"{player.Value.Character.Level}##Actor {index} 2", selected);
                ImGui.TableSetColumnIndex(3);
                nextSelected |= ImGui.Selectable($"{objectId}##Actor {index} 3", selected);
                ImGui.TableSetColumnIndex(4);
                nextSelected |= ImGui.Selectable($"{(player.IsDead ? "Dead" : "Alive")}##Actor {index} 4", selected);

                if (nextSelected) {
                    selectedActor = player;
                }

                ++index;
            }

            // Render NPCs
            foreach ((int objectId, FieldNpc npc) in Field.Npcs) {
                ImGui.TableNextRow();

                bool selected = npc == selectedActor;
                bool nextSelected = false;

                ImGui.TableSetColumnIndex(0);
                nextSelected |= ImGui.Selectable($"NPC##Actor {index} 0", selected);
                ImGui.TableSetColumnIndex(1);
                nextSelected |= ImGui.Selectable($"{npc.Value.Metadata.Name}##Actor {index} 1", selected);
                ImGui.TableSetColumnIndex(2);
                nextSelected |= ImGui.Selectable($"{npc.Value.Metadata.Basic.Level}##Actor {index} 2", selected);
                ImGui.TableSetColumnIndex(3);
                nextSelected |= ImGui.Selectable($"{objectId}##Actor {index} 3", selected);
                ImGui.TableSetColumnIndex(4);
                nextSelected |= ImGui.Selectable($"{(npc.IsDead ? "Dead" : "Alive")}##Actor {index} 4", selected);

                if (nextSelected) {
                    selectedActor = npc;
                }

                ++index;
            }

            // Render Mobs
            foreach ((int objectId, FieldNpc mob) in Field.Mobs) {
                ImGui.TableNextRow();

                bool selected = mob == selectedActor;
                bool nextSelected = false;

                ImGui.TableSetColumnIndex(0);
                nextSelected |= ImGui.Selectable($"Mob##Actor {index} 0", selected);
                ImGui.TableSetColumnIndex(1);
                nextSelected |= ImGui.Selectable($"{mob.Value.Metadata.Name}##Actor {index} 1", selected);
                ImGui.TableSetColumnIndex(2);
                nextSelected |= ImGui.Selectable($"{mob.Value.Metadata.Basic.Level}##Actor {index} 2", selected);
                ImGui.TableSetColumnIndex(3);
                nextSelected |= ImGui.Selectable($"{objectId}##Actor {index} 3", selected);
                ImGui.TableSetColumnIndex(4);
                nextSelected |= ImGui.Selectable($"{(mob.IsDead ? "Dead" : "Alive")}##Actor {index} 4", selected);

                if (nextSelected) {
                    selectedActor = mob;
                }

                ++index;
            }

            ImGui.EndTable();
        }
    }

    private void RenderFieldProperties() {
        ImGui.Text("Field Status:");
        ImGui.Indent();

        if (Field.RoomTimer != null) {
            ImGui.Text($"Room Timer Active: {Field.RoomTimer.Duration}");
        }

        if (Field.AccelerationStructure != null) {
            ImGui.Text("Acceleration Structure: Active");
        }

        ImGui.Text($"Field Instance Type: {Field.FieldInstance.Type}");
        ImGui.Unindent();
    }

    private void RenderActorDetailsPanel(Vector2 fieldWindowPos, Vector2 fieldWindowSize) {
        if (selectedActor == null) return;

        // Position the actor details panel to the right of the field information panel
        ImGui.SetNextWindowPos(new Vector2(
            fieldWindowPos.X + fieldWindowSize.X + 10, // 10px gap
            fieldWindowPos.Y
        ));

        // Set a reasonable size for the actor details panel
        ImGui.SetNextWindowSize(new Vector2(350, 500), ImGuiCond.FirstUseEver);

        // Get actor name for window title
        string actorName = GetActorName(selectedActor);
        string actorType = GetActorType(selectedActor);

        // Create a separate window for actor details
        if (ImGui.Begin($"{actorType} Details: {actorName}##ActorDetails")) {
            RenderActorBasicInfo();
            ImGui.Separator();
            RenderActorPositionInfo();
            ImGui.Separator();
            RenderActorStatsInfo();
            ImGui.Separator();
            RenderActorAdditionalInfo();
        }
        ImGui.End();
    }

    private string GetActorName(IActor actor) {
        return actor switch {
            FieldPlayer player => player.Value.Character.Name,
            FieldNpc npc => npc.Value.Metadata.Name ?? "Unknown",
            _ => "Unknown",
        };
    }

    private string GetActorType(IActor actor) {
        return actor switch {
            FieldPlayer => "Player",
            FieldNpc => "NPC",
            _ => "Unknown",
        };
    }

    private void RenderActorBasicInfo() {
        if (selectedActor == null) return;

        ImGui.Text($"Type: {GetActorType(selectedActor)}");
        ImGui.Text($"Name: {GetActorName(selectedActor)}");
        ImGui.Text($"Object ID: {selectedActor.ObjectId}");
        ImGui.Text($"Is Dead: {selectedActor.IsDead}");

        switch (selectedActor) {
            case FieldPlayer player:
                ImGui.Text($"State: {player.State}");
                ImGui.Text($"Sub State: {player.SubState}");
                Character character = player.Value.Character;
                ImGui.Text($"Level: {character.Level}");
                ImGui.Text($"Job: {character.Job}");
                ImGui.Text($"Gender: {character.Gender}");
                ImGui.Text($"Account ID: {player.Value.Account.Id}");
                ImGui.Text($"Character ID: {character.Id}");
                break;
            case FieldNpc npc:
                ImGui.Text($"State: {npc.State.State}");
                ImGui.Text($"Sub State: {npc.State.SubState}");
                ImGui.Text($"Level: {npc.Value.Metadata.Basic.Level}");
                ImGui.Text($"NPC ID: {npc.Value.Metadata.Id}");
                ImGui.Text($"Model name: {npc.Value.Metadata.Model.Name}");
                ImGui.Text($"Animation Speed: {npc.Value.Metadata.Model.AniSpeed}");
                ImGui.Text($"Is Boss: {npc.Value.IsBoss}");
                break;
        }
    }

    private void RenderActorPositionInfo() {
        if (selectedActor == null) return;

        ImGui.Text("Position & Movement:");
        ImGui.Indent();
        ImGui.Text($"Position: {selectedActor.Position}");
        ImGui.Text($"Rotation: {selectedActor.Rotation}");
        if (selectedActor is FieldNpc npc) {
            ImGui.Text($"Velocity: {npc.MovementState.Velocity}");
        }
        ImGui.Text($"Playing Sequence: {selectedActor.Animation.PlayingSequence?.Name ?? "None"}");

        if (selectedActor is FieldPlayer player) {
            ImGui.Text($"Last Ground Position: {player.LastGroundPosition}");
            ImGui.Text($"In Battle: {player.InBattle}");
        }

        ImGui.Unindent();
    }

    private void RenderActorStatsInfo() {
        if (selectedActor == null) return;

        ImGui.Text("Stats:");
        ImGui.Indent();

        var stats = selectedActor.Stats;
        ImGui.Text($"Health: {stats.Values[BasicAttribute.Health].Current}/{stats.Values[BasicAttribute.Health].Total}");

        // Show additional stats for players
        if (selectedActor is FieldPlayer) {
            ImGui.Text($"Spirit: {stats.Values[BasicAttribute.Spirit].Current}/{stats.Values[BasicAttribute.Spirit].Total}");
            ImGui.Text($"Stamina: {stats.Values[BasicAttribute.Stamina].Current}/{stats.Values[BasicAttribute.Stamina].Total}");
        }

        ImGui.Unindent();
    }

    private void RenderActorAdditionalInfo() {
        if (selectedActor == null) return;

        ImGui.Text("Additional Information:");
        ImGui.Indent();

        switch (selectedActor) {
            case FieldPlayer player:
                ImGui.Text($"Admin Permissions: {player.AdminPermissions}");
                break;
            case FieldNpc npc:
                ImGui.Text($"NPC Type: {(npc.Value.Metadata.Basic.Kind == 0 ? "Friendly" : "Hostile")}");
                if (npc.Owner != null) {
                    ImGui.Text($"Spawn Point ID: {npc.SpawnPointId}");
                }
                break;
            default:
                ImGui.Text("No additional information available.");
                break;
        }

        ImGui.Unindent();
    }

    public void CleanUp() { }

    public void AttachWindow(DebugFieldWindow window) {
        activeMutex.WaitOne();

        activeWindows.Add(window);

        activeMutex.ReleaseMutex();
    }

    public void DetachWindow(DebugFieldWindow window) {
        activeMutex.WaitOne();

        activeWindows.Remove(window);

        activeMutex.ReleaseMutex();
    }
}

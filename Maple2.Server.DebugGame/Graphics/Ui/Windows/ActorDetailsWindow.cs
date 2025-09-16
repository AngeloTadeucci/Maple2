using ImGuiNET;
using Maple2.Server.Game.Model;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Server.Game.Manager;

namespace Maple2.Server.DebugGame.Graphics.Ui.Windows;

public class ActorDetailsWindow : IUiWindow {
    public bool AllowMainWindow => false;
    public bool AllowFieldWindow => true;
    public bool Enabled { get; set; } = true;
    public bool HideFromMenuBar => true; // Hide from main menu bar, only show when an actor is selected
    public string TypeName => "Actor Details";
    public DebugGraphicsContext? Context { get; set; }
    public ImGuiController? ImGuiController { get; set; }
    public DebugFieldWindow? FieldWindow { get; set; }

    public void Initialize(DebugGraphicsContext context, ImGuiController controller, DebugFieldWindow? fieldWindow) {
        Context = context;
        ImGuiController = controller;
        FieldWindow = fieldWindow;
    }

    public void Render() {
        DebugFieldRenderer? activeRenderer = FieldWindow?.ActiveRenderer;
        if (activeRenderer?.SelectedActor == null) {
            // Hide window when no actor selected
            return;
        }

        string baseTitle = $"{activeRenderer.GetActorType(activeRenderer.SelectedActor)} Details: {activeRenderer.GetActorName(activeRenderer.SelectedActor)}###Actor Details"; // stable ID after ### for docking

        if (ImGui.Begin(baseTitle)) {
            IActor actor = activeRenderer.SelectedActor!;
            ImGui.Text($"Type: {activeRenderer.GetActorType(actor)}");
            ImGui.Text($"Name: {activeRenderer.GetActorName(actor)}");
            ImGui.Text($"Object ID: {actor.ObjectId}");
            ImGui.Text($"Is Dead: {actor.IsDead}");
            switch (actor) {
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
            ImGui.Separator();
            ImGui.Text("Position & Movement:");
            ImGui.Indent();
            ImGui.Text($"Position: {actor.Position}");
            ImGui.Text($"Rotation: {actor.Rotation}");
            if (actor is FieldNpc npc2) ImGui.Text($"Velocity: {npc2.MovementState.Velocity}");
            ImGui.Text($"Playing Sequence: {actor.Animation.PlayingSequence?.Name ?? "None"}");
            if (actor is FieldPlayer pl2) {
                ImGui.Text($"Last Ground Position: {pl2.LastGroundPosition}");
                ImGui.Text($"In Battle: {pl2.InBattle}");
            }
            ImGui.Unindent();
            ImGui.Separator();
            ImGui.Text("Stats:");
            ImGui.Indent();
            StatsManager stats = actor.Stats;
            ImGui.Text($"Health: {stats.Values[BasicAttribute.Health].Current}/{stats.Values[BasicAttribute.Health].Total}");
            if (actor is FieldPlayer) {
                ImGui.Text($"Spirit: {stats.Values[BasicAttribute.Spirit].Current}/{stats.Values[BasicAttribute.Spirit].Total}");
                ImGui.Text($"Stamina: {stats.Values[BasicAttribute.Stamina].Current}/{stats.Values[BasicAttribute.Stamina].Total}");
            }
            ImGui.Unindent();
            ImGui.Separator();
            ImGui.Text("Additional Information");
            switch (actor) {
                case FieldPlayer player:
                    ImGui.Text($"Admin Permissions: {player.AdminPermissions}");
                    ImGui.Separator();
                    ImGui.Text("Movement Controls:");
                    bool moveMode = activeRenderer.PlayerMoveMode;
                    if (ImGui.Checkbox("Move Player Mode", ref moveMode)) activeRenderer.PlayerMoveMode = moveMode;
                    if (activeRenderer.PlayerMoveMode) {
                        ImGui.Indent();
                        bool force = activeRenderer.ForceMove;
                        if (ImGui.Checkbox("Force Move (bypass validation)", ref force)) activeRenderer.ForceMove = force;
                        ImGui.Text("Click on the map to move this player");
                        ImGui.Unindent();
                    }
                    break;
                case FieldNpc npc:
                    ImGui.Text($"NPC Type: {(npc.Value.Metadata.Basic.Kind == 0 ? "Friendly" : "Hostile")}");
                    if (npc.Owner != null) ImGui.Text($"Spawn Point ID: {npc.SpawnPointId}");
                    break;
                default:
                    ImGui.Text("No additional information available.");
                    break;
            }

        }
        ImGuiController.ClampWindowToViewport();
        ImGui.End();
    }
}

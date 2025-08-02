using System.Numerics;
using Maple2.Server.Game.DebugGraphics;
using Maple2.Server.Game.Manager.Field;
using ImGuiNET;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata.FieldEntity;
using Maple2.Server.Game.Manager;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Tools.VectorMath;
using Maple2.Tools.Collision;
using Silk.NET.Maths;

namespace Maple2.Server.DebugGame.Graphics;

public class DebugFieldRenderer : IFieldRenderer {
    public DebugGraphicsContext Context { get; init; }
    public FieldManager Field { get; init; }

    // Coordinate system transformation - flip Y/Z axes but keep original scale
    private static readonly Matrix4x4 MapRotation = Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, (float) (-Math.PI / 2));

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
    private IActor? selectedActor;

    // 3D visualization settings
    private bool showBoxColliders = true; // Box colliders enabled by default (main map blocks)
    private bool showMeshColliders; // Mesh colliders disabled by default
    private bool showSpawnPoints; // Spawn points disabled by default
    private bool showVibrateObjects; // Vibrate objects disabled by default
    private bool showPortals = true; // Portals enabled by default
    private bool showPortalInformation = true; // Portal text information enabled by default
    private bool showPortalConnections = true; // Portal connection lines enabled by default
    private bool showActors = true; // Actors enabled by default
    private bool showPlayers = true; // Players enabled by default
    private bool showNpcs = true; // NPCs enabled by default
    private bool showMobs = true; // Mobs enabled by default

    // Player movement mode
    private bool playerMoveMode = false; // Player move mode disabled by default
    private bool forceMove = false; // Force move (bypass validation) disabled by default

    // Entity caching for performance
    private FieldEntity[]? cachedStaticEntities; // Cache static entities (spawn points, triggers, etc.)

    private const float AgentRadiusMeters = 0.3f;
    private const float AgentHeightMeters = 1.4f;
    private const float GameUnitsPerMeter = 100.0f;

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

        // Auto-follow first player when field window is first opened or when a new player joins
        // But only if user hasn't manually stopped following
        if (!Context.CameraController.IsFollowingPlayer && !Context.HasManuallyStopped) {
            FieldPlayer? firstPlayer = Field.Players.Values.FirstOrDefault();
            if (firstPlayer != null && (!hasTriedAutoFollow || Context.CameraController.FollowedPlayerId != firstPlayer.Value.Character.Id)) {
                Context.StartFollowingPlayer(firstPlayer.Value.Character.Id);
                hasTriedAutoFollow = true;
            }
        }

        // Update camera follow if active
        UpdateCameraFollow();

        // We'll use ImGui for text rendering instead of 3D billboards
        // ImGui is much simpler and more reliable for debug text
    }

    private bool hasTriedAutoFollow = false;

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
            RenderFieldEntityDetails();
        }
        ImGui.End();

        // Render visualization controls
        RenderVisualizationControls();

        // Show actor details panel if an actor is selected
        if (selectedActor != null) {
            RenderActorDetailsPanel(fieldWindowPos, fieldWindowSize);
        }
    }

    // New method specifically for field window 3D rendering
    public void RenderFieldWindow3D(double delta) {
        if (!IsActive) {
            return;
        }

        // Handle mouse input for actor selection
        HandleMouseInput();

        // Render 3D field visualization using the DirectX pipeline
        RenderField3DVisualization(delta);
    }

    private void HandleMouseInput() {
        // Check if mouse is clicked and we're hovering over the field window
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left)) {
            // Don't process clicks if ImGui is handling them (mouse over UI elements)
            if (ImGui.GetIO().WantCaptureMouse) {
                return;
            }

            // Get mouse position in screen coordinates
            Vector2 mousePos = ImGui.GetMousePos();

            if (playerMoveMode && selectedActor is FieldPlayer selectedPlayer) {
                // Move mode: try to move the selected player to clicked position
                TryMovePlayerToScreenPosition(selectedPlayer, mousePos);
            } else {
                // Selection mode: try to select an actor at clicked position
                TrySelectActorAtScreenPosition(mousePos);
            }
        }
    }

    private void RenderFieldBasicInfo() {
        ImGui.Text($"Map ID: {Field.MapId}");
        ImGui.Text($"Room ID: {Field.RoomId}");
        ImGui.Text($"Map Name: {Field.Metadata.Name}");
        ImGui.Text($"Field Type: {Field.FieldType}");
        ImGui.Text($"Field Instance Type: {Field.FieldInstance.Type}");
        if (Field.RoomTimer != null) {
            ImGui.Text($"Room Timer Active: {Field.RoomTimer.Duration}");
        }

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

        // Actor selection controls
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

        // Players section (collapsed by default)
        if (ImGui.CollapsingHeader($"Players ({Field.Players.Count})", ImGuiTreeNodeFlags.None)) {
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

                int playerIndex = 0;
                foreach ((int objectId, FieldPlayer player) in Field.Players) {
                    ImGui.TableNextRow();
                    bool selected = player == selectedActor;
                    bool nextSelected = false;

                    ImGui.TableSetColumnIndex(0);
                    nextSelected |= ImGui.Selectable($"{player.Value.Character.Name}##Player {playerIndex} 0", selected);
                    ImGui.TableSetColumnIndex(1);
                    nextSelected |= ImGui.Selectable($"{player.Value.Character.Level}##Player {playerIndex} 1", selected);
                    ImGui.TableSetColumnIndex(2);
                    nextSelected |= ImGui.Selectable($"{objectId}##Player {playerIndex} 2", selected);
                    ImGui.TableSetColumnIndex(3);
                    nextSelected |= ImGui.Selectable($"{(player.IsDead ? "Dead" : "Alive")}##Player {playerIndex} 3", selected);

                    if (nextSelected) {
                        selectedActor = player;
                    }
                    ++playerIndex;
                }
                ImGui.EndTable();
            }
        }

        // NPCs section (collapsed by default, includes both NPCs and Mobs)
        int totalNpcs = Field.Npcs.Count + Field.Mobs.Count;
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

                int npcIndex = 0;

                // Render NPCs
                foreach ((int objectId, FieldNpc npc) in Field.Npcs) {
                    ImGui.TableNextRow();
                    bool selected = npc == selectedActor;
                    bool nextSelected = false;

                    ImGui.TableSetColumnIndex(0);
                    nextSelected |= ImGui.Selectable($"NPC##NPC {npcIndex} 0", selected);
                    ImGui.TableSetColumnIndex(1);
                    nextSelected |= ImGui.Selectable($"{npc.Value.Metadata.Name}##NPC {npcIndex} 1", selected);
                    ImGui.TableSetColumnIndex(2);
                    nextSelected |= ImGui.Selectable($"{npc.Value.Metadata.Basic.Level}##NPC {npcIndex} 2", selected);
                    ImGui.TableSetColumnIndex(3);
                    nextSelected |= ImGui.Selectable($"{objectId}##NPC {npcIndex} 3", selected);
                    ImGui.TableSetColumnIndex(4);
                    nextSelected |= ImGui.Selectable($"{(npc.IsDead ? "Dead" : "Alive")}##NPC {npcIndex} 4", selected);

                    if (nextSelected) {
                        selectedActor = npc;
                    }
                    ++npcIndex;
                }

                // Render Mobs
                foreach ((int objectId, FieldNpc mob) in Field.Mobs) {
                    ImGui.TableNextRow();
                    bool selected = mob == selectedActor;
                    bool nextSelected = false;

                    ImGui.TableSetColumnIndex(0);
                    nextSelected |= ImGui.Selectable($"Mob##Mob {npcIndex} 0", selected);
                    ImGui.TableSetColumnIndex(1);
                    nextSelected |= ImGui.Selectable($"{mob.Value.Metadata.Name}##Mob {npcIndex} 1", selected);
                    ImGui.TableSetColumnIndex(2);
                    nextSelected |= ImGui.Selectable($"{mob.Value.Metadata.Basic.Level}##Mob {npcIndex} 2", selected);
                    ImGui.TableSetColumnIndex(3);
                    nextSelected |= ImGui.Selectable($"{objectId}##Mob {npcIndex} 3", selected);
                    ImGui.TableSetColumnIndex(4);
                    nextSelected |= ImGui.Selectable($"{(mob.IsDead ? "Dead" : "Alive")}##Mob {npcIndex} 4", selected);

                    if (nextSelected) {
                        selectedActor = mob;
                    }
                    ++npcIndex;
                }
                ImGui.EndTable();
            }
        }
    }

    private void RenderFieldEntityDetails() {
        if (Field.AccelerationStructure == null) {
            return;
        }
        ImGui.Text("Field Entity Information:");
        ImGui.Separator();

        // Basic field info
        ImGui.Text($"Grid Size: {Field.AccelerationStructure.GridSize}");
        ImGui.Text($"Min Index: {Field.AccelerationStructure.MinIndex}");
        ImGui.Text($"Max Index: {Field.AccelerationStructure.MaxIndex}");

        ImGui.Separator();

        // Entity counts by type
        FieldEntity[] allEntities = GetAllEntities().ToArray();
        Dictionary<string, int> entityCounts = allEntities.GroupBy(e => e.GetType().Name).ToDictionary(g => g.Key, g => g.Count());

        ImGui.Text("Entity Types:");
        foreach (KeyValuePair<string, int> kvp in entityCounts) {
            ImGui.Text($"  {kvp.Key}: {kvp.Value}");
        }
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

        StatsManager stats = selectedActor.Stats;
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

        ImGui.Text("Additional Information");

        switch (selectedActor) {
            case FieldPlayer player:
                ImGui.Text($"Admin Permissions: {player.AdminPermissions}");

                // Player movement controls
                ImGui.Separator();
                ImGui.Text("Movement Controls:");
                ImGui.Checkbox("Move Player Mode", ref playerMoveMode);
                if (playerMoveMode) {
                    ImGui.Indent();
                    ImGui.Checkbox("Force Move (bypass validation)", ref forceMove);
                    ImGui.Text("Click on the map to move this player");
                    ImGui.Unindent();
                }
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

    }

    public void CleanUp() {
        // No cleanup needed for ImGui text rendering
    }

    public void AttachWindow(DebugFieldWindow window) {
        activeMutex.WaitOne();

        activeWindows.Add(window);

        activeMutex.ReleaseMutex();

        // Set default camera to field overview when window is first attached
        SetFieldOverviewCamera();

        // Auto-follow first player when field window is opened
        // But only if user hasn't manually stopped following
        if (!Context.CameraController.IsFollowingPlayer && !Context.HasManuallyStopped) {
            FieldPlayer? firstPlayer = Field.Players.Values.FirstOrDefault();
            if (firstPlayer != null) {
                Context.StartFollowingPlayer(firstPlayer.Value.Character.Id);
            }
        }
    }

    public void DetachWindow(DebugFieldWindow window) {
        activeMutex.WaitOne();

        activeWindows.Remove(window);

        activeMutex.ReleaseMutex();
    }

    private void RenderField3DVisualization(double delta) {
        if (Field.AccelerationStructure == null) {
            return;
        }

        // Set up 3D rendering matrices
        Context.UpdateViewMatrix();
        Context.UpdateProjectionMatrix();

        // Enable wireframe mode for field visualization
        bool originalWireframeMode = Context.WireframeMode;
        Context.WireframeMode = true;
        Context.SetWireframeRasterizer();

        // Always use wireframe shaders for field visualization (they don't need texture samplers)
        Context.WireframeVertexShader?.Bind();
        Context.WireframePixelShader?.Bind();

        // Render field entities using the DirectX 11 pipeline
        RenderFieldEntities3D();

        // Restore original wireframe mode
        Context.WireframeMode = originalWireframeMode;
        Context.SetSolidRasterizer();
    }

    private void RenderFieldEntities3D() {
        if (Field.AccelerationStructure == null) {
            return;
        }

        // Render box colliders
        if (showBoxColliders) {
            RenderBoxColliders();
        }

        // Render mesh colliders
        if (showMeshColliders) {
            RenderMeshColliders();
        }

        // Render spawn points
        if (showSpawnPoints) {
            RenderSpawnPoints();
        }

        // Render vibrate objects
        if (showVibrateObjects) {
            RenderVibrateObjects();
        }

        // Render portals
        if (showPortals) {
            RenderPortals();

            if (showPortalConnections) {
                RenderPortalConnectionLines();
            }

            // Render portal text labels
            if (showPortalInformation) {
                RenderPortalTextLabels();
            }
        }

        // Render actors (players, NPCs, mobs)
        if (showActors) {
            RenderActors();

            // Render selection highlight for selected actor
            if (selectedActor != null) {
                RenderActorSelectionHighlight();
            }

            // Render text labels above actors using ImGui
            RenderActorTextLabels();
        }
    }

    private void RenderBoxColliders() {
        // Set white color for box colliders
        Context.SetColor(1, 1, 1, 1); // White for box colliders

        List<FieldBoxColliderEntity> allBoxColliders = GetAllEntities().OfType<FieldBoxColliderEntity>().ToList();

        foreach (FieldBoxColliderEntity boxCollider in allBoxColliders) {
            Transform transform = new() {
                Position = boxCollider.Position,
                RotationAnglesDegrees = boxCollider.Rotation,
            };

            transform.Transformation *= MapRotation;
            Matrix4x4 worldMatrix = Matrix4x4.CreateScale(boxCollider.Size) *
                                    Matrix4x4.CreateTranslation(boxCollider.Position);

            Context.UpdateConstantBuffer(worldMatrix, true); // Use current color (white)
            Context.CoreModels!.WireCube.Draw();
        }
    }

    private void RenderMeshColliders() {
        IEnumerable<FieldMeshColliderEntity> allMeshColliders = GetAllEntities().OfType<FieldMeshColliderEntity>();

        foreach (FieldMeshColliderEntity meshCollider in allMeshColliders) {
            // For mesh colliders, render a simple bounding box for now
            Vector3 size = meshCollider.Bounds.Max - meshCollider.Bounds.Min;
            Vector3 center = (meshCollider.Bounds.Max + meshCollider.Bounds.Min) * 0.5f;

            // Convert Euler angles (degrees) to quaternion
            Vector3 rotationRadians = meshCollider.Rotation * (MathF.PI / 180.0f);
            var rotation = Quaternion.CreateFromYawPitchRoll(rotationRadians.Y, rotationRadians.X, rotationRadians.Z);

            // Apply coordinate system transformation (Y↔Z flip)
            var coordinateTransform = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float) (-Math.PI / 2));
            rotation = coordinateTransform * rotation;

            Matrix4x4 worldMatrix = Matrix4x4.CreateScale(size) *
                                    Matrix4x4.CreateTranslation(center);

            Context.UpdateConstantBuffer(worldMatrix);
            Context.CoreModels!.WireCube.Draw();
        }
    }

    private void RenderSpawnPoints() {
        IEnumerable<FieldSpawnTile> allSpawnPoints = GetAllEntities().OfType<FieldSpawnTile>();

        foreach (FieldSpawnTile spawnPoint in allSpawnPoints) {
            // Convert Euler angles (degrees) to quaternion
            Vector3 rotationRadians = spawnPoint.Rotation * (MathF.PI / 180.0f);
            var rotation = Quaternion.CreateFromYawPitchRoll(rotationRadians.Y, rotationRadians.X, rotationRadians.Z);

            Matrix4x4 worldMatrix = Matrix4x4.CreateScale(Vector3.One * 50.0f) *
                                    Matrix4x4.CreateTranslation(spawnPoint.Position);

            Context.UpdateConstantBuffer(worldMatrix);
            Context.CoreModels!.WireCube.Draw();
        }
    }

    private void RenderVibrateObjects() {
        // Set pink color for vibrate objects
        Context.SetColor(1, 0.75f, 0.8f, 1); // Pink for vibrate objects

        foreach (FieldVibrateEntity vibrateEntity in Field.AccelerationStructure!.VibrateEntities) {
            // Convert Euler angles (degrees) to quaternion
            Vector3 rotationRadians = vibrateEntity.Rotation * (MathF.PI / 180.0f);
            var rotation = Quaternion.CreateFromYawPitchRoll(rotationRadians.Y, rotationRadians.X, rotationRadians.Z);

            Matrix4x4 worldMatrix = Matrix4x4.CreateScale(Vector3.One * 100.0f) *
                                    Matrix4x4.CreateTranslation(vibrateEntity.Position);

            Context.UpdateConstantBuffer(worldMatrix, true); // Use current color (pink)
            Context.CoreModels!.WireCube.Draw();
        }
    }

    private void RenderPortals() {
        // Set blue color for portals
        Context.SetColor(0, 0.5f, 1, 1); // Blue for portals

        foreach (FieldPortal portal in Field.GetPortals()) {
            // Use portal dimensions if available, otherwise use default size
            Vector3 size = portal.Value.Dimension != Vector3.Zero ? portal.Value.Dimension : Vector3.One * 100.0f;

            Matrix4x4 worldMatrix = Matrix4x4.CreateScale(size) *
                                    Matrix4x4.CreateTranslation(portal.Position);

            Context.UpdateConstantBuffer(worldMatrix, true); // Use current color (blue)
            Context.CoreModels!.WireCube.Draw();
        }
    }

    private void RenderPortalConnectionLines() {
        // Set cyan color for portal connection lines
        Context.SetColor(0, 1, 1, 0.7f); // Cyan with some transparency

        List<FieldPortal> portals = Field.GetPortals().ToList();

        foreach (FieldPortal sourcePortal in portals) {
            // Only draw lines for portals that target the same field and have a specific target portal
            if (sourcePortal.Value.TargetMapId == Field.MapId && sourcePortal.Value.TargetPortalId > 0) {
                // Find the target portal in the same field
                FieldPortal? targetPortal = portals.FirstOrDefault(p => p.Value.Id == sourcePortal.Value.TargetPortalId);

                if (targetPortal != null) {
                    // Draw a line between the two portals
                    RenderLine(sourcePortal.Position, targetPortal.Position);
                }
            }
        }
    }

    private void RenderLine(Vector3 start, Vector3 end) {
        // Create a line by drawing a thin cylinder between two points
        Vector3 direction = end - start;
        float distance = direction.Length();

        if (distance < 0.1f) return; // Skip very short lines

        Vector3 center = (start + end) * 0.5f;
        Vector3 normalizedDirection = Vector3.Normalize(direction);

        // Calculate rotation to align cylinder with the line direction
        // The cylinder model is oriented along the Y axis by default
        Vector3 defaultUp = Vector3.UnitY;
        Quaternion rotation;

        // Check if direction is parallel to Y axis
        float dot = Vector3.Dot(defaultUp, normalizedDirection);
        if (Math.Abs(dot) > 0.999f) {
            // Direction is parallel to Y axis, no rotation needed (or 180 degrees)
            rotation = dot > 0 ? Quaternion.Identity : Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI);
        } else {
            // Calculate rotation between Y axis and direction
            Vector3 axis = Vector3.Normalize(Vector3.Cross(defaultUp, normalizedDirection));
            float angle = MathF.Acos(Math.Clamp(dot, -1.0f, 1.0f));
            rotation = Quaternion.CreateFromAxisAngle(axis, angle);
        }

        Matrix4x4 worldMatrix = Matrix4x4.CreateScale(new Vector3(10.0f, distance / 2.0f, 10.0f)) *
                                Matrix4x4.CreateFromQuaternion(rotation) *
                                Matrix4x4.CreateTranslation(center);

        Context.UpdateConstantBuffer(worldMatrix, true);
        Context.CoreModels!.Cylinder.Draw();
    }

    private void RenderActors() {
        const float agentRadius = AgentRadiusMeters * GameUnitsPerMeter;
        const float agentHeight = AgentHeightMeters * GameUnitsPerMeter;

        // Render players using cylinders (like Recast agents) - Cyan color
        if (showPlayers) {
            Context.SetColor(0, 1, 1, 1); // Cyan for players
            foreach ((int _, FieldPlayer player) in Field.Players) {
                // Rotate cylinder 90 degrees around X axis to make it stand upright (Y axis becomes Z axis)
                var rotation = Matrix4x4.CreateRotationX((float) (Math.PI / 2));
                Matrix4x4 worldMatrix = Matrix4x4.CreateScale(new Vector3(agentRadius, agentHeight, agentRadius)) *
                                        rotation *
                                        Matrix4x4.CreateTranslation(player.Position);

                Context.UpdateConstantBuffer(worldMatrix, true); // Use current color (cyan)
                Context.CoreModels!.Cylinder.Draw(); // Use cylinder for players (like Recast agents)
            }
        }

        // Render NPCs using cylinders (like Recast agents) - Green for alive, Gray for dead
        if (showNpcs) {
            foreach ((int _, FieldNpc npc) in Field.Npcs) {
                // Set color based on dead status
                if (npc.IsDead) {
                    Context.SetColor(0.5f, 0.5f, 0.5f, 0.3f); // Gray and semi-transparent for dead NPCs
                } else {
                    Context.SetColor(0, 1, 0, 1); // Green for alive NPCs
                }

                // Rotate cylinder 90 degrees around X axis to make it stand upright (Y axis becomes Z axis)
                var rotation = Matrix4x4.CreateRotationX((float) (Math.PI / 2));
                Matrix4x4 worldMatrix = Matrix4x4.CreateScale(new Vector3(agentRadius, agentHeight, agentRadius)) *
                                        rotation *
                                        Matrix4x4.CreateTranslation(npc.Position);

                Context.UpdateConstantBuffer(worldMatrix, true); // Use current color (green/gray)
                Context.CoreModels!.Cylinder.Draw(); // Use cylinder for NPCs (like Recast agents)
            }
        }

        // Render Mobs using cylinders (like Recast agents) - Red for aggressive, Yellow for wandering, Gray for dead
        if (showMobs) {
            foreach ((int _, FieldNpc mob) in Field.Mobs) {
                // Set color based on dead status and battle state
                if (mob.IsDead) {
                    Context.SetColor(0.5f, 0.5f, 0.5f, 0.3f); // Gray and semi-transparent for dead mobs
                } else {
                    // Check if mob is aggressive (in battle) or wandering
                    if (mob.BattleState.InBattle) {
                        Context.SetColor(1, 0, 0, 1); // Red for aggressive mobs
                    } else {
                        Context.SetColor(1, 1, 0, 1); // Yellow for wandering mobs
                    }
                }

                // Rotate cylinder 90 degrees around X axis to make it stand upright (Y axis becomes Z axis)
                var rotation = Matrix4x4.CreateRotationX((float) (Math.PI / 2));
                Matrix4x4 worldMatrix = Matrix4x4.CreateScale(new Vector3(agentRadius, agentHeight, agentRadius)) *
                                        rotation *
                                        Matrix4x4.CreateTranslation(mob.Position);

                Context.UpdateConstantBuffer(worldMatrix, true); // Use current color (red/yellow/gray)
                Context.CoreModels!.Cylinder.Draw(); // Use cylinder for mobs (like Recast agents)
            }
        }
    }

    private void RenderActorSelectionHighlight() {
        if (selectedActor == null) return;

        const float agentRadius = AgentRadiusMeters * GameUnitsPerMeter;
        const float agentHeight = AgentHeightMeters * GameUnitsPerMeter;

        // Rotate cylinder 90 degrees around X axis to make it stand upright (Y axis becomes Z axis)
        var rotation = Matrix4x4.CreateRotationX((float) (Math.PI / 2));

        // Render a second, even larger outline with pulsing effect
        Context.SetColor(1, 1, 0, 1); // Yellow with pulsing alpha
        float pulseRadius = agentRadius * 1.4f;
        float pulseHeight = agentHeight * 1.2f;

        Matrix4x4 pulseWorldMatrix = Matrix4x4.CreateScale(new Vector3(pulseRadius, pulseHeight, pulseRadius)) *
                                     rotation *
                                     Matrix4x4.CreateTranslation(selectedActor.Position);

        Context.UpdateConstantBuffer(pulseWorldMatrix, true);
        Context.CoreModels!.Cylinder.Draw();
    }

    private void RenderVisualizationControls() {
        if (ImGui.Begin("3D Visualization Controls")) {
            ImGui.Text("Field Visualization Settings");
            ImGui.Separator();

            ImGui.Checkbox("Show Box Colliders", ref showBoxColliders);
            // ImGui.Checkbox("Show Mesh Colliders", ref showMeshColliders);
            // ImGui.Checkbox("Show Spawn Points", ref showSpawnPoints);
            ImGui.Checkbox("Show Vibrate Objects", ref showVibrateObjects);
            ImGui.Checkbox("Show Portals", ref showPortals);

            // Portal sub-options (only show when portals are enabled)
            if (showPortals) {
                ImGui.Indent();
                ImGui.Checkbox("Show Portal Information", ref showPortalInformation);
                ImGui.Checkbox("Show Portal Connections", ref showPortalConnections);
                ImGui.Unindent();
            }

            ImGui.Checkbox("Show Actors", ref showActors);

            // Actor sub-options (only show when actors are enabled)
            if (showActors) {
                ImGui.Indent();
                ImGui.Checkbox("Show Players", ref showPlayers);
                ImGui.Checkbox("Show NPCs", ref showNpcs);
                ImGui.Checkbox("Show Mobs", ref showMobs);
                ImGui.Unindent();
            }

            ImGui.Separator();

            // Camera controls
            ImGui.Text("Camera Controls:");
            if (ImGui.Button("Reset Camera")) {
                // Reset camera to default rotation for MapleStory 2
                Context.CameraController.SetDefaultRotation();
            }

            ImGui.SameLine();
            if (ImGui.Button("Toggle Wireframe")) {
                Context.ToggleWireframeMode();
            }

            if (ImGui.Button("View Field Overview")) {
                SetFieldOverviewCamera();
            }

            // Player follow controls
            ImGui.Separator();
            ImGui.Text($"Player Follow: {(Context.CameraController.IsFollowingPlayer ? $"Player ID: {Context.CameraController.FollowedPlayerId}" : string.Empty)}");

            if (Context.CameraController.IsFollowingPlayer) {
                if (ImGui.Button("Stop Following")) {
                    Context.StopFollowingPlayer();
                }
            } else {
                if (ImGui.Button("Follow First Player")) {
                    FieldPlayer? firstPlayer = Field.Players.Values.FirstOrDefault();
                    if (firstPlayer != null) {
                        Context.StartFollowingPlayer(firstPlayer.Value.Character.Id);
                    }
                }
            }

            if (Context.CameraController.IsFollowingPlayer) {
                ImGui.Text($"Player Following Position: {Context.CameraController.CameraTarget}");
            }
            // Camera position display
            ImGui.Text($"Camera Position: {Context.CameraController.CameraPosition}");

            // Camera rotation display
            Quaternion q = Context.CameraController.CameraRotation;
            ImGui.Text($"Camera Rotation: X={q.X:F3}, Y={q.Y:F3}, Z={q.Z:F3}, W={q.W:F3}");

            ImGui.Text($"Wireframe Mode: {(Context.WireframeMode ? "ON" : "OFF")}");
        }
        ImGui.End();
    }

    private void SetFieldOverviewCamera() {
        // Stop following player when switching to field overview
        if (Context.CameraController.IsFollowingPlayer) {
            Context.StopFollowingPlayer();
        }

        List<FieldBoxColliderEntity> allColliders = GetAllEntities().OfType<FieldBoxColliderEntity>().ToList();
        if (allColliders.Count > 0) {
            // Calculate field bounds in map coordinates
            Vector3 min = allColliders[0].Position;
            Vector3 max = allColliders[0].Position;

            foreach (FieldBoxColliderEntity collider in allColliders) {
                min = Vector3.Min(min, collider.Position - collider.Size * 0.5f);
                max = Vector3.Max(max, collider.Position + collider.Size * 0.5f);
            }

            // Transform to visualization coordinates
            Vector3 centerMap = (min + max) * 0.5f;
            Vector3 sizeMap = max - min;

            Vector3 center = Vector3.Transform(centerMap, MapRotation);
            float distance = Math.Max(sizeMap.X, Math.Max(sizeMap.Y, sizeMap.Z)) * 1.5f;

            Context.CameraController.SetCameraTarget(center);
            Context.CameraController.Camera.Transform.Position = center + new Vector3(0, -distance * 0.7f, distance * 0.7f);
        } else {
            // Fallback if no colliders found
            Context.CameraController.Camera.Transform.Position = new Vector3(0, -1000, 1000);
            Context.CameraController.SetCameraTarget(Vector3.Zero);
        }
        Context.CameraController.SetFieldOverviewRotation(); // Use field overview rotation
    }

    private void UpdateCameraFollow() {
        if (!Context.CameraController.IsFollowingPlayer || Context.CameraController.FollowedPlayerId == null) {
            return;
        }

        // Find the player being followed
        FieldPlayer? followedPlayer = Field.Players.Values.FirstOrDefault(p => p.Value.Character.Id == Context.CameraController.FollowedPlayerId);
        if (followedPlayer == null) {
            // Player not found, stop following
            Context.StopFollowingPlayer();
            return;
        }

        // Update camera to follow player position using the camera controller
        Vector3 playerPosition = followedPlayer.Position;
        Context.UpdatePlayerFollow(playerPosition);
    }

    private Vector2D<int> GetFieldWindowSize() {
        // Get the size from the first active field window, or use default
        activeMutex.WaitOne();
        try {
            DebugFieldWindow? firstWindow = activeWindows.FirstOrDefault();
            if (firstWindow?.DebuggerWindow != null) {
                return firstWindow.DebuggerWindow.FramebufferSize;
            }
        } finally {
            activeMutex.ReleaseMutex();
        }

        // Fallback to default field window size
        return DebugGraphicsContext.DefaultFieldWindowSize;
    }

    private bool TryWorldToScreen(Vector3 worldPos, out Vector2 screenPos) {
        screenPos = Vector2.Zero;

        // Transform world position to screen coordinates
        Matrix4x4 viewMatrix = Context.CameraController.Camera.ViewMatrix;
        Matrix4x4 projMatrix = Context.CameraController.Camera.ProjectionMatrix;
        Matrix4x4 viewProjMatrix = viewMatrix * projMatrix;

        // Transform to clip space
        Vector4 clipPos = Vector4.Transform(new Vector4(worldPos, 1.0f), viewProjMatrix);

        // Check if behind camera
        if (clipPos.W <= 0) return false;

        // Perspective divide to get NDC coordinates
        Vector3 ndcPos = new Vector3(clipPos.X, clipPos.Y, clipPos.Z) / clipPos.W;

        // Check if outside screen bounds (with some tolerance)
        if (ndcPos.X < -1.2f || ndcPos.X > 1.2f || ndcPos.Y < -1.2f || ndcPos.Y > 1.2f) return false;

        // Get the actual viewport size from the field window (not main window)
        Vector2D<int> windowSize = GetFieldWindowSize();

        // Convert NDC to screen coordinates
        // NDC: (-1, -1) = bottom-left, (1, 1) = top-right
        // Screen: (0, 0) = top-left, (width, height) = bottom-right
        float screenX = (ndcPos.X + 1.0f) * 0.5f * windowSize.X;
        float screenY = (1.0f - ndcPos.Y) * 0.5f * windowSize.Y; // Flip Y axis for screen coordinates

        screenPos = new Vector2(screenX, screenY);
        return true;
    }

    private bool TryScreenToWorldRay(Vector2 screenPos, out Vector3 rayOrigin, out Vector3 rayDirection) {
        rayOrigin = Vector3.Zero;
        rayDirection = Vector3.Zero;

        // Get the actual viewport size from the field window
        Vector2D<int> windowSize = GetFieldWindowSize();

        // Convert screen coordinates to NDC coordinates
        // Screen: (0, 0) = top-left, (width, height) = bottom-right
        // NDC: (-1, -1) = bottom-left, (1, 1) = top-right
        float ndcX = (screenPos.X / windowSize.X) * 2.0f - 1.0f;
        float ndcY = 1.0f - (screenPos.Y / windowSize.Y) * 2.0f; // Flip Y axis

        // Get camera matrices
        Matrix4x4 viewMatrix = Context.CameraController.Camera.ViewMatrix;
        Matrix4x4 projMatrix = Context.CameraController.Camera.ProjectionMatrix;

        // Calculate inverse view-projection matrix
        Matrix4x4 viewProjMatrix = viewMatrix * projMatrix;
        if (!Matrix4x4.Invert(viewProjMatrix, out Matrix4x4 invViewProjMatrix)) {
            return false; // Matrix is not invertible
        }

        // Create two points: one on near plane, one on far plane
        Vector4 nearPoint = new Vector4(ndcX, ndcY, -1.0f, 1.0f); // Near plane in NDC
        Vector4 farPoint = new Vector4(ndcX, ndcY, 1.0f, 1.0f); // Far plane in NDC

        // Transform to world space
        Vector4 nearWorld = Vector4.Transform(nearPoint, invViewProjMatrix);
        Vector4 farWorld = Vector4.Transform(farPoint, invViewProjMatrix);

        // Perspective divide
        if (nearWorld.W == 0 || farWorld.W == 0) return false;
        nearWorld /= nearWorld.W;
        farWorld /= farWorld.W;

        // Calculate ray
        rayOrigin = new Vector3(nearWorld.X, nearWorld.Y, nearWorld.Z);
        Vector3 farWorldPos = new Vector3(farWorld.X, farWorld.Y, farWorld.Z);
        rayDirection = Vector3.Normalize(farWorldPos - rayOrigin);

        return true;
    }

    private bool TrySelectActorAtScreenPosition(Vector2 screenPos) {
        if (!TryScreenToWorldRay(screenPos, out Vector3 rayOrigin, out Vector3 rayDirection)) {
            return false;
        }

        IActor? closestActor = null;
        float closestDistance = float.MaxValue;

        // Test players
        if (showPlayers) {
            foreach ((int _, FieldPlayer player) in Field.Players) {
                if (TryRayPrismIntersection(rayOrigin, rayDirection, player.Shape, out float distance)) {
                    if (distance < closestDistance) {
                        closestDistance = distance;
                        closestActor = player;
                    }
                }
            }
        }

        // Test NPCs
        if (showNpcs) {
            foreach ((int _, FieldNpc npc) in Field.Npcs) {
                if (TryRayPrismIntersection(rayOrigin, rayDirection, npc.Shape, out float distance)) {
                    if (distance < closestDistance) {
                        closestDistance = distance;
                        closestActor = npc;
                    }
                }
            }
        }

        // Test Mobs
        if (showMobs) {
            foreach ((int _, FieldNpc mob) in Field.Mobs) {
                if (TryRayPrismIntersection(rayOrigin, rayDirection, mob.Shape, out float distance)) {
                    if (distance < closestDistance) {
                        closestDistance = distance;
                        closestActor = mob;
                    }
                }
            }
        }

        if (closestActor != null) {
            selectedActor = closestActor;
            return true;
        }

        return false;
    }

    private bool TryRayPrismIntersection(Vector3 rayOrigin, Vector3 rayDirection, IPrism prism, out float distance) {
        distance = 0;

        // Sample points along the ray to test for intersection with the prism
        // This is a simple approach - for more accuracy, you could implement proper ray-prism intersection
        const float stepSize = 10.0f; // Step size in world units
        const float maxDistance = 10000.0f; // Maximum ray distance to test

        for (float t = 0; t < maxDistance; t += stepSize) {
            Vector3 testPoint = rayOrigin + t * rayDirection;

            if (prism.Contains(testPoint)) {
                distance = t;
                return true;
            }
        }

        return false;
    }

    private void TryMovePlayerToScreenPosition(FieldPlayer player, Vector2 screenPos) {
        if (!TryScreenToWorldRay(screenPos, out Vector3 rayOrigin, out Vector3 rayDirection)) {
            return;
        }

        // Cast ray down to find ground intersection
        Vector3? targetPosition = FindGroundIntersection(rayOrigin, rayDirection);

        if (targetPosition.HasValue) {
            Vector3 newPosition = targetPosition.Value;
            Vector3 currentRotation = player.Rotation; // Keep current rotation

            if (forceMove) {
                // Force move: bypass validation and send packet directly
                player.Session.Send(PortalPacket.MoveByPortal(player, newPosition, currentRotation));
            } else {
                // Normal move: use the built-in function with validation
                player.MoveToPosition(newPosition, currentRotation);
            }
        }
    }

    private Vector3? FindGroundIntersection(Vector3 rayOrigin, Vector3 rayDirection) {
        // First, get the X,Y position where the ray would hit a horizontal plane
        Vector2 targetXy = GetRayGroundProjection(rayOrigin, rayDirection);

        // Find the topmost block at this X,Y position
        float? topBlockZ = FindTopmostBlockAt(targetXy);

        if (topBlockZ.HasValue) {
            // Place player on top of the highest block (add some height for the player)
            return new Vector3(targetXy.X, targetXy.Y, topBlockZ.Value + 100.0f); // 50 units above block
        }
        // No blocks found - drop player from way above for natural falling
        const float dropHeight = 2000.0f; // Drop from 2000 units above
        return new Vector3(targetXy.X, targetXy.Y, dropHeight);
    }

    private Vector2 GetRayGroundProjection(Vector3 rayOrigin, Vector3 rayDirection) {
        // Project the ray to a reasonable ground level to get X,Y coordinates
        float targetZ = selectedActor?.Position.Z ?? 0.0f;

        if (Math.Abs(rayDirection.Z) > 0.001f) {
            float t = (targetZ - rayOrigin.Z) / rayDirection.Z;
            if (t > 0) {
                Vector3 intersection = rayOrigin + t * rayDirection;
                return new Vector2(intersection.X, intersection.Y);
            }
        }

        // Fallback: project ray forward a reasonable distance
        Vector3 projected = rayOrigin + rayDirection * 1000.0f;
        return new Vector2(projected.X, projected.Y);
    }

    private float? FindTopmostBlockAt(Vector2 position) {
        if (Field.AccelerationStructure == null) {
            return null;
        }

        float? highestZ = null;

        // Query box colliders (main map blocks) at this position
        var boxColliders = GetAllEntities().OfType<FieldBoxColliderEntity>();

        foreach (FieldBoxColliderEntity collider in boxColliders) {
            // Check if this collider contains the X,Y position
            if (IsPositionInBoxCollider(position, collider)) {
                // Calculate the top of this collider
                float colliderTop = collider.Position.Z + (collider.Size.Z * 0.5f);

                if (!highestZ.HasValue || colliderTop > highestZ.Value) {
                    highestZ = colliderTop;
                }
            }
        }

        return highestZ;
    }

    private bool IsPositionInBoxCollider(Vector2 position, FieldBoxColliderEntity collider) {
        // Simple AABB check for now - could be enhanced for rotated boxes
        Vector3 min = collider.Position - (collider.Size * 0.5f);
        Vector3 max = collider.Position + (collider.Size * 0.5f);

        return position.X >= min.X && position.X <= max.X &&
               position.Y >= min.Y && position.Y <= max.Y;
    }

    private void RenderActorTextLabels() {
        ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();

        // Render player names using ImGui
        if (showPlayers) {
            foreach ((int _, FieldPlayer player) in Field.Players) {
                Vector3 textPos = player.Position + new Vector3(0, 0, 200); // Above the cylinder
                if (!TryWorldToScreen(textPos, out Vector2 screenPos)) continue;

                string playerName = player.Value.Character.Name;
                bool isSelected = selectedActor == player;

                Vector2 textSize = ImGui.CalcTextSize(playerName);
                Vector4 textColor = isSelected ? new Vector4(1, 1, 0, 1) : new Vector4(0, 1, 1, 1); // Yellow if selected, cyan otherwise
                Vector4 bgColor = isSelected ? new Vector4(0.2f, 0.2f, 0, 0.9f) : new Vector4(0, 0, 0, 0.7f); // Darker yellow bg if selected
                Vector4 borderColor = isSelected ? new Vector4(1, 1, 0, 1) : new Vector4(0.2f, 0.2f, 0.2f, 0.8f); // Yellow border if selected

                // Draw background
                Vector2 bgMin = screenPos - new Vector2(4, 2);
                Vector2 bgMax = screenPos + textSize + new Vector2(4, 2);
                drawList.AddRectFilled(bgMin, bgMax, ImGui.ColorConvertFloat4ToU32(bgColor));
                drawList.AddRect(bgMin, bgMax, ImGui.ColorConvertFloat4ToU32(borderColor));

                // Draw text on top
                drawList.AddText(screenPos, ImGui.ColorConvertFloat4ToU32(textColor), playerName);
            }
        }

        // Render NPC names ImGui
        if (showNpcs) {
            foreach ((int _, FieldNpc npc) in Field.Npcs) {
                Vector3 textPos = npc.Position + new Vector3(0, 0, 200); // Above the cylinder
                if (!TryWorldToScreen(textPos, out Vector2 screenPos)) continue;

                string npcName = npc.Value.Metadata.Name ?? $"NPC {npc.Value.Metadata.Id}";
                bool isSelected = selectedActor == npc;

                Vector4 textColor = isSelected ? new Vector4(1, 1, 0, 1) : new Vector4(0, 1, 0, 1); // Yellow if selected, green otherwise
                Vector4 bgColor = isSelected ? new Vector4(0.2f, 0.2f, 0, 0.9f) : new Vector4(0, 0, 0, 0.7f); // Darker yellow bg if selected
                Vector4 borderColor = isSelected ? new Vector4(1, 1, 0, 1) : new Vector4(0.2f, 0.2f, 0.2f, 0.8f); // Yellow border if selected
                uint color = ImGui.ColorConvertFloat4ToU32(textColor);

                // Draw background for NPC name
                Vector2 nameSize = ImGui.CalcTextSize(npcName);
                Vector2 nameBgMin = screenPos - new Vector2(4, 2);
                Vector2 nameBgMax = screenPos + nameSize + new Vector2(4, 2);
                drawList.AddRectFilled(nameBgMin, nameBgMax, ImGui.ColorConvertFloat4ToU32(bgColor));
                drawList.AddRect(nameBgMin, nameBgMax, ImGui.ColorConvertFloat4ToU32(borderColor));

                drawList.AddText(screenPos, color, npcName);
            }
        }

        // Render Mob names and HP using ImGui
        if (showMobs) {
            foreach ((int _, FieldNpc mob) in Field.Mobs) {
                Vector3 textPos = mob.Position + new Vector3(0, 0, 200); // Above the cylinder
                if (!TryWorldToScreen(textPos, out Vector2 screenPos)) continue;

                string mobName = mob.Value.Metadata.Name ?? $"Mob {mob.Value.Metadata.Id}";
                bool isSelected = selectedActor == mob;

                // Calculate HP percentage
                long currentHp = mob.Stats.Values[BasicAttribute.Health].Current;
                long maxHp = mob.Stats.Values[BasicAttribute.Health].Total;
                float hpPercent = maxHp > 0 ? (float) currentHp / maxHp * 100f : 0f;

                // Choose color based on status, but override with yellow if selected
                Vector4 textColor;
                Vector4 bgColor;
                Vector4 borderColor;

                if (isSelected) {
                    textColor = new Vector4(1, 1, 0, 1); // Yellow for selected
                    bgColor = new Vector4(0.2f, 0.2f, 0, 0.9f); // Darker yellow bg
                    borderColor = new Vector4(1, 1, 0, 1); // Yellow border
                } else {
                    // Normal colors based on mob status
                    if (mob.IsDead) {
                        textColor = new Vector4(0.5f, 0.5f, 0.5f, 0.7f); // Gray for dead
                    } else if (mob.BattleState.InBattle) {
                        textColor = new Vector4(1, 0, 0, 1); // Red for aggressive
                    } else {
                        textColor = new Vector4(1, 1, 0, 1); // Yellow for wandering
                    }
                    bgColor = new Vector4(0, 0, 0, 0.7f); // Normal dark background
                    borderColor = new Vector4(0.2f, 0.2f, 0.2f, 0.8f); // Normal border
                }

                uint color = ImGui.ColorConvertFloat4ToU32(textColor);

                // Draw background for mob name
                Vector2 nameSize = ImGui.CalcTextSize(mobName);
                Vector2 nameBgMin = screenPos - new Vector2(4, 2);
                Vector2 nameBgMax = screenPos + nameSize + new Vector2(4, 2);
                drawList.AddRectFilled(nameBgMin, nameBgMax, ImGui.ColorConvertFloat4ToU32(bgColor));
                drawList.AddRect(nameBgMin, nameBgMax, ImGui.ColorConvertFloat4ToU32(borderColor));

                drawList.AddText(screenPos, color, mobName);

                // Position HP text below the name using screen-space offset to avoid overlap when zoomed out
                // Calculate HP text position in screen space (below the name) with consistent pixel spacing
                Vector2 hpScreenPos = new Vector2(screenPos.X, screenPos.Y + nameSize.Y + 4); // 4 pixels below name

                string hpText = $"HP: {hpPercent:F1}%";
                Vector2 hpSize = ImGui.CalcTextSize(hpText);
                Vector2 hpBgMin = hpScreenPos - new Vector2(4, 2);
                Vector2 hpBgMax = hpScreenPos + hpSize + new Vector2(4, 2);
                drawList.AddRectFilled(hpBgMin, hpBgMax, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0.7f)));
                drawList.AddRect(hpBgMin, hpBgMax, ImGui.ColorConvertFloat4ToU32(new Vector4(0.2f, 0.2f, 0.2f, 0.8f)));
                drawList.AddText(hpScreenPos, color, hpText);
            }
        }
    }

    private void RenderPortalTextLabels() {
        ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();

        // Render portal information using ImGui
        foreach (FieldPortal portal in Field.GetPortals()) {
            Vector3 textPos = portal.Position + new Vector3(0, 0, 150); // Above the portal cube
            if (!TryWorldToScreen(textPos, out Vector2 screenPos)) continue;

            // Build portal info text
            string enabledText = $"{(portal.Enabled ? "Enabled" : "Disabled")} | {(portal.Visible ? "Visible" : "Hidden")}";
            string idText = $"ID: {portal.Value.Id}";
            string targetText = $"Target: {portal.Value.TargetMapId}";
            string targetPortalText = $"Target Portal: {portal.Value.TargetPortalId}";
            string typeText = $"Type: {portal.Value.Type}";

            // Choose color based on enabled status
            Vector4 textColor = portal.Enabled ? new Vector4(0, 0.8f, 1, 1) : new Vector4(0.6f, 0.6f, 0.6f, 1); // Blue if enabled, gray if disabled
            uint color = ImGui.ColorConvertFloat4ToU32(textColor);

            // Calculate total text size for background
            Vector2 enabledSize = ImGui.CalcTextSize(enabledText);
            Vector2 idSize = ImGui.CalcTextSize(idText);
            Vector2 targetSize = ImGui.CalcTextSize(targetText);
            Vector2 targetPortalSize = ImGui.CalcTextSize(targetPortalText);
            Vector2 typeSize = ImGui.CalcTextSize(typeText);

            float maxWidth = Math.Max(Math.Max(Math.Max(enabledSize.X, idSize.X), Math.Max(targetSize.X, targetPortalSize.X)), typeSize.X);
            float totalHeight = enabledSize.Y + idSize.Y + targetSize.Y + targetPortalSize.Y + typeSize.Y + 8; // 8 pixels spacing between lines

            // Draw background
            Vector2 bgMin = screenPos - new Vector2(4, 2);
            Vector2 bgMax = screenPos + new Vector2(maxWidth + 8, totalHeight + 4);
            drawList.AddRectFilled(bgMin, bgMax, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0.8f))); // Dark background
            drawList.AddRect(bgMin, bgMax, ImGui.ColorConvertFloat4ToU32(new Vector4(0.3f, 0.3f, 0.3f, 0.9f))); // Border

            // Draw text lines
            Vector2 currentPos = screenPos;
            drawList.AddText(currentPos, color, enabledText);
            currentPos.Y += enabledSize.Y + 1;
            drawList.AddText(currentPos, color, idText);
            currentPos.Y += idSize.Y + 1;
            drawList.AddText(currentPos, color, targetText);
            currentPos.Y += targetSize.Y + 1;
            drawList.AddText(currentPos, color, targetPortalText);
            currentPos.Y += targetPortalSize.Y + 1;
            drawList.AddText(currentPos, color, typeText);
        }
    }

    private FieldEntity[] GetAllEntities() {
        // Cache static entities since they never change during field runtime
        if (cachedStaticEntities != null) {
            return cachedStaticEntities;
        }

        if (Field.AccelerationStructure == null) {
            cachedStaticEntities = [];
            return cachedStaticEntities;
        }

        var allEntities = new List<FieldEntity>();

        // Add all aligned entities
        allEntities.AddRange(Field.AccelerationStructure.AlignedEntities.ToArray());

        // Add all aligned trimmed entities
        allEntities.AddRange(Field.AccelerationStructure.AlignedTrimmedEntities.ToArray());

        // Add all unaligned entities
        allEntities.AddRange(Field.AccelerationStructure.UnalignedEntities.ToArray());

        // Add vibrate entities (they are a special type)
        allEntities.AddRange(Field.AccelerationStructure.VibrateEntities.ToArray());

        // Cache the result for future calls
        cachedStaticEntities = allEntities.ToArray();
        return cachedStaticEntities;
    }
}

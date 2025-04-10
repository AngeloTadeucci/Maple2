using Maple2.Server.Game.Session;
using System.CommandLine.Invocation;
using System.CommandLine;
using Maple2.Database.Storage;
using System.CommandLine.IO;
using Maple2.Server.Game.Util;
using Maple2.Model.Metadata;
using Maple2.Database.Storage.Metadata;
using Maple2.Model.Game.Field;
using System.Numerics;
using Maple2.Model.Common;
using System;
using Maple2.Model.Enum;
using Maple2.Server.Game.Model;

namespace Maple2.Server.Game.Commands;

public class DebugCommand : GameCommand {
    private const string NAME = "debug";
    private const string DESCRIPTION = "Debug information management.";
    public const AdminPermissions RequiredPermission = AdminPermissions.Debug;

    private readonly NpcMetadataStorage npcStorage;

    public DebugCommand(GameSession session, NpcMetadataStorage npcStorage, MapDataStorage mapDataStorage) : base(RequiredPermission, NAME, DESCRIPTION) {
        this.npcStorage = npcStorage;

        AddCommand(new DebugNpcAiCommand(session, npcStorage));
        AddCommand(new DebugAnimationCommand(session));
        AddCommand(new DebugSkillsCommand(session));
        AddCommand(new SendRawPacketCommand(session));
        AddCommand(new ResolvePacketCommand(session));
        AddCommand(new DebugQueryCommand(session, mapDataStorage));
        AddCommand(new LogoutCommand(session));
        AddCommand(new ReloadCommandsCommand(session));
    }

    private class ReloadCommandsCommand : Command {
        private readonly GameSession session;

        public ReloadCommandsCommand(GameSession session) : base("reloadcommands", "Reloads all registered commands.") {
            this.session = session;
            this.SetHandler<InvocationContext>(Handle);
        }

        private void Handle(InvocationContext ctx) {
            try {
                session.CommandHandler.RegisterCommands();

                ctx.Console.Out.WriteLine("Commands reloaded successfully.");
            } catch (Exception e) {
                ctx.Console.Error.WriteLine($"Failed to reload commands: {e.Message}");
            }
        }
    }

    private class DebugNpcAiCommand : Command {
        private readonly GameSession session;
        private readonly NpcMetadataStorage npcStorage;

        public DebugNpcAiCommand(GameSession session, NpcMetadataStorage npcStorage) : base("ai", "Toggles displaying npc AI debug info.") {
            this.session = session;
            this.npcStorage = npcStorage;

            var enable = new Argument<bool?>("enable", () => true, "Enables & disables debug messages. Prints all AI state if true.");

            AddArgument(enable);

            this.SetHandler<InvocationContext, bool?>(Handle, enable);
        }

        private void Handle(InvocationContext ctx, bool? enabled) {
            if (session.Field == null) {
                ctx.Console.Error.WriteLine("No field loaded.");
                return;
            }

            session.Player.DebugAi = enabled ?? true;

            string message = enabled ?? true ? "Enabled" : "Disabled";
            ctx.Console.Out.WriteLine($"{message} AI debug info printing");
        }
    }


    private class DebugAnimationCommand : Command {
        private readonly GameSession session;

        public DebugAnimationCommand(GameSession session) : base("anims", "Prints player animation info.") {
            this.session = session;

            var enable = new Argument<bool?>("enable", () => true, "Enables & disables debug messages. Prints all animation state if true.");

            AddArgument(enable);

            this.SetHandler<InvocationContext, bool?>(Handle, enable);
        }

        private void Handle(InvocationContext ctx, bool? enabled) {
            session.Player.Animation.DebugPrintAnimations = enabled ?? true;

            string message = enabled ?? true ? "Enabled" : "Disabled";
            ctx.Console.Out.WriteLine($"{message} animation debug info printing");
        }
    }

    private class DebugSkillsCommand : Command {
        private readonly GameSession session;

        public DebugSkillsCommand(GameSession session) : base("skills", "Prints player skill packet info.") {
            this.session = session;

            var enable = new Argument<bool?>("enable", () => true, "Enables & disables debug messages. Prints all skill cast packets if true.");

            AddArgument(enable);

            this.SetHandler<InvocationContext, bool?>(Handle, enable);
        }

        private void Handle(InvocationContext ctx, bool? enabled) {
            session.Player.DebugSkills = enabled ?? true;

            string message = enabled ?? true ? "Enabled" : "Disabled";
            ctx.Console.Out.WriteLine($"{message} skill cast packet debug info printing");
        }
    }

    private class SendRawPacketCommand : Command {
        private readonly GameSession session;

        public SendRawPacketCommand(GameSession session) : base("packet", "Sends a raw packet to the server.") {
            this.session = session;

            var packet = new Argument<string[]>("packet", "The raw packet to send.");

            AddArgument(packet);

            this.SetHandler<InvocationContext, string[]>(Handle, packet);
        }

        private void Handle(InvocationContext ctx, string[] packet) {
            // Ensure packet is a valid hex string
            if (packet.Any(x => x.Length % 2 != 0)) {
                ctx.Console.Error.WriteLine("Invalid packet. Each byte must be represented by 2 characters.");
                return;
            }

            // If packet is less than 4 bytes, it's invalid
            if (packet.Length < 4) {
                ctx.Console.Error.WriteLine("Invalid packet. Must be at least 4 bytes (8 characters).");
                return;
            }


            byte[] bytes = packet.Select(x => byte.Parse(x, System.Globalization.NumberStyles.HexNumber)).ToArray();

            session.Send(bytes);
        }
    }

    private class ResolvePacketCommand : Command {
        private readonly GameSession session;

        public ResolvePacketCommand(GameSession session) : base("resolve", "Try to resolve packet") {
            this.session = session;

            var packet = new Argument<string>("opcode", "The packet opcode to try resolve.");

            AddArgument(packet);

            this.SetHandler<InvocationContext, string>(Handle, packet);
        }

        private void Handle(InvocationContext ctx, string packet) {
            PacketStructureResolver? resolver = PacketStructureResolver.Parse(packet);
            if (resolver == null) {
                ctx.Console.Error.WriteLine("Failed to resolve packet. Possible ways to use the opcode: 81 0081 0x81 0x0081");
                return;
            }

            resolver.Start(session);
        }
    }

    private class DebugQueryCommand : Command {
        public DebugQueryCommand(GameSession session, MapDataStorage mapDataStorage) : base("query", "Tests entity spatial queries.") {
            AddCommand(new DebugQuerySpawnCommand(session, mapDataStorage));
            AddCommand(new DebugQueryFluidCommand(session, mapDataStorage));
            AddCommand(new DebugQueryVibrateCommand(session, mapDataStorage));
            AddCommand(new DebugQuerySellableTileCommand(session, mapDataStorage));
        }

        private class DebugQuerySpawnCommand : Command {
            private readonly GameSession session;
            private readonly MapDataStorage mapDataStorage;

            public DebugQuerySpawnCommand(GameSession session, MapDataStorage mapDataStorage) : base("spawns", "Searches for nearby valid mob spawn points.") {
                this.session = session;
                this.mapDataStorage = mapDataStorage;

                var radius = new Argument<float>("radius", () => 300, "Sphere radius of query.");

                AddArgument(radius);

                this.SetHandler<InvocationContext, float>(Handle, radius);
            }

            private void Handle(InvocationContext ctx, float radius) {
                if (!session.Field.MapMetadata.TryGet(session.Field.MapId, out MapMetadata? map)) {
                    return;
                }

                if (!mapDataStorage.TryGet(map.XBlock, out FieldAccelerationStructure? mapData)) {
                    return;
                }

                Vector3 center = session.Player.Position;
                Vector3S cell = FieldAccelerationStructure.PointToCell(center);

                ctx.Console.Out.WriteLine($"Player at {center.X} {center.Y} {center.Z} in cell {cell.X} {cell.Y} {cell.Z}");

                mapData.QuerySpawns(session.Player.Position, radius, (spawn) => {
                    Vector3 center = spawn.Position;
                    Vector3S cell = FieldAccelerationStructure.PointToCell(center);

                    ctx.Console.Out.WriteLine($"Mob spawn found at {center.X} {center.Y} {center.Z} in cell {cell.X} {cell.Y} {cell.Z}");
                });
            }
        }

        private class DebugQueryFluidCommand : Command {
            private readonly GameSession session;
            private readonly MapDataStorage mapDataStorage;

            public DebugQueryFluidCommand(GameSession session, MapDataStorage mapDataStorage) : base("fluids", "Searches for nearby fluids.") {
                this.session = session;
                this.mapDataStorage = mapDataStorage;

                var x = new Argument<float>("x", () => 300, "How far along the x axis to search from the player.");
                var y = new Argument<float>("y", () => 300, "How far along the y axis to search from the player.");
                var z = new Argument<float>("z", () => 300, "How far along the z axis to search from the player.");

                AddArgument(x);
                AddArgument(y);
                AddArgument(z);

                this.SetHandler<InvocationContext, float, float, float>(Handle, x, y, z);
            }

            private void Handle(InvocationContext ctx, float x, float y, float z) {
                if (!session.Field.MapMetadata.TryGet(session.Field.MapId, out MapMetadata? map)) {
                    return;
                }

                if (!mapDataStorage.TryGet(map.XBlock, out FieldAccelerationStructure? mapData)) {
                    return;
                }

                Vector3 center = session.Player.Position;
                Vector3S cell = FieldAccelerationStructure.PointToCell(center);

                ctx.Console.Out.WriteLine($"Player at {center.X} {center.Y} {center.Z} in cell {cell.X} {cell.Y} {cell.Z}");

                mapData.QueryFluidsCenter(session.Player.Position, 2 * new Vector3(x, y, z), (fluid) => {
                    Vector3 center = fluid.Position;
                    Vector3S cell = FieldAccelerationStructure.PointToCell(center);
                    string fluidType = !fluid.IsSurface ? "Deep fluid" : fluid.IsShallow ? "Shallow fluid" : "Fluid";

                    ctx.Console.Out.WriteLine($"{fluidType} found at {center.X} {center.Y} {center.Z} in cell {cell.X} {cell.Y} {cell.Z}");
                });
            }
        }

        private class DebugQueryVibrateCommand : Command {
            private readonly GameSession session;
            private readonly MapDataStorage mapDataStorage;

            public DebugQueryVibrateCommand(GameSession session, MapDataStorage mapDataStorage) : base("vibrate", "Searches for nearby vibrate objects.") {
                this.session = session;
                this.mapDataStorage = mapDataStorage;

                var x = new Argument<float>("x", () => 300, "How far along the x axis to search from the player.");
                var y = new Argument<float>("y", () => 300, "How far along the y axis to search from the player.");
                var z = new Argument<float>("z", () => 300, "How far along the z axis to search from the player.");

                AddArgument(x);
                AddArgument(y);
                AddArgument(z);

                this.SetHandler<InvocationContext, float, float, float>(Handle, x, y, z);
            }

            private void Handle(InvocationContext ctx, float x, float y, float z) {
                if (!session.Field.MapMetadata.TryGet(session.Field.MapId, out MapMetadata? map)) {
                    return;
                }

                if (!mapDataStorage.TryGet(map.XBlock, out FieldAccelerationStructure? mapData)) {
                    return;
                }

                Vector3 center = session.Player.Position;
                Vector3S cell = FieldAccelerationStructure.PointToCell(center);

                ctx.Console.Out.WriteLine($"Player at {center.X} {center.Y} {center.Z} in cell {cell.X} {cell.Y} {cell.Z}");

                int count = 0;
                mapData.QueryVibrateObjectsCenter(session.Player.Position, 2 * new Vector3(x, y, z), (vibrate) => {
                    Vector3 center = vibrate.Position;
                    Vector3S cell = FieldAccelerationStructure.PointToCell(center);

                    count++;

                    ctx.Console.Out.WriteLine($"Vibrate object found at {center.X} {center.Y} {center.Z} in cell {cell.X} {cell.Y} {cell.Z}");
                });

                ctx.Console.Out.WriteLine($"Vibrate object count: {count}");
            }
        }

        private class DebugQuerySellableTileCommand : Command {
            private readonly GameSession session;
            private readonly MapDataStorage mapDataStorage;

            public DebugQuerySellableTileCommand(GameSession session, MapDataStorage mapDataStorage) : base("sellable", "Searches for nearby sellable tiles.") {
                this.session = session;
                this.mapDataStorage = mapDataStorage;

                var x = new Argument<float>("x", () => 300, "How far along the x axis to search from the player.");
                var y = new Argument<float>("y", () => 300, "How far along the y axis to search from the player.");
                var z = new Argument<float>("z", () => 300, "How far along the z axis to search from the player.");

                AddArgument(x);
                AddArgument(y);
                AddArgument(z);

                this.SetHandler<InvocationContext, float, float, float>(Handle, x, y, z);
            }

            private void Handle(InvocationContext ctx, float x, float y, float z) {
                if (!session.Field.MapMetadata.TryGet(session.Field.MapId, out MapMetadata? map)) {
                    return;
                }

                if (!mapDataStorage.TryGet(map.XBlock, out FieldAccelerationStructure? mapData)) {
                    return;
                }

                Vector3 center = session.Player.Position;
                Vector3S cell = FieldAccelerationStructure.PointToCell(center);

                ctx.Console.Out.WriteLine($"Player at {center.X} {center.Y} {center.Z} in cell {cell.X} {cell.Y} {cell.Z}");

                mapData.QuerySellableTilesCenter(session.Player.Position, 2 * new Vector3(x, y, z), (sellableTile) => {
                    Vector3 position = sellableTile.Position;
                    Vector3S toCell = FieldAccelerationStructure.PointToCell(position);


                    ctx.Console.Out.WriteLine($"SellableGroupId {sellableTile.SellableGroup} found at {position.X} {position.Y} {position.Z} in cell {toCell.X} {toCell.Y} {toCell.Z}");
                });
            }
        }
    }

    private class LogoutCommand : Command {
        private readonly GameSession session;

        public LogoutCommand(GameSession session) : base("logout", "Logs out.") {
            this.session = session;

            this.SetHandler<InvocationContext>(Handle);
        }

        private void Handle(InvocationContext ctx) {
            session.Disconnect();
        }
    }
}

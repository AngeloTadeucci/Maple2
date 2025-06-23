using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Util;

namespace Maple2.Server.Game.Commands;

public class TutorialCommand : GameCommand {
    private const string NAME = "tutorial";
    private const string DESCRIPTION = "Tutorial management.";
    public const AdminPermissions RequiredPermission = AdminPermissions.Debug;

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required ItemStatsCalculator ItemStatsCalc { private get; init; }
    // ReSharper restore All
    #endregion

    public TutorialCommand(GameSession session) : base(RequiredPermission, NAME, DESCRIPTION) {
        Add(new RewardCommand(session, this));
    }

    private class RewardCommand : Command {
        private enum Type {
            Item,
            Skill,
            Map,
        }

        private readonly GameSession session;
        private readonly TutorialCommand parent;

        private ItemStatsCalculator ItemStatsCalc => parent.ItemStatsCalc;

        public RewardCommand(GameSession session, TutorialCommand parent) : base("reward", "Receive tutorial reward.") {
            this.session = session;
            this.parent = parent;

            var type = new Argument<Type>("type", "Type of tutorial reward.");

            AddArgument(type);
            this.SetHandler<InvocationContext, Type>(Handle, type);
        }

        private void Handle(InvocationContext ctx, Type type) {
            try {
                Player player = session.Player;
                JobTable jobTable = session.TableMetadata.JobTable;
                JobTable.Tutorial tutorial = jobTable.Entries[player.Character.Job.Code()].Tutorial;

                switch (type) {
                    case Type.Item: {
                            foreach (JobTable.Item rewardItem in tutorial.StartItem.Concat(tutorial.Reward)) {
                                Item? item = session.Field.ItemDrop.CreateItem(rewardItem.Id, rewardItem.Rarity, rewardItem.Count);
                                if (item == null) {
                                    ctx.Console.Error.WriteLine($"Failed to create item: {rewardItem.Id}");
                                    ctx.ExitCode = 1;
                                    return;
                                }

                                session.Item.Inventory.Add(item, true);
                            }
                            break;
                        }
                    case Type.Skill:
                        break;
                    case Type.Map:
                        player.Unlock.Maps.UnionWith(tutorial.OpenMaps);
                        player.Unlock.Taxis.UnionWith(tutorial.OpenTaxis);
                        break;
                }
                ctx.ExitCode = 0;
            } catch (SystemException ex) {
                ctx.Console.Error.WriteLine(ex.Message);
                ctx.ExitCode = 1;
            }
        }
    }
}

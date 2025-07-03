using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game.Party;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.World.Service;
using WorldClient = Maple2.Server.World.Service.World.WorldClient;

namespace Maple2.Server.Game.PacketHandlers;

public class PartySearchHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.PartySearch;

    private enum Command : byte {
        Add = 0,
        Remove = 1,
        Load = 2,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required WorldClient World { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Add:
                HandleAdd(session, packet);
                break;
            case Command.Remove:
                HandleRemove(session);
                break;
            case Command.Load:
                HandleLoad(session, packet);
                break;
        }
    }

    private void HandleAdd(GameSession session, IByteReader packet) {
        if (session.Party.Party == null) {
            // Create new party
            PartyResponse partyResponse = World.Party(new PartyRequest {
                RequestorId = session.CharacterId,
                Create = new PartyRequest.Types.Create { },
            });

            var error = (PartyError) partyResponse.Error;
            if (error != PartyError.none) {
                session.Send(PartyPacket.Error(error));
                return;
            }

            if (session.Party.SetParty(partyResponse.Party)) {
                session.Send(PartyPacket.Load(session.Party.Party!));
            }
        }

        string listingName = packet.ReadUnicodeString();
        bool noApproval = packet.ReadBool();
        int size = packet.ReadInt();

        if (session.Party.Party == null) {
            session.Send(PartyPacket.Error(PartyError.s_party_err_not_found));
            return;
        }

        if (session.Party.Party.Search != null) {
            session.Send(PartySearchPacket.Error(PartySearchError.s_partysearch_err_server_already_register));
            return;
        }

        if (session.Party.Party.LeaderCharacterId != session.CharacterId) {
            session.Send(PartySearchPacket.Error(PartySearchError.s_partysearch_err_server_not_chief));
            return;
        }

        if (session.Party.Party.Members.Count >= size) {
            session.Send(PartySearchPacket.Error(PartySearchError.s_partysearch_err_server_max_member));
            return;
        }

        PartySearchResponse response = session.World.PartySearch(new PartySearchRequest {
            Create = new PartySearchRequest.Types.Create {
                Name = listingName,
                NoApproval = noApproval,
                Size = size,
                RequestorId = session.CharacterId,
            },
            PartyId = session.Party.Party.Id,
        });

        if (response.Error != (int) PartySearchError.none) {
            session.Send(PartySearchPacket.Error((PartySearchError) response.Error, response.ErrorCategory));
        }
    }

    private void HandleRemove(GameSession session) {
        if (session.Party.Party == null) {
            session.Send(PartyPacket.Error(PartyError.s_party_err_not_found));
            return;
        }

        if (session.Party.Party.LeaderCharacterId != session.CharacterId) {
            session.Send(PartySearchPacket.Error(PartySearchError.s_partysearch_err_server_not_chief));
            return;
        }

        if (session.Party.Party.Search == null) {
            session.Send(PartySearchPacket.Error(PartySearchError.s_partysearch_err_server_not_find_recruit));
            return;
        }

        long partySearchId = session.Party.Party.Search.Id;
        PartySearchResponse response = World.PartySearch(new PartySearchRequest {
            Remove = new PartySearchRequest.Types.Remove(),
            PartyId = session.Party.Party.Id,
            Id = session.Party.Party.Search.Id,
        });

        if (response.Error != (int) PartySearchError.none) {
            session.Send(PartySearchPacket.Error((PartySearchError) response.Error, response.ErrorCategory));
            return;
        }

        session.Send(PartySearchPacket.Remove(partySearchId));

        if (session.Party.Party.Members.Count <= 1) {
            PartyResponse partyResponse = World.Party(new PartyRequest {
                RequestorId = session.CharacterId,
                Leave = new PartyRequest.Types.Leave {
                    PartyId = session.Party.Id,
                },
            });
        }
    }

    private void HandleLoad(GameSession session, IByteReader packet) {
        int unknown = packet.ReadInt();
        int unknown2 = packet.ReadInt();
        byte sort = packet.ReadByte();
        string searchString = packet.ReadUnicodeString();
        int pageNumber = packet.ReadInt();
        int unknown3 = packet.ReadInt();

        if (!Enum.IsDefined(typeof(PartySearchSort), sort)) {
            session.Send(PartySearchPacket.Error(PartySearchError.s_partysearch_err_server_invalid_type));
            return;
        }

        PartySearchResponse response = session.World.PartySearch(new PartySearchRequest {
            Fetch = new PartySearchRequest.Types.Fetch {
                Page = pageNumber,
                SearchString = searchString,
                SortBy = sort,
            },
        }
        );

        if (response.Error != (int) PartySearchError.none) {
            session.Send(PartySearchPacket.Error((PartySearchError) response.Error, response.ErrorCategory));
            return;
        }

        ICollection<PartySearch> entries = response.PartySearches.Select(info => new PartySearch(info.Id, info.Name, info.Size) {
            Id = info.Id,
            PartyId = info.PartyId,
            CreationTime = info.CreationTime,
            LeaderAccountId = info.LeaderAccountId,
            LeaderCharacterId = info.LeaderCharacterId,
            LeaderName = info.LeaderName,
            MemberCount = info.MemberCount,
            Name = info.Name,
            NoApproval = info.NoApproval,
            Size = info.Size,
        }).ToList();

        session.Send(PartySearchPacket.Load(entries));
    }
}

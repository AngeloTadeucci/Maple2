using Maple2.Database.Storage;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Core.Packets;
using Maple2.Server.Login.Session;

namespace Maple2.Server.Login.PacketHandlers;

public class UgcHandler : PacketHandler<LoginSession> {
    public override RecvOp OpCode => RecvOp.Ugc;

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required GameStorage GameStorage { private get; init; }
    // ReSharper restore All
    #endregion

    private enum Command : byte {
        ProfilePicture = 11,
    }

    public override void Handle(LoginSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.ProfilePicture:
                HandleProfilePicture(session, packet);
                break;
        }
    }

    private void HandleProfilePicture(LoginSession session, IByteReader packet) {
        string picture = packet.ReadUnicodeString();

        using GameStorage.Request db = GameStorage.Context();
        Character? character = db.GetCharacter(session.CharacterId, session.AccountId);
        if (character == null) {
            Logger.Warning("Unable to locate character {CharacterId} for account {AccountId}", session.CharacterId, session.AccountId);
            return;
        }

        character.Picture = picture;
        db.SaveCharacter(character);
        session.Send(UgcPacket.ProfilePicture(character));
    }
}

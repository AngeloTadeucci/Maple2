using Maple2.Database.Storage;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using static Maple2.Model.Error.CharacterCreateError;

namespace Maple2.Server.Game.PacketHandlers;

public class CheckCharacterNameHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.CheckCharName;

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required GameStorage GameStorage { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        string characterName = packet.ReadUnicodeString();
        long itemUid = packet.ReadLong();

        if (characterName.Length < Constant.CharacterNameLengthMin) {
            session.Send(CharacterListPacket.CreateError(s_char_err_name));
            return;
        }

        if (characterName.Length > Constant.CharacterNameLengthMax) {
            session.Send(CharacterListPacket.CreateError(s_char_err_system));
            return;
        }

        using GameStorage.Request db = GameStorage.Context();
        long existingId = db.GetCharacterId(characterName);
        session.Send(CheckCharacterNamePacket.Result(existingId != default, characterName, itemUid));
    }
}

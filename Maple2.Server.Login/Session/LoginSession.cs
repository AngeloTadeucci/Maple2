﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Network;
using Maple2.Server.Core.Packets;
using Maple2.Server.Global.Service;
using Maple2.Server.World.Service;
using static Maple2.Model.Error.CharacterCreateError;
using WorldClient = Maple2.Server.World.Service.World.WorldClient;

namespace Maple2.Server.Login.Session;

public class LoginSession : Core.Network.Session {
    protected override PatchType Type => PatchType.Delete;

    private bool disposed;
    public readonly LoginServer Server;

    public long AccountId { get; private set; }
    public long CharacterId { get; private set; } // Used only as a temporary variable
    public Guid MachineId { get; private set; }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required WorldClient World { private get; init; }
    public required GameStorage GameStorage { private get; init; }
    // ReSharper restore All
    #endregion

    private Account account = null!;

    public LoginSession(TcpClient tcpClient, LoginServer server) : base(tcpClient) {
        Server = server;
        State = SessionState.ChangeMap;
    }

    public void Init(long accountId, Guid machineId) {
        AccountId = accountId;
        MachineId = machineId;

        // Account is already logged into login server.
        if (Server.GetSession(accountId, out LoginSession? existing) && existing != this) {
            Send(LoginResultPacket.Error((byte) LoginResponse.Types.Code.AlreadyLogin, "", accountId));
            existing.Disconnect();
            Disconnect();
            return;
        }

        State = SessionState.Connected;
        Server.OnConnected(this);
    }

    public void ListServers() {
        ChannelsResponse response = World.Channels(new ChannelsRequest());
        Send(BannerListPacket.Load(Server.GetSystemBanners()));
        Send(ServerListPacket.Load(Target.SERVER_NAME, [new IPEndPoint(Target.LoginIp, Target.LoginPort)], response.Channels));
    }

    public void ListCharacters() {
        using GameStorage.Request db = GameStorage.Context();
        (Account? readAccount, IList<Character>? characters) = db.ListCharacters(AccountId);
        if (readAccount == null || characters == null) {
            throw new InvalidOperationException($"Failed to load characters for account: {AccountId}");
        }

        account = readAccount;
        var entries = new List<(Character, IDictionary<ItemGroup, List<Item>>)>();
        foreach (Character character in characters) {
            IDictionary<ItemGroup, List<Item>> equips =
                db.GetItemGroups(character.Id, ItemGroup.Gear, ItemGroup.Outfit, ItemGroup.Badge);
            entries.Add((character, equips));
        }

        Send(CharacterListPacket.SetMax(account.MaxCharacters, Constant.ServerMaxCharacters));
        Send(CharacterListPacket.StartList());
        // Send each character data
        Send(CharacterListPacket.AddEntries(account, entries));
        Send(CharacterListPacket.EndList());
    }

    public void CreateCharacter(Character createCharacter, List<Item> createOutfits) {
        using GameStorage.Request db = GameStorage.Context();
        db.BeginTransaction();
        Character? character = db.CreateCharacter(createCharacter);
        if (character == null) {
            throw new InvalidOperationException($"Failed to create character: {createCharacter.Id}");
        }
        CharacterId = character.Id;

        var unlock = new Unlock();

        foreach (int emoteId in Constant.DefaultEmotes) {
            unlock.Emotes.Add(emoteId);
        }
        character.AchievementInfo = db.GetAchievementInfo(AccountId, character.Id);
        character.PremiumTime = account.PremiumTime;

        db.InitNewCharacter(character.Id, unlock);

        foreach (Item item in createOutfits) {
            item.Transfer?.Bind(character);
        }
        List<Item>? outfits = db.CreateItems(character.Id, createOutfits.ToArray());

        if (outfits == null || !db.Commit()) {
            Send(CharacterListPacket.CreateError(s_char_err_system));
            return;
        }

        Send(CharacterListPacket.SetMax(account.MaxCharacters, Constant.ServerMaxCharacters));
        Send(CharacterListPacket.AppendEntry(account, character,
            new Dictionary<ItemGroup, List<Item>> { { ItemGroup.Outfit, outfits } }));
    }

    #region Dispose
    ~LoginSession() => Dispose(false);

    protected override void Dispose(bool disposing) {
        if (disposed) return;
        disposed = true;

        try {
            Server.OnDisconnected(this);
            State = SessionState.Disconnected;
            Complete();
        } finally {
            base.Dispose(disposing);
        }
    }
    #endregion
}

using Maple2.Database.Storage;
using Maple2.Model.Game;
using Maple2.Server.Core.Sync;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Util.Sync;
using Serilog;

namespace Maple2.Server.Game.Manager;

public sealed class MarriageManager {
    private readonly GameSession session;

    public static Marriage Marriage;
    private readonly CancellationTokenSource tokenSource;

    private readonly ILogger logger = Log.Logger.ForContext<MarriageManager>();

    public MarriageManager(GameSession session) {
        this.session = session;
        tokenSource = new CancellationTokenSource();
        using GameStorage.Request db = session.GameStorage.Context();
        Marriage = db.GetMarriage(session.CharacterId) ?? Marriage.Default;
        Marriage.Exp = GetExp();
    }

    public void Load() {
        session.Send(MarriagePacket.Load(Marriage));
    }

    private long GetExp() {
        DateTime creationTime = DateTimeOffset.FromUnixTimeSeconds(Marriage.CreationTime).LocalDateTime;
        int daysMarried = (DateTime.Now - creationTime).Days;

        return daysMarried * 5 + Marriage.ExpHistory.Sum(exp => exp.Amount);
    }

    #region PlayerInfo Events
    private void BeginListen(MarriagePartner partner) {
        if (Marriage.Id == 0) {
            return;
        }
        // Clean up previous token if necessary
        if (partner.TokenSource != null) {
            logger.Warning("BeginListen called on Member {Id} that was already listening", partner.CharacterId);
            EndListen(partner);
        }

        partner.TokenSource = CancellationTokenSource.CreateLinkedTokenSource(tokenSource.Token);
        CancellationToken token = partner.TokenSource.Token;
        var listener = new PlayerInfoListener(UpdateField.Marriage, (type, info) => SyncUpdate(token, partner.CharacterId, type, info));
        session.PlayerInfo.Listen(partner.Info!.CharacterId, listener);
    }

    private void EndListen(MarriagePartner partner) {
        partner.TokenSource?.Cancel();
        partner.TokenSource?.Dispose();
        partner.TokenSource = null;
    }

    private bool SyncUpdate(CancellationToken cancel, long id, UpdateField type, IPlayerInfo info) {
        if (cancel.IsCancellationRequested || Marriage.Id == 0) {
            return true;
        }

        MarriagePartner? partner = Marriage.Partner1.CharacterId == id ? Marriage.Partner1 :
            Marriage.Partner2.CharacterId == id ? Marriage.Partner2 : null;
        if (partner == null) {
            return true;
        }

        bool wasOnline = partner.Info!.Online;
        string name = partner.Info.Name;
        partner.Info.Update(type, info);

        if (name != partner.Info.Name) {
            session.Send(MarriagePacket.Load(Marriage));
        }
        return false;
    }
    #endregion
}

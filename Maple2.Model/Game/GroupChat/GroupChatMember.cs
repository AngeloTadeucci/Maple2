namespace Maple2.Model.Game.GroupChat;

public class GroupChatMember : IDisposable {
    public required PlayerInfo Info;
    public long CharacterId => Info.CharacterId;
    public string Name => Info.Name;

    public CancellationTokenSource? TokenSource;

    public void Dispose() {
        TokenSource?.Cancel();
        TokenSource?.Dispose();
        TokenSource = null;
    }
}

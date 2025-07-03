using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Maple2.Model.Game.GroupChat;

public class GroupChat {
    public required int Id { get; init; }
    public readonly ConcurrentDictionary<long, GroupChatMember> Members;

    [SetsRequiredMembers]
    public GroupChat(int id) {
        Id = id;
        Members = new ConcurrentDictionary<long, GroupChatMember>();
    }
}

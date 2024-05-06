using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Maple2.Model.Game.GroupChat;

[method: SetsRequiredMembers]
public class GroupChat(int id) {
    public required int Id { get; init; } = id;
    public readonly ConcurrentDictionary<long, GroupChatMember> Members = new();

}

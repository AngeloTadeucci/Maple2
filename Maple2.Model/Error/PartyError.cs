
// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum PartyError : byte {
    none = 0,
    [Description("The party is full.")]
    s_party_err_full = 2,
    [Description("You are not the party leader.")]
    s_party_err_not_chief = 4,
    [Description("The party has already been made.")]
    s_party_err_already = 5,
    [Description("You cannot invite that player to the party.")]
    s_party_err_not_exist = 7,
    [Description("{0} declined the party invitation.")]
    s_party_err_deny = 9,
    [Description("You cannot invite yourself to a party.")]
    s_party_err_myself = 11,
    [Description("{0} failed to respond to your party invitation.")]
    s_party_err_deny_by_timeout = 12,
    [Description("That player cannot accept party invites at this time.")]
    s_party_err_cannot_invite = 14,
    [Description("{0} has already received a party request.")]
    s_party_err_alreadyInvite = 15,
    [Description("You did not meet the entry requirements.")]
    s_party_err_fail_enterable_result = 16,
    [Description("Your Level is lower than the minimum level requirement.")]
    s_party_err_lack_level = 17,
    [Description("Your Gear Score is lower than the minimum Gear Score requirement.")]
    s_party_err_lack_gear_score = 18,
    [Description("The party is full.")]
    s_party_err_full_limit_player = 19,
    [Description("{0} refused the party invitation.")]
    s_party_err_deny_by_auto = 20,
    [Description("{0} cannot accept party invitations right now. Please try again later.")]
    s_party_err_deny_by_system = 21,
    [Description("This recruitment listing has been deleted.")]
    s_party_err_invalid_party = 22,
    [Description("Recruitment listing outdated. Please refresh and try again.")]
    s_party_err_invalid_chief = 23,
    [Description("This recruitment listing has been deleted.")]
    s_party_err_invalid_recruit = 24,
    [Description("Recruitment listing outdated. Please refresh and try again.")]
    s_party_err_wrong_party = 25,
    [Description("Recruitment listing outdated. Please refresh and try again.")]
    s_party_err_wrong_recruit = 26,
    [Description("Not enough merets.")]
    s_err_lack_merat = 27,
    [Description("You already received a party invite.")]
    s_party_err_inviteMe = 28,
    [Description("You cannot reset the dungeon while a party member is still inside.")]
    s_room_party_err_is_in_user = 29,
    [Description("You cannot send a party invite while fighting a dungeon boss.")]
    s_party_invite_boss_room = 30,
    [Description("Party not found.")]
    s_party_err_not_found = 31,
    [Description("You requested to join {0}'s party.")]
    s_party_request_invite = 32,
    [Description("Another request is already in progress.")]
    s_party_err_already_vote = 33,
    [Description("Not enough party members to start a kick vote.")]
    s_party_err_vote_need_more_people = 34,
    [Description("Please wait before requesting another vote.")]
    s_party_err_vote_cooldown = 35,
    [Description("That can only be done while battling the boss of {0}.")]
    s_party_err_vote_cannot_kick_vote = 36,
    [Description("That can only be done while fighting in dungeons.")]
    s_party_err_vote_cannot_kick_vote_only_dungeon = 37,
    [Description("You cannot kick a party member while you are fighting a dungeon boss.")]
    s_party_expel_boss_room = 38,
    [Description("You cannot kick a player that is already in a Mushking Royale match.")]
    s_party_expel_maple_survival_squad = 39,
    [Description("Only the party leader can send the request.")]
    s_dungeonMatch_error_isNotChief = 40,
    [Description("A party member has disconnected.")]
    s_dungeonMatch_error_hasOfflineUser = 41,
    [Description("A party member is still in the dungeon.\nPlease try again after all party members have exited the dungeon.")]
    s_dungeonMatch_error_insideDungeonUser = 42,
    [Description("You're already matching for other content.")]
    s_party_err_dungeon_match_another = 43,
    [Description("Only the party leader can send the request.")]
    s_party_err_isNotChief = 44,
    [Description("A party member is offline.")]
    s_party_err_hasOfflineUser = 45,
    [Description("A party member is already playing Mushking Royale.")]
    s_party_err_inside_survival = 46,
    [Description("You're already matching for other content.")]
    s_party_err_another_matching = 47,
    [Description("A party member is queueing for solo Mushking Royale.")]
    s_party_err_survival_has_solo_register = 48,
    [Description("A Mushking Royale squad cannot have more than 4 players.")]
    s_maple_survival_error_squad_register_over_count = 49,
}

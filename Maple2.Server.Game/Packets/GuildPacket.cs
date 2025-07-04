using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class GuildPacket {
    private enum Command : byte {
        Load = 0,
        Created = 1,
        Disbanded = 2,
        Invited = 3,
        InviteInfo = 4,
        InviteReply = 5,
        NotifyInvite = 6,
        Leave = 7,
        Expelled = 8,
        NotifyExpel = 9,
        UpdateMemberRank = 10,
        UpdateMemberMessage = 11,
        UpdateMemberName = 12,
        Unknown14 = 14,
        CheckedIn = 15,
        Joined = 18,
        NotifyLeave = 19,
        NotifyExpelMember = 20,
        NotifyUpdateMemberRank = 21,
        NotifyUpdateMemberMessage = 22,
        NotifyLogin = 23,
        NotifyLogout = 24,
        NotifyUpdateLeader = 25,
        NotifyUpdateNotice = 26,
        NotifyUpdateEmblem = 27,
        NotifyUpdateCapacity = 28,
        NotifyUpdateRank = 29,
        NotifyUpdateFocus = 30,
        UpdateMemberMap = 31,
        UpdateMember = 32,
        // NotifyLeave = 33, // Duplicate of 19?
        UpdateName = 34,
        Achievement = 35,
        CheckInTime = 36,
        // 37, 38, 39, 40, 41, 42, 43, 44
        ReceiveApplication = 45,
        WithdrawApplication = 46,
        NotifyApplication = 47,
        NotifyApplicant = 48,
        GuildExperience = 49,
        GuildFunds = 50,
        GuildContribution = 51,
        NotifyUseBuff = 52,
        UpgradeBuff = 53,
        // 54
        UpgradeHouse = 55,
        UpdatePoster = 56,
        UpgradeNpc = 57,
        // 59, 60
        UpdateLeader = 61,
        UpdateNotice = 62,
        UpdateEmblem = 63,
        UpdateCapacity = 64,
        UpdateRank = 65,
        UpdateFocus = 66,
        SendMail = 69,
        // 72, 73, 74
        AddTag = 75,
        RemoveTag = 76,
        Error = 77,
        // 78
        SendApplication = 80,
        CancelApplication = 81,
        RespondApplication = 82,
        ListApplications = 83,
        ListAppliedGuilds = 84,
        ListGuilds = 85,
        UseBuff = 88,
        UsePersonalBuff = 89,
        AddContribution = 95,
        // 96, 104, 107, 109
        Donated = 110,
        // 114, 115, 116, 119, 120
    }

    public static ByteWriter Load(Guild guild) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteClass<Guild>(guild);

        return pWriter;
    }

    public static ByteWriter Created(string guildName) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.Created);
        pWriter.Write<GuildError>(GuildError.none);
        pWriter.WriteUnicodeString(guildName);

        return pWriter;
    }

    public static ByteWriter Disbanded() {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.Disbanded);
        pWriter.Write<GuildError>(GuildError.none);

        return pWriter;
    }

    public static ByteWriter Invited(string playerName) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.Invited);
        pWriter.WriteUnicodeString(playerName);

        return pWriter;
    }

    public static ByteWriter InviteInfo(GuildInvite info) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.InviteInfo);
        pWriter.WriteClass<GuildInvite>(info);

        return pWriter;
    }

    // s_guild_join
    // - You joined the guild {0}.
    // s_guild_join_reject
    // - You have rejected the guild invitation from {0}.
    public static ByteWriter InviteReply(GuildInvite info, bool accept) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.InviteReply);
        pWriter.WriteClass<GuildInvite>(info);
        pWriter.WriteBool(accept);

        return pWriter;
    }

    public static ByteWriter NotifyInvite(string name, GuildInvite.Response response) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.NotifyInvite);
        pWriter.WriteUnicodeString(name);
        pWriter.WriteBool(response == GuildInvite.Response.Accept);
        pWriter.Write<GuildInvite.Response>(response);

        return pWriter;
    }

    // s_guild_leave
    // - You have left {0}.
    public static ByteWriter Leave() {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.Leave);

        return pWriter;
    }

    public static ByteWriter Expelled(string playerName) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.Expelled);
        pWriter.WriteUnicodeString(playerName);

        return pWriter;
    }

    // s_guild_notify_expeled_from
    // - {0} has kicked you out of the guild.
    // s_guild_notify_expeled
    // - You have been kicked out of the guild.
    public static ByteWriter NotifyExpel(string playerName) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.NotifyExpel);
        pWriter.WriteUnicodeString(playerName);

        return pWriter;
    }

    public static ByteWriter UpdateMemberRank(string playerName, byte rankId) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.UpdateMemberRank);
        pWriter.WriteUnicodeString(playerName);
        pWriter.WriteByte(rankId);

        return pWriter;
    }

    public static ByteWriter UpdateMemberMessage(string message) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.UpdateMemberMessage);
        pWriter.WriteUnicodeString(message);

        return pWriter;
    }

    public static ByteWriter UpdateMemberName(string oldName, string newName) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.UpdateMemberName);
        pWriter.WriteUnicodeString(oldName);
        pWriter.WriteUnicodeString(newName);

        return pWriter;
    }

    public static ByteWriter Unknown14(string playerName) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.Unknown14);
        pWriter.WriteUnicodeString(playerName);
        pWriter.WriteInt(); // 3382

        return pWriter;
    }

    // s_guild_attend_complete
    // - You checked in.
    public static ByteWriter CheckedIn() {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.CheckedIn);

        return pWriter;
    }

    // s_guild_notify_accept_invite
    // - {0} invited {1} to the guild, and they accepted.
    public static ByteWriter Joined(string requestorName, GuildMember member, bool notify = true) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.Joined);
        pWriter.WriteUnicodeString(requestorName);
        pWriter.WriteUnicodeString(member.Name);
        pWriter.WriteBool(notify);
        pWriter.WriteClass<GuildMember>(member);

        return pWriter;
    }

    // s_guild_notify_leave
    // - {0} has left the guild.
    public static ByteWriter NotifyLeave(string playerName) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.NotifyLeave);
        pWriter.WriteUnicodeString(playerName);

        return pWriter;
    }

    // s_guild_notify_expel_member
    // - {0} has kicked {1} out of the guild.
    public static ByteWriter NotifyExpelMember(string requestorName, string playerName) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.NotifyExpelMember);
        pWriter.WriteUnicodeString(requestorName);
        pWriter.WriteUnicodeString(playerName);

        return pWriter;
    }

    // s_guild_notify_change_member_grade
    // - {0} is now the rank of [{1}].
    // s_guild_notify_change_member_grade_me
    // - Updated to the rank of [{0}] in the guild.
    public static ByteWriter NotifyUpdateMemberRank(string requestorName, string playerName, byte rankId) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.NotifyUpdateMemberRank);
        pWriter.WriteUnicodeString(requestorName);
        pWriter.WriteUnicodeString(playerName);
        pWriter.WriteByte(rankId);

        return pWriter;
    }

    public static ByteWriter NotifyUpdateMemberMessage(GuildMember member) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.NotifyUpdateMemberMessage);
        pWriter.WriteUnicodeString(member.Name);
        pWriter.WriteUnicodeString(member.Message);

        return pWriter;
    }

    // s_guild_notify_login_member
    // - Guild member {0} has logged on.
    public static ByteWriter NotifyLogin(string name) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.NotifyLogin);
        pWriter.WriteUnicodeString(name);

        return pWriter;
    }

    // s_guild_notify_logout_member
    // - Guild member {0} has logged off.
    public static ByteWriter NotifyLogout(string name, long time) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.NotifyLogout);
        pWriter.WriteUnicodeString(name);
        pWriter.WriteLong(time);

        return pWriter;
    }

    // s_guild_notify_change_master
    // - {0} is now the guild leader.
    // s_guild_notify_change_master_me
    // - You have become the guild leader.
    public static ByteWriter NotifyUpdateLeader(string oldLeader, string newLeader) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.NotifyUpdateLeader);
        pWriter.WriteUnicodeString(oldLeader);
        pWriter.WriteUnicodeString(newLeader);

        return pWriter;
    }

    // s_guild_notify_change_notify
    // - {0} has changed the guild notice.
    public static ByteWriter NotifyUpdateNotice(string requestorName, byte notice, string message) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.NotifyUpdateNotice);
        pWriter.WriteUnicodeString(requestorName);
        pWriter.WriteByte(notice); // 1
        pWriter.WriteUnicodeString(message);

        return pWriter;
    }

    // s_guild_notify_change_mark
    // - {0} has changed the guild emblem.
    public static ByteWriter NotifyUpdateEmblem(string requestorName, string emblem) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.NotifyUpdateEmblem);
        pWriter.WriteClass<InterfaceText>(new InterfaceText(StringCode.s_guild_notify_change_mark, requestorName));
        pWriter.WriteUnicodeString(emblem);

        return pWriter;
    }

    // s_guild_notify_change_capacity
    // - The guild's member maximum has been changed.
    public static ByteWriter NotifyUpdateCapacity(string requestorName, int capacity) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.NotifyUpdateCapacity);
        pWriter.WriteUnicodeString(requestorName);
        pWriter.WriteInt(capacity);

        return pWriter;
    }

    // s_guild_notify_change_grade
    // - {0}'s guild rank and authority have changed.
    public static ByteWriter NotifyUpdateRank(InterfaceText text, GuildRank rank) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.NotifyUpdateRank);
        pWriter.WriteClass<InterfaceText>(text);
        pWriter.WriteByte(rank.Id);
        pWriter.WriteClass<GuildRank>(rank);

        return pWriter;
    }

    public static ByteWriter NotifyUpdateFocus(string requestorName, bool toggle, GuildFocus focus) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.NotifyUpdateFocus);
        pWriter.WriteUnicodeString(requestorName);
        pWriter.WriteBool(toggle);
        pWriter.Write<GuildFocus>(focus);

        return pWriter;
    }

    public static ByteWriter UpdateMemberMap(string playerName, int mapId) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.UpdateMemberMap);
        pWriter.WriteUnicodeString(playerName);
        pWriter.WriteInt(mapId);

        return pWriter;
    }

    public static ByteWriter UpdateMember(PlayerInfo info) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.UpdateMember);
        pWriter.WriteUnicodeString(info.Name);
        GuildMember.WriteInfo(pWriter, info);

        return pWriter;
    }

    // s_guild_notify_change_name
    // - Guild name has changed to [{0}].
    public static ByteWriter UpdateName(string guildName) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.UpdateName);
        pWriter.WriteUnicodeString(guildName);

        return pWriter;
    }

    // s_guild_notify_achieve_progress
    // - {0} has obtained a trophy. {1}
    public static ByteWriter AchievementProgress(string playerName, int trophyId, int value) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.Achievement);
        pWriter.WriteUnicodeString(playerName);
        pWriter.WriteInt(trophyId);
        pWriter.WriteInt(value);
        pWriter.WriteShort(2);

        return pWriter;
    }

    // s_guild_notify_achieve_complete
    // - {0} has completed the final tier of a trophy! {1}
    public static ByteWriter AchievementComplete(string playerName, int trophyId, int value) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.Achievement);
        pWriter.WriteUnicodeString(playerName);
        pWriter.WriteInt(trophyId);
        pWriter.WriteInt(value);
        pWriter.WriteShort();

        return pWriter;
    }

    public static ByteWriter CheckInTime(string playerName, long time) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.CheckInTime);
        pWriter.WriteUnicodeString(playerName);
        pWriter.WriteLong(time);

        return pWriter;
    }

    public static ByteWriter ReceiveApplication(GuildApplication application) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.ReceiveApplication);
        pWriter.WriteClass<GuildApplication>(application);

        return pWriter;
    }

    public static ByteWriter WithdrawApplication(long applicationId) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.WithdrawApplication);
        pWriter.WriteLong(applicationId);

        return pWriter;
    }

    // s_guild_notify_search_join_accept
    // - {0} accepted {1}'s guild membership application.
    // s_guild_notify_search_join_reject
    // - {0} denied {1}'s guild membership application.
    public static ByteWriter NotifyApplication(string requestorName, string playerName, long applicationId, bool accepted) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.NotifyApplication);
        pWriter.WriteUnicodeString(requestorName);
        pWriter.WriteUnicodeString(playerName);
        pWriter.WriteBool(accepted);
        pWriter.WriteLong(applicationId);

        return pWriter;
    }

    public static ByteWriter NotifyApplicant(string guildName, long applicationId, bool accepted) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.NotifyApplicant);
        pWriter.WriteUnicodeString(guildName);
        pWriter.WriteLong(applicationId);
        pWriter.WriteBool(accepted);

        return pWriter;
    }

    public static ByteWriter GuildExperience(int exp) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.GuildExperience);
        pWriter.WriteInt(exp);

        return pWriter;
    }

    public static ByteWriter GuildFunds(int funds) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.GuildFunds);
        pWriter.WriteInt(funds);

        return pWriter;
    }

    // s_guild_gain_contribution
    // - You have acquired {0} contributions.
    public static ByteWriter GuildContribution(GuildMember member, int contribution) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.GuildContribution);
        pWriter.WriteUnicodeString(member.Name);
        pWriter.WriteInt(contribution);
        pWriter.WriteInt(member.WeeklyContribution);
        pWriter.WriteInt(member.TotalContribution);

        return pWriter;
    }

    // s_guild_notify_use_skill_user
    // - {0} used the {1} skill.
    public static ByteWriter NotifyUseBuff(string playerName, GuildBuff buff) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.NotifyUseBuff);
        pWriter.WriteUnicodeString(playerName);
        pWriter.Write<GuildBuff>(buff);

        return pWriter;
    }

    // s_guild_notify_upgrade_skill
    // - {0} upgraded the skill [{1}].
    public static ByteWriter UpgradeBuff(string playerName, GuildBuff buff) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.UpgradeBuff);
        pWriter.WriteUnicodeString(playerName);
        pWriter.Write<GuildBuff>(buff);

        return pWriter;
    }

    // s_guild_notify_house_upgrade
    // - {0} has changed the guild house theme to [{2}].
    public static ByteWriter UpgradeHouse(string playerName, int rank, int theme) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.UpgradeHouse);
        pWriter.WriteUnicodeString(playerName);
        pWriter.WriteInt(rank);
        pWriter.WriteInt(theme);

        return pWriter;
    }

    // s_guild_notify_change_poster
    // - {0} has changed the guild poster.
    public static ByteWriter UpdatePoster(GuildPoster poster) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.UpdatePoster);
        pWriter.WriteLong(poster.OwnerId);
        pWriter.WriteUnicodeString(poster.OwnerName);
        pWriter.WriteInt(poster.Id);
        pWriter.WriteUnicodeString(poster.Picture);

        return pWriter;
    }

    // s_guild_notify_upgrade_npc
    // - {0} upgraded {1} to level {2}.
    public static ByteWriter UpgradeNpc(string playerName, GuildNpc npc) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.UpgradeNpc);
        pWriter.WriteUnicodeString(playerName);
        pWriter.Write<GuildNpc>(npc);

        return pWriter;
    }

    public static ByteWriter UpdateLeader(string leaderName) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.UpdateLeader);
        pWriter.WriteUnicodeString(leaderName);

        return pWriter;
    }

    // s_guild_change_notify
    // - The guild notice has been changed.
    public static ByteWriter UpdateNotice(string notice) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.UpdateNotice);
        pWriter.WriteBool(true); // success
        pWriter.WriteUnicodeString(notice);

        return pWriter;
    }

    public static ByteWriter UpdateEmblem(string emblem) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.UpdateEmblem);
        pWriter.WriteUnicodeString(emblem);

        return pWriter;
    }

    // s_guild_extend_capacity_success
    // - The maximum amount of guild members is now {0}.
    public static ByteWriter UpdateCapacity(int capacity) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.UpdateCapacity);
        pWriter.WriteInt(capacity);

        return pWriter;
    }

    // s_guild_change_grade_sucess
    // - The name and/or privileges of the {0} rank have changed.
    public static ByteWriter UpdateRank(GuildRank rank) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.UpdateRank);
        pWriter.WriteByte(rank.Id);
        pWriter.WriteClass<GuildRank>(rank);

        return pWriter;
    }

    public static ByteWriter UpdateFocus(bool toggle, GuildFocus focus) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.UpdateFocus);
        pWriter.WriteBool(toggle);
        pWriter.Write<GuildFocus>(focus);

        return pWriter;
    }

    // s_mail_send
    // - Mail has been sent.
    public static ByteWriter SendMail() {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.SendMail);

        return pWriter;
    }

    public static ByteWriter AddTag(string playerName, string guildName) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.AddTag);
        pWriter.WriteUnicodeString(playerName);
        pWriter.WriteUnicodeString(guildName);

        return pWriter;
    }

    public static ByteWriter RemoveTag(string playerName) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.RemoveTag);
        pWriter.WriteUnicodeString(playerName);

        return pWriter;
    }

    public static ByteWriter Error(GuildError error, int arg = 0) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.Error);
        pWriter.WriteByte(1);
        pWriter.Write<GuildError>(error);
        pWriter.WriteInt(arg);

        return pWriter;
    }

    // s_guild_search_request_join_guild
    // - You have applied to the guild {0}.
    public static ByteWriter SendApplication(long applicationId, string guildName) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.SendApplication);
        pWriter.WriteLong(applicationId);
        pWriter.WriteUnicodeString(guildName);

        return pWriter;
    }

    public static ByteWriter CancelApplication(long applicationId, string guildName) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.CancelApplication);
        pWriter.WriteLong(applicationId);
        pWriter.WriteUnicodeString(guildName);

        return pWriter;
    }

    public static ByteWriter RespondApplication(long applicationId, string guildName, bool accepted) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.RespondApplication);
        pWriter.WriteLong(applicationId);
        pWriter.WriteUnicodeString(guildName);
        pWriter.WriteBool(accepted);

        return pWriter;
    }

    public static ByteWriter ListApplications(ICollection<GuildApplication> applications) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.ListApplications);
        pWriter.WriteInt(applications.Count);
        foreach (GuildApplication application in applications) {
            pWriter.WriteBool(true);
            pWriter.WriteClass<GuildApplication>(application);
        }

        return pWriter;
    }

    public static ByteWriter ListAppliedGuilds(ICollection<GuildApplication> applications) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.ListAppliedGuilds);
        pWriter.WriteInt(applications.Count);
        foreach (GuildApplication application in applications) {
            pWriter.WriteBool(true);
            pWriter.WriteLong(application.Id);
            pWriter.WriteLong(application.Guild.Id);
            pWriter.WriteUnicodeString(application.Guild.Name);
            pWriter.WriteUnicodeString(application.Guild.Emblem);
            pWriter.Write<AchievementInfo>(application.Guild.AchievementInfo);
            pWriter.WriteInt(application.Guild.Members.Count);
            pWriter.WriteInt(application.Guild.Capacity);
            pWriter.WriteUnicodeString();
            pWriter.WriteLong(application.Applicant.AccountId);
            pWriter.WriteLong(application.Applicant.CharacterId);
            pWriter.WriteLong(application.CreationTime);
        }

        return pWriter;
    }

    public static ByteWriter ListGuilds(ICollection<Guild> guilds) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.ListGuilds);
        pWriter.WriteInt(guilds.Count);
        foreach (Guild guild in guilds) {
            pWriter.WriteBool(true);
            pWriter.WriteLong(guild.Id);
            pWriter.WriteUnicodeString(guild.Name);
            pWriter.WriteUnicodeString(guild.Emblem);
            pWriter.Write<AchievementInfo>(guild.AchievementInfo);
            pWriter.WriteInt(guild.Members.Count);
            pWriter.WriteInt(guild.Capacity);
            pWriter.Write<GuildFocus>(guild.Focus);
            pWriter.WriteLong(guild.LeaderAccountId);
            pWriter.WriteLong(guild.LeaderCharacterId);
            pWriter.WriteUnicodeString(guild.LeaderName);
        }

        return pWriter;
    }

    public static ByteWriter UseBuff(int buffId) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.UseBuff);
        pWriter.WriteInt(buffId);

        return pWriter;
    }

    public static ByteWriter UsePersonalBuff(int buffId) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.UsePersonalBuff);
        pWriter.WriteInt(buffId);

        return pWriter;
    }

    public static ByteWriter AddContribution(int addExp, int addFunds) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.AddContribution);
        pWriter.WriteInt(addExp);
        pWriter.WriteInt(addFunds);

        return pWriter;
    }

    public static ByteWriter Donated(GuildMember member) {
        var pWriter = Packet.Of(SendOp.Guild);
        pWriter.Write<Command>(Command.Donated);
        pWriter.WriteInt(member.DailyDonationCount);
        pWriter.WriteLong(member.DonationTime);

        return pWriter;
    }
}

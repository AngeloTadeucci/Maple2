using System.Collections.Concurrent;
using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model.Metadata;
using Maple2.Server.World.Service;

namespace Maple2.Server.World.Containers;

public class PlayerConfigLookUp {
    private readonly SkillMetadataStorage skillMetadataStorage;

    private readonly ConcurrentDictionary<long, ConcurrentDictionary<int, BuffInfo>> buffs;
    private readonly ConcurrentDictionary<long, ConcurrentDictionary<int, SkillCooldownInfo>> skillCooldowns;
    private readonly ConcurrentDictionary<long, DeathInfo> deaths;

    public PlayerConfigLookUp(SkillMetadataStorage skillMetadataStorage) {
        this.skillMetadataStorage = skillMetadataStorage;
        buffs = [];
        skillCooldowns = [];
        deaths = [];
    }

    public void Save(List<BuffInfo> saveBuffs, List<SkillCooldownInfo> skillCooldownInfos, DeathInfo death, long characterId) {
        // Save buffs
        if (!buffs.TryGetValue(characterId, out ConcurrentDictionary<int, BuffInfo>? list)) {
            buffs.TryAdd(characterId, new ConcurrentDictionary<int, BuffInfo>());
            list = buffs[characterId];
        }

        // Add to existing list
        foreach (BuffInfo buff in saveBuffs) {
            if (list.TryGetValue(buff.Id, out BuffInfo? _)) {
                list[buff.Id] = buff;
                continue;
            }

            list.TryAdd(buff.Id, buff);
        }

        // Save SkillCooldowns
        if (!skillCooldowns.TryGetValue(characterId, out ConcurrentDictionary<int, SkillCooldownInfo>? skillList)) {
            skillCooldowns.TryAdd(characterId, new ConcurrentDictionary<int, SkillCooldownInfo>());
            skillList = skillCooldowns[characterId];
        }

        // Add to existing list
        foreach (SkillCooldownInfo skillCooldown in skillCooldownInfos) {
            if (skillList.TryGetValue(skillCooldown.SkillId, out SkillCooldownInfo? _)) {
                skillList[skillCooldown.SkillId] = skillCooldown;
                continue;
            }

            skillList.TryAdd(skillCooldown.SkillId, skillCooldown);
        }

        // Save death
        if (!deaths.TryGetValue(characterId, out DeathInfo? _)) {
            deaths.TryAdd(characterId, death);
        } else {
            deaths[characterId] = death;
        }
    }

    public (List<BuffInfo> Buffs, List<SkillCooldownInfo> SkillCooldowns, DeathInfo Death) Retrieve(long characterId) {
        long currentTime = DateTime.Now.ToEpochSeconds();
        List<BuffInfo> buffs = RetrieveBuffs(characterId, currentTime);
        List<SkillCooldownInfo> skillCooldowns = RetrieveSkillCooldowns(characterId, currentTime);
        DeathInfo death = RetrieveDeath(characterId);
        return (buffs, skillCooldowns, death);
    }

    private List<BuffInfo> RetrieveBuffs(long characterId, long currentTime) {
        if (!buffs.TryGetValue(characterId, out ConcurrentDictionary<int, BuffInfo>? list)) {
            return [];
        }

        var removeBuffs = new List<BuffInfo>();
        foreach ((int buffId, BuffInfo buffInfo) in list) {
            if (!skillMetadataStorage.TryGetEffect(buffInfo.Id, (short) buffInfo.Level, out AdditionalEffectMetadata? metadata)) {
                throw new InvalidDataException($"Buff not found: {buffInfo.Id}, level: {buffInfo.Level}");
            }

            if (!metadata.Property.UseInGameTime) {
                long msSurpassed = (long) ((currentTime - buffInfo.StopTime) * TimeSpan.FromSeconds(1).TotalMilliseconds);
                if (msSurpassed > buffInfo.MsRemaining) {
                    removeBuffs.Add(buffInfo);
                } else {
                    buffInfo.MsRemaining -= (int) msSurpassed;
                }
            }
        }

        foreach (BuffInfo buffInfo in removeBuffs) {
            list.Remove(buffInfo.Id, out _);
        }

        return list.Values.ToList();
    }

    private List<SkillCooldownInfo> RetrieveSkillCooldowns(long characterId, long currentTime) {
        if (!skillCooldowns.TryGetValue(characterId, out ConcurrentDictionary<int, SkillCooldownInfo>? list)) {
            return [];
        }

        var removeSkillCooldowns = new List<SkillCooldownInfo>();
        foreach ((int skillId, SkillCooldownInfo skillCooldownInfo) in list) {
            if (!skillMetadataStorage.TryGet(skillCooldownInfo.SkillId, (short) skillCooldownInfo.SkillLevel, out SkillMetadata? skillMetadata)) {
                throw new InvalidDataException($"Skill not found: {skillCooldownInfo.SkillId}");
            }

            if (!skillMetadata.State.UseInGameTime) {
                long msSurpassed = (long) ((currentTime - skillCooldownInfo.StopTime) * TimeSpan.FromSeconds(1).TotalMilliseconds);
                if (msSurpassed > skillCooldownInfo.MsRemaining) {
                    removeSkillCooldowns.Add(skillCooldownInfo);
                } else {
                    skillCooldownInfo.MsRemaining -= (int) msSurpassed;
                }
            }
        }

        foreach (SkillCooldownInfo skillCooldownInfo in removeSkillCooldowns) {
            list.Remove(skillCooldownInfo.SkillId, out _);
        }

        return list.Values.ToList();
    }

    private DeathInfo RetrieveDeath(long characterId) {
        if (!deaths.TryGetValue(characterId, out DeathInfo? death)) {
            return new DeathInfo();
        }

        long msSurpassed = (long) ((DateTime.Now.ToEpochSeconds() - death.StopTime) * TimeSpan.FromSeconds(1).TotalMilliseconds);
        if (msSurpassed > death.MsRemaining) {
            deaths.Remove(characterId, out _);
            return new DeathInfo();
        }

        death.MsRemaining -= (int) msSurpassed;
        if (death.MsRemaining <= 0) {
            deaths.Remove(characterId, out _);
            return new DeathInfo();
        }

        return death;
    }

}

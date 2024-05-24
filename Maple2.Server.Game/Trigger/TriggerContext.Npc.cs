using System.Numerics;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Scripting.Trigger;

namespace Maple2.Server.Game.Trigger;

public partial class TriggerContext {
    public void ChangeMonster(int removeSpawnId, int addSpawnId) {
        DebugLog("[ChangeMonster] removeSpawnId:{RemoveId}, addSpawnId:{AddId}", removeSpawnId, addSpawnId);
        foreach (FieldNpc fieldMob in Field.Mobs.Values) {
            if (fieldMob.SpawnPointId == removeSpawnId) {
                Field.RemoveNpc(fieldMob.ObjectId);
            }
        }

        SpawnMonster(addSpawnId);
    }

    public void SpawnMonster(int[] spawnIds, bool spawnAnimation, int arg3) {
        WarnLog("[CreateMonster] spawnIds:{SpawnIds}, spawnAnimation:{SpawnAnimation}, arg3:{Arg3}", string.Join(", ", spawnIds), spawnAnimation, arg3);
        foreach (int spawnId in spawnIds) {
            SpawnNpc(spawnId);
        }
    }

    public void DestroyMonster(int[] spawnIds, bool arg2) {
        WarnLog("[DestroyMonster] spawnIds:{SpawnIds}, Arg2:{Arg2}", string.Join(", ", spawnIds), arg2);
        if (spawnIds.Contains(-1)) {
            foreach (int objectId in Field.Mobs.Keys) {
                Field.RemoveNpc(objectId);
            }

            foreach (int objectId in Field.Npcs.Keys) {
                Field.RemoveNpc(objectId);
            }
            return;
        }

        foreach (int spawnPointId in spawnIds) {
            FieldNpc? fieldNpc = Field.Npcs.Values.FirstOrDefault(npc => npc.SpawnPointId == spawnPointId);
            if (fieldNpc != null) {
                Field.RemoveNpc(fieldNpc.ObjectId);
                continue;
            }

            FieldNpc? fieldMob = Field.Mobs.Values.FirstOrDefault(mob => mob.SpawnPointId == spawnPointId);
            if (fieldMob != null) {
                Field.RemoveNpc(fieldMob.ObjectId);
            }
        }
    }

    private void SpawnMonster(int spawnId) {
        DebugLog("[SpawnMonster] spawnId:{SpawnId}", spawnId);
        SpawnNpc(spawnId);
    }

    public void InitNpcRotation(int[] spawnIds) {
        ErrorLog("[InitNpcRotation] spawnIds:{SpawnIds}", string.Join(", ", spawnIds));
    }

    public void LimitSpawnNpcCount(int limitCount, string desc) {
        ErrorLog("[LimitSpawnNpcCount] limitCount:{Count}", limitCount);
    }

    public void MoveNpc(int spawnId, string patrolName) {
        DebugLog("[MoveNpc] spawnId:{SpawnId} patrolName:{PatrolName}", spawnId, patrolName);
        var fieldNpc = Field.Npcs.Values.FirstOrDefault(npc => npc.SpawnPointId == spawnId);
        if (fieldNpc == null) {
            return;
        }

        MS2PatrolData? patrolData = Field.Entities.Patrols.FirstOrDefault(patrol => patrol.Name == patrolName);

        if (patrolData is null) {
            return;
        }

        fieldNpc.SetPatrolData(patrolData);
    }

    public void MoveNpcToPos(int spawnId, Vector3 position, Vector3 rotation) {
        WarnLog("[MoveNpcToPos] spawnId:{SpawnId}", spawnId);
        foreach (FieldNpc npc in Field.Npcs.Values) {
            if (npc.SpawnPointId == spawnId) {
                npc.Position = position;
                npc.Rotation = rotation;
            }
        }
    }

    public void NpcRemoveAdditionalEffect(int spawnId, int additionalEffectId) {
        WarnLog("[NpcRemoveAdditionalEffect] spawnId:{SpawnId}", spawnId);
        foreach (FieldNpc npc in Field.Npcs.Values) {
            if (npc.SpawnPointId == spawnId) {
                npc.Buffs.Remove(additionalEffectId);
            }
        }
    }

    public void NpcToPatrolInBox(int boxId, int npcId, string spawnId, string patrolName) {
        ErrorLog("[NpcToPatrolInBox] boxId:{BoxId}, npcId:{NpcId}, spawnId:{SpawnId}, patrolName:{PatrolName}", spawnId, npcId, spawnId, patrolName);
    }

    public void SetAiExtraData(string key, int value, bool isModify, int boxId) {
        ErrorLog("[SetAiExtraData] key:{Key}, value:{Value}, isModify:{IsModify}, boxId:{BoxId}", key, value, isModify, boxId);
    }

    public void SetDialogue(int type, int spawnId, string script, int delay, int arg5, Align align) {
        DebugLog("[SetDialogue] type:{Type}, spawnId:{SpawnId}, script:{Script}, delay:{Delay}, arg5:{Arg5}, align:{Align}",
            type, spawnId, script, delay, arg5, align);

        if (spawnId == 0) {
            var player = Field.Players.Values.FirstOrDefault();
            if (player == null) {
                return;
            }
            Broadcast(CinematicPacket.BalloonTalk(false, player.ObjectId, script, delay * 1000, 0));
            return;
        }

        if (type == 1) {
            var npc = Field.Npcs.Values.FirstOrDefault(npc => npc.SpawnPointId == spawnId);
            if (npc == null) {
                return;
            }

            Broadcast(CinematicPacket.BalloonTalk(false, npc.ObjectId, script, delay * 1000, 0));
            return;
        }

        Broadcast(CinematicPacket.Talk(spawnId, spawnId.ToString(), script, delay * 1000, align));
    }

    public void SetNpcDuelHpBar(bool isOpen, int spawnId, int durationTick, int npcHpStep) {
        ErrorLog("[SetNpcDuelHpBar] isOpen:{IsOpen}, spawnId:{SpawnId}, durationTick:{Duration}, npcHpStep:{HpStep}", isOpen, spawnId, durationTick, npcHpStep);
    }

    public void SetNpcEmotionLoop(int spawnId, string sequenceName, float duration) {
        WarnLog("[SetNpcEmotionLoop] spawnId:{SpawnId}, sequenceName:{SequenceName}, durationTick:{Duration}", spawnId, sequenceName, duration);

        FieldNpc? fieldNpc = Field.Npcs.Values.FirstOrDefault(npc => npc.SpawnPointId == spawnId);
        fieldNpc?.Animate(sequenceName, duration);
    }

    public void SetNpcEmotionSequence(int spawnId, string sequenceName, int durationTick) {
        WarnLog("[SetNpcEmotionSequence] spawnId:{SpawnId}, sequenceName:{SequenceName}, durationTick:{Duration}", spawnId, sequenceName, durationTick);

        FieldNpc? fieldNpc = Field.Npcs.Values.FirstOrDefault(npc => npc.SpawnPointId == spawnId);
        fieldNpc?.Animate(sequenceName);
    }

    public void SetNpcRotation(int spawnId, float rotation) {
        WarnLog("[SetNpcRotation] spawnId:{SpawnId}, rotation:{Rotation}", spawnId, rotation);
        foreach (FieldNpc npc in Field.Npcs.Values) {
            if (npc.SpawnPointId == spawnId) {
                npc.Rotation = npc.Rotation with { Z = rotation };
            }
        }
    }

    public void SpawnNpcRange(int[] spawnIds, bool isAutoTargeting, int randomPickCount, int score) {
        DebugLog("[SpawnNpcRange] spawnIds:{SpawnIds}, isAutoTargeting:{AutoTarget}, randomPickCount:{RandomCount}, score:{Score}",
            string.Join(", ", spawnIds), isAutoTargeting, randomPickCount, score);

        foreach (int spawnId in spawnIds) {
            SpawnNpc(spawnId);
        }
    }

    #region Conditions
    public bool CheckNpcAdditionalEffect(int spawnId, int additionalEffectId, int level) {
        ErrorLog("[CheckNpcAdditionalEffect] spawnId:{SpawnId}, additionalEffectId:{EffectId}, level:{Level}", spawnId, additionalEffectId, level);
        return false;
    }

    public bool MonsterDead(int[] spawnIds, bool autoTarget) {
        DebugLog("[MonsterDead] spawnIds:{SpawnIds}, arg2:{Arg2}", string.Join(", ", spawnIds), autoTarget);
        IEnumerable<FieldNpc> matchingMobs = Field.Mobs.Values.Where(x => spawnIds.Contains(x.SpawnPointId));

        foreach (FieldNpc mob in matchingMobs) {
            if (!mob.IsDead) {
                return false;
            }
        }

        // Either no mobs were found or they are all dead
        return true;
    }

    public bool MonsterInCombat(int[] spawnIds) {
        WarnLog("[MonsterInCombat] spawnIds:{SpawnIds}", string.Join(", ", spawnIds));
        foreach (FieldNpc mob in Field.Mobs.Values) {
            if (mob.SpawnPointId > 0 || !spawnIds.Contains(mob.SpawnPointId)) {
                continue;
            }

            if (mob.BattleState.TargetId != 0) {
                return true;
            }
        }

        return false;
    }

    public bool NpcDetected(int boxId, int[] spawnIds) {
        DebugLog("[NpcDetected] boxId:{BoxId}, spawnIds:{SpawnIds}", boxId, string.Join(", ", spawnIds));
        if (spawnIds.Length == 0 || spawnIds[0] == 0) {
            return NpcsInBox(boxId).Any();
        }

        foreach (FieldNpc mob in NpcsInBox(boxId)) {
            if (mob.SpawnPointId > 0 && spawnIds.Contains(mob.SpawnPointId)) {
                return true;
            }
        }

        return false;
    }

    public bool NpcIsDeadByStringId(string stringId) {
        ErrorLog("[NpcIsDeadByStringId] stringId:{Round}", stringId);
        return false;
    }
    #endregion

    private IEnumerable<FieldNpc> MonstersInBox(params int[] boxIds) {
        if (boxIds.Length == 0 || boxIds[0] == 0) {
            return Field.Mobs.Values;
        }

        IEnumerable<TriggerBox> boxes = boxIds
            .Select(boxId => Objects.Boxes.GetValueOrDefault(boxId))
            .Where(box => box != null)!;

        return Field.Mobs.Values.Where(mob => boxes.Any(box => box.Contains(mob.Position)));
    }

    private IEnumerable<FieldNpc> NpcsInBox(params int[] boxIds) {
        if (boxIds.Length == 0 || boxIds[0] == 0) {
            return Field.Mobs.Values;
        }

        IEnumerable<TriggerBox> boxes = boxIds
            .Select(boxId => Objects.Boxes.GetValueOrDefault(boxId))
            .Where(box => box != null)!;

        return Field.Npcs.Values.Where(mob => boxes.Any(box => box.Contains(mob.Position)));
    }

    private void SpawnNpc(int spawnId) {
        if (!Field.Entities.EventNpcSpawns.TryGetValue(spawnId, out EventSpawnPointNPC? spawn)) {
            logger.Error("[SpawnMonster] Invalid spawnId:{SpawnId}", spawnId);
            return;
        }

        foreach (SpawnPointNPCListEntry entry in spawn.NpcList) {
            if (!Field.NpcMetadata.TryGet(entry.NpcId, out NpcMetadata? npc)) {
                logger.Error("[SpawnMonster] Invalid npcId:{NpcId}", entry.NpcId);
                continue;
            }

            for (int i = 0; i < entry.Count; i++) {
                FieldNpc? fieldNpc = Field.SpawnNpc(npc, spawn.Position, spawn.Rotation);
                if (fieldNpc == null) {
                    logger.Error("[SpawnMonster] Failed to spawn npcId:{NpcId}", entry.NpcId);
                    continue;
                }

                fieldNpc.SpawnPointId = spawnId;

                Field.Broadcast(FieldPacket.AddNpc(fieldNpc));
                Field.Broadcast(ProxyObjectPacket.AddNpc(fieldNpc));
            }
        }
    }
}

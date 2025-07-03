using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Maple2.Model.Metadata;

namespace Maple2.Database.Storage;

internal class TriggerStorage : ITriggerStorage {
    private readonly ImmutableDictionary<int, Ms2Trigger> triggers;
    public ImmutableArray<Ms2TriggerActor> Actors { get; }
    public ImmutableArray<Ms2TriggerAgent> Agents { get; }
    public ImmutableArray<Ms2TriggerBox> Boxes { get; }
    public ImmutableArray<Ms2TriggerCamera> Cameras { get; }
    public ImmutableArray<Ms2TriggerCube> Cubes { get; }
    public ImmutableArray<Ms2TriggerEffect> Effects { get; }
    public ImmutableArray<Ms2TriggerLadder> Ladders { get; }
    public ImmutableArray<Ms2TriggerMesh> Meshes { get; }
    public ImmutableArray<Ms2TriggerRope> Ropes { get; }
    public ImmutableArray<Ms2TriggerSkill> Skills { get; }
    public ImmutableArray<Ms2TriggerSound> Sounds { get; }

    public TriggerStorage(List<Ms2Trigger> triggers) {
        var builder = ImmutableDictionary.CreateBuilder<int, Ms2Trigger>();
        var actorBuilder = ImmutableArray.CreateBuilder<Ms2TriggerActor>();
        var agentBuilder = ImmutableArray.CreateBuilder<Ms2TriggerAgent>();
        var boxBuilder = ImmutableArray.CreateBuilder<Ms2TriggerBox>();
        var cameraBuilder = ImmutableArray.CreateBuilder<Ms2TriggerCamera>();
        var cubeBuilder = ImmutableArray.CreateBuilder<Ms2TriggerCube>();
        var effectBuilder = ImmutableArray.CreateBuilder<Ms2TriggerEffect>();
        var ladderBuilder = ImmutableArray.CreateBuilder<Ms2TriggerLadder>();
        var meshBuilder = ImmutableArray.CreateBuilder<Ms2TriggerMesh>();
        var ropeBuilder = ImmutableArray.CreateBuilder<Ms2TriggerRope>();
        var skillBuilder = ImmutableArray.CreateBuilder<Ms2TriggerSkill>();
        var soundBuilder = ImmutableArray.CreateBuilder<Ms2TriggerSound>();

        foreach (Ms2Trigger trigger in triggers) {
            switch (trigger) {
                case Ms2TriggerActor actor:
                    actorBuilder.Add(actor);
                    break;
                case Ms2TriggerAgent agent:
                    agentBuilder.Add(agent);
                    break;
                case Ms2TriggerBox box:
                    boxBuilder.Add(box);
                    break;
                case Ms2TriggerCamera camera:
                    cameraBuilder.Add(camera);
                    break;
                case Ms2TriggerCube cube:
                    cubeBuilder.Add(cube);
                    break;
                case Ms2TriggerEffect effect:
                    effectBuilder.Add(effect);
                    break;
                case Ms2TriggerLadder ladder:
                    ladderBuilder.Add(ladder);
                    break;
                case Ms2TriggerMesh mesh:
                    meshBuilder.Add(mesh);
                    break;
                case Ms2TriggerRope rope:
                    ropeBuilder.Add(rope);
                    break;
                case Ms2TriggerSkill skill:
                    skillBuilder.Add(skill);
                    break;
                case Ms2TriggerSound sound:
                    soundBuilder.Add(sound);
                    break;
                default:
                    continue;
            }

            builder.Add(trigger.TriggerId, trigger);
        }

        this.triggers = builder.ToImmutable();
        Actors = actorBuilder.ToImmutable();
        Agents = agentBuilder.ToImmutable();
        Boxes = boxBuilder.ToImmutable();
        Cameras = cameraBuilder.ToImmutable();
        Cubes = cubeBuilder.ToImmutable();
        Effects = effectBuilder.ToImmutable();
        Ladders = ladderBuilder.ToImmutable();
        Meshes = meshBuilder.ToImmutable();
        Ropes = ropeBuilder.ToImmutable();
        Skills = skillBuilder.ToImmutable();
        Sounds = soundBuilder.ToImmutable();
    }

    public bool TryGet<T>(int key, [NotNullWhen(true)] out T? trigger) where T : Ms2Trigger {
        triggers.TryGetValue(key, out Ms2Trigger? result);
        trigger = result as T;
        return trigger != null;
    }

    public int Count => triggers.Count;
    public Ms2Trigger this[int key] => triggers[key];

    public IEnumerable<int> Keys => triggers.Keys;
    public IEnumerable<Ms2Trigger> Values => triggers.Values;

    public bool ContainsKey(int key) => triggers.ContainsKey(key);
    public bool TryGetValue(int key, [NotNullWhen(true)] out Ms2Trigger? value) => triggers.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<int, Ms2Trigger>> GetEnumerator() => triggers.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => triggers.GetEnumerator();
}

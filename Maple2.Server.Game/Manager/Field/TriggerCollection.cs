using System.Collections;
using Maple2.Model.Game;
using Maple2.Model.Metadata;

namespace Maple2.Server.Game.Manager.Field;

public sealed class TriggerCollection : IReadOnlyCollection<ITriggerObject> {
    public readonly IReadOnlyDictionary<int, TriggerObjectActor> Actors;
    public readonly IReadOnlyDictionary<int, TriggerObjectCamera> Cameras;
    public readonly IReadOnlyDictionary<int, TriggerObjectCube> Cubes;
    public readonly IReadOnlyDictionary<int, TriggerObjectEffect> Effects;
    public readonly IReadOnlyDictionary<int, TriggerObjectLadder> Ladders;
    public readonly IReadOnlyDictionary<int, TriggerObjectMesh> Meshes;
    public readonly IReadOnlyDictionary<int, TriggerObjectRope> Ropes;
    public readonly IReadOnlyDictionary<int, TriggerObjectSound> Sounds;
    public readonly IReadOnlyDictionary<int, TriggerObjectAgent> Agents;

    public readonly IReadOnlyDictionary<int, TriggerBox> Boxes;

    // These seem to get managed separately...
    // private readonly IReadOnlyDictionary<int, TriggerObjectAgent> Agents;
    // private readonly IReadOnlyDictionary<int, TriggerObjectSkill> Skills;

    public TriggerCollection(MapEntityMetadata entities) {
        Dictionary<int, TriggerObjectActor> actors = new();
        Dictionary<int, TriggerObjectCamera> cameras = new();
        Dictionary<int, TriggerObjectCube> cubes = new();
        Dictionary<int, TriggerObjectEffect> effects = new();
        Dictionary<int, TriggerObjectLadder> ladders = new();
        Dictionary<int, TriggerObjectMesh> meshes = new();
        Dictionary<int, TriggerObjectRope> ropes = new();
        Dictionary<int, TriggerObjectSound> sounds = new();
        Dictionary<int, TriggerObjectAgent> agents = new();

        foreach (Ms2TriggerActor actor in entities.Trigger.Actors) {
            actors[actor.TriggerId] = new TriggerObjectActor(actor);
        }
        foreach (Ms2TriggerCamera camera in entities.Trigger.Cameras) {
            cameras[camera.TriggerId] = new TriggerObjectCamera(camera);
        }
        foreach (Ms2TriggerCube cube in entities.Trigger.Cubes) {
            cubes[cube.TriggerId] = new TriggerObjectCube(cube);
        }
        foreach (Ms2TriggerEffect effect in entities.Trigger.Effects) {
            effects[effect.TriggerId] = new TriggerObjectEffect(effect);
        }
        foreach (Ms2TriggerLadder ladder in entities.Trigger.Ladders) {
            ladders[ladder.TriggerId] = new TriggerObjectLadder(ladder);
        }
        foreach (Ms2TriggerMesh mesh in entities.Trigger.Meshes) {
            meshes[mesh.TriggerId] = new TriggerObjectMesh(mesh);
        }
        foreach (Ms2TriggerRope rope in entities.Trigger.Ropes) {
            ropes[rope.TriggerId] = new TriggerObjectRope(rope);
        }
        foreach (Ms2TriggerSound camera in entities.Trigger.Sounds) {
            sounds[camera.TriggerId] = new TriggerObjectSound(camera);
        }

        foreach (Ms2TriggerAgent agent in entities.Trigger.Agents) {
            agents[agent.TriggerId] = new TriggerObjectAgent(agent);
        }

        Actors = actors;
        Cameras = cameras;
        Cubes = cubes;
        Effects = effects;
        Ladders = ladders;
        Meshes = meshes;
        Ropes = ropes;
        Sounds = sounds;
        Agents = agents;

        Dictionary<int, TriggerBox> boxes = new();
        foreach (Ms2TriggerBox box in entities.Trigger.Boxes) {
            boxes[box.TriggerId] = new TriggerBox(box);
        }

        Boxes = boxes;
    }

    public int Count => Actors.Count + Cameras.Count + Cubes.Count + Effects.Count + Ladders.Count + Meshes.Count + Ropes.Count + Sounds.Count + Agents.Count;

    public IEnumerator<ITriggerObject> GetEnumerator() {
        foreach (TriggerObjectActor actor in Actors.Values) yield return actor;
        foreach (TriggerObjectCamera camera in Cameras.Values) yield return camera;
        foreach (TriggerObjectCube cube in Cubes.Values) yield return cube;
        foreach (TriggerObjectEffect effect in Effects.Values) yield return effect;
        foreach (TriggerObjectLadder ladder in Ladders.Values) yield return ladder;
        foreach (TriggerObjectMesh mesh in Meshes.Values) yield return mesh;
        foreach (TriggerObjectRope rope in Ropes.Values) yield return rope;
        foreach (TriggerObjectSound sound in Sounds.Values) yield return sound;
        foreach (TriggerObjectAgent agent in Agents.Values) yield return agent;
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PlexusLineAuthoring : MonoBehaviour {
    

    private class Baker : Baker<PlexusLineAuthoring> {
        public override void Bake(PlexusLineAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddTransformUsageFlags(entity, TransformUsageFlags.NonUniformScale);
            AddComponent(entity, new PlexusLineData {  });
            AddComponent(entity, new PostTransformMatrix { });
        }
    }
}

public struct PlexusLineData : IComponentData {
    public int Position1Id;
    public int Position2Id;
    public float MaxLengthPercent;
    public float Length;
}


using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace WireframePlexus {

    public class EdgeDataAuthoring : MonoBehaviour {

        private class Baker : Baker<EdgeDataAuthoring> {
            public override void Bake(EdgeDataAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddTransformUsageFlags(entity, TransformUsageFlags.NonUniformScale);
                AddComponent(entity, new EdgeData { });
                AddComponent(entity, new PostTransformMatrix { });
                AddComponent(entity, new EdgeAlphaData {Value= 0});
            }
        }
    }
}
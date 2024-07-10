using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace WireframePlexus {

    public class EdgeEntityAuthoring : MonoBehaviour {

        private class Baker : Baker<EdgeEntityAuthoring> {
            public override void Bake(EdgeEntityAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddTransformUsageFlags(entity, TransformUsageFlags.NonUniformScale);
                AddComponent(entity, new EdgeData { });
                AddComponent(entity, new PostTransformMatrix { });
                AddComponent(entity, new EdgeAlphaData {Value= 0});
                AddComponent(entity, new EdgeColorData { });
            }
        }
    }
}
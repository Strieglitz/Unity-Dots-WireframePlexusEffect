using Unity.Entities;
using UnityEngine;

namespace WireframePlexus {

public class VertexEntityAuthoring : MonoBehaviour {

        private class Baker : Baker<VertexEntityAuthoring> {
            public override void Bake(VertexEntityAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new VertexMovementData { });
                AddComponent(entity, new VertexColorData { });
            }
        }
    }
}
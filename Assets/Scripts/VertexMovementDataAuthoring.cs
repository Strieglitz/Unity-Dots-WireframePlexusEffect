using Unity.Entities;
using UnityEngine;

namespace WireframePlexus {

public class VertexMovementDataAuthoring : MonoBehaviour {

        private class Baker : Baker<VertexMovementDataAuthoring> {
            public override void Bake(VertexMovementDataAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new VertexMovementData { });
            }
        }
    }
}
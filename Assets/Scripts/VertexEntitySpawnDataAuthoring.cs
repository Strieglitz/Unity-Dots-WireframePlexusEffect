using Unity.Entities;
using UnityEngine;

namespace WireframePlexus {

public class VertexEntitySpawnDataAuthoring : MonoBehaviour {
        public GameObject WireframePlexusVertexEntityPrefab;

        public class Baker : Baker<VertexEntitySpawnDataAuthoring> {
            public override void Bake(VertexEntitySpawnDataAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new VertexEntitySpawnData { WireframePlexusVertexEntityPrefab = GetEntity(authoring.WireframePlexusVertexEntityPrefab, TransformUsageFlags.Dynamic) });
            }
        }
    }

    public struct VertexEntitySpawnData : IComponentData {
        public Entity WireframePlexusVertexEntityPrefab;
    }
}
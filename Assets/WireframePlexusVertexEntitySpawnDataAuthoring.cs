using Unity.Entities;
using UnityEngine;

public class WireframePlexusVertexEntitySpawnDataAuthoring : MonoBehaviour
{
    public GameObject PlexusPointPrefab;

    public class Baker : Baker<WireframePlexusVertexEntitySpawnDataAuthoring> {
        public override void Bake(WireframePlexusVertexEntitySpawnDataAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new WireframePlexusVertexEntitySpawnData { PlexusPointPrefabEntity = GetEntity(authoring.PlexusPointPrefab, TransformUsageFlags.Dynamic) });
        }
    }
}

public struct WireframePlexusVertexEntitySpawnData : IComponentData {
    public Entity PlexusPointPrefabEntity;
}

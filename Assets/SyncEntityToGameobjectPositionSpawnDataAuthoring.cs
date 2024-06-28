using Unity.Entities;
using UnityEngine;

public class SyncEntityToGameobjectPositionSpawnDataAuthoring : MonoBehaviour
{
    public GameObject SyncEntityToGameobjectPositionPrefab;
    public class Baker : Baker<SyncEntityToGameobjectPositionSpawnDataAuthoring> {
        public override void Bake(SyncEntityToGameobjectPositionSpawnDataAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SyncEntityToGameobjectPositionSpawnData { SyncEntityToGameobjectPositionPrefabEntity = GetEntity(authoring.SyncEntityToGameobjectPositionPrefab, TransformUsageFlags.Dynamic) });
        }
    }
}

public struct SyncEntityToGameobjectPositionSpawnData : IComponentData {
    public Entity SyncEntityToGameobjectPositionPrefabEntity;
}

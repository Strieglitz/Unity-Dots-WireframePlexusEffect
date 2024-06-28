using Unity.Entities;
using UnityEngine;

public class PlexusLineSpawnAuthoring : MonoBehaviour { 
    public GameObject PlexusLinePrefab;

    public class Baker : Baker<PlexusLineSpawnAuthoring> {
        public override void Bake(PlexusLineSpawnAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new PlexusLineSpawnData { PlexusLinePrefabEntity = GetEntity(authoring.PlexusLinePrefab, TransformUsageFlags.Dynamic) });
        }
    }
}

public struct PlexusLineSpawnData : IComponentData {
    public Entity PlexusLinePrefabEntity;
}

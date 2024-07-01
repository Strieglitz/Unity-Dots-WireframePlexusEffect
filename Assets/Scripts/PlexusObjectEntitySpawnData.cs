using Unity.Entities;

namespace WireframePlexus {

    public struct PlexusObjectEntitySpawnData : IComponentData {
        public Entity WireframePlexusEntityPrefab;
    }
}
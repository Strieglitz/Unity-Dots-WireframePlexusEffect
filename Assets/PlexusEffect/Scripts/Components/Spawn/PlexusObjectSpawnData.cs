using Unity.Entities;

namespace WireframePlexus {

    public struct PlexusObjectSpawnData : IComponentData {
        public Entity WireframePlexusEntityPrefab;
    }
}
using Unity.Entities;

namespace WireframePlexus {

    public struct VertexSpawnData : IComponentData {
        public Entity WireframePlexusVertexEntityPrefab;
    }
}
using Unity.Entities;

namespace WireframePlexus {

	public struct EdgeSpawnData : IComponentData {
		public Entity WireframePlexusEdgeEntityPrefab;
	}
}
using Unity.Entities;
using Unity.Mathematics;

namespace WireframePlexus {

	public struct VertexAdditionalMovementData : IComponentData {
		public float3 AdditionalLocalPosition;
		public int PointId;
	}
}
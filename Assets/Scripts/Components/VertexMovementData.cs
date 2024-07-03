using Unity.Entities;
using Unity.Mathematics;

namespace WireframePlexus {

	public struct VertexMovementData : IComponentData {
		public float MoveSpeed;
		public float3 Position;
		public float3 PositionOrigin;
		public float3 PositionTarget;
		public float TotalMovementDuration;
		public float CurrentMovementDuration;
		public Unity.Mathematics.Random Random;
		public int PointId;
	}
}
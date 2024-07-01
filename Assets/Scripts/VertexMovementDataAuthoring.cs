using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace WireframePlexus {


public class VertexMovementDataAuthoring : MonoBehaviour {

        private class Baker : Baker<VertexMovementDataAuthoring> {
            public override void Bake(VertexMovementDataAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new VertexMovementData { });
            }
        }
    }

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
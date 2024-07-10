using Unity.Entities;
using Unity.Mathematics;

namespace WireframePlexus {

    public struct ContactColorAnimationData : IComponentData {
        public float3 LocalContactPosition;
        public float3 LocalContactNormal;
        public float ContactForce;
        public float4 ContactColor;
        public float TotalContactDuration;
        public float CurrentContactDuration;
    }
}
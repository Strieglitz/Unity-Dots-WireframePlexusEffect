using Unity.Entities;
using Unity.Mathematics;

namespace WireframePlexus {

    public struct ContactEffectData {
        public float3 LocalContactPosition;
        public float ContactRadius;
        public float4 ContactColor;
        public float TotalContactDuration;
        public float CurrentContactDuration;
    }
}
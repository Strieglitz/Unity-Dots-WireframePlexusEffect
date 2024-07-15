using Unity.Entities;
using Unity.Mathematics;

namespace WireframePlexus {

    public struct ContactEffectData {
        public EaseType EasingType;
        public float3 ContactWorldPosition;
        public float ContactRadius;
        public float4 ContactColor;
        public float TotalContactDuration;
        public float CurrentContactDuration;
        public float ContactVertexMaxDistance;
        public float ContactVertexDurationMultiplier; // this one has to be >= 1
    }
}
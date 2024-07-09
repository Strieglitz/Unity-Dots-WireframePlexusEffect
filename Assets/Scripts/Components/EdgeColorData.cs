using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace WireframePlexus {

    [MaterialProperty("_Color")]
    public struct EdgeColorData : IComponentData {
        public float4 Value;
    }
}

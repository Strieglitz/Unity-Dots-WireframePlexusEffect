using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace WireframePlexus {

    [MaterialProperty("_Color")]
    public struct VertexColorData : IComponentData {
        public float4 Value;
    }
}
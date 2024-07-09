using Unity.Entities;
using Unity.Rendering;

namespace WireframePlexus {

    [MaterialProperty("_Alpha")]
    public struct EdgeAlphaData : IComponentData {
        public float Value;
    }
}
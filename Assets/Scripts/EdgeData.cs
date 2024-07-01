using Unity.Entities;

namespace WireframePlexus {

    public struct EdgeData : IComponentData {
    public int Vertex1Index;
    public int Vertex2Index;
    public float Length;
}
}
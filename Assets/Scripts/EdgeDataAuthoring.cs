using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace WireframePlexus {

    public class EdgeDataAuthoring : MonoBehaviour {


        private class Baker : Baker<EdgeDataAuthoring> {
            public override void Bake(EdgeDataAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddTransformUsageFlags(entity, TransformUsageFlags.NonUniformScale);
                AddComponent(entity, new EdgeData { });
                AddComponent(entity, new PostTransformMatrix { });
            }
        }
    }

    public struct EdgeData : IComponentData {
        public int Vertex1Index;
        public int Vertex2Index;
        public float Length;
    }
}
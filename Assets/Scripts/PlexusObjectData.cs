using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace WireframePlexus {

    [ChunkSerializable]
    public struct PlexusObjectData : IComponentData, IDisposable {
        public NativeArray<float3> VertexPositions;
        public float MaxVertexMoveSpeed;
        public float MinVertexMoveSpeed;
        public float MaxVertexMoveDistance;
        public float MaxEdgeLengthPercent;
        public int WireframePlexusObjectId;

        public void Dispose() {
            VertexPositions.Dispose();
        }
    }
}
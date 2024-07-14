using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace WireframePlexus {

    [ChunkSerializable]
    public struct PlexusObjectData : IComponentData, IDisposable {
        public float MaxEdgeLengthPercent;
        public float EdgeThickness;
        public float VertexSize;
        public float MaxVertexMoveDistance;
        public float MinVertexMoveSpeed;
        public float MaxVertexMoveSpeed;
        public float4 VertexColor;
        public float4 EdgeColor;
        public NativeArray<float3> VertexPositions;
        public NativeList<ContactEffectData> ContactAnimationColorData;
        public int WireframePlexusObjectId;
        public float3 WorldPosition;
        public quaternion WorldRotation;

        public void Dispose() {
            VertexPositions.Dispose();
            ContactAnimationColorData.Dispose();
        }
    }
}
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


    public struct WireframePlexusEntityData : IComponentData, IDisposable {
    public NativeArray<float3> VertexPositions;
    public float MaxVertexMoveSpeed;
    public float MinVertexMoveSpeed;
    public float MaxVertexMoveDistance;

    public void Dispose() {
        VertexPositions.Dispose();
    }
}
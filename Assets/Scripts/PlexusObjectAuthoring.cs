using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace WireframePlexus {
public class PlexusObjectAuthoring : MonoBehaviour
{
    private class Baker : Baker<PlexusObjectAuthoring> {
        public override void Bake(PlexusObjectAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlexusObjectData {  } );
            AddComponentObject(entity, new SyncEntityPositionToGameobjectData { });
        }
    }
}

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
public class SyncEntityPositionToGameobjectData : IComponentData {
    public GameObject gameObject;
}
}
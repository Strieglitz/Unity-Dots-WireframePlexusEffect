using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PlexusPointMovementDataAuthoring : MonoBehaviour
{

    private class Baker : Baker<PlexusPointMovementDataAuthoring> {
        public override void Bake(PlexusPointMovementDataAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlexusPointMovementData { Random = new Unity.Mathematics.Random((uint) UnityEngine.Random.Range(0,int.MaxValue))});
        }
    }
}

public struct PlexusPointMovementData : IComponentData {
    public float MoveSpeed;
    public float3 Position;
    public float3 PositionOrigin;
    public float3 PositionTarget;
    public float TotalMovementDuration;
    public float CurrentMovementDuration;
    public Unity.Mathematics.Random Random;
    public int PointId;
}


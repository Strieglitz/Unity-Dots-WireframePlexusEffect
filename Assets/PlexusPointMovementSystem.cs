using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


[UpdateAfter(typeof(SyncEntityToGameobjectPositionSystem))]
public partial struct PlexusPointMovementSystem : ISystem {

    EntityQuery plexusObjectEntityQuery;
    EntityQuery plexusPointsByPlexusObjectIdEntityQuery;


    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<PlexusPointMovementData>();
        plexusObjectEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<WireframePlexusEntityData>().Build(ref state);
        plexusPointsByPlexusObjectIdEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, PlexusPointMovementData, PlexusObjectIdData>().Build(ref state);

    }


    public void OnUpdate(ref SystemState state) {
        var plexusObjectEntries = plexusObjectEntityQuery.ToEntityArray(Allocator.Temp);
        foreach (Entity plexusObject in plexusObjectEntries) {
            var plexusObjectId = state.EntityManager.GetSharedComponent<PlexusObjectIdData>(plexusObject);
            var plexusObjectData = state.EntityManager.GetComponentData<WireframePlexusEntityData>(plexusObject);
            plexusPointsByPlexusObjectIdEntityQuery.SetSharedComponentFilter(plexusObjectId);


            PlexusPointMovementJob job = new PlexusPointMovementJob { DeltaTime = SystemAPI.Time.DeltaTime, 
                                                                        PointPositions = plexusObjectData.VertexPositions,
                                                                        MaxVertexMoveDistance = plexusObjectData.MaxVertexMoveDistance,
                                                                        MaxVertexMovementSpeed = plexusObjectData.MaxVertexMoveSpeed,
                                                                        MinVertexMovementSpeed = plexusObjectData.MinVertexMoveSpeed};

            job.ScheduleParallel(plexusPointsByPlexusObjectIdEntityQuery);
        }


    }

    [BurstCompile]
    public partial struct PlexusPointMovementJob : IJobEntity {

        [ReadOnly] public float DeltaTime;
        [ReadOnly] public float MinVertexMovementSpeed;
        [ReadOnly] public float MaxVertexMovementSpeed;
        [ReadOnly] public float MaxVertexMoveDistance;
        [NativeDisableContainerSafetyRestriction] public NativeArray<float3> PointPositions;
        
        

        public void Execute(ref LocalTransform localTransform, ref PlexusPointMovementData movementData) {
            if (movementData.CurrentMovementDuration <= 0) {
                localTransform = localTransform.WithPosition(movementData.PositionTarget);
                movementData.MoveSpeed = movementData.Random.NextFloat(MinVertexMovementSpeed,MaxVertexMovementSpeed);
                movementData.PositionTarget = movementData.Position + movementData.Random.NextFloat3(-MaxVertexMoveDistance, MaxVertexMoveDistance);
                movementData.CurrentMovementDuration = math.distance(movementData.PositionTarget, movementData.PositionOrigin) / movementData.MoveSpeed;
                movementData.TotalMovementDuration = movementData.CurrentMovementDuration;
            } else {
                movementData.CurrentMovementDuration -= DeltaTime;
                float interpolationPercent = 1 - (movementData.CurrentMovementDuration / movementData.TotalMovementDuration);
                float3 newPosition = math.lerp(movementData.PositionOrigin, movementData.PositionTarget, interpolationPercent);
                localTransform = localTransform.WithPosition(newPosition);
            }
            PointPositions[movementData.PointId] = localTransform.Position;
        }
    }
}

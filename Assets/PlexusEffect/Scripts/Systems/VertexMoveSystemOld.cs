using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


namespace WireframePlexus {

    [UpdateAfter(typeof(PlexusObjectSystem))]
    public partial struct VertexMoveSystemOld : ISystem {

        EntityQuery plexusVertexByPlexusObjectIdEntityQuery;
        EntityQuery plexusObjectEntityQuery;

        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<VertexMovementData>();
            plexusObjectEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PlexusObjectData>().Build(ref state);
            plexusVertexByPlexusObjectIdEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<LocalTransform, VertexMovementData>().WithAll<LocalToWorld, PlexusObjectIdData>().Build(ref state);
            
        }
        public void OnDestroy(ref SystemState state) {
            
        }

        public void OnUpdate(ref SystemState state) {
            return;
            var plexusObjectEntries = plexusObjectEntityQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity plexusObject in plexusObjectEntries) {
                var plexusObjectData = state.EntityManager.GetComponentData<PlexusObjectData>(plexusObject);
                plexusVertexByPlexusObjectIdEntityQuery.ResetFilter();
                plexusVertexByPlexusObjectIdEntityQuery.SetSharedComponentFilter(new PlexusObjectIdData { PlexusObjectId = plexusObjectData.WireframePlexusObjectId});
                
                new PlexusVertexMovementJobOld {
                    DeltaTime = SystemAPI.Time.DeltaTime,
                    VertexPositions = plexusObjectData.VertexPositions,
                    MaxVertexMoveDistance = plexusObjectData.MaxVertexMoveDistance,
                    MaxVertexMovementSpeed = plexusObjectData.MaxVertexMoveSpeed,
                    MinVertexMovementSpeed = plexusObjectData.MinVertexMoveSpeed,
                    CameraWolrdPos = (float3)Camera.main.transform.position,
                }.ScheduleParallel(plexusVertexByPlexusObjectIdEntityQuery);
            }
        }

        [BurstCompile]

        public partial struct PlexusVertexMovementJobOld : IJobEntity {

            [ReadOnly] public quaternion ParentRotation;
            [ReadOnly] public float3 CameraWolrdPos;
            [ReadOnly] public float DeltaTime;
            [ReadOnly] public float MinVertexMovementSpeed;
            [ReadOnly] public float MaxVertexMovementSpeed;
            [ReadOnly] public float MaxVertexMoveDistance;
            [NativeDisableContainerSafetyRestriction] public NativeArray<float3> VertexPositions;

            public void Execute(ref LocalTransform localTransform, ref VertexMovementData movementData, in LocalToWorld localToWorld) {

                // calc vertex new position depending on the movement data and write the current pos to the nativeArray of vertex positions
                if ((MinVertexMovementSpeed == 0 && MaxVertexMovementSpeed == 0) || MaxVertexMoveDistance == 0) {
                    localTransform.Position = movementData.Position;
                    VertexPositions[movementData.PointId] = localTransform.Position;

                } else {
                    if (movementData.CurrentMovementDuration <= 0) {
                        localTransform = localTransform.WithPosition(movementData.PositionTarget);
                        movementData.MoveSpeed = movementData.Random.NextFloat(MinVertexMovementSpeed, MaxVertexMovementSpeed);
                        movementData.PositionOrigin = movementData.PositionTarget;
                        movementData.PositionTarget = movementData.Position + movementData.Random.NextFloat3(-MaxVertexMoveDistance, MaxVertexMoveDistance);
                        movementData.CurrentMovementDuration = math.distance(movementData.PositionTarget, movementData.PositionOrigin) / movementData.MoveSpeed;
                        movementData.TotalMovementDuration = movementData.CurrentMovementDuration;
                    } else {
                        movementData.CurrentMovementDuration -= DeltaTime;
                        float interpolationPercent = 1 - (movementData.CurrentMovementDuration / movementData.TotalMovementDuration);
                        float3 newPosition = math.lerp(movementData.PositionOrigin, movementData.PositionTarget, interpolationPercent);
                        localTransform = localTransform.WithPosition(newPosition);
                    }
                    VertexPositions[movementData.PointId] = localTransform.Position;
                }
            }
        }
    }
}
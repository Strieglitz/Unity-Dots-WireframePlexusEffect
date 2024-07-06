using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


namespace WireframePlexus {

[UpdateAfter(typeof(SyncEntityToGameobjectPositionSystem))]
    public partial struct VertexMoveSystem : ISystem {

        EntityQuery plexusObjectEntityQuery;
        EntityQuery plexusPointsByPlexusObjectIdEntityQuery;

        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<VertexMovementData>();
            plexusObjectEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PlexusObjectData>().Build(ref state);
            plexusPointsByPlexusObjectIdEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, VertexMovementData, LocalToWorld, PlexusObjectIdData>().Build(ref state);
        }

        public void OnUpdate(ref SystemState state) {
            var plexusObjectEntries = plexusObjectEntityQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity plexusObject in plexusObjectEntries) {
                var plexusObjectData = state.EntityManager.GetComponentData<PlexusObjectData>(plexusObject);
                var plexusObjectTransform = state.EntityManager.GetComponentData<LocalTransform>(plexusObject);
                plexusPointsByPlexusObjectIdEntityQuery.SetSharedComponentFilter(new PlexusObjectIdData { ObjectId = plexusObjectData.WireframePlexusObjectId });

                PlexusPointMovementJob job = new PlexusPointMovementJob {
                    DeltaTime = SystemAPI.Time.DeltaTime,
                    PointPositions = plexusObjectData.VertexPositions,
                    MaxVertexMoveDistance = plexusObjectData.MaxVertexMoveDistance,
                    MaxVertexMovementSpeed = plexusObjectData.MaxVertexMoveSpeed,
                    MinVertexMovementSpeed = plexusObjectData.MinVertexMoveSpeed,
                    CameraWolrdPos = (float3)Camera.main.transform.position,
                    ParentRotation = plexusObjectTransform.Rotation
                };

                job.ScheduleParallel(plexusPointsByPlexusObjectIdEntityQuery);
            }
        }

        [BurstCompile]
        public partial struct PlexusPointMovementJob : IJobEntity {


            [ReadOnly] public quaternion ParentRotation;
            [ReadOnly] public float3 CameraWolrdPos;
            [ReadOnly] public float DeltaTime;
            [ReadOnly] public float MinVertexMovementSpeed;
            [ReadOnly] public float MaxVertexMovementSpeed;
            [ReadOnly] public float MaxVertexMoveDistance;
            [NativeDisableContainerSafetyRestriction] public NativeArray<float3> PointPositions;

            public void Execute(ref LocalTransform localTransform, ref VertexMovementData movementData, in LocalToWorld localToWorld) {

                
                if ((MinVertexMovementSpeed == 0 && MaxVertexMovementSpeed == 0) || MaxVertexMoveDistance == 0) {
                    localTransform.Position = movementData.Position;
                    PointPositions[movementData.PointId] = localTransform.Position;

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
                    PointPositions[movementData.PointId] = localTransform.Position;
                }

                // make vertex face camera
                float3 relativePos = CameraWolrdPos - localToWorld.Position;
                UnityEngine.Debug.Log(relativePos);
                // quaternion.LookRotationSafe cannot handle vectors that are collinear so for the case of the edge faceing directly up or down hardcoded a 90 degree rotation
                if (relativePos.y == 1 || relativePos.y == -1) {
                    localTransform.Rotation = math.mul(quaternion.RotateX(math.PIHALF) , math.inverse(ParentRotation));
                } else {
                    quaternion end = quaternion.LookRotationSafe(-relativePos, math.up());
                    localTransform.Rotation = math.mul(end.value, math.inverse(ParentRotation));
                }

            }
        }
    }
}
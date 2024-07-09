using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


namespace WireframePlexus {

    [UpdateAfter(typeof(SyncEntityToGameobjectPositionSystem))]
    public partial struct VertexMoveSystem : ISystem {

        EntityQuery plexusObjectEntityQuery;
        EntityQuery plexusPointsByPlexusObjectIdEntityQuery;
        NativeArray<VertexMovementJobData> vertexMovementJobDatas;

        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<VertexMovementData>();
            vertexMovementJobDatas = new NativeArray<VertexMovementJobData>(0, Allocator.Persistent);
            plexusObjectEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PlexusObjectData>().Build(ref state);
            plexusPointsByPlexusObjectIdEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<LocalTransform, VertexMovementData>().WithAll<LocalToWorld, PlexusObjectIdData>().Build(ref state);
        }
        public void OnDestroy(ref SystemState state) {
            vertexMovementJobDatas.Dispose();
        }

        public void OnUpdate(ref SystemState state) {
            var plexusObjectEntities = plexusObjectEntityQuery.ToEntityArray(Allocator.Temp);
            if (vertexMovementJobDatas.Length != plexusObjectEntities.Length) {
                vertexMovementJobDatas.Dispose();
                vertexMovementJobDatas = new NativeArray<VertexMovementJobData>(plexusObjectEntities.Length, Allocator.Persistent);
            }

            // prepare the data for the jobs ( cannot read from localtransform in the onUpdate and write to a differnt localtransform in the job because of ecs saftey checks)
            // so read the rotation from the localTransform first and store it together with the plexusObjectData and run the jobs later
            for (int i = 0; i < plexusObjectEntities.Length; i++) {
                var plexusObjectTransform = state.EntityManager.GetComponentData<LocalTransform>(plexusObjectEntities[i]);
                var plexusObjectData = state.EntityManager.GetComponentData<PlexusObjectData>(plexusObjectEntities[i]);
                vertexMovementJobDatas[i] = new VertexMovementJobData { plexusObjectData = plexusObjectData, parentRotation = plexusObjectTransform.Rotation.value };
            }

            // run the jobs
            for (int i = 0; i < plexusObjectEntities.Length; i++) {
                plexusPointsByPlexusObjectIdEntityQuery.ResetFilter();
                plexusPointsByPlexusObjectIdEntityQuery.SetSharedComponentFilter(new PlexusObjectIdData { ObjectId = vertexMovementJobDatas[i].plexusObjectData.WireframePlexusObjectId });
                new PlexusVertexMovementJob {
                    DeltaTime = SystemAPI.Time.DeltaTime,
                    VertexPositions = vertexMovementJobDatas[i].plexusObjectData.VertexPositions,
                    MaxVertexMoveDistance = vertexMovementJobDatas[i].plexusObjectData.MaxVertexMoveDistance,
                    MaxVertexMovementSpeed = vertexMovementJobDatas[i].plexusObjectData.MaxVertexMoveSpeed,
                    MinVertexMovementSpeed = vertexMovementJobDatas[i].plexusObjectData.MinVertexMoveSpeed,
                    CameraWolrdPos = (float3)Camera.main.transform.position,
                    ParentRotation = vertexMovementJobDatas[i].parentRotation
                }.ScheduleParallel(plexusPointsByPlexusObjectIdEntityQuery);
            }
        }

        struct VertexMovementJobData {
            public PlexusObjectData plexusObjectData;
            public quaternion parentRotation;
        }

        [BurstCompile]

        public partial struct PlexusVertexMovementJob : IJobEntity {


            [ReadOnly] public quaternion ParentRotation;
            [ReadOnly] public float3 CameraWolrdPos;
            [ReadOnly] public float DeltaTime;
            [ReadOnly] public float MinVertexMovementSpeed;
            [ReadOnly] public float MaxVertexMovementSpeed;
            [ReadOnly] public float MaxVertexMoveDistance;
            [NativeDisableContainerSafetyRestriction] public NativeArray<float3> VertexPositions;

            public void Execute(ref LocalTransform localTransform, ref VertexMovementData movementData, in LocalToWorld localToWorld) {


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

                // make vertex face camera
                float3 relativePos = CameraWolrdPos - localToWorld.Position;
                // quaternion.LookRotationSafe cannot handle vectors that are collinear so for the case of the edge faceing directly up or down hardcoded a 90 degree rotation
                if (relativePos.y == 1 || relativePos.y == -1) {
                    localTransform.Rotation = math.mul(quaternion.RotateX(math.PIHALF), math.inverse(ParentRotation));
                } else {
                    quaternion end = quaternion.LookRotationSafe(-relativePos, math.up());
                    localTransform.Rotation = math.mul(end.value, math.inverse(ParentRotation));
                }

            }
        }
    }
}
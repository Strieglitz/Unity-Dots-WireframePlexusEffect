using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;


namespace WireframePlexus {

    [UpdateAfter(typeof(PlexusObjectSystem))]
    public partial struct VertexMoveSystem : ISystem {

        EntityQuery plexusObjectEntityQuery;
        EntityQuery vertexEntityQuery;
        SharedComponentTypeHandle<PlexusObjectIdData> idTypeHandle;
        NativeHashMap<int, PlexusObjectData> plexusObjectDataById;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<VertexMovementData>();
            plexusObjectEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PlexusObjectData>().Build(ref state);
            vertexEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<LocalTransform, VertexMovementData>().WithAll<PlexusObjectIdData, VertexAdditionalMovementData>().Build(ref state);
            idTypeHandle = state.GetSharedComponentTypeHandle<PlexusObjectIdData>();
            plexusObjectDataById = new NativeHashMap<int, PlexusObjectData>(0, Allocator.Persistent);
        }


        public void OnUpdate(ref SystemState state) {
            idTypeHandle.Update(ref state);
            var plexusObjectEntries = plexusObjectEntityQuery.ToEntityArray(Allocator.Temp);

            if(plexusObjectDataById.Count != plexusObjectEntries.Length) {
                plexusObjectDataById.Dispose();
                plexusObjectDataById = new NativeHashMap<int, PlexusObjectData>(plexusObjectEntries.Length, Allocator.Persistent);
            }

            foreach (Entity plexusObject in plexusObjectEntries) {
                var plexusObjectData = state.EntityManager.GetComponentData<PlexusObjectData>(plexusObject);
                plexusObjectDataById[plexusObjectData.WireframePlexusObjectId] = plexusObjectData;
            }

            new PlexusVertexMovementJob { DeltaTime = SystemAPI.Time.DeltaTime, CameraWolrdPos = (float3)Camera.main.transform.position, PlexusObjectDataById = plexusObjectDataById, IdTypeHandle = idTypeHandle }.ScheduleParallel(vertexEntityQuery);
        }
    }

    [BurstCompile]
    public partial struct PlexusVertexMovementJob : IJobEntity, IJobEntityChunkBeginEnd {

        [ReadOnly] public float DeltaTime;
        [ReadOnly] public float3 CameraWolrdPos;
        [NativeDisableContainerSafetyRestriction][ReadOnly] public NativeHashMap<int, PlexusObjectData> PlexusObjectDataById;

        public SharedComponentTypeHandle<PlexusObjectIdData> IdTypeHandle;

        int plexusObjectId;



        public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
            plexusObjectId = chunk.GetSharedComponent(IdTypeHandle).PlexusObjectId;
            return true;
        }

        public void Execute(ref LocalTransform localTransform, ref VertexMovementData movementData, in VertexAdditionalMovementData vertexAdditionalMovementData) {
            PlexusObjectData plexusObjectData = PlexusObjectDataById[plexusObjectId];

            // calc vertex new position depending on the movement data and write the current pos to the nativeArray of vertex positions
            if ((plexusObjectData.MinVertexMoveSpeed == 0 && plexusObjectData.MaxVertexMoveSpeed == 0) || plexusObjectData.MaxVertexMoveDistance == 0) {
                localTransform.Position = movementData.Position + vertexAdditionalMovementData.AdditionalLocalPosition;
                plexusObjectData.VertexPositions[movementData.PointId] = localTransform.Position;

            } else {
                if (movementData.CurrentMovementDuration <= 0) {
                    localTransform.Position = movementData.PositionTarget + vertexAdditionalMovementData.AdditionalLocalPosition;
                    movementData.MoveSpeed = movementData.Random.NextFloat(plexusObjectData.MinVertexMoveSpeed, plexusObjectData.MaxVertexMoveSpeed);
                    movementData.PositionOrigin = movementData.PositionTarget;
                    movementData.PositionTarget = movementData.Position + movementData.Random.NextFloat3(-plexusObjectData.MaxVertexMoveDistance, plexusObjectData.MaxVertexMoveDistance);
                    movementData.CurrentMovementDuration = math.distance(movementData.PositionTarget, movementData.PositionOrigin) / movementData.MoveSpeed;
                    movementData.TotalMovementDuration = movementData.CurrentMovementDuration;
                } else {
                    movementData.CurrentMovementDuration -= DeltaTime;
                    float interpolationPercent = 1 - (movementData.CurrentMovementDuration / movementData.TotalMovementDuration);
                    float3 newPosition = math.lerp(movementData.PositionOrigin, movementData.PositionTarget, interpolationPercent);
                    localTransform.Position = newPosition + vertexAdditionalMovementData.AdditionalLocalPosition;
                }
                plexusObjectData.VertexPositions[movementData.PointId] = localTransform.Position;
            }
        }

        public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted) {

        }
    }
}

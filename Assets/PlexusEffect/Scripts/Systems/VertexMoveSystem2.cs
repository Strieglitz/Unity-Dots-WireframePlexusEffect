using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


namespace WireframePlexus {

    [UpdateAfter(typeof(PlexusObjectSystem))]
    public partial struct VertexMoveSystem2 : ISystem {

        EntityQuery plexusObjectEntityQuery;
        EntityQuery plexusVertexEntityQuery;
        NativeHashMap<int, PlexusObjectData> plexusObjectDataById;
        SharedComponentTypeHandle<PlexusObjectIdData> idTypeHandle;

       [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<VertexMovementData>();
            plexusObjectEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PlexusObjectData>().Build(ref state);
            plexusVertexEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<LocalTransform, VertexMovementData>().WithAll<LocalToWorld, PlexusObjectIdData>().Build(ref state);
            plexusObjectDataById = new NativeHashMap<int, PlexusObjectData>(0,Allocator.Persistent);
            idTypeHandle = state.GetSharedComponentTypeHandle<PlexusObjectIdData>();
        }
        public void OnDestroy(ref SystemState state) {
            plexusObjectDataById.Dispose();
        }

        public void OnUpdate(ref SystemState state) {
            idTypeHandle.Update(ref state);
            var plexusObjectEntries = plexusObjectEntityQuery.ToEntityArray(Allocator.Temp);
            if(plexusObjectEntries.Length != plexusObjectDataById.Count) {
                plexusObjectDataById.Dispose();
                plexusObjectDataById = new NativeHashMap<int, PlexusObjectData>(plexusObjectEntries.Length, Allocator.Persistent);
                foreach (Entity plexusObject in plexusObjectEntries) {
                    var plexusObjectData = state.EntityManager.GetComponentData<PlexusObjectData>(plexusObject);
                    plexusObjectDataById.Add(plexusObjectData.WireframePlexusObjectId, plexusObjectData);
                }
            }
            new PlexusVertexMovementJob2 { DeltaTime = SystemAPI.Time.DeltaTime, CameraWolrdPos = (float3)Camera.main.transform.position, PlexusObjectDataById = plexusObjectDataById, IdTypeHandle = idTypeHandle}.ScheduleParallel(plexusVertexEntityQuery);
        }
    }

    [BurstCompile]
    public partial struct PlexusVertexMovementJob2 : IJobEntity, IJobEntityChunkBeginEnd {

        [ReadOnly] public float DeltaTime;
        [ReadOnly] public float3 CameraWolrdPos;
        [NativeDisableContainerSafetyRestriction][ReadOnly] public NativeHashMap<int, PlexusObjectData> PlexusObjectDataById;

        public SharedComponentTypeHandle<PlexusObjectIdData> IdTypeHandle;
        
        int plexusObjectId;



        public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
            plexusObjectId = chunk.GetSharedComponent(IdTypeHandle).PlexusObjectId;
            return true;
        }

        public void Execute(ref LocalTransform localTransform, ref VertexMovementData movementData, in LocalToWorld localToWorld) {
            PlexusObjectData plexusObjectData = PlexusObjectDataById[plexusObjectId];
            
            // calc vertex new position depending on the movement data and write the current pos to the nativeArray of vertex positions
            if ((plexusObjectData.MinVertexMoveSpeed == 0 && plexusObjectData.MaxVertexMoveSpeed == 0) || plexusObjectData.MaxVertexMoveDistance == 0) {
                localTransform.Position = movementData.Position;
                plexusObjectData.VertexPositions[movementData.PointId] = localTransform.Position;

            } else {
                if (movementData.CurrentMovementDuration <= 0) {
                    localTransform = localTransform.WithPosition(movementData.PositionTarget);
                    movementData.MoveSpeed = movementData.Random.NextFloat(plexusObjectData.MinVertexMoveSpeed, plexusObjectData.MaxVertexMoveSpeed);
                    movementData.PositionOrigin = movementData.PositionTarget;
                    movementData.PositionTarget = movementData.Position + movementData.Random.NextFloat3(-plexusObjectData.MaxVertexMoveSpeed, plexusObjectData.MaxVertexMoveDistance);
                    movementData.CurrentMovementDuration = math.distance(movementData.PositionTarget, movementData.PositionOrigin) / movementData.MoveSpeed;
                    movementData.TotalMovementDuration = movementData.CurrentMovementDuration;
                } else {
                    movementData.CurrentMovementDuration -= DeltaTime;
                    float interpolationPercent = 1 - (movementData.CurrentMovementDuration / movementData.TotalMovementDuration);
                    float3 newPosition = math.lerp(movementData.PositionOrigin, movementData.PositionTarget, interpolationPercent);
                    localTransform = localTransform.WithPosition(newPosition);
                }
                plexusObjectData.VertexPositions[movementData.PointId] = localTransform.Position;
            }

            // make vertex face camera
            float3 relativePos = CameraWolrdPos - localToWorld.Position;
            // quaternion.LookRotationSafe cannot handle vectors that are collinear so for the case of the edge faceing directly up or down hardcoded a 90 degree rotation
            if (relativePos.y == 1 || relativePos.y == -1) {
                localTransform.Rotation = math.mul(quaternion.RotateX(math.PIHALF), math.inverse(plexusObjectData.Rotation));
            } else {
                quaternion end = quaternion.LookRotationSafe(-relativePos, math.up());
                localTransform.Rotation = math.mul(end.value, math.inverse(plexusObjectData.Rotation));
            }
        }

        public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted) {
            
        }
    }
}

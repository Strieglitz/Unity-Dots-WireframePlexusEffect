using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Transforms;



namespace WireframePlexus {

    [UpdateBefore(typeof(ContactEffectAnimationSystem))]
    [UpdateAfter(typeof(PlexusObjectRecieveUpdatedDataSystem))]
    public partial struct PlexusObjectUpdateDataSystem : ISystem {

        EntityQuery plexusObjectEntityQuery;
        EntityQuery vertexEntityQuery;
        EntityQuery edgeEntityQuery;
        SharedComponentTypeHandle<PlexusObjectIdData> idTypeHandle;
        NativeHashMap<int, PlexusObjectData> plexusObjectDataById;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<VertexMovementData>();
            plexusObjectEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PlexusObjectData>().Build(ref state);
            vertexEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<VertexColorData, LocalTransform>().WithAll<PlexusObjectIdData>().Build(ref state);
            edgeEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<EdgeColorData>().WithAll<PlexusObjectIdData>().Build(ref state);
            idTypeHandle = state.GetSharedComponentTypeHandle<PlexusObjectIdData>();
            plexusObjectDataById = new NativeHashMap<int, PlexusObjectData>(0, Allocator.Persistent);
        }


        public void OnUpdate(ref SystemState state) {
            idTypeHandle.Update(ref state);
            var plexusObjectEntries = plexusObjectEntityQuery.ToEntityArray(Allocator.Temp);

            if (plexusObjectDataById.Count != plexusObjectEntries.Length) {
                plexusObjectDataById.Dispose();
                plexusObjectDataById = new NativeHashMap<int, PlexusObjectData>(plexusObjectEntries.Length, Allocator.Persistent);
            }

            foreach (Entity plexusObject in plexusObjectEntries) {
                var plexusObjectData = state.EntityManager.GetComponentData<PlexusObjectData>(plexusObject);
                plexusObjectDataById[plexusObjectData.WireframePlexusObjectId] = plexusObjectData;
            }
            new UpdateVertexJob { PlexusObjectDataById = plexusObjectDataById, IdTypeHandle = idTypeHandle }.ScheduleParallel(vertexEntityQuery);
            new UpdateEdgeJob { PlexusObjectDataById = plexusObjectDataById, IdTypeHandle = idTypeHandle }.ScheduleParallel(edgeEntityQuery);

            // id love to do that but iterating over the same query with toArray and with a IEntityJob
            // seems to be a problem instead used plexusObjectSystem to reset the dataUpdated flag
            //new UpdatePlexusObjectJob { }.ScheduleParallel(plexusObjectEntityQuery);
        }
    }

    [BurstCompile]
    public partial struct UpdateVertexJob : IJobEntity, IJobEntityChunkBeginEnd {
        [NativeDisableContainerSafetyRestriction][ReadOnly] public NativeHashMap<int, PlexusObjectData> PlexusObjectDataById;
        public SharedComponentTypeHandle<PlexusObjectIdData> IdTypeHandle;
        int plexusObjectId;

        public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
            plexusObjectId = chunk.GetSharedComponent(IdTypeHandle).PlexusObjectId;
            return PlexusObjectDataById[plexusObjectId].DataUpdated;
        }

        public void Execute(ref VertexColorData colorData, ref LocalTransform localTransform) {
            PlexusObjectData plexusObjectData = PlexusObjectDataById[plexusObjectId];
            colorData.Value = plexusObjectData.VertexColor;
            localTransform.Scale =plexusObjectData.VertexSize;
        }

        public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted) {

        }
    }
    [BurstCompile]
    public partial struct UpdateEdgeJob : IJobEntity, IJobEntityChunkBeginEnd {
        [NativeDisableContainerSafetyRestriction][ReadOnly] public NativeHashMap<int, PlexusObjectData> PlexusObjectDataById;
        public SharedComponentTypeHandle<PlexusObjectIdData> IdTypeHandle;
        int plexusObjectId;

        public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
            plexusObjectId = chunk.GetSharedComponent(IdTypeHandle).PlexusObjectId;
            return PlexusObjectDataById[plexusObjectId].DataUpdated;
        }

        public void Execute(ref EdgeColorData colorData) {
            PlexusObjectData plexusObjectData = PlexusObjectDataById[plexusObjectId];
            colorData.Value = plexusObjectData.EdgeColor;
        }

        public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted) {

        }
    }
    //[BurstCompile]
    public partial struct UpdatePlexusObjectJob : IJobEntity{
        public void Execute(ref PlexusObjectData plexusObjectData) {
            plexusObjectData.DataUpdated = false;
        }
    }
}

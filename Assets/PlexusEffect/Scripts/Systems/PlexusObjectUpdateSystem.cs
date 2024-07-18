using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;


namespace WireframePlexus {

    [UpdateBefore(typeof(ContactEffectAnimationSystem))]
    [UpdateAfter(typeof(PlexusObjectSystem))]
    public partial struct PlexusObjectUpdateSystem : ISystem {

        EntityQuery plexusObjectEntityQuery;
        EntityQuery vertexEntityQuery;
        EntityQuery edgeEntityQuery;
        SharedComponentTypeHandle<PlexusObjectIdData> idTypeHandle;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<VertexMovementData>();
            plexusObjectEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PlexusObjectData>().Build(ref state);
            vertexEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<VertexColorData>().WithAll<PlexusObjectIdData>().Build(ref state);
            edgeEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<EdgeColorData>().WithAll<PlexusObjectIdData>().Build(ref state);
            idTypeHandle = state.GetSharedComponentTypeHandle<PlexusObjectIdData>();
        }


        public void OnUpdate(ref SystemState state) {
            idTypeHandle.Update(ref state);
            NativeHashMap<int, PlexusObjectData> plexusObjectDataById = new NativeHashMap<int, PlexusObjectData>(0, Allocator.Temp);
            
            var plexusObjectEntries = plexusObjectEntityQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity plexusObject in plexusObjectEntries) {
                var plexusObjectData = state.EntityManager.GetComponentData<PlexusObjectData>(plexusObject);
                plexusObjectDataById.Add(plexusObjectData.WireframePlexusObjectId, plexusObjectData);
            }
            new UpdateVertexJob { PlexusObjectDataById = plexusObjectDataById, IdTypeHandle = idTypeHandle }.ScheduleParallel(vertexEntityQuery);
            new UpdateEdgeJob { PlexusObjectDataById = plexusObjectDataById, IdTypeHandle = idTypeHandle }.ScheduleParallel(edgeEntityQuery);
            
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

        public void Execute(ref VertexColorData colorData) {
            PlexusObjectData plexusObjectData = PlexusObjectDataById[plexusObjectId];
            
            colorData.Value = plexusObjectData.VertexColor;
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
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


namespace WireframePlexus {
    [UpdateAfter(typeof(VertexMoveSystem))]
    public partial struct EdgeMoveSystem : ISystem {
        EntityQuery plexusObjectEntityQuery;
        EntityQuery edgeEntityQuery;

        SharedComponentTypeHandle<PlexusObjectIdData> idTypeHandle;

        public void OnCreate(ref SystemState state) {
            plexusObjectEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PlexusObjectData>().Build(ref state);
            edgeEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, EdgeData, PostTransformMatrix, PlexusObjectIdData, EdgeAlphaData>().Build(ref state);

            idTypeHandle = state.GetSharedComponentTypeHandle<PlexusObjectIdData>();
        }



        public void OnUpdate(ref SystemState state) {
            idTypeHandle.Update(ref state);
            var plexusObjectEntries = plexusObjectEntityQuery.ToEntityArray(Allocator.Temp);
            NativeHashMap<int, PlexusObjectData> plexusObjectDataById = new NativeHashMap<int, PlexusObjectData>(plexusObjectEntries.Length, Allocator.Temp);
            foreach (Entity plexusObject in plexusObjectEntries) {
                var plexusObjectData = state.EntityManager.GetComponentData<PlexusObjectData>(plexusObject);
                plexusObjectDataById.Add(plexusObjectData.WireframePlexusObjectId, plexusObjectData);
            }


            new PlexusEdgeMovementJob { DeltaTime = SystemAPI.Time.DeltaTime, IdTypeHandle = idTypeHandle, PlexusObjectDataById = plexusObjectDataById }.ScheduleParallel(edgeEntityQuery);
        }

        [BurstCompile]
        public partial struct PlexusEdgeMovementJob : IJobEntity, IJobEntityChunkBeginEnd {

            public SharedComponentTypeHandle<PlexusObjectIdData> IdTypeHandle;
            [ReadOnly] public float DeltaTime;
            [NativeDisableContainerSafetyRestriction][ReadOnly] public NativeHashMap<int, PlexusObjectData> PlexusObjectDataById;
            int plexusObjectId;

            public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                plexusObjectId = chunk.GetSharedComponent(IdTypeHandle).PlexusObjectId;
                return true;
            }

            public void Execute(ref LocalTransform localTransform, in EdgeData edgeData, ref PostTransformMatrix postTransform, ref EdgeAlphaData edgeColor) {
                PlexusObjectData plexusObjectData = PlexusObjectDataById[plexusObjectId];

                // read vertex positions from native array
                float3 pos1 = plexusObjectData.VertexPositions[edgeData.Vertex1Index];
                float3 pos2 = plexusObjectData.VertexPositions[edgeData.Vertex2Index];

                // calc distance
                float distance = math.distance(pos1, pos2);
                float distancePercent = distance / edgeData.Length;

                // fade in/out of the edge depending of the lenght (is longer than the max length or not)
                if (distancePercent > plexusObjectData.MaxEdgeLengthPercent) {
                    if (edgeColor.Value > 0) {
                        edgeColor.Value -= DeltaTime;
                        if (edgeColor.Value < 0) {
                            edgeColor.Value = 0;
                        }
                    }
                } else {
                    if (edgeColor.Value < 1) {
                        edgeColor.Value += DeltaTime;
                        if (edgeColor.Value > 1) {
                            edgeColor.Value = 1;
                        }
                    }
                }

                // set edge in the middle between the vertices and rotate it to face both vertices
                localTransform.Position = math.lerp(pos1, pos2, 0.5f);
                float3 relativePos = math.normalize(pos1 - pos2);

                // quaternion.LookRotationSafe cannot handle vectors that are collinear so for the case of the edge faceing directly up or down hardcoded a 90 degree rotation
                if (relativePos.y == 1 || relativePos.y == -1) {
                    localTransform.Rotation = quaternion.RotateX(math.PIHALF);
                } else {
                    quaternion end = quaternion.LookRotationSafe(relativePos, math.up());
                    localTransform.Rotation = end;
                }

                // scale the edge to reach both vertices
                var scale = new float3(plexusObjectData.EdgeThickness, plexusObjectData.EdgeThickness, distance);
                postTransform.Value = float4x4.Scale(scale);
            }
            public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted) {

            }
        }
    }
}

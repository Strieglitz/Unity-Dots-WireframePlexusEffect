using NUnit;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


namespace WireframePlexus {
    [UpdateAfter(typeof(VertexMoveSystem))]
    public partial struct EdgeMoveSystem : ISystem {
        EntityQuery plexusObjectEntityQuery;
        EntityQuery edgesByPlexusObjectIdEntityQuery;

        public void OnCreate(ref SystemState state) {
            plexusObjectEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PlexusObjectData>().Build(ref state);
            edgesByPlexusObjectIdEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, EdgeData, PostTransformMatrix, PlexusObjectIdData>().Build(ref state);
        }

        public void OnUpdate(ref SystemState state) {
            var plexusObjectEntries = plexusObjectEntityQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity plexusObject in plexusObjectEntries) {
                var plexusObjectData = state.EntityManager.GetComponentData<PlexusObjectData>(plexusObject);
                edgesByPlexusObjectIdEntityQuery.SetSharedComponentFilter(new PlexusObjectIdData { ObjectId = plexusObjectData.WireframePlexusObjectId });

                PlexusEdgeMovementJob job = new PlexusEdgeMovementJob {     PointPositions = plexusObjectData.VertexPositions, 
                                                                            MaxEdgeLengthPercent = plexusObjectData.MaxEdgeLengthPercent,
                                                                            EdgeThickness = plexusObjectData.EdgeThickness};
                job.ScheduleParallel(edgesByPlexusObjectIdEntityQuery);
            }


        }
   //     [BurstCompile]
        public partial struct PlexusEdgeMovementJob : IJobEntity {

            [ReadOnly] public float MaxEdgeLengthPercent;
            [ReadOnly] public float EdgeThickness;
            [ReadOnly][NativeDisableContainerSafetyRestriction] public NativeArray<float3> PointPositions;

            public void Execute(ref LocalTransform localTransform, ref EdgeData edgeData, ref PostTransformMatrix postTransform) {
                float3 pos1 = PointPositions[edgeData.Vertex1Index];
                float3 pos2 = PointPositions[edgeData.Vertex2Index];

                float distance = math.distance(pos1, pos2);
                float distancePercent = distance / edgeData.Length;
                if (distancePercent > MaxEdgeLengthPercent) {
                    localTransform.Scale = 0;
                } else {
                    localTransform.Position = math.lerp(pos1, pos2, 0.5f);
                    localTransform.Scale = 1;

                    float3 relativePos = math.normalize(pos1 - pos2);

                    // quaternion.LookRotationSafe cannot handle vectors that are collinear so for the case of the edge faceing directly up or down hardcoded a 90 degree rotation
                    if (relativePos.y == 1 || relativePos.y == -1) {
                        localTransform.Rotation = quaternion.RotateX(math.PIHALF);
                    } else {
                        quaternion end = quaternion.LookRotationSafe(relativePos, math.up());
                        localTransform.Rotation = end;
                    }

                    

                    var scale = new float3(EdgeThickness, EdgeThickness, distance);
                    postTransform.Value = float4x4.Scale(scale);
                }
            }
        }
    }
}



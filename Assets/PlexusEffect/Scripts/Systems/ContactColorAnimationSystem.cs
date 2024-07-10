using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace WireframePlexus {

    [UpdateAfter(typeof(EdgeMoveSystem))]
    partial struct ContactColorAnimationSystem : ISystem {

        EntityQuery vertexByPlexusObjectIdEntityQuery;
        EntityQuery edgeByPlexusObjectIdEntityQuery;
        EntityQuery plexusObjectEntityQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            plexusObjectEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PlexusObjectData, ContactColorAnimationData>().Build(ref state);
            vertexByPlexusObjectIdEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<VertexColorData>().WithAll<LocalTransform, PlexusObjectIdData>().Build(ref state);
            edgeByPlexusObjectIdEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<EdgeColorData>().WithAll<LocalTransform, PlexusObjectIdData>().Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var plexusObjectEntries = plexusObjectEntityQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity plexusObject in plexusObjectEntries) {
                var plexusObjectData = state.EntityManager.GetComponentData<PlexusObjectData>(plexusObject);
                var contactColorAnimationData = state.EntityManager.GetComponentData<ContactColorAnimationData>(plexusObject);
                if(contactColorAnimationData.CurrentContactDuration == 0) {
                    continue;
                }

                contactColorAnimationData.CurrentContactDuration -= SystemAPI.Time.DeltaTime;
                float colorInterpolationPercent = 1 - (contactColorAnimationData.CurrentContactDuration / contactColorAnimationData.TotalContactDuration);

                vertexByPlexusObjectIdEntityQuery.SetSharedComponentFilter(new PlexusObjectIdData { ObjectId = plexusObjectData.WireframePlexusObjectId });
                edgeByPlexusObjectIdEntityQuery.SetSharedComponentFilter(new PlexusObjectIdData { ObjectId = plexusObjectData.WireframePlexusObjectId });

                ContactColorVertexJob jobVertex = new ContactColorVertexJob {
                
                };
                ContactColorEdgeJob jobEdge = new ContactColorEdgeJob {
                
                };
                jobVertex.ScheduleParallel(vertexByPlexusObjectIdEntityQuery);
                jobEdge.ScheduleParallel(edgeByPlexusObjectIdEntityQuery);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
        [BurstCompile]
        public partial struct ContactColorVertexJob : IJobEntity {

            public float ColorInterpolationPercent;
            public float3 ContactPosition;
            public float ContactForce;
            public float3 defaultColor;

            public void Execute(ref VertexColorData vertexColorData, in LocalTransform localTransform) {
                float contactDistance = math.distance(localTransform.Position, ContactPosition);

            }
        }

        [BurstCompile]
        public partial struct ContactColorEdgeJob : IJobEntity {
            
            public float ColorInterpolationPercent;
            public float3 ContactPosition;
            public float ContactForce;
            public float3 defaultColor;

            public void Execute(ref EdgeColorData edgeColorData, in LocalTransform localTransform) {

            }
        }
    }
}
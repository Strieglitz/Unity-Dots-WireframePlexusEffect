using System.Numerics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace WireframePlexus {

    [UpdateAfter(typeof(EdgeMoveSystem))]
    partial struct ContactEffectAnimationSystem : ISystem {
        EntityQuery plexusObjectEntityQuery;
        EntityQuery vertexByPlexusObjectIdEntityQuery;
        EntityQuery edgeByPlexusObjectIdEntityQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            plexusObjectEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PlexusObjectData>().Build(ref state);
            vertexByPlexusObjectIdEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<VertexColorData>().WithAll<LocalTransform, PlexusObjectIdData>().Build(ref state);
            edgeByPlexusObjectIdEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<EdgeColorData>().WithAll<LocalTransform, PlexusObjectIdData>().Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var plexusObjectEntries = plexusObjectEntityQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity plexusObject in plexusObjectEntries) {
                var plexusObjectData = state.EntityManager.GetComponentData<PlexusObjectData>(plexusObject);
                for (int i = 0; i < plexusObjectData.ContactAnimationColorData.Length; i++) {
                    ContactEffectData contactColorAnimationData = plexusObjectData.ContactAnimationColorData[i];
                    
                    if (contactColorAnimationData.CurrentContactDuration == 0) {
                        continue;
                    }
                    contactColorAnimationData.CurrentContactDuration -= SystemAPI.Time.DeltaTime;
                    if (contactColorAnimationData.CurrentContactDuration < 0) {
                        contactColorAnimationData.CurrentContactDuration = 0;
                    }

                    float colorInterpolationPercent = 1 - (contactColorAnimationData.CurrentContactDuration / contactColorAnimationData.TotalContactDuration);
                    vertexByPlexusObjectIdEntityQuery.ResetFilter();
                    vertexByPlexusObjectIdEntityQuery.SetSharedComponentFilter(new PlexusObjectIdData { ObjectId = plexusObjectData.WireframePlexusObjectId });
                    edgeByPlexusObjectIdEntityQuery.ResetFilter();
                    edgeByPlexusObjectIdEntityQuery.SetSharedComponentFilter(new PlexusObjectIdData { ObjectId = plexusObjectData.WireframePlexusObjectId });

                    plexusObjectData.ContactAnimationColorData[i] = contactColorAnimationData;

                    ContactColorVertexJob jobVertex = new ContactColorVertexJob {
                        ColorInterpolationPercent = colorInterpolationPercent,
                        ContactPosition = contactColorAnimationData.LocalContactPosition,
                        ContactMaxDistance = contactColorAnimationData.ContactRadius,
                        DefaultColor = plexusObjectData.VertexColor,
                        ContactColor = contactColorAnimationData.ContactColor
                    };
                    ContactColorEdgeJob jobEdge = new ContactColorEdgeJob {
                        ColorInterpolationPercent = colorInterpolationPercent,
                        ContactPosition = contactColorAnimationData.LocalContactPosition,
                        ContactMaxDistance = contactColorAnimationData.ContactRadius,
                        DefaultColor = plexusObjectData.EdgeColor,
                        ContactColor = contactColorAnimationData.ContactColor
                    };
                    jobVertex.ScheduleParallel(vertexByPlexusObjectIdEntityQuery);
                    jobEdge.ScheduleParallel(edgeByPlexusObjectIdEntityQuery);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
        [BurstCompile]
        public partial struct ContactColorVertexJob : IJobEntity {

            public float ColorInterpolationPercent;
            public float3 ContactPosition;
            public float ContactMaxDistance;
            public float4 DefaultColor;
            public float4 ContactColor;

            public void Execute(ref VertexColorData vertexColorData, in LocalTransform localTransform) {
                float vertexDistanceToContact = math.distance(localTransform.Position, ContactPosition);
                if(vertexDistanceToContact < ContactMaxDistance) {
                    float colorStrength = 1 - (vertexDistanceToContact / ContactMaxDistance);
                    float4 contactColorStrength = math.lerp(DefaultColor, ContactColor, colorStrength);
                    float4 color = math.lerp(contactColorStrength, DefaultColor, ColorInterpolationPercent);
                    vertexColorData.Value = color;
                }
            }
        }

        [BurstCompile]
        public partial struct ContactColorEdgeJob : IJobEntity {
            
            public float ColorInterpolationPercent;
            public float3 ContactPosition;
            public float ContactMaxDistance;
            public float4 DefaultColor;
            public float4 ContactColor;

            public void Execute(ref EdgeColorData edgeColorData, in LocalTransform localTransform) {
                float vertexDistanceToContact = math.distance(localTransform.Position, ContactPosition);
                if (vertexDistanceToContact < ContactMaxDistance) {
                    float colorStrength = 1 - (vertexDistanceToContact / ContactMaxDistance);
                    float4 contactColorStrength = math.lerp(DefaultColor, ContactColor, colorStrength);
                    float4 color = math.lerp(contactColorStrength, DefaultColor, ColorInterpolationPercent);
                    edgeColorData.Value = color;
                }
            }
        }
    }
}
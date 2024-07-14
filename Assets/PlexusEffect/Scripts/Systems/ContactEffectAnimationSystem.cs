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
            vertexByPlexusObjectIdEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<VertexColorData>().WithAll<LocalTransform, PlexusObjectIdData, LocalToWorld>().Build(ref state);
            edgeByPlexusObjectIdEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<EdgeColorData>().WithAll<LocalTransform, PlexusObjectIdData, LocalToWorld>().Build(ref state);
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
                    vertexByPlexusObjectIdEntityQuery.SetSharedComponentFilter(new PlexusObjectIdData { PlexusObjectId = plexusObjectData.WireframePlexusObjectId });
                    edgeByPlexusObjectIdEntityQuery.ResetFilter();
                    edgeByPlexusObjectIdEntityQuery.SetSharedComponentFilter(new PlexusObjectIdData { PlexusObjectId = plexusObjectData.WireframePlexusObjectId });

                    plexusObjectData.ContactAnimationColorData[i] = contactColorAnimationData;

                    ContactColorVertexJob jobVertex = new ContactColorVertexJob {
                        ColorInterpolationPercent = colorInterpolationPercent,
                        ContactWorldPosition = contactColorAnimationData.ContactWorldPosition,
                        ContactMaxDistance = contactColorAnimationData.ContactRadius,
                        DefaultColor = plexusObjectData.VertexColor,
                        ContactColor = contactColorAnimationData.ContactColor,
                        ParentWorldPos = plexusObjectData.WorldPosition,
                        ParentWorldRotation = plexusObjectData.WorldRotation
                    };
                    ContactColorEdgeJob jobEdge = new ContactColorEdgeJob {
                        ColorInterpolationPercent = colorInterpolationPercent,
                        ContactWorldPosition = contactColorAnimationData.ContactWorldPosition,
                        ContactMaxDistance = contactColorAnimationData.ContactRadius,
                        DefaultColor = plexusObjectData.EdgeColor,
                        ContactColor = contactColorAnimationData.ContactColor,
                        ParentWorldPos = plexusObjectData.WorldPosition,
                        ParentWorldRotation = plexusObjectData.WorldRotation
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
            public float3 ContactWorldPosition;
            public float ContactMaxDistance;
            public float4 DefaultColor;
            public float4 ContactColor;
            public float3 ParentWorldPos;
            public quaternion ParentWorldRotation;

            public void Execute(ref VertexColorData vertexColorData, in LocalTransform localTransform) {
                float3 relativeContactPos = ContactWorldPosition - (ParentWorldPos);
                float3 relativeVertexPos = math.mul(ParentWorldRotation, localTransform.Position);
                float vertexDistanceToContact = math.distance(relativeVertexPos, relativeContactPos);
                
                if (vertexDistanceToContact < ContactMaxDistance) {
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
            public float3 ContactWorldPosition;
            public float ContactMaxDistance;
            public float4 DefaultColor;
            public float4 ContactColor;
            public float3 ParentWorldPos;
            public quaternion ParentWorldRotation;

            public void Execute(ref EdgeColorData edgeColorData, in LocalTransform localTransform, in LocalToWorld localToWorld) {
                float3 relativeContactPos = ContactWorldPosition - (ParentWorldPos);
                float3 relativeVertexPos = math.mul(ParentWorldRotation, localTransform.Position);
                float vertexDistanceToContact = math.distance(relativeVertexPos, relativeContactPos);

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
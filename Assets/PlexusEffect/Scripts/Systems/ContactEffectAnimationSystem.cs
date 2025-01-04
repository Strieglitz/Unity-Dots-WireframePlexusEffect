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
            vertexByPlexusObjectIdEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<VertexColorData, VertexAdditionalMovementData>().WithAll<LocalTransform, PlexusObjectIdData, LocalToWorld>().Build(ref state);
            edgeByPlexusObjectIdEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<EdgeColorData>().WithAll<LocalTransform, PlexusObjectIdData, LocalToWorld>().Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var plexusObjectEntries = plexusObjectEntityQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity plexusObject in plexusObjectEntries) {
                var plexusObjectData = state.EntityManager.GetComponentData<PlexusObjectData>(plexusObject);
                if(plexusObjectData.Visible == false) {
                    plexusObjectData.ContactAnimationColorData.Clear();
                    continue;
                }

                for (int i = 0; i < plexusObjectData.ContactAnimationColorData.Length; i++) {
                    ContactEffectData contactColorAnimationData = plexusObjectData.ContactAnimationColorData[i];
                    
                    if (contactColorAnimationData.CurrentContactDuration == 0) {
                        continue;
                    }
                    contactColorAnimationData.CurrentContactDuration -= SystemAPI.Time.DeltaTime;
                    if (contactColorAnimationData.CurrentContactDuration < 0) {
                        contactColorAnimationData.CurrentContactDuration = 0;
                    }

                    float contactInterpolationPercent = 1 - (contactColorAnimationData.CurrentContactDuration / contactColorAnimationData.TotalContactDuration);
                    float contactInterpolationPercentEased = EasingFunctions.GetEaseValue( contactColorAnimationData.EasingType,contactInterpolationPercent);
                    
                    float contactVertexInterpolationPercent = contactInterpolationPercent * contactColorAnimationData.ContactVertexDurationMultiplier;
                    if (contactVertexInterpolationPercent > 1) {
                        contactVertexInterpolationPercent =1;
                    }
                    float contactVertexInterpolationPercentEased = EasingFunctions.GetEaseValue(contactColorAnimationData.EasingType, contactVertexInterpolationPercent);


                    vertexByPlexusObjectIdEntityQuery.ResetFilter();
                    vertexByPlexusObjectIdEntityQuery.SetSharedComponentFilter(new PlexusObjectIdData { PlexusObjectId = plexusObjectData.WireframePlexusObjectId });
                    edgeByPlexusObjectIdEntityQuery.ResetFilter();
                    edgeByPlexusObjectIdEntityQuery.SetSharedComponentFilter(new PlexusObjectIdData { PlexusObjectId = plexusObjectData.WireframePlexusObjectId });

                    plexusObjectData.ContactAnimationColorData[i] = contactColorAnimationData;

                    ContactAnimationVertexJob jobVertex = new ContactAnimationVertexJob {
                        InterpolationPercent = contactInterpolationPercentEased,
                        ContactWorldPosition = contactColorAnimationData.ContactWorldPosition,
                        ContactMaxDistance = contactColorAnimationData.ContactRadius,
                        DefaultColor = plexusObjectData.VertexColor,
                        ContactColor = contactColorAnimationData.ContactColor,
                        ParentWorldPos = plexusObjectData.WorldPosition,
                        ParentWorldRotation = plexusObjectData.WorldRotation,
                        VertexInterpolationPercent = contactVertexInterpolationPercentEased,
                        ContactVertexMaxDistance = contactColorAnimationData.ContactVertexMaxDistance
                    };
                    ContactAnimationEdgeJob jobEdge = new ContactAnimationEdgeJob {
                        InterpolationPercent = contactInterpolationPercentEased,
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
                // iterate backwards over all contact animations and remove the ones that are done
                for (int i = plexusObjectData.ContactAnimationColorData.Length - 1; i >= 0; i--) {
                    if (plexusObjectData.ContactAnimationColorData[i].CurrentContactDuration == 0) {
                        plexusObjectData.ContactAnimationColorData.RemoveAtSwapBack(i);
                    }
                }

            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
        [BurstCompile]
        public partial struct ContactAnimationVertexJob : IJobEntity {

            public float InterpolationPercent;
            public float VertexInterpolationPercent;
            public float ContactVertexMaxDistance;
            public float3 ContactWorldPosition;
            public float ContactMaxDistance;
            public float4 DefaultColor;
            public float4 ContactColor;
            public float3 ParentWorldPos;
            public quaternion ParentWorldRotation;



            public void Execute(ref VertexColorData vertexColorData,ref VertexAdditionalMovementData vertexAdditionalMovementData, in LocalTransform localTransform) {
                // the additionalMovementAnimation is changing the position, so we need to calculate the distance to the contact point without the additional movement
                // also the local position is rotated by the parent rotation, to match the world position of the vertex
                float3 localPosWithoutAdditionalMovement = localTransform.Position - vertexAdditionalMovementData.AdditionalLocalPosition;
                float3 relativeContactPos = ContactWorldPosition - ParentWorldPos;
                float3 relativeVertexPos = math.mul(ParentWorldRotation, localPosWithoutAdditionalMovement);
                float vertexDistanceToContact = math.distance(relativeVertexPos, relativeContactPos);
                
                if (vertexDistanceToContact < ContactMaxDistance) {
                    float colorStrength = 1 - (vertexDistanceToContact / ContactMaxDistance);
                    float4 contactColorStrength = math.lerp(DefaultColor, ContactColor, colorStrength);
                    float4 color = math.lerp(contactColorStrength, DefaultColor, InterpolationPercent);
                    vertexColorData.Value = color;


                    float3 additionalVertexMoveDirection = math.normalize(relativeVertexPos - relativeContactPos) * colorStrength * ContactVertexMaxDistance;
                    if (VertexInterpolationPercent < 0.5f) {
                        additionalVertexMoveDirection = math.lerp(0, additionalVertexMoveDirection, VertexInterpolationPercent * 2);
                    } else {
                        additionalVertexMoveDirection = math.lerp(additionalVertexMoveDirection, 0, (VertexInterpolationPercent - 0.5f) * 2);
                    }
                    vertexAdditionalMovementData.AdditionalLocalPosition = math.mul(math.inverse(ParentWorldRotation), additionalVertexMoveDirection);
                }
            }
        }

        [BurstCompile]
        public partial struct ContactAnimationEdgeJob : IJobEntity {
            
            public float InterpolationPercent;
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
                    float4 color = math.lerp(contactColorStrength, DefaultColor, InterpolationPercent);
                    edgeColorData.Value = color;
                }
            }
        }
    }
}
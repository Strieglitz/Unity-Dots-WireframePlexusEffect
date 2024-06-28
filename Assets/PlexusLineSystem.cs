using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(PlexusPointMovementSystem))]
public partial struct PlexusLineSystem : ISystem
{
    EntityQuery plexusObjectEntityQuery;
    EntityQuery plexusLinesByPlexusObjectIdEntityQuery;

    public void OnCreate(ref SystemState state) {
        plexusObjectEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<WireframePlexusEntityData>().Build(ref state);
        plexusLinesByPlexusObjectIdEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, PlexusLineData, PostTransformMatrix, PlexusObjectIdData>().Build(ref state);
    }

    public void OnUpdate(ref SystemState state) {
        var plexusObjectEntries = plexusObjectEntityQuery.ToEntityArray(Allocator.Temp);
        foreach (Entity plexusObject in plexusObjectEntries) {
            var plexusObjectId = state.EntityManager.GetSharedComponent<PlexusObjectIdData>(plexusObject);
            var plexusObjectData = state.EntityManager.GetComponentData<WireframePlexusEntityData>(plexusObject);
            plexusLinesByPlexusObjectIdEntityQuery.SetSharedComponentFilter(plexusObjectId);


            PlexusLineMovementJob job = new PlexusLineMovementJob { PointPositions = plexusObjectData.VertexPositions };
            job.ScheduleParallel(plexusLinesByPlexusObjectIdEntityQuery);
        }


    }
    [BurstCompile]
    public partial struct PlexusLineMovementJob : IJobEntity {
        [NativeDisableContainerSafetyRestriction] public NativeArray<float3> PointPositions;

        public void Execute(ref LocalTransform localTransform, ref PlexusLineData lineData, ref PostTransformMatrix postTransform) {
            float3 pos1 = PointPositions[lineData.Position1Id];
            float3 pos2 = PointPositions[lineData.Position2Id];

            float distance = math.distance(pos1, pos2);
            float distancePercent = distance / lineData.Length;
            if (distancePercent > lineData.MaxLengthPercent) {
                localTransform.Scale = 0;
            } else {
                localTransform.Position = math.lerp(pos1, pos2, 0.5f);
                localTransform.Scale = 1;

                float3 relativePos = math.normalize(pos1 - pos2);

                quaternion start = localTransform.Rotation.value;
                quaternion end = quaternion.LookRotation(relativePos, math.up());
                localTransform.Rotation = end;

                var scale = new float3(0.003f, 0.003f, distance);
                postTransform.Value = float4x4.Scale(scale);
            }
        }
    }
}
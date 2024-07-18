using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace WireframePlexus {

    [UpdateAfter(typeof(PlexusObjectSystem))]
    public partial class PlexusObjectUpdateRecieveSystem : SystemBase {

        EntityQuery plexusObjectEntityQuery;

        protected override void OnCreate() {
            plexusObjectEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<SyncEntityPositionToGameobjectPositionData, PlexusObjectData, LocalTransform>().Build(this);
        }


        public void UpdatePlexusObjectData(PlexusObjectData plexusObjectData) {
            var plexusObjectEntities = plexusObjectEntityQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in plexusObjectEntities) {
                var currentPlexusObjectData = EntityManager.GetComponentData<PlexusObjectData>(entity);
                if (currentPlexusObjectData.WireframePlexusObjectId == plexusObjectData.WireframePlexusObjectId) {
                    currentPlexusObjectData.VertexSize = plexusObjectData.VertexSize;
                    currentPlexusObjectData.MaxEdgeLengthPercent = plexusObjectData.MaxEdgeLengthPercent;
                    currentPlexusObjectData.EdgeThickness = plexusObjectData.EdgeThickness;
                    currentPlexusObjectData.MaxVertexMoveDistance = plexusObjectData.MaxVertexMoveDistance;
                    currentPlexusObjectData.MinVertexMoveSpeed = plexusObjectData.MinVertexMoveSpeed;
                    currentPlexusObjectData.VertexColor = plexusObjectData.VertexColor;
                    currentPlexusObjectData.EdgeColor = plexusObjectData.EdgeColor;
                    currentPlexusObjectData.DataUpdated = true;
                    EntityManager.SetComponentData(entity, currentPlexusObjectData);
                    break;
                }
            }
        }

        [BurstCompile]
        protected override void OnUpdate() {

        }
    }
}
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace WireframePlexus {

    [UpdateAfter(typeof(PlexusObjectSystem))]
    public partial class PlexusObjectRecieveUpdatedDataSystem : SystemBase {

        EntityQuery plexusObjectEntityQuery;
        NativeList<PlexusObjectData> changeData = new NativeList<PlexusObjectData>(0, Allocator.Persistent);

        protected override void OnDestroy() {
            changeData.Dispose();
        }

        protected override void OnCreate() {
            plexusObjectEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<SyncEntityPositionToGameobjectPositionData, PlexusObjectData, LocalTransform>().Build(this);
        }

        public void UpdatePlexusObjectData(PlexusGameObjectData plexusGameObjectData, int plexusObjectId) {
            PlexusObjectData plexusObjectData = plexusGameObjectData.ToPlexusObjectData();
            plexusObjectData.WireframePlexusObjectId = plexusObjectId;
            changeData.Add(plexusObjectData);
        }

        [BurstCompile]
        protected override void OnUpdate() {
            var plexusObjectEntities = plexusObjectEntityQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in plexusObjectEntities) {
                foreach(PlexusObjectData newPlexusObjectData in changeData) {
                    var currentPlexusObjectData = EntityManager.GetComponentData<PlexusObjectData>(entity);
                    if (currentPlexusObjectData.WireframePlexusObjectId == newPlexusObjectData.WireframePlexusObjectId) {
                        currentPlexusObjectData.VertexSize = newPlexusObjectData.VertexSize;
                        currentPlexusObjectData.MaxEdgeLengthPercent = newPlexusObjectData.MaxEdgeLengthPercent;
                        currentPlexusObjectData.EdgeThickness = newPlexusObjectData.EdgeThickness;
                        currentPlexusObjectData.MaxVertexMoveDistance = newPlexusObjectData.MaxVertexMoveDistance;
                        currentPlexusObjectData.MinVertexMoveSpeed = newPlexusObjectData.MinVertexMoveSpeed;
                        currentPlexusObjectData.VertexColor = newPlexusObjectData.VertexColor;
                        currentPlexusObjectData.EdgeColor = newPlexusObjectData.EdgeColor;
                        currentPlexusObjectData.DataUpdated = true;
                        EntityManager.SetComponentData(entity, currentPlexusObjectData);
                    }
                }
                
            }
            changeData.Clear();
        }
        
    }
}
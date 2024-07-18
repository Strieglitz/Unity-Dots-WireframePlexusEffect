
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace WireframePlexus {

    [UpdateBefore(typeof(PlexusObjectSystem))]
    public partial class PlexusObjectDeleteSystem : SystemBase {

        EntityQuery plexusObjectEntityQuery;
        EntityQuery vertexByPlexusObjectIdEntityQuery;
        EntityQuery edgeByPlexusObjectIdEntityQuery;

        NativeHashSet<int> plexusObjectIdsToDestroy = new NativeHashSet<int>(0,Allocator.Persistent);

        protected override void OnDestroy() {
            plexusObjectIdsToDestroy.Dispose();
        }

        protected override void OnCreate() {
            plexusObjectEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<SyncEntityPositionToGameobjectPositionData, PlexusObjectData, LocalTransform>().Build(this);
            vertexByPlexusObjectIdEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<VertexColorData, VertexAdditionalMovementData>().WithAll<LocalTransform, PlexusObjectIdData, LocalToWorld>().Build(this);
            edgeByPlexusObjectIdEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW<EdgeColorData>().WithAll<LocalTransform, PlexusObjectIdData, LocalToWorld>().Build(this);

        }

        protected override void OnUpdate() {
            if (plexusObjectIdsToDestroy.Count > 0) {
                var plexusObjectEntities = plexusObjectEntityQuery.ToEntityArray(Allocator.Temp);
                foreach (Entity entity in plexusObjectEntities) {
                    var plexusObjectData = EntityManager.GetComponentData<PlexusObjectData>(entity);
                    if (plexusObjectIdsToDestroy.Contains(plexusObjectData.WireframePlexusObjectId)) {
                        vertexByPlexusObjectIdEntityQuery.ResetFilter();
                        vertexByPlexusObjectIdEntityQuery.SetSharedComponentFilter(new PlexusObjectIdData { PlexusObjectId = plexusObjectData.WireframePlexusObjectId });
                        edgeByPlexusObjectIdEntityQuery.ResetFilter();
                        edgeByPlexusObjectIdEntityQuery.SetSharedComponentFilter(new PlexusObjectIdData { PlexusObjectId = plexusObjectData.WireframePlexusObjectId });

                        EntityManager.DestroyEntity(entity);
                        EntityManager.DestroyEntity(vertexByPlexusObjectIdEntityQuery);
                        EntityManager.DestroyEntity(edgeByPlexusObjectIdEntityQuery);
                        break;
                    }

                }
                plexusObjectIdsToDestroy.Clear();
            }
        }

        public void DeletePlexusObject(int plexusObjectId) {
            plexusObjectIdsToDestroy.Add(plexusObjectId);
            
        }
    }
}
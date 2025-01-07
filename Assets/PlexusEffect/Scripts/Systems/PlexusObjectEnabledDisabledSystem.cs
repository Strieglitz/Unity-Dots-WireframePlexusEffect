
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace WireframePlexus {

    [UpdateAfter(typeof(PlexusObjectDeleteSystem))]
    public partial class PlexusObjectEnabledDisabledSystem : SystemBase {

        EntityQuery plexusObjectEntityQueryById;
        EntityQuery vertexByPlexusObjectbyIdEntityQuery;
        EntityQuery edgeByPlexusObjectByIdEntityQuery;

        protected override void OnCreate() {
            plexusObjectEntityQueryById = new EntityQueryBuilder(Allocator.Temp).WithAll<PlexusObjectData>().WithOptions(EntityQueryOptions.IncludeDisabledEntities).Build(this);
            vertexByPlexusObjectbyIdEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PlexusObjectIdData>().WithOptions(EntityQueryOptions.IncludeDisabledEntities).Build(this);
            edgeByPlexusObjectByIdEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PlexusObjectIdData>().WithOptions(EntityQueryOptions.IncludeDisabledEntities).Build(this);
        }

        protected override void OnUpdate() {

        }

        public void SetEntityEnabled(int plexusObjectId, bool isEnabled) {
            var plexusObjectEntities = plexusObjectEntityQueryById.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in plexusObjectEntities) {
                var plexusObjectData = EntityManager.GetComponentData<PlexusObjectData>(entity);
                if (plexusObjectData.WireframePlexusObjectId == plexusObjectId) {
                    EntityManager.SetEnabled(entity, isEnabled);


                    vertexByPlexusObjectbyIdEntityQuery.ResetFilter();
                    vertexByPlexusObjectbyIdEntityQuery.SetSharedComponentFilter(new PlexusObjectIdData { PlexusObjectId = plexusObjectId });
                    var vertexEntities = vertexByPlexusObjectbyIdEntityQuery.ToEntityArray(Allocator.Temp);
                    foreach (Entity vertexEntity in vertexEntities) {
                        EntityManager.SetEnabled(vertexEntity, isEnabled);
                    }

                    edgeByPlexusObjectByIdEntityQuery.ResetFilter();
                    edgeByPlexusObjectByIdEntityQuery.SetSharedComponentFilter(new PlexusObjectIdData { PlexusObjectId = plexusObjectId });
                    var edgeEntities = edgeByPlexusObjectByIdEntityQuery.ToEntityArray(Allocator.Temp);
                    foreach (Entity edgeEntity in edgeEntities) {
                        EntityManager.SetEnabled(edgeEntity, isEnabled);
                    }

                    break;
                }
            }
        }
    }
}
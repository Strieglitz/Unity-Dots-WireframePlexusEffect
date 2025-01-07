
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace WireframePlexus {

    [UpdateAfter(typeof(PlexusObjectDeleteSystem))]
    public partial class PlexusObjectSystem : SystemBase {

        EntityQuery plexusObjectEntityQuery;

        protected override void OnCreate() {
            plexusObjectEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<SyncEntityPositionToGameobjectPositionData, PlexusObjectData, LocalTransform>().Build(this);
        }

        protected override void OnUpdate() {
            var plexusObjectEntities = plexusObjectEntityQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in plexusObjectEntities) {

                var plexusObjectData = EntityManager.GetComponentData<PlexusObjectData>(entity);

                // sync entity position to gameobject position
                var localTransform = EntityManager.GetComponentData<LocalTransform>(entity);
                var gameobjectReference = EntityManager.GetComponentData<SyncEntityPositionToGameobjectPositionData>(entity);
                if (localTransform.Position.Equals(gameobjectReference.PlexusGameObject.transform.position) && localTransform.Rotation.Equals(gameobjectReference.PlexusGameObject.transform.rotation) && localTransform.Scale.Equals((gameobjectReference.PlexusGameObject.transform.lossyScale.x + gameobjectReference.PlexusGameObject.transform.lossyScale.y + gameobjectReference.PlexusGameObject.transform.lossyScale.z) / 3)) {
                    continue;
                }
                EntityManager.SetComponentData(entity, new LocalTransform { Position = gameobjectReference.PlexusGameObject.transform.position, Rotation = gameobjectReference.PlexusGameObject.transform.rotation, Scale = (gameobjectReference.PlexusGameObject.transform.lossyScale.x + gameobjectReference.PlexusGameObject.transform.lossyScale.y + gameobjectReference.PlexusGameObject.transform.lossyScale.z) / 3 });

                // set the rotation to the plexus object data so the vertex movement system can use it
                
                plexusObjectData.WorldRotation = gameobjectReference.PlexusGameObject.transform.rotation;
                plexusObjectData.WorldPosition = gameobjectReference.PlexusGameObject.transform.position;
                // set the update flag to false, because a new frame beginns and updated happen afterwards
                plexusObjectData.DataUpdated = false;
                EntityManager.SetComponentData(entity, plexusObjectData);

            }
        }
    }
}
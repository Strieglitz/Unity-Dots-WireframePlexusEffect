
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace WireframePlexus {

    public partial class PlexusObjectSystem : SystemBase {

        EntityQuery plexusObjectEntityQuery;

        protected override void OnCreate() {
            plexusObjectEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAllRW< PlexusObjectData, LocalTransform>().Build(this);
        }

        protected override void OnUpdate() {
            var plexusObjectEntities = plexusObjectEntityQuery.ToEntityArray(Allocator.Temp);
            foreach(Entity entity in plexusObjectEntities) {
                var localTransform = EntityManager.GetComponentData<LocalTransform>(entity);
                var gameobjectReference = EntityManager.GetComponentData<SyncEntityPositionToGameobjectPositionData>(entity);
                if (localTransform.Position.Equals(gameobjectReference.gameObject.transform.position) && localTransform.Rotation.Equals(gameobjectReference.gameObject.transform.rotation) && localTransform.Scale.Equals((gameobjectReference.gameObject.transform.lossyScale.x + gameobjectReference.gameObject.transform.lossyScale.y + gameobjectReference.gameObject.transform.lossyScale.z) / 3)) {
                    continue;
                }
                EntityManager.SetComponentData(entity, new LocalTransform { Position = gameobjectReference.gameObject.transform.position, Rotation = gameobjectReference.gameObject.transform.rotation, Scale = (gameobjectReference.gameObject.transform.lossyScale.x + gameobjectReference.gameObject.transform.lossyScale.y + gameobjectReference.gameObject.transform.lossyScale.z) / 3 });
                var plexusObjectData = EntityManager.GetComponentData<PlexusObjectData>(entity);
                plexusObjectData.CurrentRotation = gameobjectReference.gameObject.transform.rotation;
                EntityManager.SetComponentData(entity, plexusObjectData);
            }
        }
    }
}
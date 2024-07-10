
using System.Linq;
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
                if (localTransform.Position.Equals(gameobjectReference.PlexusGameObject.transform.position) && localTransform.Rotation.Equals(gameobjectReference.PlexusGameObject.transform.rotation) && localTransform.Scale.Equals((gameobjectReference.PlexusGameObject.transform.lossyScale.x + gameobjectReference.PlexusGameObject.transform.lossyScale.y + gameobjectReference.PlexusGameObject.transform.lossyScale.z) / 3)) {
                    continue;
                }
                EntityManager.SetComponentData(entity, new LocalTransform { Position = gameobjectReference.PlexusGameObject.transform.position, Rotation = gameobjectReference.PlexusGameObject.transform.rotation, Scale = (gameobjectReference.PlexusGameObject.transform.lossyScale.x + gameobjectReference.PlexusGameObject.transform.lossyScale.y + gameobjectReference.PlexusGameObject.transform.lossyScale.z) / 3 });
                var plexusObjectData = EntityManager.GetComponentData<PlexusObjectData>(entity);
                plexusObjectData.CurrentRotation = gameobjectReference.PlexusGameObject.transform.rotation;
                EntityManager.SetComponentData(entity, plexusObjectData);
            }
        }

        public void SetPlexusContactAnimation(PlexusGameObject plexusGameObject, ContactColorAnimationData contactColorAnimationData) {
            var plexusObjectEntities = plexusObjectEntityQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in plexusObjectEntities) {
                var gameobjectReference = EntityManager.GetComponentData<SyncEntityPositionToGameobjectPositionData>(entity);
                if (gameobjectReference.PlexusGameObject == plexusGameObject) {
                    EntityManager.SetComponentData(entity, contactColorAnimationData);
                    break;
                }
            }
        }
    }
}
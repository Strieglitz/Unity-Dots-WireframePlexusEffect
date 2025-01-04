using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


namespace WireframePlexus {

    [UpdateAfter(typeof(PlexusObjectSystem))]
    public partial class ContactEffectSpawnSystem : SystemBase {

        EntityQuery plexusObjectEntityQuery;

        protected override void OnCreate() {
            plexusObjectEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PlexusObjectData>().Build(this);
        }

        protected override void OnUpdate() {

        }

        public void SpawnContactEffect(ContactEffectData contactEffectData, int plexusObjectId) {
            var plexusObjectEntities = plexusObjectEntityQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in plexusObjectEntities) {
                var plexusObjectData = EntityManager.GetComponentData<PlexusObjectData>(entity);
                if (plexusObjectData.WireframePlexusObjectId == plexusObjectId && plexusObjectData.Visible == true) {
                    plexusObjectData.ContactAnimationColorData.Add(contactEffectData);
                    EntityManager.SetComponentData(entity, plexusObjectData);
                    break;
                }
            }
        }
    }
}
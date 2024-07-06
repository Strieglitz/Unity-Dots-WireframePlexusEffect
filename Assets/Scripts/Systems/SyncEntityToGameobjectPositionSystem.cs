
using Unity.Entities;
using Unity.Transforms;

namespace WireframePlexus {

    public partial class SyncEntityToGameobjectPositionSystem : SystemBase {
        protected override void OnUpdate() {
            foreach (var (localTransform, gameobjectReference) in SystemAPI.Query<RefRW<LocalTransform>, SyncEntityPositionToGameobjectPositionData>()) {
                localTransform.ValueRW = new LocalTransform { Position = gameobjectReference.gameObject.transform.position, Rotation = gameobjectReference.gameObject.transform.rotation, Scale = gameobjectReference.gameObject.transform.localScale.magnitude };
            }
        }
    }
}
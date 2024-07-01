
using Unity.Entities;
using Unity.Transforms;

namespace WireframePlexus {
    public partial class SyncEntityToGameobjectPositionSystem : SystemBase {
        protected override void OnUpdate() {
            foreach (var (localTransform, gameobjectReference) in SystemAPI.Query<RefRW<LocalTransform>, SyncEntityPositionToGameobjectData>()) {
                localTransform.ValueRW = localTransform.ValueRW.WithPosition(gameobjectReference.gameObject.transform.position);
            }
        }
    }
}
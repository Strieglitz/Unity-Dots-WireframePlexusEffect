
using Unity.Entities;
using Unity.Transforms;

public partial class SyncEntityToGameobjectPositionSystem : SystemBase {
    protected override void OnUpdate() {
        foreach(var( localTransform, gameobjectReference) in SystemAPI.Query<RefRW<LocalTransform>, SyncEntityToGameobjectPositionData>()) {
            localTransform.ValueRW = localTransform.ValueRW.WithPosition(gameobjectReference.gameObject.transform.position);
        }
    }
}

using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SyncEntityToGameobjectPositionAuthoring : MonoBehaviour
{
    private class Baker : Baker<SyncEntityToGameobjectPositionAuthoring> {
        public override void Bake(SyncEntityToGameobjectPositionAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new SyncEntityToGameobjectPositionData { });
        }
    }
}
public class SyncEntityToGameobjectPositionData : IComponentData {
    public GameObject gameObject;
}

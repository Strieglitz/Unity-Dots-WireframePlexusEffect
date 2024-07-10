using Unity.Entities;
using UnityEngine;

namespace WireframePlexus {

    public class PlexusObjectEntityAuthoring : MonoBehaviour {
        private class Baker : Baker<PlexusObjectEntityAuthoring> {
            public override void Bake(PlexusObjectEntityAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponentObject(entity, new SyncEntityPositionToGameobjectPositionData { });
            }
        }
    }
}
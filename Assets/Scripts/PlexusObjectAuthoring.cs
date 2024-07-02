using Unity.Entities;
using UnityEngine;

namespace WireframePlexus {

    public class PlexusObjectAuthoring : MonoBehaviour {
        private class Baker : Baker<PlexusObjectAuthoring> {
            public override void Bake(PlexusObjectAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponentObject(entity, new SyncEntityPositionToGameobjectPositionData { });
            }
        }
    }
}
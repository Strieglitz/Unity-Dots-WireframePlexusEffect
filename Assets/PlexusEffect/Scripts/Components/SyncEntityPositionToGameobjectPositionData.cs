using Unity.Entities;
using UnityEngine;

namespace WireframePlexus {

    public class SyncEntityPositionToGameobjectPositionData : IComponentData {
        public PlexusGameObject PlexusGameObject;
    }
}
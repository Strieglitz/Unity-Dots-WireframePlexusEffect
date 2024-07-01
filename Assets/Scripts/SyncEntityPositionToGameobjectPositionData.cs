using Unity.Entities;
using UnityEngine;

namespace WireframePlexus {

    public class SyncEntityPositionToGameobjectPositionData : IComponentData {
        public GameObject gameObject;
    }
}
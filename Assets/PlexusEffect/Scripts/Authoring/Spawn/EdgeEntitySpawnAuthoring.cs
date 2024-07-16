using Unity.Entities;
using UnityEngine;

namespace WireframePlexus {

    public class EdgeEntitySpawnAuthoring : MonoBehaviour {
        public GameObject WireframePlexusEdgeEntityPrefab;

        public class Baker : Baker<EdgeEntitySpawnAuthoring> {
            public override void Bake(EdgeEntitySpawnAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new EdgeSpawnData { WireframePlexusEdgeEntityPrefab = GetEntity(authoring.WireframePlexusEdgeEntityPrefab, TransformUsageFlags.Dynamic) });
            }
        }
    }
}
using Unity.Entities;
using UnityEngine;


 namespace WireframePlexus {

    public class PlexusObjectEntitySpawnDataAuthoring : MonoBehaviour {
        public GameObject WireframePlexusObjectPrefab;
        public class Baker : Baker<PlexusObjectEntitySpawnDataAuthoring> {
            public override void Bake(PlexusObjectEntitySpawnDataAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlexusObjectEntitySpawnData { WireframePlexusEntityPrefab = GetEntity(authoring.WireframePlexusObjectPrefab, TransformUsageFlags.Dynamic) });
            }
        }
    }

    public struct PlexusObjectEntitySpawnData : IComponentData {
        public Entity WireframePlexusEntityPrefab;
    }
 }
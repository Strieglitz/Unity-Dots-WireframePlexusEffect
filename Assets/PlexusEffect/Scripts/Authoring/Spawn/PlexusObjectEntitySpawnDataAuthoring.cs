using Unity.Entities;
using UnityEngine;

 namespace WireframePlexus {

    public class PlexusObjectplexusGameObjectAuthoring : MonoBehaviour {
        public GameObject WireframePlexusObjectPrefab;
        public class Baker : Baker<PlexusObjectplexusGameObjectAuthoring> {
            public override void Bake(PlexusObjectplexusGameObjectAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlexusObjectEntitySpawnData { WireframePlexusEntityPrefab = GetEntity(authoring.WireframePlexusObjectPrefab, TransformUsageFlags.Dynamic) });
            }
        }
    }
 }
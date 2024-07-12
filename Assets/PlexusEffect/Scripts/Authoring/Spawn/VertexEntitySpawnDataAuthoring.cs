using Unity.Entities;
using UnityEngine;

namespace WireframePlexus {

public class VertexplexusGameObjectAuthoring : MonoBehaviour {
        public GameObject WireframePlexusVertexEntityPrefab;

        public class Baker : Baker<VertexplexusGameObjectAuthoring> {
            public override void Bake(VertexplexusGameObjectAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new VertexplexusGameObject { WireframePlexusVertexEntityPrefab = GetEntity(authoring.WireframePlexusVertexEntityPrefab, TransformUsageFlags.Dynamic) });
            }
        }
    }
}
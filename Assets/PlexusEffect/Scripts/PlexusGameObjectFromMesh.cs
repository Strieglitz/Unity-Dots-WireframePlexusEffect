using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace WireframePlexus {

    [RequireComponent(typeof(MeshFilter))]
    public class PlexusGameObjectFromMesh : PlexusGameObjectBase {

        MeshFilter meshFilter;

        protected void Awake() {
            this.meshFilter = GetComponent<MeshFilter>();
        }

        protected override int GenerateECSPlexusObject(SpawnSystem spawnSystem, PlexusGameObjectData plexusGameObjectData) {
            return spawnSystem.SpawnPlexusObject(plexusGameObjectData, this, meshFilter.mesh);

        }

        protected override Bounds GetMeshBounds() {
            return meshFilter.mesh.bounds;
        }
    }
}

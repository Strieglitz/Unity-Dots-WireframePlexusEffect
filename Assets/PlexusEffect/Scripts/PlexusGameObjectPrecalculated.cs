using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace WireframePlexus {

    
    public class PlexusGameObjectFromPrecalculated : PlexusGameObjectBase {
        [field: SerializeField]
        [Tooltip("The scriptableObject with the precalculated mesh data so it does not has to be calculated when the plexusObject is created")]
        public PlexusObjectPrecalculatedMeshData PlexusObjectPrecalculatedMeshData { get; private set; }
       

        protected override int GenerateECSPlexusObject(SpawnSystem spawnSystem, PlexusGameObjectData plexusGameObjectData) {
            return spawnSystem.SpawnPlexusObject(plexusGameObjectData, this, PlexusObjectPrecalculatedMeshData);
        }

        protected override Bounds GetMeshBounds() {
            return PlexusObjectPrecalculatedMeshData.mesh.bounds;
        }
    }
}

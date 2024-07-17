using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace WireframePlexus {

    
    public class PlexusGameObjectFromPrecalculated : PlexusGameObjectBase {
        [field: SerializeField]
        [Tooltip("Only draw the wireframes edge when its length is smaller than x Percent of the original length in the mesh")]
        public PlexusObjectPrecalculatedMeshData PlexusObjectPrecalculatedMeshData { get; private set; }

        protected override void GenerateECSPlexusObject(SpawnSystem spawnSystem, PlexusObjectData plexusObjectData) {
            spawnSystem.SpawnPlexusObject(ref wireframePlexusObjectId, plexusObjectData, this, PlexusObjectPrecalculatedMeshData);
        }
    }
}

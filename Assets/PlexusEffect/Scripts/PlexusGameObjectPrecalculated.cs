using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace WireframePlexus {

    
    public class PlexusGameObjectFromPrecalculated : PlexusGameObjectBase {
        [field: SerializeField]
        [Tooltip("The scriptableObject with the precalculated mesh data so it does not has to be calculated when the plexusObject is created")]
        public PlexusObjectPrecalculatedMeshData PlexusObjectPrecalculatedMeshData { get; private set; }
       

        protected override void GenerateECSPlexusObject(SpawnSystem spawnSystem, PlexusGameObjectData plexusGameObjectData) {
            spawnSystem.SpawnPlexusObject(ref wireframePlexusObjectId, plexusGameObjectData, this, PlexusObjectPrecalculatedMeshData);
        }

    }
}

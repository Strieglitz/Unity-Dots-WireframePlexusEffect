using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace WireframePlexus {

    [RequireComponent(typeof(MeshFilter))]
    public class PlexusGameObjectFromMesh : PlexusGameObjectBase {


        protected override int GenerateECSPlexusObject(SpawnSystem spawnSystem, PlexusGameObjectData plexusGameObjectData) {
            return spawnSystem.SpawnPlexusObject(plexusGameObjectData, this, GetComponent<MeshFilter>().mesh);

        }
    }
}

using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace WireframePlexus {

    [RequireComponent(typeof(MeshFilter))]
    public class PlexusGameObjectFromMesh : PlexusGameObjectBase {

        protected override void GenerateECSPlexusObject(SpawnSystem spawnSystem, PlexusObjectData plexusObjectData) {
            spawnSystem.SpawnPlexusObject(ref wireframePlexusObjectId,plexusObjectData, this, GetComponent<MeshFilter>().mesh);
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }
}

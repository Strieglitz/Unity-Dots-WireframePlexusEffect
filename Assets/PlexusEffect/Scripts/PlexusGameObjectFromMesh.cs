using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace WireframePlexus {

    [RequireComponent(typeof(MeshFilter))]
    public class PlexusGameObjectFromMesh : PlexusGameObjectBase {


        protected override void GenerateECSPlexusObject(SpawnSystem spawnSystem, PlexusGameObjectData plexusGameObjectData) {
            spawnSystem.SpawnPlexusObject(ref wireframePlexusObjectId,plexusGameObjectData, this, GetComponent<MeshFilter>().mesh);

        }
    }
}

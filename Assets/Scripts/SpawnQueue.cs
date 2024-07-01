using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace WireframePlexus {

public class SpawnQueue : MonoBehaviour {
        public static SpawnQueue Instance { get; private set; }
        public Queue<EntityBuildData> PlexusBuildDataQueue { get; private set; } = new Queue<EntityBuildData>();

        private void Awake() {
            if (Instance == null || Instance == this) {
                Instance = this;
            } else {
                Debug.LogError("Singelton collision", this);
                Destroy(this.gameObject);
            }
        }

        private void Update() {
            if (PlexusBuildDataQueue.Count > 0) {
                World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SpawnSystem>().Enabled = true;
            }
        }
    }

    public struct EntityBuildData {
        public Mesh Mesh;
        public float MaxEdgeLengthPercent;
        public float MinVertexMoveSpeed;
        public float MaxVertexMoveSpeed;
        public float MaxVertexMoveDistance;
        public Vector3 CameraWorldPos;
        public GameObject PlexusParent;
    }
}
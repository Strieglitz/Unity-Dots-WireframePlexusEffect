using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace WireframePlexus {

    public class SpawnQueue : MonoBehaviour {
        public static SpawnQueue Instance { get; private set; }
        public Queue<EntitySpawnData> PlexusSpawnDataQueue { get; private set; } = new Queue<EntitySpawnData>();

        private void Awake() {
            if (Instance == null || Instance == this) {
                Instance = this;
            } else {
                Debug.LogError("Singelton collision", this);
                Destroy(this.gameObject);
            }
        }

        private void Update() {
            if (PlexusSpawnDataQueue.Count > 0) {
                World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SpawnSystem>().Enabled = true;
            }
        }
    }


}
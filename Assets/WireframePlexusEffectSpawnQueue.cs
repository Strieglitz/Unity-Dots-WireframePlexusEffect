using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class WireframePlexusEffectSpawnQueue : MonoBehaviour
{
    public static WireframePlexusEffectSpawnQueue Instance { get; private set; }
    public Queue<WireframePlexusBuildData> PlexusBuildDataQueue { get; private set; } = new Queue<WireframePlexusBuildData>();

    private void Awake() {
        if(Instance == null || Instance == this) {
            Instance = this;
        }else {
            Debug.LogError("Singelton collision", this);
            Destroy(this.gameObject);
        }
    }

    private void Update() {
        if(PlexusBuildDataQueue.Count > 0) {
            World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PlexusSpawnSystem>().Enabled = true;
        }
    }
}

public struct WireframePlexusBuildData {
    public Mesh Mesh;
    public float MaxEdgeLengthPercent;
    public float MinVertexMoveSpeed;
    public float MaxVertexMoveSpeed;
    public float MaxVertexMoveDistance;
    public Vector3 CameraWorldPos;
    public GameObject PlexusParent;
}

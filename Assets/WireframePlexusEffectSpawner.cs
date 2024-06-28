using UnityEngine;

public class WireframePlexusEffectSpawner : MonoBehaviour
{
    [SerializeField] Mesh mesh;
    
    [SerializeField] [Tooltip("Only draw the wireframes edge when its length is smaller than x Percent of the original length in the mesh")] 
    float maxEdgeLengthPercent;
    
    [SerializeField] [Tooltip("The vertecies are always in motion, relative to their original position in the mesh, this value sets how far from the original possition they can go")] 
    public float maxVertexMoveDistance;

    [SerializeField] [Tooltip("The Minimum Speed a Vertex will have to move randomly around its original position in the mesh")] 
    public float MinVertexMoveSpeed;
    
    [SerializeField] [Tooltip("The Maximum Speed a Vertex will have to move randomly around its original position in the mesh")]
    public float MaxVertexMoveSpeed;

    [SerializeField] public Transform CameraWorldPos;
    [SerializeField] public GameObject PlexusParent;


    private void Start() {
        TestPlexus();
    }

    private void TestPlexus() {
        WireframePlexusEffectSpawnQueue.Instance.PlexusBuildDataQueue.Enqueue(new WireframePlexusBuildData { Mesh = mesh, 
                                                                                        CameraWorldPos = CameraWorldPos.position,
                                                                                        MaxEdgeLengthPercent = maxEdgeLengthPercent,
                                                                                        MaxVertexMoveSpeed = MaxVertexMoveSpeed, 
                                                                                        MinVertexMoveSpeed = MinVertexMoveSpeed,
                                                                                        PlexusParent = PlexusParent,
                                                                                        MaxVertexMoveDistance = maxVertexMoveDistance
});
    }
}

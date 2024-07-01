using UnityEngine;

namespace WireframePlexus {

    public class SpawnerGameObject : MonoBehaviour {
        [SerializeField] 
        Mesh mesh;

        [SerializeField]
        [Tooltip("Only draw the wireframes edge when its length is smaller than x Percent of the original length in the mesh")]
        float maxEdgeLengthPercent;

        [SerializeField]
        [Tooltip("The vertecies are always in motion, relative to their original position in the mesh, this value sets how far from the original possition they can go")]
        float maxVertexMoveDistance;

        [SerializeField]
        [Tooltip("The Minimum Speed a Vertex will have to move randomly around its original position in the mesh")]
        float minVertexMoveSpeed;

        [SerializeField]
        [Tooltip("The Maximum Speed a Vertex will have to move randomly around its original position in the mesh")]
        float maxVertexMoveSpeed;

        [SerializeField] Transform cameraWorldPos;
        [SerializeField] GameObject wireframePlexusParentGameobject;


        private void Start() {
            TestPlexus();
        }

        private void TestPlexus() {
            SpawnQueue.Instance.PlexusBuildDataQueue.Enqueue(new EntitySpawnData {
                Mesh = mesh,
                CameraWorldPos = cameraWorldPos.position,
                MaxEdgeLengthPercent = maxEdgeLengthPercent,
                MaxVertexMoveSpeed = maxVertexMoveSpeed,
                MinVertexMoveSpeed = minVertexMoveSpeed,
                PlexusParent = wireframePlexusParentGameobject,
                MaxVertexMoveDistance = maxVertexMoveDistance
            });
        }
    }
}
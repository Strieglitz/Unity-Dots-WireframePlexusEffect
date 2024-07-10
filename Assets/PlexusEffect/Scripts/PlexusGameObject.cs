using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace WireframePlexus {

    [RequireComponent(typeof(MeshFilter))]
    public class PlexusGameObject : MonoBehaviour {
        
        [SerializeField]
        [Tooltip("Only draw the wireframes edge when its length is smaller than x Percent of the original length in the mesh")]
        float maxEdgeLengthPercent;

        [SerializeField]
        [Tooltip("how thick the edges gonna be")]
        float edgeThickness;

        [SerializeField]
        [Tooltip("The size of the visible vertex particle")]
        float vertexSize;

        [SerializeField]
        [Tooltip("The vertices are always in motion, relative to their original position in the mesh, this value sets how far from the original possition they can go")]
        float maxVertexMoveDistance;

        [SerializeField]
        [Tooltip("The Minimum Speed a Vertex will have to move randomly around its original position in the mesh")]
        float minVertexMoveSpeed;

        [SerializeField]
        [Tooltip("The Maximum Speed a Vertex will have to move randomly around its original position in the mesh")]
        float maxVertexMoveSpeed;

        [SerializeField]
        [ColorUsage(true, true)]
        Color vertexColor;

        [SerializeField]
        [ColorUsage(true, true)]
        Color edgeColor;


        [SerializeField] Transform cameraWorldPos;


        private void Start() {
            TestPlexus();
        }

        private void TestPlexus() {
            SpawnQueue.Instance.PlexusSpawnDataQueue.Enqueue(new EntitySpawnData {
                MeshFilter = gameObject.GetComponent<MeshFilter>(),
                CameraWorldPos = cameraWorldPos.position,
                MaxEdgeLengthPercent = maxEdgeLengthPercent,
                EdgeThickness = edgeThickness,
                VertexSize = vertexSize,
                MaxVertexMoveSpeed = maxVertexMoveSpeed,
                MinVertexMoveSpeed = minVertexMoveSpeed,
                PlexusGameObject = this,
                MaxVertexMoveDistance = maxVertexMoveDistance,
                VertexColor = vertexColor,
                EdgeColor = edgeColor
            });
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }

        public void SetPlexusContactAnimation(ContactColorAnimationData contactColorAnimationData) {
            PlexusObjectSystem plexusObjectSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<PlexusObjectSystem>();
            contactColorAnimationData.CurrentContactDuration = contactColorAnimationData.TotalContactDuration;
            plexusObjectSystem.SetPlexusContactAnimation(this, contactColorAnimationData);
        }
    }
}

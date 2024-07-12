using System.Collections;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace WireframePlexus {

    [RequireComponent(typeof(MeshFilter))]
    public class PlexusGameObject : MonoBehaviour {
        
        [field:SerializeField]
        [Tooltip("Only draw the wireframes edge when its length is smaller than x Percent of the original length in the mesh")]
        public float MaxEdgeLengthPercent { get; private set; }

        [field: SerializeField]
        [Tooltip("how thick the edges gonna be")]
        public float EdgeThickness { get; private set; }

        [field: SerializeField]
        [Tooltip("The size of the visible vertex particle")]
        public float VertexSize { get; private set; }

        [field: SerializeField]
        [Tooltip("The vertices are always in motion, relative to their original position in the mesh, this value sets how far from the original possition they can go")]
        public float MaxVertexMoveDistance { get; private set; }

        [field: SerializeField]
        [Tooltip("The Minimum Speed a Vertex will have to move randomly around its original position in the mesh")]
        public float MinVertexMoveSpeed { get; private set; }

        [field: SerializeField]
        [Tooltip("The Maximum Speed a Vertex will have to move randomly around its original position in the mesh")]
        public float MaxVertexMoveSpeed { get; private set; }

        [field: SerializeField]
        [ColorUsage(true, true)]
        Color vertexColor;
        public float4 VertexColor => new float4(vertexColor.r, vertexColor.g, vertexColor.b, vertexColor.a);

        [field: SerializeField]
        [ColorUsage(true, true)]
        Color edgeColor;
        public float4 EdgeColor => new float4(edgeColor.r, edgeColor.g, edgeColor.b, edgeColor.a);

        int wireframePlexusObjectId;

        private void Start() {
            TestPlexus();
        }

        private void TestPlexus() {
            World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SpawnSystem>().SpawnPlexusObject(ref wireframePlexusObjectId,
                new PlexusObjectData {
                    MaxEdgeLengthPercent = MaxEdgeLengthPercent,
                    EdgeThickness = EdgeThickness,
                    MaxVertexMoveDistance = MaxVertexMoveDistance,
                    MinVertexMoveSpeed = MinVertexMoveSpeed,
                    MaxVertexMoveSpeed = MaxVertexMoveSpeed,
                    VertexColor = VertexColor,
                    EdgeColor = EdgeColor,
                    VertexSize = VertexSize,
                    rotation = transform.rotation
                }, 
                this, 
                GetComponent<MeshFilter>().mesh
                );
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }

        public void SetPlexusContactAnimation(ContactEffectData contactColorAnimationData) {
            contactColorAnimationData.CurrentContactDuration = contactColorAnimationData.TotalContactDuration;
            World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<ContactEffectSpawnSystem>().SpawnContactEffect(contactColorAnimationData, wireframePlexusObjectId);
        }
    }
}

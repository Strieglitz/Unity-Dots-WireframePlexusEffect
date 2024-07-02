using UnityEngine;

namespace WireframePlexus {

    public struct EntitySpawnData {
        public Mesh Mesh;
        public float MaxEdgeLengthPercent;
        public float EdgeThickness;
        public float VertexSize;
        public float MinVertexMoveSpeed;
        public float MaxVertexMoveSpeed;
        public float MaxVertexMoveDistance;
        public Vector3 CameraWorldPos;
        public GameObject PlexusParent;
    }
}
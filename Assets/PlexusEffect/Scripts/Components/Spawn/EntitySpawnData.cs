using System.Collections.Generic;
using UnityEngine;

namespace WireframePlexus {

    public struct EntitySpawnData {
        public MeshFilter MeshFilter;
        public float MaxEdgeLengthPercent;
        public float EdgeThickness;
        public float VertexSize;
        public float MinVertexMoveSpeed;
        public float MaxVertexMoveSpeed;
        public float MaxVertexMoveDistance;
        public Vector3 CameraWorldPos;
        public PlexusGameObject PlexusGameObject;
        public Color EdgeColor;
        public Color VertexColor;
    }
}
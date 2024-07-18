

using System;
using Unity.Mathematics;
using UnityEngine;

namespace WireframePlexus {

    [Serializable]
    public struct PlexusGameObjectData {
        [Tooltip("Only draw the wireframes edge when its length is smaller than x Percent of the original length in the mesh")]
        public float MaxEdgeLengthPercent;
        [Tooltip("how thick the edges gonna be")]
        public float EdgeThickness;
        [Tooltip("The size of the visible vertex particle")]
        public float VertexSize;
        [Tooltip("The vertices are always in motion, relative to their original position in the mesh, this value sets how far from the original possition they can go")]
        public float MaxVertexMoveDistance;
        [Tooltip("The Minimum Speed a Vertex will have to move randomly around its original position in the mesh")]
        public float MinVertexMoveSpeed;
        [Tooltip("The Maximum Speed a Vertex will have to move randomly around its original position in the mesh")]
        public float MaxVertexMoveSpeed;

        [ColorUsage(true, true)]
        public Color VertexColor;
        [ColorUsage(true, true)]
        public Color EdgeColor;

        public PlexusObjectData ToPlexusObjectData() {
            return new PlexusObjectData {
                MaxEdgeLengthPercent = MaxEdgeLengthPercent,
                EdgeThickness = EdgeThickness,
                VertexSize = VertexSize,
                MaxVertexMoveDistance = MaxVertexMoveDistance,
                MinVertexMoveSpeed = MinVertexMoveSpeed,
                MaxVertexMoveSpeed = MaxVertexMoveSpeed,
                VertexColor = new float4(VertexColor.r, VertexColor.g, VertexColor.b, VertexColor.a),
                EdgeColor = new float4(EdgeColor.r, EdgeColor.g, EdgeColor.b, EdgeColor.a)
            };
        }
    }
}
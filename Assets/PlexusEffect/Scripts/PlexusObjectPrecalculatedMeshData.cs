using System.Collections.Generic;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace WireframePlexus {

    [CreateAssetMenu(fileName = "PlexusObjectPrecalculatedData", menuName = "Scriptable Objects/PlexusObjectPrecalculatedData")]
    public class PlexusObjectPrecalculatedMeshData : ScriptableObject {
        
        public Mesh mesh;

        public PrecalculatedMeshVertexData[] precalculatedVertexMeshData;
        public PrecalculatedMeshEdgeData[] precalculatedEdgeMeshData;

        // iterate over mesh when it changed in the editor
        private void OnValidate() {
            if (mesh == null)
                return;

            // calculate like in the spawn system
            Dictionary<int, float3> usedPositionById = new Dictionary<int, float3>();
            Dictionary<float3, int> usedIdByPosition = new Dictionary<float3, int>();
            
            int vertexId = 0;

            for (int i = 0; i < mesh.vertices.Length; i++) {

                float3 pos = mesh.vertices[i];

                if (usedIdByPosition.ContainsKey(pos)) {
                    continue;
                }

                usedPositionById.Add(vertexId, pos);
                usedIdByPosition.Add(pos, vertexId);
                vertexId++;
            }

            precalculatedVertexMeshData = new PrecalculatedMeshVertexData[usedPositionById.Count];
            int index = 0;
            foreach (var item in usedPositionById) {
                precalculatedVertexMeshData[index] = new PrecalculatedMeshVertexData() {
                    VertexId = item.Key,
                    Position = item.Value
                };
                index++;
            }

            Dictionary<Tuple<int, int>, float> edgeDistancesByEdgeConnections = new Dictionary<Tuple<int, int>, float>();

            for (int i = 0; i < mesh.triangles.Length - 2; i = i + 3) {
                int pos1Id = usedIdByPosition[(float3)mesh.vertices[mesh.triangles[i]]];
                int pos2Id = usedIdByPosition[(float3)mesh.vertices[mesh.triangles[i + 1]]];
                int pos3Id = usedIdByPosition[(float3)mesh.vertices[mesh.triangles[i + 2]]];

                if (edgeDistancesByEdgeConnections.ContainsKey(SpawnSystem.EdgePair(pos1Id, pos2Id)) == false) {
                    edgeDistancesByEdgeConnections.Add(SpawnSystem.EdgePair(pos1Id, pos2Id), math.distance(usedPositionById[pos1Id], usedPositionById[pos2Id]));
                }
                if (edgeDistancesByEdgeConnections.ContainsKey(SpawnSystem.EdgePair(pos2Id, pos3Id)) == false) {
                    edgeDistancesByEdgeConnections.Add(SpawnSystem.EdgePair(pos2Id, pos3Id), math.distance(usedPositionById[pos2Id], usedPositionById[pos3Id]));
                }
                if (edgeDistancesByEdgeConnections.ContainsKey(SpawnSystem.EdgePair(pos1Id, pos3Id)) == false) {
                    edgeDistancesByEdgeConnections.Add(SpawnSystem.EdgePair(pos1Id, pos3Id), math.distance(usedPositionById[pos3Id], usedPositionById[pos1Id]));
                }
            }

            precalculatedEdgeMeshData = new PrecalculatedMeshEdgeData[edgeDistancesByEdgeConnections.Count];
            index = 0;
            foreach (var item in edgeDistancesByEdgeConnections) {
                precalculatedEdgeMeshData[index] = new PrecalculatedMeshEdgeData() {
                    Vertex1Id = item.Key.Item1,
                    Vertex2Id = item.Key.Item2,
                    Distance = item.Value
                };
                index++;
            }
        }
    }

    [Serializable]
    public struct PrecalculatedMeshVertexData {
        public int VertexId;
        public float3 Position;

    }
    [Serializable]
    public struct PrecalculatedMeshEdgeData {
        public int Vertex1Id;
        public int Vertex2Id;
        public float Distance;

    }
}
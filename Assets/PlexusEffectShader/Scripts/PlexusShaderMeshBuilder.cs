using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WireframePlexusShader {

    /// <summary>
    /// Turns a regular mesh into a "plexus mesh": one quad per unique edge and one quad per unique vertex.
    /// All motion happens in the vertex shader, so every quad corner only stores what the shader needs
    /// to find its place on its own:
    ///
    ///   POSITION   rest position of the vertex (dot) or of this end of the edge
    ///   TEXCOORD0  x, y = quad corner (dot) or ribbon side and end flag (edge), z = type, w = id of this vertex
    ///   TEXCOORD1  xyz = rest position of the other end of the edge, w = id of that vertex
    ///
    /// The ids are what the shader hashes to animate a vertex. Both ends of an edge carry the id of the dot
    /// they are attached to, which is why lines and dots always meet without any data going back to the CPU.
    /// </summary>
    public static class PlexusShaderMeshBuilder {

        public const float TypeDot = 0f;
        public const float TypeEdge = 1f;

        public struct Result {
            public Mesh Mesh;
            public int VertexCount;
            public int EdgeCount;
        }

        public static Result Build(Mesh source) {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (!source.isReadable) {
                throw new InvalidOperationException(
                    $"Mesh '{source.name}' is not readable. Enable Read/Write in its import settings, " +
                    "or bake a plexus mesh asset in the editor and assign it instead.");
            }

            // Fetch the arrays once. Mesh.vertices and Mesh.triangles copy the whole array on every access.
            Vector3[] sourceVertices = source.vertices;

            // 1. Collapse vertices that share a position. Meshes duplicate them along UV and normal seams.
            var idByPosition = new Dictionary<Vector3, int>(sourceVertices.Length);
            var positions = new List<Vector3>(sourceVertices.Length);
            var idBySourceIndex = new int[sourceVertices.Length];
            for (int i = 0; i < sourceVertices.Length; i++) {
                Vector3 p = sourceVertices[i];
                p.x += 0f; p.y += 0f; p.z += 0f; // turns -0 into +0 so both hash the same
                if (!idByPosition.TryGetValue(p, out int id)) {
                    id = positions.Count;
                    idByPosition.Add(p, id);
                    positions.Add(p);
                }
                idBySourceIndex[i] = id;
            }

            // 2. Collect every triangle edge once.
            var seenEdges = new HashSet<ulong>();
            var edgeStarts = new List<int>();
            var edgeEnds = new List<int>();
            for (int subMesh = 0; subMesh < source.subMeshCount; subMesh++) {
                if (source.GetTopology(subMesh) != MeshTopology.Triangles) continue;
                int[] triangles = source.GetTriangles(subMesh);
                for (int i = 0; i + 2 < triangles.Length; i += 3) {
                    int a = idBySourceIndex[triangles[i]];
                    int b = idBySourceIndex[triangles[i + 1]];
                    int c = idBySourceIndex[triangles[i + 2]];
                    AddEdge(a, b, seenEdges, edgeStarts, edgeEnds);
                    AddEdge(b, c, seenEdges, edgeStarts, edgeEnds);
                    AddEdge(a, c, seenEdges, edgeStarts, edgeEnds);
                }
            }

            // 3. Emit the quads. Edges first, so dots draw on top of them inside the single draw call.
            int edgeCount = edgeStarts.Count;
            int dotCount = positions.Count;
            int quadCount = edgeCount + dotCount;
            var outPositions = new Vector3[quadCount * 4];
            var outUv0 = new Vector4[quadCount * 4];
            var outUv1 = new Vector4[quadCount * 4];
            var outIndices = new int[quadCount * 6];

            int v = 0;
            int t = 0;
            for (int e = 0; e < edgeCount; e++) {
                int idA = edgeStarts[e];
                int idB = edgeEnds[e];
                Vector3 a = positions[idA];
                Vector3 b = positions[idB];
                var otherIsB = new Vector4(b.x, b.y, b.z, idB);
                var otherIsA = new Vector4(a.x, a.y, a.z, idA);

                WriteQuadIndices(outIndices, ref t, v);
                // uv0: x = ribbon side, y = end flag (0 = start, 1 = end)
                outPositions[v] = a; outUv0[v] = new Vector4(-1f, 0f, TypeEdge, idA); outUv1[v] = otherIsB; v++;
                outPositions[v] = a; outUv0[v] = new Vector4(1f, 0f, TypeEdge, idA); outUv1[v] = otherIsB; v++;
                outPositions[v] = b; outUv0[v] = new Vector4(1f, 1f, TypeEdge, idB); outUv1[v] = otherIsA; v++;
                outPositions[v] = b; outUv0[v] = new Vector4(-1f, 1f, TypeEdge, idB); outUv1[v] = otherIsA; v++;
            }
            for (int id = 0; id < dotCount; id++) {
                Vector3 p = positions[id];
                var self = new Vector4(p.x, p.y, p.z, id);

                WriteQuadIndices(outIndices, ref t, v);
                // uv0: xy = quad corner
                outPositions[v] = p; outUv0[v] = new Vector4(-1f, -1f, TypeDot, id); outUv1[v] = self; v++;
                outPositions[v] = p; outUv0[v] = new Vector4(1f, -1f, TypeDot, id); outUv1[v] = self; v++;
                outPositions[v] = p; outUv0[v] = new Vector4(1f, 1f, TypeDot, id); outUv1[v] = self; v++;
                outPositions[v] = p; outUv0[v] = new Vector4(-1f, 1f, TypeDot, id); outUv1[v] = self; v++;
            }

            var mesh = new Mesh {
                name = source.name + " (Plexus)",
                indexFormat = outPositions.Length > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16
            };
            mesh.SetVertices(outPositions);
            mesh.SetUVs(0, outUv0);
            mesh.SetUVs(1, outUv1);
            mesh.SetIndices(outIndices, MeshTopology.Triangles, 0, false);
            // The shader moves everything, so these are only a starting point. PlexusShaderObject widens
            // the renderer's bounds by the move distance.
            mesh.bounds = source.bounds;

            return new Result { Mesh = mesh, VertexCount = dotCount, EdgeCount = edgeCount };
        }

        static void AddEdge(int a, int b, HashSet<ulong> seen, List<int> starts, List<int> ends) {
            if (a == b) return; // degenerate triangle side
            int low = a < b ? a : b;
            int high = a < b ? b : a;
            ulong key = ((ulong)(uint)low << 32) | (uint)high;
            if (!seen.Add(key)) return;
            starts.Add(low);
            ends.Add(high);
        }

        static void WriteQuadIndices(int[] indices, ref int t, int firstVertex) {
            indices[t++] = firstVertex;
            indices[t++] = firstVertex + 1;
            indices[t++] = firstVertex + 2;
            indices[t++] = firstVertex;
            indices[t++] = firstVertex + 2;
            indices[t++] = firstVertex + 3;
        }
    }
}

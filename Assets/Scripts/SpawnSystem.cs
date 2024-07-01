using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WireframePlexus {

    public partial class SpawnSystem : SystemBase {


        int plexusObjectId = 0;
        Unity.Mathematics.Random random = new Unity.Mathematics.Random(1);

        protected override void OnCreate() {
            RequireForUpdate<VertexEntitySpawnData>();
            RequireForUpdate<PlexusObjectEntitySpawnData>();
            RequireForUpdate<EdgeEntitySpawnData>();
        }

        protected override void OnUpdate() {
            if (!SystemAPI.TryGetSingleton<VertexEntitySpawnData>(out VertexEntitySpawnData wireframePlexusVertexSpawnData)) {
                World.EntityManager.CreateSingleton<VertexEntitySpawnData>();
            }
            wireframePlexusVertexSpawnData = SystemAPI.GetSingleton<VertexEntitySpawnData>();

            if (!SystemAPI.TryGetSingleton<PlexusObjectEntitySpawnData>(out PlexusObjectEntitySpawnData wireframePlexusObjectSpawnData)) {
                World.EntityManager.CreateSingleton<PlexusObjectEntitySpawnData>();
            }
            wireframePlexusObjectSpawnData = SystemAPI.GetSingleton<PlexusObjectEntitySpawnData>();

            if (!SystemAPI.TryGetSingleton<EdgeEntitySpawnData>(out EdgeEntitySpawnData wireframePlexusEdgeSpawnData)) {
                World.EntityManager.CreateSingleton<EdgeEntitySpawnData>();
            }
            wireframePlexusEdgeSpawnData = SystemAPI.GetSingleton<EdgeEntitySpawnData>();

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (EntitySpawnData plexusBuildData in SpawnQueue.Instance.PlexusBuildDataQueue) {


                Entity wireframePlexusObjectEntity = ecb.Instantiate(wireframePlexusObjectSpawnData.WireframePlexusEntityPrefab);

                SyncEntityPositionToGameobjectPositionData parentReference = new SyncEntityPositionToGameobjectPositionData();
                parentReference.gameObject = plexusBuildData.PlexusParent;
                ecb.SetComponent(wireframePlexusObjectEntity, parentReference);

                int points = plexusBuildData.Mesh.triangles.Length;
                ecb.SetComponent(wireframePlexusObjectEntity, new PlexusObjectData {
                    VertexPositions = new NativeArray<float3>(points, Allocator.Persistent),
                    MaxVertexMoveSpeed = plexusBuildData.MaxVertexMoveSpeed,
                    MinVertexMoveSpeed = plexusBuildData.MinVertexMoveSpeed,
                    MaxVertexMoveDistance = plexusBuildData.MaxVertexMoveDistance,
                    MaxEdgeLengthPercent = plexusBuildData.MaxEdgeLengthPercent,
                    WireframePlexusObjectId = plexusObjectId
                });

                Dictionary<int, float3> usedPositionById = new Dictionary<int, float3>();
                Dictionary<float3, int> usedIdByPosition = new Dictionary<float3, int>();
                int pointId = 0;
                for (int i = 0; i < plexusBuildData.Mesh.vertices.Length; i++) {
                    if (usedPositionById.ContainsValue(plexusBuildData.Mesh.vertices[i])) {
                        continue;
                    }

                    float3 pos = plexusBuildData.Mesh.vertices[i];
                    usedPositionById.Add(i, pos);
                    usedIdByPosition.Add(pos, i);

                    Entity plexusPointEntity = ecb.Instantiate(wireframePlexusVertexSpawnData.WireframePlexusVertexEntityPrefab);
                    VertexMovementData movementData = new VertexMovementData();
                    movementData.Position = pos;
                    movementData.PositionTarget = pos;

                    movementData.MoveSpeed = UnityEngine.Random.Range(plexusBuildData.MinVertexMoveSpeed, plexusBuildData.MaxVertexMoveSpeed);
                    movementData.Random = new Unity.Mathematics.Random(random.NextUInt());
                    movementData.PointId = pointId;
                    pointId++;


                    ecb.SetComponent(plexusPointEntity, movementData);
                    ecb.AddSharedComponent(plexusPointEntity, new PlexusObjectIdData { ObjectId = plexusObjectId });
                    ecb.AddComponent(plexusPointEntity, new Parent { Value = wireframePlexusObjectEntity });

                }

                List<Connection> connections = new List<Connection>();
                for (int i = 0; i < plexusBuildData.Mesh.triangles.Length - 2; i = i + 3) {
                    int pos1Id = plexusBuildData.Mesh.triangles[i];
                    int pos2Id = plexusBuildData.Mesh.triangles[i + 1];
                    int pos3Id = plexusBuildData.Mesh.triangles[i + 2];
                    if (usedPositionById.ContainsKey(pos1Id) == false) {
                        pos1Id = usedIdByPosition[plexusBuildData.Mesh.vertices[pos1Id]];
                    }
                    if (usedPositionById.ContainsKey(pos2Id) == false) {
                        pos2Id = usedIdByPosition[plexusBuildData.Mesh.vertices[pos2Id]];
                    }
                    if (usedPositionById.ContainsKey(pos3Id) == false) {
                        pos3Id = usedIdByPosition[plexusBuildData.Mesh.vertices[pos3Id]];
                    }

                    if (connections.Contains(new Connection { Id1 = pos1Id, Id2 = pos2Id }) == false && connections.Contains(new Connection { Id1 = pos2Id, Id2 = pos1Id }) == false) {
                        connections.Add(new Connection { Id1 = pos1Id, Id2 = pos2Id });
                        AddPlexusEdge(ref ecb, wireframePlexusEdgeSpawnData.WireframePlexusEdgeEntityPrefab, pos1Id, pos2Id, math.distance(plexusBuildData.Mesh.vertices[pos1Id], plexusBuildData.Mesh.vertices[pos2Id]), wireframePlexusObjectEntity);
                    }
                    if (connections.Contains(new Connection { Id1 = pos2Id, Id2 = pos3Id }) == false && connections.Contains(new Connection { Id1 = pos3Id, Id2 = pos2Id }) == false) {
                        connections.Add(new Connection { Id1 = pos2Id, Id2 = pos3Id });
                        AddPlexusEdge(ref ecb, wireframePlexusEdgeSpawnData.WireframePlexusEdgeEntityPrefab, pos2Id, pos3Id, math.distance(plexusBuildData.Mesh.vertices[pos2Id], plexusBuildData.Mesh.vertices[pos3Id]), wireframePlexusObjectEntity);
                    }
                    if (connections.Contains(new Connection { Id1 = pos1Id, Id2 = pos3Id }) == false && connections.Contains(new Connection { Id1 = pos3Id, Id2 = pos1Id }) == false) {
                        connections.Add(new Connection { Id1 = pos1Id, Id2 = pos3Id });
                        AddPlexusEdge(ref ecb, wireframePlexusEdgeSpawnData.WireframePlexusEdgeEntityPrefab, pos1Id, pos3Id, math.distance(plexusBuildData.Mesh.vertices[pos3Id], plexusBuildData.Mesh.vertices[pos1Id]), wireframePlexusObjectEntity);
                    }
                }
                plexusObjectId++;
            }
            Debug.Log("Playback Beginn");
            ecb.Playback(EntityManager);
            Debug.Log("Playback Finished");

            SpawnQueue.Instance.PlexusBuildDataQueue.Clear();
            Enabled = false;
        }

        private void AddPlexusEdge(ref EntityCommandBuffer ecb, Entity plexusEdgePrefabEntity, int pointId1, int pointId2, float plexusEdgeLength, Entity parentEntity) {
            Entity plexusEdgeEntity = ecb.Instantiate(plexusEdgePrefabEntity);
            ecb.AddSharedComponent(plexusEdgeEntity, new PlexusObjectIdData { ObjectId = plexusObjectId });
            ecb.SetComponent(plexusEdgeEntity, new EdgeData { Vertex1Index = pointId1, Vertex2Index = pointId2, Length = plexusEdgeLength });
            ecb.AddComponent(plexusEdgeEntity, new Parent { Value = parentEntity });
        }

    }
    struct Connection {
        public int Id1;
        public int Id2;
    }
}
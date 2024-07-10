using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
            foreach (EntitySpawnData entitySpawnData in SpawnQueue.Instance.PlexusSpawnDataQueue) {

                // create an parent entity
                Entity wireframePlexusObjectEntity = ecb.Instantiate(wireframePlexusObjectSpawnData.WireframePlexusEntityPrefab);

                SyncEntityPositionToGameobjectPositionData parentReference = new SyncEntityPositionToGameobjectPositionData();
                parentReference.PlexusGameObject = entitySpawnData.PlexusGameObject;
                ecb.SetComponent(wireframePlexusObjectEntity, parentReference);

                int points = entitySpawnData.MeshFilter.mesh.triangles.Length;
                ecb.AddComponent(wireframePlexusObjectEntity, new PlexusObjectData {
                    VertexPositions = new NativeArray<float3>(points, Allocator.Persistent),
                    MaxVertexMoveSpeed = entitySpawnData.MaxVertexMoveSpeed,
                    MinVertexMoveSpeed = entitySpawnData.MinVertexMoveSpeed,
                    MaxVertexMoveDistance = entitySpawnData.MaxVertexMoveDistance,
                    MaxEdgeLengthPercent = entitySpawnData.MaxEdgeLengthPercent,
                    EdgeThickness = entitySpawnData.EdgeThickness,
                    VertexSize = entitySpawnData.VertexSize,
                    WireframePlexusObjectId = plexusObjectId,
                    EdgeColor = new float4(entitySpawnData.EdgeColor.r, entitySpawnData.EdgeColor.g, entitySpawnData.EdgeColor.b, entitySpawnData.EdgeColor.a),
                    VertexColor = new float4(entitySpawnData.VertexColor.r, entitySpawnData.VertexColor.g, entitySpawnData.VertexColor.b, entitySpawnData.VertexColor.a),
                });

                // create the vertex entities
                Dictionary<int, float3> usedPositionById = new Dictionary<int, float3>();
                Dictionary<float3, int> usedIdByPosition = new Dictionary<float3, int>();
                int pointId = 0;


                for (int i = 0; i < entitySpawnData.MeshFilter.mesh.vertices.Length; i++) {

                    float3 pos = entitySpawnData.MeshFilter.mesh.vertices[i];

                    if (usedIdByPosition.ContainsKey(pos)) {
                        continue;
                    }

                    usedPositionById.Add(pointId, pos);
                    usedIdByPosition.Add(pos, pointId);


                    Entity plexusVertexEntity = ecb.Instantiate(wireframePlexusVertexSpawnData.WireframePlexusVertexEntityPrefab);
                    VertexMovementData movementData = new VertexMovementData();
                    movementData.Position = pos;
                    movementData.PositionTarget = pos;

                    movementData.MoveSpeed = UnityEngine.Random.Range(entitySpawnData.MinVertexMoveSpeed, entitySpawnData.MaxVertexMoveSpeed);
                    movementData.Random = new Unity.Mathematics.Random(random.NextUInt());
                    movementData.PointId = pointId;

                    ecb.SetComponent(plexusVertexEntity, movementData);
                    ecb.SetComponent(plexusVertexEntity, new LocalTransform { Position = pos, Scale = entitySpawnData.VertexSize });
                    ecb.SetComponent(plexusVertexEntity, new VertexColorData { Value = new float4(entitySpawnData.VertexColor.r, entitySpawnData.VertexColor.g, entitySpawnData.VertexColor.b, entitySpawnData.VertexColor.a) });
                    ecb.AddSharedComponent(plexusVertexEntity, new PlexusObjectIdData { ObjectId = plexusObjectId });
                    ecb.AddComponent(plexusVertexEntity, new Parent { Value = wireframePlexusObjectEntity });

                    pointId++;
                }


                // create the edge entities
                List<Connection> connections = new List<Connection>();
                
                for (int i = 0; i < entitySpawnData.MeshFilter.mesh.triangles.Length - 2; i = i + 3) {
                    int pos1Id = usedIdByPosition[(float3)entitySpawnData.MeshFilter.mesh.vertices[entitySpawnData.MeshFilter.mesh.triangles[i]]];
                    int pos2Id = usedIdByPosition[(float3)entitySpawnData.MeshFilter.mesh.vertices[entitySpawnData.MeshFilter.mesh.triangles[i + 1]]];
                    int pos3Id = usedIdByPosition[(float3)entitySpawnData.MeshFilter.mesh.vertices[entitySpawnData.MeshFilter.mesh.triangles[i + 2]]];

                    if (connections.Contains(new Connection { Id1 = pos1Id, Id2 = pos2Id }) == false && connections.Contains(new Connection { Id1 = pos2Id, Id2 = pos1Id }) == false) {
                        connections.Add(new Connection { Id1 = pos1Id, Id2 = pos2Id });
                        AddPlexusEdge(ref ecb, wireframePlexusEdgeSpawnData.WireframePlexusEdgeEntityPrefab, pos1Id, pos2Id, math.distance(usedPositionById[pos1Id], usedPositionById[pos2Id]), wireframePlexusObjectEntity, entitySpawnData);
                    }
                    if (connections.Contains(new Connection { Id1 = pos2Id, Id2 = pos3Id }) == false && connections.Contains(new Connection { Id1 = pos3Id, Id2 = pos2Id }) == false) {
                        connections.Add(new Connection { Id1 = pos2Id, Id2 = pos3Id });
                        AddPlexusEdge(ref ecb, wireframePlexusEdgeSpawnData.WireframePlexusEdgeEntityPrefab, pos2Id, pos3Id, math.distance(usedPositionById[pos2Id], usedPositionById[pos3Id]), wireframePlexusObjectEntity, entitySpawnData);
                    }
                    if (connections.Contains(new Connection { Id1 = pos1Id, Id2 = pos3Id }) == false && connections.Contains(new Connection { Id1 = pos3Id, Id2 = pos1Id }) == false) {
                        connections.Add(new Connection { Id1 = pos1Id, Id2 = pos3Id });
                        AddPlexusEdge(ref ecb, wireframePlexusEdgeSpawnData.WireframePlexusEdgeEntityPrefab, pos1Id, pos3Id, math.distance(usedPositionById[pos3Id], usedPositionById[pos1Id]), wireframePlexusObjectEntity, entitySpawnData);
                    }

                }
                plexusObjectId++;
            }

            ecb.Playback(EntityManager);
            SpawnQueue.Instance.PlexusSpawnDataQueue.Clear();
            Enabled = false;
        }

        private void AddPlexusEdge(ref EntityCommandBuffer ecb,
                                    Entity plexusEdgePrefabEntity,
                                    int pointId1,
                                    int pointId2,
                                    float edgeLength,
                                    Entity parentEntity,
                                    EntitySpawnData entitySpawnData) {
            Entity plexusEdgeEntity = ecb.Instantiate(plexusEdgePrefabEntity);
            ecb.AddSharedComponent(plexusEdgeEntity, new PlexusObjectIdData { ObjectId = plexusObjectId });
            ecb.SetComponent(plexusEdgeEntity, new EdgeData { Vertex1Index = pointId1, Vertex2Index = pointId2, Length = edgeLength });
            ecb.SetComponent(plexusEdgeEntity, new EdgeColorData { Value = new float4(entitySpawnData.EdgeColor.r, entitySpawnData.EdgeColor.g, entitySpawnData.EdgeColor.b, entitySpawnData.EdgeColor.a) });
            ecb.AddComponent(plexusEdgeEntity, new Parent { Value = parentEntity });
        }
    }

    struct Connection {
        public int Id1;
        public int Id2;
    }
}
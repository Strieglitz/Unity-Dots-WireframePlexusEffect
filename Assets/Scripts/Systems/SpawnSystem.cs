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
            foreach (EntitySpawnData entitySpawnData in SpawnQueue.Instance.PlexusSpawnDataQueue) {


                Entity wireframePlexusObjectEntity = ecb.Instantiate(wireframePlexusObjectSpawnData.WireframePlexusEntityPrefab);

                SyncEntityPositionToGameobjectPositionData parentReference = new SyncEntityPositionToGameobjectPositionData();
                parentReference.gameObject = entitySpawnData.PlexusParent;
                ecb.SetComponent(wireframePlexusObjectEntity, parentReference);

                int points = entitySpawnData.Mesh.triangles.Length;
                ecb.AddComponent(wireframePlexusObjectEntity, new PlexusObjectData {
                    VertexPositions = new NativeArray<float3>(points, Allocator.Persistent),
                    MaxVertexMoveSpeed = entitySpawnData.MaxVertexMoveSpeed,
                    MinVertexMoveSpeed = entitySpawnData.MinVertexMoveSpeed,
                    MaxVertexMoveDistance = entitySpawnData.MaxVertexMoveDistance,
                    MaxEdgeLengthPercent = entitySpawnData.MaxEdgeLengthPercent,
                    EdgeThickness = entitySpawnData.EdgeThickness,
                    VertexSize = entitySpawnData.VertexSize,
                    WireframePlexusObjectId = plexusObjectId
                });

                Dictionary<int, float3> usedPositionById = new Dictionary<int, float3>();
                Dictionary<float3, int> usedIdByPosition = new Dictionary<float3, int>();
                int pointId = 0;
                for (int i = 0; i < entitySpawnData.Mesh.vertices.Length; i++) {
                    if (usedPositionById.ContainsValue(entitySpawnData.Mesh.vertices[i])) {
                        continue;
                    }

                    float3 pos = entitySpawnData.Mesh.vertices[i];
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
                    ecb.AddSharedComponent(plexusVertexEntity, new PlexusObjectIdData { ObjectId = plexusObjectId });
                    ecb.AddComponent(plexusVertexEntity, new Parent { Value = wireframePlexusObjectEntity });

                    pointId++;
                }

                List<Connection> connections = new List<Connection>();
                for (int i = 0; i < entitySpawnData.Mesh.triangles.Length - 2; i = i + 3) {
                    int pos1Id = usedIdByPosition[entitySpawnData.Mesh.vertices[entitySpawnData.Mesh.triangles[i]]];
                    int pos2Id = usedIdByPosition[entitySpawnData.Mesh.vertices[entitySpawnData.Mesh.triangles[i+1]]];
                    int pos3Id = usedIdByPosition[entitySpawnData.Mesh.vertices[entitySpawnData.Mesh.triangles[i+2]]];

                    if (connections.Contains(new Connection { Id1 = pos1Id, Id2 = pos2Id }) == false && connections.Contains(new Connection { Id1 = pos2Id, Id2 = pos1Id }) == false) {
                        connections.Add(new Connection { Id1 = pos1Id, Id2 = pos2Id });
                        AddPlexusEdge(ref ecb, wireframePlexusEdgeSpawnData.WireframePlexusEdgeEntityPrefab, pos1Id, pos2Id, math.distance(usedPositionById[pos1Id], usedPositionById[pos2Id]), wireframePlexusObjectEntity);
                    }
                    if (connections.Contains(new Connection { Id1 = pos2Id, Id2 = pos3Id }) == false && connections.Contains(new Connection { Id1 = pos3Id, Id2 = pos2Id }) == false) {
                        connections.Add(new Connection { Id1 = pos2Id, Id2 = pos3Id });
                        AddPlexusEdge(ref ecb, wireframePlexusEdgeSpawnData.WireframePlexusEdgeEntityPrefab, pos2Id, pos3Id, math.distance(usedPositionById[pos2Id], usedPositionById[pos3Id]), wireframePlexusObjectEntity);
                    }
                    if (connections.Contains(new Connection { Id1 = pos1Id, Id2 = pos3Id }) == false && connections.Contains(new Connection { Id1 = pos3Id, Id2 = pos1Id }) == false) {
                        connections.Add(new Connection { Id1 = pos1Id, Id2 = pos3Id });
                        AddPlexusEdge(ref ecb, wireframePlexusEdgeSpawnData.WireframePlexusEdgeEntityPrefab, pos1Id, pos3Id, math.distance(usedPositionById[pos3Id], usedPositionById[pos1Id]), wireframePlexusObjectEntity);
                    }
                }
                plexusObjectId++;
            }

            ecb.Playback(EntityManager);
            SpawnQueue.Instance.PlexusSpawnDataQueue.Clear();
            Enabled = false;
        }

        private void AddPlexusEdge( ref EntityCommandBuffer ecb, 
                                    Entity plexusEdgePrefabEntity, 
                                    int pointId1, 
                                    int pointId2, 
                                    float edgeLength, 
                                    Entity parentEntity){
            Entity plexusEdgeEntity = ecb.Instantiate(plexusEdgePrefabEntity);
            ecb.AddSharedComponent(plexusEdgeEntity, new PlexusObjectIdData { ObjectId = plexusObjectId });
            ecb.SetComponent(plexusEdgeEntity, new EdgeData { Vertex1Index = pointId1, Vertex2Index = pointId2, Length = edgeLength });
            ecb.AddComponent(plexusEdgeEntity, new Parent { Value = parentEntity });
        }

    }
    struct Connection {
        public int Id1;
        public int Id2;
    }
}
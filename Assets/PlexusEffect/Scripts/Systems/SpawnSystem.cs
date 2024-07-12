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
            RequireForUpdate<VertexplexusGameObject>();
            RequireForUpdate<PlexusObjectplexusGameObject>();
            RequireForUpdate<EdgeplexusGameObject>();
        }

        public void SpawnPlexusObject(ref int PlexusObjectToSpawnId, PlexusObjectData plexusObjectData,PlexusGameObject plexusGameObject, Mesh mesh) {
            if (!SystemAPI.TryGetSingleton<VertexplexusGameObject>(out VertexplexusGameObject wireframePlexusVertexSpawnData)) {
                World.EntityManager.CreateSingleton<VertexplexusGameObject>();
            }
            wireframePlexusVertexSpawnData = SystemAPI.GetSingleton<VertexplexusGameObject>();
            
            if (!SystemAPI.TryGetSingleton<PlexusObjectplexusGameObject>(out PlexusObjectplexusGameObject wireframePlexusObjectSpawnData)) {
                World.EntityManager.CreateSingleton<PlexusObjectplexusGameObject>();
            }
            wireframePlexusObjectSpawnData = SystemAPI.GetSingleton<PlexusObjectplexusGameObject>();

            if (!SystemAPI.TryGetSingleton<EdgeplexusGameObject>(out EdgeplexusGameObject wireframePlexusEdgeSpawnData)) {
                World.EntityManager.CreateSingleton<EdgeplexusGameObject>();
            }
            wireframePlexusEdgeSpawnData = SystemAPI.GetSingleton<EdgeplexusGameObject>();

            var ecb = new EntityCommandBuffer(Allocator.Temp);


            // create an parent entity
            Entity wireframePlexusObjectEntity = ecb.Instantiate(wireframePlexusObjectSpawnData.WireframePlexusEntityPrefab);

            SyncEntityPositionToGameobjectPositionData parentReference = new SyncEntityPositionToGameobjectPositionData();
            parentReference.PlexusGameObject = plexusGameObject;
            ecb.SetComponent(wireframePlexusObjectEntity, parentReference);

            

            int points = mesh.triangles.Length;
            plexusObjectData.VertexPositions = new NativeArray<float3>(points, Allocator.Persistent);
            plexusObjectData.ContactAnimationColorData = new NativeList<ContactEffectData>(Allocator.Persistent);
            plexusObjectData.WireframePlexusObjectId = plexusObjectId;

            ecb.AddComponent(wireframePlexusObjectEntity, plexusObjectData);

            // create the vertex entities
            Dictionary<int, float3> usedPositionById = new Dictionary<int, float3>();
            Dictionary<float3, int> usedIdByPosition = new Dictionary<float3, int>();
            int pointId = 0;

            for (int i = 0; i < mesh.vertices.Length; i++) {

                float3 pos = mesh.vertices[i];

                if (usedIdByPosition.ContainsKey(pos)) {
                    continue;
                }

                usedPositionById.Add(pointId, pos);
                usedIdByPosition.Add(pos, pointId);


                Entity plexusVertexEntity = ecb.Instantiate(wireframePlexusVertexSpawnData.WireframePlexusVertexEntityPrefab);
                VertexMovementData movementData = new VertexMovementData();
                movementData.Position = pos;
                movementData.PositionTarget = pos;

                movementData.MoveSpeed = UnityEngine.Random.Range(plexusGameObject.MinVertexMoveSpeed, plexusGameObject.MaxVertexMoveSpeed);
                movementData.Random = new Unity.Mathematics.Random(random.NextUInt());
                movementData.PointId = pointId;

                ecb.SetComponent(plexusVertexEntity, movementData);
                ecb.SetComponent(plexusVertexEntity, new LocalTransform { Position = pos, Scale = plexusGameObject.VertexSize });
                ecb.SetComponent(plexusVertexEntity, new VertexColorData { Value = plexusGameObject.VertexColor});
                ecb.AddSharedComponent(plexusVertexEntity, new PlexusObjectIdData { ObjectId = plexusObjectId });
                ecb.AddComponent(plexusVertexEntity, new Parent { Value = wireframePlexusObjectEntity });

                pointId++;
            }


            // create the edge entities
            List<Connection> connections = new List<Connection>();
            for (int i = 0; i < mesh.triangles.Length - 2; i = i + 3) {
                int pos1Id = usedIdByPosition[(float3)mesh.vertices[mesh.triangles[i]]];
                int pos2Id = usedIdByPosition[(float3)mesh.vertices[mesh.triangles[i + 1]]];
                int pos3Id = usedIdByPosition[(float3)mesh.vertices[mesh.triangles[i + 2]]];

                if (connections.Contains(new Connection { Id1 = pos1Id, Id2 = pos2Id }) == false && connections.Contains(new Connection { Id1 = pos2Id, Id2 = pos1Id }) == false) {
                    connections.Add(new Connection { Id1 = pos1Id, Id2 = pos2Id });
                    AddPlexusEdge(ref ecb, wireframePlexusEdgeSpawnData.WireframePlexusEdgeEntityPrefab, pos1Id, pos2Id, math.distance(usedPositionById[pos1Id], usedPositionById[pos2Id]), wireframePlexusObjectEntity, plexusObjectData);
                }
                if (connections.Contains(new Connection { Id1 = pos2Id, Id2 = pos3Id }) == false && connections.Contains(new Connection { Id1 = pos3Id, Id2 = pos2Id }) == false) {
                    connections.Add(new Connection { Id1 = pos2Id, Id2 = pos3Id });
                    AddPlexusEdge(ref ecb, wireframePlexusEdgeSpawnData.WireframePlexusEdgeEntityPrefab, pos2Id, pos3Id, math.distance(usedPositionById[pos2Id], usedPositionById[pos3Id]), wireframePlexusObjectEntity, plexusObjectData);
                }
                if (connections.Contains(new Connection { Id1 = pos1Id, Id2 = pos3Id }) == false && connections.Contains(new Connection { Id1 = pos3Id, Id2 = pos1Id }) == false) {
                    connections.Add(new Connection { Id1 = pos1Id, Id2 = pos3Id });
                    AddPlexusEdge(ref ecb, wireframePlexusEdgeSpawnData.WireframePlexusEdgeEntityPrefab, pos1Id, pos3Id, math.distance(usedPositionById[pos3Id], usedPositionById[pos1Id]), wireframePlexusObjectEntity, plexusObjectData);
                }


                
            }
            PlexusObjectToSpawnId = plexusObjectId;
            plexusObjectId++;
            ecb.Playback(EntityManager);
        }

        protected override void OnUpdate() {
        }

        private void AddPlexusEdge(ref EntityCommandBuffer ecb,
                                    Entity plexusEdgePrefabEntity,
                                    int pointId1,
                                    int pointId2,
                                    float edgeLength,
                                    Entity parentEntity,
                                    PlexusObjectData plexusObjectData) {
            Entity plexusEdgeEntity = ecb.Instantiate(plexusEdgePrefabEntity);
            ecb.AddSharedComponent(plexusEdgeEntity, new PlexusObjectIdData { ObjectId = plexusObjectId });
            ecb.SetComponent(plexusEdgeEntity, new EdgeData { Vertex1Index = pointId1, Vertex2Index = pointId2, Length = edgeLength });
            ecb.SetComponent(plexusEdgeEntity, new EdgeColorData { Value = plexusObjectData.EdgeColor });
            ecb.AddComponent(plexusEdgeEntity, new Parent { Value = parentEntity });
        }
    }

    struct Connection {
        public int Id1;
        public int Id2;
    }
}
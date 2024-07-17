using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace WireframePlexus {

    public partial class SpawnSystem : SystemBase {

        int plexusObjectId = 0;
        Unity.Mathematics.Random random = new Unity.Mathematics.Random(1);

        protected override void OnCreate() {
            RequireForUpdate<VertexSpawnData>();
            RequireForUpdate<PlexusObjectSpawnData>();
            RequireForUpdate<EdgeSpawnData>();
        }

        public void SpawnPlexusObject( ref int PlexusObjectToSpawnId, 
                                                    PlexusObjectData plexusObjectData, 
                                                    PlexusGameObjectBase plexusGameObject, 
                                                    PlexusObjectPrecalculatedMeshData plexusObjectPrecalculatedMeshData) {
            // get spawn data singletons
            if (!SystemAPI.TryGetSingleton<VertexSpawnData>(out VertexSpawnData wireframePlexusVertexSpawnData)) {
                World.EntityManager.CreateSingleton<VertexSpawnData>();
            }
            wireframePlexusVertexSpawnData = SystemAPI.GetSingleton<VertexSpawnData>();

            if (!SystemAPI.TryGetSingleton<PlexusObjectSpawnData>(out PlexusObjectSpawnData wireframePlexusObjectSpawnData)) {
                World.EntityManager.CreateSingleton<PlexusObjectSpawnData>();
            }
            wireframePlexusObjectSpawnData = SystemAPI.GetSingleton<PlexusObjectSpawnData>();

            if (!SystemAPI.TryGetSingleton<EdgeSpawnData>(out EdgeSpawnData wireframePlexusEdgeSpawnData)) {
                World.EntityManager.CreateSingleton<EdgeSpawnData>();
            }
            wireframePlexusEdgeSpawnData = SystemAPI.GetSingleton<EdgeSpawnData>();


            // fill ecb to cache all strucutral changes and execute them at once later on ecb play
            var ecb = new EntityCommandBuffer(Allocator.Temp);


            // create an parent entity 
            Entity wireframePlexusObjectEntity = ecb.Instantiate(wireframePlexusObjectSpawnData.WireframePlexusEntityPrefab);
            // set parent sync Gameobject 
            SyncEntityPositionToGameobjectPositionData parentReference = new SyncEntityPositionToGameobjectPositionData();
            parentReference.PlexusGameObject = plexusGameObject;

            ecb.AddComponent(wireframePlexusObjectEntity, parentReference);

            foreach ( var vertexData in plexusObjectPrecalculatedMeshData.precalculatedVertexMeshData) {

                float3 pos = vertexData.Position;
                int vertexId = vertexData.VertexId;

                Entity plexusVertexEntity = ecb.Instantiate(wireframePlexusVertexSpawnData.WireframePlexusVertexEntityPrefab);
                VertexMovementData movementData = new VertexMovementData();
                movementData.Position = pos;
                movementData.PositionTarget = pos;

                movementData.MoveSpeed = UnityEngine.Random.Range(plexusGameObject.MinVertexMoveSpeed, plexusGameObject.MaxVertexMoveSpeed);
                movementData.Random = new Unity.Mathematics.Random(random.NextUInt());
                movementData.PointId = vertexId;

                ecb.SetComponent(plexusVertexEntity, movementData);
                ecb.SetComponent(plexusVertexEntity, new LocalTransform { Position = pos, Scale = plexusGameObject.VertexSize });
                ecb.SetComponent(plexusVertexEntity, new VertexColorData { Value = plexusGameObject.VertexColor });
                ecb.AddSharedComponent(plexusVertexEntity, new PlexusObjectIdData { PlexusObjectId = plexusObjectId });
                ecb.AddComponent(plexusVertexEntity, new Parent { Value = wireframePlexusObjectEntity });
            }

            // create parent plexusObjectData because now the number of vertices is known
            plexusObjectData.ContactAnimationColorData = new NativeList<ContactEffectData>(Allocator.Persistent);
            plexusObjectData.VertexPositions = new NativeArray<float3>(plexusObjectPrecalculatedMeshData.precalculatedVertexMeshData.Length + 1, Allocator.Persistent);
            plexusObjectData.WireframePlexusObjectId = plexusObjectId;
            plexusObjectData.WorldPosition = plexusGameObject.transform.position;
            plexusObjectData.WorldRotation = plexusGameObject.transform.rotation;
            ecb.AddComponent(wireframePlexusObjectEntity, plexusObjectData);

            foreach (var edgeData in plexusObjectPrecalculatedMeshData.precalculatedEdgeMeshData) {
                AddPlexusEdge(ref ecb, wireframePlexusEdgeSpawnData.WireframePlexusEdgeEntityPrefab, edgeData.Vertex1Id, edgeData.Vertex2Id, edgeData.Distance, wireframePlexusObjectEntity, plexusObjectData);
            }
            PlexusObjectToSpawnId = plexusObjectId;
            plexusObjectId++;
            ecb.Playback(EntityManager);
        }

        public void SpawnPlexusObject(ref int PlexusObjectToSpawnId, PlexusObjectData plexusObjectData, PlexusGameObjectBase plexusGameObject, Mesh mesh) {
            // get spawn data singletons
            if (!SystemAPI.TryGetSingleton<VertexSpawnData>(out VertexSpawnData wireframePlexusVertexSpawnData)) {
                World.EntityManager.CreateSingleton<VertexSpawnData>();
            }
            wireframePlexusVertexSpawnData = SystemAPI.GetSingleton<VertexSpawnData>();

            if (!SystemAPI.TryGetSingleton<PlexusObjectSpawnData>(out PlexusObjectSpawnData wireframePlexusObjectSpawnData)) {
                World.EntityManager.CreateSingleton<PlexusObjectSpawnData>();
            }
            wireframePlexusObjectSpawnData = SystemAPI.GetSingleton<PlexusObjectSpawnData>();

            if (!SystemAPI.TryGetSingleton<EdgeSpawnData>(out EdgeSpawnData wireframePlexusEdgeSpawnData)) {
                World.EntityManager.CreateSingleton<EdgeSpawnData>();
            }
            wireframePlexusEdgeSpawnData = SystemAPI.GetSingleton<EdgeSpawnData>();


            // fill ecb to cache all strucutral changes and execute them at once later on ecb play
            var ecb = new EntityCommandBuffer(Allocator.Temp);


            // create an parent entity 
            Entity wireframePlexusObjectEntity = ecb.Instantiate(wireframePlexusObjectSpawnData.WireframePlexusEntityPrefab);
            // set parent sync Gameobject 
            SyncEntityPositionToGameobjectPositionData parentReference = new SyncEntityPositionToGameobjectPositionData();
            parentReference.PlexusGameObject = plexusGameObject;

            ecb.AddComponent(wireframePlexusObjectEntity, parentReference);

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
                ecb.SetComponent(plexusVertexEntity, new VertexColorData { Value = plexusGameObject.VertexColor });
                ecb.AddSharedComponent(plexusVertexEntity, new PlexusObjectIdData { PlexusObjectId = plexusObjectId });
                ecb.AddComponent(plexusVertexEntity, new Parent { Value = wireframePlexusObjectEntity });

                pointId++;
            }

            // create parent plexusObjectData because now the number of vertices is known
            ecb.AddComponent(wireframePlexusObjectEntity, new PlexusObjectData {
                ContactAnimationColorData = new NativeList<ContactEffectData>(Allocator.Persistent),
                EdgeColor = plexusObjectData.EdgeColor,
                VertexColor = plexusObjectData.VertexColor,
                VertexPositions = new NativeArray<float3>(pointId + 1, Allocator.Persistent),
                WireframePlexusObjectId = plexusObjectId,
                EdgeThickness = plexusObjectData.EdgeThickness,
                VertexSize = plexusObjectData.VertexSize,
                MaxEdgeLengthPercent = plexusObjectData.MaxEdgeLengthPercent,
                MaxVertexMoveSpeed = plexusObjectData.MaxVertexMoveSpeed,
                MinVertexMoveSpeed = plexusObjectData.MinVertexMoveSpeed,
                MaxVertexMoveDistance = plexusObjectData.MaxVertexMoveDistance,
                WorldPosition = plexusGameObject.transform.position,
                WorldRotation = plexusGameObject.transform.rotation

            });
            // create the edge entities without duplicates
            HashSet<Tuple<int, int>> edgeConnections = new HashSet<Tuple<int, int>>();
            for (int i = 0; i < mesh.triangles.Length - 2; i = i + 3) {
                int pos1Id = usedIdByPosition[(float3)mesh.vertices[mesh.triangles[i]]];
                int pos2Id = usedIdByPosition[(float3)mesh.vertices[mesh.triangles[i + 1]]];
                int pos3Id = usedIdByPosition[(float3)mesh.vertices[mesh.triangles[i + 2]]];

                if (edgeConnections.Contains(EdgePair(pos1Id, pos2Id)) == false) {
                    edgeConnections.Add(EdgePair(pos1Id, pos2Id));
                    AddPlexusEdge(ref ecb, wireframePlexusEdgeSpawnData.WireframePlexusEdgeEntityPrefab, pos1Id, pos2Id, math.distance(usedPositionById[pos1Id], usedPositionById[pos2Id]), wireframePlexusObjectEntity, plexusObjectData);
                }
                if (edgeConnections.Contains(EdgePair(pos2Id, pos3Id)) == false) {
                    edgeConnections.Add(EdgePair(pos2Id, pos3Id));
                    AddPlexusEdge(ref ecb, wireframePlexusEdgeSpawnData.WireframePlexusEdgeEntityPrefab, pos2Id, pos3Id, math.distance(usedPositionById[pos2Id], usedPositionById[pos3Id]), wireframePlexusObjectEntity, plexusObjectData);
                }
                if (edgeConnections.Contains(EdgePair(pos1Id, pos3Id)) == false) {
                    edgeConnections.Add(EdgePair(pos1Id, pos3Id));
                    AddPlexusEdge(ref ecb, wireframePlexusEdgeSpawnData.WireframePlexusEdgeEntityPrefab, pos1Id, pos3Id, math.distance(usedPositionById[pos3Id], usedPositionById[pos1Id]), wireframePlexusObjectEntity, plexusObjectData);
                }

            }

            PlexusObjectToSpawnId = plexusObjectId;
            plexusObjectId++;
            ecb.Playback(EntityManager);
        }
        public static Tuple<int, int> EdgePair(int id1, int id2) {
            // Ensure the pair is always ordered the same way
            return id1 < id2 ? Tuple.Create(id1, id2) : Tuple.Create(id2, id1);
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
            ecb.AddSharedComponent(plexusEdgeEntity, new PlexusObjectIdData { PlexusObjectId = plexusObjectId });
            ecb.SetComponent(plexusEdgeEntity, new EdgeData { Vertex1Index = pointId1, Vertex2Index = pointId2, Length = edgeLength });
            ecb.SetComponent(plexusEdgeEntity, new EdgeColorData { Value = plexusObjectData.EdgeColor });
            ecb.AddComponent(plexusEdgeEntity, new Parent { Value = parentEntity });
        }


    }


}
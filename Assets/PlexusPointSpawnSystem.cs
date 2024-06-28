using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Collections.Generic;


public partial class PlexusSpawnSystem : SystemBase {


    int plexusObjectId = 0;
    Random random = new Random(1);

    protected override void OnUpdate() {

        WireframePlexusVertexEntitySpawnData plexusPointSpawnData = SystemAPI.GetSingleton<WireframePlexusVertexEntitySpawnData>();
        SyncEntityToGameobjectPositionSpawnData syncEntityToGameobjectPositionSpawnData = SystemAPI.GetSingleton<SyncEntityToGameobjectPositionSpawnData>();
        PlexusLineSpawnData plexusLineSpawnData = SystemAPI.GetSingleton<PlexusLineSpawnData>();

        foreach (WireframePlexusBuildData plexusBuildData in WireframePlexusEffectSpawnQueue.Instance.PlexusBuildDataQueue) {

            Entity plexusObjectEntity = EntityManager.Instantiate(syncEntityToGameobjectPositionSpawnData.SyncEntityToGameobjectPositionPrefabEntity);
            SyncEntityToGameobjectPositionData parentReference = new SyncEntityToGameobjectPositionData();
            parentReference.gameObject = plexusBuildData.PlexusParent;
            EntityManager.SetComponentData(plexusObjectEntity, parentReference);
            EntityManager.AddSharedComponent(plexusObjectEntity, new PlexusObjectIdData { PlexusObjectId = plexusObjectId });
            int points = plexusBuildData.Mesh.triangles.Length;
            EntityManager.AddComponentData(plexusObjectEntity, new WireframePlexusEntityData { VertexPositions = new NativeArray<float3>(points, Allocator.Persistent),
                                                                                                MaxVertexMoveSpeed = plexusBuildData.MaxVertexMoveSpeed, 
                                                                                                MinVertexMoveSpeed = plexusBuildData.MinVertexMoveSpeed, 
                                                                                                MaxVertexMoveDistance = plexusBuildData.MaxEdgeLengthPercent });

            Dictionary<int,float3> usedPositionById = new Dictionary<int, float3>();
            Dictionary<float3, int> usedIdByPosition = new Dictionary<float3, int>();
            int pointId = 0;
            for (int i = 0; i < plexusBuildData.Mesh.vertices.Length; i++) {
                if (usedPositionById.ContainsValue(plexusBuildData.Mesh.vertices[i])) {
                    continue;
                }
                float3 pos = plexusBuildData.Mesh.vertices[i];
                usedPositionById.Add(i,pos);
                usedIdByPosition.Add(pos,i);

                Entity plexusPointEntity = EntityManager.Instantiate(plexusPointSpawnData.PlexusPointPrefabEntity);
                PlexusPointMovementData movementData = SystemAPI.GetComponent<PlexusPointMovementData>(plexusPointEntity);
                movementData.Position = pos;
                movementData.PositionTarget = pos;
                
                movementData.MoveSpeed = UnityEngine.Random.Range(plexusBuildData.MinVertexMoveSpeed, plexusBuildData.MaxVertexMoveSpeed);
                movementData.Random = new Random(random.NextUInt());
                movementData.PointId = pointId;
                pointId++;


                SystemAPI.SetComponent(plexusPointEntity, movementData);
                SystemAPI.SetComponent(plexusPointEntity, new PlexusPointIdData { Id = i });
                EntityManager.AddSharedComponent(plexusPointEntity, new PlexusObjectIdData { PlexusObjectId = plexusObjectId });
                EntityManager.AddComponentData(plexusPointEntity, new Parent { Value = plexusObjectEntity });

            }

            List<Connection> connections = new List<Connection>();
            for (int i = 0; i < plexusBuildData.Mesh.triangles.Length - 2; i = i + 3) {
                int pos1Id = plexusBuildData.Mesh.triangles[i];
                int pos2Id = plexusBuildData.Mesh.triangles[i+1];
                int pos3Id = plexusBuildData.Mesh.triangles[i+2];
                if(usedPositionById.ContainsKey(pos1Id)==false) { 
                    pos1Id = usedIdByPosition[plexusBuildData.Mesh.vertices[pos1Id]];
                }
                if (usedPositionById.ContainsKey(pos2Id) == false) {
                    pos2Id = usedIdByPosition[plexusBuildData.Mesh.vertices[pos2Id]];
                }
                if (usedPositionById.ContainsKey(pos3Id) == false) {
                    pos3Id = usedIdByPosition[plexusBuildData.Mesh.vertices[pos3Id]];
                }

                if(connections.Contains(new Connection{ Id1 = pos1Id, Id2 = pos2Id }) == false && connections.Contains(new Connection { Id1 = pos2Id, Id2 = pos1Id }) == false){
                    connections.Add(new Connection { Id1 = pos1Id, Id2 = pos2Id });
                    AddPlexusLine(plexusLineSpawnData.PlexusLinePrefabEntity, pos1Id, pos2Id, plexusBuildData.MaxEdgeLengthPercent, math.distance(plexusBuildData.Mesh.vertices[pos1Id], plexusBuildData.Mesh.vertices[pos2Id]) , plexusObjectEntity);
                }
                if (connections.Contains(new Connection { Id1 = pos2Id, Id2 = pos3Id }) == false && connections.Contains(new Connection { Id1 = pos3Id, Id2 = pos2Id }) == false) {
                    connections.Add(new Connection { Id1 = pos2Id, Id2 = pos3Id });
                    AddPlexusLine(plexusLineSpawnData.PlexusLinePrefabEntity, pos2Id, pos3Id, plexusBuildData.MaxEdgeLengthPercent, math.distance(plexusBuildData.Mesh.vertices[pos2Id], plexusBuildData.Mesh.vertices[pos3Id]), plexusObjectEntity);
                }
                if (connections.Contains(new Connection { Id1 = pos1Id, Id2 = pos3Id }) == false && connections.Contains(new Connection { Id1 = pos3Id, Id2 = pos1Id }) == false) {
                    connections.Add(new Connection { Id1 = pos1Id, Id2 = pos3Id });
                    AddPlexusLine(plexusLineSpawnData.PlexusLinePrefabEntity, pos1Id, pos3Id, plexusBuildData.MaxEdgeLengthPercent, math.distance(plexusBuildData.Mesh.vertices[pos3Id], plexusBuildData.Mesh.vertices[pos1Id]), plexusObjectEntity);
                }
            }
            plexusObjectId++;
        }

        WireframePlexusEffectSpawnQueue.Instance.PlexusBuildDataQueue.Clear();
        Enabled = false;
    }

    private void AddPlexusLine(Entity plexusLinePrefabEntity, int pointId1, int pointId2, float maxPlexusLineLengthPercent, float plexusLineLength, Entity parentEntity) {
        Entity plexusLineEntity = EntityManager.Instantiate(plexusLinePrefabEntity);
        EntityManager.AddSharedComponent(plexusLineEntity, new PlexusObjectIdData { PlexusObjectId = plexusObjectId });
        EntityManager.SetComponentData(plexusLineEntity, new PlexusLineData { Position1Id = pointId1, Position2Id = pointId2, MaxLengthPercent = maxPlexusLineLengthPercent, Length = plexusLineLength });
        EntityManager.AddComponentData(plexusLineEntity, new Parent { Value = parentEntity });
    }

}
struct Connection {
    public int Id1;
    public int Id2;
}
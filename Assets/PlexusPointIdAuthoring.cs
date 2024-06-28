using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlexusPointIdAuthoring : MonoBehaviour
{
    private class Baker : Baker<PlexusPointIdAuthoring> {
        public override void Bake(PlexusPointIdAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new PlexusPointIdData { });
        }
    }
}

public struct PlexusPointIdData : IComponentData {
    public int Id;
}

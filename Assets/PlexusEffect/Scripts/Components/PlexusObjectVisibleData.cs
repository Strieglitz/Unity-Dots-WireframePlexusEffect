using Unity.Entities;
using UnityEngine;

namespace WireframePlexus {

    public struct PlexusObjectVisibleData : ISharedComponentData {
        public bool Visible;
    }
}
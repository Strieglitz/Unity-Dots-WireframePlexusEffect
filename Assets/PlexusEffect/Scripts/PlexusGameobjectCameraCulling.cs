using UnityEngine;

namespace WireframePlexus {

    [RequireComponent(typeof(PlexusGameObjectBase))]
    public class PlexusGameobjectCameraCulling : MonoBehaviour {
 
        public Camera Camera;
        private PlexusGameObjectBase plexusGameObjectBase;


        private void Awake() {
            plexusGameObjectBase = GetComponent<PlexusGameObjectBase>();
        }

        private void Update() {
            var planes = GeometryUtility.CalculateFrustumPlanes(Camera);
            if(GeometryUtility.TestPlanesAABB(planes, plexusGameObjectBase.Bounds)) {
                plexusGameObjectBase.SetPlexusObjectEnabled(true);
            } else {
                plexusGameObjectBase.SetPlexusObjectEnabled(false);
            }
        }
    }
}
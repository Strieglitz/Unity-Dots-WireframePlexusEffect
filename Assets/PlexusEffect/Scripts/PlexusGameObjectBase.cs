using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace WireframePlexus {

    public abstract class PlexusGameObjectBase : MonoBehaviour {

        [SerializeField]
        [Tooltip("Generate the Plexus ECS Object on Start?")]
        bool genrateOnStart = true;

        [SerializeField]
        [Tooltip("If this gameobject has a MeshRenderer should it get disableb when the plexusObject is created")]
        bool disableMeshRenderer = true;

        [SerializeField]
        [Tooltip("If this is checked, when this gameobject gets destoryed the plexusObject also gets destroyed, recommended otherwise it will live as ling as the subscene or until it gets destriyed by hand")]
        bool destroyPlexusObjectWhenGameobjectGetsDestroyed = true;

        public PlexusGameObjectData PlexusGameObjectData;
        Bounds defaultBounds;
        Bounds bounds;
        public Bounds Bounds => bounds;

        protected int wireframePlexusObjectId;

        protected void Start() {
            defaultBounds = GetMeshBounds();
            if (genrateOnStart) {
                GenerateECSPlexusObject();
            }
        }

        private void OnDestroy() {
            if (destroyPlexusObjectWhenGameobjectGetsDestroyed) {
                try {
                    World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PlexusObjectDeleteSystem>().DeletePlexusObject(wireframePlexusObjectId);
                } catch {
                    Debug.LogWarning("PlexusObjectDeleteSystem not found when destroying the PlexusObject, this is normal if the scene gets destroyed");
                }

            }
        }


        public void UpdatePlexusObjectData() {
            World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PlexusObjectRecieveUpdatedDataSystem>().UpdatePlexusObjectData(PlexusGameObjectData, wireframePlexusObjectId);
            CalcBounds();
        }


        private void CalcBounds() {
            bounds = defaultBounds;
            bounds.center = transform.position;
            bounds.extents = defaultBounds.extents * transform.localScale.x;
            bounds.Expand(PlexusGameObjectData.MaxVertexMoveDistance*2);
        }

        public void GenerateECSPlexusObject() {
            SpawnSystem spawnSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SpawnSystem>();
            wireframePlexusObjectId = GenerateECSPlexusObject(spawnSystem, PlexusGameObjectData);
            if (disableMeshRenderer) {
                if (GetComponent<MeshRenderer>()) {
                    GetComponent<MeshRenderer>().enabled = false;
                }
            }
            CalcBounds();
        }
        protected abstract int GenerateECSPlexusObject(SpawnSystem spawnSystem, PlexusGameObjectData plexusGameObjectData);
        protected abstract Bounds GetMeshBounds();


        /*** Set a contact animation on the plexus object
         *         * @param contactColor the color of the contact animation
         *                 * @param contactRadius the radius of the contact animation
         *                         * @param contactDuration the duration of the contact animation
         *                                 * @param contactWorldPosition the world position of the contact animation
         *                                         * @param contactVertexAnimationDurationMultiplier the multiplier for the duration of the vertex animation, has to be >= 1 (greater or equals 1) or 0 to not use a vertex animation
         *                                         * @param contactVertexMaxDistance the max distance a vertex will move at the contact animation
         *                                                 */
        public void SetPlexusContactAnimation(Color contactColor, float contactRadius, float contactDuration, Vector3 contactWorldPosition, float contactVertexAnimationDurationMultiplier, float contactVertexMaxDistance, EaseType easeType) {
            ContactEffectData contactColorAnimationData = new ContactEffectData {
                ContactColor = new float4(contactColor.r, contactColor.g, contactColor.b, contactColor.a),
                ContactRadius = contactRadius,
                TotalContactDuration = contactDuration,
                CurrentContactDuration = contactDuration,
                ContactWorldPosition = contactWorldPosition,
                ContactVertexDurationMultiplier = contactVertexAnimationDurationMultiplier,
                ContactVertexMaxDistance = contactVertexMaxDistance,
                EasingType = easeType
            };
            World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<ContactEffectSpawnSystem>().SpawnContactEffect(contactColorAnimationData, wireframePlexusObjectId);
        }

        public void SetPlexusObjectEnabled(bool enabled) {
            World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PlexusObjectEnabledDisabledSystem>().SetEntityEnabled(wireframePlexusObjectId, enabled);
        }
    }
}

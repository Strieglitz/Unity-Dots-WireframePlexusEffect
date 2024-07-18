using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace WireframePlexus {

    public abstract class PlexusGameObjectBase : MonoBehaviour {

        [SerializeField]
        [Tooltip("Generate the Plexus ECS Object on Start?")]
        bool genrateOnStart = true;

        [SerializeField]
        [Tooltip("If this gameobject has a Meshrenderer should it get disableb when the plexusObject is created")]
        bool disableMeshRenderer = true;

        [Tooltip("Only draw the wireframes edge when its length is smaller than x Percent of the original length in the mesh")]
        public float MaxEdgeLengthPercent;

        [Tooltip("how thick the edges gonna be")]
        public float EdgeThickness;

        [Tooltip("The size of the visible vertex particle")]
        public float VertexSize;

        [Tooltip("The vertices are always in motion, relative to their original position in the mesh, this value sets how far from the original possition they can go")]
        public float MaxVertexMoveDistance;

        [Tooltip("The Minimum Speed a Vertex will have to move randomly around its original position in the mesh")]
        public float MinVertexMoveSpeed;

        [Tooltip("The Maximum Speed a Vertex will have to move randomly around its original position in the mesh")]
        public float MaxVertexMoveSpeed;


        [ColorUsage(true, true)]
        public Color VertexColor;
        public float4 VertexColorFloat4 => new float4(VertexColor.r, VertexColor.g, VertexColor.b, VertexColor.a);


        [ColorUsage(true, true)]
        public Color EdgeColor;
        public float4 EdgeColorFloat4 => new float4(EdgeColor.r, EdgeColor.g, EdgeColor.b, EdgeColor.a);

        protected int wireframePlexusObjectId;

        private PlexusObjectData getPlexusObjectData => new PlexusObjectData {
            WireframePlexusObjectId = wireframePlexusObjectId,
            MaxEdgeLengthPercent = MaxEdgeLengthPercent,
            EdgeThickness = EdgeThickness,
            MaxVertexMoveDistance = MaxVertexMoveDistance,
            MinVertexMoveSpeed = MinVertexMoveSpeed,
            MaxVertexMoveSpeed = MaxVertexMoveSpeed,
            VertexColor = VertexColorFloat4,
            EdgeColor = EdgeColorFloat4,
            VertexSize = VertexSize,
            WorldPosition = transform.position,
            WorldRotation = transform.rotation,
        };

        protected void Start() {
            if (genrateOnStart) {
                GenerateECSPlexusObject();
            }
        }


        public void UpdatePlexusObjectData() {
            PlexusObjectData plexusObjectData = getPlexusObjectData;
            World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PlexusObjectUpdateRecieveSystem>().UpdatePlexusObjectData(plexusObjectData);
        }

        public void GenerateECSPlexusObject() {
            SpawnSystem spawnSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SpawnSystem>();
            PlexusObjectData plexusObjectData = getPlexusObjectData;
            GenerateECSPlexusObject(spawnSystem, plexusObjectData);
            if (disableMeshRenderer) {
                if (GetComponent<MeshRenderer>()) {
                    GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }
        protected abstract void GenerateECSPlexusObject(SpawnSystem spawnSystem, PlexusObjectData plexusObjectData);

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
    }
}

using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace WireframePlexus {

    public abstract class PlexusGameObjectBase : MonoBehaviour {

        [SerializeField]
        [Tooltip("Generate the Plexus ECS Object on Start?")]
        bool genrateOnStart = true;

        [field: SerializeField]
        [Tooltip("Only draw the wireframes edge when its length is smaller than x Percent of the original length in the mesh")]
        public float MaxEdgeLengthPercent { get; private set; }

        [field: SerializeField]
        [Tooltip("how thick the edges gonna be")]
        public float EdgeThickness { get; private set; }

        [field: SerializeField]
        [Tooltip("The size of the visible vertex particle")]
        public float VertexSize { get; private set; }

        [field: SerializeField]
        [Tooltip("The vertices are always in motion, relative to their original position in the mesh, this value sets how far from the original possition they can go")]
        public float MaxVertexMoveDistance { get; private set; }

        [field: SerializeField]
        [Tooltip("The Minimum Speed a Vertex will have to move randomly around its original position in the mesh")]
        public float MinVertexMoveSpeed { get; private set; }

        [field: SerializeField]
        [Tooltip("The Maximum Speed a Vertex will have to move randomly around its original position in the mesh")]
        public float MaxVertexMoveSpeed { get; private set; }

        [field: SerializeField]
        [ColorUsage(true, true)]
        Color vertexColor;
        public float4 VertexColor => new float4(vertexColor.r, vertexColor.g, vertexColor.b, vertexColor.a);

        [field: SerializeField]
        [ColorUsage(true, true)]
        Color edgeColor;
        public float4 EdgeColor => new float4(edgeColor.r, edgeColor.g, edgeColor.b, edgeColor.a);

        protected int wireframePlexusObjectId;

        protected void Start() {
            if (genrateOnStart) {
                GenerateECSPlexusObject();
            }
        }

        private void GenerateECSPlexusObject() {
            SpawnSystem spawnSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SpawnSystem>();
            PlexusObjectData plexusObjectData = new PlexusObjectData {
                MaxEdgeLengthPercent = MaxEdgeLengthPercent,
                EdgeThickness = EdgeThickness,
                MaxVertexMoveDistance = MaxVertexMoveDistance,
                MinVertexMoveSpeed = MinVertexMoveSpeed,
                MaxVertexMoveSpeed = MaxVertexMoveSpeed,
                VertexColor = VertexColor,
                EdgeColor = EdgeColor,
                VertexSize = VertexSize,
            };
            GenerateECSPlexusObject(spawnSystem, plexusObjectData);
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

using UnityEngine;

namespace WireframePlexusShader {

    /// <summary>Example driver: fires a contact animation at a random vertex of the target every few seconds.</summary>
    public class PlexusShaderContactDemo : MonoBehaviour {

        public PlexusShaderObject Target;
        [Min(0.05f)] public float Interval = 2f;
        [ColorUsage(true, true)]
        public Color ContactColor = new Color(4f, 0.9f, 0.2f, 1f);
        [Tooltip("World space radius.")]
        public float ContactRadius = 0.6f;
        public float ContactDuration = 1.5f;
        public float VertexDurationMultiplier = 1f;
        [Tooltip("Object units, like MaxVertexMoveDistance.")]
        public float VertexMaxDistance = 0.1f;
        public PlexusEase Ease = PlexusEase.EaseOutCubic;

        Vector3[] surfacePoints;
        float timer = -1f;

        void Reset() {
            Target = GetComponent<PlexusShaderObject>();
        }

        void Update() {
            if (Target == null) Target = GetComponent<PlexusShaderObject>();
            if (Target == null || !Target.IsGenerated) return;
            if (timer < 0f) timer = Interval * 0.5f;

            timer -= Time.deltaTime;
            if (timer > 0f) return;
            timer = Interval;

            // Fetched lazily, so the demo also survives a script reload during play mode.
            if (surfacePoints == null) surfacePoints = ReadSurfacePoints();
            Vector3 local = surfacePoints.Length > 0
                ? surfacePoints[Random.Range(0, surfacePoints.Length)]
                : Random.onUnitSphere * 0.5f;
            // The seven parameter call aims the impact at the middle of the object. With a real collision,
            // pass the negated contact normal or the velocity of the projectile as an eighth parameter.
            Target.SetPlexusContactAnimation(ContactColor, ContactRadius, ContactDuration, Target.transform.TransformPoint(local), VertexDurationMultiplier, VertexMaxDistance, Ease);
        }

        Vector3[] ReadSurfacePoints() {
            Mesh mesh = Target.SourceMesh;
            if (mesh == null && Target.TryGetComponent(out MeshFilter filter)) mesh = filter.sharedMesh;
            return mesh != null && mesh.isReadable ? mesh.vertices : new Vector3[0];
        }
    }
}

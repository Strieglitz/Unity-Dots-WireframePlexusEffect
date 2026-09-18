using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WireframePlexusShader {

    /// <summary>
    /// Shader based counterpart of the ECS PlexusGameObjectFromMesh. It builds one static "plexus mesh" from the
    /// source mesh and lets the vertex shader animate it. After that there is no per frame work on the CPU,
    /// apart from a few uniforms while a contact animation is playing.
    /// The public methods mirror the ECS component, so calling code can switch between the two.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Wireframe Plexus/Plexus Shader Object")]
    public class PlexusShaderObject : MonoBehaviour {

        public const string ShaderName = "WireframePlexus/Plexus Shader (URP)";
        public const int MaxContacts = 8;
        const string RendererObjectName = "Plexus Shader Renderer";

        [Tooltip("Material that uses the shader " + ShaderName + ". It can be shared by all plexus objects, per object values travel in a MaterialPropertyBlock.")]
        public Material PlexusMaterial;
        [Tooltip("Mesh to build the effect from. Leave empty to use the MeshFilter on this GameObject.")]
        public Mesh SourceMesh;
        [Tooltip("Optional mesh baked ahead of time with the context menu entry Bake Plexus Mesh Asset. Skips the build on Start, which matters for big meshes.")]
        public Mesh BakedPlexusMesh;
        [Tooltip("Generate the plexus renderer on Start?")]
        public bool GenerateOnStart = true;
        [Tooltip("Disable the MeshRenderer of this GameObject once the plexus renderer exists.")]
        public bool DisableMeshRenderer = true;
        [Tooltip("Objects with different seeds move differently even when they share a mesh. 0 picks a random seed.")]
        public int Seed;

        public PlexusShaderSettings Settings = new PlexusShaderSettings();

        public bool IsGenerated => plexusRenderer != null;
        /// <summary>World space bounds of the effect, already widened by the vertex movement.</summary>
        public Bounds Bounds => plexusRenderer != null ? plexusRenderer.bounds : new Bounds(transform.position, Vector3.zero);
        public int PlexusVertexCount { get; private set; }
        public int PlexusEdgeCount { get; private set; }

        struct Contact {
            public Vector3 WorldPosition;
            public float Radius;
            public Vector4 Color;
            public float TotalDuration;
            public float Remaining;
            public float VertexDurationMultiplier;
            public float VertexMaxDistance;
            public PlexusEase Ease;
            public Vector3 ImpactDirection; // world space, zero means: towards the middle of the object
        }

        sealed class CachedMesh {
            public Mesh Mesh;
            public int VertexCount;
            public int EdgeCount;
            public int References;
        }

        static class Ids {
            public static readonly int VertexColor = Shader.PropertyToID("_VertexColor");
            public static readonly int EdgeColor = Shader.PropertyToID("_EdgeColor");
            public static readonly int VertexSize = Shader.PropertyToID("_VertexSize");
            public static readonly int EdgeThickness = Shader.PropertyToID("_EdgeThickness");
            public static readonly int DotCore = Shader.PropertyToID("_DotCore");
            public static readonly int DotSharpness = Shader.PropertyToID("_DotSharpness");
            public static readonly int MinEdgePixelWidth = Shader.PropertyToID("_MinEdgePixelWidth");
            public static readonly int MaxVertexMoveDistance = Shader.PropertyToID("_MaxVertexMoveDistance");
            public static readonly int MinVertexMoveSpeed = Shader.PropertyToID("_MinVertexMoveSpeed");
            public static readonly int MaxVertexMoveSpeed = Shader.PropertyToID("_MaxVertexMoveSpeed");
            public static readonly int MaxEdgeLengthPercent = Shader.PropertyToID("_MaxEdgeLengthPercent");
            public static readonly int EdgeFadeDuration = Shader.PropertyToID("_EdgeFadeDuration");
            public static readonly int EdgeFadeTaps = Shader.PropertyToID("_EdgeFadeTaps");
            public static readonly int EdgeFadeRange = Shader.PropertyToID("_EdgeFadeRange");
            public static readonly int Seed = Shader.PropertyToID("_PlexusSeed");
            public static readonly int SpawnTime = Shader.PropertyToID("_PlexusSpawnTime");
            public static readonly int ContactPosRadius = Shader.PropertyToID("_PlexusContactPosRadius");
            public static readonly int ContactColor = Shader.PropertyToID("_PlexusContactColor");
            public static readonly int ContactParams = Shader.PropertyToID("_PlexusContactParams");
            public static readonly int ContactOrigin = Shader.PropertyToID("_PlexusContactOrigin");
            public static readonly int ContactCount = Shader.PropertyToID("_PlexusContactCount");
        }

        // One plexus mesh per source mesh, shared by every object that uses it.
        static readonly Dictionary<Mesh, CachedMesh> meshCache = new Dictionary<Mesh, CachedMesh>();
        static Material fallbackMaterial;

        readonly List<Contact> contacts = new List<Contact>();
        readonly Vector4[] contactPosRadius = new Vector4[MaxContacts];
        readonly Vector4[] contactColors = new Vector4[MaxContacts];
        readonly Vector4[] contactParams = new Vector4[MaxContacts];
        readonly Vector4[] contactOrigins = new Vector4[MaxContacts];

        MeshRenderer plexusRenderer;
        MeshRenderer sourceRendererWeDisabled;
        MaterialPropertyBlock block;
        Bounds sourceBounds;
        Mesh cachedMeshKey;
        bool holdsCachedMesh;
        float shaderSeed;
        float spawnTime;
        float contactPushMargin;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() {
            meshCache.Clear();
            fallbackMaterial = null;
        }

        void Start() {
            if (GenerateOnStart) GeneratePlexus();
        }

        void OnEnable() {
            if (plexusRenderer != null) {
                plexusRenderer.enabled = true;
                return;
            }
            // A script reload during play mode wipes the private fields while the generated child survives.
            // Drop the orphan and build a fresh one, so the effect and its contacts keep working.
            Transform orphan = transform.Find(RendererObjectName);
            if (orphan == null) return;
            orphan.gameObject.SetActive(false);
            Destroy(orphan.gameObject);
            GeneratePlexus();
        }

        void OnDisable() {
            if (plexusRenderer != null) plexusRenderer.enabled = false;
        }

        void OnDestroy() {
            if (plexusRenderer != null) Destroy(plexusRenderer.gameObject);
            if (sourceRendererWeDisabled != null) sourceRendererWeDisabled.enabled = true;
            ReleaseCachedMesh();
        }

        void OnValidate() {
            // Lets you tune the values in the inspector while the game runs.
            if (Application.isPlaying && IsGenerated) UpdatePlexusObjectData();
        }

        void Update() {
            if (contacts.Count == 0) return;
            float deltaTime = Time.deltaTime;
            for (int i = contacts.Count - 1; i >= 0; i--) {
                Contact contact = contacts[i];
                contact.Remaining -= deltaTime;
                if (contact.Remaining <= 0f) contacts.RemoveAt(i);
                else contacts[i] = contact;
            }
            UploadContacts();
        }

        /// <summary>Builds the plexus renderer. Called from Start unless GenerateOnStart is off.</summary>
        public void GeneratePlexus() {
            if (IsGenerated) return;

            Mesh plexusMesh = BakedPlexusMesh;
            if (plexusMesh != null) {
                sourceBounds = plexusMesh.bounds;
            } else {
                Mesh source = ResolveSourceMesh();
                if (source == null) {
                    Debug.LogError("PlexusShaderObject needs a SourceMesh, a BakedPlexusMesh, or a MeshFilter with a mesh.", this);
                    return;
                }
                plexusMesh = AcquireCachedMesh(source);
                if (plexusMesh == null) return;
                sourceBounds = source.bounds;
            }

            Material material = ResolveMaterial();
            if (material == null) return;

            var child = new GameObject(RendererObjectName) { hideFlags = HideFlags.DontSave, layer = gameObject.layer };
            child.transform.SetParent(transform, false);
            child.AddComponent<MeshFilter>().sharedMesh = plexusMesh;
            plexusRenderer = child.AddComponent<MeshRenderer>();
            plexusRenderer.sharedMaterial = material;
            plexusRenderer.shadowCastingMode = ShadowCastingMode.Off;
            plexusRenderer.receiveShadows = false;
            plexusRenderer.lightProbeUsage = LightProbeUsage.Off;
            plexusRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            plexusRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            plexusRenderer.enabled = isActiveAndEnabled;

            if (DisableMeshRenderer && TryGetComponent(out MeshRenderer ownRenderer) && ownRenderer.enabled) {
                ownRenderer.enabled = false;
                sourceRendererWeDisabled = ownRenderer;
            }

            shaderSeed = Seed != 0 ? Mathf.Abs(Seed) % (1 << 20) : Random.Range(1, 1 << 20);
            // _Time.y in shaders counts from the last level load, the same clock as this.
            spawnTime = Time.timeSinceLevelLoad;
            block = new MaterialPropertyBlock();
            UpdatePlexusObjectData();
        }

        /// <summary>Sends the current Settings to the GPU. Call it after changing Settings from code.</summary>
        public void UpdatePlexusObjectData() {
            if (!IsGenerated) return;
            PlexusShaderSettings s = Settings;
            // SetVector instead of SetColor: the ECS version hands the raw HDR values to its shader, without color space conversion.
            block.SetVector(Ids.VertexColor, s.VertexColor);
            block.SetVector(Ids.EdgeColor, s.EdgeColor);
            block.SetFloat(Ids.VertexSize, s.VertexSize);
            block.SetFloat(Ids.EdgeThickness, s.EdgeThickness);
            block.SetFloat(Ids.DotCore, s.DotCore);
            block.SetFloat(Ids.DotSharpness, s.DotSharpness);
            block.SetFloat(Ids.MinEdgePixelWidth, s.MinEdgePixelWidth);
            block.SetFloat(Ids.MaxVertexMoveDistance, s.MaxVertexMoveDistance);
            block.SetFloat(Ids.MinVertexMoveSpeed, s.MinVertexMoveSpeed);
            block.SetFloat(Ids.MaxVertexMoveSpeed, s.MaxVertexMoveSpeed);
            block.SetFloat(Ids.MaxEdgeLengthPercent, s.MaxEdgeLengthPercent);
            block.SetFloat(Ids.EdgeFadeDuration, s.EdgeFadeDuration);
            block.SetFloat(Ids.EdgeFadeTaps, s.EdgeFadeTaps);
            block.SetFloat(Ids.EdgeFadeRange, s.EdgeFadeRange);
            block.SetFloat(Ids.Seed, shaderSeed);
            block.SetFloat(Ids.SpawnTime, spawnTime);
            UploadContacts();
            UpdateBounds();
        }

        /// <summary>
        /// Plays a contact animation: vertices and edges within the radius blend towards the contact color and back,
        /// and vertices get pushed away from the contact point and return.
        /// </summary>
        /// <param name="contactColor">Color at the contact point.</param>
        /// <param name="contactRadius">World space radius of the effect.</param>
        /// <param name="contactDuration">Length of the animation in seconds.</param>
        /// <param name="contactWorldPosition">World space position of the contact.</param>
        /// <param name="contactVertexAnimationDurationMultiplier">1 or more makes the vertex push finish that much faster than the color. 0 turns the push off.</param>
        /// <param name="contactVertexMaxDistance">How far vertices at the contact point get pushed, in object units like MaxVertexMoveDistance.</param>
        /// <param name="easeType">Easing of the animation.</param>
        /// <param name="impactDirection">World space direction the impact travels in, for example the velocity of a projectile or the negated collision normal. The surface dents along it. Pass Vector3.zero to aim at the middle of the object.</param>
        public void SetPlexusContactAnimation(Color contactColor, float contactRadius, float contactDuration, Vector3 contactWorldPosition, float contactVertexAnimationDurationMultiplier, float contactVertexMaxDistance, PlexusEase easeType, Vector3 impactDirection) {
            if (!IsGenerated || contactDuration <= 0f || contactRadius <= 0f) return;
            contacts.Add(new Contact {
                WorldPosition = contactWorldPosition,
                Radius = contactRadius,
                Color = contactColor,
                TotalDuration = contactDuration,
                Remaining = contactDuration,
                VertexDurationMultiplier = Mathf.Max(0f, contactVertexAnimationDurationMultiplier),
                VertexMaxDistance = contactVertexMaxDistance,
                Ease = easeType,
                ImpactDirection = impactDirection.sqrMagnitude > 1e-12f ? impactDirection.normalized : Vector3.zero
            });
            UploadContacts();
        }

        /// <summary>Same call as in the ECS version. The impact is aimed at the middle of the object.</summary>
        public void SetPlexusContactAnimation(Color contactColor, float contactRadius, float contactDuration, Vector3 contactWorldPosition, float contactVertexAnimationDurationMultiplier, float contactVertexMaxDistance, PlexusEase easeType) {
            SetPlexusContactAnimation(contactColor, contactRadius, contactDuration, contactWorldPosition, contactVertexAnimationDurationMultiplier, contactVertexMaxDistance, easeType, Vector3.zero);
        }

        /// <summary>Shows or hides the effect without destroying it.</summary>
        public void SetPlexusObjectEnabled(bool enabled) {
            if (plexusRenderer != null) plexusRenderer.enabled = enabled;
        }

        void UploadContacts() {
            if (!IsGenerated) return;
            // The shader has room for MaxContacts. If more are running, the newest ones win.
            int count = Mathf.Min(contacts.Count, MaxContacts);
            int first = contacts.Count - count;
            float pushMargin = 0f;
            Vector3 objectMiddle = plexusRenderer.bounds.center;
            for (int i = 0; i < MaxContacts; i++) {
                if (i >= count) {
                    contactPosRadius[i] = Vector4.zero;
                    contactColors[i] = Vector4.zero;
                    contactParams[i] = Vector4.zero;
                    contactOrigins[i] = Vector4.zero;
                    continue;
                }
                Contact contact = contacts[first + i];
                float progress = 1f - Mathf.Clamp01(contact.Remaining / contact.TotalDuration);
                float easedColor = PlexusEasing.Evaluate(contact.Ease, progress);
                float easedPush = PlexusEasing.Evaluate(contact.Ease, Mathf.Min(1f, progress * contact.VertexDurationMultiplier));
                // Out during the first half, back during the second half.
                float pushShape = easedPush < 0.5f ? easedPush * 2f : 1f - (easedPush - 0.5f) * 2f;

                Vector3 p = contact.WorldPosition;
                contactPosRadius[i] = new Vector4(p.x, p.y, p.z, contact.Radius);
                contactColors[i] = contact.Color;
                contactParams[i] = new Vector4(1f - easedColor, pushShape * contact.VertexMaxDistance, 1f, 0f);

                // Vertices are pushed away from a point that sits off the surface, against the impact direction.
                // That presses the area around the contact inwards instead of tearing it apart.
                Vector3 impact = contact.ImpactDirection;
                if (impact == Vector3.zero) {
                    Vector3 toMiddle = objectMiddle - p;
                    if (toMiddle.sqrMagnitude > 1e-10f) impact = toMiddle.normalized;
                }
                Vector3 origin = p - impact * (Mathf.Max(0f, Settings.ContactImpactLift) * contact.Radius);
                contactOrigins[i] = new Vector4(origin.x, origin.y, origin.z, 0f);
                pushMargin = Mathf.Max(pushMargin, Mathf.Abs(contact.VertexMaxDistance));
            }
            block.SetVectorArray(Ids.ContactPosRadius, contactPosRadius);
            block.SetVectorArray(Ids.ContactColor, contactColors);
            block.SetVectorArray(Ids.ContactParams, contactParams);
            block.SetVectorArray(Ids.ContactOrigin, contactOrigins);
            block.SetFloat(Ids.ContactCount, count);
            plexusRenderer.SetPropertyBlock(block);

            if (!Mathf.Approximately(pushMargin, contactPushMargin)) {
                contactPushMargin = pushMargin;
                UpdateBounds();
            }
        }

        void UpdateBounds() {
            // The shader moves vertices outside the static mesh bounds, so culling needs a wider box.
            PlexusShaderSettings s = Settings;
            float margin = Mathf.Abs(s.MaxVertexMoveDistance) * 1.7320508f // corner of the move cube
                           + Mathf.Abs(s.VertexSize) * 0.5f
                           + Mathf.Abs(s.EdgeThickness)
                           + contactPushMargin;
            Bounds bounds = sourceBounds;
            bounds.Expand(margin * 2f);
            plexusRenderer.localBounds = bounds;
        }

        Mesh ResolveSourceMesh() {
            if (SourceMesh != null) return SourceMesh;
            return TryGetComponent(out MeshFilter filter) ? filter.sharedMesh : null;
        }

        Material ResolveMaterial() {
            if (PlexusMaterial != null) return PlexusMaterial;
            if (fallbackMaterial == null) {
                Shader shader = Shader.Find(ShaderName);
                if (shader == null) {
                    Debug.LogError("Shader " + ShaderName + " not found. Assign a PlexusMaterial.", this);
                    return null;
                }
                fallbackMaterial = new Material(shader) { name = "Plexus Shader (fallback)", hideFlags = HideFlags.DontSave };
                Debug.LogWarning("PlexusShaderObject has no PlexusMaterial assigned, using a generated one. Assign a material so the shader is included in builds.", this);
            }
            return fallbackMaterial;
        }

        Mesh AcquireCachedMesh(Mesh source) {
            Mesh key = source;
            if (!meshCache.TryGetValue(key, out CachedMesh cached) || cached.Mesh == null) {
                PlexusShaderMeshBuilder.Result result;
                try {
                    result = PlexusShaderMeshBuilder.Build(source);
                } catch (System.InvalidOperationException exception) {
                    Debug.LogError(exception.Message, this);
                    return null;
                }
                result.Mesh.hideFlags = HideFlags.DontSave;
                cached = new CachedMesh { Mesh = result.Mesh, VertexCount = result.VertexCount, EdgeCount = result.EdgeCount };
                meshCache[key] = cached;
            }
            cached.References++;
            cachedMeshKey = key;
            holdsCachedMesh = true;
            PlexusVertexCount = cached.VertexCount;
            PlexusEdgeCount = cached.EdgeCount;
            return cached.Mesh;
        }

        void ReleaseCachedMesh() {
            if (!holdsCachedMesh) return;
            holdsCachedMesh = false;
            if (!meshCache.TryGetValue(cachedMeshKey, out CachedMesh cached)) return;
            cached.References--;
            if (cached.References > 0) return;
            meshCache.Remove(cachedMeshKey);
            if (cached.Mesh != null) Destroy(cached.Mesh);
        }

#if UNITY_EDITOR
        [ContextMenu("Bake Plexus Mesh Asset")]
        void BakePlexusMeshAsset() {
            Mesh source = ResolveSourceMesh();
            if (source == null) {
                Debug.LogError("Nothing to bake: assign a SourceMesh or add a MeshFilter with a mesh.", this);
                return;
            }
            string path = UnityEditor.EditorUtility.SaveFilePanelInProject("Save plexus mesh", source.name + "_Plexus", "asset", "Where should the baked plexus mesh go?");
            if (string.IsNullOrEmpty(path)) return;
            PlexusShaderMeshBuilder.Result result = PlexusShaderMeshBuilder.Build(source);
            // Keep the asset readable. The vertex compression of Unity only touches non readable meshes, and half
            // precision texture coordinates would destroy the vertex ids stored in them.
            UnityEditor.AssetDatabase.CreateAsset(result.Mesh, path);
            UnityEditor.Undo.RecordObject(this, "Assign baked plexus mesh");
            BakedPlexusMesh = result.Mesh;
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log("Baked " + result.VertexCount + " vertices and " + result.EdgeCount + " edges to " + path + ".", this);
        }
#endif
    }
}

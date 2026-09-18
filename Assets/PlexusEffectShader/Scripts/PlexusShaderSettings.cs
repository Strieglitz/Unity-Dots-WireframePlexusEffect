using System;
using UnityEngine;

namespace WireframePlexusShader {

    /// <summary>
    /// Look and motion settings of one plexus object. The first eight fields mean the same as in
    /// PlexusGameObjectData of the ECS version, so values can be copied over one to one.
    /// </summary>
    [Serializable]
    public class PlexusShaderSettings {

        [Tooltip("Only draw an edge while its current length is below this multiple of its rest length in the mesh. 1.1 means 110 percent.")]
        public float MaxEdgeLengthPercent = 1.1f;
        [Tooltip("Edge thickness in object units.")]
        public float EdgeThickness = 0.002f;
        [Tooltip("Size of the vertex dot in object units.")]
        public float VertexSize = 0.04f;
        [Tooltip("How far a vertex may drift from its original position in the mesh, in object units.")]
        public float MaxVertexMoveDistance = 0.02f;
        [Tooltip("Slowest drift speed of a vertex, in object units per second.")]
        public float MinVertexMoveSpeed = 0.005f;
        [Tooltip("Fastest drift speed of a vertex, in object units per second.")]
        public float MaxVertexMoveSpeed = 0.01f;

        [ColorUsage(true, true)]
        public Color VertexColor = new Color(0f, 0.68213385f, 1.6055588f, 1f);
        [ColorUsage(true, true)]
        public Color EdgeColor = new Color(0f, 0.38041875f, 1.7207953f, 1f);

        [Header("Shader version only")]
        [Tooltip("Seconds an edge needs to fade in or out after crossing the length limit. 0 makes the fade follow the stretch directly.")]
        [Min(0f)] public float EdgeFadeDuration = 1f;
        [Tooltip("How many points in time the shader samples to build that fade. More is smoother and costs more vertex shader work.")]
        [Range(1, 8)] public int EdgeFadeTaps = 4;
        [Tooltip("Soft band around the length limit in which an edge fades, as a fraction of the limit.")]
        [Range(0.001f, 1f)] public float EdgeFadeRange = 0.05f;
        [Tooltip("Edges thinner than this many pixels are drawn this wide and dimmed instead. Keeps very thin lines from breaking up. 0 turns it off.")]
        [Min(0f)] public float MinEdgePixelWidth = 1f;
        [Tooltip("0 is a soft glow like the default particle of Unity, values near 1 give a hard disc.")]
        [Range(0f, 0.99f)] public float DotCore = 0f;
        [Tooltip("How tightly the glow of a dot sits around its centre. 8 matches the default particle texture of the ECS version.")]
        [Range(1f, 16f)] public float DotSharpness = 8f;
        [Tooltip("Shapes the vertex push of contact animations. 0 pushes vertices straight away from the contact point, which tears the surface open like an explosion. Higher values move the centre of the push off the surface, by this many contact radii, so the area around the contact is pressed inwards like a dent.")]
        [Range(0f, 4f)] public float ContactImpactLift = 0.5f;
    }
}

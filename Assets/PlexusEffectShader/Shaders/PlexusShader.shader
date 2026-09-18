// Shader based version of the wireframe plexus effect (URP).
// Works on meshes built by PlexusShaderMeshBuilder. The logic lives in PlexusShaderCore.hlsl, this file only
// holds the properties, the render state and the URP glue.
Shader "WireframePlexus/Plexus Shader (URP)"
{
    Properties
    {
        [Header(Look)]
        [HDR] _VertexColor ("Vertex Color", Color) = (0, 0.68, 1.6, 1)
        [HDR] _EdgeColor ("Edge Color", Color) = (0, 0.38, 1.72, 1)
        _VertexSize ("Vertex Size (object units)", Float) = 0.04
        _EdgeThickness ("Edge Thickness (object units)", Float) = 0.002
        _DotCore ("Dot Core (0 soft glow, 1 hard disc)", Range(0, 0.99)) = 0
        _DotSharpness ("Dot Sharpness", Range(1, 16)) = 8
        _MinEdgePixelWidth ("Min Edge Width (pixels)", Float) = 1

        [Header(Motion)]
        _MaxVertexMoveDistance ("Max Vertex Move Distance", Float) = 0.02
        _MinVertexMoveSpeed ("Min Vertex Move Speed", Float) = 0.005
        _MaxVertexMoveSpeed ("Max Vertex Move Speed", Float) = 0.01

        [Header(Edge visibility)]
        _MaxEdgeLengthPercent ("Max Edge Length Percent", Float) = 1.1
        _EdgeFadeDuration ("Edge Fade Duration (seconds)", Float) = 1
        [IntRange] _EdgeFadeTaps ("Edge Fade Taps", Range(1, 8)) = 4
        _EdgeFadeRange ("Edge Fade Range", Range(0.001, 1)) = 0.05

        [Header(Per object values set by PlexusShaderObject)]
        _PlexusSeed ("Seed", Float) = 1
        _PlexusSpawnTime ("Spawn Time", Float) = -1000

        [Header(Blending)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Source Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Destination Blend", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "Plexus"

            // Additive by default, like the two Shader Graphs of the ECS version.
            Blend [_SrcBlend] [_DstBlend], One One
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex PlexusVert
            #pragma fragment PlexusFrag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _VertexColor;
                float4 _EdgeColor;
                float _VertexSize;
                float _EdgeThickness;
                float _DotCore;
                float _DotSharpness;
                float _MinEdgePixelWidth;
                float _MaxVertexMoveDistance;
                float _MinVertexMoveSpeed;
                float _MaxVertexMoveSpeed;
                float _MaxEdgeLengthPercent;
                float _EdgeFadeDuration;
                float _EdgeFadeTaps;
                float _EdgeFadeRange;
                float _PlexusSeed;
                float _PlexusSpawnTime;
            CBUFFER_END

            #include "PlexusShaderCore.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 quadUV : TEXCOORD0;
                float4 color : TEXCOORD1; // HDR, so not a COLOR semantic
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings PlexusVert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 positionWS;
                PlexusComputeVertex(input.positionOS, input.uv0, input.uv1, positionWS, output.color, output.quadUV);
                output.positionCS = TransformWorldToHClip(positionWS);
                return output;
            }

            half4 PlexusFrag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return PlexusShade(input.color, input.quadUV);
            }
            ENDHLSL
        }
    }

    Fallback Off
}

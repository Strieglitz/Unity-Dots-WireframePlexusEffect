#ifndef PLEXUS_SHADER_CORE_INCLUDED
#define PLEXUS_SHADER_CORE_INCLUDED

// Core of the shader based plexus effect. Include it after Core.hlsl of the render pipeline and after the
// UnityPerMaterial uniforms (see PlexusShader.shader). Everything pipeline specific stays in that wrapper,
// PlexusComputeVertex only returns a world space position, a color and the quad coordinates.
//
// The whole animation is a pure function of (vertex id, time). Nothing is stored between frames, which is why
// an edge can work out where both of its end points are right now without asking anyone.

#define PLEXUS_MAX_CONTACTS 8
#define PLEXUS_MAX_FADE_TAPS 8
// Mean distance between two random points in a cube, as a multiple of its half extent (2 * Robbins constant).
#define PLEXUS_MEAN_HOP_LENGTH 1.3234
// The ECS version draws an edge as a bar with a square cross section. Seen from a random angle such a bar looks
// 4 / pi times wider than its thickness on average. The flat ribbon used here gets the same factor to match.
#define PLEXUS_BAR_WIDTH 1.2732

// Contact animations. PlexusShaderObject uploads them per object in a MaterialPropertyBlock.
float4 _PlexusContactPosRadius[PLEXUS_MAX_CONTACTS]; // xyz = world position, w = world radius
float4 _PlexusContactColor[PLEXUS_MAX_CONTACTS];
float4 _PlexusContactParams[PLEXUS_MAX_CONTACTS];    // x = color weight, y = push distance in object units, z = 1 when the origin below is valid
float4 _PlexusContactOrigin[PLEXUS_MAX_CONTACTS];    // xyz = world position the push moves away from
float _PlexusContactCount;

// ---------------------------------------------------------------------------------------------------------
// Hashing (PCG)
// ---------------------------------------------------------------------------------------------------------

uint PlexusPcg(uint v)
{
    uint state = v * 747796405u + 2891336453u;
    uint word = ((state >> ((state >> 28u) + 4u)) ^ state) * 277803737u;
    return (word >> 22u) ^ word;
}

float PlexusUnitFloat(uint h)
{
    return (float)(h >> 8u) * (1.0 / 16777216.0); // 24 bits, exact in a float, range 0..1
}

float3 PlexusHash3(uint key)
{
    uint h1 = PlexusPcg(key);
    uint h2 = PlexusPcg(h1);
    uint h3 = PlexusPcg(h2);
    return float3(PlexusUnitFloat(h1), PlexusUnitFloat(h2), PlexusUnitFloat(h3));
}

// ---------------------------------------------------------------------------------------------------------
// Stateless random walk
//
// The ECS version keeps an origin, a target and a timer per vertex: drift to a random point near the rest
// position, then pick the next one. Here time is cut into hops of fixed length per vertex. Hop number k runs
// from point hash(id, k) to point hash(id, k + 1), so the path is continuous and needs no memory.
// ---------------------------------------------------------------------------------------------------------

struct PlexusWalker
{
    uint key;          // hash key of this vertex, differs per object through _PlexusSeed
    float hopDuration; // seconds per hop
    float phase;       // keeps vertices from changing direction in the same frame
    float amplitude;   // max move distance in object units, 0 when the vertex does not move
};

PlexusWalker PlexusMakeWalker(float vertexId)
{
    PlexusWalker w;
    uint id = (uint)(vertexId + 0.5);
    uint seed = (uint)(_PlexusSeed + 0.5);
    w.key = PlexusPcg(id ^ (seed * 0x9E3779B9u));

    float3 r = PlexusHash3(w.key ^ 0x51ED270Bu);
    float fastest = max(_MinVertexMoveSpeed, _MaxVertexMoveSpeed);
    float speed = max(lerp(_MinVertexMoveSpeed, _MaxVertexMoveSpeed, r.x), 1e-6);
    bool moving = _MaxVertexMoveDistance > 0.0 && fastest > 0.0;

    w.amplitude = moving ? _MaxVertexMoveDistance : 0.0;
    // An average hop is PLEXUS_MEAN_HOP_LENGTH * distance long. Dividing by the speed gives the same pace as
    // the ECS version, where a hop takes distance / speed.
    w.hopDuration = max(PLEXUS_MEAN_HOP_LENGTH * _MaxVertexMoveDistance / speed, 1e-4);
    w.phase = r.y * 997.0;
    return w;
}

float3 PlexusWalkAt(PlexusWalker w, float time)
{
    if (w.amplitude <= 0.0)
        return float3(0.0, 0.0, 0.0);

    float t = max(time, 0.0) / w.hopDuration + w.phase;
    float hop = floor(t);
    float f = t - hop;
    uint k = (uint)hop;
    float3 from = PlexusHash3(w.key + k * 0x632BE5ABu) * 2.0 - 1.0;
    float3 to = PlexusHash3(w.key + (k + 1u) * 0x632BE5ABu) * 2.0 - 1.0;

    // New objects start as the exact mesh and loosen up during their first hop, like the ECS version.
    float unfold = saturate((time - _PlexusSpawnTime) / w.hopDuration);
    return lerp(from, to, f) * (w.amplitude * unfold);
}

// ---------------------------------------------------------------------------------------------------------
// Contacts
// ---------------------------------------------------------------------------------------------------------

void PlexusApplyContacts(float3 positionWS, float objectScale, inout float4 color, inout float3 pushWS)
{
    int count = (int)min(_PlexusContactCount + 0.5, (float)PLEXUS_MAX_CONTACTS);
    [loop] for (int i = 0; i < count; i++)
    {
        float3 offset = positionWS - _PlexusContactPosRadius[i].xyz;
        float radius = _PlexusContactPosRadius[i].w;
        float dist = length(offset);
        if (dist < radius)
        {
            float strength = 1.0 - dist / radius;
            color = lerp(color, _PlexusContactColor[i], strength * _PlexusContactParams[i].x);
            // The push does not start at the contact point itself. Pushing straight away from a point on the
            // surface sends its neighbours sideways in all directions and tears the mesh open. The origin sits
            // off the surface instead, so everything in range is pressed the same way and the surface dents.
            float3 origin = _PlexusContactParams[i].z > 0.5 ? _PlexusContactOrigin[i].xyz : _PlexusContactPosRadius[i].xyz;
            float3 fromOrigin = positionWS - origin;
            float originDist = length(fromOrigin);
            float3 direction = originDist > 1e-6 ? fromOrigin / originDist : float3(0.0, 1.0, 0.0);
            float pushStrength = strength * strength * (3.0 - 2.0 * strength); // rounded, so the dent has no sharp tip
            pushWS += direction * (pushStrength * _PlexusContactParams[i].y * objectScale);
        }
    }
}

// ---------------------------------------------------------------------------------------------------------
// Edge visibility
// ---------------------------------------------------------------------------------------------------------

// 1 below the length limit, 0 above it, with a soft band centred on the limit so the average brightness
// matches a hard threshold.
float PlexusEdgeVisibility(float lengthRatio)
{
    float band = max(_MaxEdgeLengthPercent * _EdgeFadeRange, 1e-5);
    return saturate((_MaxEdgeLengthPercent - lengthRatio) / band + 0.5);
}

// ---------------------------------------------------------------------------------------------------------
// Vertex stage
// ---------------------------------------------------------------------------------------------------------

// restOS  rest position of this vertex (dot) or of this end of the edge
// uv0     x, y = quad corner (dot) or ribbon side and end flag (edge), z = type (0 dot, 1 edge), w = id of this vertex
// uv1     xyz = rest position of the other end of the edge, w = its id
void PlexusComputeVertex(float3 restOS, float4 uv0, float4 uv1, out float3 positionWS, out float4 color, out float3 quadUV)
{
    float time = _Time.y;
    float3x3 linearOW = (float3x3)GetObjectToWorldMatrix();
    // One uniform scale for sizes, like the ECS version that averages the lossy scale.
    float objectScale = (length(linearOW._m00_m10_m20) + length(linearOW._m01_m11_m21) + length(linearOW._m02_m12_m22)) / 3.0;

    PlexusWalker walker = PlexusMakeWalker(uv0.w);
    float3 restWS = TransformObjectToWorld(restOS);
    float3 walkedWS = restWS + mul(linearOW, PlexusWalkAt(walker, time));

    if (uv0.z < 0.5)
    {
        // ---- dot: a quad that always faces the camera ----
        color = _VertexColor;
        float3 pushWS = float3(0.0, 0.0, 0.0);
        PlexusApplyContacts(walkedWS, objectScale, color, pushWS);

        float halfSize = 0.5 * _VertexSize * objectScale;
        float3 cameraRight = UNITY_MATRIX_V[0].xyz;
        float3 cameraUp = UNITY_MATRIX_V[1].xyz;
        positionWS = walkedWS + pushWS + (cameraRight * uv0.x + cameraUp * uv0.y) * halfSize;
        quadUV = float3(uv0.xy, 0.0);
    }
    else
    {
        // ---- edge: a ribbon between two animated vertices ----
        PlexusWalker otherWalker = PlexusMakeWalker(uv1.w);
        float3 otherRestWS = TransformObjectToWorld(uv1.xyz);
        float3 otherWalkedWS = otherRestWS + mul(linearOW, PlexusWalkAt(otherWalker, time));

        color = _EdgeColor;
        float4 otherColor = _EdgeColor; // only the push of the other end matters here
        float3 pushWS = float3(0.0, 0.0, 0.0);
        float3 otherPushWS = float3(0.0, 0.0, 0.0);
        PlexusApplyContacts(walkedWS, objectScale, color, pushWS);
        PlexusApplyContacts(otherWalkedWS, objectScale, otherColor, otherPushWS);

        float3 thisWS = walkedWS + pushWS;
        float3 otherWS = otherWalkedWS + otherPushWS;

        // Visible while current length / rest length stays below the limit. The ECS version fades alpha over
        // one second after the limit is crossed. Without state, the same ramp comes from averaging the
        // visibility over the last _EdgeFadeDuration seconds.
        float restLength = max(distance(restWS, otherRestWS), 1e-7);
        float visibility = PlexusEdgeVisibility(distance(thisWS, otherWS) / restLength);
        int taps = (int)clamp(_EdgeFadeTaps + 0.5, 1.0, (float)PLEXUS_MAX_FADE_TAPS);
        if (_EdgeFadeDuration > 0.0 && taps > 1)
        {
            float tapSpacing = _EdgeFadeDuration / taps;
            [loop] for (int j = 1; j < taps; j++)
            {
                float pastTime = time - tapSpacing * j;
                float3 a = restWS + mul(linearOW, PlexusWalkAt(walker, pastTime)) + pushWS;
                float3 b = otherRestWS + mul(linearOW, PlexusWalkAt(otherWalker, pastTime)) + otherPushWS;
                visibility += PlexusEdgeVisibility(distance(a, b) / restLength);
            }
            visibility /= taps;
        }
        // Fresh edges fade in, the ECS version starts them at alpha 0 as well.
        visibility *= saturate((time - _PlexusSpawnTime) / max(_EdgeFadeDuration, 1e-3));

        // Same direction at both ends, so the side vector agrees along the ribbon.
        float3 along = uv0.y < 0.5 ? otherWS - thisWS : thisWS - otherWS;
        float3 side = cross(along, GetWorldSpaceNormalizeViewDir(thisWS));
        float sideLength = length(side);
        side = sideLength > 1e-9 ? side / sideLength : float3(0.0, 0.0, 0.0);

        // Lines thinner than _MinEdgePixelWidth stay that wide and get dimmer instead, which keeps them from
        // breaking up into dashes.
        float thickness = _EdgeThickness * objectScale * PLEXUS_BAR_WIDTH;
        float clipW = TransformWorldToHClip(thisWS).w;
        float pixelSize = 2.0 * clipW / (abs(UNITY_MATRIX_P[1][1]) * _ScaledScreenParams.y);
        float width = max(thickness, _MinEdgePixelWidth * pixelSize);
        float widthFade = width > 1e-9 ? saturate(thickness / width) : 0.0;

        positionWS = thisWS + side * (uv0.x * 0.5 * width);
        color.a *= visibility * widthFade;
        quadUV = float3(uv0.x, 0.0, 1.0);
    }
}

// ---------------------------------------------------------------------------------------------------------
// Fragment stage
// ---------------------------------------------------------------------------------------------------------

float4 PlexusShade(float4 color, float3 quadUV)
{
    if (quadUV.z < 0.5)
    {
        // Round glow. With _DotCore = 0 and _DotSharpness = 8 it matches the default particle texture that the
        // ECS version uses. A _DotCore near 1 gives a hard disc.
        float r = length(quadUV.xy);
        float glow = 1.0 - smoothstep(_DotCore, 1.0, r);
        color.a *= pow(glow, _DotSharpness);
    }
    color.rgb = max(color.rgb, 0.0);
    return color;
}

#endif

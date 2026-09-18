# Plexus effect, shader version

A second implementation of the wireframe plexus effect that lives next to the ECS one in `Assets/PlexusEffect`.
It has no dependency on Entities. The whole animation runs in one vertex shader, so after the mesh is built
there is no per frame work on the CPU.

Open `ExampleScenes/PlexusShaderExampleScene` and press Play. No subscene is needed.

## How it works

1. `PlexusShaderMeshBuilder` turns the source mesh into a plexus mesh once: vertices that share a position
   are merged, every triangle edge is kept once, then one quad per edge and one quad per vertex is emitted.
2. Each quad corner stores what the shader needs to place itself: its rest position, the rest position of
   the other end of the edge, and the ids of both vertices.
3. The vertex shader moves a vertex with a stateless random walk. Time is cut into hops. Hop number k runs
   from the point `hash(id, k)` to the point `hash(id, k + 1)`, so the path is continuous without storing
   anything. An edge evaluates the same function for both of its ends, which is why lines and dots always meet.
4. An edge is visible while `current length / rest length` stays below `MaxEdgeLengthPercent`. The one second
   fade of the ECS version is rebuilt by averaging that test over the last `EdgeFadeDuration` seconds.
5. Dots are camera facing quads with a round glow. Edges are camera facing ribbons.

## Usage

1. Add `PlexusShaderObject` to a GameObject that has a MeshFilter, or assign a `SourceMesh`.
2. Assign `Materials/PlexusShaderMat` to `PlexusMaterial`. One material serves all objects, the per object
   values travel in a MaterialPropertyBlock.
3. Set the values under `Settings`. The first eight mean the same as in the ECS version.

The mesh must have Read/Write enabled. For big meshes, use the context menu entry `Bake Plexus Mesh Asset`
on the component and the build on Start is skipped.

The script API mirrors the ECS component:

- `UpdatePlexusObjectData()` sends changed `Settings` to the GPU. Inspector changes in play mode apply by themselves.
- `SetPlexusContactAnimation(...)` plays a contact animation. Up to 8 run at the same time per object.
  The seven parameter form is the same call as in the ECS version. An eighth parameter takes the direction of
  the impact, for example the velocity of a projectile or the negated collision normal.
- `SetPlexusObjectEnabled(bool)` shows or hides the effect.

`Examples/PlexusShaderContactDemo` fires a contact at a random vertex every few seconds.

## Differences to the ECS version

- A vertex keeps one speed and its hops have a fixed duration. In the ECS version the speed is rolled again
  for every hop. The average pace is the same.
- Contacts work in true world space. The ECS version compares unscaled vertex positions with a world space
  contact point. On a scale 2 object that puts the contact point outside the vertex cloud, so the surface
  dents, while a scale 1 object tears open. Here the dent is designed in: vertices are pushed away from a
  point lifted off the surface by `ContactImpactLift` contact radii, against the impact direction. 0 gives
  the tearing explosion, the default 0.5 gives a squeezed impact at any scale.
- The contact color runs as a gradient along an edge instead of one color per edge.
- Edges thinner than `MinEdgePixelWidth` pixels are drawn that wide and dimmed instead, so very thin lines
  do not break up. Set it to 0 to turn that off.
- The CPU does not know where the vertices are. The hash in `PlexusShaderCore.hlsl` can be ported to C# if
  gameplay ever needs the positions.

## Limits

- The shader file is written for URP. All logic sits in `PlexusShaderCore.hlsl` and returns a world space
  position, a color and quad coordinates, so a wrapper for another pipeline or a Shader Graph custom
  function only has to replace `PlexusShader.shader`.
- Objects with a MaterialPropertyBlock are not batched by the SRP Batcher. Each plexus object is one draw call.
- The walk uses `_Time.y` as a float. After many hours in one scene the motion of very fast settings can
  start to stutter.
- Keep baked plexus mesh assets readable. Vertex compression in builds only touches non readable meshes,
  and half precision texture coordinates would break the vertex ids stored in them.

## Measured in the editor

Unity 6000.6, 1280 x 720 game view, editor in the background, same camera and objects.

| Scene | Objects | CPU main thread | GPU | Frame rate |
|---|---|---|---|---|
| ECS | 6 (9,372 entities) | 3.3 to 3.5 ms | 3.4 to 3.6 ms | 187 fps |
| Shader | 6 | 2.1 to 2.9 ms | 0.5 ms | 239 fps |
| ECS | 66 (101,712 entities) | 5.8 to 7.0 ms | 5.3 to 5.7 ms | 116 fps |
| Shader | 66 | 2.3 ms | 0.5 ms | 229 fps |

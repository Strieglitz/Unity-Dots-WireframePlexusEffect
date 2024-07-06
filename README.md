# Unity-Dots-WireframePlexusEffect
A Plexus like effect with glowing vertecies and connection-lines based on the wireframe of the mesh. When the vertecies move around they connect and disconnect based on the distance. Implemented with ECS, parallel running jobs and burst compile.

Had no idea how to implement this effect in Shadergraph or Effectgraph so i tried it with ecs/dots. Would love to know how to achieve this with Shadergraph or an Effectgraph!

You can set any mesh and andjust some values like how fast and far the vertecies move from their original Position or how thick the connection lines are and how streched they have to be until they get invisible.

There is still tonns of room for improvements like adding lots of options to make the the effect more variable. If you have an idea or an request pleas let me know.

# Example with the buildin sphere mesh
![](https://github.com/Strieglitz/Unity-Dots-WireframePlexusEffect/blob/main/effect.gif)

# Or you can abuse it as a Wireframe Shader, when setting the vertices to not move around

![](https://github.com/Strieglitz/Unity-Dots-WireframePlexusEffect/blob/main/effect2.gif)

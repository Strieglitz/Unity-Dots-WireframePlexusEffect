# Unity-Dots-WireframePlexusEffect
A Plexus like effect with glowing vertecies and connection-lines based on the wireframe of the mesh. When the vertecies move around they connect and disconnect based on the distance. Implemented with ECS, parallel running jobs and burst compile.

Had no idea how to implement this effect in Shadergraph or Effectgraph so i tried it with ecs/dots. Would love to know how to achieve this with Shadergraph or an Effectgraph!

You can set any mesh and andjust some values like how fast and far the vertecies move from their original Position or how thick the connection lines are and how streched they have to be until they get invisible.

There is still tonns of room for improvements like adding lots of options to make the the effect more variable. If you have an idea or an request pleas let me know.

## How to Use
the Unity Project was build with the URP Renderpipeline. If you want to use the HDRP Renderpipeline you just have to convert the 2 Shaders that are used in the project. im not sure if you can automaticly upgrade them to hdrp in a hdrp project. but you can always rebuild them. in the URP project just open the shadres with the shadergraph editor and you can see how the shader is built. then open the HDRP project and rebuilt the Shader in Shadergraph.

### how to use the repo projcet
after donwloading or cloning the repo open it with the corresponding Unity version. The opne the "SampleScene" from "Assets/Scenes/". 
In the Scene you have to make sure the "EntitySubScene" is activated (is ticked) 

![grafik](https://github.com/user-attachments/assets/200b3a8f-5acc-428c-94f6-6f722e4d659f)

Then when you enter the Playmode the PlexusObjects should load and animate due to the parameters.
Now you can change the paramters, exit the PlayMode select the Gameobject "PlexeusObjectSphere" and change the parameters, then restart the playmode to what changed.

### import the package
todo

# Example with the buildin sphere mesh

![](https://github.com/Strieglitz/Unity-Dots-WireframePlexusEffect/blob/main/effect.gif)

# Or you can abuse it as a Wireframe Shader, when setting the vertices to not move around

![](https://github.com/Strieglitz/Unity-Dots-WireframePlexusEffect/blob/main/effect2.gif)

# Multi Object setup with different settings, meshes and colors

![](https://github.com/Strieglitz/Unity-Dots-WireframePlexusEffect/blob/main/effect3.gif)

# An contact color change animation for later use in a game

![](https://github.com/Strieglitz/Unity-Dots-WireframePlexusEffect/blob/main/effect4.gif)

# Contact animation with vertex distortion

![](https://github.com/Strieglitz/Unity-Dots-WireframePlexusEffect/blob/main/effect5.gif)

# multiple spheres stacked on top of each other

![](https://github.com/Strieglitz/Unity-Dots-WireframePlexusEffect/blob/main/effect6.gif)


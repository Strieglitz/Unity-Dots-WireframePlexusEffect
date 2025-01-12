# Unity-Dots-WireframePlexusEffect

A Plexus like effect with glowing vertecies and connection-lines based on the wireframe of the mesh. When the vertecies move around they connect and disconnect based on the distance. Implemented with ECS, parallel running jobs and burst compile.

![](https://github.com/Strieglitz/Unity-Dots-WireframePlexusEffect/blob/main/effect6.gif)

Had no idea how to implement this effect in Shadergraph or Effectgraph so i tried it with ecs/dots. Would love to know how to achieve this with Shadergraph or an Effectgraph!

You can set any mesh and andjust some values like how fast and far the vertecies move from their original Position or how thick the connection lines are and how streched they have to be until they get invisible.

There is still tonns of room for improvements like adding lots of options to make the the effect more variable. If you have an idea or an request please let me know.

## How to Use

### RenderPipeline

The effect does not run on heavy Shaders and can be used in **ALL RenderPipelines**. The development Unity Project was build with the URP Renderpipeline. If you want to use the HDRP Renderpipeline you just have to convert the 2 Shaders that are used in the project. Im not sure if you can automaticly upgrade them to HDRD in a HDRP-Project. But you can always rebuild them. In the URP-Project just open the shaders with the shadergraph editor and you can see how the shader is built. then open the HDRP project and rebuilt the Shader in a new Shadergraph.

### Unity Version

i built the Project with Unity 6 but i dont know in which version it will break, i will try out 2023 and 2022 and will report the results here.

### The Project Repository

After donwloading or cloning the repo, open it with the corresponding Unity version. The opne the "SampleScene" from "Assets/Scenes/".
In the Scene you have to make sure the "EntitySubScene" is activated (is ticked)
![grafik](https://github.com/user-attachments/assets/cb68dfd6-d566-433b-991a-22d6cb2642c5)

Then when you enter the Playmode the PlexusObjects should load and animate due to the parameters.
Now you can change the paramters, exit the PlayMode select the Gameobject "PlexeusObjectSphere" and change the parameters, then restart the playmode to what changed.

### How to import the package to your Project

here is an example on how to import the effect into a fresh project. 

- Download this Repository.
- Create a new Project
![NewPorject](TutorialImages/Step1_createProject.png)
- Open The PackageManager window to import the needed ECS dependencies.
![NewPorject](TutorialImages/Step2_goToPackageManager.png)
- In the PackageManager window, switch to the "UnityRegistry" section.
![NewPorject](TutorialImages/Step3_SwitchToUnityRegistry.png)
- Install the Entities package, which grabs all the neccessary packages for ECS 
![NewPorject](TutorialImages/Step4_installEntitiesPackage.png)
- Install the EntitiesGraphics package, which is needed to render meshes with ECS 
![NewPorject](TutorialImages/Step5_installEntitiesGraphicsPackage.png)
- close the Package Manager and rightclick in the Project panel to import the WireframePlexusEffect package
![NewPorject](TutorialImages/Step6_clickImport.png)

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

# Unity-Dots-WireframePlexusEffect

## Table of contents ##
1. [Overview](#overview)
    1. [Render Pipeline Compability](#renderpipeline)
    2. [Unity Version Compability](#unityversion)
3. [Getting Started](#gettingstarted)
    1. [Play around in the Repository ExampleScene](#examplescene)
    2. [Import to new project and setup](#import)
    3. [How to Spawn a PlexusEffect](#spawn)
4. [How it actualy works](#implementation)
5. [Tips](#tips)
6. [Visual Examples](#examples)

  
## Overview <a name="overview"></a>

A Plexus like effect with glowing vertecies and connection-lines based on the wireframe of the mesh. When the vertecies move around they connect and disconnect based on the distance. Implemented with ECS, parallel running jobs and burst compile.

![](https://github.com/Strieglitz/Unity-Dots-WireframePlexusEffect/blob/main/effect6.gif)

Had no idea how to implement this effect in Shadergraph or Effectgraph so i tried it with ecs/dots. Would love to know how to achieve this with Shadergraph or an Effectgraph!

You can set any mesh and andjust some values like how fast and far the vertecies move from their original Position or how thick the connection lines are and how streched they have to be until they get invisible.

There is still tonns of room for improvements like adding lots of options to make the the effect more variable. If you have an idea or an request please let me know.

### Render Pipeline Compability <a name="renderpipeline"></a>
The effect does not run on heavy Shaders and can be used in **ALL RenderPipelines**. The development Unity Project was build with the URP Renderpipeline. If you want to use the HDRP Renderpipeline you just have to convert the 2 Shaders that are used in the project. Im not sure if you can automaticly upgrade them to HDRD in a HDRP-Project. But you can always rebuild them. In the URP-Project just open the shaders with the shadergraph editor and you can see how the shader is built. then open the HDRP project and rebuilt the Shader in a new Shadergraph.

### Unity Version Compability <a name="unityversion"></a>
i built the Project with Unity 6 but i dont know in which version it will break. I tried out with 2022.3.55f1 and it worked just like described, so i just assume the 2023 will work too until some says otherwise.

## Getting Started <a name="gettingstarted"></a>
how to get started with this repository. you can just download it an play around in the sampleScene, create a new project and play around or load it into a existing one.

### Play around in the Repository ExampleScene <a name="examplescene"></a> 

After donwloading or cloning the repo, open it with the corresponding Unity version. The opne the "SampleScene" from "Assets/PlexusEffect/ExampleScene/".
In the Scene you have to make sure the "EntitySubScene" is activated (is ticked)
![grafik](https://github.com/user-attachments/assets/cb68dfd6-d566-433b-991a-22d6cb2642c5)

Then when you enter the Playmode the PlexusObjects should load and animate due to the parameters.
Now you can change the paramters, exit the PlayMode select the Gameobject "PlexeusObjectSphere" and change the parameters, then restart the playmode to what changed.

### Import it into your Project and Setup <a name="import"></a>

here is an example on how to import the effect into a fresh project. 

- Download this Repository.
- Create a new Project
![NewPorject](TutorialImages/Step1_createProject.png)
- Open The "PackageManager" window to import the needed ECS dependencies
![NewPorject](TutorialImages/Step2_goToPackageManager.png)
- In the "PackageManager" window, switch to the "UnityRegistry" section
![NewPorject](TutorialImages/Step3_SwitchToUnityRegistry.png)
- Install the "Entities" package, which grabs all the neccessary packages for ECS 
![NewPorject](TutorialImages/Step4_installEntitiesPackage.png)
- Install the "EntitiesGraphics" package, which is needed to render meshes with ECS 
![NewPorject](TutorialImages/Step5_installEntitiesGraphicsPackage.png)
- to import the "WireframePlexusEffect" package, close the "PackageManager" and rightclick in the Project panel and click "Import Package" -> "Custom Package" 
![NewPorject](TutorialImages/Step6_clickImport.png)
- navigate to the location where you donwloaded/cloned this repository and select the "plexusEffectPackageToImport" package from the "assets" folder, and confirm
![NewPorject](TutorialImages/Step7_choosePackageToImport.png)
- in the import dialog click "import", this will genreate a new folder called "PlexusEffect" where you can find all the PlexusEffect realated stuff  
![NewPorject](TutorialImages/Step8_Click_import.png)
- now that that the plexusEffect is imported, lets make some effects. In order to use ECS Entites a "Subscene" is needed. So rightclick into the "Hierarchy" and create a new Subscene. The Subscene is like a Scene but for ECS Entites only, so we will fill it later with ECS life.
![NewPorject](TutorialImages/Step9_CreateNewSubscene.png)
- give the Subscene a meaningfull name
![NewPorject](TutorialImages/Step10_CreateNewSubscene2.png)
- now that the Subscene is created, make sure that it is ticked and enabled, because a subscene can also be disabled
![NewPorject](TutorialImages/Step11_MakeSureSubsceneIsTicked.png)
- In order to fill the Subscene with life, we have to register the Entities that we want to use later in the Subscene. To do so, navigate to PlexusEffect/prefabs/Subscene folder in your assets. 
![NewPorject](TutorialImages/Step12_NavigateToSubscenePrefabs.png)
- and drag and drop all 3 "EntitySpawner"-Prefabs into the Subscene we created earlier. These will tell the Subscene how to Instantitate a new Entity instance. 
![NewPorject](TutorialImages/Step13_DragSubscenePrefabsToSubscene.png)

Great, now the PlexusEffect stuff is setup and ready to be used to actualy render something! 

### Spawn PlexusEffects in your Project <a name="spawn"></a> 

in this example we create a simple PlexusEffect on a Unity-sphere

- In our regular Scene lets create a new Gameobject and choose the default Sphere, with rightclick into the "Hierarchy" and select GameObject->3D Object->Sphere 
![NewPorject](TutorialImages/Step14_CreateNewSphere.png)

- Add the "PlexusGameobjectFromMesh" Component to the Sphere Gameobject. This Component will iterate over the MeshData and convert it to a kind of PlexusEffectMesh data that will be used by the Effect. this will happen when the scene starts and is quite a heavy calculation. for an example like this, it is fine, but for a more preformance critical setup you should consider using the "PlexusGameObjectPrecalculated" Component
![NewPorject](TutorialImages/Step15_AddPlexusGameobjectFromMeshComponent.png)

- now the Spehere is a "PlexusGameObject" but you will not be able to see it, because there are no values in the "PlexusGameObjectData" Field of the "PlexusGameobjectFromMesh" Component. So lets fill it with some values.

![NewPorject](TutorialImages/Step16_FillTheCompoenntWithValues.png)

- to better see the actual Plexus Effect, move the Sphere Closer to the Camera, because the position of the PlexusEffect will be synced with the position of the Sphere, even at runtime. 
![NewPorject](TutorialImages/Step17_MoveSphereCloserToCamera.png)

- test out the Effect and press the play button. If you used the values from the previous section, you should get a blue Sphere with the plexusEffect.   
![NewPorject](TutorialImages/Step18_TestEffect.png)

- the effect could look much better, with the default settings you will see aliasing and hard edges, so lets set the AntiAliasing. In this case with an URP Proejct you can set the AA in the PC_RP.asset  
![NewPorject](TutorialImages/Step19_IncreaseAntiAliasing.png)

- Postprocessing will also help, increasing the Bloom will also result in a better look with softer edges on the edges and vertecies.
![NewPorject](TutorialImages/Step20_IncreaseBloom.png)

- Now you have a working example and can play around with the values and find out cool combinations and visuals. 
![NewPorject](TutorialImages/Step21_PlayAround.png)

## How it works <a name="implementation"></a>
Todo
## Tips <a name="tips"></a>

## Examples <a name="examples"></a>

### Example with the buildin sphere mesh

![](https://github.com/Strieglitz/Unity-Dots-WireframePlexusEffect/blob/main/effect.gif)

### Or you can abuse it as a Wireframe Shader, when setting the vertices to not move around

![](https://github.com/Strieglitz/Unity-Dots-WireframePlexusEffect/blob/main/effect2.gif)

### Multi Object setup with different settings, meshes and colors

![](https://github.com/Strieglitz/Unity-Dots-WireframePlexusEffect/blob/main/effect3.gif)

### An contact color change animation for later use in a game

![](https://github.com/Strieglitz/Unity-Dots-WireframePlexusEffect/blob/main/effect4.gif)

### Contact animation with vertex distortion

![](https://github.com/Strieglitz/Unity-Dots-WireframePlexusEffect/blob/main/effect5.gif)

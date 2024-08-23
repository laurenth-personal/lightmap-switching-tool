
[![openupm](https://img.shields.io/npm/v/com.laurenth.lightingtools-lightmapswitcher?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.laurenth.lightingtools-lightmapswitcher/) _click this badge to go to the open upm page for this package_

Tool intended for **switching pre-baked lightmaps**, light probes and realtime lighting on a static scene at runtime.

Depending on the platform or depending on the content, the switch might not be instant but take some seconds, this script just allows you avoid duplicating your scene if you just want to change the lighting.

This version is compatible with **unity 2019.3** and above, check previous releases for unity 5.5 - 5.6 version.

If you want to use lightmaps of different resolutions in your different lighting scenarios you will probably need to **disable static batching** in the PlayerSettings (if you use the same lightmap resolution on all your lighting scenarios and the object packing in the lightmap atlas doesn't change accross lighting scenarios it's ok to keep static batching enabled).

The system relies on this component :

**LevelLightmapData**
It references the different lighting scenes, builds the lighting, and stores the dependencies to the lightmaps.

If your lighting scene contains Realtime/Mixed lights or Reflection probes, the script will consider it's necessary to load the lighting scene at runtime to replicate the full lighting. The lighting scene will thus need to be part of the "Scenes in Build" list in the Build settings (File/Build Settings).

### How it works

- Make a scene with your static geometry only. Disable Auto baking (important). If you want to use lightprobes, also add a lightprobe group to the geometry scene.
- Make several lighting scenes in your project. These scenes should not contain static geometry. The Lighting scene settings must not use auto baking.
- Add your lighting scenes and your static geometry scene to your Build Settings scene list.
- In your static geometry scene, add an empty gameObject and attach a **LevelLightmapData** component to it. 
- Fill the "lighting scenarios size" with the count of lighting scenarios you want, and in the "element" fields, either drag and drop scenes from your Project view or click the little point on the right of the field to choose a scene from your project
- One by one, you can now Build the lighting scenario, and when the bake is done Store it. You need to do these steps in the lighting scenario order ( you have to build and then store lighting scenario 1 before lighting scenario 2 according to the order in the list). The first time it is crucial to do it in the right order, if you misclicked I'd recommend redoing the whole setup (click the wheel in the top right corner of the component and hit "reset" and do the setup again).
- Call the public method **LoadLightingScenario** using an integer argument that represents the index of the lighting scenario in the list of scenarios. The UI buttons in this sample project do this through the use of the button's **UnityEvent**.
- Start playing -> In the sample project, click the different buttons on the screen to switch to a different lighting. In your own project, use script or UnityEvents to call the LoadLightingScenario method as described previously

### Tutorial
- [Quick start video tutorial](https://drive.google.com/file/d/11InmKeKM6IMg445iYz4N89Zkerre_Mot/view?usp=sharing)

### Supports

- Lightmaps
- Light Probes
- Mixed lighting mode (tested only "baked indirect" and "shadowmask")
- Reflection probes, they need to be placed in the lighting scenes.

### FAQ

- How do I select the default lighting scenario / fallback lighting scenario ?
  - The LevelLightmapData script doesn't load any lighting scenario by default, your scene will load in the state you last saved it in. You need to explicitly call the LevelLightmapData from a script or a unity event OnEnable or OnAwake if you want to load a lighting scenario when you start your scene. This way you are in full control of when you want to load your lighting scenario.

### Contributors :

- Thanks to [Kretin1](https://github.com/Kretin1) for his help on shadowmask support.

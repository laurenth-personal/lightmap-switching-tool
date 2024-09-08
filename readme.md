[![openupm](https://img.shields.io/npm/v/com.laurenth.lightingtools-lightmapswitcher?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.laurenth.lightingtools-lightmapswitcher/) _click this badge to go to the open upm page for this package_

# Lightmap switching tool

Tool intended for **switching pre-baked lightmaps**, light probes and realtime lighting on a static scene at runtime.

Depending on the platform or depending on the content, the switch might not be instant but take some seconds, this script just allows you avoid duplicating your scene if you just want to change the lighting.

This version is compatible with **unity 2019.3** and above, check previous releases for unity 5.5 - 5.6 version.

If you want to use lightmaps of different resolutions in your different lighting scenarios you will probably need to **disable static batching** in the PlayerSettings (if you use the same lightmap resolution on all your lighting scenarios and the object packing in the lightmap atlas doesn't change accross lighting scenarios it's ok to keep static batching enabled).

The system relies on the **LevelLightmapData** component :

It references the different lighting scenes, builds the lighting, and stores the dependencies to the lightmaps.

If your lighting scene contains Realtime/Mixed lights or Reflection probes, the script will consider it's necessary to load the lighting scene at runtime to replicate the full lighting. The lighting scene will thus need to be part of the "Scenes in Build" list in the Build settings (File/Build Settings).

### How it works

- Make a scene with your static geometry only. Disable Auto baking (important). If you want to use lightprobes, also add a lightprobe group to the geometry scene.
- Make several lighting scenes in your project. These scenes should not contain static geometry. The Lighting scene settings must not use auto baking.
- Add your lighting scenes and your static geometry scene to your Build Settings scene list.
- In your static geometry scene, add an empty gameObject and attach a **LevelLightmapData** component to it. 
- Fill the "lighting scenarios size" with the count of lighting scenarios you want, and in the "element" fields, either drag and drop scenes from your Project view or click the little point on the right of the field to choose a scene from your project
- One by one, you can now Build the lighting scenario. At the end make sure to save your scene.
- Call the public method **LoadLightingScenario** using an integer argument that represents the index of the lighting scenario in the list of scenarios. The UI buttons in this sample project do this through the use of the button's **UnityEvent**.
- Start playing -> In the sample project, click the different buttons on the screen to switch to a different lighting. In your own project, use script or UnityEvents to call the LoadLightingScenario method as described previously

### Scriptable object workflow

- Rigt click in the project window, Create / Lighting / Lighting Scenario Data
- Name your asset
- In the inspector type the geometry scene name and the lighting scene name
- Choose if you want to store renderer infos (useful only if your lightmap scales and offsets change on the different scenes)
- Click "Generate lighting scenario data"
- Do this for all your lighting scenarios

- In the Geometry scene, add a Gameobject and add the LevelLightmapData script on it
- Click "Show scenario data"
- In "Lighting scenarios" type the number of scenarios you have
- Drag and drop the lighting scenarios in the slots
- Call the method **LoadLightingScenario** using either an integer argument the represents the index in the list of lighting scenario datas, or you can use a string argument to load the lighting scenario by name
- This example is illustrated by the Scene "GeometrySceneScriptableObj.unity"

### Tutorial
- [Quick start video tutorial](https://drive.google.com/file/d/11InmKeKM6IMg445iYz4N89Zkerre_Mot/view?usp=sharing)
- [Setup from scratch](https://drive.google.com/file/d/19YsWl-rD-s2akd87UENzj4woynY13-Yh/view?usp=drive_link)
- [Scriptable object workflow (for asset bundles)](https://drive.google.com/file/d/1kSKWHFYy_BN-1uIkYIcdwnXiTzX0R_wp/view?usp=drive_link)

### Supports

- Lightmaps
- Light Probes
- Mixed lighting mode (tested only "baked indirect" and "shadowmask")
- Reflection probes, they need to be placed in the lighting scenes.
- Terrain with lightmaps

### Static batching
If you want to use static batching, lightmap scale and offset cannot be applied at runtime.
Make sure your lightmap resolutions are the same in all lighting scenarios, and on the LevelLightmapData component make sure to disable "Apply Lightmap Scale and Offset".

### How to install (2020.2 and Newer)
- In Unity, Open Project Settings Window (Edit/Project Settings) 
- Select Package Manager
- Add a new Scoped Registry that references the openupm registry: https://package.openupm.com
- Add the following scope to the OpenUPM Scoped Registry : com.laurenth
- Open the Package Manager window (Window/Package Manager) and Select Packages : My Registries in the toolbar
- Select Ligthmap switcher in the list, then click the Install Button

### Importing the sample :

- Go to Window / Package manager
- In the left panel select the Lightmap switcher
- In the right panel you should see a "Samples" section
- See the "Example scene" line and click the button "Import to Project"
- Open ExampleScene.unity at Assets\Samples\Lightmap Switcher\VersionNumber\Example scene\Scenes

### FAQ

- How do I select the default lighting scenario / fallback lighting scenario ?
  - The LevelLightmapData script doesn't load any lighting scenario by default, your scene will load in the state you last saved it in. You need to explicitly call the LevelLightmapData from a script or a unity event OnEnable or OnAwake if you want to load a lighting scenario when you start your scene. This way you are in full control of when you want to load your lighting scenario.

### Contributors :

- Thanks to [Kretin1](https://github.com/Kretin1) for his help on shadowmask support.

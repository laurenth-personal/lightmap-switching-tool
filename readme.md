Tool intended for switching pre-baked lightmaps and realtime lighting on a static scene at runtime.
Depending on the platform or depending on the content the switch might not be instant but take some seconds, this script just allows you avoid duplicating your scene if you just want to change the lighting.

This version is compatible with unity 2017.1 and above, check previous releases for unity 5.5 - 5.6 version.

If you want to use lightmaps of different resolutions in your different lighting scenarios you will probably need to disable static batching in the PlayerSettings (if the object packing in the lightmap changes you need to disable static batching, if just the lightmaps' scale changes, that's fine).

The system relies on 2 components :

LevelLightmapData : References the different lighting scenarios, builds the lighting, and stores the dependencies to the lightmaps.
LightingScenarioSwitcher : This is just an example of asset that calls the LevelLightmapData in order to switch lightmaps at runtime. You could build other components that call the LevelLightmapData in the same way but on different events (like you could use a box trigger running the same script on OnTriggerEnter ).

How it works :

- Make a scene with your static geometry only. Disable Auto baking (important).
- Make several lighting scenes in a "Scenes" folder (under Assets/ ). These scenes should not contain static geometry. The Lighting scene settings must not use auto baking.
- In your static geometry scene, add an empty gameObject and attach a LevelLightmapData component to it. 
- Fill the "lighting scenarios size" with the count of lighting scenarios you want, and in the "element" fields, either drag and drop scenes from your Project view or click the little point on the right of the field to choose a scene from your project
- One by one, you can now Build the lighting scenario, and when the bake is done Store it. You need to do these steps in the lighting scenario order ( you cannot build and store lighting scenario 2 before lighting scenario 1 ). 
- Now add an empty Gameobject to your scene and add a LightingScenarioSwitcher for previewing the switch
- The Default lighting scenario field is the ID of the lighting scenario you want to load when you start playing. The ID is the number associated to the lighting scenario in the LevelLightmapData ( ID of "Element 0" is 0 for example )
- Start playing and when clicking ( left mouse button or Fire1 if you have changed the input assignments ) the lightmaps should switch between your different lighting scenarios.

NEW :

- Mixed lighting mode are supported (tested only "baked indirect" and "shadowmask")
- Reflection probes supported, they need to be placed in the lighting scenes.

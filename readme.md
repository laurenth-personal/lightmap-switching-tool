Tool intended for switching pre-baked lightmaps at runtime.
This tool is not compatible with unity versions below 5.5, see notes.
If you want to use lightmaps of different resolutions in your different lighting scenarios you will probably need to disable static batching in the PlayerSettings (if the lightmaps atlassing changes you need to disable static batching, if just the lightmaps' scale changes, that's fine).

The system relies on 2 components :
- LevelLightmapData : References the different lighting scenarios, builds the lighting, and stores the dependencies to the lightmaps.
- LightingScenarioSwitcher : This is just an example of asset that calls the LevelLightmapData in order to switch lightmaps at runtime. You could build other components that call the LevelLightmapData in the same way but on different events (like you could use a box trigger running the same script on OnTriggerEnter ).

How it works :
- Make a scene with your static geometry only. Disable Auto baking.
- Make several lighting scenes in a "Scenes" folder (under Assets/ ). These scenes should not contain static geometry (unless you want extra baked shadows). The Lighting scene settings must be : no Auto baking, only baked GI. Your lights in the scene must be baked, see notes.
- In your static geometry scene, add an empty gameObject and attach a LevelLightmapData component to it
- Fill the "lighting scenarios size" with the count of lighting scenarios you want, and in the "element" fields write down the names or paths for the lighting scenes (path relative to Assets/Scenes/ with scene name without .unity)
- One by one, you can now Build the lighting scenarios. You need to do Build in the lighting scenario order ( you cannot build element 2 before element 1 ). 
- Now add an empty Gameobject to your scene and add a LightingScenarioSwitcher for previewing the switch
- The Default lighting scenario field is the ID of the lighting scenario you want to load when you start playing. The ID is the number associated to the lighting scenario in the LevelLightmapData ( ID of "Element 0" is 0 for example )
- Start playing and when clicking ( left mouse button or Fire1 if you have changed the input assignments ) the lightmaps should switch between your different lighting scenarios.

NEW :
- Combined "Build" and "Store" steps, no need to start playing in order to load  the lightmaps correctly anymore.
- Stores also the lightprobes from your StaticGeometry scene ( the number and position of the lightprobes cannot change between lighting scenarios )
- Refactor, grouped custom wrappers, renamed variables for consistency and clarity

CHANGES :
- Default lighting scenario moved to LightingScenarioSwitcher so that the LevelLightmapData does not have any runtime logic, all lightmap switches need to be triggered from the switcher.

NOTES : 
- if you want to use unity versions below 5.5, you should at least change the name of the lightmaps in the LevelLightmapData script ( replace lightmapLight and lightmapDir respectively with lightmapNear and lightmapFar, not tested ).
- if you want to use lights that don't have direct lighting baked, then you would need to also load the lighting scene associated with the lightmaps when operating the switch.

TODO :
- Include reflection probes
- Include an example of lighting switching with lights that have baked GI but realtime direct lighting

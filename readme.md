Tool intended for switching pre-baked lightmaps at runtime.
This tool is not compatible with unity versions below 5.5, see notes.
If you want to use lightmaps of different resolutions in your different lighting scenarios you will probably need to disable static batching in the PlayerSettings (if the number of lightmaps changes, if just the lightmaps' scale changes that's fine).

The system relies on 2 components :
- LevelLightmapData : References the different lighting scenarios, builds the lighting, and stores the dependencies to the lightmaps.
- LightingScenarioSwitcher : This is just an example of asset that calls the LevelLightmapData in order to switch lightmaps at runtime. You could build other components that call the LevelLightmapData in the same way but on different events (like you could use a box trigger running the same script on OnTriggerEnter ).

How it works :
- Make a scene with your static geometry only. Disable Auto baking.
- Make several lighting scenes in a "Scenes" folder (under Assets/ ). These scenes should not contain static geometry (unless you want extra baked shadows). The Lighting scene settings must be : no Auto baking, only baked GI. Your lights in the scene must be baked, see notes.
- In your static geometry scene, add an empty gameObject and attach a LevelLightmapData component to it
- Fill the "lighting scenarios size" with the count of lighting scenarios you want, and in the "element" fields write down the names or paths for the lighting scenes (path relative to Assets/Scenes/ without .unity)
- One by one, you can now Build the lighting scenario, and then Store it. Those two steps have not been merged because sometimes the lightmaps don't load correctly after baking and you need to start playing and stop in order to see them properly and THEN you can store them (otherwise you get a bunch of errors I believe)
- The Default lighting scenario field is the ID of the lighting scenario you want to load when you start playing and when you go back to the editor ( ID of "Element 0" is 0 for example )
- Now add an empty Gameobject to your scene and add a LightingScenarioSwitcher for previewing the switch
- Start playing and when clicking ( left mouse button or Fire1 if you have changed the input assignments ) the lightmaps should switch between your different lighting scenarios.

NOTES : 
- if you want to use unity versions below 5.5, you should at least change the name of the lightmaps in the script (lightmapNear and lightmapFar have become lightmapLight and lightmapDir).
- if you want to use lights that don't have direct lighting baked, then you would need to also load the lighting scene associated with the lightmaps when operating the switch.

TODO :
- Store lightprobes for each lighting scenario
- Include reflection probes
- Include an example of lighting switching with lights that have baked GI but realtime direct lighting

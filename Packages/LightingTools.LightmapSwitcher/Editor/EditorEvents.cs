using UnityEngine;
using UnityEditor;

// ensure class initializer is called whenever scripts recompile
[InitializeOnLoadAttribute]
public static class EditorEvents
{
    // register an event handler when the class is initialized
    static EditorEvents()
    {
        EditorApplication.playModeStateChanged += PlayModeChange;
    }

    private static void PlayModeChange(PlayModeStateChange state)
    {
        var lightmapData = GameObject.FindObjectOfType<LevelLightmapData>();
        if (lightmapData != null)
        {
                switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    lightmapData.OnEnteredPlayMode_EditorOnly();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    lightmapData.OnExitingPlayMode_EditorOnly();
                    break;
            }
        }
    }
}
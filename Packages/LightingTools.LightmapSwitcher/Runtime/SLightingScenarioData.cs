using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "LightingScenario", menuName = "Lighting/Lighting Scenario Data")]
public class SLightingScenarioData : ScriptableObject
{
    public string sceneName;
    public LevelLightmapData.RendererInfo[] rendererInfos;
    public Texture2D[] lightmaps;
    public Texture2D[] lightmapsDir;
    public Texture2D[] shadowMasks;
    public LightmapsMode lightmapsMode;
    public LevelLightmapData.SphericalHarmonics[] lightProbes;
    public bool hasRealtimeLights;

}
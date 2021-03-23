using UnityEngine;

[CreateAssetMenu(fileName = "LightingScenario", menuName = "Lighting/Lighting Scenario Data")]
public class LightingScenarioData : ScriptableObject
{
    public string sceneName;
    public LevelLightmapData.RendererInfo[] rendererInfos;
    public Texture2D[] lightmaps;
    public Texture2D[] lightmapsDir;
    public Texture2D[] shadowMasks;
    public LightmapsMode lightmapsMode;
    public UnityEngine.Rendering.SphericalHarmonicsL2[] lightProbes;
    public bool hasRealtimeLights;
}
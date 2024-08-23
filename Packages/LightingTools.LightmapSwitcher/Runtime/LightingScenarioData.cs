using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "LightingScenario", menuName = "Lighting/Lighting Scenario Data")]
public class LightingScenarioData : ScriptableObject
{
    [FormerlySerializedAs("sceneName")]
    public string lightingSceneName;
    public string geometrySceneName;
    public bool storeRendererInfos;
    public LevelLightmapData.RendererInfo[] rendererInfos;
    public Texture2D[] lightmaps;
    public Texture2D[] lightmapsDir;
    public Texture2D[] shadowMasks;
    public LightmapsMode lightmapsMode;
    public LightProbesAsset lightProbesAsset;
    public bool hasRealtimeLights;
}
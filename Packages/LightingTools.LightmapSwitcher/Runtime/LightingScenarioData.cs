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
    [FormerlySerializedAs("lightProbes")]
    public UnityEngine.Rendering.SphericalHarmonicsL2[] coefficients;
    [SerializeField]
    public LightProbes lightprobes;
    public bool hasRealtimeLights;
}

using UnityEngine;

[CreateAssetMenu(fileName = "LightingScenario", menuName = "Lighting/Lighting Scenario Data")]
public class LightingScenarioData : ScriptableObject
{
    public string geometrySceneName;
    public string lightingSceneName;
    public LightingSwitcher.RendererInfo[] rendererInfos;
    public Texture2D[] lightmaps;
    public Texture2D[] lightmapsDir;
    public Texture2D[] shadowMasks;
    public UnityEngine.Rendering.SphericalHarmonicsL2[] lightProbes;
    public LightmapsMode lightmapsMode;
    [Tooltip("Tells if the lighting scene contains lights (mixed or realtime) or reflection probes that need to be present at runtime")]
    public bool hasRealtimeLights;
}
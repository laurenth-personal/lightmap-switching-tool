using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "LightProbes", menuName = "Lighting/Light Probes Asset")]
public class LightProbesAsset : ScriptableObject
{
    [SerializeField]
    public UnityEngine.Rendering.SphericalHarmonicsL2[] lightProbes;
}

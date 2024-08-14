using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "LightProbes", menuName = "Lighting/Light Probes Asset")]
public class LightProbesAsset : ScriptableObject
{
    [SerializeField]
    [FormerlySerializedAs("lightProbes")]
    public UnityEngine.Rendering.SphericalHarmonicsL2[] coefficients;
    [SerializeField]
    public LightProbes lightprobes;
}

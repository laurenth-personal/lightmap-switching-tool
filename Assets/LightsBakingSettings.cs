using System;
using System.Collections.Generic;
using UnityEngine;

public class LightsBakingSettings : MonoBehaviour
{

    void MuteBakedLights()
    {
        var sceneLights = FindObjectsOfType<Light>();
        var sceneBakedLights = new List<Light>();

        foreach (Light light in sceneLights)
        {
            if (light.lightmapBakeType == LightmapBakeType.Baked)
                sceneBakedLights.Add(light);
        }
        foreach(Light light in sceneBakedLights)
        {
        }
    }
}
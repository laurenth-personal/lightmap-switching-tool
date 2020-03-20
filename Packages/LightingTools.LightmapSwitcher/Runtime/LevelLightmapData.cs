using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Collections;

[ExecuteInEditMode]
public class LevelLightmapData : MonoBehaviour
{
    [System.Serializable]
    public class SphericalHarmonics
    {
        public float[] coefficients = new float[27];
    }

	[System.Serializable]
	public class RendererInfo
	{
        public int hash;
		public int lightmapIndex;
		public Vector4 lightmapOffsetScale;
	}

    public LightingScenarioData[] lightingScenarioDatas;
	

    public bool latestBuildHasReltimeLights;
    public bool allowLoadingLightingScenes = true;
    [Tooltip("Enable this if you want to use different lightmap resolutions in your different lighting scenarios. In that case you'll have to disable Static Batching in the Player Settings. When disabled, Static Batching can be used but all your lighting scenarios need to use the same lightmap resolution.")]
    public bool applyLightmapScaleAndOffset = true;

#if UNITY_EDITOR
    [SerializeField]
	public List<UnityEditor.SceneAsset> lightingScenariosScenes;
#endif
    [SerializeField]
    public String[] lightingScenesNames = new string[1];
    public string currentLightingScenario;
    public string previousLightingScenario;

    private Coroutine m_SwitchSceneCoroutine;

    Dictionary<string, LightingScenarioData> dictionnary;

    //TODO : enable logs only when verbose enabled
    public bool verbose = false;

    private List<SphericalHarmonicsL2[]> lightProbesRuntime = new List<SphericalHarmonicsL2[]>();

    public void LoadLightingScenario(string scenarioName)
    {
        if(scenarioName != currentLightingScenario)
        {
            previousLightingScenario = currentLightingScenario == null ? scenarioName : currentLightingScenario;

            currentLightingScenario = scenarioName;

            if(dictionnary == null)
            {
                Debug.Log("No lighting scenario found");
                return;
            }

            var lightingData = (LightingScenarioData)ScriptableObject.CreateInstance(typeof(LightingScenarioData));
            var currentscenario = dictionnary.TryGetValue(scenarioName, out lightingData);

            LightmapSettings.lightmapsMode = lightingData.lightmapsMode;

            if (allowLoadingLightingScenes)
                m_SwitchSceneCoroutine = StartCoroutine(SwitchSceneCoroutine(previousLightingScenario, currentLightingScenario, lightingData));

            var newLightmaps = LoadLightmaps(lightingData);

            //if(applyLightmapScaleAndOffset)
            //{
            //    ApplyRendererInfo(lightingScenarioDatas[index].rendererInfos);
            //}

            LightmapSettings.lightmaps = newLightmaps;

            LoadLightProbes(lightingData);
        }
    }

    private void Start()
    {
        dictionnary = new Dictionary<string,LightingScenarioData>();

        var scenarios = Resources.FindObjectsOfTypeAll<LightingScenarioData>();
        foreach (var scenario in scenarios)
        {
            dictionnary.Add(scenario.sceneName, scenario);
        }
        if (verbose && Application.isEditor)
            Debug.Log("Loaded " + scenarios.Length + " lighting scenarios.");
    }

    private SphericalHarmonicsL2[] DeserializeLightProbes(LightingScenarioData lightingData)
    {
        var sphericalHarmonicsArray = new SphericalHarmonicsL2[lightingData.lightProbes.Length];

        for (int i = 0; i < lightingData.lightProbes.Length; i++)
        {
            var sphericalHarmonics = new SphericalHarmonicsL2();

            // j is coefficient
            for (int j = 0; j < 3; j++)
            {
                //k is channel ( r g b )
                for (int k = 0; k < 9; k++)
                {
                    sphericalHarmonics[j, k] = lightingData.lightProbes[i].coefficients[j * 9 + k];
                }
            }

            sphericalHarmonicsArray[i] = sphericalHarmonics;
        }
        return sphericalHarmonicsArray;
    }

    IEnumerator SwitchSceneCoroutine(string sceneToUnload, string sceneToLoad, LightingScenarioData lightingData)
    {
        AsyncOperation unloadop = null;
        AsyncOperation loadop = null;

        if (sceneToUnload != null && sceneToUnload != string.Empty && sceneToUnload != sceneToLoad)
        {
            unloadop = SceneManager.UnloadSceneAsync(sceneToUnload);
            while (!unloadop.isDone)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        if(sceneToLoad != null && sceneToLoad != string.Empty && sceneToLoad != "")
        {
            loadop = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
            while ((!loadop.isDone || loadop == null))
            {
                yield return new WaitForEndOfFrame();
            }   
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad));
        }
        LoadLightProbes(lightingData);
    }

    LightmapData[] LoadLightmaps(LightingScenarioData lightingData)
    {
        if (lightingData.lightmaps == null
                || lightingData.lightmaps.Length == 0)
        {
            if (verbose && Application.isEditor)
                Debug.LogWarning("No lightmaps stored in scenario " + lightingData.sceneName);
            return null;
        }

        var newLightmaps = new LightmapData[lightingData.lightmaps.Length];

        for (int i = 0; i < newLightmaps.Length; i++)
        {
            newLightmaps[i] = new LightmapData();
            newLightmaps[i].lightmapColor = lightingData.lightmaps[i];

            if (lightingData.lightmapsMode != LightmapsMode.NonDirectional)
            {
                newLightmaps[i].lightmapDir = lightingData.lightmapsDir[i];
            }
            if (lightingData.shadowMasks.Length > 0)
            {
                newLightmaps[i].shadowMask = lightingData.shadowMasks[i];
            }
        }

        return newLightmaps;
    }

    public void ApplyRendererInfo(RendererInfo[] infos)
    {
        try
        {
            Terrain terrain = FindObjectOfType<Terrain>();
            int i = 0;
            if (terrain != null)
            {
                terrain.lightmapIndex = infos[i].lightmapIndex;
                terrain.lightmapScaleOffset = infos[i].lightmapOffsetScale;
                i++;
            }

            for (int j = i; j < infos.Length; j++)
            {
                //RendererInfo info = infos[j];
                //info.renderer.lightmapIndex = infos[j].lightmapIndex;
                //if (!info.renderer.isPartOfStaticBatch)
                //{
                //    info.renderer.lightmapScaleOffset = infos[j].lightmapOffsetScale;
                //}
                //if (info.renderer.isPartOfStaticBatch && verbose == true && Application.isEditor)
                //{
                //    Debug.Log("Object " + info.renderer.gameObject.name + " is part of static batch, skipping lightmap offset and scale.");
                //}
            }
        }
        catch (Exception e)
        {
            if(verbose && Application.isEditor)
                Debug.LogError("Error in ApplyRendererInfo:" + e.GetType().ToString());
        }
    }

    public void LoadLightProbes(LightingScenarioData lightingData)
    {
        try
        {
            LightmapSettings.lightProbes.bakedProbes = DeserializeLightProbes(lightingData);
        }
        catch
        {
            if(verbose && Application.isEditor)
                Debug.LogWarning("Warning, error when trying to load lightprobes for scenario " + lightingData.sceneName);
        }
    }

    public void StoreLightmapInfos(int index)
    {
        var newRendererInfos = new List<RendererInfo>();
        var newLightmapsTextures = new List<Texture2D>();
        var newLightmapsTexturesDir = new List<Texture2D>();
		var newLightmapsMode = LightmapSettings.lightmapsMode;
        var newSphericalHarmonicsList = new List<SphericalHarmonics>();
        var newLightmapsShadowMasks = new List<Texture2D>();

        GenerateLightmapInfo(gameObject, newRendererInfos, newLightmapsTextures, newLightmapsTexturesDir, newLightmapsShadowMasks, newLightmapsMode);

		var scene_LightProbes = new SphericalHarmonicsL2[LightmapSettings.lightProbes.bakedProbes.Length];
		scene_LightProbes = LightmapSettings.lightProbes.bakedProbes;

        for (int i = 0; i < scene_LightProbes.Length; i++)
        {
            var SHCoeff = new SphericalHarmonics();

            // j is coefficient
            for (int j = 0; j < 3; j++)
            {
                //k is channel ( r g b )
                for (int k = 0; k < 9; k++)
                {
                    SHCoeff.coefficients[j*9+k] = scene_LightProbes[i][j, k];
                }
            }

            newSphericalHarmonicsList.Add(SHCoeff);
        }

        //Save all to the scriptable object
        lightingScenarioDatas[index].sceneName = lightingScenesNames[index];
        lightingScenarioDatas[index].lightmaps = newLightmapsTextures.ToArray();
        if (newLightmapsMode != LightmapsMode.NonDirectional)
            lightingScenarioDatas[index].lightmapsDir = newLightmapsTexturesDir.ToArray();
        lightingScenarioDatas[index].lightmapsMode = newLightmapsMode;
        lightingScenarioDatas[index].rendererInfos = newRendererInfos.ToArray();
        lightingScenarioDatas[index].hasRealtimeLights = latestBuildHasReltimeLights;
        lightingScenarioDatas[index].shadowMasks = newLightmapsShadowMasks.ToArray();
        lightingScenarioDatas[index].lightProbes = newSphericalHarmonicsList.ToArray();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(lightingScenarioDatas[index]);
#endif
    }

    static void GenerateLightmapInfo(GameObject root, List<RendererInfo> newRendererInfos, List<Texture2D> newLightmapsLight, List<Texture2D> newLightmapsDir, List<Texture2D> newLightmapsShadow, LightmapsMode newLightmapsMode)
    {
        Terrain terrain = FindObjectOfType<Terrain>();
        if (terrain != null && terrain.lightmapIndex != -1 && terrain.lightmapIndex != 65534)
        {
            RendererInfo terrainRendererInfo = new RendererInfo();
            terrainRendererInfo.lightmapOffsetScale = terrain.lightmapScaleOffset;

            Texture2D lightmaplight = LightmapSettings.lightmaps[terrain.lightmapIndex].lightmapColor;
            terrainRendererInfo.lightmapIndex = newLightmapsLight.IndexOf(lightmaplight);
            if (terrainRendererInfo.lightmapIndex == -1)
            {
                terrainRendererInfo.lightmapIndex = newLightmapsLight.Count;
                newLightmapsLight.Add(lightmaplight);
            }

            if (newLightmapsMode != LightmapsMode.NonDirectional)
            {
                Texture2D lightmapdir = LightmapSettings.lightmaps[terrain.lightmapIndex].lightmapDir;
                terrainRendererInfo.lightmapIndex = newLightmapsDir.IndexOf(lightmapdir);
                if (terrainRendererInfo.lightmapIndex == -1)
                {
                    terrainRendererInfo.lightmapIndex = newLightmapsDir.Count;
                    newLightmapsDir.Add(lightmapdir);
                }
            }
            if (LightmapSettings.lightmaps[terrain.lightmapIndex].shadowMask != null)
            {
                Texture2D lightmapShadow = LightmapSettings.lightmaps[terrain.lightmapIndex].shadowMask;
                terrainRendererInfo.lightmapIndex = newLightmapsShadow.IndexOf(lightmapShadow);
                if (terrainRendererInfo.lightmapIndex == -1)
                {
                    terrainRendererInfo.lightmapIndex = newLightmapsShadow.Count;
                    newLightmapsShadow.Add(lightmapShadow);
                }
            }
            newRendererInfos.Add(terrainRendererInfo);

            if (Application.isEditor)
                Debug.Log("Terrain lightmap stored in" + terrainRendererInfo.lightmapIndex.ToString());
        }

        var renderers = FindObjectsOfType(typeof(Renderer));

        if (Application.isEditor)
            Debug.Log("stored info for " + renderers.Length + " meshrenderers");

        foreach (Renderer renderer in renderers)
        {
            if (renderer.lightmapIndex != -1 && renderer.lightmapIndex != 65534)
            {
                RendererInfo info = new RendererInfo();
                info.hash = renderer.GetHashCode();
                info.lightmapOffsetScale = renderer.lightmapScaleOffset;

                Texture2D lightmaplight = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapColor;
                info.lightmapIndex = newLightmapsLight.IndexOf(lightmaplight);
                if (info.lightmapIndex == -1)
                {
                    info.lightmapIndex = newLightmapsLight.Count;
                    newLightmapsLight.Add(lightmaplight);
                }

                if (newLightmapsMode != LightmapsMode.NonDirectional)
                {
                    Texture2D lightmapdir = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapDir;
                    info.lightmapIndex = newLightmapsDir.IndexOf(lightmapdir);
                    if (info.lightmapIndex == -1)
                    {
                        info.lightmapIndex = newLightmapsDir.Count;
                        newLightmapsDir.Add(lightmapdir);
                    }
                }
                if (LightmapSettings.lightmaps[renderer.lightmapIndex].shadowMask != null)
                {
                    Texture2D lightmapShadow = LightmapSettings.lightmaps[renderer.lightmapIndex].shadowMask;
                    info.lightmapIndex = newLightmapsShadow.IndexOf(lightmapShadow);
                    if (info.lightmapIndex == -1)
                    {
                        info.lightmapIndex = newLightmapsShadow.Count;
                        newLightmapsShadow.Add(lightmapShadow);
                    }
                }
                newRendererInfos.Add(info);
            }
        }
    }
}

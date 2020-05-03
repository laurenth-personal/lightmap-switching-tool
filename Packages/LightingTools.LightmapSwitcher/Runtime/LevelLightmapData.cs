using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Collections;

[ExecuteInEditMode]
public class LevelLightmapData : MonoBehaviour
{
	[System.Serializable]
	public class RendererInfo
	{
        public int transformHash;
        public int meshHash;
        public string name;
		public int lightmapIndex;
		public Vector4 lightmapScaleOffset;
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

    Dictionary<string, LightingScenarioData> scenarioDictionnary;
    Dictionary<int, RendererInfo> hashRendererPairs;

    //TODO : enable logs only when verbose enabled
    public bool verbose = false;

    public void LoadLightingScenario(string scenarioName)
    {
        if (scenarioDictionnary == null || scenarioDictionnary.Count == 0)
        {
            Debug.Log("No lighting scenario found");
            return;
        }

        if (scenarioName != currentLightingScenario)
        {
            previousLightingScenario = currentLightingScenario == null ? scenarioName : currentLightingScenario;

            currentLightingScenario = scenarioName;

            var lightingData = (LightingScenarioData)ScriptableObject.CreateInstance(typeof(LightingScenarioData));

            //Find the Lighting Scenario Data associated to the scene name.
            bool scenarioFound = scenarioDictionnary.TryGetValue(scenarioName, out lightingData);

            if (!scenarioFound)
                return;

            LightmapSettings.lightmapsMode = lightingData.lightmapsMode;

            if (allowLoadingLightingScenes)
                m_SwitchSceneCoroutine = StartCoroutine(SwitchSceneCoroutine(previousLightingScenario, currentLightingScenario, lightingData));

            var newLightmaps = LoadLightmaps(lightingData);

            if(applyLightmapScaleAndOffset)
            {
                ApplyRendererInfo(lightingData.rendererInfos);
            }

            LightmapSettings.lightmaps = newLightmaps;

            LoadLightProbes(lightingData);
        }
    }

    private void Start()
    {
        FillDictionnary();
    }

    void FillDictionnary()
    {
        scenarioDictionnary = new Dictionary<string, LightingScenarioData>();

        //Gather all Lighting scenario data assets found
        var scenarios = Resources.FindObjectsOfTypeAll<LightingScenarioData>();
        foreach (var scenario in scenarios)
        {
            //Add them to the dictionnary so that one can load a scenario data by knowing the associated scene name.
            scenarioDictionnary.Add(scenario.sceneName, scenario);
        }

        if (verbose && Application.isEditor)
            Debug.Log("Found " + scenarios.Length + " lighting scenarios.");
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
            //Dirty exception for terrain
            Terrain terrain = FindObjectOfType<Terrain>();
            int i = 0;
            if (terrain != null)
            {
                terrain.lightmapIndex = infos[i].lightmapIndex;
                terrain.lightmapScaleOffset = infos[i].lightmapScaleOffset;
                i++;
            }

            hashRendererPairs = new Dictionary<int, RendererInfo>();

            //Fill with lighting scenario to load renderer infos
            foreach (var info in infos)
            {
                hashRendererPairs.Add(info.transformHash, info);
            }

            //Find all renderers
            var renderers = FindObjectsOfType<Renderer>();
            
            //Apply stored scale and offset if transform and mesh hashes match
            foreach (var render in renderers)
            {
                var infoToApply = new RendererInfo();

                if (hashRendererPairs.TryGetValue(render.gameObject.transform.GetHashCode(),out infoToApply))
                {
                    if(render.GetComponent<MeshFilter>().GetHashCode() == infoToApply.meshHash)
                    {
                        render.lightmapIndex = infoToApply.lightmapIndex;
                        render.lightmapScaleOffset = infoToApply.lightmapScaleOffset;
                    }
                }
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
            LightmapSettings.lightProbes.bakedProbes = lightingData.lightProbes;
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
        var newLightmapsShadowMasks = new List<Texture2D>();

        GenerateLightmapInfo(gameObject, newRendererInfos, newLightmapsTextures, newLightmapsTexturesDir, newLightmapsShadowMasks, newLightmapsMode);

        //Save all to the scriptable object
        lightingScenarioDatas[index].sceneName = lightingScenesNames[index];
        lightingScenarioDatas[index].lightmaps = newLightmapsTextures.ToArray();
        if (newLightmapsMode != LightmapsMode.NonDirectional)
            lightingScenarioDatas[index].lightmapsDir = newLightmapsTexturesDir.ToArray();
        lightingScenarioDatas[index].lightmapsMode = newLightmapsMode;
        lightingScenarioDatas[index].rendererInfos = newRendererInfos.ToArray();
        lightingScenarioDatas[index].hasRealtimeLights = latestBuildHasReltimeLights;
        lightingScenarioDatas[index].shadowMasks = newLightmapsShadowMasks.ToArray();
        lightingScenarioDatas[index].lightProbes = LightmapSettings.lightProbes.bakedProbes;

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
            terrainRendererInfo.lightmapScaleOffset = terrain.lightmapScaleOffset;

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
                info.transformHash = renderer.gameObject.transform.GetHashCode();
                info.meshHash = renderer.gameObject.GetComponent<MeshFilter>().GetHashCode();
                info.name = renderer.gameObject.name;
                info.lightmapScaleOffset = renderer.lightmapScaleOffset;

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

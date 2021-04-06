using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Collections;

[ExecuteInEditMode]
public class LightingSwitcher : MonoBehaviour
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

    public bool allowLoadingLightingScenes = true;
    [Tooltip("Enable this if you want to use different lightmap resolutions in your different lighting scenarios. In that case you'll have to disable Static Batching in the Player Settings. When disabled, Static Batching can be used but all your lighting scenarios need to use the same lightmap resolution.")]
    public bool applyLightmapScaleAndOffset = true;

    private string currentLightingSceneName;
    private string previousLightingSceneName;

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

        if (scenarioName != currentLightingSceneName)
        {
            //Find the Lighting Scenario Data associated to the scene name.
            if(!scenarioDictionnary.ContainsKey(scenarioName))
            {
                Debug.Log("Lighting switcher - scenario " + scenarioName+ " not found");
                return;
            }
            var lightingData = (LightingScenarioData)ScriptableObject.CreateInstance(typeof(LightingScenarioData));
            bool scenarioFound = scenarioDictionnary.TryGetValue(scenarioName, out lightingData);
            if (!scenarioFound)
                return;

            //Find the lighting scene name
            previousLightingSceneName = currentLightingSceneName == null ? lightingData.lightingSceneName : currentLightingSceneName;
            currentLightingSceneName = lightingData.lightingSceneName;

            LightmapSettings.lightmapsMode = lightingData.lightmapsMode;

            if (allowLoadingLightingScenes)
                m_SwitchSceneCoroutine = StartCoroutine(SwitchSceneCoroutine(previousLightingSceneName, lightingData.hasRealtimeLights ? currentLightingSceneName : null ));

            if(applyLightmapScaleAndOffset)
            {
                ApplyRendererInfo(lightingData.rendererInfos);
            }

            LightmapSettings.lightmaps = LoadLightmaps(lightingData);

            LoadLightProbes(lightingData);
        }
    }

    private void Start()
    {
        //On Start load all lighting scenarios in resource folder
        Resources.LoadAll<LightingScenarioData>("");
        FindLightingScenarios();
    }

    public void FindLightingScenarios()
    {
        scenarioDictionnary = new Dictionary<string, LightingScenarioData>();

        //Gather all Lighting scenario data assets found
        var scenarios = Resources.FindObjectsOfTypeAll<LightingScenarioData>();

        //var scenarios = Resources.FindObjectsOfTypeAll<LightingScenarioData>();
        foreach (var scenario in scenarios)
        {
            //Add them to the dictionnary so that one can load a scenario data by knowing the associated scene name.
            if(scenario.name != null  && scenario.name != string.Empty)
                scenarioDictionnary.Add(scenario.name, scenario);
        }

        if (verbose && Application.isEditor)
            Debug.Log("Found " + scenarioDictionnary.Count + " lighting scenarios.");
    }

    IEnumerator SwitchSceneCoroutine(string sceneToUnload, string sceneToLoad)
    {
        AsyncOperation unloadop = null;
        AsyncOperation loadop = null;

        if (sceneToUnload != null && sceneToUnload != string.Empty && sceneToUnload != sceneToLoad)
        {
            if (SceneManager.GetSceneByName(sceneToUnload).name != null)
            {
                unloadop = SceneManager.UnloadSceneAsync(sceneToUnload);
                while (!unloadop.isDone)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        if(sceneToLoad != null && sceneToLoad != string.Empty && sceneToLoad != "" )
        {
            loadop = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
            while ((!loadop.isDone || loadop == null))
            {
                yield return new WaitForEndOfFrame();
            }   
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad));
        }
    }

    LightmapData[] LoadLightmaps(LightingScenarioData lightingData)
    {
        if (lightingData.lightmaps == null
                || lightingData.lightmaps.Length == 0)
        {
            if (verbose && Application.isEditor)
                Debug.LogWarning("No lightmaps stored in scenario " + lightingData.name);
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
            //TODO : find better way to handle terrain. This doesn't support multiple terrains.
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

                //int transformHash = render.gameObject.transform.position

                if (hashRendererPairs.TryGetValue(GetStableHash(render.gameObject.transform),out infoToApply))
                {
                    if(render.gameObject.name == infoToApply.name)
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

    //Too much precision makes hash unrealiable because transforms often change very slightly..
    public static int GetStableHash(Transform transform)
    {
        Vector3 stablePos = new Vector3(LimitDecimals(transform.position.x,2), LimitDecimals(transform.position.y,2), LimitDecimals(transform.position.z,2));
        Vector3 stableRot = new Vector3(LimitDecimals(transform.rotation.x,1), LimitDecimals(transform.rotation.y,1), LimitDecimals(transform.rotation.z,1));
        return stablePos.GetHashCode() + stableRot.GetHashCode();
    }

    static float LimitDecimals(float input, int decimalcount)
    {
        var multiplier = Mathf.Pow(10, decimalcount);
        return Mathf.Floor(input * multiplier) / multiplier;
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
                Debug.LogWarning("Warning, error when trying to load lightprobes for scenario " + lightingData.name);
        }
    }
}

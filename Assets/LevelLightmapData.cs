using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;

public class LevelLightmapData : MonoBehaviour
{
    [System.Serializable]
    public struct RendererInfo
    {
        public Renderer renderer;
        public int lightmapIndex;
        public Vector4 lightmapOffsetScale;
    }
    [System.Serializable]
    public struct T2DArrayWrapper
    {
        public Texture2D[] myT2DArray;

        public int Length { get { return myT2DArray.Length; } }

        public Texture2D this[int i]
        {
            get
            {
                return myT2DArray[i];
            }
            set
            {
                myT2DArray[i] = value;
            }
        }
    }
    [System.Serializable]
    public struct RIArrayWrapper
    {
        public RendererInfo[] myRIArray;

        public int Length { get { return myRIArray.Length; } }

        public RendererInfo this[int i]
        {
            get
            {
                return myRIArray[i];
            }
            set
            {
                myRIArray[i] = value;
            }
        }
    }
    [SerializeField]
    List<RIArrayWrapper> m_RendererInfos;
    [SerializeField]
    List<T2DArrayWrapper> m_Lightmaps;
    [SerializeField]
    List<T2DArrayWrapper> m_Lightmapsdir;
    [SerializeField]
    List<LightmapsMode> m_LightmapsModes;

    [SerializeField]
    public string[] LightingScenarios;
    [SerializeField]
    public int DefaultLightingScenario;

    void Awake()
    {
        LoadLightingScenario(DefaultLightingScenario);
        Debug.Log("Load default lighting scenario");
        EditorApplication.playmodeStateChanged += PlaymodeCallback;
    }

    //Need to load some lightmaps after going back to editor because lightingDataAsset is set to null after storage phase
    //in order to avoid erasing lightmaps during later bakes
    void PlaymodeCallback()
    {
        if (!Application.isPlaying && Lightmapping.lightingDataAsset == null)
        {
            LoadLightingScenario(DefaultLightingScenario);
            Debug.Log("Load default lighting scenario");
        }
    }

    public void LoadLightingScenario(int index)
    {
        if (m_RendererInfos[index].Length == 0 || m_RendererInfos[index].myRIArray == null)
        {
            Debug.Log("empty lighting scenario");
            return;
        }

        LightmapSettings.lightmapsMode = m_LightmapsModes[index];

        var newLightmaps = new LightmapData[m_Lightmaps[index].Length];

        for (int i = 0; i < m_Lightmaps[index].Length; i++)
        {
            newLightmaps[i] = new LightmapData();
            newLightmaps[i].lightmapLight = m_Lightmaps[index].myT2DArray[i];

            if (m_LightmapsModes[index] != LightmapsMode.NonDirectional)
            {
                newLightmaps[i].lightmapDir = m_Lightmapsdir[index].myT2DArray[i];
            }
        }

        ApplyRendererInfo(m_RendererInfos[index].myRIArray);

        LightmapSettings.lightmaps = newLightmaps;
    }

    static void ApplyRendererInfo (RendererInfo[] infos)
	{
		for (int i=0;i<infos.Length;i++)
		{
			var info = infos[i];
			info.renderer.lightmapIndex = info.lightmapIndex;
			info.renderer.lightmapScaleOffset = info.lightmapOffsetScale;
		}
	}

#if UNITY_EDITOR

    public void StoreLightmapInfos(int index)
    {
        Debug.Log("Storing lightmaps for index " + index);
        if (UnityEditor.Lightmapping.giWorkflowMode != UnityEditor.Lightmapping.GIWorkflowMode.OnDemand)
        {
            Debug.LogError("ExtractLightmapData requires that you have baked you lightmaps and Auto mode is disabled.");
            return;
        }

        LevelLightmapData lightmapdata = FindObjectOfType<LevelLightmapData>();

        var rendererInfos = new List<RendererInfo>();
        var lightmaps = new List<Texture2D>();
        var lightmapsdir = new List<Texture2D>();

        if ( m_LightmapsModes.Count <= index )
        {
            m_LightmapsModes.Insert(index, LightmapSettings.lightmapsMode);
        }
        else
        {
            m_LightmapsModes[index] = LightmapSettings.lightmapsMode;
        }

        GenerateLightmapInfo(gameObject, rendererInfos, lightmaps, lightmapsdir, m_LightmapsModes[index]);
        
        var LMWrapper = new T2DArrayWrapper();
        LMWrapper.myT2DArray = lightmaps.ToArray();
        if ( lightmapdata.m_Lightmaps.Count <= index )
        {
            lightmapdata.m_Lightmaps.Insert(index, LMWrapper);
        }
        else
        {
            lightmapdata.m_Lightmaps[index] = LMWrapper;
        }


        Debug.Log("Lightmapslight count for index : " + m_Lightmaps[index].Length);

        if (m_LightmapsModes[index] != LightmapsMode.NonDirectional)
        {
            var LMDWrapper = new T2DArrayWrapper();
            LMDWrapper.myT2DArray = lightmapsdir.ToArray();
            if (lightmapdata.m_Lightmapsdir.Count <= index )
            {
                lightmapdata.m_Lightmapsdir.Insert(index, LMDWrapper);
            }
            else
            {
                lightmapdata.m_Lightmapsdir[index] = LMWrapper;
            }
        }

        var RIWrapper = new RIArrayWrapper();
        RIWrapper.myRIArray = rendererInfos.ToArray();
        if (lightmapdata.m_RendererInfos.Count <= index )
        {
            lightmapdata.m_RendererInfos.Insert(index, RIWrapper);
        }
        else
        {
            lightmapdata.m_RendererInfos[index] = RIWrapper;
        }

        Debug.Log(index);

        Lightmapping.lightingDataAsset = null;
    }

    static void GenerateLightmapInfo (GameObject root, List<RendererInfo> rendererInfos, List<Texture2D> lightmapslight, List<Texture2D> lightmapsdir, LightmapsMode lightmapmode )
	{
		var renderers = FindObjectsOfType(typeof(MeshRenderer));
        Debug.Log("stored info for "+renderers.Length+" meshrenderers");
        foreach (MeshRenderer renderer in renderers)
		{
			if (renderer.lightmapIndex != -1)
			{
				RendererInfo info = new RendererInfo();
				info.renderer = renderer;
				info.lightmapOffsetScale = renderer.lightmapScaleOffset;

				Texture2D lightmaplight = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapLight;
                info.lightmapIndex = lightmapslight.IndexOf(lightmaplight);
                if (info.lightmapIndex == -1)
				{
					info.lightmapIndex = lightmapslight.Count;
                    lightmapslight.Add(lightmaplight);
				}

                if (lightmapmode != LightmapsMode.NonDirectional)
                {
                    Texture2D lightmapdir = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapDir;
                    info.lightmapIndex = lightmapsdir.IndexOf(lightmapdir);
                    if (info.lightmapIndex == -1)
                    {
                        info.lightmapIndex = lightmapsdir.Count;
                        lightmapsdir.Add(lightmapdir);
                    }
                }
                rendererInfos.Add(info);
			}
		}
	}

    public void BuildLightingScenario(string ScenarioName)
    {
        Debug.Log("Baking"+ScenarioName);
        EditorSceneManager.OpenScene("Assets/Scenes/" + ScenarioName + ".unity", OpenSceneMode.Additive);
        EditorSceneManager.SetActiveScene(EditorSceneManager.GetSceneByPath("Assets/Scenes/" + ScenarioName + ".unity"));
        var newLightmapMode = new LightmapsMode();
        newLightmapMode = LightmapSettings.lightmapsMode;
        UnityEditor.Lightmapping.Bake();
        EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByPath("Assets/Scenes/" + ScenarioName + ".unity"), true);
        LightmapSettings.lightmapsMode = newLightmapMode;
    }
#endif

}
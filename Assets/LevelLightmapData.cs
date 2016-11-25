using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class LevelLightmapData : MonoBehaviour
{
    [System.Serializable]
    public class SerializedSH
    {
        public float[] SHcoefficients = new float[27];
    }
    [System.Serializable]
    public class SerializedSHWrapper
    {
        public SerializedSH[] mySerializedSHArray;

        public int Length { get { return mySerializedSHArray.Length; } }

        public SerializedSH this[int i]
        {
            get
            {
                return mySerializedSHArray[i];
            }
            set
            {
                mySerializedSHArray[i] = value;
            }
        }
    }
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
    List<SerializedSHWrapper> m_LightProbes;

    [SerializeField]
    public string[] LightingScenarios;
    

    public void LoadLightingScenario(int index)
    {
        if (m_RendererInfos[index].Length == 0 || m_RendererInfos[index].myRIArray == null)
        {
            Debug.Log("empty lighting scenario");
            return;
        }

        LightmapSettings.lightmapsMode = m_LightmapsModes[index];

        var newLightmaps = new LightmapData[m_Lightmaps[index].myT2DArray.Length];

        for (int i = 0; i < newLightmaps.Length; i++)
        {
            newLightmaps[i] = new LightmapData();
            newLightmaps[i].lightmapLight = m_Lightmaps[index].myT2DArray[i];

            if (m_LightmapsModes[index] != LightmapsMode.NonDirectional)
            {
                newLightmaps[i].lightmapDir = m_Lightmapsdir[index].myT2DArray[i];
            }
        }

        LoadLightProbes(index);

        ApplyRendererInfo(m_RendererInfos[index].myRIArray);

        LightmapSettings.lightmaps = newLightmaps;
    }

    public void ApplyRendererInfo (RendererInfo[] infos)
	{
		for (int i=0;i<infos.Length;i++)
		{
			var info = infos[i];
			info.renderer.lightmapIndex = info.lightmapIndex;
			info.renderer.lightmapScaleOffset = info.lightmapOffsetScale;
		}
	}

    public void LoadLightProbes(int index)
    {
        var SHArray = new SphericalHarmonicsL2[m_LightProbes[index].mySerializedSHArray.Length];

        for (int i = 0; i < m_LightProbes[index].mySerializedSHArray.Length; i++)
        {
            var SH = new SphericalHarmonicsL2();

            // j is coefficient
            for (int j = 0; j < 3; j++)
            {
                //k is channel ( r g b )
                for (int k = 0; k < 9; k++)
                {
                    SH[j, k] = m_LightProbes[index].mySerializedSHArray[i].SHcoefficients[j * 9 + k];
                }
            }

            SHArray[i] = SH;
        }

        LightmapSettings.lightProbes.bakedProbes = SHArray;
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

        //LevelLightmapData lightmapdata = FindObjectOfType<LevelLightmapData>();

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
        if ( m_Lightmaps.Count <= index )
        {
            m_Lightmaps.Insert(index, LMWrapper);
            m_Lightmapsdir.Insert(index, new T2DArrayWrapper());
        }
        else
        {
            m_Lightmaps[index] = LMWrapper;
        }

        Debug.Log("Lightmapslight count for index : " + m_Lightmaps[index].Length);

        if (m_LightmapsModes[index] != LightmapsMode.NonDirectional)
        {
            var LMDWrapper = new T2DArrayWrapper();
            LMDWrapper.myT2DArray = lightmapsdir.ToArray();
            m_Lightmapsdir[index] = LMDWrapper;
            Debug.Log("Lightmapsdir count for index : " + m_Lightmapsdir[index].Length);
        }

        var RIWrapper = new RIArrayWrapper();
        RIWrapper.myRIArray = rendererInfos.ToArray();
        if (m_RendererInfos.Count <= index )
        {
            m_RendererInfos.Insert(index, RIWrapper);
        }
        else
        {
            m_RendererInfos[index] = RIWrapper;
        }

        var scene_LightProbes = new SphericalHarmonicsL2[LightmapSettings.lightProbes.bakedProbes.Length];
        scene_LightProbes = LightmapSettings.lightProbes.bakedProbes;

        var SHCoeffList = new List<SerializedSH>();

        for (int i = 0; i < scene_LightProbes.Length; i++)
        {
            var SHCoeff = new SerializedSH();

            // j is coefficient
            for (int j = 0; j < 3; j++)
            {
                //k is channel ( r g b )
                for (int k = 0; k < 9; k++)
                {
                    SHCoeff.SHcoefficients[j*9+k] = scene_LightProbes[i][j, k];
                }
            }

            SHCoeffList.Add(SHCoeff);
        }
        if (m_LightProbes.Count <= index)
        {
            m_LightProbes.Insert(index, new SerializedSHWrapper());
        }
        m_LightProbes[index].mySerializedSHArray = SHCoeffList.ToArray();

        Debug.Log(index);
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
        //Remove reference to LightingDataAsset so that Unity doesn't delete the previous bake
        Lightmapping.lightingDataAsset = null;

        Debug.Log("Baking"+ScenarioName);
        EditorSceneManager.OpenScene("Assets/Scenes/" + ScenarioName + ".unity", OpenSceneMode.Additive);
        EditorSceneManager.SetActiveScene(EditorSceneManager.GetSceneByPath("Assets/Scenes/" + ScenarioName + ".unity"));
        var newLightmapMode = new LightmapsMode();
        newLightmapMode = LightmapSettings.lightmapsMode;
        UnityEditor.Lightmapping.Bake();
        EditorSceneManager.SaveScene(EditorSceneManager.GetSceneByPath("Assets/Scenes/" + ScenarioName + ".unity"));
        EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByPath("Assets/Scenes/" + ScenarioName + ".unity"), true);
        LightmapSettings.lightmapsMode = newLightmapMode;
    }
#endif

}
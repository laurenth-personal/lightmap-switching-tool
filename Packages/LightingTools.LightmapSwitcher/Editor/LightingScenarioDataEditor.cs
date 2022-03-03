using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using System.Collections.Generic;

[CustomEditor(typeof(LightingScenarioData))]
public class LightingScenarioEditor : Editor
{

    public SerializedProperty geometrySceneName;
    public SerializedProperty lightingSceneName;
    public SerializedProperty storeRendererInfos;
    public SerializedProperty lightmapsMode;
    public SerializedProperty lightmaps;
    public SerializedProperty lightProbes;
    public SerializedProperty rendererInfos;
    public SerializedProperty hasRealtimeLights;

    public void OnEnable()
    {
        geometrySceneName = serializedObject.FindProperty("geometrySceneName");
        lightingSceneName = serializedObject.FindProperty("lightingSceneName");
        storeRendererInfos = serializedObject.FindProperty("storeRendererInfos");
        lightmapsMode = serializedObject.FindProperty("lightmapsMode");
        lightmaps = serializedObject.FindProperty("lightmaps");
        lightProbes = serializedObject.FindProperty("lightProbes");
        rendererInfos = serializedObject.FindProperty("rendererInfos");
        hasRealtimeLights = serializedObject.FindProperty("hasRealtimeLights");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(geometrySceneName);
        EditorGUILayout.PropertyField(lightingSceneName);
        EditorGUILayout.PropertyField(storeRendererInfos);
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Stored Data", EditorStyles.boldLabel);
        //Begin disabled group as this is a data summary display
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(lightmapsMode);
        EditorGUILayout.TextField("Lightmaps count", lightmaps.arraySize.ToString());
        EditorGUILayout.TextField("Light probes count", lightProbes.arraySize.ToString());
        EditorGUILayout.TextField("Renderer Infos count", rendererInfos.arraySize.ToString());
        EditorGUILayout.PropertyField(hasRealtimeLights);

        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Generate lighting scenario data"))
        {
            if (UnityEditor.Lightmapping.giWorkflowMode != UnityEditor.Lightmapping.GIWorkflowMode.OnDemand)
            {
                Debug.LogError("Lightmap switcher requires Auto Generate lighting mode disabled.");
            }
            else
                BuildLightingScenario();
        }

        serializedObject.ApplyModifiedProperties();
    }

    public void BuildLightingScenario()
    {

        LightingScenarioData scenarioData = (LightingScenarioData)target;

        Debug.Log("Loading scenario " + scenarioData.name);

        string lightingSceneGUID = AssetDatabase.FindAssets(scenarioData.lightingSceneName)[0];
        string lightingScenePath = AssetDatabase.GUIDToAssetPath(lightingSceneGUID);
        if (!lightingScenePath.EndsWith(".unity"))
            lightingScenePath = lightingScenePath + ".unity";

        string geometrySceneGUID = AssetDatabase.FindAssets(scenarioData.geometrySceneName)[0];
        string geometryScenePath = AssetDatabase.GUIDToAssetPath(geometrySceneGUID);
        if (!geometryScenePath.EndsWith(".unity"))
            geometryScenePath = geometryScenePath + ".unity";

        EditorSceneManager.OpenScene(geometryScenePath);
        EditorSceneManager.OpenScene(lightingScenePath, OpenSceneMode.Additive);

        //Remove reference to LightingDataAsset so that Unity doesn't delete the previous bake
        Lightmapping.lightingDataAsset = null;
        Scene lightingScene = SceneManager.GetSceneByName(scenarioData.lightingSceneName);
        Scene geometryScene = SceneManager.GetSceneByName(scenarioData.geometrySceneName);
        EditorSceneManager.SetActiveScene(lightingScene);

        //Check if the lighting scene needs requires dynamic lighting ( if not, never try to load the lighting scene ).
        scenarioData.hasRealtimeLights = SearchLightsNeededRealtime();

        Debug.Log("Lightmap switcher - Start baking");
        EditorCoroutineUtility.StartCoroutine(BuildLightingAsync(lightingScene, geometryScene), this);
    }

    private IEnumerator BuildLightingAsync(Scene lightingScene, Scene geometryScene)
    {
        Lightmapping.BakeAsync();
        while (Lightmapping.isRunning) { yield return null; }
        EditorSceneManager.SaveScene(geometryScene);
        EditorSceneManager.SaveScene(lightingScene);
        StoreLightingData();
        EditorSceneManager.CloseScene(lightingScene, true);
        AssetDatabase.SaveAssets();
    }

    public bool SearchLightsNeededRealtime()
    {
        bool latestBuildHasRealtimeLights = false;

        var lights = FindObjectsOfType<Light>();
        var reflectionProbes = FindObjectsOfType<ReflectionProbe>();

        foreach (Light light in lights)
        {
            if (light.lightmapBakeType == LightmapBakeType.Mixed || light.lightmapBakeType == LightmapBakeType.Realtime)
                latestBuildHasRealtimeLights = true;
        }
        if (reflectionProbes.Length > 0)
            latestBuildHasRealtimeLights = true;

        return latestBuildHasRealtimeLights;
    }

    public void StoreLightingData()
    {
        LightingScenarioData scenarioData = (LightingScenarioData)target;

        GenerateLightingData(scenarioData);
        scenarioData.lightProbes = LightmapSettings.lightProbes.bakedProbes;
        EditorUtility.SetDirty(scenarioData);
        AssetDatabase.SaveAssets();
    }

    static void GenerateLightingData(LightingScenarioData data)
    {
        var newRendererInfos = new List<LevelLightmapData.RendererInfo>();
        var newLightmapsLight = new List<Texture2D>();
        var newLightmapsDir = new List<Texture2D>();
        var newLightmapsShadow = new List<Texture2D>();

        data.lightmapsMode = LightmapSettings.lightmapsMode;

        //TODO : Fin better solution for terrain. This is not compatible with several terrains.
        Terrain terrain = FindObjectOfType<Terrain>();
        if (terrain != null && terrain.lightmapIndex != -1 && terrain.lightmapIndex != 65534)
        {
            LevelLightmapData.RendererInfo terrainRendererInfo = new LevelLightmapData.RendererInfo();
            terrainRendererInfo.lightmapScaleOffset = terrain.lightmapScaleOffset;

            Texture2D lightmaplight = LightmapSettings.lightmaps[terrain.lightmapIndex].lightmapColor;
            terrainRendererInfo.lightmapIndex = newLightmapsLight.IndexOf(lightmaplight);
            if (terrainRendererInfo.lightmapIndex == -1)
            {
                terrainRendererInfo.lightmapIndex = newLightmapsLight.Count;
                newLightmapsLight.Add(lightmaplight);
            }

            if (data.lightmapsMode != LightmapsMode.NonDirectional)
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
            if (data.storeRendererInfos)
            {
                newRendererInfos.Add(terrainRendererInfo);
                if (Application.isEditor)
                    Debug.Log("Terrain lightmap stored in" + terrainRendererInfo.lightmapIndex.ToString());
            }

        }

        var renderers = FindObjectsOfType(typeof(Renderer));

        foreach (Renderer renderer in renderers)
        {
            if (renderer.lightmapIndex != -1 && renderer.lightmapIndex != 65534)
            {
                LevelLightmapData.RendererInfo info = new LevelLightmapData.RendererInfo();
                info.transformHash = LevelLightmapData.GetStableHash(renderer.gameObject.transform);
                info.meshHash = renderer.gameObject.GetComponent<MeshFilter>().sharedMesh.vertexCount;
                info.name = renderer.gameObject.name;
                info.lightmapScaleOffset = renderer.lightmapScaleOffset;

                Texture2D lightmaplight = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapColor;
                info.lightmapIndex = newLightmapsLight.IndexOf(lightmaplight);
                if (info.lightmapIndex == -1)
                {
                    info.lightmapIndex = newLightmapsLight.Count;
                    newLightmapsLight.Add(lightmaplight);
                }

                if (data.lightmapsMode != LightmapsMode.NonDirectional)
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
                if (data.storeRendererInfos)
                {
                    newRendererInfos.Add(info);
                    if (Application.isEditor)
                        Debug.Log("stored info for " + renderers.Length + " meshrenderers");
                }
            }
        }
        data.lightmaps = newLightmapsLight.ToArray();
        data.lightmapsDir = newLightmapsDir.ToArray();
        data.shadowMasks = newLightmapsShadow.ToArray();
        data.rendererInfos = newRendererInfos.ToArray();
    }
}
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
    /*
    public SerializedProperty lightingScenariosScenes;
    public SerializedProperty lightingScenesNames;
    public SerializedProperty allowLoadingLightingScenes;
    public SerializedProperty applyLightmapScaleAndOffset;

    GUIContent allowLoading = new GUIContent("Allow loading Lighting Scenes", "Allow the Level Lightmap Data script to load a lighting scene additively at runtime if the lighting scenario contains realtime lights.");
    */

    public void OnEnable()
    {
        //lightingScenariosScenes = serializedObject.FindProperty("lightingScenariosScenes");
        //lightingScenesNames = serializedObject.FindProperty("lightingScenesNames");
        //allowLoadingLightingScenes = serializedObject.FindProperty("allowLoadingLightingScenes");
        //applyLightmapScaleAndOffset = serializedObject.FindProperty("applyLightmapScaleAndOffset");
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginDisabledGroup(true);
        base.DrawDefaultInspector();
        EditorGUI.EndDisabledGroup();
        /*
        serializedObject.Update();
        LevelLightmapData lightmapData = (LevelLightmapData)target;

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(lightingScenariosScenes, new GUIContent("Lighting Scenarios Scenes"), includeChildren: true);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            lightingScenesNames.arraySize = lightingScenariosScenes.arraySize;

            for (int i = 0; i < lightingScenariosScenes.arraySize; i++)
            {
                lightingScenesNames.GetArrayElementAtIndex(i).stringValue = lightingScenariosScenes.GetArrayElementAtIndex(i).objectReferenceValue == null ? "" : lightingScenariosScenes.GetArrayElementAtIndex(i).objectReferenceValue.name;
            }
            serializedObject.ApplyModifiedProperties();
        }
        EditorGUILayout.PropertyField(allowLoadingLightingScenes, allowLoading);
        EditorGUILayout.PropertyField(applyLightmapScaleAndOffset);

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();

        for (int i = 0; i < lightmapData.lightingScenariosScenes.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            if (lightmapData.lightingScenariosScenes[i] != null)
            {
                EditorGUILayout.LabelField(lightmapData.lightingScenariosScenes[i].name.ToString(), EditorStyles.boldLabel);
                if (GUILayout.Button("Build "))
                {
                    if (UnityEditor.Lightmapping.giWorkflowMode != UnityEditor.Lightmapping.GIWorkflowMode.OnDemand)
                    {
                        Debug.LogError("ExtractLightmapData requires that you have baked you lightmaps and Auto mode is disabled.");
                    }
                    else
                        BuildLightingScenario(i, lightmapData);
                }
                if (GUILayout.Button("Store "))
                {
                    lightmapData.StoreLightmapInfos(i);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        */

        if (GUILayout.Button("Build "))
        {
            if (UnityEditor.Lightmapping.giWorkflowMode != UnityEditor.Lightmapping.GIWorkflowMode.OnDemand)
            {
                Debug.LogError("Lightmap switcher requires Auto Generate lighting mode disabled.");
            }
            else
                BuildLightingScenario();
        }
        if (GUILayout.Button("Store "))
        {
            StoreLightmapInfos();
        }
    }

    public void BuildLightingScenario()
    {
        //Remove reference to LightingDataAsset so that Unity doesn't delete the previous bake
        Lightmapping.lightingDataAsset = null;

        //
        LightingScenarioData scenarioData = (LightingScenarioData)target;


        //string currentBuildScenename = lightingScenariosScenes.GetArrayElementAtIndex(ScenarioID).objectReferenceValue.name;

        Debug.Log("Loading " + scenarioData.sceneName);

        string lightingSceneGUID = AssetDatabase.FindAssets(scenarioData.sceneName)[0];
        string lightingScenePath = AssetDatabase.GUIDToAssetPath(lightingSceneGUID);
        if (!lightingScenePath.EndsWith(".unity"))
            lightingScenePath = lightingScenePath + ".unity";

        EditorSceneManager.OpenScene(lightingScenePath, OpenSceneMode.Additive);

        Scene lightingScene = SceneManager.GetSceneByName(scenarioData.sceneName);
        EditorSceneManager.SetActiveScene(lightingScene);

        SearchLightsNeededRealtime(scenarioData);

        Debug.Log("Lightmap switcher - Start baking");
        EditorCoroutineUtility.StartCoroutine(BuildLightingAsync(lightingScene), this);
    }

    private IEnumerator BuildLightingAsync(Scene lightingScene)
    {
        Lightmapping.BakeAsync();
        while (Lightmapping.isRunning) { yield return null; }
        EditorSceneManager.SaveScene(lightingScene);
        EditorSceneManager.CloseScene(lightingScene, true);
    }

    public void SearchLightsNeededRealtime(LightingScenarioData data)
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

        data.hasRealtimeLights = latestBuildHasRealtimeLights;
    }

    public void StoreLightmapInfos()
    {
        LightingScenarioData scenarioData = (LightingScenarioData)target;

        var newRendererInfos = new List<LevelLightmapData.RendererInfo>();
        var newLightmapsTextures = new List<Texture2D>();
        var newLightmapsTexturesDir = new List<Texture2D>();
        var newLightmapsMode = new LightmapsMode();
        var newLightmapsShadowMasks = new List<Texture2D>();

        newLightmapsMode = LightmapSettings.lightmapsMode;

        GenerateLightmapInfo(newRendererInfos, newLightmapsTextures, newLightmapsTexturesDir, newLightmapsShadowMasks, newLightmapsMode);

        scenarioData.lightmapsMode = newLightmapsMode;

        scenarioData.lightmaps = newLightmapsTextures.ToArray();

        if (newLightmapsMode != LightmapsMode.NonDirectional)
        {
            scenarioData.lightmapsDir = newLightmapsTexturesDir.ToArray();
        }

        //Mixed or realtime support
        //scenarioData.hasRealtimeLights = latestBuildHasReltimeLights;

        scenarioData.shadowMasks = newLightmapsShadowMasks.ToArray();

        scenarioData.rendererInfos = newRendererInfos.ToArray();

        scenarioData.lightProbes = LightmapSettings.lightProbes.bakedProbes;

        AssetDatabase.SaveAssets();

    }

    static void GenerateLightmapInfo(List<LevelLightmapData.RendererInfo> newRendererInfos, List<Texture2D> newLightmapsLight, List<Texture2D> newLightmapsDir, List<Texture2D> newLightmapsShadow, LightmapsMode newLightmapsMode)
    {
        //TODO : Fin better solution for terrain. This is not compatible with several terrains.
        Terrain terrain = FindObjectOfType<Terrain>();
        if (terrain != null && terrain.lightmapIndex != -1 && terrain.lightmapIndex != 65534)
        {
            LevelLightmapData.RendererInfo terrainRendererInfo = new LevelLightmapData.RendererInfo();
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
                LevelLightmapData.RendererInfo info = new LevelLightmapData.RendererInfo();
                info.renderer = renderer;
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
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEngine.UIElements;

[CustomEditor(typeof(LevelLightmapData))]
public class LevelLightmapDataEditor : Editor
{
    public SerializedProperty lightingScenariosScenes;
    public SerializedProperty lightingScenariosData;
    public SerializedProperty allowLoadingLightingScenes;
    public SerializedProperty applyLightmapScaleAndOffset;
    public bool usev2;
    LevelLightmapData lightmapData;
    GUIContent allowLoading = new GUIContent("Allow loading Lighting Scenes", "Allow the Level Lightmap Data script to load a lighting scene additively at runtime if the lighting scenario contains realtime lights.");

    private Editor[] _editors;

    public void OnEnable()
    {
        lightmapData = target as LevelLightmapData;
        lightingScenariosScenes = serializedObject.FindProperty("lightingScenariosScenes");
        lightingScenariosData = serializedObject.FindProperty("lightingScenariosData");
        allowLoadingLightingScenes = serializedObject.FindProperty("allowLoadingLightingScenes");
        applyLightmapScaleAndOffset = serializedObject.FindProperty("applyLightmapScaleAndOffset");
        _editors = new Editor[lightingScenariosData.arraySize];
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        //Temp switch between old and new mode
        usev2 = EditorGUILayout.Toggle(new GUIContent("Show scenario data"), usev2);
        EditorGUILayout.PropertyField(allowLoadingLightingScenes, allowLoading);
        if (!usev2)
            EditorGUILayout.PropertyField(applyLightmapScaleAndOffset);
        if(usev2)
        {
            EditorGUILayout.PropertyField(lightingScenariosData, new GUIContent("Lighting Scenarios"), includeChildren: true);
        }
        else
        {
            EditorGUILayout.PropertyField(lightingScenariosScenes, new GUIContent("Lighting Scenes"), includeChildren: true);
        }
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }

        if(usev2 )
        {
            EditorGUILayout.Space();
            for (int i = 0; i < _editors.Length; i++)
            {
                var data = lightingScenariosData.GetArrayElementAtIndex(i).objectReferenceValue;
                if (data != null)
                {
                    CreateCachedEditor(data, null, ref _editors[i]);
                    EditorGUILayout.LabelField(data.name, EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    _editors[i].OnInspectorGUI();
                    EditorGUI.indentLevel--;
                }
            }
        }

        serializedObject.ApplyModifiedProperties();

        if(!usev2)
        {
            EditorGUILayout.Space();
            if (Event.current.type!= EventType.DragPerform && lightmapData.lightingScenariosScenes != null)
            {
                if (Lightmapping.giWorkflowMode == Lightmapping.GIWorkflowMode.OnDemand)
                {
                    for (int i = 0; i < lightmapData.lightingScenariosScenes.Count; i++)
                    {
                        var scene = lightmapData.lightingScenariosScenes[i];
                        if (scene != null)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(scene.name, EditorStyles.boldLabel);
                                if (GUILayout.Button("Build"))
                                {
                                    {
                                        BuildLightingScenario(scene, i);
                                    }
                                }
                            //if (GUILayout.Button("Store "))
                            //{
                            //    lightmapData.StoreLightmapInfos(i);
                            //}
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
                else
                    EditorGUILayout.LabelField("Disable Auto Generate in the Lighting window.");
            }
        }
    }

    public void BuildLightingScenario(SceneAsset scene, int index)
    {
        //Remove reference to LightingDataAsset so that Unity doesn't delete the previous bake
        Lightmapping.lightingDataAsset = null;

        Debug.Log("Loading " + scene.name);

        string lightingScenePath = AssetDatabase.GetAssetOrScenePath(scene);
        Scene lightingScene = EditorSceneManager.OpenScene(lightingScenePath, OpenSceneMode.Additive);
        EditorSceneManager.SetActiveScene(lightingScene);

        SearchLightsNeededRealtime();

        Debug.Log("Start baking");
        EditorCoroutineUtility.StartCoroutine(BuildLightingAsync(lightingScene, index), this);
    }

    private IEnumerator BuildLightingAsync(Scene lightingScene, int index)
    {
        var newLightmapMode = LightmapSettings.lightmapsMode;
        Lightmapping.BakeAsync();
        while (Lightmapping.isRunning) { yield return null; }
        //Lightmapping.lightingDataAsset = null;
        EditorSceneManager.SaveScene(lightingScene);
        lightmapData.StoreLightmapInfos(index);
        //LightmapSettings.lightmapsMode = newLightmapMode;
        Debug.Log("Bake Finished");
        EditorSceneManager.CloseScene(lightingScene, true);
    }

    public void SearchLightsNeededRealtime()
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

        lightmapData.latestBuildHasReltimeLights = latestBuildHasRealtimeLights;
    }
}
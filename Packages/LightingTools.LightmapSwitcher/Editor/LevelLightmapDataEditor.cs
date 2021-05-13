using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using Unity.EditorCoroutines.Editor;

[CustomEditor(typeof(LevelLightmapData))]
public class LevelLightmapDataEditor : Editor
{
    public SerializedProperty lightingScenariosScenes;
    public SerializedProperty lightingScenesNames;
    public SerializedProperty allowLoadingLightingScenes;
    public SerializedProperty applyLightmapScaleAndOffset;
    LevelLightmapData lightmapData;
    GUIContent allowLoading = new GUIContent("Allow loading Lighting Scenes", "Allow the Level Lightmap Data script to load a lighting scene additively at runtime if the lighting scenario contains realtime lights.");

    public void OnEnable()
    {
        lightmapData = target as LevelLightmapData;
        lightingScenariosScenes = serializedObject.FindProperty("lightingScenariosScenes");
        lightingScenesNames = serializedObject.FindProperty("lightingScenesNames");
        allowLoadingLightingScenes = serializedObject.FindProperty("allowLoadingLightingScenes");
        applyLightmapScaleAndOffset = serializedObject.FindProperty("applyLightmapScaleAndOffset");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(lightingScenariosScenes, new GUIContent("Lighting Scenarios Scenes"), includeChildren: true);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            lightingScenesNames.arraySize = lightingScenariosScenes.arraySize;

            for (int i = 0; i < lightingScenariosScenes.arraySize; i++) // Conside use onvalidate function to fill lightingSceneNames.
            {
                lightingScenesNames.GetArrayElementAtIndex(i).stringValue = lightingScenariosScenes.GetArrayElementAtIndex(i).objectReferenceValue == null ? "" : lightingScenariosScenes.GetArrayElementAtIndex(i).objectReferenceValue.name;
            }
            serializedObject.ApplyModifiedProperties();
        }
        EditorGUILayout.PropertyField(allowLoadingLightingScenes, allowLoading);
        EditorGUILayout.PropertyField(applyLightmapScaleAndOffset);

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        if (Event.current.type!= EventType.DragPerform)
        {

            for (int i = 0; i < lightmapData.lightingScenariosScenes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var scene = lightmapData.lightingScenariosScenes[i];
                if (scene != null)
                {
                    EditorGUILayout.LabelField(scene.name, EditorStyles.boldLabel);
                    if (GUILayout.Button("Build "))
                    {
                        if (Lightmapping.giWorkflowMode != Lightmapping.GIWorkflowMode.OnDemand)
                        {
                            Debug.LogError("ExtractLightmapData requires that you have baked you lightmaps and Auto mode is disabled.");
                        }
                        else
                        {
                            BuildLightingScenario(scene);
                        }
                    }
                    if (GUILayout.Button("Store "))
                    {
                        lightmapData.StoreLightmapInfos(i);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    public void BuildLightingScenario(SceneAsset scene)
    {
        //Remove reference to LightingDataAsset so that Unity doesn't delete the previous bake
        Lightmapping.lightingDataAsset = null;

        Debug.Log("Loading " + scene.name);

        string lightingScenePath = AssetDatabase.GetAssetOrScenePath(scene);
        Scene lightingScene = EditorSceneManager.OpenScene(lightingScenePath, OpenSceneMode.Additive);
        EditorSceneManager.SetActiveScene(lightingScene);

        SearchLightsNeededRealtime();

        Debug.Log("Start baking");
        EditorCoroutineUtility.StartCoroutine(BuildLightingAsync(lightingScene), this);
    }

    private IEnumerator BuildLightingAsync(Scene lightingScene)
    {
        var newLightmapMode = LightmapSettings.lightmapsMode;
        Lightmapping.BakeAsync();
        while (Lightmapping.isRunning) { yield return null; }
        //Lightmapping.lightingDataAsset = null;
        EditorSceneManager.SaveScene(lightingScene);
        EditorSceneManager.CloseScene(lightingScene, true);
        LightmapSettings.lightmapsMode = newLightmapMode;
        Debug.Log("Bake Finished");
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
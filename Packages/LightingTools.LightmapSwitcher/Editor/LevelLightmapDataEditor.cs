using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelLightmapData))]
public class LevelLightmapDataEditor : Editor
{
	public SerializedProperty lightingScenariosScenes;
    public SerializedProperty allowLoadingLightingScenes;

    GUIContent allowLoading = new GUIContent("Allow loading Lighting Scenes", "Allow the Level Lightmap Data script to load a lighting scene additively at runtime if the lighting scenario contains realtime lights.");

    public void OnEnable()
    {
		lightingScenariosScenes = serializedObject.FindProperty("lightingScenariosScenes");
        allowLoadingLightingScenes = serializedObject.FindProperty("allowLoadingLightingScenes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

		LevelLightmapData lightmapData = (LevelLightmapData)target;

        EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(lightingScenariosScenes, new GUIContent("Lighting Scenarios Scenes"), includeChildren:true);
        if(EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            lightmapData.updateNames();
        }
        EditorGUILayout.PropertyField(allowLoadingLightingScenes, allowLoading);

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();

        for (int i = 0; i < lightmapData.lightingScenariosScenes.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            if ( lightmapData.lightingScenariosScenes[i] != null )
            {
                EditorGUILayout.LabelField(lightmapData.lightingScenariosScenes[i].name.ToString(), EditorStyles.boldLabel);
                if (GUILayout.Button("Build "))
                {
                    lightmapData.BuildLightingScenario(i);
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

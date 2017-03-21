using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelLightmapData))]
public class LightinScenariosInspector : Editor
{
	public SerializedProperty lightingScenariosScenes;

    public void OnEnable()
    {
		lightingScenariosScenes = serializedObject.FindProperty("lightingScenariosScenes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

		LevelLightmapData lightmapData = (LevelLightmapData)target;

		EditorGUILayout.PropertyField(lightingScenariosScenes, new GUIContent("Lighting Scenarios Scenes"), includeChildren:true);

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();

        var ScenariosCount = new int();

        if ( lightmapData.lightingScenariosScenes != null )
        {
            ScenariosCount = lightmapData.lightingScenariosScenes.Count;
        }
        else
        {
            ScenariosCount = 0;
        }

        for (int i = 0; i < ScenariosCount; i++)
        {
            if ( lightmapData.lightingScenariosScenes[i] != null )
            {
                if (GUILayout.Button("Build " + lightmapData.lightingScenariosScenes[i].name.ToString()))
                {
                    lightmapData.BuildLightingScenario(lightmapData.lightingScenariosScenes[i].name.ToString());
                }
                if (GUILayout.Button("Store " + lightmapData.lightingScenariosScenes[i].name.ToString()))
                {
                    lightmapData.StoreLightmapInfos(i);
                }
            }
        }
    }
}

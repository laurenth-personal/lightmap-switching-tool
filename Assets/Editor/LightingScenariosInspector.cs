using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(LevelLightmapData))]
public class LightinScenariosInspector : Editor
{
    public SerializedProperty LightingScenarios;

    public void OnEnable()
    {
		LightingScenarios = serializedObject.FindProperty("lightingScenarios");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        LevelLightmapData LightmapData = (LevelLightmapData)target;

        EditorGUILayout.PropertyField(LightingScenarios, new GUIContent("Lighting Scenarios"), includeChildren:true);

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();

        var ScenariosCount = new int();

        if ( LightmapData.lightingScenarios != null )
        {
            ScenariosCount = LightmapData.lightingScenarios.Length;
        }
        else
        {
            ScenariosCount = 0;
        }

        for (int i = 0; i < ScenariosCount; i++)
        {
            if (GUILayout.Button("Build Lighting Scenario " + (i+1) ))
            {
                LightmapData.BuildLightingScenario(LightmapData.lightingScenarios[i]);
                //LightmapData.StoreLightmapInfos(i);
            }
        }

        EditorGUILayout.Space();

        for (int i = 0; i < ScenariosCount; i++)
        {
            if (GUILayout.Button("Store Lighting Scenario " + (i + 1)))
            {
                LightmapData.StoreLightmapInfos(i);
            }
        }
    }
}

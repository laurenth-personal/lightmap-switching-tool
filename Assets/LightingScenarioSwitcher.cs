using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingScenarioSwitcher : MonoBehaviour {

    private LevelLightmapData LocalLevelLightmapData;
    private int LightingScenarioSelector;
    private int LightingScenariosCount;
    [SerializeField]
    public int DefaultLightingScenario;

    // Use this for initialization
    void Start ()
    {
        LocalLevelLightmapData = FindObjectOfType<LevelLightmapData>();
        LightingScenarioSelector = DefaultLightingScenario;
        LightingScenariosCount = LocalLevelLightmapData.LightingScenarios.Length;
        LocalLevelLightmapData.LoadLightingScenario(DefaultLightingScenario);
        Debug.Log("Load default lighting scenario");
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            LightingScenarioSelector = LightingScenarioSelector + 1;
            if (LightingScenarioSelector > (LightingScenariosCount - 1))
            {
                LightingScenarioSelector = 0;
            }
            LocalLevelLightmapData.LoadLightingScenario(LightingScenarioSelector);
            Debug.Log("Lighting Scenario " + (LightingScenarioSelector + 1));
        }
    }
}

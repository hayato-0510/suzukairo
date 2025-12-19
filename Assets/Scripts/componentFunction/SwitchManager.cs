using System.Collections.Generic;
using UnityEngine;

public class SwitchManager : MonoBehaviour
{
    [Header("スイッチ状態")]
    public int[] switches = new int[2]; // 例: [0,0]

    public CircuitLoader circuitLoader;
    private List<GameObject> switchObjects = new List<GameObject>();

    void Start()
    {
        switchObjects = circuitLoader.switches;

        UpdateSwitchValues();
    }

    void Update()
    {
        UpdateSwitchValues();
    }

    void UpdateSwitchValues()
    {
        foreach (var sw in switchObjects)
        {
            componentData cData = sw.GetComponent<componentData>();
            int index = (int)cData.value;

            if (index < 0 || index >= switches.Length)
                continue;

            if (switches[index] == 1)
                cData.value2 = 1;
            else
                cData.value2 = 0;
        }
    }
}

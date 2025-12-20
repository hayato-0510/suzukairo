using System.Collections.Generic;
using UnityEngine;

public class SwitchManager : MonoBehaviour
{
    [Header("�X�C�b�`���")]
    public int[] switches = new int[2]; // ��: [0,0]

    public CircuitLoader circuitLoader;
    private List<GameObject> switchObjects = new List<GameObject>();
    [SerializeField] private SerialHandler serialHandler;
    [SerializeField] private SerialCommandRouter serialRouter;

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
            int value = (int)cData.value;

            if (serialHandler.isConnected)
            {
                if ((value == 0 && serialRouter.msw1 == false) || (value == 1 && serialRouter.msw2 == false))
                    cData.value2 = 1;
                else
                    cData.value2 = 0;
            }
            else
            {
                cData.value2 = 0;
            }

        }
    }
}

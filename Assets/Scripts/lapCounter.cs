using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class LapCounter : MonoBehaviour
{
    public CircuitLoader loader;
    private List<int> checkpointOrder;
    [SerializeField]
    private int nextIndex = 0;
    public int lap = 0;
    public TextMeshProUGUI lapText;
    private int lapNum;
    public controller controller;
    public GameObject finishText;
    private float batteryVoltage;

    private void Start()
    {
        checkpointOrder = loader.checkPointIds;
        nextIndex = 0;
        lapNum = loader.circuitData.lapNum;
        lapText.text = "0/" + lapNum.ToString();
        finishText.SetActive(false);
        batteryVoltage = loader.circuitData.batteryVoltage;
        controller.voltage = batteryVoltage;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("checkpoint"))
        {
            componentData cp = other.GetComponent<componentData>();
            if (cp == null) return;

            int cpId = cp.id;

            if (nextIndex >= checkpointOrder.Capacity) return;
            if (nextIndex < 0) return;
            if (cpId == checkpointOrder[nextIndex])
            {
                nextIndex++;

                if (nextIndex >= checkpointOrder.Count)
                {
                    nextIndex = -1;
                }
            }
            return;
        }

        if (other.CompareTag("goal"))
        {
            if (nextIndex == -1)
            {
                lap++;
                lapText.text=lap.ToString()+"/"+lapNum;
                ResetAllComponents();
                controller.voltage+=batteryVoltage;
                if (lap >= lapNum) 
                { 
                    controller.canMove = false;
                    finishText.SetActive(true);
                }
                nextIndex = 0;
            }
        }
    }

    private void ResetAllComponents()
    {
        var all = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var comp in all)
        {
            if (comp is IResettableComponent resettable)
            {
                resettable.ResetComponent();
            }
        }
    }
}

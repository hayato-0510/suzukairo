using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class LapCounter : MonoBehaviour
{
    public CircuitLoader loader;
    private List<int> checkpointOrder;
    private int nextIndex = 0;
    public int lap = 0;
    public TextMeshProUGUI lapText;
    private int lapNum;
    public controller controller;
    public GameObject finishText;

    private void Start()
    {
        checkpointOrder = loader.checkPointIds;
        nextIndex = 0;
        lapNum = loader.circuitData.lapNum;
        lapText.text = "0/" + lapNum.ToString();
        finishText.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("checkpoint"))
        {
            componentData cp = other.GetComponent<componentData>();
            if (cp == null) return;

            int cpId = cp.id;

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
                if (lap >= lapNum) 
                { 
                    controller.canMove = false;
                    finishText.SetActive(true);
                }
                nextIndex = 0;
            }
        }
    }
}

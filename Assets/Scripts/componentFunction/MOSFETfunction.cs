using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class MOSFETfunction : MonoBehaviour
{
    private componentData cData;
    private bool isOpen;
    public GameObject open;
    public GameObject close;
    void Start()
    {
        close.SetActive(false);
        open.SetActive(true);
        isOpen = false;
        cData = GetComponent<componentData>();
        InvokeRepeating(nameof(toggle), 0f, cData.value);
    }
    private void toggle()
    {
        if (isOpen)
        {
            close.SetActive(true);
            open.SetActive(false);
            isOpen = false;
        }
        else
        {
            close.SetActive(false);
            open.SetActive(true);
            isOpen = true;
        }
    }
}

using UnityEngine;
using TMPro;
using static UnityEngine.Rendering.DebugUI;

public class resistorFunction : MonoBehaviour
{
    private componentData cData;
    private TextMeshProUGUI text;
    public string unit;

    void Start()
    {
        cData = GetComponent<componentData>();
        text = GetComponentInChildren<TextMeshProUGUI>();

        if (text != null)
        {
            text.text = cData.value2.ToString();   // ï\é¶ì‡óeÇïœçX
        }
    }

    void Update()
    {
        
    }
}

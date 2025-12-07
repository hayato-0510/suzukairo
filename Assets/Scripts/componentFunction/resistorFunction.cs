using UnityEngine;
using TMPro;
using static UnityEngine.Rendering.DebugUI;

public class resistorFunction : MonoBehaviour
{
    private componentData cData;
    private TextMeshProUGUI text;
    private bool isPassed = false;

    void Start()
    {
        cData = GetComponent<componentData>();
        text = GetComponentInChildren<TextMeshProUGUI>();

        if (text != null)
        {
            text.text = cData.value.ToString()+"É∂";   // ï\é¶ì‡óeÇïœçX
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPassed) return;
        if (collision.gameObject.tag == "electron")
        {
            isPassed = true;
            collision.GetComponent<controller>().ampere /= 1f+cData.value*0.001f;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isPassed) return;
        if (collision.gameObject.tag == "electron")
        {
            isPassed = true;
            collision.GetComponent<controller>().ampere -= cData.value * 0.001f;
        }
    }
}

using TMPro;
using UnityEngine;

public class DCDCfunction : MonoBehaviour,IResettableComponent
{
    private componentData cData;
    private bool isPassed = false;
    void Start()
    {
        cData = GetComponent<componentData>();
    }

    public void ResetComponent()
    {
        isPassed = false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPassed) return;
        if (collision.gameObject.tag == "electron")
        {
            isPassed = true;
            collision.GetComponent<controller>().voltage += cData.value;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isPassed) return;
        if (collision.gameObject.tag == "electron")
        {
            isPassed = true;
            collision.GetComponent<controller>().voltage += cData.value;
        }
    }
}

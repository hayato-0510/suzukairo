using UnityEngine;

public class switchFunction : MonoBehaviour
{
    componentData cData;
    public GameObject open;
    public GameObject close;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cData = GetComponent<componentData>();
    }

    // Update is called once per frame
    void Update()
    {
        if (cData.value2 == 1)
        {
            open.SetActive(false);
            close.SetActive(true);
        }
        else
        {
            open.SetActive(true);
            close.SetActive(false);
        }
    }
}

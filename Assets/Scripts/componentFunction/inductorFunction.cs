using UnityEngine;

public class inductorFunction : MonoBehaviour,IResettableComponent
{
    private componentData cData;
    [SerializeField]
    private float incrementAmpere=0;
    public KeyCode MoveFront = KeyCode.W;
    public KeyCode MoveBack = KeyCode.S;

    void Start()
    {
        cData = GetComponent<componentData>();
    }

    public void ResetComponent()
    {
        incrementAmpere = 0;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (incrementAmpere>=cData.value) return;
        if (collision.gameObject.tag == "electron")
        {
            if (Input.GetKey(MoveFront) || Input.GetKey(MoveBack))
            {
                if (incrementAmpere + cData.value2 * Time.deltaTime >= cData.value) { 
                    collision.GetComponent<controller>().ampere += cData.value - incrementAmpere;
                    incrementAmpere = cData.value;
                }
                else { 
                    collision.GetComponent<controller>().ampere += cData.value2 * Time.deltaTime; 
                    incrementAmpere += cData.value2 * Time.deltaTime;
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (incrementAmpere >= cData.value) return;
        if (collision.gameObject.tag == "electron")
        {
            if (Input.GetKey(MoveFront) || Input.GetKey(MoveBack))
            {
                if (incrementAmpere + cData.value2 * Time.deltaTime >= cData.value)
                {
                    collision.GetComponent<controller>().ampere += cData.value - incrementAmpere;
                    incrementAmpere = cData.value;
                }
                else
                {
                    collision.GetComponent<controller>().ampere += cData.value2 * Time.deltaTime;
                    incrementAmpere += cData.value2 * Time.deltaTime;
                }
            }
        }
    }
}

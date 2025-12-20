using UnityEngine;

public class rcdFunction : MonoBehaviour,IResettableComponent
{
    private componentData cData;
    [SerializeField]
    private float decrementAmpere=0;
    public KeyCode MoveFront = KeyCode.W;
    public KeyCode MoveBack = KeyCode.S;

    void Start()
    {
        cData = GetComponent<componentData>();
    }

    public void ResetComponent()
    {
        decrementAmpere = 0;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (decrementAmpere>=cData.value) return;
        if (collision.gameObject.tag == "electron")
        {
            if (Input.GetKey(MoveFront) || Input.GetKey(MoveBack))
            {
                if (decrementAmpere + cData.value2 * Time.deltaTime >= cData.value) { 
                    collision.GetComponent<controller>().ampere -= cData.value - decrementAmpere;
                    decrementAmpere = cData.value;
                }
                else { 
                    collision.GetComponent<controller>().ampere -= cData.value2 * Time.deltaTime; 
                    decrementAmpere += cData.value2 * Time.deltaTime;
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (decrementAmpere >= cData.value) return;
        if (collision.gameObject.tag == "electron")
        {
            if (Input.GetKey(MoveFront) || Input.GetKey(MoveBack))
            {
                if (decrementAmpere + cData.value2 * Time.deltaTime >= cData.value)
                {
                    if (collision.GetComponent<controller>().ampere - (cData.value - decrementAmpere) >= collision.GetComponent<controller>().minAmpere)
                    {
                        collision.GetComponent<controller>().ampere -= cData.value - decrementAmpere;
                        decrementAmpere = cData.value;
                    }
                    else
                    {
                        collision.GetComponent<controller>().ampere = collision.GetComponent<controller>().minAmpere;
                    }
                }
                else
                {
                    if (collision.GetComponent<controller>().ampere - cData.value2 * Time.deltaTime >= collision.GetComponent<controller>().minAmpere)
                    {

                        collision.GetComponent<controller>().ampere -= cData.value2 * Time.deltaTime;
                        decrementAmpere += cData.value2 * Time.deltaTime;
                    } else
                    {
                        collision.GetComponent<controller>().ampere = collision.GetComponent <controller>().minAmpere;
                    }
                }
            }
        }
    }
}

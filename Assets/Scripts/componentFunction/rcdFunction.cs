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
                controller ctrl = collision.GetComponent<controller>();
                if (decrementAmpere + cData.value2 * Time.deltaTime >= cData.value) { 
                    ctrl.ampere -= cData.value - decrementAmpere;
                    // 電流の下限値チェック
                    if (ctrl.ampere < ctrl.minAmpere)
                    {
                        ctrl.ampere = ctrl.minAmpere;
                    }
                    decrementAmpere = cData.value;
                }
                else { 
                    ctrl.ampere -= cData.value2 * Time.deltaTime;
                    // 電流の下限値チェック
                    if (ctrl.ampere < ctrl.minAmpere)
                    {
                        ctrl.ampere = ctrl.minAmpere;
                    }
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
                controller ctrl = collision.GetComponent<controller>();
                if (decrementAmpere + cData.value2 * Time.deltaTime >= cData.value)
                {
                    ctrl.ampere -= cData.value - decrementAmpere;
                    // 電流の下限値チェック
                    if (ctrl.ampere < ctrl.minAmpere)
                    {
                        ctrl.ampere = ctrl.minAmpere;
                    }
                    decrementAmpere = cData.value;
                }
                else
                {
                    ctrl.ampere -= cData.value2 * Time.deltaTime;
                    // 電流の下限値チェック
                    if (ctrl.ampere < ctrl.minAmpere)
                    {
                        ctrl.ampere = ctrl.minAmpere;
                    }
                    decrementAmpere += cData.value2 * Time.deltaTime;
                }
            }
        }
    }
}

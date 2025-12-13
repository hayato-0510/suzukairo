using TMPro;
using UnityEngine;
using System.Collections;

public class countdown : MonoBehaviour
{
    public TextMeshProUGUI countDownText;
    public TextMeshProUGUI timerText;
    public int timeLimit=90;
    public controller controller;
    public GameObject finishText;
    public CircuitLoader cl;

    private int time;

    void Start()
    {
        StartCoroutine(CountDown());
        timeLimit=cl.circuitData.timeLimit;
    }

    IEnumerator CountDown()
    {
        string[] seq = { "3", "2", "1", "GO!" };

        foreach (var s in seq)
        {
            countDownText.text = s;

            // 透明＆縮小からスタート
            float t = 0;
            countDownText.color = new Color(1, 1, 1, 0);
            countDownText.transform.localScale = Vector3.zero;

            // 拡大＋フェード
            while (t < 1f)
            {
                t += Time.deltaTime * 1.5f;
                countDownText.color = new Color(1, 1, 1, t);
                countDownText.transform.localScale = Vector3.one * t;
                yield return null;
            }

            // 少し待つ
            yield return new WaitForSeconds(0.3f);

            // フェードアウト
            t = 1f;
            while (t > 0f)
            {
                t -= Time.deltaTime * 2f;
                countDownText.color = new Color(1, 1, 1, t);
                yield return null;
            }
        }
        controller.canMove = true;
        countDownText.text = "";
        time = timeLimit;
        InvokeRepeating(nameof(Timer), 0f,1f);
    }

    private void Timer()
    {
        timerText.text = time.ToString();
        if (time <= 0) 
        { 
            CancelInvoke(nameof(Timer)); 
            controller.canMove = false;
            finishText.SetActive(true);
        }
        time--;
    }
}

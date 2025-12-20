using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class countdown : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI countDownText;
    public TextMeshProUGUI timerText;
    public controller controller;
    public GameObject finishText;
    public CircuitLoader cl;

    [Header("Settings")]
    public int timeLimit = 90;
    
    [HideInInspector] public bool isFinished = false;
    private int time;

    [Header("BGM")]
    public GameBGMManager gameBGMManager;

    void Start()
    {
        StartCoroutine(CountDown());
        timeLimit=cl.circuitData.timeLimit;
    }

    IEnumerator CountDown()
    {
        string[] seq = { "3", "2", "1", "GO!" };
        Debug.Log("<color=green>[Countdown] Starting countdown sequence.</color>");
        
        if (gameBGMManager != null)
        {
            gameBGMManager.PlayBGM();
            Debug.Log("<color=green>[Countdown] Game BGM Started.</color>");
        }
        else
        {
            Debug.LogError("<color=red>[Countdown] GameBGMManager is NOT assigned! Please assign it in the Inspector.</color>");
        }

        foreach (var s in seq)
        {
            countDownText.text = s;
            float t = 0;
            countDownText.color = new Color(1, 1, 1, 0);
            countDownText.transform.localScale = Vector3.zero;

            while (t < 1f)
            {
                t += Time.deltaTime * 1.5f;
                countDownText.color = new Color(1, 1, 1, t);
                countDownText.transform.localScale = Vector3.one * t;
                yield return null;
            }

            yield return new WaitForSeconds(0.3f);

            t = 1f;
            while (t > 0f)
            {
                t -= Time.deltaTime * 2f;
                countDownText.color = new Color(1, 1, 1, t);
                yield return null;
            }
        }
        //ここからレース開始
        controller.canMove = true;
        countDownText.text = "";
        time = timeLimit;
        InvokeRepeating(nameof(Timer), 0f, 1f);
    }

    private void Timer()
    {
        if (isFinished)
        {
            CancelInvoke(nameof(Timer));
            return;
        }

        timerText.text = time.ToString();
        
        if (time <= 0) 
        { 
            CancelInvoke(nameof(Timer)); 
            OnTimeUp();
        }
        time--;
    }

    private void OnTimeUp()
    {
        if (gameBGMManager != null) gameBGMManager.StopBGMWithFade();
        if (isFinished) return;
        isFinished = true;

        controller.canMove = false;
        finishText.SetActive(true);

        // --- IDベースの保存ロジック ---
        // タイムアップでもランキングデータとして保存（順位計算に含めるため）
        string uniqueId = System.Guid.NewGuid().ToString();
        string mapName = (cl != null && cl.circuitData != null) 
                         ? cl.circuitData.circuitName 
                         : "Unknown Course";

        var timeUpEntry = new RankingEntry {
            id = uniqueId,
            mapName = mapName,
            playerName = "", // RankingManagerで自動設定されるため空でOK
            timeText = "TIME UP",
            timeInSeconds = 9999f, // ワースト記録として扱う
            timestamp = System.DateTimeOffset.Now.ToUnixTimeSeconds()
        };

        // データベースに保存
        RankingManager rankingMgr = new RankingManager();
        rankingMgr.SaveRecord(timeUpEntry);

        // IDをリザルト画面へ渡す
        GameResultData.LastEntryId = uniqueId;

        // 2秒後に遷移
        Invoke(nameof(TransitionToResult), 2.0f);
    }

    private void TransitionToResult()
    {
        SceneManager.LoadScene(GameResultData.ResultSceneName);
    }
}
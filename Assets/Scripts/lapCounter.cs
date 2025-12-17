using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class LapCounter : MonoBehaviour
{
    public CircuitLoader loader;
    public controller controller;
    public TextMeshProUGUI lapText;
    public GameObject finishText;

    private List<int> checkpointOrder;
    [SerializeField] private int nextIndex = 0;
    public int lap = 0;
    private int lapNum;
    private float batteryVoltage;

    // タイマー・遷移管理用
    private float raceTimer = 0f;
    private bool isRacing = false;
    private bool isFinished = false;

    [Header("BGM")]
    public GameBGMManager gameBGMManager;

    private void Start()
    {
        if (loader == null || loader.circuitData == null)
        {
            Debug.LogError("CircuitLoader または CircuitData が設定されていません。");
            return;
        }

        checkpointOrder = loader.checkPointIds;
        nextIndex = 0;
        lapNum = loader.circuitData.lapNum;
        lapText.text = "0/" + lapNum.ToString();
        finishText.SetActive(false);
        batteryVoltage = loader.circuitData.batteryVoltage;
        controller.voltage = batteryVoltage;

        // カウントダウン終了後に controller.canMove が true になるのを監視して開始
        raceTimer = 0f;
    }

    private void Update()
    {
        // コントローラーが動けるようになったらタイマー開始
        if (!isRacing && controller.canMove && !isFinished)
        {
            isRacing = true;
        }

        // レース中のみ加算
        if (isRacing && !isFinished)
        {
            raceTimer += Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isFinished) return;

        // チェックポイント判定
        if (other.CompareTag("checkpoint"))
        {
            componentData cp = other.GetComponent<componentData>();
            if (cp == null) return;

            int cpId = cp.id;
            if (nextIndex == -1 || nextIndex >= checkpointOrder.Count) return;

            if (cpId == checkpointOrder[nextIndex])
            {
                nextIndex++;
                if (nextIndex >= checkpointOrder.Count)
                {
                    nextIndex = -1; // 全チェックポイント通過フラグ
                }
            }
            return;
        }

        // ゴール判定
        if (other.CompareTag("goal"))
        {
            // 全チェックポイントを通過している場合のみカウント
            if (nextIndex == -1)
            {
                lap++;
                lapText.text = lap.ToString() + "/" + lapNum;
                
                ResetAllComponents();
                controller.voltage += batteryVoltage;

                if (lap >= lapNum)
                {
                    OnFinishRace();
                }
                else
                {
                    nextIndex = 0; // 次の周回のためにリセット
                }
            }
        }
    }
    private void OnFinishRace()
    {
        if (gameBGMManager != null) gameBGMManager.StopBGMWithFade();
        if (isFinished) return;
        isFinished = true;

        // 1. 保存用のエンティティを作成
        string uniqueId = System.Guid.NewGuid().ToString(); 
        var newEntry = new RankingEntry {
            id = uniqueId,
            mapName = loader.circuitData.circuitName,
            playerName = "YOU",
            timeText = TimeFormatter.Format(raceTimer),
            timeInSeconds = raceTimer,
            timestamp = System.DateTimeOffset.Now.ToUnixTimeSeconds()
        };

        // 2. この場で保存
        RankingManager rankingMgr = new RankingManager();
        rankingMgr.SaveRecord(newEntry);

        // 3. IDだけをセットして遷移
        GameResultData.LastEntryId = uniqueId;
        
        Invoke(nameof(TransitionToResult), 2.0f);
    }

    private void TransitionToResult()
    {
        SceneManager.LoadScene(GameResultData.ResultSceneName);
    }

    private void ResetAllComponents()
    {
        var all = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var comp in all)
        {
            if (comp is IResettableComponent resettable)
            {
                resettable.ResetComponent();
            }
        }
    }
}
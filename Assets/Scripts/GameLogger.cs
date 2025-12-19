using UnityEngine;
using System.IO;

public class GameLogger : MonoBehaviour
{
    public static GameLogger Instance { get; private set; }

    [Header("ロギング設定")]
    [Tooltip("ロギングを有効にするか")]
    public bool enableLogging = true;
    [Tooltip("ログを記録する間隔（秒）")]
    public float loggingInterval = 0.1f;

    // メモリ内ログ（AI分析用）
    public System.Collections.Generic.List<PlayerStateData> logs = new System.Collections.Generic.List<PlayerStateData>();

    private StreamWriter writer;
    private string logPath;
    public string CurrentLogPath => logPath; // 外部から参照可能にする
    private bool isLogging = false;

    private void Awake()
    {
        // Singleton パターンの実装
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // シーンをまたいで存在させる
    }

    /// <summary>
    /// レース開始時に呼び出され、ロギングを開始する
    /// </summary>
    public void StartLogging()
    {
        if (!enableLogging) return;
        if (isLogging)
        {
            Debug.LogWarning("[GameLogger] Logging is already active.");
            return;
        }

        logs.Clear(); // 新しいレースのためにログをクリア

        string logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
        Directory.CreateDirectory(logDirectory); // フォルダがなければ作成

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"race_log_{timestamp}.jsonl";
        logPath = Path.Combine(logDirectory, fileName);

        writer = new StreamWriter(logPath, false, System.Text.Encoding.UTF8);
        isLogging = true;

        Debug.Log($"[GameLogger] Logging started. Log file at: {logPath}");
        
        // レース開始イベントを記録
        LogRaceStart();
    }

    /// <summary>
    /// レース終了時に呼び出され、ロギングを終了する
    /// </summary>
    public void StopLogging()
    {
        if (!isLogging) return;
        
        // レース終了イベントを記録
        LogRaceFinish();
        
        writer?.Close();
        writer = null;
        isLogging = false;
        Debug.Log("[GameLogger] Logging stopped.");
    }

    public void LogPlayerState(PlayerStateData data)
    {
        if (!isLogging) return;
        
        data.timestamp = Time.time; // タイムスタンプを記録
        logs.Add(data); // メモリに保存

        PlayerStateLog log = new PlayerStateLog
        {
            timestamp = data.timestamp,
            eventType = "player_state",
            eventData = data
        };
        string json = JsonUtility.ToJson(log);
        writer.WriteLine(json);
    }
    
    public void LogBoost(BoostActivatedData data)
    {
        if (!isLogging) return;
        BoostLog log = new BoostLog
        {
            timestamp = Time.time,
            eventType = "boost_activated",
            eventData = data
        };
        string json = JsonUtility.ToJson(log);
        writer.WriteLine(json);
    }
    
    public void LogLapComplete(LapCompleteData data)
    {
        if (!isLogging) return;
        LapCompleteLog log = new LapCompleteLog
        {
            timestamp = Time.time,
            eventType = "lap_complete",
            eventData = data
        };
        string json = JsonUtility.ToJson(log);
        writer.WriteLine(json);
    }

    private void LogRaceStart()
    {
        GameEventLog log = new GameEventLog
        {
            timestamp = Time.time,
            eventType = "race_start"
        };
        string json = JsonUtility.ToJson(log);
        writer.WriteLine(json);
    }

    private void LogRaceFinish()
    {
        GameEventLog log = new GameEventLog
        {
            timestamp = Time.time,
            eventType = "race_finish"
        };
        string json = JsonUtility.ToJson(log);
        writer.WriteLine(json);
    }


    private void OnApplicationQuit()
    {
        // アプリケーション終了時に確実にファイルを閉じる
        StopLogging();
    }
}

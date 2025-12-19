using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class LiveCommentary : MonoBehaviour
{
    [Header("Dependencies")]
    public AICoachService aiService;
    
    [Header("Settings")]
    public float commentInterval = 15f; // 15秒ごとにコメント
    public bool isCommentaryActive = false;

    // UI表示用コールバック
    public System.Action<string> OnCommentReceived;

    private float timer;
    private int lastLogIndex = 0;

    private void Start()
    {
        if (aiService == null)
        {
            aiService = FindObjectOfType<AICoachService>();
            if (aiService == null)
            {
                Debug.LogError("[LiveCommentary] AICoachService not found!");
                enabled = false;
                return;
            }
        }

        // ゲーム開始時は少し待ってからスタート
        timer = 5f; 
        isCommentaryActive = true;
        Debug.Log("[LiveCommentary] Started. First comment in 5 seconds.");
    }

    private void Update()
    {
        if (!isCommentaryActive) return;
        if (GameLogger.Instance == null || !GameLogger.Instance.enableLogging) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = commentInterval;
            RequestCommentary();
        }
    }

    private void RequestCommentary()
    {
        Debug.Log("[LiveCommentary] Requesting commentary...");

        var logs = GameLogger.Instance.logs;
        if (logs.Count == 0) 
        {
            Debug.Log("[LiveCommentary] No logs found.");
            return;
        }

        // 前回のチェックから今回までのログを抽出
        int currentCount = logs.Count;
        if (currentCount <= lastLogIndex) 
        {
            Debug.Log("[LiveCommentary] No new logs since last check.");
            return;
        }

        var recentLogs = logs.GetRange(lastLogIndex, currentCount - lastLogIndex);
        lastLogIndex = currentCount;

        // ログが少なすぎる場合はスキップ
        if (recentLogs.Count < 10) 
        {
            Debug.Log($"[LiveCommentary] Not enough logs ({recentLogs.Count} < 10). Skipping.");
            return;
        }

        string prompt = GenerateMiniPrompt(recentLogs);
        Debug.Log($"[LiveCommentary] Sending prompt to AI: {prompt.Replace("\n", " ")}");
        
        StringBuilder sb = new StringBuilder();
        aiService.GetAdvice(
            systemPrompt: "あなたはレースゲームの実況者です。プレイヤーの直近の走りを20文字以内の短い言葉で、少し辛口に実況してください。",
            userPrompt: prompt,
            onTokenReceived: (token) => sb.Append(token),
            onComplete: () => 
            {
                string comment = sb.ToString();
                Debug.Log($"[Live Commentary] Received: {comment}");
                OnCommentReceived?.Invoke(comment);
            }
        );
    }

    private string GenerateMiniPrompt(List<PlayerStateData> logs)
    {
        // 直近のハイライトを抽出
        float avgSpeed = logs.Average(l => new Vector2(l.velocity.x, l.velocity.y).magnitude);
        float minAmpere = logs.Min(l => l.ampere);
        var resistorContact = logs.FirstOrDefault(l => l.componentContext.componentType == "Resistor");
        var boostUsed = logs.FirstOrDefault(l => l.velocity.x > 10f); // 仮のブースト判定

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"直近{commentInterval}秒のデータ:");
        sb.AppendLine($"平均速度: {avgSpeed:F1}");
        sb.AppendLine($"最低電流: {minAmpere:F2}A");
        
        if (resistorContact != null) sb.AppendLine("イベント: 抵抗エリアを通過");
        if (boostUsed != null) sb.AppendLine("イベント: 急加速（ブースト）");

        return sb.ToString();
    }
}

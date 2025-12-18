using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RaceLogAnalyzer : MonoBehaviour
{
    [Header("Dependencies")]
    public AICoachService aiService;
    
    // UIへの依存を削除し、コールバックで処理する形に変更
    
    /// <summary>
    /// 分析を実行する
    /// </summary>
    /// <param name="onTokenReceived">トークン受信時のコールバック（オプション）</param>
    /// <param name="onComplete">完了時のコールバック（オプション）</param>
    public void AnalyzeLastRun(System.Action<string> onTokenReceived = null, System.Action onComplete = null)
    {
        // GameLoggerからログを取得（シングルトンや静的参照を想定）
        if (GameLogger.Instance == null || GameLogger.Instance.logs.Count == 0)
        {
            Debug.LogWarning("ログデータがありません。");
            return;
        }

        var logs = GameLogger.Instance.logs;
        string prompt = GeneratePrompt(logs);
        
        Debug.Log($"[AI Prompt] {prompt}");

        // レスポンスをまとめるためのバッファ
        StringBuilder fullResponse = new StringBuilder();

        // AIに送信
        aiService.GetAdvice(
            systemPrompt: "あなたは電子回路レースゲームの熱血コーチです。物理法則（オームの法則：V=IR）や電子部品の特性（抵抗、コンデンサ、コイル）を交えて、プレイヤーにタイム短縮のアドバイスをしてください。口調は短く、的確に、少しユーモアを交えて。",
            userPrompt: prompt,
            onTokenReceived: (token) => 
            {
                // バッファに追加
                fullResponse.Append(token);

                // 外部からのコールバックがあれば実行
                onTokenReceived?.Invoke(token);
            },
            onComplete: () => 
            {
                // まとめてログ出力
                Debug.Log($"[AI Advice Result]\n{fullResponse.ToString()}");
                Debug.Log("AI Analysis Complete.");
                onComplete?.Invoke();
            }
        );
    }

    private string GeneratePrompt(List<PlayerStateData> logs)
    {
        // データの要約
        float maxSpeed = 0f;
        float minAmpere = float.MaxValue;
        float totalVoltageUsed = 0f;
        float startVoltage = logs[0].voltage;
        
        // 最も遅かった抵抗エリアを探す
        var resistorLogs = logs.Where(l => l.componentContext.componentType == "Resistor").ToList();
        float minResistorSpeed = float.MaxValue;
        int worstResistorId = -1;

        foreach (var log in logs)
        {
            float speed = new Vector2(log.velocity.x, log.velocity.y).magnitude;
            if (speed > maxSpeed) maxSpeed = speed;
            if (log.ampere < minAmpere) minAmpere = log.ampere;
        }
        
        if (logs.Count > 0)
        {
            totalVoltageUsed = startVoltage - logs[logs.Count - 1].voltage;
        }

        // 抵抗でのパフォーマンス分析
        if (resistorLogs.Count > 0)
        {
            var grouped = resistorLogs.GroupBy(l => l.componentContext.componentId);
            foreach (var group in grouped)
            {
                float avgSpeed = group.Average(l => new Vector2(l.velocity.x, l.velocity.y).magnitude);
                if (avgSpeed < minResistorSpeed)
                {
                    minResistorSpeed = avgSpeed;
                    worstResistorId = group.Key;
                }
            }
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("【走行データ分析】");
        sb.AppendLine($"最高速度: {maxSpeed:F2}");
        sb.AppendLine($"最低電流: {minAmpere:F2}A (電流が低いと速度が出ません)");
        sb.AppendLine($"消費電圧: {totalVoltageUsed:F1}V (ブーストに使用)");
        
        if (worstResistorId != -1)
        {
            sb.AppendLine($"課題: 抵抗(ID:{worstResistorId})で速度が {minResistorSpeed:F2} まで低下しました。");
        }

        sb.AppendLine("\nこのデータを見て、次のレースで勝つための具体的なアドバイスを140文字以内でください。");
        
        return sb.ToString();
    }
}

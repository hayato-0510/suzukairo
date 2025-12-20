using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RaceLogAnalyzer : MonoBehaviour
{
    public static RaceLogAnalyzer Instance { get; private set; }

    [Header("Dependencies")]
    public AICoachService aiService;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
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

        if (aiService == null)
        {
            Debug.LogError("[RaceLogAnalyzer] AICoachService is not assigned!");
            return;
        }

        // AIに送信
        string systemPrompt = @"
あなたは電子回路レースゲーム『スズカイロ』の熱血AIコーチです。
プレイヤーは電子となり、回路上の素子（抵抗、コイル、スイッチ、MOSFET、RCD回路など）を攻略してゴールを目指します。
物理法則（オームの法則 V=IR、レンツの法則、過渡現象、ジュール熱など）や素子のギミック特性を交えて、プレイヤーにタイム短縮のための具体的なアドバイスをしてください。

【キャラクター設定】
- 口調は熱血で、少しユーモアがある。
- 専門用語を使いつつも、ゲーム的な攻略法（ブーストの使い所、タイミング、エネルギー管理など）に落とし込んで解説する。
- 単に「速く走れ」と言うだけでなく、「エネルギー効率」「タイミング」「物理現象への対抗策」など多角的な視点でアドバイスする。

【回答のルール】
- 100文字程度で簡潔にまとめること。
- 提示された[課題]や[直近の素子]のデータに基づき、具体的な改善点を1つ以上挙げること。
- [課題]がない場合は、[総評]を元に褒めること。無理に課題を捏造しないこと。
- 特に「最後に通過した素子」での挙動について触れると、プレイヤーにとって直感的で良い。
- 挨拶は省略し、いきなりアドバイスから入ること。
- 空の回答は絶対にしないこと。
";

        aiService.GetAdvice(
            systemPrompt: systemPrompt,
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
        if (logs == null || logs.Count == 0) return "データなし";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("【走行データ分析レポート】");

        // 1. 全体統計
        float totalTime = logs.Last().timestamp - logs.First().timestamp;
        float maxSpeed = logs.Max(l => new Vector2(l.velocity.x, l.velocity.y).magnitude);
        float avgSpeed = logs.Average(l => new Vector2(l.velocity.x, l.velocity.y).magnitude);
        float startVoltage = logs.First().voltage;
        float endVoltage = logs.Last().voltage;
        float voltageConsumed = startVoltage - endVoltage;

        sb.AppendLine($"[全体] タイム: {totalTime:F2}秒, 最高速度: {maxSpeed:F2}, 平均速度: {avgSpeed:F2}");
        sb.AppendLine($"[エネルギー] 消費電圧: {voltageConsumed:F1}V (残: {endVoltage:F1}V)");

        // 2. 素子ごとの分析
        var components = logs.Where(l => l.componentContext.onComponent)
                             .GroupBy(l => new { l.componentContext.componentType, l.componentContext.componentId })
                             .Select(g => new 
                             { 
                                 Type = g.Key.componentType, 
                                 Id = g.Key.componentId,
                                 AvgSpeed = g.Average(l => new Vector2(l.velocity.x, l.velocity.y).magnitude),
                                 MinSpeed = g.Min(l => new Vector2(l.velocity.x, l.velocity.y).magnitude),
                                 EntrySpeed = new Vector2(g.First().velocity.x, g.First().velocity.y).magnitude,
                                 ExitSpeed = new Vector2(g.Last().velocity.x, g.Last().velocity.y).magnitude,
                                 VoltageDrop = g.First().voltage - g.Last().voltage
                             })
                             .ToList();

        // 抵抗 (Resistor): エネルギーロスと減速
        // 閾値: 進入速度の70%以下 または 電圧降下が大きい(0.5V以上)
        var worstResistor = components.Where(c => c.Type == "Resistor")
                                      .OrderByDescending(c => c.VoltageDrop) // 電圧ロスが大きい順
                                      .FirstOrDefault();
        if (worstResistor != null && (worstResistor.MinSpeed < worstResistor.EntrySpeed * 0.7f || worstResistor.VoltageDrop > 0.5f))
        {
            sb.AppendLine($"[課題:抵抗] ID:{worstResistor.Id} で {worstResistor.VoltageDrop:F2}V も電圧を消費している(速度低下: {worstResistor.EntrySpeed:F2} -> {worstResistor.MinSpeed:F2})。ジュール熱としてエネルギーを捨てているぞ！ブーストで一気に抜けてロスを減らせ！");
        }

        // コイル (Inductor): 立ち上がりの遅さ
        // ギミック: 徐々に加速する（電流の変化を妨げる）
        var worstInductor = components.Where(c => c.Type == "Inductor")
                                      .OrderBy(c => c.AvgSpeed)
                                      .FirstOrDefault();
        if (worstInductor != null && worstInductor.AvgSpeed < 5.0f)
        {
            sb.AppendLine($"[課題:コイル] ID:{worstInductor.Id} での立ち上がりが重い。コイルの逆起電力に負けている証拠だ。突入速度を上げて、磁界の変化をねじ伏せろ！");
        }

        // スイッチ (Switch): 停止したかどうか
        // ギミック: ボタンで開閉
        var stoppedSwitch = components.Where(c => c.Type == "Switch" && c.MinSpeed < 0.5f).FirstOrDefault();
        if (stoppedSwitch != null)
        {
            sb.AppendLine($"[課題:スイッチ] ID:{stoppedSwitch.Id} で完全に足が止まった。回路が遮断されているぞ！開閉のタイミングを体に叩き込め！");
        }

        // MOSFET: タイミング
        // ギミック: 時間で開閉
        var stoppedMosfet = components.Where(c => c.Type == "MOSFET" && c.MinSpeed < 0.5f).FirstOrDefault();
        if (stoppedMosfet != null)
        {
            sb.AppendLine($"[課題:MOSFET] ID:{stoppedMosfet.Id} のゲートが閉じている。無理に通ろうとしても弾かれるだけだ。周期を見極める冷静さを持て！");
        }

        // RCD (RC + Diode): 減速
        // ギミック: 徐々に減速
        var worstRCD = components.Where(c => c.Type == "RCD")
                                 .OrderBy(c => c.ExitSpeed)
                                 .FirstOrDefault();
        if (worstRCD != null && worstRCD.ExitSpeed < worstRCD.EntrySpeed * 0.5f)
        {
            sb.AppendLine($"[課題:RCD回路] ID:{worstRCD.Id} でコンデンサに電荷を奪われている(電圧降下: {worstRCD.VoltageDrop:F2}V)。充電完了を待つな、勢いで押し切れ！");
        }

        // 3. ブースト評価

        // 3. ブースト評価
        // 電圧が急激に減った区間を探す（簡易実装）
        int boostCount = 0;
        for(int i=1; i<logs.Count; i++)
        {
            if (logs[i-1].voltage - logs[i].voltage > 0.5f) // 閾値
            {
                boostCount++;
                i += 10; // 連続検知を防ぐ
            }
        }
        
        if (boostCount == 0 && endVoltage > 5.0f)
        {
            sb.AppendLine("[アドバイス] 電圧が余りすぎている。もっとブーストを使ってタイムを縮められるはずだ。V=IR、電圧をかければ電流（速度）は上がる！");
        }
        else if (endVoltage < 1.0f)
        {
            sb.AppendLine("[アドバイス] 電圧ギリギリでのゴール、ナイスファイトだ。だが、配分を間違えると途中で止まるぞ。");
        }

        // 4. 直近の素子 (Last Component)
        var lastComponent = components.LastOrDefault();
        if (lastComponent != null)
        {
             sb.AppendLine($"[直近の素子] 最後に通過したのは {lastComponent.Type} (ID:{lastComponent.Id}) だ。進入速度: {lastComponent.EntrySpeed:F2}, 脱出速度: {lastComponent.ExitSpeed:F2}。");
             if (lastComponent.ExitSpeed < lastComponent.EntrySpeed * 0.8f) {
                 sb.AppendLine("最後で失速しているぞ！終わり良ければ全て良し、最後まで気を抜くな！");
             } else if (lastComponent.ExitSpeed > lastComponent.EntrySpeed * 1.1f) {
                 sb.AppendLine("いい加速で抜けられたな！その調子だ！");
             } else {
                 sb.AppendLine("安定して通過できたな。次はもっと攻めてみよう！");
             }
        }

        // 課題が見つからなかった場合
        if (!sb.ToString().Contains("[課題"))
        {
            sb.AppendLine("[総評] 大きなミスもなく、素晴らしい走りだ！この調子でタイムを縮めていこう！");
        }

        sb.AppendLine("以上のデータを元に、次のレースに向けた熱いアドバイスを頼む！特に[直近の素子]や[課題]について具体的に触れてくれ！");
        
        return sb.ToString();
    }
}

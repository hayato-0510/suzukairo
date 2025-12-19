using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ReplayController : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private GameObject playerVisualPrefab; // リプレイ用のプレイヤー表示
    [SerializeField] private Color playerColor = Color.yellow;
    [SerializeField] private float visualScale = 10.0f; // デフォルト値をさらに大きく
    [SerializeField] private bool showTrail = true;
    [SerializeField] private float trailTime = 6.0f;
    [SerializeField] private float trailWidth = 3.0f; // デフォルト値をさらに太く

    private GameObject playerInstance;
    private TrailRenderer trailRenderer;
    private List<PlayerStateData> replayData;
    private bool isPlaying = false;
    private float replayStartTime;
    private float logDuration;

    private void Update()
    {
        if (!isPlaying || replayData == null || replayData.Count == 0) return;

        float currentTime = Time.time - replayStartTime;

        // ループ再生
        if (currentTime > logDuration)
        {
            replayStartTime = Time.time;
            currentTime = 0;
            if (trailRenderer != null) trailRenderer.Clear(); // ループ時に軌跡をクリア
        }

        UpdatePlayerPosition(currentTime);
    }

    public void StartReplay(List<PlayerStateData> logs, GameObject prefab = null)
    {
        if (logs == null || logs.Count == 0)
        {
            Debug.LogWarning("[ReplayController] No logs to replay.");
            return;
        }

        replayData = logs;
        logDuration = logs.Last().timestamp - logs.First().timestamp; // ログの期間を計算
        
        // タイムスタンプを0開始に正規化するためのオフセットは、再生時に考慮する
        // ここでは単純に、ログの最初のタイムスタンプを基準にする

        // プレイヤー生成
        if (playerInstance != null) Destroy(playerInstance);

        Debug.Log($"[ReplayController] Starting replay with Scale: {visualScale}, TrailWidth: {trailWidth}");

        if (prefab != null)
        {
            playerInstance = Instantiate(prefab, transform);
            // プレハブを使う場合もスケールを適用
            playerInstance.transform.localScale = Vector3.one * visualScale;
        }
        else if (playerVisualPrefab != null)
        {
            playerInstance = Instantiate(playerVisualPrefab, transform);
            playerInstance.transform.localScale = Vector3.one * visualScale;
        }
        else
        {
            // デフォルトの見た目（黄色い球）
            playerInstance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            playerInstance.transform.SetParent(transform);
            playerInstance.transform.localScale = Vector3.one * visualScale;
            var renderer = playerInstance.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = playerColor;
            // コライダーは不要
            var collider = playerInstance.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
        }

        // 軌跡の追加
        if (showTrail)
        {
            trailRenderer = playerInstance.AddComponent<TrailRenderer>();
            trailRenderer.time = trailTime;
            trailRenderer.startWidth = trailWidth;
            trailRenderer.endWidth = 0f;
            trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
            trailRenderer.startColor = playerColor;
            trailRenderer.endColor = new Color(playerColor.r, playerColor.g, playerColor.b, 0f);
        }

        isPlaying = true;
        replayStartTime = Time.time;
    }

    public void StopReplay()
    {
        isPlaying = false;
        if (playerInstance != null) Destroy(playerInstance);
    }

    private void UpdatePlayerPosition(float time)
    {
        if (playerInstance == null) return;

        // 現在時刻に対応するログデータを検索（線形探索だがデータ量が少なければOK。多ければ二分探索にする）
        // timestampは絶対時間なので、最初のログのtimestampを引いて相対時間で比較する
        float startTimestamp = replayData[0].timestamp;
        float targetTimestamp = startTimestamp + time;

        // 該当する区間を探す
        // TODO: パフォーマンス最適化（インデックスをキャッシュするなど）
        for (int i = 0; i < replayData.Count - 1; i++)
        {
            if (replayData[i].timestamp <= targetTimestamp && replayData[i+1].timestamp >= targetTimestamp)
            {
                // 補間
                float t = (targetTimestamp - replayData[i].timestamp) / (replayData[i+1].timestamp - replayData[i].timestamp);
                
                Vector2 p0 = new Vector2(replayData[i].position.x, replayData[i].position.y);
                Vector2 p1 = new Vector2(replayData[i+1].position.x, replayData[i+1].position.y);
                
                Vector2 currentPos = Vector2.Lerp(p0, p1, t);
                playerInstance.transform.position = new Vector3(currentPos.x, currentPos.y, 0);
                return;
            }
        }

        // 範囲外（最後）
        if (replayData.Count > 0)
        {
            var last = replayData.Last();
            playerInstance.transform.position = new Vector3(last.position.x, last.position.y, 0);
        }
    }
}

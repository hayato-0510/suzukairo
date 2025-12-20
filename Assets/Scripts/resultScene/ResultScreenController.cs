using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class ResultScreenController : MonoBehaviour
{
    [System.Serializable]
    public struct MapBackground
    {
        public string mapName;
        public Texture2D backgroundTexture;
    }

    [Header("UI Document")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Navigation")]
    [SerializeField] private string mainMenuSceneName = "StartScene";

    [Header("Map Visuals")]
    [SerializeField] private List<MapBackground> mapBackgrounds;
    [SerializeField] private Texture2D fallbackBackground;
    [SerializeField] private CircuitLoader circuitLoader;
    [SerializeField] private ReplayController replayController; // 追加

    [Header("Debug")]
    [SerializeField] private bool useDebugData = false;
    [SerializeField] private string debugMapName = "Circuit 1";

    private VisualElement root;

    void Start()
    {
        if (uiDocument == null) return;
        root = uiDocument.rootVisualElement;

        // 1. データの取得・判定・表示（この中で背景も設定）
        ProcessAndDisplayResults();

        // 2. ボタンのイベント登録
        SetupButtons();

        // 3. AI分析の実行
        var analyzer = FindObjectOfType<RaceLogAnalyzer>();
        if (analyzer != null)
        {
            // UI Toolkitのラベルを取得（事前にUXMLに "AdviceLabel" という名前のLabelを追加しておく必要があります）
            var adviceLabel = root.Q<Label>("AdviceLabel");
            if (adviceLabel != null)
            {
                adviceLabel.text = "AI Coach is analyzing..."; // 初期化（空文字は避ける）
                analyzer.AnalyzeLastRun(
                    onTokenReceived: (token) => 
                    {
                        if (adviceLabel.text == "AI Coach is analyzing...") adviceLabel.text = "";
                        adviceLabel.text += token;
                    },
                    onComplete: () => Debug.Log("Advice displayed.")
                );
            }
            else
            {
                // ラベルが見つからない場合はログのみ
                analyzer.AnalyzeLastRun();
            }
        }
    }

    private void ProcessAndDisplayResults()
    {
        // デバッグモードの場合の処理
        if (string.IsNullOrEmpty(GameResultData.LastEntryId) && useDebugData)
        {
            Debug.LogWarning("[ResultScreenController] Using Debug Data because LastEntryId is empty.");
            
            // デバッグ用の背景生成（コース生成）
            if (circuitLoader != null)
            {
                // デバッグ時は CourseSelectionData.SelectedMapId を使用するか、
                // もしくは debugMapName に対応するIDを探すロジックが必要ですが、
                // ここではシンプルに現在の CourseSelectionData.SelectedMapId で生成します。
                circuitLoader.autoLoadOnStart = false;
                circuitLoader.spawnPlayer = false;
                circuitLoader.LoadData();
                circuitLoader.BuildCircuit();
                circuitLoader.FitCameraToCircuit(Camera.main);

                // 背景を白にする
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = Color.white;

                // リプレイ再生（デバッグ用データがあれば）
                if (replayController != null && GameLogger.Instance != null && GameLogger.Instance.logs.Count > 0)
                {
                    GameObject prefab = circuitLoader.playerPrefab != null ? circuitLoader.playerPrefab.gameObject : null;
                    replayController.StartReplay(GameLogger.Instance.logs, prefab);
                }
            }
            return;
        }

        // IDが渡されていない場合は何もしない
        if (string.IsNullOrEmpty(GameResultData.LastEntryId))
        {
            Debug.LogWarning("LastEntryId is empty.");
            return;
        }

        var rankingMgr = new RankingManager();
        var allEntries = rankingMgr.LoadAllData().entries;

        // 1. 今回のデータをIDで特定
        var myEntry = allEntries.FirstOrDefault(e => e.id == GameResultData.LastEntryId);
        if (myEntry == null)
        {
            Debug.LogError($"Entry not found: {GameResultData.LastEntryId}");
            return;
        }

        // 2. 【背景の設定】確定した mapName を使用
        if (circuitLoader != null)
        {
            // CircuitLoaderを使ってコースを生成し、カメラを合わせる
            circuitLoader.autoLoadOnStart = false;
            circuitLoader.spawnPlayer = false;
            circuitLoader.LoadData();
            circuitLoader.BuildCircuit();
            circuitLoader.FitCameraToCircuit(Camera.main);

            // 背景を白にする
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = Color.white;

            // リプレイ再生
            if (replayController != null && GameLogger.Instance != null && GameLogger.Instance.logs.Count > 0)
            {
                GameObject prefab = circuitLoader.playerPrefab != null ? circuitLoader.playerPrefab.gameObject : null;
                replayController.StartReplay(GameLogger.Instance.logs, prefab);
            }
        }
        else
        {
            // CircuitLoaderが設定されていない場合は従来の背景画像を使用
            SetBackgroundByMapName(myEntry.mapName);
        }

        // 3. 同じマップのデータだけを抽出してソート
        var mapRanking = allEntries
            .Where(e => e.mapName == myEntry.mapName)
            .OrderBy(e => e.timeInSeconds)
            .ToList();

        // 4. 順位の特定
        int myRank = mapRanking.FindIndex(e => e.id == myEntry.id) + 1;

        // 5. UIへの反映
        // タイム表示
        var timeLabel = root.Q<Label>("TimeLabel");
        if (timeLabel != null) timeLabel.text = $"TIME: {myEntry.timeText}";

        // 順位表示
        var rankLabel = root.Q<Label>("RankLabel");
        if (rankLabel != null) rankLabel.text = $"RANK: #{myRank}";

        // NEW RECORD! 判定
        var newRecordLabel = root.Q<Label>("NewRecordLabel");
        if (newRecordLabel != null)
        {
            bool isNewRecord = (myRank == 1);
            newRecordLabel.style.display = isNewRecord ? DisplayStyle.Flex : DisplayStyle.None;
            if (isNewRecord)
            {
                newRecordLabel.text = "NEW RECORD!";
                newRecordLabel.style.color = new StyleColor(new Color(1f, 0.84f, 0f)); // Gold
            }
        }

        // 6. ランキングリスト（トップ5）の描画
        var listContainer = root.Q<VisualElement>("RankingList");
        if (listContainer != null)
        {
            RenderRanking(listContainer, mapRanking.Take(5).ToList(), myEntry.id);
        }
    }

    private void SetBackgroundByMapName(string mapName)
    {
        var bg = root.Q<VisualElement>("BackgroundMap");
        if (bg == null) return;

        var config = mapBackgrounds.FirstOrDefault(m => m.mapName == mapName);
        Texture2D texture = config.backgroundTexture != null ? config.backgroundTexture : fallbackBackground;

        if (texture != null)
        {
            bg.style.backgroundImage = new StyleBackground(texture);
            Debug.Log($"Background set for: {mapName}");
        }
    }

    private void RenderRanking(VisualElement container, List<RankingEntry> top5, string currentId)
    {
        container.Clear();
        // ヘッダーはUXMLで定義されているため、データ行のみを追加する形にするのが理想だが、
        // 現在のコードはコンテナをClearしてヘッダーから再生成しているため、それに合わせる。
        container.Add(CreateRow("POS", "NAME", "STATUS", "TIME", true));

        for (int i = 0; i < top5.Count; i++)
        {
            var entry = top5[i];
            // IDで今回の自分かどうかを判定
            bool isPlayer = (entry.id == currentId);
            
            string status = "CLEAR";
            string time = entry.timeText;

            if (entry.timeText == "TIME UP")
            {
                status = "TIME UP";
                time = "-"; 
            }

            container.Add(CreateRow(GetRankText(i + 1), entry.playerName, status, time, false, isPlayer));
        }
    }

    private VisualElement CreateRow(string pos, string name, string status, string time, bool isHeader, bool isPlayer = false)
    {
        var row = new VisualElement();
        row.AddToClassList("ranking-row");
        if (isHeader) row.AddToClassList("header-row");
        if (isPlayer) row.AddToClassList("player-highlight");

        row.Add(CreateLabel(pos, "col-pos"));
        row.Add(CreateLabel(name, "col-name"));
        row.Add(CreateLabel(status, "col-status"));
        row.Add(CreateLabel(time, "col-time"));

        return row;
    }

    private Label CreateLabel(string text, string className)
    {
        var label = new Label(text);
        label.AddToClassList(className);
        return label;
    }

    private string GetRankText(int rank)
    {
        if (rank == 1) return "1st";
        if (rank == 2) return "2nd";
        if (rank == 3) return "3rd";
        return $"{rank}th";
    }

    private void SetupButtons()
    {
        var returnButton = root.Q<Button>("ReturnButton"); 
        if (returnButton != null)
        {
            returnButton.clicked += () => SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
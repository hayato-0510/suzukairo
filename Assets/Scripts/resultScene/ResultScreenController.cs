using UnityEngine;
using TMPro;
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

    private VisualElement root;

    void Start()
    {
        if (uiDocument == null) return;
        root = uiDocument.rootVisualElement;

        // 1. 背景の初期化
        SetBackgroundByMapName(GameResultData.MapName);

        // 2. データの保存と表示
        ProcessAndDisplayResults();

        // 3. ボタンのイベント登録
        SetupButtons();
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
        }
    }

    private void ProcessAndDisplayResults()
{
    var rankingMgr = new RankingManager();
    float currentSeconds = ConvertTimeToSeconds(GameResultData.FinalTime);

    // --- 1. NEW RECORD 判定 ---
    var top5Before = rankingMgr.GetTop5(GameResultData.MapName);
    bool isNewRecord = false;

    // まだデータがない、もしくは今回のタイムが既存の1位より速い場合
    if (top5Before.Count == 0 || currentSeconds < top5Before[0].timeInSeconds)
    {
        isNewRecord = true;
    }

    // UIに「NEW RECORD!」を表示
    Label newRecordLabel = root.Q<Label>("NewRecordLabel"); // UXMLに作成してください
    if (newRecordLabel != null)
    {
        newRecordLabel.style.display = isNewRecord ? DisplayStyle.Flex : DisplayStyle.None;
        
        // ちょっとしたアニメーション風の処理（簡易版）
        if (isNewRecord) {
            newRecordLabel.style.color = new Color(1f, 0.9f, 0f); // 金色
        }
    }

    // --- 2. データの保存 ---
    rankingMgr.SaveRecord(new RankingEntry {
        mapName = GameResultData.MapName,
        playerName = "YOU",
        timeText = GameResultData.FinalTime,
        timeInSeconds = currentSeconds
    });

    // --- 3. UI更新 ---
    var top5After = rankingMgr.GetTop5(GameResultData.MapName);
    RenderRankingList(root.Q<VisualElement>("RankingList"), top5After);
}

    private void RenderRankingList(VisualElement container, List<RankingEntry> top5)
    {
        container.Clear();
        container.Add(CreateRow("POS", "NAME", "TIME", true));

        for (int i = 0; i < top5.Count; i++)
        {
            var entry = top5[i];
            // 保存したデータが今回のタイムと一致するか（またはPlayerNameがYOUか）でハイライト
            // ※より厳密にはTimestampなどで判定しますが、一旦これで実装
            bool isPlayer = (entry.playerName == "YOU" && entry.timeText == GameResultData.FinalTime);
            
            container.Add(CreateRow(GetRankText(i + 1), entry.playerName, entry.timeText, false, isPlayer));
        }
    }

    private VisualElement CreateRow(string pos, string name, string time, bool isHeader, bool isPlayer = false)
    {
        var row = new VisualElement();
        row.AddToClassList("ranking-row");
        if (isHeader) row.AddToClassList("header-row");
        if (isPlayer) row.AddToClassList("player-highlight");

        row.Add(CreateLabel(pos, "col-pos"));
        row.Add(CreateLabel(name, "col-name"));
        row.Add(CreateLabel(time, "col-time"));

        return row;
    }

    private Label CreateLabel(string text, string className)
    {
        var label = new Label(text);
        label.AddToClassList(className);
        // 垂直方向の微調整用（前回の修正を適用）
        label.style.unityTextAlign = TextAnchor.MiddleLeft;
        return label;
    }

    private string GetRankText(int rank)
    {
        if (rank == 1) return "1st";
        if (rank == 2) return "2nd";
        if (rank == 3) return "3rd";
        return $"{rank}th";
    }

    private float ConvertTimeToSeconds(string timeText)
    {
        try {
            string[] parts = timeText.Split(':');
            return (float.Parse(parts[0]) * 60f) + float.Parse(parts[1]);
        } catch {
            return 9999f;
        }
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
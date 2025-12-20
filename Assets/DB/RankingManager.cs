using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class RankingManager
{
    private string FilePath => Path.Combine(Application.persistentDataPath, "ranking.json");

    // データの保存
    public void SaveRecord(RankingEntry newEntry)
    {
        RankingData data = LoadAllData();
        
        // プレイヤー名を自動生成 (PLAYER 1, PLAYER 2...)
        // 既存のデータ数 + 1 を番号とする
        int playerNumber = data.entries.Count + 1;
        newEntry.playerName = $"PLAYER {playerNumber}";

        data.entries.Add(newEntry);
        
        // JSON化して書き込み
        string json = JsonUtility.ToJson(data, true); // trueで整形出力
        File.WriteAllText(FilePath, json);
    }

    // データの読み込み
    public RankingData LoadAllData()
    {
        if (!File.Exists(FilePath)) return new RankingData();

        string json = File.ReadAllText(FilePath);
        return JsonUtility.FromJson<RankingData>(json);
    }

    // 特定マップのトップ5を取得
    public List<RankingEntry> GetTop5(string mapName)
    {
        var data = LoadAllData();
        return data.entries
            .Where(e => e.mapName == mapName)
            .OrderBy(e => e.timeInSeconds) // 速い順
            .Take(5)
            .ToList();
    }
}
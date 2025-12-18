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
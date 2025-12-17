using System;
using System.Collections.Generic;

[Serializable]
public class RankingEntry
{
    public string id;
    public string mapName;
    public string playerName;
    public string timeText;
    public float timeInSeconds;
    public long timestamp;
}

[Serializable]
public class RankingData
{
    public List<RankingEntry> entries = new List<RankingEntry>();
}

// 共通ユーティリティ
public static class TimeFormatter
{
    public static string Format(float seconds)
    {
        // 負の数は 0 に丸める
        seconds = Math.Max(0, seconds);
        
        int minutes = (int)(seconds / 60);
        float restSeconds = seconds % 60;
        
        // 00:00.000 形式
        // {0:00} は2桁固定、{1:00.000} は2桁.3桁固定
        return string.Format("{0:00}:{1:00.000}", minutes, restSeconds);
    }
}
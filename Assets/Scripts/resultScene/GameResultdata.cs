using UnityEngine;

public static class GameResultData
{
    // リザルトシーンへ渡すデータをここに定義
    public static string FinalTime = "00:00.000";
    public static string MapName = "Default Course";
    public static int FinalRank = 0;
    
    // リザルトシーンのファイル名を定義
    public const string ResultSceneName = "ResultScene"; 

    // ゲーム終了時にデータを設定するためのメソッド
    public static void SetResult(string time, string course, int rank)
    {
        FinalTime = time;
        MapName = course;
        FinalRank = rank;
    }
}
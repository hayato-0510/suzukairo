using System;

// このファイルはMonoBehaviourを継承しない、データ構造定義専用のスクリプトです。

/// <summary>
/// ログエントリの基本構造
/// </summary>
[Serializable]
public class LogEntry
{
    public float timestamp;
    public string eventType;
    // Note: eventDataは具体的なイベントクラスを別途JsonUtilityで変換して文字列として格納するか、
    // もしくはより複雑なシリアライザを使う必要があります。
    // ここでは簡潔さのため、各イベントでこのクラスを継承するアプローチは取らず、
    // GameLogger側で各イベントデータをシリアライズすることを想定します。
    // 今回は、eventDataを直接各データクラスで定義します。
}

// Vector2/3はデフォルトでJsonUtilityのトップレベルオブジェクトとしてシリアライズできないため、
// 独自のシリアライズ可能なクラスを作成します。
[Serializable]
public struct Vector2Serializable
{
    public float x;
    public float y;

    public Vector2Serializable(UnityEngine.Vector2 vec)
    {
        x = vec.x;
        y = vec.y;
    }
}

/// <summary>
/// イベントタイプ: player_state
/// </summary>
[Serializable]
public class PlayerStateLog : LogEntry
{
    public PlayerStateData eventData;
}

[Serializable]
public class PlayerStateData
{
    public Vector2Serializable position;
    public float angle;
    public Vector2Serializable velocity;
    public float ampere;
    public float voltage;
    public ComponentContext componentContext;
}

[Serializable]
public class ComponentContext
{
    public bool onComponent;
    public string componentType;
    public int componentId;
    public float componentValue;
}

/// <summary>
/// イベントタイプ: boost_activated
/// </summary>
[Serializable]
public class BoostLog : LogEntry
{
    public BoostActivatedData eventData;
}

[Serializable]
public class BoostActivatedData
{
    public float remainingVoltage;
}

/// <summary>
/// イベントタイプ: lap_complete
/// </summary>
[Serializable]
public class LapCompleteLog : LogEntry
{
    public LapCompleteData eventData;
}

[Serializable]
public class LapCompleteData
{
    public int lapNumber;
    public float lapTime;
}

/// <summary>
/// イベントタイプ: race_start, race_finish
/// </summary>
[Serializable]
public class GameEventLog : LogEntry
{
    // eventDataは不要
}

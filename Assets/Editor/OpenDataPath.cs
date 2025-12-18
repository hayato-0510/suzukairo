#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class OpenDataPath
{
    [MenuItem("Debug/Open Persistent Data Path")]
    public static void Open()
    {
        EditorUtility.RevealInFinder(Application.persistentDataPath);
    }

    [MenuItem("Debug/Open Logs Folder")]
    public static void OpenLogs()
    {
        string logPath = Path.Combine(Application.persistentDataPath, "Logs");
        
        // フォルダが存在しない場合に作成
        if (!Directory.Exists(logPath))
        {
            Directory.CreateDirectory(logPath);
        }
        
        EditorUtility.RevealInFinder(logPath);
    }
}
#endif
using UnityEngine;

public class SerialCommandRouter : MonoBehaviour
{
    [SerializeField] private SerialHandler serialHandler;

    void Awake()
    {
        // Inspector で SerialHandler をアサインしておく想定
        serialHandler.OnDataReceived += HandleSerialData;
    }

    void OnDestroy()
    {
        serialHandler.OnDataReceived -= HandleSerialData;
    }

    private void HandleSerialData(string data)
    {
        Debug.Log($"Received: {data}");
        string[] commands = data.Split(' ');
        switch (commands[0])
        {
            case "sw":
            case "pad":
                //移動の処理
                break;
            case "map":
                //マップ変更の処理
                break;
            case "health":
                Debug.Log("[Serial] health check");
                serialHandler.SendMessage("ok");
                break;
            case "start":
                Debug.Log("[Serial] started");
                break;
        }
    }
}